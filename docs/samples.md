
### visit index 
GET http://localhost:5001/



### Swagger UI url 
http://localhost:5001/swagger/index.html 


### health check  
GET http://localhost:5001/health
content-type: application/json


### status check (behind firewall) 
GET http://localhost:5001/status
content-type: application/json


### SQL API format
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "string",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "string",
      "sqlId": "string",
      "sql": "string",
      "parameters": [
        {
          "name": "string",
          "value": "string",
          "dataType": "string",
          "direction": "string",
          "format": "string",
          "separator":"string"
        }
      ]
    },
    "traceId": "string"
  }
}
```


### greenChannelSelect API format


POST http://localhost:5001/greenChannelSelect HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "string",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "string",
      "sqlId": "string",
      "sql": "string",
      "parameters": [
        {
          "name": "string",
          "value": "string",
          "dataType": "string",
          "direction": "string",
          "format": "string",
          "separator":"string"
        }
      ]
    },
    "traceId": "string"
  }
}