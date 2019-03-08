using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarDataSyncReceiver : Holojam.Tools.SynchronizableTrackable
{
    public string label = "AvatarTransit";

    [SerializeField] string scope = "";

    [HideInInspector]
    public GameObject localAvatarGameObject;
    [HideInInspector]
    public OvrAvatar localAvatar;
    [HideInInspector]
    public OculusManager om;
    [HideInInspector]
    public TransitUserData localDataToSend;

    public bool isTracked;
    [SerializeField] bool host = false;
    [SerializeField] bool autoHost = false;

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    void Start()
    {
        localAvatarGameObject = GameObject.Find("LocalAvatar");
        localAvatar = localAvatarGameObject.GetComponent<OvrAvatar>();
        om = localAvatar.GetComponent<OculusManager>();
    }

    protected override void Sync()
    {
        host = false;

        if (label == "AvatarTransit" && om.remoteNames.Count > 0) {
            label = "AvatarTransit_" + om.remoteNames[0]; // TODO just pick the first remote
        }

        isTracked = true;

        if (!om.alternativeViewEnabled) {
            return;
        }

        TransitUserData remoteData;
        Debug.Log(om.remoteNames.Count);
        if (om.usernameToUserDataMap.TryGetValue(om.remoteNames[0], out remoteData)) {
            Debug.Log("<color=green>FOUND remoteData for" + om.remoteNames[0]);
            GetReceivedData(remoteData);
        }
    }

    protected override void Update()
    {
        base.Update();
        host = false;
    }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(2, 1, 0, 0, 1, false);
    }


    public GameObject testObject;
    public void GetReceivedData(TransitUserData transit)
    {
        transit.position = data.vector3s[0];
        transit.forward  = data.vector3s[1];
        transit.rotation = data.vector4s[0];
        transit.flags    = data.bytes[0];

        Debug.Log("updating receive data");
        Debug.Log("<color=green>" + transit.ToString() + "</color>");

        if (testObject == null) {
            return;
        }
        testObject.transform.position = transit.position;
        testObject.transform.rotation = transit.rotation;
    }
}
