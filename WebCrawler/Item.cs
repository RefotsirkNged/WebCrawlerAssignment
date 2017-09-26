using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WebCrawler
{
    public class Item
    {
        [XmlAttribute]
        public string key;
        [XmlAttribute]
        public string value;
    }
}
