using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OculusInput : MonoBehaviour
{
    public Transform controllerTransform;
    public OVRInput.Controller activeController;
    public GameObject selected;
    public Vector3 selectedOffset;
    public Transform curBoard, cursor, secondaryCursor;
    Renderer secondaryCursorRenderer;
    public StylusSyncTrackable stylusSync;
    bool prevTriggerState = false;//false means up

    bool drawPermissionsToggleInProgress = false;
    public Chalktalk.Renderer ctRenderer;

    PerspectiveView perspView;
    bool prevOneState = false;

    // Use this for initialization
    void Start()
    {
        selected = transform.Find("selected").gameObject;
        selectedOffset = new Vector3(0, 0f, 0.04f);
        cursor = GameObject.Find("cursor").transform;

        GameObject secondaryCursorGameObject = GameObject.Find("secondaryCursor");
        this.secondaryCursor = secondaryCursorGameObject.transform;
        this.secondaryCursorRenderer = secondaryCursorGameObject.GetComponentInChildren<Renderer>();


        stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        ctRenderer = GameObject.Find("ChalktalkHandler").GetComponent<Chalktalk.Renderer>();

        if (activeController == OVRInput.Controller.None) {
            activeController = OVRInput.Controller.RTouch;
        }
        perspView = GameObject.Find("LocalAvatar").GetComponent<PerspectiveView>();

    }

    void UpdateCursor(int trySwitchBoard = -1)
    {
        //Debug.Log(ChalktalkBoard.currentBoard);
        // calculate the vive controller transform in board space, and then assign the pos to the cursor by discarding the z

        GameObject board = trySwitchBoard == -1 ? GameObject.Find("Board" + ChalktalkBoard.currentLocalBoardID) : GameObject.Find("Board" + trySwitchBoard); // temp search every time TODO: need a map from boardID to gameObject
        if (board == null) {
            return;
        }
        curBoard = board.transform;

        Vector3 p = curBoard.InverseTransformPoint(OVRInput.GetLocalControllerPosition(activeController));
        Vector3 cursorPos = new Vector3(p.x, p.y, 0);

        cursor.position = curBoard.TransformPoint(cursorPos);
        if (stylusSync.zOffset != 0.0f) {
            if (prevZOffset == 0.0f) {
                secondaryCursorRenderer.enabled = true;
            }
            secondaryCursor.position = curBoard.TransformPoint(new Vector3(p.x, p.y, stylusSync.zOffset));
        }
        else if (prevZOffset != 0.0f) {
            secondaryCursorRenderer.enabled = false;
        }
        prevZOffset = stylusSync.zOffset;

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

    public int FindIDClosestBoard(Ray facingRay, ref Plane closestBoardPlane, ref Vector3 closestHitPoint, ref ChalktalkBoard theBoard)
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
                    closestBoardID = ChalktalkBoard.boardList[i].boardID;
                    theBoard = ChalktalkBoard.boardList[i];

                    closestBoardPlane = boardPlane;
                    //Get the point that is clicked
                    closestHitPoint = facingRay.GetPoint(enter);
                }
            }
        }

        return closestBoardID;
    }

    public bool controlInProgress = false;
    public bool depthPositionControlInProgress = false;


    public bool TrySwitchBoard(int boardID, ref Plane boardPlane, ref Ray facingRay, ref ChalktalkBoard theBoard)
    {
        // test 1: angle should be near 90-degrees (TODO figure whether this calculation works for long-distances)
        {
            float dot = Vector3.Dot(-facingRay.direction, -theBoard.transform.forward);
            float angle = Vector3.Angle(-facingRay.direction, -theBoard.transform.forward);
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
                float dot = Vector3.Dot(-controllerRay.direction, -theBoard.transform.forward);
                float angle = Vector3.Angle(-controllerRay.direction, -theBoard.transform.forward);
                if (angle > Utility.SwitchCtrlThres) {
                    return false;
                }
            }
        }

        // all tests passed
        // only send when we have the control
        if (stylusSync.Host && boardID != ChalktalkBoard.activeBoardID)
        {
            print("switch active board " + boardID);
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SKETCHPAGE_SET, new int[] { boardID });
        }
            
        if(boardID != ChalktalkBoard.currentLocalBoardID)
        {
            ChalktalkBoard.UpdateCurrentLocalBoard(boardID);
            print("Select board: current closest board:" + boardID);
        }
            
        //ChalktalkBoard.selectionWaitingForCompletion = true;
        //Debug.Log("<color=red>SET PAGE BLOCK</color>" + Time.frameCount);

        return true;
    }

    float prevZOffset = 0.0f;
    const bool V1_TWO_HANDED_CONTROLS = false;
    public void HandleObjectSelection(int ctBoardID, float stickY, ref bool controlInProgress)
    {
        if (V1_TWO_HANDED_CONTROLS) { // secondary controller for in/out movement in z, primary controller thumbstick for selection
            if (ChalktalkBoard.selectionIsOn) {
                if (stickY < -0.8f) {
                    //Debug.Log("<color=red>" + "(Selection End)" + "</color>");

                    ChalktalkBoard.selectionIsOn = false;
                    controlInProgress = true;
                    ChalktalkBoard.selectionWaitingForPermissionToAct = true;

                    MSGSenderIns.GetIns().sender.Add(CommandToServer.DESELECT_CTOBJECT, new int[] { Time.frameCount, ctBoardID });
                    Debug.Log("<color=red>MOVE OFF BLOCK</color>" + Time.frameCount);
                }
                else {
                    OVRInput.Controller secondaryController = OVRInput.Controller.LTouch;
                    switch (activeController) {
                    case OVRInput.Controller.LTouch:
                        secondaryController = OVRInput.Controller.RTouch;
                        break;
                    case OVRInput.Controller.RTouch:
                        secondaryController = OVRInput.Controller.LTouch;
                        break;
                    }

                    float secondaryStickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, secondaryController).y;
                    if (!depthPositionControlInProgress) {
                        if (secondaryStickY < -0.8f) {
                            depthPositionControlInProgress = true;

                            // send command to move towards
                            Debug.Log("<color=blue>MOVE FWBW 1 </color>");
                            MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, 1 });
                        }
                        else if (secondaryStickY > 0.8f) {
                            depthPositionControlInProgress = true;
                            // send command to move away
                            Debug.Log("<color=blue>MOVE FWBW -1</color>");
                            MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, -1 });
                        }

                    }
                    else if (Mathf.Abs(secondaryStickY) < 0.25f) {
                        depthPositionControlInProgress = false;
                        // send off command
                        Debug.Log("<color=blue>MOVE FWBW 0</color>");
                        MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, 0 });
                    }
                }
            }
            else if (stickY < -0.8f) {
                //Debug.Log("<color=green>" + "(Selection Begin)" + "</color>");
                ChalktalkBoard.selectionIsOn = true;
                controlInProgress = true;
                ChalktalkBoard.selectionWaitingForPermissionToAct = true;

                //Debug.Log("<color=red>SENDING COMMAND 6[" + Time.frameCount + "]</color>");
                MSGSenderIns.GetIns().sender.Add(CommandToServer.SELECT_CTOBJECT, new int[] { Time.frameCount });
                Debug.Log("<color=red>MOVE ON BLOCK</color>" + Time.frameCount);
            }
        }
        else { // all on primary controller: button one for selection/placement, thumbstick for in/out
            if (ChalktalkBoard.selectionIsOn) {
                if (OVRInput.GetDown(OVRInput.Button.One, activeController)) {

                    ChalktalkBoard.selectionIsOn = false;
                    //controlInProgress = true;
                    ChalktalkBoard.selectionWaitingForPermissionToAct = true;

                    MSGSenderIns.GetIns().sender.Add(CommandToServer.DESELECT_CTOBJECT, new int[] { Time.frameCount, ctBoardID });
                    Debug.Log("<color=red>MOVE OFF BLOCK</color>" + Time.frameCount);
                }
                else {
                    float zPosStickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, activeController).y;
                    if (!depthPositionControlInProgress) {
                        if (zPosStickY < -0.8f) {
                            depthPositionControlInProgress = true;

                            // send command to move towards

                            Debug.Log("<color=blue>MOVE FWBW 1 </color>");
                            MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, 1 });
                        }
                        else if (zPosStickY > 0.8f) {
                            depthPositionControlInProgress = true;

                            // send command to move away
                            Debug.Log("<color=blue>MOVE FWBW -1</color>");
                            MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, -1 });
                        }
                    }
                    else if (Mathf.Abs(zPosStickY) < 0.25f) {
                        depthPositionControlInProgress = false;

                        // send off command
                        Debug.Log("<color=blue>MOVE FWBW 0</color>");
                        MSGSenderIns.GetIns().sender.Add(CommandToServer.MOVE_FW_BW_CTOBJECT, new int[] { Time.frameCount, 0 });
                    }
                }
            }
            else if (OVRInput.GetDown(OVRInput.Button.One, activeController)) { // select
         
                ChalktalkBoard.selectionIsOn = true; // selection on
                //controlInProgress = true;
                ChalktalkBoard.selectionWaitingForPermissionToAct = true; // wait until server says it's okay

                MSGSenderIns.GetIns().sender.Add(CommandToServer.SELECT_CTOBJECT, new int[] { Time.frameCount });
                Debug.Log("<color=red>MOVE ON BLOCK</color>" + Time.frameCount);
            }
        }
    }


    private int UpdateBoardAndSelectObjects()
    {
        int boardCount = ctRenderer.ctBoards.Count;

        // handle creating-new-board operation
        if (OVRInput.GetDown(OVRInput.Button.Two, activeController) && stylusSync.Host) {
            Debug.Log("creating a new board");
            MSGSenderIns.GetIns().sender.Add(CommandToServer.SKETCHPAGE_CREATE, new int[] { ChalktalkBoard.curMaxBoardID, 0 });
        }
        if (ChalktalkBoard.selectionWaitingForPermissionToAct) {
            Debug.Log("WAITING FOR COMPLETION");
            return -1;
        }

        Plane closestBoardPlane = new Plane();
        Vector3 closestHitPoint = Vector3.zero;
        ChalktalkBoard closestBoard = null;
        // find ID of the closest board based on face direction
        Ray facingRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        int closestFceBoardID = FindIDClosestBoard(facingRay, ref closestBoardPlane, ref closestHitPoint, ref closestBoard);
        // find ID of the closest board based on control direction
        Ray controllerRay = new Ray(OVRInput.GetLocalControllerPosition(activeController),
                OVRInput.GetLocalControllerRotation(activeController) * Vector3.forward);
        int closestCtrlBoardID = FindIDClosestBoard(controllerRay, ref closestBoardPlane, ref closestHitPoint, ref closestBoard);
        // (do not check if currently drawing)
        int closestBoardID = ((OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController) <= 0.8f) &&
                              (closestFceBoardID == closestCtrlBoardID)) ? closestCtrlBoardID : -1;

        // then test if should switch board based on facing angle and controller position/orientation
        if (closestBoardID != -1) {
            TrySwitchBoard(closestBoardID, ref closestBoardPlane, ref facingRay, ref closestBoard);
        }

        float stickY = 0.0f;
        if (V1_TWO_HANDED_CONTROLS) {
            stickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, activeController).y;
            if (controlInProgress) {
                Debug.Log("CONTROL IN PROGRESS");
                if (Mathf.Abs(stickY) < 0.25f) {
                    controlInProgress = false;
                }

                return closestBoardID;
            }
        }

        if (closestBoardID == -1) {
            HandleObjectSelection(ChalktalkBoard.currentLocalBoardID, stickY, ref controlInProgress);
        }
        else {
            HandleObjectSelection(closestBoardID, stickY, ref controlInProgress);
        }

        return closestBoardID;
    }

    OVRInput.Controller prevHandTriggerDown = OVRInput.Controller.None;


    void Update()
    {
        init();

        HandleHandTrigger();

        HandleIndexTrigger();

        HandleSecondaryOneButton();

        // Handle two index trigger interaction
        // manipulation of the current board by two controllers
        ManipulateBoard();

        // update the pos of selected spheres
        updateSelected();

        // update the closest board and sketch selection
        int trySwitchClosest = UpdateBoardAndSelectObjects();

        // update the pos of cursor based on current board
        UpdateCursor(trySwitchClosest);
    }

    // assign gameObject in case it is null during the start func
    void init()
    {
        if (stylusSync == null)
            stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
    }

    // deal with hand trigger
    void HandleHandTrigger()
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


        bool previouslyHost = stylusSync.Host;
        if (handTriggerDown) {
            //print("drawPermissionsToggleInProgress:" + drawPermissionsToggleInProgress);
            if (!drawPermissionsToggleInProgress) {
                // toggle the stylus only if using the same hand
                if ((prevHandTriggerDown == activeController) || (!stylusSync.Host)) {
                    //print("toggle hand trigger");
                    stylusSync.ChangeSend();
                    if (stylusSync.Host) {
                        MSGSenderIns.GetIns().sender.Add(CommandToServer.STYLUS_RESET, new int[] { stylusSync.ID });
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
        selected.GetComponent<MeshRenderer>().enabled = stylusSync.Host;

        stylusSync.Data = (previouslyHost == stylusSync.Host) ? 1 : 2;    // moving by default
    }

    void HandleIndexTrigger()
    {
        // avoid quick switch between select and deselect
        bool isIndexTriggerDown = (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController) > 0.8f);
        if (isIndexTriggerDown) {
            if (!prevTriggerState) {
                stylusSync.Data = 0;
                print("data 0 onmousedown");
            }
        }
        else {
            if (prevTriggerState) {
                stylusSync.Data = 2;
                print("data 2 onmouseup");
            }
        }
        prevTriggerState = isIndexTriggerDown;
    }

    void HandleSecondaryOneButton()
    {
        // perspective mode
        if (activeController == OVRInput.Controller.LTouch || activeController == OVRInput.Controller.RTouch) {
            OVRInput.Controller nonDominantCtrl = (int)OVRInput.Controller.LTouch + (int)OVRInput.Controller.RTouch - activeController;
            bool curOneState = OVRInput.Get(OVRInput.Button.One, nonDominantCtrl);
            if (curOneState) {
                //if (!prevOneState) {
                perspView.DoObserve(0, OVRInput.GetLocalControllerPosition(nonDominantCtrl), OVRInput.GetLocalControllerRotation(nonDominantCtrl));
                prevOneState = curOneState;
                //}
            }
            else {
                if (prevOneState) {
                    // from button down to up
                    perspView.DoObserve(1);
                }
            }
            prevOneState = curOneState;
        }
    }

    bool prevDualIndex = false;
    Vector3[] prevDualPoses = new Vector3[2];
    Vector3[] curDualPoses = new Vector3[2];
    Quaternion[] prevDualRots = new Quaternion[2];
    Quaternion[] curDualRots = new Quaternion[2];
    public void ManipulateBoard()
    {
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.8 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.8) {
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

            ChalktalkBoard.latestUpdateFrame = Time.frameCount;
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
        if (angle < Utility.SwitchFaceThres) {
            // treat it as translation
            Vector3 averMove = (leftHandMove + rightHandMove) / 2;
            ChalktalkBoard.GetCurLocalBoard().transform.position += averMove;
            //print("moving averagly " + angle.ToString("F3"));
        }
        else if (angle > 180 - Utility.SwitchFaceThres) {
            // treat movement as rotation
            Vector3 prevHandLine = prevPos[1] - prevPos[0];
            Vector3 curHandLine = curPos[1] - curPos[0];
            Quaternion q = Quaternion.identity;
            q.SetFromToRotation(prevHandLine, curHandLine);
            ChalktalkBoard.GetCurLocalBoard().transform.rotation = q * ChalktalkBoard.GetCurLocalBoard().transform.rotation;
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
