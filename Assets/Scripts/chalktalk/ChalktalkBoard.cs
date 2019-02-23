using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalktalkBoard : MonoBehaviour {

    public static int currentBoardID = 0;
    public static int curMaxBoardID = 0;//next one to create
    public int boardID;// the same as sketchPageID
    static List<ChalktalkBoard> returnBoardList = new List<ChalktalkBoard>();

    public static List<ChalktalkBoard> boardList = new List<ChalktalkBoard>();
    // TODO support duplicates as a list so IDs match with the boards
    public static List<List<ChalktalkBoard>> duplicateBoardList = new List<List<ChalktalkBoard>>(); 

    public static bool selectionInProgress = false;
    public static bool selectionWaitingForCompletion = false;

    public static int latestUpdateFrame = 0;

    // Use this for initialization
    void Start()
    {
        //returnBoardList = new List<ChalktalkBoard>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static int MaxExistingID()
    {
        // TODO handle eyes-free duplicate boards and possibly deletion of boards
        return ChalktalkBoard.boardList.Count - 1;
    }

    public enum ModeFlags {
        NONE = (1 << 0),
        TEMPORARY_BOARD_ON = (1 << 1),
        TEMPORARY_BOARD_TURNING_OFF = (1 << 2),
    }

    public static class Mode {
        public static ModeFlags flags = ModeFlags.NONE;
    }

    public static ChalktalkBoard GetCurBoard() {
        //return boardList[currentBoardID];
        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            if(boardList.Count > currentBoardID+1)
                return boardList[currentBoardID + 1];
            else
                return boardList[currentBoardID];
        }            
        else
            return boardList[currentBoardID];
    }

    public static List<ChalktalkBoard> GetBoard(int index)
    {
        returnBoardList.Clear();
        if (index >= curMaxBoardID)
            return returnBoardList;

        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            returnBoardList.Add(boardList[index + 1]);
            if (index == currentBoardID)
                returnBoardList.Add(boardList[0]);
        }
        else
            returnBoardList.Add(boardList[index]);
        return returnBoardList;
    }

    public static bool isOutlineOfFrame(Vector3[] points)
    {
        bool ret = true;
        if (points.Length == 5) {
            for (int i = 0; i < points.Length; i++) {
                if ((points[i].x >= 1.0f) && (points[i].x <= -1.0f)) {
                    ret = false;
                    break;
                }
            }
            return ret;
        }
        else
            return false;
    }

    public static void UpdateCurrentBoard(int id)
    {
        
        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            UpdateCurrentBoardEyesfree(id);
        }
        currentBoardID = id;
    }

    static void UpdateCurrentBoardEyesfree(int id)
    {
        // change whenever current board changes
        boardList[0].boardID = id;
        boardList[0].name = "Board" + id.ToString();
        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
        boardList[0].transform.localPosition = GetCurBoard().transform.TransformPoint(0, -0.5f, -0.5f);
        boardList[0].transform.localRotation = GetCurBoard().transform.rotation * Quaternion.Euler(90f, 0, 0);
        // disable prev helper
        EyesfreeHelper helper = boardList[currentBoardID + 1].gameObject.GetComponent<EyesfreeHelper>();
        if (helper != null) {
            helper.isFocus = false;
        }
        // update cursor
        helper = boardList[id + 1].gameObject.GetComponent<EyesfreeHelper>();
        if(helper == null) {
            helper = boardList[id + 1].gameObject.AddComponent<EyesfreeHelper>();
            
            helper.activeBindingbox = boardList[0].transform;
            helper.activeCursor = GameObject.Find("cursor").transform;
            helper.dupCursor = GameObject.Find("dupcursor").transform;
            helper.dupCursor.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
        }            
        helper.dupBindingbox = boardList[id + 1].gameObject.transform;
        helper.isFocus = true;
    }
}
