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

        // prefab for each line in chalktalk sketch
        public SketchCurve sketchLine; // or we can just create a new instance when we want to if it is not a gameObject

        // container for chalktalk board and chalktalk line
        [SerializeField]
        List<ChalktalkBoard> ctBoards; // multiple chalktalk boards, support runtime creation
        List<SketchCurve> ctSketchLines;

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
        }


        // Update is called once per frame
        void Update()
        {
            // retrieve and parse the data
            if (displaySync.Tracked && displaySync.publicData != null)
            {
                ctParser.Parse(displaySync.publicData, ref ctSketchLines, ref entityPool);
            // draw them
            entityPool.FinalizeFrameData();
            // Draw()
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
