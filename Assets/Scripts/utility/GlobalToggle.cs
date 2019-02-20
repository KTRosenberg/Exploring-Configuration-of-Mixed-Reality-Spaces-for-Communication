using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalToggle : MonoBehaviour
{

    public enum NetworkOption { Photon, Holojam, Holodeck };
    public NetworkOption networkForAvatar, networkForControl, networkForData;
    NetworkOption[] networkForAll;
    GameObject networkHolojam, networkHolodeck, networkPhoton;

    public enum LineOption { LineRenderer, Vectrosity };
    public LineOption rendererForLine;

    public enum FilledOption { Mesh };
    public FilledOption rendererForFilled;

    public enum TextOption { TextMesh, Vectrosity };
    public TextOption rendererForText;

    public enum PoolOption { Pooled, NotPooled };
    public PoolOption poolForSketch;

    public float ChalktalkBoardScale;
    public Vector2 chalktalkRes;

    public enum Configuration { sidebyside, mirror, eyesfree };
    public Configuration MRConfig;

    public string username = "zhenyi";

    void networkInit()
    {
        networkForAll = new NetworkOption[3];
        networkForAll[0] = networkForAvatar;
        networkForAll[1] = networkForControl;
        networkForAll[2] = networkForData;

        networkHolojam = GameObject.Find("holojam");
        networkHolojam.SetActive(false);
        networkHolodeck = GameObject.Find("holodeck");
        networkHolodeck.SetActive(false);
        networkPhoton = GameObject.Find("photon");
        networkPhoton.SetActive(false);

        foreach (NetworkOption networkOpt in networkForAll)
        {
            switch (networkOpt)
            {
                case NetworkOption.Holodeck:
                    networkHolodeck.SetActive(true);
                    break;
                case NetworkOption.Holojam:
                    networkHolojam.SetActive(true);
                    break;
                case NetworkOption.Photon:
                    networkPhoton.SetActive(true);
                    break;
                default:
                    break;
            }

        }
    }

    private void Awake()
    {
        networkInit();
        assignToIns();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void assignToIns()
    {
        GlobalToggleIns.GetInstance().rendererForFilled = rendererForFilled;
        GlobalToggleIns.GetInstance().rendererForLine = rendererForLine;
        GlobalToggleIns.GetInstance().rendererForText = rendererForText;
        GlobalToggleIns.GetInstance().poolForSketch = poolForSketch;
        GlobalToggleIns.GetInstance().ChalktalkBoardScale = ChalktalkBoardScale;
        GlobalToggleIns.GetInstance().chalktalkRes = chalktalkRes;
        GlobalToggleIns.GetInstance().username = username;
        GlobalToggleIns.GetInstance().MRConfig = MRConfig;
    }
}

public class GlobalToggleIns
{
    static public GlobalToggleIns GetInstance()
    {
        //if (instance == null)
        //    instance = new GlobalToggleIns();
        return instance;
    }

    static public GlobalToggleIns RefreshIns()
    {
        instance = new GlobalToggleIns();
        return instance;
    }

    public GlobalToggle.LineOption rendererForLine;

    public GlobalToggle.FilledOption rendererForFilled;

    public GlobalToggle.TextOption rendererForText;

    public GlobalToggle.PoolOption poolForSketch;

    public float ChalktalkBoardScale;

    public string username;

    public GlobalToggle.Configuration MRConfig;

    public Vector2 chalktalkRes;

    public Vector2 ChalktalkRes
    {
        get
        {
            //Some other code
            return chalktalkRes;
        }
        set
        {
            Debug.Log("VAL: " + value.x + ":" + value.y);
            //Some other code
            chalktalkRes = value;
            gt.chalktalkRes = value;
        }
    }

    public GlobalToggle gt;

    static GlobalToggleIns instance = new GlobalToggleIns();

    GlobalToggleIns()
    {
        gt = GameObject.Find("GlobalToggle").GetComponent<GlobalToggle>();
        //rendererForFilled = gt.rendererForFilled;
        //rendererForLine = gt.rendererForLine;
        //rendererForText = gt.rendererForText;
        //poolForSketch = gt.poolForSketch;
        //ChalktalkBoardScale = gt.ChalktalkBoardScale;
        //chalktalkRes = gt.chalktalkRes;
    }

    public void assignToInspector()
    {
        gt.rendererForFilled = rendererForFilled;
        gt.rendererForLine = rendererForLine;
        gt.rendererForText = rendererForText;
        gt.poolForSketch = poolForSketch;
        gt.ChalktalkBoardScale = ChalktalkBoardScale;
        gt.chalktalkRes = chalktalkRes;
        gt.username = username;
        gt.MRConfig = MRConfig;
    }
}