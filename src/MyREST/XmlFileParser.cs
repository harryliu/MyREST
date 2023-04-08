using Dapper;
using System.Data;
using System.Globalization;
using System.Xml.Serialization;

namespace MyREST
{
    public class XmlFileParser
    {
        private string _fullFileName;
        private XmlFileRoot? _xmlFileRoot;

        public XmlFileParser(string fullFileName, bool autoParse)
        {
            _fullFileName = fullFileName;
            if (autoParse)
            {
                parse();
            }
        }

        //public XmlNode? getSqlXmlNode(string sqlId)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.Load(_fullFileName);
        //    string xmlPath = $"/root/sql[@id='{sqlId}']";
        //    XmlNode? node = xmlDoc.SelectSingleNode(xmlPath);
        //    return node;
        //}

        private XmlSql? getXmlSqlById(string sqlId)
        {
            if (_xmlFileRoot != null)
            {
                var xmlSqls = from sql in _xmlFileRoot.sqlList where sql.id.Trim() == sqlId.Trim() select sql;
                if (xmlSqls.Count() != 1)
                {
                    throw new XmlFileException($"Expected one Sql node with id={sqlId}, but {xmlSqls.Count()} found.");
                }
                else
                {
                    return xmlSqls.First();
                }
            }
            return null;
        }

        /// <summary>
        /// parse the whole Sql file into XmlFileRoot object
        /// </summary>
        public void parse()
        {
            _xmlFileRoot = null;
            String xmlString = System.IO.File.ReadAllText(_fullFileName);
            XmlSerializer serializer = new XmlSerializer(typeof(XmlFileRoot));
            if (String.IsNullOrWhiteSpace(xmlString) == false)
            {
                using (StringReader reader = new StringReader(xmlString))
                {
                    var root = (XmlFileRoot?)serializer.Deserialize(reader);
                    _xmlFileRoot = root;
                }
            }
        }

        /// <summary>
        /// rebuild SqlContext, including:
        /// (1)for Sql statement, the server side sql statement will be used in priority
        /// (2)for Sql parameters, the client side parameters will be used in priority
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="sqlId"></param>
        /// <returns></returns>
        public SqlContext rebuildSqlContext(SqlContext sqlContext, string sqlId)
        {
            XmlSql? xmlSql = getXmlSqlById(sqlId);
            if (xmlSql != null)
            {
                sqlContext.setUseClientSql(false);

                //use serverSql in priority
                sqlContext.sql = xmlSql.text.Trim();

                //firstly, merge client side parameters into server side
                mergeClientParamToServer(sqlContext, xmlSql);

                //secondly, merge server side parameters into client side
                mergeServerParamToClient(sqlContext, xmlSql);
            }
            else
            {
                sqlContext.setUseClientSql(true);
            }
            return sqlContext;
        }

        private void mergeServerParamToClient(SqlContext sqlContext, XmlSql xmlSql)
        {
            foreach (var clientParam in sqlContext.parameters)
            {
                var serverParams = from item in xmlSql.parameters.parameterList
                                   where item.name.Trim() == clientParam.name.Trim()
                                   select item;
                if (serverParams.Count() > 1)
                {
                    throw new XmlFileException($"Expected 0 or 1 parameter {clientParam.name.Trim()} in server side, but {serverParams.Count()} found.");
                }
                else if (serverParams.Count() == 1)
                {
                    var serverParam = serverParams.First();
                    clientParam.value = serverParam.value;
                    if (String.IsNullOrWhiteSpace(clientParam.dataType))
                    {
                        clientParam.dataType = serverParam.dataType;
                    }
                    if (String.IsNullOrWhiteSpace(clientParam.direction))
                    {
                        clientParam.direction = serverParam.direction;
                    }
                    if (String.IsNullOrWhiteSpace(clientParam.format))
                    {
                        clientParam.format = serverParam.format;
                    }
                }
                else
                {
                    //nothing
                }
            }
        }

        private void mergeClientParamToServer(SqlContext sqlContext, XmlSql xmlSql)
        {
            foreach (var serverParam in xmlSql.parameters.parameterList)
            {
                var clientParams = from item in sqlContext.parameters
                                   where item.name.Trim() == serverParam.name.Trim()
                                   select item;
                if (clientParams.Count() > 1)
                {
                    throw new XmlFileException($"Expected 0 or 1 parameter {serverParam.name.Trim()} in client side, but {clientParams.Count()} found.");
                }
                else if (clientParams.Count() == 1)
                {
                    var clientParam = clientParams.First();
                    serverParam.value = clientParam.value;
                    if (String.IsNullOrWhiteSpace(serverParam.dataType))
                    {
                        serverParam.dataType = clientParam.dataType;
                    }
                    if (String.IsNullOrWhiteSpace(serverParam.direction))
                    {
                        serverParam.direction = clientParam.direction;
                    }
                    if (String.IsNullOrWhiteSpace(serverParam.format))
                    {
                        serverParam.format = clientParam.format;
                    }
                }
                else
                {
                    //nothing
                }
            }
        }

