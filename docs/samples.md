
### visit index 
```
GET http://localhost:5001/
```


### Swagger UI url 
```
https://localhost:5002/swagger/index.html
```


### health check
```
GET http://localhost:5001/health
content-type: application/json
```


### status check (behind firewall) 
```
GET http://localhost:5001/status
content-type: application/json
```

### sql API format
sql 接口需要通过 BasicAuth/JwtAuth 验证 
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
greenChannelSelect 接口不需要通过 BasicAuth/JwtAuth 验证, 但只允许执行服务器端sql查询, 而且只允许一行记录返回. 
其他参数含义同 `sql` API. 
```
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
```


### client side  SQL 示例
client sql 需要设定 `sqlContext.sql` 参数. 
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate   from actor      ",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}
```

### server side SQL 示例
server sql 需要设定 `sqlContext.sqlFile` 和 `sqlContext.sqlId` 参数. 即真正的sql语句是从server端的文件中读取. 
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "/sampleSql.xml",
      "sqlId": "queryAllActors",
      "sql": "",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}
```


### Scalar select 标量值查询示例
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": true,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select count(*) from actor      ",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}

```

### DML statement 示例
可以同时设定多条sql. 
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": false,
      "isScalar": false,
      "sqlFile": "",
      "sqlId": "",
      "sql": "update actor set first_name='10' where actor_id=1 ;update actor set first_name='20' where actor_id=2 ; ",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}
```


### 带参数示例
```
POST http://localhost:5001/sql HTTP/1.1
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

### In 子句带参数示例
支持In子句参数是MyREST的一大亮点, 避免了拼SQL字符串问题. 
注意点:
1. in 子句的参数不能用括号() 括起来, 即正确的写法是 ` actor_id in @ids` , 而不是 ` actor_id in (@ids)` 
2. in 子句的参数 dataType 属性, 需要设定为基础类型加 array 或 list, 比如例子中的 `Decimal Array` 或 `Decimal List` , 这里的 array 和 list 含义完全相同. 
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json
Authorization: Basic dXNlcjEyMzpwYXNzd29yZDEyMw==

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": false,      
      "sqlFile": "",
      "sqlId": "",
      "sql": "select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate  from actor where actor_id=@id1 or actor_id in @ids or actor_id in @newIds or actor_id=@id13" ,  
      "parameters":[
         {
          "name": "@ids",
          "value": "1,2",
          "dataType": "Decimal Array",
          "direction": "Input",
          "format": ",",
          "separator":","
        },       {
          "name": "@newIds",
          "value": "3,4",
          "dataType": "Decimal List",
          "direction": "Input",
          "format": ",",
          "separator":","
        },       {
          "name": "@id1",
          "value": "5",
          "dataType": "Decimal",
          "direction": "Input",
          "format": ",",
          "separator":","
        },       {
          "name": "@id13",
          "value": "6",
          "dataType": "Decimal",
          "direction": "Input",
          "format": ",",
          "separator":","
        }        
      ]
    },
    "traceId": "1111"
  }
}

```


### compress response
在 MyREST.toml 文件中 useResponseCompression=true 后, 即开启了Response压缩功能, 在发request时候, 需要设定 Accept-Encoding 请求头. 
```
POST http://localhost:5001/sql HTTP/1.1
Content-type: application/json
Accept-Encoding: br, gzip

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": false,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select actor_id , first_name FirstName, last_name LastName, last_update LastUpdate   from actor      ",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}
```


### 启用 BasicAuth 安全
开启MyRest BasicAuth, 并设定用户为 `user123`, 密码为 `password123`.
在之后的API请求需要带上 Basic Authorization 请求头, 并指定token.  
下面的示例中 token为 `dXNlcjEyMzpwYXNzd29yZDEyMw==` 是 `user123:password123` 的base64形式. 
```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json
Authorization: Basic dXNlcjEyMzpwYXNzd29yZDEyMw==

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": true,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select count(*) from actor      ",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}

```



### 启用 JwtAuth 安全
对于安全性要求较高的场景, 可以使用 JWT Auth 机制, 这时您还需额外部署一个Auth server用来签发JWT token, MyREST 服务器作为资源服务器. MyREST 要求JWT token必须按 RSA 非对称算法进行签名. 
可通过 <https://dinochiesa.github.io/jwt/> 网站生成的私钥/公钥和token.  实际项目中私钥由Auth server保存并生成JWT token, 公钥保存在 MyREST.toml 文件, 用于验证request的token是否合法并在有效期内. 

下面是MyREST.toml中开启 JwtAuth的配置. 

