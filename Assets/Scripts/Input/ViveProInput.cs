using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveProInput : MonoBehaviour {

    // receive information from holojam about vive ctrl
    ViveCtrl1 viveCtrl1;

    StylusSyncTrackable stylusSync;
    Transform curBoard;
    Vector3 cursorPos;

    // Use this for initialization
    void Start () {
        viveCtrl1 = GameObject.Find("Display").GetComponent<ViveCtrl1>();
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        cursorPos = GameObject.Find("cursor").transform.position;
        curBoard = GameObject.Find("Board0").transform;
    }

    void updateCursor()
    {
        // calculate the vive controller transform in board space, and then assign the pos to the cursor by discarding the z
    }

    // Update is called once per frame
    void Update () {
		if(viveCtrl1.Grip == 1)
        {
            // toggle the stylus
            stylusSync.ChangeSend();
        }
        if(viveCtrl1.Trigger == 1)
        {
            if (stylusSync.Data != 2)
            {
                stylusSync.Data = 1;
                print("data 1 onmousemove");
            }
            else
            {
                stylusSync.Data = 0;
                print("data 0 onmousedown");
            }
        }
        else
        {
            if (stylusSync.Data != 2)
            {
                stylusSync.Data = 2;
                print("data 2 onmouseup");
            }                
        }

        updateCursor();
    }
}
