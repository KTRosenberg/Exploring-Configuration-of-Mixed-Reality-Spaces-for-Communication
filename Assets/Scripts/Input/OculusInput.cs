﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OculusInput : MonoBehaviour {
    public Transform controllerTransform;
    public OVRInput.Controller activeController;
    public GameObject selected;
    public Vector3 selectedOffset;
    public Transform curBoard, cursor;
    public StylusSyncTrackable stylusSync;
    //public ResetStylusSync resetSync;
    public MSGSender msgSender;
    bool prevTriggerState = false;//false means up

    bool drawPermissionsToggleInProgress = false;
    public Chalktalk.Renderer ctRenderer;

    // Use this for initialization
    void Start () {
        selected = transform.Find("selected").gameObject;
        selectedOffset = new Vector3(0, 0f, 0.04f);
        cursor = GameObject.Find("cursor").transform;
        //curBoard = GameObject.Find("Board0").transform;
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        //resetSync = GameObject.Find("Display").GetComponent<ResetStylusSync>();
        msgSender = GameObject.Find("Display").GetComponent<MSGSender>();

        ctRenderer = GameObject.Find("ChalktalkHandler").GetComponent<Chalktalk.Renderer>();
    }

    void updateCursor()
    {
        //Debug.Log(ChalktalkBoard.currentBoard);
        // calculate the vive controller transform in board space, and then assign the pos to the cursor by discarding the z

        GameObject board = GameObject.Find("Board" + ChalktalkBoard.currentBoardID); // temp search every time TODO
        if (board == null) {
            return;
        }
        curBoard = board.transform;

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


    Vector3[] BoardToQuad(ChalktalkBoard board)
    {
        Transform tf = board.transform;
        Vector3 pos = tf.position;
        Vector3 dirx = tf.right;
        Vector3 diry = tf.up;
        float bsx = tf.localScale.x * 0.5f;
        float bsy = tf.localScale.y * 0.5f;

        Vector3 vx = dirx * bsx;
        Vector3 vy = diry * bsy;
        return new Vector3[] {
            pos - vx + vy, // TL,
            pos - vx - vy, // BL,
            pos + vx - vy, // BR,
            pos + vx + vy, // TR
        };
    }

    public GameObject destinationMarker = null;
    bool controlInProgress = false;
    private void LateUpdate()
    {
        activeController = OVRInput.GetActiveController();
        int boardCount = ctRenderer.ctBoards.Count;

        if (OVRInput.GetDown(OVRInput.Button.Two, activeController)) {
            //Debug.Log("Increase");
            //if (ChalktalkBoard.currentBoardID + 1 > ChalktalkBoard.MaxExistingID()) {
                Debug.Log("CREATING A NEW BOARD");
                msgSender.Add(2, new int[] { ChalktalkBoard.currentBoardID + 1, 1 });
            //}
            //else {
            //    Debug.Log("CYCLING THROUGH EXISTING BOARDS");
            //    msgSender.Add(4, new int[] { ChalktalkBoard.currentBoardID + 1 });
            //}
        }
        
        if (ChalktalkBoard.selectionWaitingForCompletion) {
            return;
        }

        // automatically select the closest board
        Ray facingRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        
        float minDist = Mathf.Infinity;
        int closestBoardID = -1;
        ChalktalkBoard closestBoard = null;
        for (int i = 0; i < ChalktalkBoard.boardList.Count; i += 1) {
            Plane boardPlane = new Plane(ChalktalkBoard.boardList[i].transform.forward, ChalktalkBoard.boardList[i].transform.position);
            Debug.DrawRay(ChalktalkBoard.boardList[i].transform.position, ChalktalkBoard.boardList[i].transform.forward, Color.green);
            // need to vis the plane
            float enter = 0.0f;
            if (boardPlane.Raycast(facingRay, out enter)) {
                //Get the point that is clicked
                Vector3 hitPoint = facingRay.GetPoint(enter);
                if(enter < minDist) {
                    minDist = enter;
                    closestBoardID = i;
                }
            }
        }
        if (closestBoardID != -1) {
            closestBoard = ChalktalkBoard.boardList[closestBoardID];
            print("current closest board:" + closestBoardID);
        }
            
        ////////////////////////////////////////////////////
        if (ChalktalkBoard.selectionInProgress) {
            if (closestBoard != null) {
                if (destinationMarker != null) {
                    destinationMarker.transform.localScale = Vector3.zero;
                }
                if (closestBoard.boardID != ChalktalkBoard.currentBoardID) {
                    Debug.Log("Select board");
                    msgSender.Add(4, new int[] { closestBoard.boardID });
                }
            }

        }
        ////////////////////////////////////////////////////

        float stickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, activeController).y;
        if (controlInProgress) {
            if (Mathf.Abs(stickY) < 0.25f) {
                controlInProgress = false;
            }
            else {
                //Debug.Log("control in progress");
            }
            return; 
        }

        if (closestBoard == null) {
            if (destinationMarker != null) {
                destinationMarker.transform.localScale = Vector3.zero;
            }
        }
        else {
            if (closestBoard.boardID != ChalktalkBoard.currentBoardID 
                //&& OVRInput.GetDown(OVRInput.Button.One, activeController)
                ) {
                Debug.Log("Select board");
                msgSender.Add(4, new int[] { closestBoard.boardID });
            }
            if (destinationMarker != null) {

                if (closestBoard.boardID != ChalktalkBoard.currentBoardID && ChalktalkBoard.selectionInProgress) {
                    destinationMarker.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    destinationMarker.transform.position = closestBoard.transform.position;
                }
                else {
                    destinationMarker.transform.localScale = Vector3.zero;
                }
            }
            //Debug.DrawRay(r.origin, r.direction, Color.red);
            //Debug.Log("<color=green>boardID: " + closestBoard.boardID + "</color>");
            if (ChalktalkBoard.selectionInProgress) {

                //if (stickY > 0.8f) {
                if (stickY < -0.8f) {
                    Debug.Log("<color=red>" + "(Selection End)" + "</color>");

                    ChalktalkBoard.selectionInProgress = false;
                    controlInProgress = true;

                    //msgSender.Send(4, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID - 1, ChalktalkBoard.MaxExistingID() + 1) });

                    msgSender.Add(7, new int[] {Time.frameCount, closestBoard.boardID });

                    ChalktalkBoard.selectionWaitingForCompletion = true;
                }
                //else if (stickY < -0.8f) {
                //    Debug.Log("<color=black>" + "Down" + "</color>");
                //    controlInProgress = true;
                //}
            }
            else {
                //Debug.Log("sticky:" + stickY);
                //if (stickY > 0.8f) {
                //    Debug.Log("<color=black>" + "Up" + "</color>");
                //    controlInProgress = true;
                //}
                //else if (stickY < -0.8f) {
                if (stickY < -0.8f) {
                    Debug.Log("<color=green>" + "(Selection Begin)" + "</color>");

                    ChalktalkBoard.selectionInProgress = true;
                    controlInProgress = true;

                    Debug.Log("<color=red>SENDING COMMAND 6[" + Time.frameCount + "]</color>");
                    msgSender.Add(6, new int[]{Time.frameCount});
                }
            }
        }

        //float stickYTest = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, activeController).y;
        //if (stickYTest > 0.8f) {
        //    Debug.Log("<color=green>" + "Up" + "</color>");
        //}
        //else if (stickYTest < -0.8f) {
        //    Debug.Log("<color=green>" + "Down" + "</color>");
        //}
    }
    void Update () {
        //Debug.Log("WEEEE:" + ChalktalkBoard.currentBoard);
        activeController = OVRInput.GetActiveController();
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, activeController) > 0.8f) {
            print("drawPermissionsToggleInProgress:" + drawPermissionsToggleInProgress);
            if (!drawPermissionsToggleInProgress) {
                // toggle the stylus
                print("toggle hand trigger");
                stylusSync.ChangeSend();
                if (stylusSync.Host) {

                    //msgSender.Send(1, new int[] { stylusSync.ID });
                    msgSender.Add(1, new int[] { stylusSync.ID });
                    //resetSync.ResetStylus(stylusSync.ID);
                }
                drawPermissionsToggleInProgress = true;
            }
        }
        else {
            drawPermissionsToggleInProgress = false;
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
                //print("data 0 onmousedown");
            }
        }
        else
        {
            if (prevTriggerState)
            {
                stylusSync.Data = 2;
                //print("data 2 onmouseup");
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
