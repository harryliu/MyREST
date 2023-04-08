namespace MyREST
{
    public class RestException : Exception
    {
        public RestException(string message) : base(message)
        {
        }

        public virtual int getErrorCode()
        {
            return 1;
        }
    }

    public class TomlFileException : RestException
    {
        public TomlFileException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 2;
        }
    }

    public class XmlFileException : RestException
    {
        public XmlFileException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 3;
        }
    }

    public class RequestArgumentException : RestException
    {
        public RequestArgumentException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 4;
        }
    }

    public class SecurityException : RestException
    {
        public SecurityException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 5;
        }
    }

    public class SqlExecuteException : RestException
    {
        public SqlExecuteException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 6;
        }
    }
}