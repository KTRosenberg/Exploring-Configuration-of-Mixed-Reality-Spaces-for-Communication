using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ViveInput : MonoBehaviour
{

    // labels
    StylusSyncTrackable stylusSync;

    Vector3 cursorPos, screenPoint, offset;
    Transform curBoard;

    public string[] names;

    SteamVR_Behaviour_Pose viveControllerLeft,viveControllerRight;

    // Use this for initialization
    void Start()
    {
        print("GetJoystickNames:" + Input.GetJoystickNames());
        names = Input.GetJoystickNames();
        foreach (string s in names)
            print(s);

        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        cursorPos = GameObject.Find("cursor").transform.position;
        curBoard = GameObject.Find("Board0").transform;

        viveControllerLeft = GameObject.Find("Controller (left)").GetComponent< SteamVR_Behaviour_Pose>();
        viveControllerRight = GameObject.Find("Controller (right)").GetComponent< SteamVR_Behaviour_Pose>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(SteamVR_Input._default.inActions.Teleport.GetStateDown(viveControllerLeft.))
        if (Input.GetButtonDown("Left Controller Menu Button"))// for vive
        {
            // toggle the stylus
            stylusSync.ChangeSend();
        }
        //if (Input.GetMouseButton(14) || Input.GetMouseButton(15))
        //Input.GetButtonDown("OpenVR Controller(VIVE Controller Pro MV) - Right")
        if(Input.GetButtonDown("Left Controller Trigger"))
        {
            if (stylusSync.Data != 2)
            {
                stylusSync.Data = 1;
                print("data 1");

            }
        }
        if (Input.GetButtonDown("Left Controller Trigger") )
        {
            if (stylusSync.Data != 0)
            {
                stylusSync.Data = 0;
                print("data 0");
            }
        }

        if (Input.GetButtonDown("Left Controller Trigger") )
        {
            stylusSync.Data = 2;
            print("data 2");
        }

        if (curBoard == null)
            curBoard = GameObject.Find("Board0").transform;
        Vector3 p = curBoard.InverseTransformPoint(transform.position);
        print("pos in board:" + p);

        p.y = -p.y + 0.5f;
        p.x = p.x + 0.5f;
        print("pos after convert:" + p);
        stylusSync.Pos = p;
        stylusSync.Rot = transform.eulerAngles;
    }
}
