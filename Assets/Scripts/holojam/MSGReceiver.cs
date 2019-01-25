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

    GameObject localAvatar;

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
        // #3 for receiving avatar name
        int cmdNumber = BitConverter.ToInt16(data.bytes, 0);
        print("command number:" + cmdNumber);
        switch (cmdNumber)
        {
            case 0:
                // resolution
                Vector2Int res = ParseDisplayInfo(data.bytes, 2);
                GlobalToggleIns.GetInstance().ChalktalkRes = res;
                break;
            case 1:
                // receive stylus id
                int stylusID = BitConverter.ToInt16(data.bytes, 2);
                print("stylus id:" + stylusID);
                if (GetComponent<StylusSyncTrackable>().ID != stylusID)
                    GetComponent<StylusSyncTrackable>().SetSend(false);
                break;
            case 2:
                // receive page id
                int cnt = ParseSketchpageCnt(data.bytes, 2);
                Debug.Log("confirm chalktalk has " + cnt + " boards");
                break;
            case 3:
                // add to remote labels if it is not the local one
                if (localAvatar == null)
                    localAvatar = GameObject.Find("LocalAvatar");
                OculusManager om = localAvatar.GetComponent<OculusManager>();

                // receive the whole avatar id mapping.
                int nPair = BitConverter.ToInt16(data.bytes, 2);
                int index = 4;
                for(int i = 0; i < nPair; i++)
                {
                    int nStr = BitConverter.ToInt16(data.bytes, index);
                    index += 2;
                    string name = Encoding.UTF8.GetString(data.bytes, index, nStr);
                    index += nStr;
                    Debug.Log("receive avatar:" + nStr + "\t" + name);
                    UInt64 remoteID = BitConverter.ToUInt64(data.bytes, index);
                    index += 8;
                    om.AddRemoteAvatarname(name, remoteID);
                }
                break;
            case 4:
                // when first joining, get the active page index
                int boardIndex = Utility.ParsetoInt16(data.bytes, 2);
                Debug.Log("setting page index: " + boardIndex);
                ChalktalkBoard.currentBoardID = boardIndex;
                break;
            default:
                break;
        }
    }
    int ParseSketchpageCnt(byte[] bytes, int offset = 0)
    {

        if (bytes.Length >= offset)
        {
            int cursor = offset;
            int cnt = Utility.ParsetoInt16(bytes, cursor);
            return cnt;
        }
        return 0;
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
