$url = "http://localhost:5000/api/orders"
$json = '{
  "customerName": "LoadTestUser",
  "items": [
    {
      "productId": "7f39564c-8367-4a6a-81f1-80775a96860a",
      "productName": "Laptop",
      "price": 1000,
      "quantity": 1
    }
  ]
}'

Write-Host "Запуск нагрузки: 10 запросов на $url" -ForegroundColor Cyan

1..10 | ForEach-Object {
    Invoke-RestMethod -Method Post -Uri $url -ContentType "application/json" -Body $json
    Write-Host "Заказ $_ отправлен"
}

Write-Host "Тест завершен. Проверь логи воркера и базу!" -ForegroundColor Green
