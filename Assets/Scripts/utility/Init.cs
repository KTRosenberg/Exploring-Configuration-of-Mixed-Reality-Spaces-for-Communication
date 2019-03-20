﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Text;


public class Init : MonoBehaviour {

	// init glow
	public GameObject glowPrefab;
	public string globalConfigName;

	//init teleportation for Camera Rig
	public GameObject cameraRig;
	public Transform newLocation;

	public InputSystem.DeviceType deviceType;


    private Teleport teleport;

	void Start() {
		var serializer = new XmlSerializer(typeof(Xml2CSharp.GlobalToggle));
		if (File.Exists(globalConfigName)) {
			//}
			var stream = new FileStream(("GlobalConfig.xml"), FileMode.Open);
			//if (stream != null) {
			//Utility.Log("load GlobalConfig.xml", Color.green);
            Utility.Log(2, Color.green, "init", "load GlobalConfig.xml");
			var container = serializer.Deserialize(stream) as Xml2CSharp.GlobalToggle;
			GlobalToggleIns.GetInstance().MRConfig = Utility.StringToConfig(container.MRConfig);
			GlobalToggleIns.GetInstance().username = container.username;
			stream.Close();
			print("change to config:" + GlobalToggleIns.GetInstance().MRConfig);
			GlobalToggleIns.GetInstance().assignToInspector();
		} else {
            Utility.Log(2, Color.red, "init", "GlobalConfig.xml not found, use inspector value directly");
		    Utility.Log(1, Color.white, "init", "SampleGlobalConfig.xml is the example file for you to create GlobalConfig.xml. Create one and put it into root folder.");
		}

		GameObject glowOutline = Instantiate(glowPrefab);
		//teleport = SetUpTeleportation(glowOutline);
	}

	[SerializeField]
	public TransitionUtility.TransitionOverlay transitionOverlay;
	[SerializeField]
	public TransitionUtility.ColorInterp colorInterp;

	//this void attaches the teleportation script to the camera
	private Teleport SetUpTeleportation(GameObject glowOutline) {
		cameraRig.AddComponent<Teleport>();
		Teleport tel = cameraRig.GetComponent<Teleport>();
		tel.newLocation = newLocation;

		tel.transitionOverlay = new TransitionUtility.TransitionOverlay(transitionOverlay);
		// necessary right now since I cannot choose a procedure in the inspector
		tel.interp = new TransitionUtility.ColorInterp(
				TransitionUtility.TwoWay_ColorMiddleFlatline,
				colorInterp.timeDuration
		);

		tel.transitionOverlay.obj.transform.SetParent(Camera.main.transform);
		tel.transitionOverlay.obj.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
		tel.transitionOverlay.obj.transform.localPosition += Camera.main.transform.forward * 0.15f;
		tel.glowOutlineCommand = glowOutline.GetComponent<GlowObjectCmd>();

        return tel;
	}

    private void LateUpdate()
    {
        //if (teleport.InitRemoteAvatars() == true) {
        //    Debug.Log("INIT COMPLETE");
        //    this.gameObject.SetActive(false);
        //}
    }
}
