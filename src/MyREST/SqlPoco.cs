namespace MyREST
{
    public class FieldMeta
    {
        public string name { get; set; }
        public int index { get; set; }
        public string type { get; set; }
    }

    public class SqlParameter
    {
        public string name { get; set; }
        public string value { get; set; }
        public string dbType { get; set; }
        public string direction { get; set; }
        public string format { get; set; }

        //direction: out/in/return
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

    public class SqlRequest
    {
        public string command { get; set; } //execute/query
        public string sqlFile { get; set; }
        public string sqlId { get; set; }
        public List<SqlParameter> parameters { get; set; }
        public string traceId { get; set; }
        public bool requireTransaction { get; set; }
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
}