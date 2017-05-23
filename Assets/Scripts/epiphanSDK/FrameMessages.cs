// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Sharing;
using System;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Test class for demonstrating how to send custom messages between clients.
/// </summary>
public class FrameMessages : Singleton<FrameMessages>
{

    /// <summary>
    /// Message enum containing our information bytes to share.
    /// The first message type has to start with UserMessageIDStart
    /// so as not to conflict with HoloToolkit internal messages.
    /// </summary>
    public enum EpiphanMessageID : byte
    {
        StartID = MessageID.UserMessageIDStart,
        Max
    }

    // Broadcasted message must have an identifying ID
    private enum MsgID : byte
    {
        PACK1,
        PACK2
    }

    public enum UserMessageChannels
    {
        Anchors = MessageChannel.UserMessageChannelStart
    }

    /// <summary>
    /// Cache the local user's ID to use when sending messages
    /// </summary>
    public long LocalUserID
    {
        get; set;
    }

    public delegate void MessageCallback(NetworkInMessage msg);
    private Dictionary<EpiphanMessageID, MessageCallback> messageHandlers = new Dictionary<EpiphanMessageID, MessageCallback>();
    public Dictionary<EpiphanMessageID, MessageCallback> MessageHandlers
    {
        get
        {
            return messageHandlers;
        }
    }

    /// <summary>
    /// Helper object that we use to route incoming message callbacks to the member
    /// functions of this class
    /// </summary>
    private NetworkConnectionAdapter connectionAdapter;

    /// <summary>
    /// Cache the connection object for the sharing service
    /// </summary>
    private NetworkConnection serverConnection;

    private void Start()
    {

        // SharingStage should be valid at this point, but we may not be connected.
        if (SharingStage.Instance.IsConnected)
        {
            Connected();
        }
        else
        {
            SharingStage.Instance.SharingManagerConnected += Connected;
        }
    }

    private void Connected(object sender = null, EventArgs e = null)
    {
        SharingStage.Instance.SharingManagerConnected -= Connected;
        InitializeMessageHandlers();
    }

    private void InitializeMessageHandlers()
    {
        SharingStage sharingStage = SharingStage.Instance;

        if (sharingStage == null)
        {
            Debug.Log("Cannot Initialize CustomMessages. No SharingStage instance found.");
            return;
        }

        serverConnection = sharingStage.Manager.GetServerConnection();
        if (serverConnection == null)
        {
            Debug.Log("Cannot initialize CustomMessages. Cannot get a server connection.");
            return;
        }

        connectionAdapter = new NetworkConnectionAdapter();
        connectionAdapter.MessageReceivedCallback += OnMessageReceived;

        // Cache the local user ID
        LocalUserID = SharingStage.Instance.Manager.GetLocalUser().GetID();

        for (byte index = (byte)EpiphanMessageID.StartID; index < (byte)EpiphanMessageID.Max; index++)
        {
            if (MessageHandlers.ContainsKey((EpiphanMessageID)index) == false)
            {
                MessageHandlers.Add((EpiphanMessageID)index, null);
            }

            serverConnection.AddListener(index, connectionAdapter);
        }
    }

    private NetworkOutMessage CreateMessage(byte messageType)
    {
        NetworkOutMessage msg = serverConnection.CreateMessage(messageType);
        msg.Write(messageType);
        // Add the local userID so that the remote clients know whose message they are receiving
        //msg.Write(LocalUserID);
        return msg;
    }
#if UNITY_EDITOR || UNITY_STANDALONE
    public void SendFrame()
    {
        // If we are connected to a session, broadcast our head info
        if (serverConnection != null && serverConnection.IsConnected())
        {

            // Create an outgoing network message to contain all the info we want to send
            NetworkOutMessage msg = CreateMessage((byte)EpiphanMessageID.StartID);
            //Debug.Log("Sending message type " + (byte)MsgID.PACK1);
            msg.Write((byte)MsgID.PACK1);

            byte[] oneFrame; UInt16 size;  bool success;
            // Grab one frame from the frame grabber
            UltrasoundFrameGrabber.Instance.GrabFrame( out oneFrame, out size, out success );
            AppendBuffer(msg, oneFrame, size);


            // Send the message as a broadcast, which will cause the server to forward it to all other users in the session.
            serverConnection.Broadcast(
                msg,
                MessagePriority.Immediate,
                MessageReliability.UnreliableSequenced,
                MessageChannel.Avatar);
        }
    }

    int counter = 0;
    private void Update()
    {
        if (counter++ % 2 == 0)
        {
            SendFrame();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SendFrame();
        }
    }

#endif
    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (serverConnection != null)
        {
            for (byte index = (byte)EpiphanMessageID.StartID; index < (byte)EpiphanMessageID.Max; index++)
            {
                serverConnection.RemoveListener(index, connectionAdapter);
            }
            connectionAdapter.MessageReceivedCallback -= OnMessageReceived;
        }
    }

    private void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
    {
        byte messageType = msg.ReadByte();
        MessageCallback messageHandler = MessageHandlers[(EpiphanMessageID)messageType];
        if (messageHandler != null)
        {
            messageHandler(msg);
        }
    }

    #region HelperFunctionsForWriting
#if UNITY_EDITOR || UNITY_STANDALONE
    private void AppendBuffer(NetworkOutMessage msg, byte[] frame_buffer, int buf_size)
    {
        unsafe
        {
            //byte* buf = (byte*)frame_buffer;
            for (int i = 0; i < buf_size; i++)
            {
                msg.Write(frame_buffer[i]);
            }
        }
    }
#endif

    private void AppendVector3(NetworkOutMessage msg, Vector3 vector)
    {
        msg.Write(vector.x);
        msg.Write(vector.y);
        msg.Write(vector.z);
    }

    private void AppendQuaternion(NetworkOutMessage msg, Quaternion rotation)
    {
        msg.Write(rotation.x);
        msg.Write(rotation.y);
        msg.Write(rotation.z);
        msg.Write(rotation.w);
    }

    #endregion

    #region HelperFunctionsForReading

    public Vector3 ReadVector3(NetworkInMessage msg)
    {
        return new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
    }

    public Quaternion ReadQuaternion(NetworkInMessage msg)
    {
        return new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
    }

    #endregion
}