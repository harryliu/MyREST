using GlobExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using RSAExtensions;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace MyREST.Plugin
{
    /// <summary>
    /// JWT auth, use RSA
    /// https://blog.csdn.net/HongzhuoO/article/details/115290559
    /// </summary>
    public class JwtAuthPlugin : SecurityPlugin
    {
        private JwtAuthConfig _jwtAuthConfig;
        private SigningCredentials _signingCredentials;
        private TokenValidationParameters _validationParameters;
        private JwtSecurityTokenHandler _tokenHandler;

        public JwtAuthPlugin(ILogger<SecurityPlugin> logger, IConfiguration configuration, GlobalConfig globalConfig) :
            base(logger, configuration, globalConfig)
        {
            _jwtAuthConfig = globalConfig.jwtAuth;

            _tokenHandler = new JwtSecurityTokenHandler();

            //非对称密钥
            var rsa = RSA.Create();
            // "公钥Base64(掐头去尾,不带Begin...以及回车空格等格式的)"
            byte[] publicKey = Convert.FromBase64String(_jwtAuthConfig.publicKey);
            //rsa.ImportPkcs8PublicKey 这是一个扩展方法,来源于 RSAExtensions 包，
            //大家可以关注一下这位大哥的Github  https://github.com/stulzq/RSAExtensions 。这个包提供了导入 pkcs8 格式公钥的方法。
            rsa.ImportPkcs8PublicKey(publicKey);
            var sKey = new RsaSecurityKey(rsa);

            //非对称密钥
            _signingCredentials = new SigningCredentials(sKey, SecurityAlgorithms.RsaPKCS1);

            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = _jwtAuthConfig.validateIssuer,
                ValidIssuer = _jwtAuthConfig.issuer,
                ValidateAudience = _jwtAuthConfig.validateAudience,
                ValidAudience = _jwtAuthConfig.audience,
                ValidateIssuerSigningKey = _jwtAuthConfig.validateIssuerSigningKey,
                IssuerSigningKey = _signingCredentials.Key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(0.5),
            };
        }

        protected override bool internalCheck(HttpContext httpContext, out string checkMessage)
        {
            _logger.LogInformation("Begin to do jwtAuth check ");
            bool result = jwtAuthCheck(httpContext, out checkMessage);
            _logger.LogInformation(checkMessage);
            return result;
        }

        private string getToken(HttpContext httpContext)
        {
            string result = "";
            var authHeaders = httpContext.Request.Headers["Authorization"];
            foreach (var header in authHeaders)
            {
                if (string.IsNullOrWhiteSpace(header) == false && header.StartsWith("Bearer "))
                {
                    result = header.Substring("Bearer ".Length).Trim();
                }
            }
            return result;
        }

        private bool jwtAuthCheck(HttpContext httpContext, out string checkMessage)
        {
            checkMessage = "JwtAuth check bypassed";
            if (_jwtAuthConfig.enableJwtAuth == false)
            {
                return true;
            }

            string token = getToken(httpContext);
            if (string.IsNullOrWhiteSpace(token))
            {
                checkMessage = "JwtAuth check failed because of missed authorization header";
                return false;
            }

            _tokenHandler.ValidateToken(token, _validationParameters, out _);
            checkMessage = "JwtAuth check passed";
            return true;
        }
    }
}