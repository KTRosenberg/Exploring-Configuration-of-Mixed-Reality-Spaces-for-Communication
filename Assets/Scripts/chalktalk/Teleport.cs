using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script controls the teleportation of the camera and controllers to a new location (ex if someone
//wanted to view someone else's board...). In the scene I have a target that is used as the teleportation
//destination. I put some cubes surrounding target to give some visual indication of being
//somewhere else in the scene. Basically the cubes are just for demonstration. This script should
//be attatched to the object that will be teleporting in the Init script! Also I noticed something weird where if that
//SteamVR popup comes up, you have to X out of it otherwise the keyboard input will not work -Nina

public class Teleport : MonoBehaviour
{

    [HideInInspector]
    public Transform newLocation; //a new location to teleport to for debugging purposes
    private Transform _originLocation; //your original location

    private OVRManager _manager;

    private OculusInput inputDevice;

    private bool alternativeViewEnabled = false;

    GameObject localAvatar;

    [System.Serializable]
    public struct ColorInterp {
        public Utility.Proc_ColorInterp interpProc;
        [HideInInspector]
        public float timeElapsed;
        public float timeDuration;

        public ColorInterp(Utility.Proc_ColorInterp interpProc, float timeDuration)
        {
            this.interpProc = interpProc;
            this.timeElapsed = 0.0f;
            this.timeDuration = timeDuration;
        }

        public void ReSet()
        {
            this.timeElapsed = 0.0f;
        }
    }
    public ColorInterp interp;

    [System.Serializable]
    public struct TransitionOverlay {
        public GameObject obj;
        public Color startColor;
        public Color endColor;
        [HideInInspector]
        public Material mat;
        [HideInInspector]
        public int colorID;
        [HideInInspector]
        public Renderer rend;

        public TransitionOverlay(TransitionOverlay transitionOverlay)
        {
            this.obj = Instantiate(transitionOverlay.obj);
            this.obj.name = "transitionOverlay";

            this.startColor = transitionOverlay.startColor;
            this.endColor = transitionOverlay.endColor;

            this.rend = this.obj.GetComponent<Renderer>();

            this.mat = new Material(rend.sharedMaterial);
            this.rend.sharedMaterial = this.mat;
            this.colorID = Shader.PropertyToID("_Color");

            UpdateColor(this.startColor);
        }
        public TransitionOverlay(GameObject prefab, Color startColor, Color endColor)
        {
            this.obj = Instantiate(prefab);
            this.obj.name = "transitionOverlay";

            this.startColor = startColor;
            this.endColor = endColor;

            this.rend = this.obj.GetComponent<Renderer>();

            this.mat = new Material(rend.sharedMaterial);
            this.rend.sharedMaterial = this.mat;
            this.colorID = Shader.PropertyToID("_Color");

            UpdateColor(this.startColor);
        }

        public void UpdateColor(Color c)
        {
            this.mat.SetColor(this.colorID, c);
        }
    }
    public TransitionOverlay transitionOverlay;

    public GlowObjectCmd glowOutlineCommand;

    private void Start()
    {
        _originLocation = transform.parent.transform;
        _manager = gameObject.GetComponent<OVRManager>();

        GameObject inputDeviceObject = GameObject.Find("oculusController");
        if (inputDeviceObject == null) {
            Debug.LogError("Cannot find input device");
        }
        inputDevice = inputDeviceObject.GetComponent<OculusInput>();

        localAvatar = GameObject.Find("LocalAvatar");

        //transitionOverlay.
    }


    bool OculusDoTeleport()
    {
        switch (inputDevice.activeController) {
        case OVRInput.Controller.LTouch:
            return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        case OVRInput.Controller.RTouch:
            return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);
        }

        return false;
    }

    void EnablePositionTracking()
    {
        _manager.usePositionTracking = true; //turn on position tracking to unlock movement
        //_manager.useIPDInPositionTracking = true;

        //localAvatar.SetActive(true);
        localAvatar.transform.localScale = Vector3.one;
    }

    void DisablePositionTracking()
    {
        _manager.usePositionTracking = false; //turn off position tracking to lock movement
        //_manager.useIPDInPositionTracking = false;

        //localAvatar.SetActive(false);
        localAvatar.transform.localScale = Vector3.Scale(localAvatar.transform.localScale, new Vector3(-1.0f, 1.0f, 1.0f));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) || OculusDoTeleport()) { //teleport if the T key is pressed
            if (gameObject.transform.position != _originLocation.transform.position) { //move back to the start location if we are elsewhere
                UpdatePosition(_originLocation);
                EnablePositionTracking();
                Debug.Log("Moving back to the start location");
                alternativeViewEnabled = false;
                gameObject.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
            }
            else { //move to a new location!!
                UpdatePosition(newLocation);
                DisablePositionTracking();
                Debug.Log("Moving to new location");
                alternativeViewEnabled = true;
                gameObject.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
            }
        }

        if (alternativeViewEnabled) {
            UpdatePosition(newLocation);
            //gameObject.transform.Rotate(new Vector3(0.0f, 10.0f * Time.deltaTime, 0.0f));
        }

        { // temp
            //Color transitionColor = interp.interpProc(transitionOverlay.startColor, transitionOverlay.endColor, interp.timeElapsed / interp.timeDuration);
            //transitionOverlay.UpdateColor(transitionColor);
            //interp.timeElapsed += Time.deltaTime;
            //if (interp.timeElapsed >= interp.timeDuration) {
            //    interp.ReSet();
            //}
        }
    }

    //This function just updates the camera (gameObject) transform to have the same rotation and position as
    //the new location that we are teleporting to.
    private void UpdatePosition(Transform t)
    {
        if (t != _originLocation) {
            CalculatePositionMirroredOverBoard(t);
        }

        gameObject.transform.position = t.transform.position; // + new Vector3(Mathf.Sin(Time.time / 2.0f), 0.0f, 0.0f);
                                                              //gameObject.transform.rotation = Quaternion.Euler(-1.0f * gameObject.transform.rotation.eulerAngles);


    }

    private void CalculatePositionMirroredOverBoard(Transform xform)
    {

        // borrowing face-to-face code temporarily, does not affect the cursor correctly TODO (I don't think we ever mirrored the cursor position actually)
        {
			// change GetCurLocalBoard() to activeBoardID if necessary
            Transform remoteCurChalktalkBoard = ChalktalkBoard.GetCurLocalBoard().transform;
            Vector3 localPos = remoteCurChalktalkBoard.InverseTransformPoint(Vector3.zero);
            localPos.x *= remoteCurChalktalkBoard.localScale.x;
            localPos.y *= remoteCurChalktalkBoard.localScale.y;
            localPos.z *= remoteCurChalktalkBoard.localScale.z;

            Vector3 intersectionPoint = Vector3.ProjectOnPlane(localPos, Vector3.forward);
            Vector3 mirrorPos = 2 * intersectionPoint - localPos;
            mirrorPos.x /= remoteCurChalktalkBoard.localScale.x;
            mirrorPos.y /= remoteCurChalktalkBoard.localScale.y;
            mirrorPos.z /= remoteCurChalktalkBoard.localScale.z;

            xform.position = remoteCurChalktalkBoard.TransformPoint(mirrorPos);



            //Quaternion q = Quaternion.identity;
            //q.SetFromToRotation(Vector3.forward, remoteCurChalktalkBoard.forward);
            //xform.forward = q * q * Vector3.forward;
        }
    }
}