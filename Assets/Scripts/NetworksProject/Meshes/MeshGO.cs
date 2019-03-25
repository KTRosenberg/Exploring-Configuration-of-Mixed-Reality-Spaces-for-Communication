using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGO : MonoBehaviour {
    public MeshContent.MeshData meshData;

    public MeshFilter filter;
    public MeshRenderer meshRenderer;

    public bool isDup;

    // test
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    public void Init(MeshContent.MeshData meshData)
    {
        this.meshData = meshData;
        isDup = false;
        UpdateMeshDataAll();
    }

    public void UpdateMeshDataTransform()
    {
        //transform.position = meshData.position;
        //transform.rotation = Quaternion.Euler(
        //    meshData.rotation.y * Mathf.Rad2Deg, -meshData.rotation.x * Mathf.Rad2Deg, meshData.rotation.z * Mathf.Rad2Deg
        //);
        //transform.localScale = meshData.scale;
        // test
        pos = meshData.position;
        rot = Quaternion.Euler(
            meshData.rotation.x * Mathf.Rad2Deg, meshData.rotation.y * Mathf.Rad2Deg, meshData.rotation.z * Mathf.Rad2Deg
        );
        scale = meshData.scale;
        //Debug.Log(scale.ToString("F3"));
    }
    public void UpdateMeshDataAll()
    {
        filter.mesh = meshData.mesh;
        UpdateMeshDataTransform();
    }
}
