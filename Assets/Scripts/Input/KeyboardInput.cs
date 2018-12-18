using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KeyboardInput : MonoBehaviour {

    GameObject ChalktalkHandler;
    Chalktalk.Renderer ctRenderer;
    CreateSPSync createSP;

    // Use this for initialization
    void Start () {
        ChalktalkHandler = GameObject.Find("ChalktalkHandler");
        ctRenderer = ChalktalkHandler.GetComponent<Chalktalk.Renderer>();
        
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // use for testing
            ctRenderer.CreateBoard();
            // send to chalktalk
            if(createSP == null)
                createSP = GameObject.Find("Display").GetComponent<CreateSPSync>();
            createSP.boardsCnt = ctRenderer.ctBoards.Count;
            createSP.SetHost(true);
        }
	}
}
