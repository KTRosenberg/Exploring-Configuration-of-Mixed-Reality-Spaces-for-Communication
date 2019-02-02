using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommandToServer
{
    RESOLUTION_REQUEST = 0,
    STYLUS_RESET = 1,
    SKETCHPAGE_CREATE = 2,
    AVATAR_SYNC = 3,
    INITDATA_GET = 5
}

public enum CommandFromServer
{
    RESOLUTION_REQUEST = 0,
    STYLUS_RESET = 1,
    SKETCHPAGE_CREATE = 2,
    AVATAR_SYNC = 3,
    SKETCHPAGE_SET = 4,
    INITDATA_GET = 5,
    TMP_BOARD_ON = 6,
    TMP_BOARD_OFF = 7,
}