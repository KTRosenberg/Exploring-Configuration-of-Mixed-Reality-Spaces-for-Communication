using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommandToServer
{
    RESOLUTION_REQUEST ,
    STYLUS_RESET ,
    SKETCHPAGE_CREATE ,
    AVATAR_SYNC ,
    SKETCHPAGE_SET ,
    INIT_COMBINE ,
    SELECT_CTOBJECT,
    DESELECT_CTOBJECT,
    AVATAR_LEAVE
}

public enum CommandFromServer
{
    RESOLUTION_REQUEST,
    STYLUS_RESET,
    SKETCHPAGE_CREATE,
    AVATAR_SYNC,
    SKETCHPAGE_SET ,
    INIT_COMBINE ,
    SELECT_CTOBJECT ,
    DESELECT_CTOBJECT,
    AVATAR_LEAVE
}