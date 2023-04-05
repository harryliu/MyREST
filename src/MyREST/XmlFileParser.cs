using System.Xml;
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

        public XmlNode? getSqlXmlNode(string sqlId)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_fullFileName);
            string xmlPath = $"/root/sql[@id='{sqlId}']";
            XmlNode? node = xmlDoc.SelectSingleNode(xmlPath);
            return node;
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
        /// (1)If request contains sqlFile and sqlId values, the server side sql statement  will be used in priority
        /// (2)The client side parameters always be used
        /// </summary>
        /// <param name="sqlContext"></param>
        /// <param name="sqlId"></param>
        /// <returns></returns>
        public SqlContext rebuildSqlContext(SqlContext sqlContext, string sqlId)
        {
            XmlSql? xmlSql = getSqlNodeById(sqlId);
            if (xmlSql != null)
            {
                sqlContext.setFromClientSql(false);
                sqlContext.sql = xmlSql.text;

                //first, merge client side parameters into server side
                mergeClientParamToServer(sqlContext, xmlSql);

                //merge, merge server side parameters into client side
                mergeServerParamToClient(sqlContext, xmlSql);
            }
            else
            {
                sqlContext.setFromClientSql(true);
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
                    throw new ArgumentException($"Expected 0 or 1 parameter {clientParam.name.Trim()} in server side, but {serverParams.Count()} found.");
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
                    throw new ArgumentException($"Expected 0 or 1 parameter {serverParam.name.Trim()} in client side, but {clientParams.Count()} found.");
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

        public XmlSql? getSqlNodeById(string sqlId)
        {
            if (_xmlFileRoot != null)
            {
                var xmlSqls = from sql in _xmlFileRoot.sqlList where sql.id.Trim() == sqlId.Trim() select sql;
                if (xmlSqls.Count() != 1)
                {
                    throw new ArgumentException($"Expected one Sql node, but {xmlSqls.Count()} found.");
                }
                else
                {
                    return xmlSqls.First();
                }
            }
            return null;
        }
    }
}