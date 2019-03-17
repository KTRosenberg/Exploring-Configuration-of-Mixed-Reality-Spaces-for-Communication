using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTest : MonoBehaviour {

    public Material mat;
    private Material sharedMaterial;
    public MeshContent.MeshData data;
	void Start () {
        data = MeshContent.CreateCubeMesh(0);
        data.mat = new Material(mat);

        //GameObject go = new GameObject("WEE");
        //go.AddComponent<MeshFilter>().mesh = data.mesh;
        //go.AddComponent<MeshRenderer>().sharedMaterial = data.mat;
        
	}
	
	void Update () {
        Graphics.DrawMesh(data.mesh, data.xform, data.mat, 0);
	}
}
