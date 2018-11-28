using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace Chalktalk
{
    public enum ChalktalkDrawType { STROKE = 0, FILL = 1, TEXT = 2 };

    public enum ChalkTalkType { CURVE, PROCEDURE };

    public class SketchCurve : MonoBehaviour
    {

        public Vector3[] points;
        public Color color = Color.black;
        public float width = 0.0f;

        public LineRenderer line;
        public MeshRenderer meshRenderer;
        public TextMesh textMesh;
        public ChalktalkDrawType type;
        public Mesh shape;
        public MeshFilter meshFilter;

        /// <summary>
        /// text
        /// </summary>
        public static float CT_TEXT_SCALE_FACTOR = 0.638f * 0.855f;
        public Vector3 textPos = Vector3.zero;
        public float textScale;
        public float facingDirection;
        public string text;

        /// <summary>
        /// vectorsity
        /// </summary>
        VectorLine vectrosityLine;
        VectorLine vText;
        public Transform forDrawTransform;

        /// <summary>
        /// color map
        /// </summary>
        public static IDictionary<Color, KeyValuePair<Material, Color>> colorToMaterialInfoMap = new Dictionary<Color, KeyValuePair<Material, Color>>();

        /// <summary>
        /// material
        /// </summary>
        public Material defaultMat;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InitWithText(string textStr, Vector3 textPos, float scale, float facingDirection, Color color)
        {
            if (GlobalToggleIns.GetInstance().rendererForLine == GlobalToggle.LineOption.Vectrosity)
            {
                DrawVectrosityText(textStr, textPos, scale, facingDirection, color);
            }
            else
            {
                InitTextMeshText(textStr, textPos, scale, facingDirection, color);
            }
        }

        void InitTextMeshText(string textStr, Vector3 tp, float scale, float fd, Color c)
        {
            gameObject.SetActive(true);

            // don't really need to save these to the object
            text = textStr;
            facingDirection = fd;
            textPos = tp;
            textScale = scale;
            color = c;

            textMesh.anchor = TextAnchor.MiddleCenter;

            // reorient to face towards you
            transform.localRotation = Quaternion.Euler(0, facingDirection, 0);
            //transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            //                    textMesh.fontSize = 3;
            //textMesh.font = Resources.Load("Nevis") as Font;
            textMesh.text = text;
            //textMesh.font = myfont;
            //textMesh.font.material = fontMat;
            textMesh.fontSize = 355;
            textMesh.characterSize = 0.1f;
            textMesh.color = color;
            if (!float.IsNaN(textPos.x))
            {
                transform.localPosition = textPos;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }


            transform.localScale = new Vector3(
            textScale * CT_TEXT_SCALE_FACTOR,
            textScale * CT_TEXT_SCALE_FACTOR, 1.0f);
        }

        void DrawVectrosityText(string textStr, Vector3 textPos, float textScale, float facingDirection, Color color)
        {
            if (vText == null)
            {
                vText = new VectorLine("3DText-" + textStr, new List<Vector3>(), 1.0f);
            }

            vText.color = color;
            //vText.drawTransform.localRotation = Quaternion.Euler(0, facingDirection, 0);

            vText.MakeText(textStr, Vector3.zero, textScale);
            forDrawTransform.localPosition = textPos;
            forDrawTransform.localRotation = Quaternion.Euler(0, facingDirection, 0);

            vText.drawTransform = forDrawTransform;


            //vText.MakeText(text, textPos, textScale);

            vText.Draw3D();
        }

        public void InitWithLines(Vector3[] points, Color color, float width)
        {
            if (GlobalToggleIns.GetInstance().rendererForLine == GlobalToggle.LineOption.Vectrosity)
            {
                DrawVectrosityLine(points, color, width);
            }
            else
            {
                DrawLineRendererLine(points, color, width);
            }
        }

        void DrawLineRendererLine(Vector3[] points, Color c, float width)
        {
            line.positionCount = points.Length;
            line.SetPositions(points);

            // do not replace the material if nothing has changed
            if (c == color)
            {
                return;
            }

            color = c;
            KeyValuePair<Material, Color> materialInfo;
            Color matColor;
            if (colorToMaterialInfoMap.TryGetValue(c, out materialInfo))
            {
                line.sharedMaterial = materialInfo.Key;
                matColor = materialInfo.Value;
                //Debug.Log("Reusing a color");
            }
            else
            {
                matColor = new Color(Mathf.Pow(c.r, 0.45f), Mathf.Pow(c.g, 0.45f), Mathf.Pow(c.b, 0.45f));
                Material mat = new Material(defaultMat);
                mat.SetColor("_Color", matColor);
                line.sharedMaterial = mat;

                colorToMaterialInfoMap.Add(c, new KeyValuePair<Material, Color>(mat, matColor));
                //Debug.Log("Adding a color");
            }

            line.startColor = matColor;
            line.endColor = matColor;
            // NEW
            //this.materialPropertyBlock.SetColor(colorPropID, c);
            //this.line.SetPropertyBlock(this.materialPropertyBlock);

            line.startWidth = width;
            line.endWidth = width;
        }

        void DrawVectrosityLine(Vector3[] points, Color color, float width)
        {
            // TODO: VectorLine only takes List
            List<Vector3> vPoints = new List<Vector3>(points);
            if (vectrosityLine == null)
            {
                vectrosityLine = new VectorLine("haha", vPoints, width * 1080.0f / 3.0f, LineType.Continuous);  //TODO
            }
            else
            {
                vectrosityLine.points3 = vPoints;
            }
            //vectrosityLine.material = defaultMat;
            Color c = new Color(Mathf.Pow(color.r, 0.45f), Mathf.Pow(color.g, 0.45f), Mathf.Pow(color.b, 0.45f));
            //vectrosityLine.material.color = c;
            //vectrosityLine.material.SetColor("_EmissionColor", c);

            vectrosityLine.SetColor(c);
            vectrosityLine.Draw3D();
        }

        public void InitWithFill(Vector3[] points, Color color)
        {
            Mesh shape = this.shape;
            shape.vertices = points;

            MeshRenderer mr = this.meshRenderer;
            MeshFilter filter = this.meshFilter;

            int countSides = points.Length;
            int countTris = countSides - 2;
            int[] indices = new int[countTris * 3 * 2];
            for (int i = 0, off = 0; i < countTris; ++i, off += 6)
            {
                indices[off] = 0;
                indices[off + 1] = i + 1;
                indices[off + 2] = i + 2;
                indices[off + 3] = 0;
                indices[off + 4] = i + 2;
                indices[off + 5] = i + 1;
            }
            shape.triangles = indices;
            Material mymat = new Material(defaultMat);
            // similar to what chalktalk do to the color TODO check if shader material is the same as what already exists (in which case, don't modify)
            Color c = new Color(Mathf.Pow(color.r, 0.45f), Mathf.Pow(color.g, 0.45f), Mathf.Pow(color.b, 0.45f));

            mymat.SetColor("_Color", c);
            mr.material = mymat;

            shape.RecalculateBounds();
            shape.RecalculateNormals();

            filter.mesh = shape;
        }
    }
}