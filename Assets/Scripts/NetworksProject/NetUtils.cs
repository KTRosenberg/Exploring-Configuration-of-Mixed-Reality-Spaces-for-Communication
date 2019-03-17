using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetUtils
{
    public struct PayloadActionsSend { 
        public uint objID;
    }
    public struct PayloadActionsRecv {
        public uint objID;
        public byte actionAccepted;
    }

    public struct PayloadModifyObjectSend {
        public uint objID;
        public uint actionType;

        public int modifiedTriangleIndex;
        public Vector3 newPosition;
    }
    public struct PayloadModifyObjectRecv {
        public uint objID;
    }

    public struct PayloadSelectObjectSend {
        public uint objID;
    }
    public struct PayloadSelectObjectRecv {
        public byte actionAccepted;
        public uint objID;
    }

    public struct PayloadDeselectObjectSend {
        public uint objID;
    }
    public struct PayloadDeselectObjectRecv {
        public uint objID;
    }

    public struct PayloadCreateObjectSend {
        public uint objID;
    }
    public struct PayloadCreateObjectRecv {
        public byte actionAccepted;
        public uint objID;
    }


    public struct PacketActionsSend {
        public uint seqID;
        public ulong usrID;
        public float time;

        public uint ackBitfield; // redundant acks

        public uint payloadCount;
        public uint actionType;
        public object[] payloads;
    }

    public struct PacketActionsRecv {
        public uint seqID;
        public ulong usrID;
        public float time;

        public uint ackBitfield; // redundant acks

        public uint payloadCount;
        public uint actionType;
        public object[] payloads;
    }
}
