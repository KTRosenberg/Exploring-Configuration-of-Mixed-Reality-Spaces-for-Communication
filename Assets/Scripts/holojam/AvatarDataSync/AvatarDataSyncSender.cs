﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarDataSyncSender : Holojam.Tools.SynchronizableTrackable {

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
    [SerializeField] bool host = true;
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

        if (GlobalToggleIns.GetInstance().username != "") {
            label = "AvatarTransit_" + GlobalToggleIns.GetInstance().username;
        }
    }

    protected override void Sync()
    {
        host = true;
        if (label == "AvatarTransit" && GlobalToggleIns.GetInstance().username != "") {
            label = "AvatarTransit_" + GlobalToggleIns.GetInstance().username;
        }

        isTracked = true;

        SetSendData();
    }

    protected override void Update()
    {
        base.Update();
        host = true;
    }

    public override void ResetData()
    {
        label = "AvatarTransit_" + GlobalToggleIns.GetInstance().username;
        data = new Holojam.Network.Flake(2, 1, 0, 0, 1, false);
    }

    public void SetSendData()
    {
        Transform xform = Camera.main.transform;
        data.vector3s[0] = xform.position;
        data.vector3s[1] = xform.forward;
        data.vector4s[0] = xform.rotation;
        data.bytes[0]    = 0; // TODO

        TransitUserData transit = new TransitUserData();
        transit.position = data.vector3s[0];
        transit.forward  = data.vector3s[1];
        transit.rotation = data.vector4s[0];
        transit.flags    = data.bytes[0];

        //Debug.Log("SetSendData()");
        Debug.Log(transit.ToString());
    }
}
