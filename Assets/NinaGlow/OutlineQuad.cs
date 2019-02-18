using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineQuad : MonoBehaviour {
    public GameObject world; //parent of boards
    private int _boardID; //keeps track of which board the outline is currently placed at
    private ChalktalkBoard _boardObj;
    private bool _assignFirstBoard = false; //makes sure that the outline is assigned after the boards created
    private Renderer rend;

    private void Start()
    {
        this.rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update () {
        if (!_assignFirstBoard) {
            _assignFirstBoard = false;
            SetPosition();
            _boardID = ChalktalkBoard.currentBoardID;
            _boardObj = ChalktalkBoard.boardList[_boardID];
        }
        else if (_boardID != ChalktalkBoard.currentBoardID) { //only update position if a new board was selected
            _boardID = ChalktalkBoard.currentBoardID;
            _boardObj = ChalktalkBoard.boardList[_boardID];
            SetPosition();
        }
	}

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.name.Equals("CenterEyeAnchor")) {
            this.rend.enabled = false;
        }
    }

    private void OnTriggerStay(Collider c)
    {
        //Debug.Log(c.gameObject.name);
    }

    private void OnTriggerExit(Collider c)
    {
        if (c.gameObject.name.Equals("CenterEyeAnchor")) {
            this.rend.enabled = true;
        }
    }

    //Summary
    //Set position will find the board in the world that corresponds to the current
    //board id. Then, it will set the glowing outline to the same rotation, position,
    //and size of the currently selected board.
    void SetPosition(){
        GameObject boardToOutline = world.transform.Find("Board"+_boardID).gameObject;
        gameObject.transform.position = boardToOutline.transform.position;
        gameObject.transform.rotation = boardToOutline.transform.rotation;
        gameObject.transform.localScale = boardToOutline.transform.localScale;
    }
}
