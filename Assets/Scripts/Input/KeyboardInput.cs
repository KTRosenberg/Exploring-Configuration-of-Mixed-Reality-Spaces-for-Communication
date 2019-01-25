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
            ctRenderer.CreateBoard();
            // send to chalktalk
            msgSender.Send(2, new int[] { ctRenderer.ctBoards.Count });
            print("sending test:\t" + ctRenderer.ctBoards.Count);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {   
            //msgSender.Send("test");
            //msgSender.Send(0, new int[] { });
            StylusSyncTrackable stylusSync = GameObject.Find("Display").GetComponent<StylusSyncTrackable>();
            //msgSender.Send(1, new int[] { stylusSync.ID });
            msgSender.Send(2, new int[] { ctRenderer.ctBoards.Count });
            print("sending test:\t" + ctRenderer.ctBoards.Count);
        }
	}
}
