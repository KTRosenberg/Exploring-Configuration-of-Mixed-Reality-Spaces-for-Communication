using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour {

    // change the text when necessary

    bool isVisible;

    public bool isDominant;

    public TMPro.TextMeshPro drawToggleText, one, two, move, draw;

    Transform[] children;
    //public Vector3 ButtonOne, ButtonTwo, PrimaryIndex, SecondaryIndex, PrimaryHand, SecondaryHand, PrimaryThumbstick, SecondaryThumbstick;
    //public Vector3 drawToggleEuler;
    //public float drawToggleScale;
    //public Vector3 drawTogglePos;
    //public GameObject drawToggle;
    // Use this for initialization
    void Start() {
        //drawToggleScale = Vector3.one;
        isVisible = true;
        //children = GameObject.find
    }

    // Update is called once per frame
    void Update() {

    }

    public void ToggleTooltip(){
        isVisible = !isVisible;
        if (isVisible) {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(true);
            }
        }
        else {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void SwitchDominantHand(bool isDom)
    {
        if(isDom != isDominant) {
            isDominant = isDom;
            if (isDominant) {
                one.text = "Select";
                two.text = "Create Board";
                move.text = "Move";
            }
            else {
                one.text = "Observe";
                two.text = "Help";
                move.text = "Empty";
            }
        }
    }
}
