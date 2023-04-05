using System.Xml.Serialization;

namespace MyREST
{
    public class FieldMeta
    {
        public string name { get; set; }
        public int index { get; set; }
        public string dataType { get; set; }
    }

    //public class SqlParameter
    //{
    //    public string name { get; set; }
    //    public string value { get; set; }
    //    public string dataType { get; set; }
    //    public string direction { get; set; }
    //    public string format { get; set; }

    //    //direction: out/in/return
    //}

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
    }

    public class SqlRequestWrapper
    {
        public SqlRequest request { get; set; }
    }

    public class SqlResultWrapper
    {
        public SqlRequest request { get; set; }
        public SqlResponse response { get; set; }
    }

    public class SqlContext
    {
        public string db { get; set; } = "";
        public bool isSelect { get; set; } = true;
        public string sqlFile { get; set; } = "";
        public string sqlId { get; set; } = "";
        public string sql { get; set; } = "";
        public List<SqlParameter> parameters { get; set; }
        public bool requireTransaction { get; set; } = false;

        private bool useClientSql = false;

        public bool isUseClientSql()
        {
            return useClientSql;
        }

        public void setUseClientSql(bool value)
        {
            useClientSql = value;
        }
    }

    public class SqlRequest
    {
        public SqlContext sqlContext { get; set; }
        public string traceId { get; set; }
    }

    public class SqlResponse
    {
        //public List<FieldMeta> meta { get; set; }
        public int returnCode { get; set; }

        public string errorMessage { get; set; }
        public int rowCount { get; set; }
        public int affectedCount { get; set; }

        public IEnumerable<dynamic> rows { get; set; }
    }

    [XmlRoot(ElementName = "parameters")]
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
}