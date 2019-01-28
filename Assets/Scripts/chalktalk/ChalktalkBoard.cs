using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalktalkBoard : MonoBehaviour {

    public static int currentBoardID = 0;
    public int boardID;// the same as sketchPageID

    public static List<ChalktalkBoard> boardList;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static int MaxExistingID()
    {
        // TODO handle eyes-free duplicate boards and possibly deletion of boards
        return ChalktalkBoard.boardList.Count - 1;
    }
}
