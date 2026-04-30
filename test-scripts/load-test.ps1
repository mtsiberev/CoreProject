$url = "http://localhost:5000/api/orders"
$jsonUser1 = '{
  "customerName": "LoadTestUser1",
  "items": [
    {
      "productId": "7f39564c-8367-4a6a-81f1-80775a96860a",
      "productName": "Laptop",
      "price": 1000,
      "quantity": 1
    },
	{
      "productId": "f13ea1ad-1bd0-4e0f-8a76-11368fe7178d",
      "productName": "Keyboard",
      "price": 20,
      "quantity": 1
    },
	{
      "productId": "9e2ad445-3188-46e2-b2c4-a7d70c569017",
      "productName": "Mouse",
      "price": 10,
      "quantity": 1
    }	
  ]
}'

$jsonUser2 = '{
  "customerName": "LoadTestUser2",
  "items": [
    {
      "productId": "f13ea1ad-1bd0-4e0f-8a76-11368fe7178d",
      "productName": "Keyboard",
      "price": 20,
      "quantity": 3
    },
	{
      "productId": "9e2ad445-3188-46e2-b2c4-a7d70c569017",
      "productName": "Mouse",
      "price": 10,
      "quantity": 3
    }	
  ]
}'

$jsonUser3 = '{
  "customerName": "LoadTestUser3",
  "items": [
    {
      "productId": "9e2ad445-3188-46e2-b2c4-a7d70c569017",
      "productName": "Mouse",
      "price": 10,
      "quantity": 1
    }
  ]
}'

Write-Host "Запуск нагрузки: 10 запросов на $url" -ForegroundColor Cyan

1..10 | ForEach-Object {
    Invoke-RestMethod -Method Post -Uri $url -ContentType "application/json" -Body $jsonUser1
    Write-Host "Заказ $_ отправлен"
}

1..10 | ForEach-Object {
    Invoke-RestMethod -Method Post -Uri $url -ContentType "application/json" -Body $jsonUser2
    Write-Host "Заказ $_ отправлен"
}

1..10 | ForEach-Object {
    Invoke-RestMethod -Method Post -Uri $url -ContentType "application/json" -Body $jsonUser3
    Write-Host "Заказ $_ отправлен"
}

Write-Host "Тест завершен. Проверь логи воркера и базу!" -ForegroundColor Green
