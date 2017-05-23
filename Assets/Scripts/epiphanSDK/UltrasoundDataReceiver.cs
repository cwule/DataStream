/*
 * DataReceiver.cs
 *
 * Receives depth and color data from the network
 * Requires CustomMessagesPointCloud.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// Receives the frame data messages
public class UltrasoundDataReceiver : Singleton<UltrasoundDataReceiver> {

    private RawImage img;

    public int MAX_PACKET_SIZE;

#if !UNITY_EDITOR && UNITY_METRO

    private int _width = 200;
    private int _height = 200;

    // Arrays to store received data
    private byte[] _UltraData;
    private int dataCount;

    //// Flag to indicate whether frame param has been received
    //private bool _generalReceived;

    // Current state of the rendering loop
    private enum State
    {
        WaitingForPacket1,
        WaitingForPacket2
    }

    // Broadcasted message must have an identifying ID
    private enum MsgID : byte
    {
        PACK1,
        PACK2
    }

    // Start waiting for general message
    State currentState = State.WaitingForPacket1;

    /// <summary>
    /// 
    /// </summary>
    void Start() {
        // Get the raw image to which the texture will be applied.
        img = GetComponent<RawImage>();

        _UltraData = new byte[_width * _height];
        dataCount = 0;
        FrameMessages.Instance.MessageHandlers[FrameMessages.EpiphanMessageID.StartID] = this.ReceiveData;
    }

    // Called when reading in Kinect data
    void ReceiveData(NetworkInMessage msg) {

        // 1) Read message ID type
        byte msgID = msg.ReadByte();

        int length = 0; // store message length

        if (msgID == (byte) MsgID.PACK1)
        {
            length = 200*200;
            for (int i = 0; i < length; i++)
            {
                _UltraData[dataCount++] = msg.ReadByte();
            }
            if (dataCount == ( _height * _width ))
            {
                byte[] GrayRGBA32;
                GrayRGBA32 = Grayscale2RGBA32(_UltraData, dataCount);
                TextureFormat texFormat = TextureFormat.RGBA32;
                int len = _height * _width;
                Texture2D iTexture = new Texture2D(_width, _height, texFormat, false);
                iTexture.LoadRawTextureData(GrayRGBA32);
                iTexture.Apply(false);
                img.texture = iTexture;
                dataCount = 0;
            } else {
                Debug.Log("Received wrong size packet");
            }
        }    
    }

    private const byte RMASK = 0xE0;
    private const byte GMASK = 0x1C;
    private const byte BMASK = 0x03;

    private byte[] Grayscale2RGBA32( byte[] gray, int len )
    {
        //dataOut = new IntPtr();
        byte[] dataOut = new byte[len * 4];
        for (int i = 0; i < len; i++)
        {
            byte currPix = gray[i];
            dataOut[4*i] = (byte) ((int)((RMASK&currPix) >> 5) * 255/7);
            dataOut[4*i+1] = (byte) ((int)((GMASK&currPix) >> 2)*255/7);
            dataOut[4*i+2] = (byte) ((int)(BMASK&currPix)*255/3);
            dataOut[4*i+3] = 255; // Set Alpha value to max
        }
        return dataOut;
    }
#endif
}