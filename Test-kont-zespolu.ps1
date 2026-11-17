$ErrorActionPreference='Stop'
$base='http://127.0.0.1:5180/api'
$config=Get-Content "$PSScriptRoot/backend/AutoPartsSales.Api/appsettings.Development.json" -Raw|ConvertFrom-Json
$admin=Invoke-RestMethod "$base/auth/login" -Method Post -ContentType 'application/json' -Body (@{userName=$config.InitialAdmin.UserName;password=$config.InitialAdmin.Password}|ConvertTo-Json)
$ah=@{Authorization=('Bearer '+$admin.token)}
$cars=Invoke-RestMethod "$base/cars" -Headers $ah
$parts=Invoke-RestMethod "$base/parts" -Headers $ah
$ids=@()
function Assert($condition,$name){if(-not $condition){throw $name};Write-Output "OK: $name"}
try {
    foreach($account in @(@{login='Krystian';name='Krystian';seller=1},@{login='Adam';name='Adam';seller=2},@{login='Damian';name='Damian';seller=3},@{login='Pawel';name='Paweł';seller=4})) {
        $password=[Environment]::GetEnvironmentVariable('TeamPasswords__'+$account.login)
        if(-not $password){throw 'Podaj hasla testowe przez zmienne TeamPasswords__LOGIN.'}
        $session=Invoke-RestMethod "$base/auth/login" -Method Post -ContentType 'application/json' -Body (@{userName=$account.login;password=$password}|ConvertTo-Json)
        $h=@{Authorization=('Bearer '+$session.token)}
        Assert ($session.user.userName -ceq $account.login -and $session.user.displayName -eq $account.name -and $session.user.roles -contains 'Employee' -and $session.user.roles -notcontains 'Administrator') "Konto $($account.login)"
        $denied=Invoke-WebRequest "$base/reports/summary" -Headers $h -SkipHttpErrorCheck
        Assert ($denied.StatusCode -eq 403) 'Blokada raportow administratora'
        $body=@{carId=$cars[0].id;items=@(@{partId=$parts[0].id;price=1;quantity=2});saleDate=(Get-Date -Format 'yyyy-MM-dd');userId=$admin.user.id}
        $saved=Invoke-RestMethod "$base/sales/batch" -Headers $h -Method Post -ContentType 'application/json' -Body ($body|ConvertTo-Json -Depth 5)
        $ids+=@($saved.ids)
        $history=Invoke-RestMethod "$base/sales?seller=seller:$($account.seller)&pageSize=100" -Headers $ah
        $row=$history.items|Where-Object id -eq $saved.ids[0]
        Assert ($row.userName -eq $account.name -and $row.userId -eq $session.user.id) 'Automatyczne przypisanie sprzedawcy i autora'
        $body.salespersonId=5
        $denied=Invoke-WebRequest "$base/sales/batch" -Headers $h -Method Post -ContentType 'application/json' -Body ($body|ConvertTo-Json -Depth 5) -SkipHttpErrorCheck
        Assert ($denied.StatusCode -eq 403) 'Blokada wyboru innego sprzedawcy'
        $body.salespersonId=$account.seller
        $savedByAdmin=Invoke-RestMethod "$base/sales/batch" -Headers $ah -Method Post -ContentType 'application/json' -Body ($body|ConvertTo-Json -Depth 5)
        $ids+=@($savedByAdmin.ids)
        $own=Invoke-RestMethod "$base/sales?pageSize=100" -Headers $h
        Assert (@($own.items|Where-Object id -eq $savedByAdmin.ids[0]).Count -eq 1) 'Widocznosc wlasnej sprzedazy dodanej przez administratora'
        Assert (@($own.items|Where-Object userName -ne $account.name).Count -eq 0) 'Historia bez sprzedazy innych osob'
    }
} finally { foreach($id in $ids){Invoke-RestMethod "$base/sales/$id" -Headers $ah -Method Delete|Out-Null};Write-Output 'Usunieto wylacznie wpisy testowe.' }