        private static ParameterDirection? getDapperDirection(string oldDirection)
        {
            ParameterDirection? dapperDirection = null;
            if (string.IsNullOrWhiteSpace(oldDirection) == false)
            {
                var direction = oldDirection.Trim().ToLower();
                if (direction == "Input".ToLower())
                {
                    dapperDirection = ParameterDirection.Input;
                }
                else if (direction == "Output".ToLower())
                {
                    dapperDirection = ParameterDirection.Output;
                }
                else if (direction == "InputOutput".ToLower())
                {
                    dapperDirection = ParameterDirection.InputOutput;
                }
                else if (direction == "ReturnValue".ToLower())
                {
                    dapperDirection = ParameterDirection.ReturnValue;
                }
                else
                {
                    throw new MyRestException($"invalid parameter direction {oldDirection} . It must be assigned as Input/OutputInputOutput/ReturnValue");
                }
            }
            return dapperDirection;
        }

        private static void convertDapperDataType(string oldType, string oldValue, string format, out DbType? newType, out object? newValue)
        {
            newType = null;
            newValue = null;
            var exceptionMessage = $"invalid parameter dataType {oldType}. Only some common kinds of dataType are supported at https://learn.microsoft.com/en-us/dotnet/api/system.data.dbtype";
            if (string.IsNullOrWhiteSpace(oldType) == false)
            {
                var dataType = oldType.Trim().ToLower();
                if (dataType == "AnsiString".ToLower())
                {
                    newType = DbType.AnsiString;
                    newValue = oldValue;
                }
                else if (dataType == "Binary".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "Byte".ToLower())
                {
                    newType = DbType.Byte;
                    newValue = Convert.ToByte(oldValue);
                }
                else if (dataType == "Boolean".ToLower())
                {
                    newType = DbType.Boolean;
                    newValue = Convert.ToBoolean(oldValue);
                }
                else if (dataType == "Currency".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "Date".ToLower())
                {
                    newType = DbType.Date;
                    newValue = DateTime.ParseExact(oldValue, format, CultureInfo.InvariantCulture);
                }
                else if (dataType == "DateTime".ToLower())
                {
                    newType = DbType.DateTime;
                    newValue = DateTime.ParseExact(oldValue, format, CultureInfo.InvariantCulture);
                }
                else if (dataType == "Decimal".ToLower())
                {
                    newType = DbType.Decimal;
                    newValue = Convert.ToDecimal(oldValue);
                }
                else if (dataType == "Double".ToLower())
                {
                    newType = DbType.Double;
                    newValue = Convert.ToDouble(oldValue);
                }
                else if (dataType == "Guid".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "Int16".ToLower())
                {
                    newType = DbType.Int16;
                    newValue = Convert.ToInt16(oldValue);
                }
                else if (dataType == "Int32".ToLower())
                {
                    newType = DbType.Int32;
                    newValue = Convert.ToInt32(oldValue);
                }
                else if (dataType == "Int64".ToLower())
                {
                    newType = DbType.Int64;
                    newValue = Convert.ToInt64(oldValue);
                }
                else if (dataType == "Object".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "SByte".ToLower())
                {
                    newType = DbType.SByte;
                    newValue = Convert.ToSByte(oldValue);
                }
                else if (dataType == "Single".ToLower())
                {
                    newType = DbType.Single;
                    newValue = Convert.ToSingle(oldValue);
                }
                else if (dataType == "String".ToLower())
                {
                    newType = DbType.String;
                    newValue = oldValue;
                }
                else if (dataType == "Time".ToLower())
                {
                    newType = DbType.Time;
                    newValue = DateTime.ParseExact(oldValue, format, CultureInfo.InvariantCulture);
                }
                else if (dataType == "UInt16".ToLower())
                {
                    newType = DbType.UInt16;
                    newValue = Convert.ToUInt16(oldValue);
                }
                else if (dataType == "UInt32".ToLower())
                {
                    newType = DbType.UInt32;
                    newValue = Convert.ToUInt32(oldValue);
                }
                else if (dataType == "UInt64".ToLower())
                {
                    newType = DbType.UInt64;
                    newValue = Convert.ToUInt64(oldValue);
                }
                else if (dataType == "VarNumeric".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "AnsiStringFixedLength".ToLower())
                {
                    newType = DbType.AnsiStringFixedLength;
                    newValue = oldValue;
                }
                else if (dataType == "StringFixedLength".ToLower())
                {
                    newType = DbType.StringFixedLength;
                    newValue = oldValue;
                }
                else if (dataType == "Xml".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "DateTime2".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "DateTimeOffset".ToLower())
                {
                    throw new MyRestException(exceptionMessage);
                }
                else if (dataType == "String Array".ToLower())
                {
                    newType = DbType.String;
                    newValue = oldValue.Split(format);
                }
                else if (dataType == "Decimal Array".ToLower())
                {
                    newType = DbType.Decimal;
                    string[] strArray = oldValue.Split(format);
                    newValue = new List<decimal>();
                    foreach (var item in strArray)
                    {
                        newValue = Convert.ToDecimal(item);
                    }
                }
                else
                {
                    throw new MyRestException(exceptionMessage);
                }
            }
        }

        public static DynamicParameters buildDapperParameters(SqlContext sqlContext)
        {
            DynamicParameters dapperParameters = new DynamicParameters();
            foreach (var param in sqlContext.parameters)
            {
                DbType? dapperDataType = null;
                object? newValue = null;
                ParameterDirection? dapperDirection = getDapperDirection(param.direction);
                convertDapperDataType(param.dataType, param.value, param.format, out dapperDataType, out newValue);
                dapperParameters.Add(param.name, newValue, dapperDataType, dapperDirection);
            }

            return dapperParameters;
        }
    }
}