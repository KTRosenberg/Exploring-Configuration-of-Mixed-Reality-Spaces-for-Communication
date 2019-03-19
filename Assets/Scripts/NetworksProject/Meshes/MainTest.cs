using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTest : MonoBehaviour {

    public Material mat;
    private Material sharedMaterial;
    public MeshContent.MeshData data;
	void Start () {
        sharedMaterial = new Material(mat);

        //GameObject go = new GameObject("WEE");
        //go.AddComponent<MeshFilter>().mesh = data.mesh;
        //go.AddComponent<MeshRenderer>().sharedMaterial = data.mat;
        
	}

    void Update() {
        MeshContent.MeshData asset;

        Debug.Log("Asset Count: " + MeshContent.meshAssets.Count);
        for (int i = 0; i < MeshContent.meshAssets.Count; i += 1) {
            asset = MeshContent.meshAssets[i];
            Graphics.DrawMesh(asset.mesh, asset.xform, sharedMaterial, 0);
        }
	}
}
