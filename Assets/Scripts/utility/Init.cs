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

    void Start()
    {
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

        GameObject glowOutline = Instantiate(glowPrefab);
        SetUpTeleportation(glowOutline);
    }

    [SerializeField]
    public Teleport.TransitionOverlay transitionOverlay;
    [SerializeField]
    public Teleport.ColorInterp colorInterp;

    //this void attaches the teleportation script to the camera
    private void SetUpTeleportation(GameObject glowOutline)
    {
        cameraRig.AddComponent<Teleport>();
        Teleport tel = cameraRig.GetComponent<Teleport>();
        tel.newLocation = newLocation;

        tel.transitionOverlay = new Teleport.TransitionOverlay(transitionOverlay);
        // necessary right now since I cannot choose a procedure in the inspector
        tel.interp = new Teleport.ColorInterp(
            Utility.TwoWay_ColorMiddleFlatline,
            colorInterp.timeDuration
        );
        
        tel.transitionOverlay.obj.transform.SetParent(Camera.main.transform);
        tel.transitionOverlay.obj.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        tel.transitionOverlay.obj.transform.localPosition += Camera.main.transform.forward * 0.15f;
        tel.glowOutlineCommand = glowOutline.GetComponent<GlowObjectCmd>();
    }
}
