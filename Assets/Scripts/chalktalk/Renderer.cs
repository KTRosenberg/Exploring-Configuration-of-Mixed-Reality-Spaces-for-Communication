﻿using System.Collections;
using System.Collections.Generic;
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
        CTEntityPool entityPool;
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

            //msgSender.Send(0, new int[] { });
        }

        // Use this for initialization
        void Start()
        {
            msgSender.Add(5, new int[] { });

            Debug.Log("starting");
            world = GameObject.Find("World");
            ctBoards = new List<ChalktalkBoard>();

            ChalktalkBoard.boardList = ctBoards;

            CreateBoard();

            GameObject display = GameObject.Find("Display");
            displaySync = display.GetComponent<DisplaySyncTrackable>();
            ownLightHouse = display.GetComponent<LHOwnSync>();
            refLightHouse = display.GetComponent<LHRefSync>();

            ctSketchLines = new List<SketchCurve>();
            if (GlobalToggleIns.GetInstance().poolForSketch == GlobalToggle.PoolOption.Pooled)
            {
                entityPool = new CTEntityPool();
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
            if(ownLightHouse.Tracked && refLightHouse.Tracked)
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
                if(!entityPool.ApplyBoard(ctBoards))
                    CreateBoard(new Vector3(1.5f, 0, 1.5f), Quaternion.Euler(0, 90, 0));
                //foreach (SketchCurve sc in ctSketchLines)
                //sc.ApplyTransform(ctBoards);
                // draw them
                entityPool.FinalizeFrameData();
                // Draw()
            }


        }

        public void CreateBoard(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
        {
            // create the board based on the configuration
            switch (GlobalToggleIns.GetInstance().MRConfig)
            {
                case GlobalToggle.Configuration.sidebyside:
                    {
                        ChalktalkBoard ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                        ctBoard.boardID = ctBoards.Count;
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
                    if(ctBoards.Count == 0)
                    {
                        ChalktalkBoard ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                        ctBoard.boardID = ctBoards.Count;
                        ctBoard.name = "Board" + ctBoard.boardID.ToString();
                        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                        Vector3 boardPos = new Vector3(1, 0, 0);
                        boardPos = Quaternion.Euler(0, 90, 0) * boardPos;
                        ctBoard.transform.localPosition = boardPos;
                        ctBoard.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        //ctBoard.gameObject.transform.localScale *= GlobalToggleIns.GetInstance().ChalktalkBoardScale;

                        ctBoards.Add(ctBoard);
                    }
                    break;
                case GlobalToggle.Configuration.eyesfree:
                    if (ctBoards.Count == 0)
                    {
                        ChalktalkBoard ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                        ctBoard.boardID = ctBoards.Count;
                        ctBoard.name = "Board" + ctBoard.boardID.ToString();
                        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                        Vector3 boardPos = new Vector3(1, 0, 0);
                        boardPos = Quaternion.Euler(0, 90, 0) * boardPos;
                        ctBoard.transform.localPosition = boardPos;
                        ctBoard.transform.localRotation = Quaternion.Euler(90, 0, 0);
                        ctBoard.transform.Translate(0, -1, 0);
                        //ctBoard.gameObject.transform.localScale *= GlobalToggleIns.GetInstance().ChalktalkBoardScale;

                        ChalktalkBoard ctBoardDup = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
                        ctBoardDup.boardID = ctBoards.Count;
                        ctBoardDup.name = "Board" + ctBoard.boardID.ToString() + "Dup";
                        // we can decide the position and rotation by the amount, currently we support eight at most, so four in the first circle and four the the second if exist
                        boardPos = new Vector3(1, 1, 0);
                        boardPos = Quaternion.Euler(0, 90, 0) * boardPos;
                        ctBoardDup.transform.localPosition = boardPos;
                        ctBoardDup.transform.localRotation = Quaternion.Euler(0, 0, 0);

                        EyesfreeHelper helper = ctBoardDup.gameObject.AddComponent<EyesfreeHelper>();
                        helper.activeBindingbox = ctBoard.transform;
                        helper.activeCursor = GameObject.Find("cursor").transform;
                        helper.dupBindingbox = ctBoardDup.transform;
                        helper.dupCursor = GameObject.Find("dupcursor").transform;
                        helper.dupCursor.Find("Cube").GetComponent<MeshRenderer>().enabled = true;

                        ctBoards.Add(ctBoard);
                        ctBoards.Add(ctBoardDup);
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
