## 防火墙设置
可以启用防火墙, 设置信任的incoming IP, 可以按需启用白名单和黑名单机制, 甚至可以同时启用白名单和黑名单. 
```toml
[firewall]
enableIpWhiteList=true # enable IP white list
enableIpBlackList=true # enable IP black list
ipWhiteList=["127.0.0.1","192.168.0.1"]
ipBlackList=["127.0.0.2","192.168.0.156"]
```

## Basic Auth 验证
除了防火墙安全保护机制, MyREST 还提供Basic Auth 和JWT Auth两种身份鉴证机制, Basic Auth 和JWT Auth两种机制不能同时启用. 
对于安全性要求不太高的场景, 可以使用 Basic Auth 机制, 客户端使用非常方便. 
```toml
[basicAuth]
enableBasicAuth=false # enable Basic Auth
username="user123"
password="password123"
```

在Basic Auth 验证之后的API请求需要带上 Basic Authorization 请求头, 并指定token.  
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


## JWT Auth 验证
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

