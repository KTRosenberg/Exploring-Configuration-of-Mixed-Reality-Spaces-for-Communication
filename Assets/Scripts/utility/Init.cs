using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;


public class Init : MonoBehaviour {

    void Awake()
    {
        
    }

    void Start()
    {
        var serializer = new XmlSerializer(typeof(Xml2CSharp.GlobalToggle));
        var stream = new FileStream(Path.Combine(Application.dataPath, "GlobalConfig.xml"), FileMode.Open);
        var container = serializer.Deserialize(stream) as Xml2CSharp.GlobalToggle;
        GlobalToggleIns.GetInstance().MRConfig = Utility.StringToConfig(container.MRConfig);
        stream.Close();
    }
}
