using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script controls the teleportation of the camera and controllers to a new location (ex if someone
//wanted to view someone else's board...). In the scene I have a target that is used as the teleportation
//destination. I put some cubes surrounding target to give some visual indication of being
//somewhere else in the scene. Basically the cubes are just for demonstration. This script should
//be attatched to the object that will be teleporting in the Init script! Also I noticed something weird where if that
//SteamVR popup comes up, you have to X out of it otherwise the keyboard input will not work -Nina

public class Teleport : MonoBehaviour {

    [HideInInspector]
    public Transform newLocation; //a new location to teleport to for debugging purposes
    private Transform _originLocation; //your original location

    private OVRManager _manager;

    private void Start()
    {
        _originLocation = transform.parent.transform;
        _manager = gameObject.GetComponent<OVRManager>();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.T)) { //teleport if the T key is pressed
            if(gameObject.transform.position != _originLocation.transform.position) { //move back to the start location if we are elsewhere
                UpdatePosition(_originLocation);
                _manager.usePositionTracking = true; //turn on position tracking to unlock movement
                _manager.useIPDInPositionTracking = true;
                Debug.Log("Moving back to the start location");
            }
            else { //move to a new location!!
                UpdatePosition(newLocation);
                _manager.usePositionTracking = false; //turn off position tracking to lock movement
                _manager.useIPDInPositionTracking = false;
                Debug.Log("Moving to new location");
            }
        }
	}

    //This function just updates the camera (gameObject) transform to have the same rotation and position as
    //the new location that we are teleporting to.
    private void UpdatePosition(Transform t)
    {
        gameObject.transform.position = t.transform.position;
        gameObject.transform.rotation = t.transform.rotation;
    }
}
