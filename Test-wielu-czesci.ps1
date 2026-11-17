$ErrorActionPreference='Stop'
. "$PSScriptRoot/Test-aplikacji.ps1"
$makes=Invoke-RestMethod "$base/car-makes" -Headers $ah
$expected='mercedes,bmw,peugeot,tata,saab,smart,chrysler,jaguar,mini,dacia,renault,volvo,alfa romeo,iveco,lada,jeep,ford,fiat,land rover,suzuki,toyota,opel,vw,citroen,mg,daewoo,chevrolet,seat,skoda,audi,ds,porsche,nissan,hummer,fsc'.Split(',')
Assert (@($expected|Where-Object {$_ -notin $makes}).Count -eq 0) 'Wszystkie 35 marek dostepne'
$parts=Invoke-RestMethod "$base/parts" -Headers $ah
$items=@($parts | Select-Object -First 4 | ForEach-Object {@{partId=$_.id;price=123}})
$payload=@{carId=$cars[0].id;items=$items;saleDate='2026-09-24';salespersonId=1;priceType='gross'}
$before=Invoke-RestMethod "$base/sales?pageSize=1" -Headers $ah
foreach($invalid in @(@(), @(@{partId=$parts[0].id;price=123},@{partId=2147483647;price=123}), @(@{partId=$parts[0].id;price=123},@{partId=$parts[0].id;price=1}), @(@{partId=$parts[0].id;price=123},@{partId=$parts[1].id;price=-1}))) {
    $bad=$payload.Clone();$bad.items=$invalid
    $r=Invoke-WebRequest "$base/sales/batch" -Headers $ah -Method Post -ContentType 'application/json' -Body ($bad|ConvertTo-Json -Depth 5) -SkipHttpErrorCheck
    Assert ($r.StatusCode -eq 400) 'Nieprawidlowy zestaw odrzucony'
}
$r=Invoke-WebRequest "$base/sales/batch" -Headers $eh -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json -Depth 5) -SkipHttpErrorCheck
Assert ($r.StatusCode -eq 403) 'Pracownik nie wybiera sprzedawcy'
$after=Invoke-RestMethod "$base/sales?pageSize=1" -Headers $ah
Assert ($after.total -eq $before.total) 'Bledny zestaw nie zapisuje zadnej pozycji'
$ids=@()
try {
    $saved=Invoke-RestMethod "$base/sales/batch" -Headers $ah -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json -Depth 5)
    $ids=@($saved.ids)
    Assert ($saved.count -eq 4 -and $saved.totalNet -eq 400) 'Cztery czesci: 492 brutto = 400 netto'
    $h=Invoke-RestMethod "$base/sales?seller=seller:1&carId=$($cars[0].id)&from=2026-09-24&to=2026-09-24&pageSize=100" -Headers $ah
    $rows=@($h.items|Where-Object id -in $ids)
    Assert ($rows.Count -eq 4 -and @($rows|Where-Object {$_.price -ne 100 -or $_.userName -ne 'Krystian'}).Count -eq 0) 'Kazda czesc widoczna w historii z cena i sprzedawca'
    $employeePayload=$payload.Clone();$employeePayload.Remove('salespersonId');$employeePayload.priceType='net';$employeePayload.items=@(@{partId=$parts[0].id;price=50})
    $saved=Invoke-RestMethod "$base/sales/batch" -Headers $eh -Method Post -ContentType 'application/json' -Body ($employeePayload|ConvertTo-Json -Depth 5)
    $ids+=@($saved.ids)
    Assert ($saved.count -eq 1 -and $saved.totalNet -eq 50) 'Jedna czesc nadal dziala na koncie pracownika'
} finally {
    foreach($id in $ids){Invoke-RestMethod "$base/sales/$id" -Headers $ah -Method Delete|Out-Null}
    Write-Output 'Usunieto tylko pozycje utworzone przez test.'
}
