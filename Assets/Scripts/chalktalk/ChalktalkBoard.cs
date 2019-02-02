using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalktalkBoard : MonoBehaviour {

    public static int currentBoardID = 0;
    public int boardID;// the same as sketchPageID

    public static List<ChalktalkBoard> boardList = new List<ChalktalkBoard>();

    public static bool selectionInProgress = false;
    public static bool selectionWaitingForCompletion = false;
    // Use this for initialization
    void Start()
    {

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
}
