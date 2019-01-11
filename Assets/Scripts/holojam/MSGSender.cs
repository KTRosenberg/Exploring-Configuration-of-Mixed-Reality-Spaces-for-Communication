using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class MSGSender : Holojam.Tools.SynchronizableTrackable
{

    [SerializeField] string label = "MSGSender";
    [SerializeField] string scope = "";

    [SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    // Override Sync()
    protected override void Sync()
    {
        if (Sending)
        {

        }
        else
        {
            
        }
    }

    public void Send(string msg)
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, msg.Length * sizeof(char), false
        );
        data.bytes = Encoding.ASCII.GetBytes(msg);
        host = true;
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        //base.Update();
        host = false;
    }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, 0, false
        );
    }
}
