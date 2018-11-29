using System.Collections;
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
        List<ChalktalkBoard> ctBoards; // multiple chalktalk boards, support runtime creation
        List<SketchCurve> ctSketchLines;
        public ChalktalkBoard ctBoardPrefab;

        // parser
        ChalktalkParse ctParser;

        // pool support
        CTEntityPool entityPool;
        public int initialLineCap = 0;
        public int initialFillCap = 0;
        public int initialTextCap = 0;

        // Use this for initialization
        void Start()
        {
            displaySync = GameObject.Find("Display").GetComponent<DisplaySyncTrackable>();
            world = GameObject.Find("World");
            ctBoards = new List<ChalktalkBoard>();
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

            CreateBoard();
        }


        // Update is called once per frame
        void Update()
        {
            
            if (displaySync.Tracked && displaySync.publicData != null && displaySync.publicData.Length > 0)
            {
                // retrieve and parse the data
                ctSketchLines.Clear();
                ctParser.Parse(displaySync.publicData, ref ctSketchLines, ref entityPool);
                // apply the transformation from the specific board to corresponding data
                foreach (SketchCurve sc in ctSketchLines)
                    sc.ApplyTransform(ctBoards);
                // draw them
                entityPool.FinalizeFrameData();
                // Draw()
            }


        }

        public void CreateBoard()
        {
            ChalktalkBoard ctBoard = Instantiate(ctBoardPrefab, world.transform) as ChalktalkBoard;
            ctBoard.boardID = ctBoards.Count;
            ctBoard.name = "Board" + ctBoard.boardID.ToString();
            ctBoard.gameObject.transform.localScale *= GlobalToggleIns.GetInstance().ChalktalkBoardScale;
            ctBoards.Add(ctBoard);

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
