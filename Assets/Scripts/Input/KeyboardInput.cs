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
    static int toggle = 0;


    void Update()
    {    
        if (Input.GetKeyDown(KeyCode.Space)) {
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
        if (Input.GetKeyDown(KeyCode.M)) {   
            //msgSender.Send("test");
            //msgSender.Send(0, new int[] { });
            //StylusSyncTrackable stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
            //msgSender.Send(1, new int[] { stylusSync.ID });
            msgSender.Add(2, new int[] { ctRenderer.ctBoards.Count });
            print("sending test:\t" + ctRenderer.ctBoards.Count);
        }
        //if (Input.GetKeyDown(KeyCode.B)) {
        //    switch (ChalktalkBoard.Mode.flags) {
        //        case ChalktalkBoard.ModeFlags.NONE: {
        //            msgSender.Send(6, new int[] { });
        //            break;
        //        }
        //        case ChalktalkBoard.ModeFlags.TEMPORARY_BOARD_ON: {
        //            msgSender.Send(7, new int[] { });
        //            break;
        //        }
        //        case ChalktalkBoard.ModeFlags.TEMPORARY_BOARD_TURNING_OFF: {
        //            ChalktalkBoard.Mode.flags = ChalktalkBoard.ModeFlags.NONE;
        //            break;
        //        }
        //    }
            
        //    Debug.Log("sending test board transferring message");
        //}
        if (Input.GetKeyDown(KeyCode.B)) {
            // temporarily just moves the currently selected sketch to the next board
            msgSender.Send(6, new int[] { });
        }

        if (Input.GetKeyDown(KeyCode.Minus)) {
            msgSender.Send(4, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID + 1, 4) });
        }
        if (Input.GetKeyDown(KeyCode.Equals)) {
            msgSender.Send(4, new int[] { Utility.Mod(ChalktalkBoard.currentBoardID - 1, 4) });
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            // toggle
            toggle = 1 - toggle;
            msgSender.Send(5, new int[] { toggle, 562000 });
        }
	}
}
