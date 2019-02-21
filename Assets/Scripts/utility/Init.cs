using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Text;


public class Init : MonoBehaviour {

    // init glow
    public GameObject glowPrefab;

    //init teleportation for Camera Rig
    public GameObject cameraRig;
    public Transform newLocation;

    void Awake()
    {
        
    }

    public bool createDefaultConfigFileIfDoesNotExist = false;
    void GenerateDefaultConfigFile()
    {
        FileStream stream;
        if (!File.Exists("GlobalConfig.xml")) {
            byte[] defaultGlobalConfigXML = GlobalToggleIns.GetDefaultConfigXMLBytes();

            stream = new FileStream("GlobalConfig.xml", FileMode.CreateNew);

            stream.Write(defaultGlobalConfigXML, 0, defaultGlobalConfigXML.Length);
            stream.Flush();
            stream.Close();

            Debug.Log("<color=green>wrote default GlobalConfig.xml</color>");
        }
    }
    void Start()
    {
        var serializer = new XmlSerializer(typeof(Xml2CSharp.GlobalToggle));

        if(createDefaultConfigFileIfDoesNotExist) {
            GenerateDefaultConfigFile();
        }
        FileStream stream = null;
        if (File.Exists("GlobalConfig.xml")) {
            stream = new FileStream(("GlobalConfig.xml"), FileMode.Open);
            var container = serializer.Deserialize(stream) as Xml2CSharp.GlobalToggle;
            GlobalToggleIns.GetInstance().MRConfig = Utility.StringToConfig(container.MRConfig);
            GlobalToggleIns.GetInstance().username = container.username;
            stream.Close();
            print("change to config:" + GlobalToggleIns.GetInstance().MRConfig);
            GlobalToggleIns.GetInstance().assignToInspector();

        }

        Instantiate(glowPrefab); //create the glow outline
        SetUpTeleportation();

    }

    //this void attaches the teleportation script to the camera
    private void SetUpTeleportation()
    {
        cameraRig.AddComponent<Teleport>();
        Teleport t = cameraRig.GetComponent<Teleport>();
        t.newLocation = newLocation;
    }
}
