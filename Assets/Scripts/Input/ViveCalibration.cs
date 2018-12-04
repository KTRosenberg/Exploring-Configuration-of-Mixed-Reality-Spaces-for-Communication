using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveCalibration : MonoBehaviour {

    LHOwnSync ownLightHouse;
    LHRefSync refLightHouse;

    Transform curBoard;

	// Use this for initialization
	void Start () {
        ownLightHouse = GameObject.Find("Display").GetComponent<LHOwnSync>();
        refLightHouse = GameObject.Find("Display").GetComponent<LHRefSync>();
    }
	
	// Update is called once per frame
	void Update () {
		// update the board here
	}
}
