using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KeyboardInput : MonoBehaviour {

    GameObject ChalktalkHandler;
    Chalktalk.Renderer ctRenderer;
    CreateSPSync createSP;
    MSGSender msgSender;

    // Use this for initialization
    void Start () {
        ChalktalkHandler = GameObject.Find("ChalktalkHandler");
        ctRenderer = ChalktalkHandler.GetComponent<Chalktalk.Renderer>();
        msgSender = GameObject.Find("Display").GetComponent<MSGSender>();
        //msgSender.Send(0, new int[] { });
        msgSender.Add(0, new int[] { });
    }
	
	// Update is called once per frame
	void Update () {
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // use for testing
            //ctRenderer.CreateBoard();
            // add a new page
            //msgSender.Add(2, new int[] { ctRenderer.ctBoards.Count });
            msgSender.Add(2, new int[] { ChalktalkBoard.currentBoardID + 1, 1 });
            // ask for resolution
            msgSender.Add(0, new int[] { });
            // switch btw pages
            msgSender.Add(4, new int[] { ChalktalkBoard.currentBoardID + 1 });
            msgSender.Add(4, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID - 1, ChalktalkBoard.MaxExistingID()) });
            // test ownership
            //msgSender.Add(1, new int[] { 0 });
            // init
            msgSender.Add(5, new int[] { });
            //print("sending test:\t" + ctRenderer.ctBoards.Count);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {   
            //msgSender.Send("test");
            //msgSender.Send(0, new int[] { });
            //StylusSyncTrackable stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
            //msgSender.Send(1, new int[] { stylusSync.ID });
            msgSender.Add(2, new int[] { ctRenderer.ctBoards.Count });
            print("sending test:\t" + ctRenderer.ctBoards.Count);
        }
	}
}
