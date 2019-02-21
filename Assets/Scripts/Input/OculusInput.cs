using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OculusInput : MonoBehaviour
{
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
    void Start()
    {
        selected = transform.Find("selected").gameObject;
        selectedOffset = new Vector3(0, 0f, 0.04f);
        cursor = GameObject.Find("cursor").transform;
        //curBoard = GameObject.Find("Board0").transform;
        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        //resetSync = GameObject.Find("Display").GetComponent<ResetStylusSync>();
        msgSender = GameObject.Find("Display").GetComponent<MSGSender>();

        ctRenderer = GameObject.Find("ChalktalkHandler").GetComponent<Chalktalk.Renderer>();

        if (activeController == OVRInput.Controller.None) {
            activeController = OVRInput.Controller.RTouch;
        }
    }

    void UpdateCursor(int trySwitchBoard = -1)
    {
        //Debug.Log(ChalktalkBoard.currentBoard);
        // calculate the vive controller transform in board space, and then assign the pos to the cursor by discarding the z

        GameObject board = trySwitchBoard == -1 ? GameObject.Find("Board" + ChalktalkBoard.currentBoardID) : GameObject.Find("Board" + trySwitchBoard); // temp search every time TODO: need a map from boardID to gameObject
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

    public int FindIDClosestBoard(Ray facingRay)
    {
        float minDist = Mathf.Infinity;
        int closestBoardID = -1;
        Vector3 hitPoint = Vector3.zero;
        for (int i = 0; i < ChalktalkBoard.boardList.Count; i += 1) {
            Plane boardPlane = new Plane(ChalktalkBoard.boardList[i].transform.forward, ChalktalkBoard.boardList[i].transform.position);
            // need to vis the plane
            float enter = 0.0f;
            if (boardPlane.Raycast(facingRay, out enter)) {

                if (enter < minDist) {
                    minDist = enter;
                    closestBoardID = i;
                    //Get the point that is clicked
                    hitPoint = facingRay.GetPoint(enter);
                }
            }
        }

        return closestBoardID;
    }

    public int FindIDClosestBoard(Ray facingRay, ref Plane closestBoardPlane, ref Vector3 closestHitPoint)
    {
        float minDist = Mathf.Infinity;
        int closestBoardID = -1;

        for (int i = 0; i < ChalktalkBoard.boardList.Count; i += 1) {
            Plane boardPlane = new Plane(ChalktalkBoard.boardList[i].transform.forward, ChalktalkBoard.boardList[i].transform.position);
            // need to vis the plane
            float enter = 0.0f;
            if (boardPlane.Raycast(facingRay, out enter)) {

                if (enter < minDist) {
                    minDist = enter;
                    closestBoardID = i;

                    closestBoardPlane = boardPlane;
                    //Get the point that is clicked
                    closestHitPoint = facingRay.GetPoint(enter);
                }
            }
        }

        return closestBoardID;
    }

    public GameObject destinationMarker = null;
    bool controlInProgress = false;


    public bool TrySwitchBoard(int boardID, ref Plane boardPlane, ref Ray facingRay)
    {
        ChalktalkBoard closestBoard = ChalktalkBoard.boardList[boardID];

        // test 1: angle should be near 90-degrees (TODO figure whether this calculation works for long-distances)
        {
            float dot = Vector3.Dot(-facingRay.direction, -closestBoard.transform.forward);
            float angle = Vector3.Angle(-facingRay.direction, -closestBoard.transform.forward);
            if (angle > Utility.SwitchFaceThres) {
                return false;
            }
        }

        // test 2: controller should  face the board
        {
            Ray controllerRay = new Ray(
                OVRInput.GetLocalControllerPosition(activeController),
                OVRInput.GetLocalControllerRotation(activeController) * Vector3.forward
            );
            float enter = 0.0f;
            if (boardPlane.Raycast(controllerRay, out enter)) {
                // angle test
                float dot = Vector3.Dot(-controllerRay.direction, -closestBoard.transform.forward);
                float angle = Vector3.Angle(-controllerRay.direction, -closestBoard.transform.forward);
                if (angle > Utility.SwitchCtrlThres) {
                    return false;
                }
            }
        }

        // all tests passed
        msgSender.Add((int)CommandToServer.SKETCHPAGE_SET, new int[] { boardID });
        print("Select board: current closest board:" + boardID);
        //ChalktalkBoard.selectionWaitingForCompletion = true;
        //Debug.Log("<color=red>SET PAGE BLOCK</color>" + Time.frameCount);

        return true;
    }

    public void HandleObjectSelection(int ctBoardID, float stickY, ref bool controlInProgress)
    {
        if (ChalktalkBoard.selectionInProgress) {
            if (stickY < -0.8f) {
                //Debug.Log("<color=red>" + "(Selection End)" + "</color>");

                ChalktalkBoard.selectionInProgress = false;
                controlInProgress = true;
                ChalktalkBoard.selectionWaitingForCompletion = true;

                msgSender.Add((int)CommandToServer.TMP_BOARD_OFF, new int[] { Time.frameCount, ctBoardID });
                Debug.Log("<color=red>MOVE OFF BLOCK</color>" + Time.frameCount);
            }
        }
        else if (stickY < -0.8f) {
            //Debug.Log("<color=green>" + "(Selection Begin)" + "</color>");

            ChalktalkBoard.selectionInProgress = true;
            controlInProgress = true;
            ChalktalkBoard.selectionWaitingForCompletion = true;

            //Debug.Log("<color=red>SENDING COMMAND 6[" + Time.frameCount + "]</color>");
            msgSender.Add((int)CommandToServer.TMP_BOARD_ON, new int[] { Time.frameCount });
            Debug.Log("<color=red>MOVE ON BLOCK</color>" + Time.frameCount);
        }
    }

    private int UpdateBoardAndSelectObjects()
    {
        int boardCount = ctRenderer.ctBoards.Count;

        // handle creating-new-board operation
        if (OVRInput.GetDown(OVRInput.Button.Two, activeController)) {
            Debug.Log("creating a new board");
            msgSender.Add((int)CommandToServer.SKETCHPAGE_CREATE, new int[] { ChalktalkBoard.curMaxBoardID, 0 });
        }
        if (ChalktalkBoard.selectionWaitingForCompletion) {
            Debug.Log("WAITING FOR COMPLETION");
            return -1;
        }

        Plane closestBoardPlane = new Plane();
        Vector3 closestHitPoint = Vector3.zero;
        // find ID of the closest board based on face direction
        Ray facingRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);        
        int closestFceBoardID = FindIDClosestBoard(facingRay, ref closestBoardPlane, ref closestHitPoint);
        // find ID of the closest board based on control direction
        Ray controllerRay = new Ray(OVRInput.GetLocalControllerPosition(activeController),
                OVRInput.GetLocalControllerRotation(activeController) * Vector3.forward);
        int closestCtrlBoardID = FindIDClosestBoard(controllerRay, ref closestBoardPlane, ref closestHitPoint);
        // (do not check if currently drawing)
        int closestBoardID = ((OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController) <= 0.8f) && 
                              (closestFceBoardID == closestCtrlBoardID)) ? closestCtrlBoardID : -1;

        // then test if should switch board based on facing angle and controller position/orientation
        if (closestBoardID != -1 && closestBoardID != ChalktalkBoard.currentBoardID) {
            TrySwitchBoard(closestBoardID, ref closestBoardPlane, ref facingRay);
        }

        float stickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, activeController).y;
        if (controlInProgress) {
            Debug.Log("CONTROL IN PROGRESS");
            if (Mathf.Abs(stickY) < 0.25f) {
                controlInProgress = false;
            }
            return closestBoardID;
        }

        if (closestBoardID == -1) {
            HandleObjectSelection(ChalktalkBoard.currentBoardID, stickY, ref controlInProgress);
        }
        else {
            HandleObjectSelection(closestBoardID, stickY, ref controlInProgress);
        }

        return closestBoardID;
    }

    OVRInput.Controller prevHandTriggerDown = OVRInput.Controller.None;
    void Update()
    {
        bool handTriggerDown = false;
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.8f) {
            handTriggerDown = true;
            activeController = OVRInput.Controller.RTouch;
        }
        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.8f) {
            handTriggerDown = true;
            activeController = OVRInput.Controller.LTouch;
        }
        
        if (handTriggerDown) {
            print("drawPermissionsToggleInProgress:" + drawPermissionsToggleInProgress);
            if (!drawPermissionsToggleInProgress) {
                // toggle the stylus only if using the same hand
                if((prevHandTriggerDown == activeController) || (!stylusSync.Host)) {
                    print("toggle hand trigger");
                    stylusSync.ChangeSend();
                    if (stylusSync.Host) {
                        msgSender.Add((int)CommandToServer.STYLUS_RESET, new int[] { stylusSync.ID });
                    }
                }
                drawPermissionsToggleInProgress = true;
            }
            prevHandTriggerDown = activeController;
        }
        else {
            drawPermissionsToggleInProgress = false;
        }

        // enable the selected sphere
        if (stylusSync == null)
            stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        selected.GetComponent<MeshRenderer>().enabled = stylusSync.Host;
        stylusSync.Data = 1;    // moving by default

        // avoid quick switch between select and deselect
        bool isIndexTriggerDown = (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController) > 0.8f);
        if (isIndexTriggerDown) {
            if (!prevTriggerState) {
                stylusSync.Data = 0;
                //print("data 0 onmousedown");
            }
        }
        else {
            if (prevTriggerState) {
                stylusSync.Data = 2;
                //print("data 2 onmouseup");
            }
        }

        prevTriggerState = isIndexTriggerDown;

        // update the pos of selected spheres
        updateSelected();

        // update the closest board and sketch selection
        int trySwitchClosest = UpdateBoardAndSelectObjects();

        // update the pos of cursor based on current board
        UpdateCursor(trySwitchClosest);

        // manipulation of the current board by two controllers
        ManipulateBoard();
    }

    bool prevDualIndex = false;
    Vector3[] prevDualPoses = new Vector3[2];
    Vector3[] curDualPoses = new Vector3[2];
    Quaternion[] prevDualRots = new Quaternion[2];
    Quaternion[] curDualRots = new Quaternion[2];
    public void ManipulateBoard()
    {
        if(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.8 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.8) {
            // two index fingers are holding
            if (!prevDualIndex) {
                // the first frame for this control session
                prevDualPoses[0] = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                prevDualPoses[1] = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                prevDualRots[0] = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
                prevDualRots[1] = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            }
            else {
                // apply manipulation based on previous and current positions
                curDualPoses[0] = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                curDualPoses[1] = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                curDualRots[0] = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
                curDualRots[1] = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
                applyPosesToBoard(prevDualPoses, curDualPoses, prevDualRots, curDualRots);
                prevDualPoses[0] = curDualPoses[0];
                prevDualPoses[1] = curDualPoses[1];
                prevDualRots[0] = curDualRots[0];
                prevDualRots[1] = curDualRots[1];
            }
            prevDualIndex = true;
        }
        else {
            prevDualIndex = false;
        }
    }

    void applyPosesToBoard(Vector3[] prevPos, Vector3[] curPos, Quaternion[] prevRot, Quaternion[] curRot)
    {
        // apply the translation if two hands are moving almost parallely
        Vector3 leftHandMove = curPos[0] - prevPos[0];
        Vector3 rightHandMove = curPos[1] - prevPos[1];
        
        if (leftHandMove.magnitude < 0.002f)
            return;
        if (rightHandMove.magnitude < 0.002f)
            return;
        float angle = Vector3.Angle(leftHandMove, rightHandMove);
        if(angle < Utility.SwitchFaceThres) {
            // treat it as translation
            Vector3 averMove = (leftHandMove + rightHandMove) / 2;
            ChalktalkBoard.GetCurBoard().transform.position += averMove;
            //print("moving averagly " + angle.ToString("F3"));
        }
        else if(angle > 180 - Utility.SwitchFaceThres){
            // treat movement as rotation
            Vector3 prevHandLine = prevPos[1] - prevPos[0];
            Vector3 curHandLine = curPos[1] - curPos[0];
            Quaternion q = Quaternion.identity;
            q.SetFromToRotation(prevHandLine, curHandLine);
            ChalktalkBoard.GetCurBoard().transform.rotation = q * ChalktalkBoard.GetCurBoard().transform.rotation;
            print("rotating based on movements " + angle.ToString("F3"));
        }
        else {
            // apply average rotation
            //print("leftHandMove:" + leftHandMove.ToString("F3") + "\trightHandMove:" + rightHandMove.ToString("F3"));
            //Quaternion leftHandRot = Quaternion.RotateTowards(prevRot[0], curRot[0], 180f);
            //leftHandRot.Normalize();
            //Quaternion rightHandRot = Quaternion.RotateTowards(prevRot[1], curRot[1], 180f);
            //rightHandRot.Normalize();
            //float qangle = Quaternion.Angle(leftHandRot, rightHandRot);
            //if(qangle < Utility.SwitchFaceThres) {
            //    ChalktalkBoard.GetCurBoard().transform.rotation = 
            //        Quaternion.Lerp(leftHandRot, rightHandRot, 0.5f) * ChalktalkBoard.GetCurBoard().transform.rotation;
            //    print("rotating averagly " + qangle.ToString("F3") + "\tleftHandRot:" + leftHandRot.w.ToString("F3") + "\trightHandRot:" + rightHandRot.w.ToString("F3"));
            //}
        }
    }
}
