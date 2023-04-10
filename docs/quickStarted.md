## 安装 dotnet
目前支持 .net7 +

## 下载 MyRest 编译包
github 主页中下载编译包

## 调整 appsettings.json 配置 
1. 修改 appsettings.json 文件, 修改 http 和 https 的 Url 属性. 
2. 按需调整 logging 相关配置 

## 调整 myrest.toml  配置 
1. system section 配置 
```toml
enableSwagger=true # allow enable Swagger UI
enableClientSql=true  # allow client to submit SQL statement. It only works in debug mode. 
hotReloadSqlFile=true # hot reload server side SQL file 
writebackRequest=true # writeback the whole request rather than only traceId
enableIpWhiteList=true # enable IP white list
enableIpBlackList=true # enable IP black list
ipWhiteList=["127.0.0.1","192.168.0.1"]
ipBlackList=["127.0.0.2","192.168.0.156"]
useResponseCompression=false # enable response compression
```
2. 配置 database 
```toml
[[databases]]
name="sakila"  # db connection name 
dbType="mysql" # sqlite,mysql,mssql,postgresql,oracle
connectionString="Server=localhost;Port=3306;Database=sakila;Uid=root;Pwd=TOOR;"    
sqlFileHome="c://temp"   #sql file home
```

## 启动 MyREST 服务 
如果部署在 Windows , 可以直接运行 MyREST.exe  
```shell
dotnet MyREST.dll
```


## 关于Sample数据库
测试用例中使用了业界常用的sample 数据:
1. MySQL sakila sample DB <https://dev.mysql.com/doc/sakila/en/sakila-installation.html>
2. MS SQL Server northwind-pubs sample DB <https://github.com/Microsoft/sql-server-samples/tree/master/samples/databases/northwind-pubs>
