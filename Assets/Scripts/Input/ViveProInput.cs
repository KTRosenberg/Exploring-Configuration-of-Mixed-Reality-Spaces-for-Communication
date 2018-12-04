using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveProInput : MonoBehaviour {

    // receive information from holojam about vive ctrl
    ViveCtrl1 viveCtrl1;

    StylusSyncTrackable stylusSync;
    ResetStylusSync resetSync;
    Transform curBoard,cursor;
    Vector3 cursorPos;

    GameObject selected;
    Vector3 selectedOffset;

    bool prevTriggerState = false;//false means up

    // Use this for initialization
    void Start () {
        viveCtrl1 = GameObject.Find("Display").GetComponent<ViveCtrl1>();
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        resetSync = GameObject.Find("Display").GetComponent<ResetStylusSync>();
        cursor = GameObject.Find("cursor").transform;
        curBoard = GameObject.Find("Board0").transform;
        selected = transform.Find("selected").gameObject;
        selectedOffset = new Vector3(0, 0f, 0.04f);
    }

    void updateCursor()
    {
        // calculate the vive controller transform in board space, and then assign the pos to the cursor by discarding the z
        if (curBoard == null)
            curBoard = GameObject.Find("Board0").transform;
        Vector3 p = curBoard.InverseTransformPoint(viveCtrl1.Pos);
        Vector3 cursorPos = new Vector3(p.x, p.y, 0);
        cursor.position = curBoard.TransformPoint( cursorPos);
        //print("pos in board:" + p);

        p.y = -p.y + 0.5f;
        p.x = p.x + 0.5f;
        //print("pos after convert:" + p);
        stylusSync.Pos = p;
        stylusSync.Rot = transform.eulerAngles;

        
    }

    void updateSelected(){
        selected.transform.position = viveCtrl1.Pos + viveCtrl1.Rot * selectedOffset;
        selected.transform.rotation = viveCtrl1.Rot;
    }

    // Update is called once per frame
    void Update () {
		if(viveCtrl1.Grip == 1)
        {
            // toggle the stylus
            stylusSync.ChangeSend();
            if (stylusSync.Host)
                resetSync.ResetStylus();
            else
                resetSync.ClearOwn();
        }
        // enable the selected sphere
        selected.GetComponent<MeshRenderer>().enabled = stylusSync.Host;
        stylusSync.Data = 1;    // moving by default
        if (viveCtrl1.Trigger == 1)
        {
            if (!prevTriggerState)
            {
                stylusSync.Data = 0;
                print("data 0 onmousedown");
            }
        }
        else
        {
            if (prevTriggerState)
            {
                stylusSync.Data = 2;
                print("data 2 onmouseup");
            }                
        }
        if(stylusSync.Data == 1)
            print("data 2 onmousemove");
        prevTriggerState = viveCtrl1.Trigger == 1;
        updateSelected();
        updateCursor();
    }
}
