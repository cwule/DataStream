using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Epiphan.FrmGrab;

public class UltraSoundFrameGrabber : MonoBehaviour
{

    [Tooltip("The canvas where the streamed image will be displayed.")]
    public GameObject rawImage;
    private RawImage img;
    private Texture2D texture;
    private FrameGrabber _grabber;

    // Use this for initialization
    void Start()
    {
        img = rawImage.GetComponent<RawImage>();
        texture = new Texture2D(300, 400);
        bool success = OpenGrabber();
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

    private void GrabFrame()
    {
        if (OpenGrabber())
        {
            VideoMode vm = _grabber.DetectVideoMode();
            if (vm != null)
            {
                if (vm.IsValid())
                {
                    Frame frame = _grabber.GrabFrame();

                    /* The following part should be done on the HL side */
                    RemoteMeshTarget.Is
                    if (frame != null)
                    {
                        texture = frame.GetTexture();
                        if (texture != null)
                        {
                            texture.Apply(false);
                            img.texture = texture;
                        }
                    }
                    else
                    {
                        Debug.Log("Capture failed");
                        SetGrabber(null);
                    }
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
    }

    // Update is called once per frame
    void Update()
    {
        GrabFrame();
        if (Input.GetKeyDown(KeyCode.G))
        {
            GrabFrame();
        }
    }
}
