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
    TMP_BOARD_ON ,
    TMP_BOARD_OFF ,
}

public enum CommandFromServer
{
    RESOLUTION_REQUEST,
    STYLUS_RESET,
    SKETCHPAGE_CREATE,
    AVATAR_SYNC,
    SKETCHPAGE_SET ,
    INIT_COMBINE ,
    TMP_BOARD_ON ,
    TMP_BOARD_OFF ,
}