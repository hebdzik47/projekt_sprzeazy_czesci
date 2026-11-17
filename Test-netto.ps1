$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/Test-aplikacji.ps1"
$sellers = Invoke-RestMethod "$base/salespeople" -Headers $ah
Assert (($sellers.name -join ',') -eq 'Krystian,Adam,Damian,Paweł,sprzedaż na miejscu') 'Pieciu sprzedawcow administratora'
$allParts = Invoke-RestMethod "$base/parts" -Headers $eh
foreach ($c in $cars) { $available=Invoke-RestMethod "$base/cars/$($c.id)/parts" -Headers $eh; Assert (($available.id -join ',') -eq ($allParts.id -join ',')) "Wszystkie czesci: $($c.make) $($c.model)" }
Assert (@($cars | Where-Object make -match '^Volkswagen$').Count -eq 0) 'Skrot vw'
$names = @($cars | ForEach-Object { ($_.make+' '+$_.model).ToLowerInvariant() })
Assert (($names -join '|') -eq (($names | Sort-Object) -join '|')) 'Sortowanie marek bez wielkosci liter'
$payload=@{carId=$cars[0].id;partId=$allParts[0].id;price=123;priceType='gross';salespersonId=1;saleDate='2026-09-23'}
$r=Invoke-WebRequest "$base/sales" -Headers $eh -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json) -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 403) 'Pracownik nie moze wybrac innego sprzedawcy'
$payload.salespersonId=9999
$r=Invoke-WebRequest "$base/sales" -Headers $ah -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json) -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 400) 'Nieistniejacy sprzedawca odrzucony'
$payload.salespersonId=1
$payload.priceType='invalid'
$r=Invoke-WebRequest "$base/sales" -Headers $ah -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json) -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 400) 'Nieprawidlowy rodzaj ceny odrzucony'
$createdIds = [System.Collections.Generic.List[int]]::new()
try {
    $before=Invoke-RestMethod "$base/reports/summary?from=2026-09-23&to=2026-09-23" -Headers $ah
    foreach ($seller in $sellers) {
        $payload.salespersonId=$seller.id; $payload.priceType='gross'
        $created=Invoke-RestMethod "$base/sales" -Headers $ah -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json)
        $createdIds.Add($created.id)
        Assert ($created.price -eq 100) "123 brutto = 100 netto: $($seller.name)"
        $h=Invoke-RestMethod "$base/sales?seller=seller:$($seller.id)&carId=$($cars[0].id)&from=2026-09-23&to=2026-09-23&pageSize=100" -Headers $ah
        $entry=$h.items|Where-Object id -eq $created.id
        Assert ($entry.userName -eq $seller.name -and $entry.userId -eq $admin.user.id -and $entry.price -eq 100) 'Sprzedawca, autor, samochod, data i netto w historii'
        Assert (@($h.items|Where-Object { $_.userName -ne $seller.name -or $_.carId -ne $cars[0].id }).Count -eq 0) 'Laczone filtry sprzedawcy i samochodu'
    }
    $after=Invoke-RestMethod "$base/reports/summary?from=2026-09-23&to=2026-09-23" -Headers $ah
    Assert (([decimal]$after.totalValue-[decimal]$before.totalValue) -eq 500 -and $after.salesCount-$before.salesCount -eq 5) 'Raport sumuje netto wszystkich sprzedawcow'
    Assert ($null -eq $after.byPart) 'Brak rankingu czesci w raporcie'
} finally {
    foreach ($id in $createdIds) { Invoke-RestMethod "$base/sales/$id" -Headers $ah -Method Delete | Out-Null }
    Write-Output 'Usunieto wylacznie sprzedaze utworzone przez ten test.'
}
