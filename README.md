# MyREST
a universal database RESTful service

## 背景
在前后端分离架构越来越流行的今天, 我们是否可以再向前发展一步, 前端实现更多的CURD业务逻辑? 后端是否仅提供存储支持? 本项目将实现这样的愿景:提供一个通用的Restful数据库服务程序, 我们只需要在前端实现业务逻辑, 对于中小型应用, 极大提升项目开发效率. 

本项目受到 pREST 项目的启发, 该项目能将 PostgreSQL 数据库以RESTful方式暴露出来, 但 pREST 项目局限性太大, 便有想法自己造个轮子, pREST 项目限制有:
1. 仅仅支持PostgreSQL数据库
2. API 表达能力太弱. 

## 项目特性
- 本项目采用 .net core 实现. 
- 支持多种数据库, MySQL, SQLite, Postgresql 和 Oracle 等
- 采用 Dapper ORM 框架
- 使用 JWT 进行安全校验
- 支持客户端SQL和服务端SQL(SQL语句存放在服务端)两种形式, 推荐使用服务端SQL形式
- 支持白名单
- 支持 OpenAPI
- 支持 miniProfiler 
- 支持优雅停机

## 安全方面的设计
- 后台数据库中, 需要增加一个表 myrest_users, 该table将作为前端应用的登录账号表, 对于具体应用可以再为这个表配一个从表, 用于扩展用户信息. 
- myrest_users 表的字段: username, password, refreshToken, refreshTokenExpireTime,clientInfo,lockedFlag,lastLogin 字段.
- 前端系统登录时即访问 /myrest/auth 获取 accessToken 和 refreshToken, 之后调用 myrest/service 需要带上 accessToken. 说明: 在每次提交data service 之前需要先判断 accessToken 是否过期, 如过期的话, 需要先刷新 refresh token, 如 refreshToken 也过期了, 需要跳转到登录界面. 
- accessToken 的 Expires 为 10 分钟,  refreshToken Expires 为 60 分钟, 刷新 refreshToken 需要带 clientInfo, 避免被盗用. 
- 系统支持客户端SQL和服务端SQL两种形式, 对于正式项目更推荐使用服务端SQL形式, 避免泄漏后台数据库结构.

## 两个工具
- myrest_server: 这是一个 api server
- myrest_tool: 这是一个命令行工具, 用来生成  myrest_users 表, 增加/删除/锁定 myrest_users 的用户记录

## 后台配置
```toml
[system]
    debug=true  #启用debug模式, 将不需要进行authentication
    port = 3000
    host = localhost
	enableClientSql =true
[auth]
	enabled = true  
	type = "body"
	encrypt = "MD5"
	accessTokenExpires=10	
	refreshTokenExpire=60
	jwtKey="secret"
	jwtAlgo="HS256"
[database]
    dbType=sqlite,mysql,mssql,postgresql,oracle
	connectionString=""
	sqlPath=""
```


## SQL API 设计
post 请求
```
POST /myrest/db1  HTTP/1.1
content-type: application/json

{	
	"request": {
		"sqlFile": "",
		"sqlId": "",
		"parameters": [
			{"name": "","value": "","format": ""},
			{"name": "","value": "","format": ""}
		],
		"traceId": "11",
		"requireTransaction": true
	},
	"response": {
		"errorMessage": "",
		"returnCode": 0,
		"rowCount": 1,
		"affectedCount": 1,
		"rows": [
			{
				"field1": "value11",
				"field2": "value12"
			},
			{
				"field1": "value21",
				"field2": "value22"
			}
				]
	}	
}


```


## Auth API 设计
https://dapper-tutorial.net/parameter-dynamic
参考 [](https://jasonwatmore.com/post/2021/06/15/net-5-api-jwt-authentication-with-refresh-tokens)
[](https://www.cnblogs.com/ittranslator/p/refresh-jwt-with-refresh-tokens-in-asp-net-core-5-rest-api-step-by-step.html)
[](https://jasontaylor.dev/api-key-authentication-with-aspnetcore/)
[](https://andrewlock.net/5-new-mvc-features-in-dotnet-7/)
[](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-7.0)
[](https://www.cnblogs.com/lwqlun/p/10222505.html)
[](https://github.com/ZeeLyn/Dapper.Extensions)
### public 认证接口
``` json
POST /myrest/auth/access-token  HTTP/1.1
content-type: application/json

{
    "username": "username",
    "password ": "password",
	"clientInfo":"clientInfo"
} 
```
返回格式:
```
{
    "accessToken": "accessToken",
    "refreshToken ": "refreshToken",
	"message ": "Token issued"
} 
```
 
### public 的refresh-token接口
``` json
POST /myrest/auth/refresh-token  HTTP/1.1
content-type: application/json

{
    "refreshToken": "refreshToken",
	"clientInfo":"clientInfo"
} 
```
返回格式:
```
{
    "accessToken": "accessToken",
    "refreshToken ": "refreshToken",
	"message ": "Token issued"
} 
``` 

### secure 的 revoke-token接口
``` json
POST /myrest/auth/refresh-token  HTTP/1.1
content-type: application/json

{
    "refreshToken": "refreshToken",
	"clientInfo":"clientInfo"
} 
```
返回格式:
```
{
    "message ": "Token revoked"
} 
``` 