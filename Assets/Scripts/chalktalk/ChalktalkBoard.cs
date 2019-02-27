using System.Collections.Generic;
using UnityEngine;

public class ChalktalkBoard : MonoBehaviour {

    public static int currentLocalBoardID = 0; // the board id of the current user, locally
		public static int activeBoardID = 0;	// the active board from chalktalk server side, universally
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

    public static ChalktalkBoard GetCurLocalBoard() {
        //return boardList[currentBoardID];
        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            if(boardList.Count > currentLocalBoardID+1)
                return boardList[currentLocalBoardID + 1];
            else
                return boardList[currentLocalBoardID];
        }            
        else
            return boardList[currentLocalBoardID];
    }

    public static List<ChalktalkBoard> GetBoard(int index)
    {
        returnBoardList.Clear();
        if (index >= curMaxBoardID)
            return returnBoardList;

        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            returnBoardList.Add(boardList[index + 1]);
            if (index == activeBoardID)
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

    public static void UpdateCurrentLocalBoard(int id)
    {
        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
            UpdateCurrentBoardEyesfree(id);
        }
        currentLocalBoardID = id;
    }

	public static void UpdateActiveBoard(int id) {
		activeBoardID = id;
	}

    static void UpdateCurrentBoardEyesfree(int id)
    {
        // change whenever current board changes
        boardList[0].boardID = id;
        boardList[0].name = "Board" + id.ToString();
        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
        boardList[0].transform.localPosition = GetCurLocalBoard().transform.TransformPoint(0, -0.5f, -0.5f);
        boardList[0].transform.localRotation = GetCurLocalBoard().transform.rotation * Quaternion.Euler(90f, 0, 0);
        // disable prev helper
        EyesfreeHelper helper = boardList[currentLocalBoardID + 1].gameObject.GetComponent<EyesfreeHelper>();
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
