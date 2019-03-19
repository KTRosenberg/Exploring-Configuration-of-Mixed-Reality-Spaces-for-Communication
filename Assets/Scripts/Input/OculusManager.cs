using Oculus.Platform;
using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusManager : MonoBehaviour {

    // take care of avatar
    public OvrAvatar myAvatar;
    public GameObject remoteAvatarPrefab;

    // user id map, could be more smart later
    public Dictionary<string, string> mapLabelUserID;

    public List<Transform> remoteAvatars;
    public List<string> remoteNames;

    PerspectiveView perspView;
    
    public bool isObserving = false;
    

    void Awake()
    {
        myAvatar = GetComponent<OvrAvatar>();
        remoteNames = new List<string>();
        remoteAvatars = new List<Transform>();
        perspView = GetComponent<PerspectiveView>();

        Core.Initialize();
        Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        Request.RunCallbacks();  //avoids race condition with OvrAvatar.cs Start().
    }

    private void GetLoggedInUserCallback(Message<User> message)
    {
        if (!message.IsError)
        {
            myAvatar.oculusUserID = message.Data.ID.ToString();
        }
    }

    public Dictionary<string, SyncUserData> usernameToUserDataMap = new Dictionary<string, SyncUserData>();
    public void AddRemoteAvatarname(string name, ulong remoteid)
    {
        if(name != GlobalToggleIns.GetInstance().username)
        {
            // check if already added
            if (!remoteNames.Contains(name))
            {
                remoteNames.Add(name);
                GameObject go = Instantiate(remoteAvatarPrefab, transform.parent);
                go.name = "remote-" + name;
                remoteAvatars.Add(go.transform);

                //usernameToUserDataMap.Add(name, new SyncUserData());

                OculusAvatarSync ovs = go.GetComponent<OculusAvatarSync>();
                ovs.label = name + "avatar";
                ovs.isLocal = false;
                go.GetComponent<OvrAvatar>().oculusUserID = remoteid.ToString();
                // same to meta
                OculusMetaSync oms = go.GetComponent<OculusMetaSync>();
                oms.label = name + "Meta";
                oms.isLocal = false;
            }

        }
    }

    public void RemoveRemoteAvatarname(string name)
    {
        int index = remoteNames.IndexOf(name);
        
        if (index != -1) {
            remoteNames.Remove(name);
            remoteAvatars[index].gameObject.SetActive(false);
            remoteAvatars.RemoveAt(index);

            usernameToUserDataMap.Remove(name);
        }
    }

    // Use this for initialization
    void Start () {
        // init user id map
        mapLabelUserID = new Dictionary<string, string>();
        mapLabelUserID.Add("zhenyi", "1682533711857130");
        mapLabelUserID.Add("A1", "2182355055148104");
        mapLabelUserID.Add("A2", "2347129931970208");

        //localAvatar.GetComponent<AvatarManager>().label = localLabel;
        //remoteAvatars = new Transform[remoteLabels.Length];
        //TODO: use message to receive new logged oculus users and add more remote avatars based on that

        // send own name
        string curusername = GlobalToggleIns.GetInstance().username;
        MSGSenderIns.GetIns().sender.Add((int)CommandToServer.AVATAR_SYNC, curusername, myAvatar.oculusUserID);
               
        //applyConfiguration();        
    }
	
	// Update is called once per frame
	void Update () {
        applyConfiguration();
    }

    void OnDestroy()
    {
        print("bye bye");
        MSGSenderIns.GetIns().sender.Add((int)CommandToServer.AVATAR_LEAVE, GlobalToggleIns.GetInstance().username, myAvatar.oculusUserID);//msgSender.Add(3, curusername, myAvatar.oculusUserID);
    }

    //[RuntimeInitializeOnLoadMethod]
    //static void RunOnStart()
    //{
    //    UnityEngine.Application.quitting += Quit;
    //    UnityEngine.Application.wantsToQuit += WantsToQuit;
    //}

    static void Quit()
    {
        Debug.Log("Quitting the Player");
        MSGSenderIns.GetIns().sender.Add((int)CommandToServer.AVATAR_LEAVE, GlobalToggleIns.GetInstance().username, "0");//msgSender.Add(3, curusername, myAvatar.oculusUserID);
    }

    static bool WantsToQuit()
    {
        Debug.Log("Player prevented from quitting.");
        MSGSenderIns.GetIns().sender.Add((int)CommandToServer.AVATAR_LEAVE, GlobalToggleIns.GetInstance().username, "0");//msgSender.Add(3, curusername, myAvatar.oculusUserID);
        return true;
    }

    void applyRole()
    {
        // TODO: I forgot the reason why we need this
        //switch (role)
        //{
        //    case Role.Audience:
        //        // if i am the audience, sending my ovrcamera
        //        GameObject go = Instantiate(emptyHeadPrefab);
        //        go.GetComponent<HeadFlake>().isPresenter = false;
        //        go.GetComponent<HeadFlake>().label = localLabel + "head";
        //        //
        //        dataCollection.SetActive(false);
        //        break;
        //    case Role.Presentor:
        //        // if i am the presenter, receiving from audience about ovrcamera
        //        for (int i = 0; i < remoteLabels.Length; i++)
        //        {
        //            GameObject go2 = Instantiate(emptyHeadPrefab);
        //            go2.GetComponent<HeadFlake>().isPresenter = true;
        //            go2.GetComponent<HeadFlake>().label = remoteLabels[i] + "head";
        //            go2.transform.localScale = new Vector3(-1, 1, 1);
        //        }

        //        dataCollection.SetActive(false);
        //        break;
        //    default:
        //        break;
        //}
    }

    void applyConfiguration()
    {
        switch (GlobalToggleIns.GetInstance().MRConfig)
        {
            case GlobalToggle.Configuration.sidebyside:
                break;
            case GlobalToggle.Configuration.mirror:
                // flip the remote avatars
                // TODO: 
                foreach (Transform remoteAvatar in remoteAvatars) {
                    // for perspective mode, this specific avatar should be placed without mirroring; TODO for render texture.
                    if (GlobalToggleIns.GetInstance().perspMode != GlobalToggle.ObserveMode.RT && perspView.observeeName.Equals(remoteAvatar.name.Substring(7)))
                    {
                        //for observee
                        remoteAvatar.position = Vector3.zero;
                        remoteAvatar.rotation = Quaternion.identity;
                        remoteAvatar.localScale = Vector3.one;
                    }
                    
                    else
                    {
                        Transform remoteBase = remoteAvatar.Find("base");
                        //remoteAvatar.localScale = new Vector3(1, 1, -1);
                        int remoteCurBoardID = remoteAvatar.GetComponent<OculusAvatarSync>().remoteCurBoardID;
                        if (remoteCurBoardID != -1 && remoteCurBoardID < ChalktalkBoard.boardList.Count)
                        {
                            Transform remoteCurChalktalkBoard = ChalktalkBoard.boardList[remoteCurBoardID].transform;
                            Vector3 localPos = remoteCurChalktalkBoard.InverseTransformPoint(Vector3.zero);
                            localPos.x *= remoteCurChalktalkBoard.localScale.x;
                            localPos.y *= remoteCurChalktalkBoard.localScale.y;
                            localPos.z *= remoteCurChalktalkBoard.localScale.z;

                            Vector3 intersectionPoint = Vector3.ProjectOnPlane(localPos, Vector3.forward);
                            Vector3 mirrorPos = 2 * intersectionPoint - localPos;
                            mirrorPos.x /= remoteCurChalktalkBoard.localScale.x;
                            mirrorPos.y /= remoteCurChalktalkBoard.localScale.y;
                            mirrorPos.z /= remoteCurChalktalkBoard.localScale.z;


                            remoteAvatar.position = remoteCurChalktalkBoard.TransformPoint(mirrorPos);

                            Quaternion q = Quaternion.identity;
                            q.SetFromToRotation(Vector3.forward, remoteCurChalktalkBoard.forward);
                            remoteAvatar.forward = q * q * Vector3.forward;

                            Vector3 localFwd = remoteCurChalktalkBoard.InverseTransformPoint(-Vector3.forward);
                            localFwd.x *= remoteCurChalktalkBoard.localScale.x;
                            localFwd.y *= remoteCurChalktalkBoard.localScale.y;
                            localFwd.z *= remoteCurChalktalkBoard.localScale.z;

                            //Vector3 intersectionFwdPoint = Vector3.ProjectOnPlane(localFwd, Vector3.forward);
                            //Vector3 mirrorFwd = 2 * intersectionFwdPoint - localFwd;
                            //mirrorFwd.x /= remoteCurChalktalkBoard.localScale.x;
                            //mirrorFwd.y /= remoteCurChalktalkBoard.localScale.y;
                            //mirrorFwd.z /= remoteCurChalktalkBoard.localScale.z;

                            //remoteAvatar.forward = remoteCurChalktalkBoard.TransformPoint(-mirrorFwd);

                            //Plane boardPlane = new Plane(remoteCurChalktalkBoard.forward, remoteCurChalktalkBoard.TransformPoint(remoteBase.localPosition));
                            //float dis = 0;
                            //Ray userRay = new Ray(Vector3.zero, remoteBase.forward);
                            //boardPlane.Raycast(userRay, out dis);
                            //Vector3 intersectionDir = userRay.GetPoint(dis);
                            ////Vector3 remoteFwd = intersectionDir - mirrorPos;
                            //remoteAvatar.forward = intersectionDir - remoteAvatar.position;

                            //Vector3 boardFwd = remoteCurChalktalkBoard.forward;
                            //if ((Mathf.Abs(boardFwd.x) > Mathf.Abs(boardFwd.y)) && (Mathf.Abs(boardFwd.x) > Mathf.Abs(boardFwd.z)))
                            //    remoteAvatar.localScale = new Vector3(-1, 1, 1);
                            //else if ((Mathf.Abs(boardFwd.y) > Mathf.Abs(boardFwd.x)) && (Mathf.Abs(boardFwd.y) > Mathf.Abs(boardFwd.z)))
                            //    remoteAvatar.localScale = new Vector3(1, -1, 1);
                            //else
                            //    remoteAvatar.localScale = new Vector3(1, 1, -1);
                            remoteAvatar.localScale = new Vector3(1, 1, -1);
                            //Vector3 userFwd = remoteAvatar.forward;
                            //remoteAvatar.localScale = new Vector3((Mathf.Abs(boardFwd.x) > 0.01? -1 : 1),
                            //    (Mathf.Abs(boardFwd.y) > 0.01 ? -1 : 1),
                            //    (Mathf.Abs(boardFwd.z) > 0.01 ? -1 : 1));
                            //remoteAvatar.position = remoteCurChalktalkBoard.TransformPoint(mirrorPos);
                            //remoteAvatar.forward = remoteCurChalktalkBoard.TransformDirection()
                            remoteAvatar.gameObject.SetActive(true);
                        }
                        else
                        {
                            remoteAvatar.gameObject.SetActive(false);
                        }
                    }
                } 
                break;
            case GlobalToggle.Configuration.eyesfree:
                break;
            default:
                break;
        }
    }
}
