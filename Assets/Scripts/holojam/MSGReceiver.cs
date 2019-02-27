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
    GameObject ctRenderer;
    float[] timestamps = new float[8];

    // Override Sync()
    protected override void Sync()
    {
        //print("Tracked:" + Tracked);
        if (Tracked) {
            //receivedMsg = Encoding.Default.GetString(data.bytes);
            // receiving several messages
            decode();
        }
    }

    // #0 for resolution
    // #1 for reset ownership
    // #2 for creating sketchPage
    // #3 for receiving avatar name
    void decode()
    {
        int cursor = 0;
        int cmdCount = BitConverter.ToInt16(data.bytes, cursor);
        cursor += 2;
        print(label + "\tcommand count:" + cmdCount);
        for (int i = 0; i < cmdCount; i++) {
            int cmdNumber = BitConverter.ToInt16(data.bytes, cursor);
            cursor += 2;
            print("command number:" + cmdNumber);
            switch ((CommandFromServer)cmdNumber) {
            case CommandFromServer.RESOLUTION_REQUEST: {
                // resolution
                Vector2Int res = ParseDisplayInfo(data.bytes, cursor);
                cursor += 4;
                GlobalToggleIns.GetInstance().ChalktalkRes = res;
                break;
            }
            case CommandFromServer.STYLUS_RESET:
                // receive stylus id
                int stylusID = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                print("stylus id:" + stylusID);
                if (GetComponent<StylusSyncTrackable>().ID != stylusID)
                    GetComponent<StylusSyncTrackable>().SetSend(false);
                break;
            case CommandFromServer.SKETCHPAGE_CREATE:
                // receive page id
                int id = ParseSketchpageID(data.bytes, cursor);
                cursor += 2;

                int setImmediately = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                if (setImmediately == 1) {
                    Debug.Log("setting board immediately");
                }
                Debug.Log("received id:" + id + "set immediately?:" + setImmediately);
                //ChalktalkBoard.UpdateCurrentLocalBoard(id);
					
                break;
            case CommandFromServer.AVATAR_SYNC:
                // add to remote labels if it is not the local one
                print("add to remote labels");
                if (localAvatar == null)
                    localAvatar = GameObject.Find("LocalAvatar");
                OculusManager om = localAvatar.GetComponent<OculusManager>();

                // receive the whole avatar id mapping.
                int nPair = BitConverter.ToInt16(data.bytes, cursor);
                cursor += 2;
                for (int j = 0; j < nPair; j++) {
                    int nStr = BitConverter.ToInt16(data.bytes, cursor);
                    cursor += 2;
                    string name = Encoding.UTF8.GetString(data.bytes, cursor, nStr);
                    cursor += nStr;
                    Debug.Log("receive avatar:" + nStr + "\t" + name);
                    UInt64 remoteID = BitConverter.ToUInt64(data.bytes, cursor);
                    cursor += 8;
                    om.AddRemoteAvatarname(name, remoteID);
                }
                break;
            case CommandFromServer.SKETCHPAGE_SET: {
                int boardIndex = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                Debug.Log("setting page index: " + boardIndex);
                //ChalktalkBoard.UpdateCurrentLocalBoard(boardIndex);
								ChalktalkBoard.UpdateActiveBoard(boardIndex);
                //ChalktalkBoard.selectionWaitingForCompletion = false;
                //Debug.Log("<color=orange>SKETCHPAGE SET UNBLOCK</color>" + Time.frameCount);
                break;
            }
            //case CommandFromServer.INIT_COMBINE: {
            //    Debug.Log("initialization data arrived");
            //    // resolution
            //    Vector2Int res = ParseDisplayInfo(data.bytes, cursor);
            //    cursor += 4;
            //    GlobalToggleIns.GetInstance().ChalktalkRes = res;
            //    Debug.Log("setting resolution:[" + res.x + ", " + res.y + "]");

            //    // when first joining, get the active page index
            //    int boardIndex = Utility.ParsetoInt16(data.bytes, cursor);
            //    cursor += 2;
            //    Debug.Log("setting page index: " + boardIndex);
            //    ChalktalkBoard.UpdateCurrentLocalBoard(boardIndex);

            //    if (ctRenderer == null) {
            //        ctRenderer = GameObject.Find("ChalktalkHandler");
            //    }
            //    if (ctRenderer == null) {
            //        Debug.LogError("The renderer is missing");
            //    }
            //    ctRenderer.GetComponent<Chalktalk.Renderer>().enabled = true;
            //    break;
            //}
            case CommandFromServer.TMP_BOARD_ON: {
                float timestamp = Utility.ParsetoRealFloat(data.bytes, cursor);
                cursor += 4;
                //Debug.Log("<color=magenta>" + timestamp + "</color>");
                if (timestamp <= timestamps[6]) {
                    //Debug.Log("<color=blue>Old timestamp arrived for cmd 6</color>");
                    cursor += 2;
                    break;
                }
                else {
                    timestamps[6] = timestamp;
                }

                int status = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                Debug.Log("<color=green>turn on temporary board mode, value=[" + status + "]</color>");
                if (status == 0) {
                    ChalktalkBoard.selectionInProgress = false;
                    Debug.Log("<color=orange>something was not selected</color>");
                }
                else {
                    Debug.Log("<color=green>something was selected</color>");
                }

                ChalktalkBoard.selectionWaitingForCompletion = false;
                Debug.Log("<color=orange>MOVE ON UNBLOCK</color>" + Time.frameCount);

                break;
            }
            case CommandFromServer.TMP_BOARD_OFF: {
                Debug.Log("<color=green>turn off temporary board mode</color>");

                float timestamp = Utility.ParsetoRealFloat(data.bytes, cursor);
                cursor += 4;
                if (timestamp <= timestamps[7]) {
                    //Debug.Log("<color=blue>Old timestamp arrived for cmd 7</color>");
                    cursor += 2;
                    break;
                }
                else {
                    timestamps[7] = timestamp;
                }

                Debug.Log("<color=magenta>" + timestamp + "</color>");

                int status = Utility.ParsetoInt16(data.bytes, cursor);
                cursor += 2;
                switch (status) {
                case 0:
                    Debug.Log("NOTHING MOVED");
                    break;
                case 1:
                    Debug.Log("SKETCH MOVED");
                    break;
                case 2:
                    Debug.Log("GROUP MOVED");
                    break;
                }

                ChalktalkBoard.selectionInProgress = false;
                ChalktalkBoard.selectionWaitingForCompletion = false;
                Debug.Log("<color=orange>MOVE OFF UNBLOCK</color>" + Time.frameCount);
                break;
            }
            default:
                break;
            }
        }

    }
    int ParseSketchpageID(byte[] bytes, int offset = 0)
    {

        if (bytes.Length >= offset) {
            int cursor = offset;
            int cnt = Utility.ParsetoInt16(bytes, cursor);
            return cnt;
        }
        return 0;
    }
    Vector2Int ParseDisplayInfo(byte[] bytes, int offset = 0)
    {
        if (bytes.Length >= offset) {
            int cursor = offset;
            int resW = Utility.ParsetoInt16(bytes, cursor);
            cursor += 2;
            int resH = Utility.ParsetoInt16(bytes, cursor);
            return new Vector2Int(resW, resH);
        }
        return new Vector2Int(0, 0);
    }

    int ParseSketchpageCnt(byte[] bytes, int offset = 0)
    {
        if (bytes.Length > 8) {
            int cursor = 8 + offset;
            int cnt = Utility.ParsetoInt16(bytes, cursor);
            return cnt;
        }
        return 0;
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
