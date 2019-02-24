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

    //init teleportation for Camera Rig
    public GameObject cameraRig;
    public Transform newLocation;

    public InputSystem.DeviceType deviceType;

    public bool useConfigFile = true;

    void Start()
    {
        if (useConfigFile) {
            var serializer = new XmlSerializer(typeof(Xml2CSharp.GlobalToggle));
            var stream = new FileStream(("GlobalConfig.xml"), FileMode.Open);
            if (stream != null) {
                Debug.Log("<color=green>load GlobalConfig.xml</color>");
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
        }



        Instantiate(glowPrefab);
        SetUpTeleportation();
    }

    public InterpOverlay overlayPrefab;

    //this void attaches the teleportation script to the camera
    private void SetUpTeleportation()
    {
        cameraRig.AddComponent<Teleport>();
        Teleport t = cameraRig.GetComponent<Teleport>();
        t.newLocation = newLocation;

        t.transitionOverlay = Instantiate(overlayPrefab);
        t.transitionOverlay.name = "ViewOverlay";
        t.transitionOverlay.transform.SetParent(Camera.main.transform);
    }
}
