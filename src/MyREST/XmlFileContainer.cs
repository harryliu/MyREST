using System.Xml;
using System.Xml.Serialization;

namespace MyREST
{
    public class XmlFileContainer
    {
        private bool _hotReload;
        private Dictionary<string, XmlFileParser> xmlParserDictionary;

        public XmlFileContainer(bool hotReload)
        {
            _hotReload = hotReload;
            xmlParserDictionary = new Dictionary<string, XmlFileParser>();
        }

        public void AddFile(string fullFileName)
        {
            if (xmlParserDictionary.ContainsKey(fullFileName) == false)
            {
                var parser = new XmlFileParser(fullFileName, true);
                xmlParserDictionary.Add(fullFileName, parser);
            }
        }

        public XmlFileParser? getParser(string fullFileName)
        {
            if (xmlParserDictionary.ContainsKey(fullFileName))
            {
                XmlFileParser parser = xmlParserDictionary[fullFileName];
                if (_hotReload)
                {
                    parser.parse();
                }
                return parser;
            }
            return null;
        }
    }
}