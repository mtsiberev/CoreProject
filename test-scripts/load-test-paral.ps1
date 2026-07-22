$availableProducts = @(
    @{ productId = "7f39564c-8367-4a6a-81f1-80775a96860a"; productName = "Laptop"; price = 1200 },
    @{ productId = "f13ea1ad-1bd0-4e0f-8a76-11368fe7178d"; productName = "Keyboard"; price = 75 },
    @{ productId = "9e2ad445-3188-46e2-b2c4-a7d70c569017"; productName = "Mouse"; price = 45 }
)

$jsonList = 1..20 | ForEach-Object {
    $pickedProducts = $availableProducts | Get-Random -Count (Get-Random -Minimum 1 -Maximum 4)    

    $items = New-Object System.Collections.Generic.List[Object]
    
    foreach ($prod in $pickedProducts) {
        $items.Add(@{
            productId   = $prod.productId
            productName = $prod.productName
            price       = $prod.price
            quantity    = Get-Random -Minimum 1 -Maximum 6
        })
    }

    $orderStructure = @{
        customerName = "LoadTestUser$_"
        items        = $items.ToArray()
    }

    $orderStructure | ConvertTo-Json -Depth 5 -Compress
}

$url = "http://localhost:5000/api/orders"

Write-Host "Start sending 20 requests" -ForegroundColor Cyan

$jobs = $jsonList | ForEach-Object {
    Start-Job -ScriptBlock {
        param($innerUrl, $innerJson)
        try {
			Write-Host "Sending..." -ForegroundColor Green

            $response = Invoke-RestMethod -Method Post -Uri $innerUrl -ContentType "application/json" -Body $innerJson            

            [PSCustomObject]@{ Status = "Success"; Response = $response; SentJson = $innerJson }
        } catch {
            [PSCustomObject]@{ Status = "Error"; Message = $_.Exception.Message; SentJson = $innerJson }
        }
    } -ArgumentList $url, $_
}

$jobs | Wait-Job | Out-Null

$results = $jobs | Receive-Job

$jobs | Remove-Job


Write-Host "Test finished" -ForegroundColor Green