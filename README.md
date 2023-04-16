# MyREST
a universal database RESTful service

## 背景
在前后端分离架构越来越流行的今天, 我们是否可以再向前发展一步?  让前端实现更多的CURD业务逻辑,  后端是否仅提供存储支持.  
本项目提供一个通用的Restful数据库服务程序, 我们只需要在前端实现业务逻辑, 服务端无需任何编码, 非常适合中小型应用, 可极大提升项目开发效率. 


本项目受到 [pREST](https://github.com/prest/prest) 项目的启发,  pREST 项目能将 PostgreSQL 数据库以RESTful方式暴露出来, 但该项目局限性太大, 便有想法自己造个轮子, pREST 项目限制有:
1. 仅仅支持PostgreSQL数据库
2. API 表达能力太弱, 无法实现复杂SQL

## 实现技术
- 采用 .Net Core 实现. 
- 采用 Dapper ORM 框架


## 项目特性
- 支持多种数据库, MySQL, SQLite, Postgresql 和 Oracle 等
- 支持多个数据库连接 
- 支持客户端直接提交SQL命令
- 支持调用服务端SQL(推荐)
- SQL 命令支持绑定变量(参数化)
- 支持 ResponseCompression 压缩(gzip/br格式)
- 支持 OpenAPI (即 Swagger)
- 内置防火墙安全插件(白名单或黑名单)
- 支持 Basic Auth安全插件
- 支持 JWT Auth安全插件
- (todo) 支持 miniProfiler 
- (todo) 支持优雅停机



## 执行 SQL 示例
Post 请求: 
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

消息返回: 
```json

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate   from actor   where actor_id=@actorId1 or actor_id=@actorId2     ",
      "parameters": [
        {
          "name": "@actorId1",
          "value": "1",
          "dataType": "Int32",
          "direction": "Input",
          "format": "",
          "separator": ""
        },
        {
          "name": "@actorId2",
          "value": "20",
          "dataType": "Int32",
          "direction": "Input",
          "format": "",
          "separator": ""
        }
      ]
    },
    "traceId": "1111"
  },
  "response": {
    "returnCode": 0,
    "errorMessage": "",
    "rowCount": 2,
    "affectedCount": 0,
    "scalarValue": null,
    "rows": [
      {
        "actor_id": 1,
        "FirstName": "10",
        "LastName": "GUINESS",
        "LastUpdate": "2023-04-05T16:31:20+08:00"
      },
      {
        "actor_id": 20,
        "FirstName": "LUCILLE",
        "LastName": "TRACY",
        "LastUpdate": "2006-02-15T04:34:33+08:00"
      }
    ]
  }
}
```

## 更多文档
- Quick started : <https://github.com/harryliu/MyREST/blob/main/docs/quickStarted.md>
- Configurations : <https://github.com/harryliu/MyREST/blob/main/docs/configurations.md>
- Security : <https://github.com/harryliu/MyREST/blob/main/docs/security.md>
