using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chalktalk
{
    public class ChalktalkParse
    {
        public void Parse(byte[] bytes, ref List<SketchCurve> sketchCurves, ref CTEntityPool pool)
        {
            // Check the header
            string header = Utility.ParsetoString(bytes, 0, 8);

            if (header == "CTdata01")
            {
                ParseStroke(bytes, ref sketchCurves, ref pool);
            }
            else
            {
                //ParseProcedureAnimation(bytes, ref ctobj, GetComponent<Renderer>());
            }
        }

        string ParseTextForEachStroke(byte[] bytes, ref int cursor, int length)
        {
            string textStr = "";
            for (int j = 0; j < (length - 12); j++)
            {
                int curInt = Utility.ParsetoInt16(bytes, cursor);
                int res1 = curInt >> 8;
                int res2 = curInt - (res1 << 8);
                textStr += ((char)res1).ToString() + ((char)res2).ToString();
                cursor += 2;
            }
            return textStr;
        }

        void ParseEachStroke(byte[] bytes, ref int cursor, ref List<SketchCurve> sketchLines, ref CTEntityPool pool)
        {
            for (; cursor < bytes.Length;) {
                //The length of the current line
                int length = Utility.ParsetoInt16(bytes, cursor);
                cursor += 2;
                // if the line data is less than 12, we skip this one curve
                if (length < 12)
                    return;

                // The ID of current line, TODO: could be assigned as sketchPage index
                int ID = Utility.ParsetoInt16(bytes, cursor);
                //TODO
                //bool norender = false;
                //if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree && ID > 0)
                //norender = true;

                //ID = 0;
                cursor += 2;

                // Parse the color of the line
                Color color = Utility.ParsetoColor(bytes, cursor);
                cursor += 4;

                //Parse the Transform of this Curve; new version, use real float instead of fake float
                Vector3 translation = Utility.ParsetoRealVector3(bytes, cursor, 1);
                cursor += 6 * 2;
                Quaternion rotation = Utility.ParsetoRealQuaternion(bytes, cursor, 1);
                cursor += 6 * 2;
                float scale = Utility.ParsetoRealFloat(bytes, cursor);
                cursor += 2 * 2;
                //Debug.Log("header transformation:" + translation.ToString("F3") +"\t"+ scale.ToString("F3"));

                //Parse the type(line, filled, text) of the stroke
                int type = Utility.ParsetoInt16(bytes, cursor);
                cursor += 2;
                //Debug.Log("CT type:" + type);
                //Parse the width of the line
                float width = 0;
                //Debug.Log("Current Line's points count: " + (length - 12) / 4);

                if (GlobalToggleIns.GetInstance().poolForSketch == GlobalToggle.PoolOption.NotPooled) {

                }
                else {
                    ChalktalkDrawType ctType = (ChalktalkDrawType)type;
                    // parse text
                    if (ctType == ChalktalkDrawType.TEXT) {
                        string textStr = ParseTextForEachStroke(bytes, ref cursor, length);
                        if (textStr.Length >= 0) {
                            SketchCurve curve = pool.GetCTEntityText();
                            curve.InitWithText(textStr, translation, scale, 0/*renderer.facingDirection*/, color, ctType, ID);
                            sketchLines.Add(curve);
                            if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree && ID == ChalktalkBoard.activeBoardID) {
                                curve = pool.GetCTEntityText();
                                curve.InitWithText(textStr, translation, scale, 0/*renderer.facingDirection*/, color, ctType, ID);
                                curve.isDup = true;
                                sketchLines.Add(curve);
                            }
                        }
                        return;
                    }

                    // otherwise parse into points for stroke or fill
                    int pointCount = (length - 12) / 4;
                    Vector3[] points = new Vector3[pointCount];
                    for (int j = 0; j < pointCount; j += 1) {
                        Vector3 point = Utility.ParsetoRealVector3(bytes, cursor, 1);
                        points[j] = point;
                        cursor += 6 * 2;
                        width = Utility.ParsetoRealFloat(bytes, cursor);
                        cursor += 2 * 2;
                    }

                    // if (!norender) {
                    switch ((ChalktalkDrawType)type) {
                    case ChalktalkDrawType.STROKE: {
                        SketchCurve curve = pool.GetCTEntityLine();
                        curve.InitWithLines(points, /*isFrame ? new Color(1, 1, 1, 1) : */ color, width * 3, ctType, ID);
                        sketchLines.Add(curve);
                        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
                            sketchLines[sketchLines.Count - 1].isDup = true;
                            if (ID == ChalktalkBoard.activeBoardID) {
                                curve = pool.GetCTEntityLine();
                                curve.InitWithLines(points, /*isFrame ? new Color(1, 1, 1, 1) : */ color, width * 3, ctType, ID);
                                sketchLines.Add(curve);
                            }
                        }
                        break;
                    }
                    case ChalktalkDrawType.FILL: {
                        SketchCurve curve = pool.GetCTEntityFill();
                        curve.InitWithFill(points, /*isFrame ? new Color(1, 1, 1, 1) : */ color, ctType, ID);
                        sketchLines.Add(curve);
                        if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
                            sketchLines[sketchLines.Count - 1].isDup = true;
                            if (ID == ChalktalkBoard.activeBoardID) {
                                curve = pool.GetCTEntityLine();
                                curve.InitWithFill(points, /*isFrame ? new Color(1, 1, 1, 1) : */ color, ctType, ID);
                                sketchLines.Add(curve);
                            }
                        }
                        break;
                    }
                    default: {
                        break;
                    }
                    }
                    //}                
                }
            }
        }

        public void ParseStroke(byte[] bytes, ref List<SketchCurve> sketchLines, ref CTEntityPool pool)
        {
            // data byte cursor (skip the 8-byte header)
            int cursor = 8;
            // The total number of words in this packet, then get the size of the bytes size
            int sketchLineCnt = Utility.ParsetoInt16(bytes, cursor);

            //Debug.Log("CURVE COUNT: " + sketchLineCnt);
            cursor += 2;


            ParseEachStroke( bytes, ref cursor, ref sketchLines, ref pool);
        }

    }
}