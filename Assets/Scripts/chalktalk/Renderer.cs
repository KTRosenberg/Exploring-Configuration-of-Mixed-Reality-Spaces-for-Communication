using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Chalktalk
{
    // retrieve label display from display object under holojam
    public class Renderer : MonoBehaviour
    {
        // container for display data
        byte[] displayData;

        // labels
        DisplaySyncTrackable displaySync;

        MeshDisplaySyncTrackable displaySyncMesh;

        // world
        GameObject world;

        // prefab for each line in chalktalk sketch
        public SketchCurve sketchLine; // or we can just create a new instance when we want to if it is not a gameObject

        // container for chalktalk board and chalktalk line
        [SerializeField]
        public List<ChalktalkBoard> ctBoards; // multiple chalktalk boards, support runtime creation
        List<SketchCurve> ctSketchLines;
        public ChalktalkBoard ctBoardPrefab;

        // parser
        ChalktalkParse ctParser;

        // pool support
        public CTEntityPool entityPool = new CTEntityPool();
        public int initialLineCap = 0;
        public int initialFillCap = 0;
        public int initialTextCap = 0;

        // vive calibration
        LHOwnSync ownLightHouse;
        LHRefSync refLightHouse;

        // for resolution
        MSGSender msgSender;

        private void Awake()
        {
            msgSender = GameObject.Find("Display").GetComponent<MSGSender>();
        }

        // Use this for initialization
        void Start()
        {
            //msgSender.Add((int)CommandToServer.INIT_COMBINE, new int[] { });

            Debug.Log("starting");
            world = GameObject.Find("World");
            ctBoards = new List<ChalktalkBoard>();

            ChalktalkBoard.boardList = ctBoards;

            CreateBoard();

            GameObject display = GameObject.Find("Display");
            displaySync = display.GetComponent<DisplaySyncTrackable>();

            displaySyncMesh = display.GetComponent<MeshDisplaySyncTrackable>();

            ownLightHouse = display.GetComponent<LHOwnSync>();
            refLightHouse = display.GetComponent<LHRefSync>();

            ctSketchLines = new List<SketchCurve>();
            if (GlobalToggleIns.GetInstance().poolForSketch == GlobalToggle.PoolOption.Pooled)
            {
                entityPool.Init(
                    sketchLine.gameObject, sketchLine.gameObject, sketchLine.gameObject,
                    initialLineCap, initialFillCap, initialTextCap
                );
            }
            //displayData = new byte[0];
            ctParser = new ChalktalkParse();
        }

        // Update is called once per frame
        void Update()
        {
            // update all boards' transform
            if (ownLightHouse.Tracked && refLightHouse.Tracked)
            {
                Matrix4x4 mOwn = Matrix4x4.TRS(ownLightHouse.Pos, ownLightHouse.Rot, Vector3.one);
                Matrix4x4 mRef = Matrix4x4.TRS(refLightHouse.Pos, refLightHouse.Rot, Vector3.one);
                foreach (ChalktalkBoard b in ctBoards)
                {
                    Vector3 p = b.transform.position;
                    // TODO: p should be the original place in source's coordinate system
                    // which is 0,1,0 for now
                    p = new Vector3(0, 1f, 0);
                    Vector4 p4 =  mOwn * mRef.inverse * new Vector4(p.x, p.y, p.z, 1);
                    b.transform.position = new Vector3(p4.x, p4.y, p4.z);

                    Matrix4x4 mq = Matrix4x4.Rotate(Quaternion.identity);
                    b.transform.rotation = ( mOwn * mRef.inverse * mq).rotation;
                }
            }

            //
            if (displaySync.Tracked && displaySync.publicData != null && displaySync.publicData.Length > 0)
            {
                // retrieve and parse the data
                ctSketchLines.Clear();
                ctParser.Parse(displaySync.publicData, ref ctSketchLines, ref entityPool);
                // apply the transformation from the specific board to corresponding data
                while(!entityPool.ApplyBoard(ctBoards))
                    CreateBoard(new Vector3(1.5f, 0, 1.5f), Quaternion.Euler(0, 90, 0));
                //foreach (SketchCurve sc in ctSketchLines)
                //sc.ApplyTransform(ctBoards);
                // draw them
                entityPool.FinalizeFrameData();
                // Draw()
            }


            if (displaySyncMesh.Tracked && displaySyncMesh.publicData != null && displaySyncMesh.publicData.Length > 0) {
                ctParser.ParseMesh(displaySyncMesh.publicData);
            }
        }

        StringBuilder sbDebug = new StringBuilder();
        public string meshMessage;
        public void CreateBoard(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
        {
            // create the board based on the configuration
            switch (GlobalToggleIns.GetInstance().MRConfig)
            {
                case GlobalToggle.Configuration.sidebyside:
                    {
                        ChalktalkBoard ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                        ctBoard.boardID = ChalktalkBoard.curMaxBoardID++;
                        ctBoard.name = "Board" + ctBoard.boardID.ToString();
                        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                        Vector3 boardPos = new Vector3(ctBoards.Count / 4 + 1, 0, 0);
                        boardPos = Quaternion.Euler(0, (ctBoards.Count + 1) * -90 + ctBoards.Count / 4 * 45, 0) * boardPos;
                        //boardPos.z += 2;
                        ctBoard.transform.localPosition = boardPos;
                        ctBoard.transform.localRotation = Quaternion.Euler(0, ctBoards.Count * -90 + ctBoards.Count / 4 * 45, 0);
                        //ctBoard.gameObject.transform.localScale *= GlobalToggleIns.GetInstance().ChalktalkBoardScale;

                        ctBoards.Add(ctBoard);
                    }
                    break;
                case GlobalToggle.Configuration.mirror:
                    //if(ctBoards.Count == 0)
                    {
                        ChalktalkBoard ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                        ctBoard.boardID = ChalktalkBoard.curMaxBoardID++;
                        ctBoard.name = "Board" + ctBoard.boardID.ToString();
                        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                        Vector3 boardPos = new Vector3(1, 0, 0);
                        boardPos = Quaternion.Euler(0, (ctBoard.boardID + 1) * -90 + ctBoard.boardID / 4 * 45, 0) * boardPos;
                        ctBoard.transform.localPosition = boardPos;
                        ctBoard.transform.localRotation = Quaternion.Euler(0, ctBoard.boardID * -90 + ctBoards.Count / 4 * 45, 0);
                        //ctBoard.gameObject.transform.localScale *= GlobalToggleIns.GetInstance().ChalktalkBoardScale;

                        ctBoards.Add(ctBoard);
                    }
                    break;
                case GlobalToggle.Configuration.eyesfree: {
                    ChalktalkBoard ctBoard = null;
                    bool isInit = false;

                    ChalktalkBoard ctBoardDup = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                    ctBoardDup.boardID = ChalktalkBoard.curMaxBoardID++;
                    ctBoardDup.name = "Board" + ctBoardDup.boardID.ToString() + "Dup";
                    // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                    int n = ctBoardDup.boardID;
                    Vector3 boardPos2 = new Vector3(n / 4 + 1, 0, 0);
                    boardPos2 = Quaternion.Euler(0, (n + 1) * -90 + n / 4 * 45, 0) * boardPos2;
                    ctBoardDup.transform.localPosition = boardPos2;
                    ctBoardDup.transform.localRotation = Quaternion.Euler(0, ctBoardDup.boardID * -90 + n / 4 * 45, 0);
                
                if (ctBoardDup.boardID == 0) {
                    isInit = true;
                    ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                    // change whenever current board changes
                    ctBoard.boardID = ctBoardDup.boardID;
                    ctBoard.name = "Board" + ctBoard.boardID.ToString();
                    // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                    ctBoard.transform.localPosition = ctBoardDup.transform.TransformPoint(0, -0.5f, -0.5f);
                    ctBoard.transform.localRotation = ctBoardDup.transform.rotation * Quaternion.Euler(90f,0,0);
                    //Vector3 boardPos = new Vector3(1, 0, 0);
                    //boardPos = Quaternion.Euler(0, -90, 0) * boardPos;
                    //ctBoard.transform.localPosition = boardPos;
                    //ctBoard.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    //ctBoard.transform.Translate(0, -1, 1);

                    ctBoards.Add(ctBoard);
                }
                ctBoards.Add(ctBoardDup);
                if (isInit) {
                        EyesfreeHelper helper = ctBoardDup.gameObject.AddComponent<EyesfreeHelper>();
                    helper.isFocus = true;
                        helper.activeBindingbox = ctBoard.transform;
                        helper.activeCursor = GameObject.Find("cursor").transform;
                        helper.dupBindingbox = ctBoardDup.transform;
                        helper.dupCursor = GameObject.Find("dupcursor").transform;
                        helper.dupCursor.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
                    }
                }                    
                    break;
                default:
                    break;
            }
            

        }

        //void Draw()
        //{
        //    for (int i = 0; i < ctSketchLines.Count; i += 1)
        //    {
        //        ctSketchLines[i].Draw();
        //    }
        //}
    }

}
