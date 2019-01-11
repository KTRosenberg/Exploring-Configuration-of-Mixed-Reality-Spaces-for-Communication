using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MSGReceiver : Holojam.Tools.SynchronizableTrackable
{
    [SerializeField] string label = "MSGRcv";
    [SerializeField] string scope = "";

    [SerializeField] bool host = false;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    public string receivedMsg;

    // Override Sync()
    protected override void Sync()
    {
        if (Tracked)
        {
            receivedMsg = Encoding.Default.GetString(data.bytes);
        }
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        base.Update();
    }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, 0, false
        );
    }
}
