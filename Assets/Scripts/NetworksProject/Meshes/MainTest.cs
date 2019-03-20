
using System.Collections.Generic;
using UnityEngine;

public class MainTest : MonoBehaviour {

    public Material mat;
    private Material sharedMaterial;
    //public MeshContent.MeshData data;
    Vector3 originalPosition;


    MeshFilter filter;
    MeshRenderer rend;

	void Start () {
        sharedMaterial = new Material(mat);

        filter = gameObject.AddComponent<MeshFilter>();
        //rend = gameObject.AddComponent<MeshRenderer>();
        //rend.sharedMaterial = sharedMaterial;

        originalPosition = transform.position;
	}

    float prevX = 0.0f;
    float prevY = 0.0f;
    float prevZ = 0.0f;

    float zChangeCounter = 0;

    bool DelayZMovement(MeshContent.MeshData meshInfo)
    {
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
                return true;
            }
        }
        else {

            if (meshInfo.position.z != prevZ) {
                prevZ = meshInfo.position.z;

                if (meshInfo.position.x == prevX &&
                    meshInfo.position.y == prevY) {

                    zChangeCounter = 3;

                    return true;
                }
            }
        }

        return false;
    }


    // TODO reuse game objects
    Queue<GameObject> freeGameObjects = new Queue<GameObject>();
    bool GetOrDeferMeshGameObject(Queue<int> q, int key, out MeshContentGO meshGO)
    {
        //Debug.Log("Checking key: " + key);
        if (MeshContent.idToMeshGOMap.TryGetValue(key, out meshGO)) {
            if (DelayZMovement(meshGO.meshData)) {
                return false;
            }
        }
        else {
            // need to create a new game object

            Debug.Log("getting new game object");


            GameObject go = new GameObject();
            MeshContentGO mcGO = go.AddComponent<MeshContentGO>();

            Debug.Log("setting mesh data");
            mcGO.meshData = MeshContent.idToMeshMap[key];
            Debug.Log(mcGO.meshData);
            Debug.Log("mesh data id: [" + mcGO.meshData.ID + ":" + mcGO.meshData.subID + "]");
            go.name = "M:" + mcGO.meshData.ID + ":" + mcGO.meshData.subID;
            mcGO.meshRenderer.sharedMaterial = sharedMaterial;

            MeshContent.idToMeshGOMap.Add(key, mcGO);

            meshGO = mcGO;
        }

        return true;
    }

    // all objects that need to be updated this frame
    Queue<int> q = MeshContent.needToUpdateQ;
    // temp I'm just going to destroy the game objects every frame ...
    // store all objects that were actually updated
    HashSet<int> qUpdated = new HashSet<int>();

    void ApplyBoardToMesh(MeshContentGO go)
    {
        Transform xform = go.transform;
        Vector3 position = go.meshData.position;

        List<ChalktalkBoard> boards = ChalktalkBoard.GetBoard(go.meshData.boardID);
        if (boards.Count == 0) {
            Debug.Log("board list length is 0, boardID: " + go.meshData.boardID + " is invalid then");
            return;
        }

        Transform refBoard = boards[0].transform;
        float boardScale = GlobalToggleIns.GetInstance().ChalktalkBoardScale;
        //xform.localPosition = new Vector3(
        //    position.x / refBoard.localScale.x * boardScale,
        //    position.y / refBoard.localScale.y * boardScale,
        //    position.z / refBoard.localScale.z * boardScale
        //);
        //xform.parent = refBoard;

        //Vector3 scaling = xform.localScale;
        //scaling.x /= refBoard.localScale.x;
        //scaling.y /= refBoard.localScale.y;
        //scaling.z /= refBoard.localScale.z;

        //xform.localScale = scaling;

        //transform.localRotation = transform.rotation;
        //transform.rotation = Quaternion.identity;


        //xform.position += refBoard.position;
        //xform.rotation *= refBoard.rotation;
        //xform.localScale *= boardScale;

        
        
        go.transform.parent = refBoard;
        go.transform.localPosition = new Vector3( go.pos.x / refBoard.localScale.x, go.pos.y / refBoard.localScale.y, go.pos.z / refBoard.localScale.z);
        go.transform.localRotation = go.rot;
        go.transform.localScale = new Vector3(go.scale.x/ refBoard.localScale.x, go.scale.y / refBoard.localScale.y, go.scale.z / refBoard.localScale.z);
    }

    void UpdateMeshGameObjects()
    {
        int count = q.Count;

        // check all objects' keys for update
        for (int i = 0; i < count; i += 1) {
            int key = q.Dequeue();

            //Debug.Log("updating key=[" + (key & 0x0000FFFF) + ":" + (key & 0xFFFF0000) + "]");

            MeshContentGO go;
            if (!GetOrDeferMeshGameObject(q, key, out go)) {
                //q.Enqueue(key);
                continue;
            }

            go.UpdateMeshDataAll();
            ApplyBoardToMesh(go);

            qUpdated.Add(key);
        }

        q.Clear();
    }

    void LateUpdate()
    {
        UpdateMeshGameObjects();



        // TODO no deletion yet
        return;

        // TODO do NOT destroy game objects per frame... but need a reliable way for
        // Chalktalk to tell the client that a mesh sketch has been removed (use commands?)
        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, MeshContentGO> entry in MeshContent.idToMeshGOMap) {
            int key = Utility.ShortPairToInt(entry.Value.meshData.ID, entry.Value.meshData.subID);
            if (!qUpdated.Contains(key)) {
                GameObject.Destroy(entry.Value.gameObject);
                toRemove.Add(key);
            }
        }
        for (int i = 0; i < toRemove.Count; i += 1) {
            MeshContent.idToMeshGOMap.Remove(toRemove[i]);
        }

        q.Clear();
        qUpdated.Clear();
    }
}
