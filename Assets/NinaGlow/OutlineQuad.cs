using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineQuad : MonoBehaviour {
    public GameObject world; //parent of boards
    private int _board; //keeps track of which board the outline is currently placed at
    private bool _assignFirstBoard = false; //makes sure that the outline is assigned after the boards created
	
	// Update is called once per frame
	void Update () {
        if (!_assignFirstBoard) {
            _assignFirstBoard = false;
            SetPosition();
            _board = ChalktalkBoard.currentBoardID;
        }
        else if (_board != ChalktalkBoard.currentBoardID) { //only update position if a new board was selected
            _board = ChalktalkBoard.currentBoardID;
            SetPosition();
        }
	}

    //Summary
    //Set position will find the board in the world that corresponds to the current
    //board id. Then, it will set the glowing outline to the same rotation, position,
    //and size of the currently selected board.
    void SetPosition(){
        GameObject boardToOutline = world.transform.Find("Board"+_board).gameObject;
        gameObject.transform.position = boardToOutline.transform.position;
        gameObject.transform.rotation = boardToOutline.transform.rotation;
        gameObject.transform.localScale = boardToOutline.transform.localScale;
    }
}
