using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OculusInput : MonoBehaviour {

    public OVRInput.Controller activeController;
    public GameObject selected;
    public Vector3 selectedOffset;
    public Transform curBoard, cursor;
    public StylusSyncTrackable stylusSync;
    //public ResetStylusSync resetSync;
    public MSGSender msgSender;
    bool prevTriggerState = false;//false means up


    public Chalktalk.Renderer renderer;


    // Use this for initialization
    void Start () {
        selected = transform.Find("selected").gameObject;
        selectedOffset = new Vector3(0, 0f, 0.04f);
        cursor = GameObject.Find("cursor").transform;
        //curBoard = GameObject.Find("Board0").transform;
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        //resetSync = GameObject.Find("Display").GetComponent<ResetStylusSync>();
        msgSender = GameObject.Find("Display").GetComponent<MSGSender>();

        renderer = GameObject.Find("ChalktalkHandler").GetComponent<Chalktalk.Renderer>();
    }

    void updateCursor()
    {
        //Debug.Log(ChalktalkBoard.currentBoard);
        // calculate the vive controller transform in board space, and then assign the pos to the cursor by discarding the z

        curBoard = GameObject.Find("Board" + ChalktalkBoard.currentBoardID).transform; // temp search every time TODO
        if (curBoard == null) {
            return;
        }

        Vector3 p = curBoard.InverseTransformPoint(OVRInput.GetLocalControllerPosition(activeController));
        Vector3 cursorPos = new Vector3(p.x, p.y, 0);

        Vector3 projected = Vector3.ProjectOnPlane(p, curBoard.transform.forward); // but this makes the cursor position wrong if I use it instead of cursorPos... probably because line 43 and 44 also need to be changed?
        //cursorPos = projected;

        cursor.position = curBoard.TransformPoint(cursorPos);
        //print("pos in board:" + p);

        p.y = -p.y + 0.5f;
        p.x = p.x + 0.5f;
        //print("pos after convert:" + p);
        stylusSync.Pos = p;
        stylusSync.Rot = transform.eulerAngles;


    }

    void updateSelected()
    {
        selected.transform.position = OVRInput.GetLocalControllerPosition(activeController) + OVRInput.GetLocalControllerRotation(activeController) * selectedOffset;
        selected.transform.rotation = OVRInput.GetLocalControllerRotation(activeController);
    }

    public float testF;
    // Update is called once per frame


    private void LateUpdate()
    {
        activeController = OVRInput.GetActiveController();
        int boardCount = renderer.ctBoards.Count;

        if (OVRInput.GetDown(OVRInput.Button.Two, activeController)) {
            Debug.Log("Increase");
            msgSender.Send(4, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID + 1, 4) });
        }
        if (OVRInput.GetDown(OVRInput.Button.One, activeController)) {
            Debug.Log("Decrease");
            msgSender.Send(4, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID - 1, 4) });
        }
    }
    void Update () {
        //Debug.Log("WEEEE:" + ChalktalkBoard.currentBoard);
        activeController = OVRInput.GetActiveController();
        testF = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, activeController) > 0.8f)
        {
            // toggle the stylus
            print("toggle hand trigger");
            stylusSync.ChangeSend();
            if (stylusSync.Host)
            {
                msgSender.Send(1,new int[] { stylusSync.ID });
                //resetSync.ResetStylus(stylusSync.ID);
            }

        }
        // enable the selected sphere
        if(stylusSync == null)
            stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        selected.GetComponent<MeshRenderer>().enabled = stylusSync.Host;
        stylusSync.Data = 1;    // moving by default

        bool isIndexTriggerDown = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController) > 0.8f;
        if (isIndexTriggerDown)
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


        //if(stylusSync.Data == 1)
        //print("data 2 onmousemove");
        // if prev trigger state is down, we at most neglect 10 onmousemove
        //    if(prevTriggerState && moveCounter < 3)
        //{
        //    ++moveCounter;
        //    stylusSync.Data = 0;
        //}
        prevTriggerState = isIndexTriggerDown;
        updateSelected();
        updateCursor();
    }
}
