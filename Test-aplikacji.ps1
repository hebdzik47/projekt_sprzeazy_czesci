$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:5180/api'
$config = Get-Content (Join-Path $PSScriptRoot 'backend/AutoPartsSales.Api/appsettings.Development.json') -Raw | ConvertFrom-Json
function Assert($condition, $label) { if (-not $condition) { throw "FAIL: $label" }; Write-Output "OK: $label" }
function Login($account) { Invoke-RestMethod -Uri "$base/auth/login" -Method Post -ContentType 'application/json' -Body (@{userName=$account.UserName;password=$account.Password}|ConvertTo-Json) }
$admin = Login $config.InitialAdmin
$employee = Login $config.InitialEmployee
$ah = @{Authorization=('Bearer '+$admin.token)}
$eh = @{Authorization=('Bearer '+$employee.token)}
Assert ($admin.user.roles -contains 'Administrator') 'Logowanie administratora'
Assert ($employee.user.roles -contains 'Employee') 'Logowanie sprzedawcy'
$cars = Invoke-RestMethod "$base/cars" -Headers $eh
Assert ($cars.Count -gt 0) 'Katalog samochodow'
$car = $null; $part = $null
foreach ($candidate in $cars) { $parts=Invoke-RestMethod "$base/cars/$($candidate.id)/parts" -Headers $eh; if($parts.Count -gt 0){$car=$candidate;$part=$parts[0];break} }
Assert ($null -ne $part) 'Wspolne czesci samochodu'
foreach ($url in @('/reports/summary','/cars')) {
    if($url -eq '/cars') { $r=Invoke-WebRequest "$base$url" -Headers $eh -Method Post -ContentType 'application/json' -Body '{"make":"TEST","model":"TEST"}' -SkipHttpErrorCheck }
    else { $r=Invoke-WebRequest "$base$url" -Headers $eh -SkipHttpErrorCheck }
    Assert ($r.StatusCode -eq 403) "Blokada pracownika: $url"
}
$r=Invoke-WebRequest "$base/sales" -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 401) 'Brak dostepu bez zalogowania'
foreach ($endpoint in @('/sales?from=2026-09-22&to=2026-09-01','/reports/summary?from=2026-09-22&to=2026-09-01')) {
    $r=Invoke-WebRequest "$base$endpoint" -Headers $ah -SkipHttpErrorCheck
    Assert ($r.StatusCode -eq 400) "Walidacja zakresu dat: $endpoint"
}
foreach ($price in @(-1, 0, 1.123, 10000000000)) {
    $body=@{carId=$car.id;partId=$part.id;price=$price;saleDate='2026-09-22'}|ConvertTo-Json
    $r=Invoke-WebRequest "$base/sales" -Headers $eh -Method Post -ContentType 'application/json' -Body $body -SkipHttpErrorCheck
    Assert ($r.StatusCode -eq 400) "Walidacja ceny: $price"
}
$r=Invoke-WebRequest "$base/sales" -Headers $eh -Method Post -ContentType 'application/json' -Body (@{carId=$car.id;partId=2147483647;price=1;saleDate='2026-09-22'}|ConvertTo-Json) -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 400) 'Blokada nieistniejacej czesci'
$r=Invoke-WebRequest "$base/sales" -Headers $eh -Method Post -ContentType 'application/json' -Body (@{carId=$car.id;partId=$part.id;price=1}|ConvertTo-Json) -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 400) 'Wymagana data sprzedazy'
$created=$null
try {
    $before=Invoke-RestMethod "$base/reports/summary?from=2026-09-22&to=2026-09-22" -Headers $ah
    $created=Invoke-RestMethod "$base/sales" -Headers $eh -Method Post -ContentType 'application/json' -Body (@{carId=$car.id;partId=$part.id;price=1.23;saleDate='2026-09-22';userId=$admin.user.id}|ConvertTo-Json)
    $history=Invoke-RestMethod "$base/sales?from=2026-09-22&to=2026-09-22&pageSize=100" -Headers $eh
    $sale=$history.items | Where-Object id -eq $created.id
    Assert ($sale.userId -eq $employee.user.id -and $sale.price -eq 1.23) 'Zapis netto z autorem z tokenu (ignorowanie obcego userId)'
    $after=Invoke-RestMethod "$base/reports/summary?from=2026-09-22&to=2026-09-22" -Headers $ah
    Assert (($after.salesCount - $before.salesCount) -eq 1 -and ([decimal]$after.totalValue - [decimal]$before.totalValue) -eq 1.23) 'Raport uwzglednia zapis i poprawna kwote'
    $page=Invoke-RestMethod "$base/sales?pageSize=1&page=1" -Headers $eh
    Assert ($page.items.Count -eq 1) 'Paginacja serwerowa'
    $history=Invoke-RestMethod "$base/sales?userId=$($admin.user.id)&pageSize=100" -Headers $eh
    Assert (@($history.items | Where-Object userId -ne $employee.user.id).Count -eq 0) 'Pracownik nie widzi obcych transakcji'
} finally {
    if($created) { Invoke-RestMethod "$base/sales/$($created.id)" -Headers $ah -Method Delete | Out-Null; Write-Output 'Usunieto wylacznie zapis utworzony przez test.' }
}
