using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveInput : MonoBehaviour
{

    // labels
    StylusSyncTrackable stylusSync;

    Vector3 cursorPos, screenPoint, offset;
    Transform curBoard;

    // Use this for initialization
    void Start()
    {
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        cursorPos = GameObject.Find("cursor").transform.position;
        curBoard = GameObject.Find("Board0").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))// for vive
        {
            // toggle the stylus
            stylusSync.ChangeSend();
        }
        if (Input.GetMouseButton(14) || Input.GetMouseButton(15))
        {
            if (stylusSync.Data != 2)
            {
                stylusSync.Data = 1;
                print("data 1");

            }
        }
        if (Input.GetMouseButtonDown(14) || Input.GetMouseButton(15))
        {
            if (stylusSync.Data != 0)
            {
                stylusSync.Data = 0;
                print("data 0");
            }
        }

        if (Input.GetMouseButtonUp(14) || Input.GetMouseButton(15))
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
