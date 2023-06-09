﻿using System.Data;
using System.Text;
using System.Xml.Serialization;

namespace MyREST
{
    public class EndpointContext
    {
        public string name { get; set; }
        public bool enabled { get; set; } = true;
        public bool needFirewallCheck { get; set; } = true;
        public bool needBasicAuthCheck { get; set; } = true;
        public bool needJwtAuthCheck { get; set; } = true;
        public bool onlyAllowSelect { get; set; } = false;
        public bool onlyServerSideSql { get; set; } = false;
        public int rowCountLimit { get; set; } = -1;
    }

    [XmlRoot(ElementName = "parameter")]
    public class SqlParameter
    {
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string value { get; set; }

        [XmlAttribute(AttributeName = "dataType")]
        public string dataType { get; set; }

        [XmlAttribute(AttributeName = "direction")]
        public string direction { get; set; }

        [XmlAttribute(AttributeName = "format")]
        public string format { get; set; }

        [XmlAttribute(AttributeName = "separator")]
        public string separator { get; set; }
    }

    [ToString]
    public class SqlRequestWrapper
    {
        public SqlRequest request { get; set; }
    }

    [ToString]
    public class SqlResultWrapper
    {
        public SqlRequest request { get; set; }
        public SqlResponse response { get; set; }
    }

    [ToString]
    public class SqlContext
    {
        public string db { get; set; } = "";
        public bool isSelect { get; set; } = true;
        public bool isScalar { get; set; } = false;
        public string sqlFile { get; set; } = "";
        public string sqlId { get; set; } = "";
        public string sql { get; set; } = "";

        public List<SqlParameter> parameters { get; set; }

        private bool useClientSql = true; //default true

        public bool isUseClientSql()
        {
            return useClientSql;
        }

        public void setUseClientSql(bool value)
        {
            useClientSql = value;
        }

        public string getPlainSql()
        {
            if (sqlIsBase64() == true)
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(sql);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            return sql;
        }

        public string getBase64Sql()
        {
            if (sqlIsBase64() == false)
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(sql);
                return System.Convert.ToBase64String(byteArray);
            }
            return sql;
        }

        private bool sqlIsBase64()
        {
            bool isBase64 = false;
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                try
                {
                    Convert.FromBase64String(sql);
                    isBase64 = true;
                }
                catch (FormatException)
                {
                    // The input string was not Base64-encoded
                }
            }
            return isBase64;
        }
    }

    [ToString]
    public class SqlRequest
    {
        public SqlContext sqlContext { get; set; }
        public string traceId { get; set; }
    }

    [ToString]
    public class SqlResponse
    {
        //public List<FieldMeta> meta { get; set; }
        public int returnCode { get; set; }

        public string errorMessage { get; set; }
        public int rowCount { get; set; }
        public int affectedCount { get; set; }

        public dynamic scalarValue { get; set; }
        public IEnumerable<dynamic> rows { get; set; }
    }

    public class XmlSqlParameters
    {
        [XmlElement(ElementName = "parameter")]
        public List<SqlParameter> parameterList { get; set; }
    }

    [XmlRoot(ElementName = "sql")]
    public class XmlSql
    {
        [XmlElement(ElementName = "parameters")]
        public XmlSqlParameters parameters { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }

        [XmlText]
        public string text { get; set; }
    }

    [XmlRoot(ElementName = "root")]
    public class XmlFileRoot
    {
        [XmlElement(ElementName = "sql")]
        public List<XmlSql> sqlList { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public double version { get; set; }

        [XmlText]
        public string text { get; set; }
    }

    public class DapperParameterItem
    {
        public string name { get; set; }
        public object value { get; set; }
        public DbType? dataType { get; set; }
        public ParameterDirection? direction { get; set; }
    }
}