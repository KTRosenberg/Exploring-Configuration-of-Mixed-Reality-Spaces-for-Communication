using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

public class MSGSender : Holojam.Tools.SynchronizableTrackable
{

    [SerializeField] string label = "MSGSender";
    [SerializeField] string scope = "";

    [SerializeField] bool host = false;
    [SerializeField] bool autoHost = false;

    // Point the property overrides to the public inspector fields

    public override string Label { get { return label; } }
    public override string Scope { get { return scope; } }
    public override bool Host { get { return host; } }
    public override bool AutoHost { get { return autoHost; } }

    //string msgToSend;
    byte[] bMSG;
    void encodeCommand(int commandNumber, int[] parameters)// could be byte array for parameters for future
    {
        // #0 for resolution
        // #1 for reset ownership
        // #2 for creating sketchPage
        byte[] bCN = BitConverter.GetBytes(commandNumber);
        byte[] bPN = BitConverter.GetBytes(parameters.Length);

        bMSG = new byte[bCN.Length + bPN.Length + bPN.Length * parameters.Length];
        System.Buffer.BlockCopy(bCN, 0, bMSG, 0, bCN.Length);
        System.Buffer.BlockCopy(bPN, 0, bMSG, bCN.Length, bPN.Length);

        for(int i = 0; i < parameters.Length; i++)
        {
            byte[] bP = BitConverter.GetBytes(parameters[i]);
            System.Buffer.BlockCopy(bP, 0, bMSG, bCN.Length + bPN.Length + i*bP.Length, bP.Length);
        }

    }

    // Override Sync()
    protected override void Sync()
    {
        if (Sending)
        {

        }
        else
        {
            
        }
    }

    public void Send(string msg)
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, msg.Length * sizeof(char), false
        );
        data.bytes = Encoding.ASCII.GetBytes(msg);
        host = true;
    }

    public void Send(int cmd, int[] parameters)
    {
        encodeCommand(cmd, parameters);
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, bMSG.Length, false
        );
        data.bytes = bMSG;
        host = true;
    }

    protected override void Update()
    {
        if (autoHost) host = Sending; // Lock host flag
        //base.Update();
        host = false;
    }

    public override void ResetData()
    {
        data = new Holojam.Network.Flake(
          0, 0, 0, 0, 0, false
        );
    }
}
