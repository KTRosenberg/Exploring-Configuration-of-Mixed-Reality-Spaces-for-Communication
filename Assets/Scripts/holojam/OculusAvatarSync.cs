using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OculusAvatarSync : Holojam.Tools.SynchronizableTrackable
{
    public string label = "Avatar";
    [SerializeField] string scope = "";

    //[SerializeField] bool host = true;
    [SerializeField] bool autoHost = false;

    public bool isLocal; // if it is local, packet the information and send, if it is remote, receive and decode the information
    public OvrAvatar ovrAvatar;
    private List<byte> latestPosture = new List<byte>();
    private int localSequence;

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return isLocal; } }
    public override bool AutoHost { get { return autoHost; } }

    // Override Sync()
    protected override void Sync()
    {
        if(label == "avatar")
            label = GlobalToggleIns.GetInstance().username + "avatar";

        if (isLocal)
        {

        }
        if(!isLocal && Tracked)
        {
            DeserializeAndQueuePacketData(data.bytes);
        }
    }

    protected override void Update()
    {
        //if (autoHost) host = isLocal; // Lock host flag
        base.Update();
    }

    public override void ResetData()
    {
        label = GlobalToggleIns.GetInstance().username + "avatar";
        ovrAvatar = GetComponent<OvrAvatar>();
        if (isLocal)
        {
            ovrAvatar.RecordPackets = true;
            ovrAvatar.PacketRecorded += OnLocalAvatarPacketRecorded;
        }        
    }

    public void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
    {
        using (MemoryStream outputStream = new MemoryStream())
        {
            BinaryWriter writer = new BinaryWriter(outputStream);

            var size = Oculus.Avatar.CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
            byte[] avatardata = new byte[(int)size];
            Oculus.Avatar.CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, size, avatardata);
            latestPosture.Clear();
            latestPosture.AddRange(avatardata);
            //Debug.LogWarning("send seq: " + localSequence);
            //Debug.LogWarning("send avatar size: " + size + "\t" + (int)size);
            //Debug.LogWarning("send avatardata: " + BitConverter.ToString(avatardata));
            writer.Write(localSequence++);
            //writer.Write(System.UInt64.Parse(ovrAvatar.oculusUserID));
            writer.Write((int)size);
            writer.Write(avatardata);

            // here we only send current outputStream.ToArray()
            int sendSize = outputStream.ToArray().Length;
            Debug.LogWarning("send data size: " + sendSize);
            data = new Holojam.Network.Flake(0, 0, 0, 0, sendSize, false);
            outputStream.ToArray().CopyTo(data.bytes, 0);
        }
    }

    private void DeserializeAndQueuePacketData(byte[] avatardata)
    {
        if (avatardata.Length < 4)
        {
            Debug.LogWarning("avatardata length < 4");
            return;
        }

        using (MemoryStream inputStream = new MemoryStream(avatardata))
        {
            BinaryReader reader = new BinaryReader(inputStream);
            int remoteSequence = reader.ReadInt32();
            //ulong remoteAvatarId = (ulong)reader.ReadUInt64();
            int size = reader.ReadInt32();
            byte[] sdkData = reader.ReadBytes(size);
            System.IntPtr packet = Oculus.Avatar.CAPI.ovrAvatarPacket_Read((System.UInt32)size, sdkData);
            //Debug.LogWarning("recv seq: " + remoteSequence);
            //Debug.LogWarning("recv avatar size: " + size);
            //Debug.LogWarning("recv avatardata: " + BitConverter.ToString(sdkData));
            
            ovrAvatar.GetComponent<OvrAvatarRemoteDriver>().QueuePacket(remoteSequence, new OvrAvatarPacket { ovrNativePacket = packet });
            //this.GetComponent<SpacetimeAvatar>().DriveParallelOrGhostAvatarPosture(remoteSequence, sdkData);
        }
    }
}
