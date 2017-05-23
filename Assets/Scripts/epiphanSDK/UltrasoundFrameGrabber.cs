using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Epiphan.FrmGrab;

public class UltrasoundFrameGrabber : Singleton<UltrasoundFrameGrabber>
{

    private FrameGrabber _grabber;

#if UNITY_EDITOR || UNITY_STANDALONE

    // Use this for initialization
    void Start()
    {
        OpenGrabber();
    }

    unsafe bool OpenGrabber()
    {
        if (_grabber == null)
        {
            FrameGrabber grabber = null;
            try
            {
                grabber = new FrameGrabber();
                V2U_Property p;
                p.key = V2UPropertyKey.V2UKey_Version;
                grabber.GetProperty(&p);
            }
            catch (Exception x)
            {
                Debug.Log(x.Message);
            }
            SetGrabber(grabber);
        }
        return (_grabber != null);
    }

    void SetGrabber(FrameGrabber grabber)
    {
        _grabber = grabber;
        if (grabber != null)
        {
            Debug.Log(grabber.ToString());
        }
        else
        {
            Debug.Log("No frame grabber");
        }
    }

    public void GrabFrame(out byte[] frameOut, out UInt16 len, out bool success)
    {
        if (OpenGrabber())
        {
            VideoMode vm = _grabber.DetectVideoMode();
            if (vm != null)
            {
                if (vm.IsValid())
                {
                    Frame frame;
                    unsafe
                    {
                        V2URect crop = new V2URect();
                        crop.x = 0; crop.y = 0; crop.width = 400; crop.height = 400;
                        frame = _grabber.GrabFrame(FrameGrabber.V2U_GRABFRAME_FORMAT_RGB8, &crop);

                    }

                    int imgWidth;
                    int imgHeight;
                    byte[] newFrame;
                    success = frame.GetData(out newFrame, out imgWidth, out imgHeight, 2);
                    frameOut = newFrame;
                    len = (UInt16) (imgWidth * imgHeight);
                    return;
                }
                else
                {
                    Debug.Log("No signal detected");
                }
            }
            else
            {
                Debug.Log("Failed to detect video mode");
                SetGrabber(null);
            }
        }
        frameOut = new byte[1];
        success = false;
        len = 0;
    }
#endif
}
