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
    }

    void UpdateCursor()
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
            if (dot < Mathf.Cos(Utility.SwitchFaceThres * Mathf.Deg2Rad)) {
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
                if (dot < Mathf.Cos(Utility.SwitchCtrlThres * Mathf.Deg2Rad)) {
                    return false;
                }
            }
        }

        // all tests passed
        msgSender.Add((int)CommandFromServer.SKETCHPAGE_SET, new int[] { boardID });
        print("Select board: current closest board:" + boardID);

        return true;
    }

    public void HandleObjectSelection(int ctBoardID, float stickY, ref bool controlInProgress)
    {
        if (ChalktalkBoard.selectionInProgress) {
            if (stickY < -0.8f) {
                //Debug.Log("<color=red>" + "(Selection End)" + "</color>");

                ChalktalkBoard.selectionInProgress = false;
                controlInProgress = true;

                msgSender.Add((int)CommandFromServer.TMP_BOARD_OFF, new int[] { Time.frameCount, ctBoardID });

                ChalktalkBoard.selectionWaitingForCompletion = true;
            }
        }
        else if (stickY < -0.8f) {
            //Debug.Log("<color=green>" + "(Selection Begin)" + "</color>");

            ChalktalkBoard.selectionInProgress = true;
            controlInProgress = true;

            //Debug.Log("<color=red>SENDING COMMAND 6[" + Time.frameCount + "]</color>");
            msgSender.Add(6, new int[] { Time.frameCount });
        }
    }

    private void UpdateBoardAndSelectObjects()
    {
        activeController = OVRInput.GetActiveController();
        int boardCount = ctRenderer.ctBoards.Count;

        // handle creating-new-board operation
        if (OVRInput.GetDown(OVRInput.Button.Two, activeController)) {
            Debug.Log("creating a new board");
            msgSender.Add((int)CommandFromServer.SKETCHPAGE_CREATE, new int[] { ChalktalkBoard.currentBoardID + 1, 1 });
        }
        if (ChalktalkBoard.selectionWaitingForCompletion) {
            return;
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
        int closestBoardID = ((OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, activeController) <= 0.8f) && (closestFceBoardID == closestCtrlBoardID)) ? closestCtrlBoardID : -1;

        // then test if should switch board based on facing angle and controller position/orientation
        if (closestBoardID != -1 && closestBoardID != ChalktalkBoard.currentBoardID) {
            TrySwitchBoard(closestBoardID, ref closestBoardPlane, ref facingRay);
        }

        float stickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, activeController).y;
        if (controlInProgress) {
            if (Mathf.Abs(stickY) < 0.25f) {
                controlInProgress = false;
            }
            return;
        }

        HandleObjectSelection(closestBoardID, stickY, ref controlInProgress);
    }

    void Update()
    {
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
        if (stylusSync == null)
            stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
        selected.GetComponent<MeshRenderer>().enabled = stylusSync.Host;
        stylusSync.Data = 1;    // moving by default

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
        UpdateCursor();

        UpdateBoardAndSelectObjects();
    }
}
