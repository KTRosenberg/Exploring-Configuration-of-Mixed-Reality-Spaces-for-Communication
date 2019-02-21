using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Text;


public class Init : MonoBehaviour
{

    // init glow
    public GameObject glowPrefab;
    public string globalConfigName;

    void Awake()
    {

    }


    void Start()
    {
        var serializer = new XmlSerializer(typeof(Xml2CSharp.GlobalToggle));

        FileStream stream;
        if (File.Exists(globalConfigName)) {
            Debug.Log("<color=green>load GlobalConfig.xml</color>");
            stream = new FileStream(("GlobalConfig.xml"), FileMode.Open);
            var container = serializer.Deserialize(stream) as Xml2CSharp.GlobalToggle;
            GlobalToggleIns.GetInstance().MRConfig = Utility.StringToConfig(container.MRConfig);
            GlobalToggleIns.GetInstance().username = container.username;
            stream.Close();
            print("change to config:" + GlobalToggleIns.GetInstance().MRConfig);
            GlobalToggleIns.GetInstance().assignToInspector();
        }
        else {
            Debug.Log("<color=red>GlobalConfig.xml not found, use inspector value directly.</color>");
            Debug.Log("<color=red>SampleGlobalConfig.xml is the example file for you to create GlobalConfig.xml. Create one and put it into root folder.</color>");
        }

        Instantiate(glowPrefab);
    }
}
