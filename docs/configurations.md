## 如何准备Sample数据库
测试用例中使用了业界常用的sample 数据, 您可以从下面的网站下载sample db. 
1. MySQL sakila sample DB <https://dev.mysql.com/doc/sakila/en/sakila-installation.html>
2. MS SQL Server northwind-pubs sample DB <https://github.com/Microsoft/sql-server-samples/tree/master/samples/databases/northwind-pubs>

## 如何配置 db connection string
MyRest 使用ADO.net connection string, 您按照下面网站的示例生成您的数据库connection string. 
[www.connectionstrings.com](https://www.connectionstrings.com/)

## 发出第一个 SQL 命令
您可以使用 postman 或 jmeter等工具发送http请求, 也可以使用Visual studio code的 REST Client插件. 下面是一个示例:
```
POST http://localhost:5001/run HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate   from actor   where actor_id=@actorId1 or actor_id=@actorId2     ",  
      "parameters":[
         {
          "name": "@actorId1",
          "value": "1",
          "dataType": "Int32",
          "direction": "Input",
          "format": "",
          "separator":""
        },
        {
          "name": "@actorId2",
          "value": "20",
          "dataType": "Int32",
          "direction": "Input",
          "format": "",
          "separator":""
        }
      ]
    },
    "traceId": "1111"
  }
}
```

参数说明:
- db: 用来执行在哪个数据库上执行sql, 该db 应该已 myrest.toml 文件中设定
- isSelect: 如果是select查询语句, 设置为true, 否则设置为false
- isScalar: 如果返回的是一个标量数据(单行单列),设置为true, 否则设置为false
- sqlFile: 用来设定 server side sql, 指定server side SQL 文件名
- sqlId: 用来设定 server side sql, 指定server side SQL 文件中的SQL 语句. 
- sql: 用来设定 client side sql, 如果同时设定了sqlFile+sqlId, 优先使用服务端SQL 
- traceId: 用来设定请求的 traceId, 用来跟踪request消息报文 和 response消息报文. 
- parameters 设定, 用来设定sql的参数, 包含下面元素:
  - name: 参数名
  - value: 参数取值
  - dataType 数据类型, [参考微软文档](https://learn.microsoft.com/en-us/dotnet/api/system.data.dbtype)
  - direction 参数方向, 可选 Input/OutputInputOutput/ReturnValue
  - format 参数, 用于设定 datetime 类型的格式, [参考微软文档](https://learn.microsoft.com/zh-cn/dotnet/standard/base-types/standard-date-and-time-format-strings)
  - separator 参数, 用于支持SQL的 in 子句, value 参数应该是一个可以分隔的字符串, 使用 separator 设定具体的分隔符 

