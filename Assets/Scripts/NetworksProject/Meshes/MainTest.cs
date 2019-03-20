using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTest : MonoBehaviour {

    public Material mat;
    private Material sharedMaterial;
    public MeshContent.MeshData data;
    Vector3 originalPosition;


    MeshFilter filter;
    MeshRenderer rend;

	void Start () {
        sharedMaterial = new Material(mat);

        filter = gameObject.AddComponent<MeshFilter>();
        rend = gameObject.AddComponent<MeshRenderer>();
        rend.sharedMaterial = sharedMaterial;

        originalPosition = transform.position;
	}

    float prevX = 0.0f;
    float prevY = 0.0f;
    float prevZ = 0.0f;

    float zChangeCounter = 0;

    void Update() {
        //Debug.Log("Asset Count: " + MeshAsset.idToMeshMap.Count);
        foreach (KeyValuePair<ushort, MeshContent.MeshData> entry in MeshContent.idToMeshMap) {
            MeshContent.MeshData meshInfo = entry.Value;

            if (zChangeCounter != 0) {

                zChangeCounter -= 1;

                if (meshInfo.position.x != prevX ||
                    meshInfo.position.y != prevY) {

                    zChangeCounter = 0;

                    prevX = meshInfo.position.x;
                    prevY = meshInfo.position.y;
                    prevZ = meshInfo.position.z;
                }
                else {
                    continue;
                }
            }
            else {

                if (meshInfo.position.z != prevZ) {
                    prevZ = meshInfo.position.z;

                    if (meshInfo.position.x == prevX &&
                        meshInfo.position.y == prevY) {

                        zChangeCounter = 3;

                        continue;
                    }
                }
            }

            prevX = meshInfo.position.x;
            prevY = meshInfo.position.y;
            prevZ = meshInfo.position.z;

            filter.mesh = meshInfo.mesh;
            this.transform.position = originalPosition + meshInfo.position;
            this.transform.localScale = meshInfo.scale;
            this.transform.rotation = Quaternion.Euler(meshInfo.rotation.y * Mathf.Rad2Deg, -meshInfo.rotation.x * Mathf.Rad2Deg, meshInfo.rotation.z * Mathf.Rad2Deg);

            //filter.mesh.RecalculateBounds();

            //Graphics.DrawMesh(asset.mesh, asset.xform, sharedMaterial, 0);
        }
	}
}
