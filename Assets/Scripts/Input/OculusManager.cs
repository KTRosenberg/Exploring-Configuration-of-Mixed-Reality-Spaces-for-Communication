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



    // TODO: use msg to receive remote labels and create remote avatar and add to this array
    List<Transform> remoteAvatars;
    List<string> remoteNames;
    MSGSender msgSender;

    void Awake()
    {
        myAvatar = GetComponent<OvrAvatar>();
        remoteNames = new List<string>();
        remoteAvatars = new List<Transform>();

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
                OculusAvatarSync ovs = go.GetComponent<OculusAvatarSync>();
                ovs.label = name + "avatar";
                ovs.isLocal = false;
                go.GetComponent<OvrAvatar>().oculusUserID = remoteid.ToString();
            }

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

        msgSender = GameObject.Find("Display").GetComponent<MSGSender>();
        // send own name
        string curusername = GlobalToggleIns.GetInstance().username;
        msgSender.Add(3, curusername, myAvatar.oculusUserID);
        msgSender.Add(3, curusername, myAvatar.oculusUserID);
        msgSender.Add(3, curusername, myAvatar.oculusUserID);


        applyConfiguration();        
    }
	
	// Update is called once per frame
	void Update () {
		
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
                foreach (Transform remoteAvatar in remoteAvatars)
                    //remoteAvatar.localRotation = Quaternion.Euler(0, 180, 0);
                    remoteAvatar.localScale = new Vector3(-1, 1, 1);
                
                break;
            case GlobalToggle.Configuration.eyesfree:
                break;
            default:
                break;
        }
    }
}
