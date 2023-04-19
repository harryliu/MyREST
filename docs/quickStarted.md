## 安装 dotnet
目前支持 .net6 和 .net7

## 下载 MyRest 编译包
github 主页中下载编译包

## 调整 appsettings.json 配置 
1. 修改 appsettings.json 文件, 修改 http 和 https 的 Url 属性, 比如设置http为5001端口, https为5002端口:

```json
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5001"
      },
      "Https": {
        "Url": "https://localhost:5002"
      }
    }
  }
```

2. 按需调整 logging 相关配置 

## 调整 myrest.toml  配置 
1. system section 配置 
```toml
[system]
enableSwagger=true # allow enable Swagger UI
enableClientSql=true  # allow client to submit SQL statement. It only works in debug mode. 
hotReloadSqlFile=true # hot reload server side SQL file 
writebackRequest=true # writeback the whole request rather than only traceId
writebackInBase64=false # writeback SQL in plain or base64 format
useResponseCompression=false # enable response compression
```

2. 配置安全插件
```toml
[firewall]
enableIpWhiteList=true # enable IP white list
enableIpBlackList=true # enable IP black list
ipWhiteList=["127.0.0.1","192.168.0.1"]
ipBlackList=["127.0.0.2","192.168.0.156"]


[basicAuth]
enableBasicAuth=false # enable Basic Auth
username="user123"
password="password123"

```

3. 配置 database 
下面是配置一个mysql数据库的示例, 我们可以配置多个数据库. 
```toml
[[databases]]
name="sakila"  # db connection name 
dbType="mysql" # sqlite,mysql,mssql,postgresql,oracle
connectionString="Server=localhost;Port=3306;Database=sakila;Uid=root;Pwd=TOOR;"    
sqlFileHome="c://temp"   #sql file home
```

## 启动 MyREST 服务 
如果部署在 Windows , 可以直接运行 `MyREST.exe`
或者使用下面的命令, 适用于Windows/Linux/MacOS: 
```shell
dotnet MyREST.dll
```


## 访问 MyREST http endpoint
1. 访问 MyREST Swagger end point
Swagger UI <https://localhost:5002/swagger/index.html>

2. 访问 MyREST health check end point
```
GET http://localhost:5001/health
content-type: application/json
```

3. 访问系统状态 end point
```
GET http://localhost:5001/status
content-type: application/json
```

