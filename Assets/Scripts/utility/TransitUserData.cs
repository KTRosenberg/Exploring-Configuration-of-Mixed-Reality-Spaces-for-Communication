using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransitUserData {
    public Vector3    position;
    public Quaternion rotation;
    public Vector3    forward;
    public byte       flags; // ( & (1 << 0) should mean has control, & (1 << 1) should mean is currently observing someone)


    public bool UserHasControl()
    {
        return (flags & 1) != 0;
    }

    public bool UserIsObserving()
    {
        return (flags & (1 << 1)) != 0;
    }

    public void SetHasControl()
    {
        flags |= 1;
    }

    public void SetIsObserving()
    {
        flags |= (1 << 1);
    }

    public override string ToString()
    {
        return "[" + 
            position.ToString("F3") + " : " + 
            rotation.ToString("F3") + ":" + 
            forward.ToString() + ":" + 
            UserHasControl() + ":" + 
            UserIsObserving() + 
            "]";
    }
}
