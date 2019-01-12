using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

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
        //print("Tracked:" + Tracked);
        if (Tracked)
        {
            //receivedMsg = Encoding.Default.GetString(data.bytes);
            decode();
        }
    }

    void decode()
    {
        // #0 for resolution
        // #1 for reset ownership
        // #2 for creating sketchPage
        int cmdNumber = BitConverter.ToInt16(data.bytes, 0);
        print("command number:" + cmdNumber);
        switch (cmdNumber)
        {
            case 0:
                // resolution
                Vector2Int res = ParseDisplayInfo(data.bytes, 2);
                GlobalToggleIns.GetInstance().ChalktalkRes = res;
                break;
            default:
                break;
        }
    }

    Vector2Int ParseDisplayInfo(byte[] bytes, int offset = 0)
    {

        if (bytes.Length >= (offset))
        {
            int cursor = offset;
            int resW = Utility.ParsetoInt16(bytes, cursor);
            cursor += 2;
            int resH = Utility.ParsetoInt16(bytes, cursor);
            return new Vector2Int(resW, resH);
        }
        return new Vector2Int(0, 0);
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        Sync();
    }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, 0, false
        );
    }
}
