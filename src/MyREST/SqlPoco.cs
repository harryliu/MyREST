namespace MyREST
{
    public class FieldMeta
    {
        public string name { get; set; }
        public int index { get; set; }
        public string type { get; set; }
    }

    public class FieldData
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class SqlParameter
    {
        public string name { get; set; }
        public string value { get; set; }
        public string type { get; set; }
        public string format { get; set; }

        //direction: out/in/return
    }

    public class SqlRequest
    {
        public string command { get; set; } //execute/query
        public string sqlFile { get; set; }
        public string xmlPath { get; set; }

        public List<SqlParameter> parameters { get; set; }
        public string traceId { get; set; }
        public bool returnRequestBody { get; set; }
    }

    public class SqlResponse
    {
        public List<FieldMeta> meta { get; set; }
        public string returnCode { get; set; }
        public string errorMessage { get; set; }
        public int rowCount { get; set; }
        public int affectedCount { get; set; }

        public List<FieldData> rows { get; set; }
    }
}