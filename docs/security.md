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

## JWT Auth 验证
对于安全性要求较高的场景, 可以使用 JWT Auth 机制, 这时您还需额外部署一个Auth server用来签发JWT token, MyREST 服务器作为资源服务器. MyREST 要求JWT token必须按 RSA 非对称算法进行签名. 

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
