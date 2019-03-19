using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveView : MonoBehaviour {

    private OVRManager ovrManager;
    private OculusManager oculusManager;
    private GameObject OVRCameraRig;

    private OvrAvatar ovrAvatar;
    private string observeeName;
    private SyncUserData observee;

    public bool isObserving;
    Vector3 posBeforeObserve;
    LineRenderer lr;

    void Start()
    {
        OVRCameraRig = GameObject.Find("OVRCameraRig");
        ovrManager = OVRCameraRig.GetComponent<OVRManager>();
        isObserving = false;

        oculusManager = gameObject.GetComponent<OculusManager>();
        ovrAvatar = gameObject.GetComponent<OvrAvatar>();
        lr = gameObject.GetComponent<LineRenderer>();
    }

    public void DoObserve(int state, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
    {
        //print("tryObserve start: curState " + isObserving);
        if (state == 0) {
            // button down
            if (!isObserving) {
                SelectObservee(pos, rot);
            }
        }
        else if (state == 1) {
            // button up
            if (!isObserving) {
                ObserveObservee();
            }
            else {
                DisableObserve();
            }
        }
        else if (state == 2) {
            // use keycode
            if (!isObserving) {
                if (oculusManager.remoteNames.Count > 0) {
                    oculusManager.usernameToUserDataMap.TryGetValue(oculusManager.remoteNames[0], out observee);
                    print("Observing:" + oculusManager.remoteNames[0]);
                    ObserveObservee();
                }
            }
            else {
                DisableObserve();
            }
                
        }
    //print("tryObserve end: curState " + isObserving);
}

void SelectObservee(Vector3 pos, Quaternion rot)
{
    // find the observee
    if (oculusManager.remoteAvatars.Count > 0) {
        // either use ray cast or 0 by default
        RaycastHit hit;
        int layerMask = 1 << 12;
        //layerMask = ~layerMask;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(pos, rot * Vector3.forward, out hit, Mathf.Infinity, layerMask)) {
            lr.SetPosition(0, pos);
            lr.SetPosition(1, pos + rot * Vector3.forward * hit.distance);
            //Gizmos.DrawLine(pos, rot * Vector3.forward * hit.distance);
            //Gizmos.color = Color.yellow;
            observeeName = hit.transform.name.Substring(7);//get rid of "remote-"
                                                           // if the observee is observing, then shift to next or just cancel this
            oculusManager.usernameToUserDataMap.TryGetValue(observeeName, out observee);
            print("Observing:" + observeeName);
        }
        else {
            //Gizmos.DrawRay(pos, rot * Vector3.forward);
            //Gizmos.color = Color.red;
            lr.SetPosition(0, pos);
            lr.SetPosition(1, pos + rot * Vector3.forward * 2);
            observee = null;
            observeeName = "";
        }
    }
}

void ObserveObservee()
{
    if (observee != null && !observee.UserIsObserving()) {
        //oculusManager.remoteAvatars[0].gameObject.SetActive(false);
        // turn off position tracking
        ovrManager.usePositionTracking = false;
        // turn off thrid view of local avatar
        ovrAvatar.ShowThirdPerson = false;
        // turn off packet record?
        ovrAvatar.RecordPackets = false;
        // record the pos
        posBeforeObserve = OVRCameraRig.transform.position;

        isObserving = true;
    }
    lr.SetPosition(0, Vector3.zero);
    lr.SetPosition(1, Vector3.zero);
}

void DisableObserve()
{
    // turn on position tracking
    ovrManager.usePositionTracking = true;
    // turn on thrid view of local avatar
    ovrAvatar.ShowThirdPerson = true;
    // turn on packet record?
    ovrAvatar.RecordPackets = true;
    // reset observee
    //if (oculusManager.remoteAvatars.Count > 0) {
    //    oculusManager.remoteAvatars[0].gameObject.SetActive(true);
    //}
    observee = null;
    OVRCameraRig.transform.position = Vector3.zero;

    isObserving = false;
}
    public float damping = 1;
    Vector3 observeOffset = new Vector3(0, -0.2f, 0.2f);
    void UpdateObservingPos()
{
    if (isObserving)
        if (observee != null) {
            // not sure
            Camera.main.transform.localPosition = Vector3.zero;
                Vector3 finalPos;
                if(GlobalToggleIns.GetInstance().perspMode == GlobalToggle.ObserveMode.FPP) {
                    finalPos = observee.position;
                }
                else {
                    float angle = observee.rotation.eulerAngles.y;
                    Vector3 rotatedOffset = Quaternion.Euler(0, angle, 0) * observeOffset;
                    finalPos = observee.position - rotatedOffset;
                }
                OVRCameraRig.transform.position = Vector3.Lerp(OVRCameraRig.transform.position, finalPos, Time.deltaTime * damping);
                // if we want the camera to look at the person
                //OVRCameraRig.transform.LookAt(target.transform);
            }

    }

private void Update()
{
    UpdateObservingPos();
}

}
