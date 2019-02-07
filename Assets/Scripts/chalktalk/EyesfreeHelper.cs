﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesfreeHelper : MonoBehaviour {

    public Transform activeBindingbox;
    public Transform activeCursor;
    public Transform dupBindingbox;
    public Transform dupCursor;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // render dupCursor the same pos referring to activeCursor in activeBoard
        dupCursor.position = dupBindingbox.TransformPoint(activeBindingbox.InverseTransformPoint(activeCursor.position));
    }
}