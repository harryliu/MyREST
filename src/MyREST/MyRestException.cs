namespace MyREST
{
    public class MyRestException : Exception
    {
        public MyRestException(string message) : base(message)
        {
        }

        public virtual int getErrorCode()
        {
            return 1;
        }
    }

    public class TomlFileException : MyRestException
    {
        public TomlFileException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 2;
        }
    }

    public class XmlFileException : MyRestException
    {
        public XmlFileException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 3;
        }
    }

    public class RequestArgumentException : MyRestException
    {
        public RequestArgumentException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 4;
        }
    }

    public class SecurityException : MyRestException
    {
        public SecurityException(string message) : base(message)
        {
        }

        public override int getErrorCode()
        {
            return 5;
        }
    }

    public class SqlExecuteException : MyRestException
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