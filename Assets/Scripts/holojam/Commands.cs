﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommandToServer
{
    RESOLUTION_REQUEST = 0,
    STYLUS_RESET       = 1,
    SKETCHPAGE_CREATE  = 2,
    AVATAR_SYNC        = 3,
}

public enum CommandFromServer
{
    RESOLUTION_REQUEST = 0,
    STYLUS_RESET       = 1,
    SKETCHPAGE_CREATE  = 2,
    AVATAR_SYNC        = 3,
}