```
[jwtAuth]
enableJwtAuth=true # enable JWT Auth
audience="myrest client"
validateAudience=true
issuer="some auth server"
validateIssuer=true
publicKey="MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuDnJaGFITMmnFrYrW1ZMK9YXE+9mSoJRPDLGu33VepOaA3+NUWAaASW8xzL+pWWIFLgvLwcMYMIv8YvSRNazlMrpsdtLs7SYEBx5Y1m3RC0p468vtUVyWmnFRoGpG0VRl9n5eNSz4iJm2zb+z6QI5DRd+KVVHLpwwyKWXTHwmgy2mvvT0ohBjKnW8/LX/r0H00qJd5fJVRwKWpGUX4/UhUPwO/lNwKbukV+JHKOQ6ytbgHLCgTISxYtT3dszBVMYeoQ9YlEqVcaUeZKQIynGgwBLEgVUhntvJgSU2DwbC6yPUBl0xXamtZiz93DE5bWrTCgaSqPGRRDOtyEMemHtkQIDAQAB"
```

配置说明:
- 可以自行确定是否要验证JWT token中的 audience 信息(推荐验证). 
- 可以自行确定是否要验证JWT token中的 issuer 信息(推荐验证).
- publicKey 取值, 需要将 PEM-formatted public key 去除掉首行和尾行, 并将中间各行的回车换行符, 下面是个示例: 

PEM-formatted public key 格式的Key
```
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuDnJaGFITMmnFrYrW1ZM
K9YXE+9mSoJRPDLGu33VepOaA3+NUWAaASW8xzL+pWWIFLgvLwcMYMIv8YvSRNaz
lMrpsdtLs7SYEBx5Y1m3RC0p468vtUVyWmnFRoGpG0VRl9n5eNSz4iJm2zb+z6QI
5DRd+KVVHLpwwyKWXTHwmgy2mvvT0ohBjKnW8/LX/r0H00qJd5fJVRwKWpGUX4/U
hUPwO/lNwKbukV+JHKOQ6ytbgHLCgTISxYtT3dszBVMYeoQ9YlEqVcaUeZKQIynG
gwBLEgVUhntvJgSU2DwbC6yPUBl0xXamtZiz93DE5bWrTCgaSqPGRRDOtyEMemHt
kQIDAQAB
-----END PUBLIC KEY-----
```

MyREST.toml文件中的 publicKey:
```
publicKey="MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuDnJaGFITMmnFrYrW1ZMK9YXE+9mSoJRPDLGu33VepOaA3+NUWAaASW8xzL+pWWIFLgvLwcMYMIv8YvSRNazlMrpsdtLs7SYEBx5Y1m3RC0p468vtUVyWmnFRoGpG0VRl9n5eNSz4iJm2zb+z6QI5DRd+KVVHLpwwyKWXTHwmgy2mvvT0ohBjKnW8/LX/r0H00qJd5fJVRwKWpGUX4/UhUPwO/lNwKbukV+JHKOQ6ytbgHLCgTISxYtT3dszBVMYeoQ9YlEqVcaUeZKQIynGgwBLEgVUhntvJgSU2DwbC6yPUBl0xXamtZiz93DE5bWrTCgaSqPGRRDOtyEMemHtkQIDAQAB"
```

Auth Server 基于验证Token请求者的身份后, 为请求者生成资源服务方(MyREST)的访问token, 请求者再带上该token去访问MyREST, MyREST服务会验证该token是否合法并在有效期内. 

```
POST http://localhost:5001/sql HTTP/1.1
content-type: application/json
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImVudGl0bGVtZW50IjoieWxjMzJkMmFpcG8wZnJ1MDJxdWRucGMifQ.eyJpc3MiOiJzb21lIGF1dGggc2VydmVyIiwic3ViIjoibmFpbWlzaCIsImF1ZCI6Im15cmVzdCBjbGllbnQiLCJpYXQiOjE2ODE0ODAyMzIsImV4cCI6MTY4MjQ4MDgzMn0.FPDzCOffg3yNyj1E6BdUJi8r_U2gkU4I07uIi7IZj7VcjkHiSp5O9m5TVZk9y9acv7s55ttrOOSi0ptQRjARQ40xg3z1ZS-pDVHogWhSiY89fYAddGfcd_9_wKqSBDvDyZgbT0rLQxK-KSDOKlXGvnkRVdKCogKDGMVYhA4j3bCPwhMUQZzaQAbvCONy9uPiso50xIJxPSuE4erDKvfNcqrBVs7jX62JKytl8mV4ajIn3GNXbnfcctjwvnnOZAtKKNF2lHUpts6Piwqacd0kVI47oOND2KYd4q_SLmNAHMjTW780NAgt3REoyt_CPS5JEdt4lgp7kw86EI9dxtVUTg

{
  "request": {
    "sqlContext": {
      "db": "sakila",
      "isSelect": true,
      "isScalar": true,
      "sqlFile": "",
      "sqlId": "",
      "sql": "select count(*) from actor      ",  
      "parameters":[]
    },
    "traceId": "1111"
  }
}
```


