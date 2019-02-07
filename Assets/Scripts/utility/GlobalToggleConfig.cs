using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "GlobalToggle")]
    public class GlobalToggle
    {
        [XmlElement(ElementName = "MRConfig")]
        public string MRConfig { get; set; }
    }

}
