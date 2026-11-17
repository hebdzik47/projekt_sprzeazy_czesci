$ErrorActionPreference='Stop'
. "$PSScriptRoot/Test-wielu-czesci.ps1"
$payload=@{carId=$cars[0].id;items=@(@{partId=$parts[0].id;price=123;quantity=8},@{partId=$parts[1].id;price=61.50;quantity=2});saleDate='2026-09-24';salespersonId=1;priceType='gross'}
$before=Invoke-RestMethod "$base/reports/summary?from=2026-09-24&to=2026-09-24" -Headers $ah
foreach($quantity in @(0,-1,1.5,10001)) {
    $bad=@{carId=$cars[0].id;items=@(@{partId=$parts[0].id;price=1;quantity=$quantity});saleDate='2026-09-24';salespersonId=1}
    $r=Invoke-WebRequest "$base/sales/batch" -Headers $ah -Method Post -ContentType 'application/json' -Body ($bad|ConvertTo-Json -Depth 5) -SkipHttpErrorCheck
    Assert ($r.StatusCode -eq 400) "Odrzucona ilosc: $quantity"
}
$ids=@()
try {
    $result=Invoke-RestMethod "$base/sales/batch" -Headers $ah -Method Post -ContentType 'application/json' -Body ($payload|ConvertTo-Json -Depth 5)
    $ids=@($result.ids)
    Assert ($result.quantity -eq 10 -and $result.count -eq 2 -and $result.totalNet -eq 900) '8 x 100 + 2 x 50 = 900 netto'
    $history=Invoke-RestMethod "$base/sales?from=2026-09-24&to=2026-09-24&pageSize=100" -Headers $ah
    $line=$history.items|Where-Object id -eq $ids[0]
    Assert ($line.quantity -eq 8 -and $line.price -eq 100 -and $line.totalNet -eq 800) 'Historia: cena jednostkowa, ilosc i suma'
    $after=Invoke-RestMethod "$base/reports/summary?from=2026-09-24&to=2026-09-24" -Headers $ah
    Assert ($after.partsCount-$before.partsCount -eq 10 -and $after.salesCount-$before.salesCount -eq 2 -and [decimal]$after.totalValue-[decimal]$before.totalValue -eq 900) 'Raport rozroznia sztuki od pozycji i sumuje wartosci'
    $beforeUser=($before.byUser|Where-Object name -eq 'Krystian'|Measure-Object count -Sum).Sum
    $afterUser=($after.byUser|Where-Object name -eq 'Krystian'|Measure-Object count -Sum).Sum
    Assert ($afterUser-$beforeUser -eq 10) 'Zestawienie sprzedawcy liczy sztuki'
} finally {
    foreach($id in $ids){Invoke-RestMethod "$base/sales/$id" -Headers $ah -Method Delete|Out-Null}
    Write-Output 'Usunieto wylacznie testowe zapisy ilosci.'
}
