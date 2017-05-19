//
// $Id: MainForm.cs 9987 2010-06-09 12:03:18Z monich $
//
// Copyright (C) 2008-2009 Epiphan Systems Inc. All rights reserved.
//

using System;
using System.Text;
using System.IO;
using UnityEngine;

using Epiphan.FrmGrab;

public partial class MainForm
{
    FrameGrabber _grabber;

    public MainForm()
    {
        OpenGrabber();
    }

    unsafe bool OpenGrabber()
    {
        if (_grabber == null) {
            FrameGrabber grabber = null;
            //string wanted_path = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.));
            //Debug.WriteLine("Current path is " + wanted_path);
            try {
                grabber = new FrameGrabber();
                V2U_Property p;
                p.key = V2UPropertyKey.V2UKey_Version;
                grabber.GetProperty(&p);
            } catch (Exception x) {
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
        if (OpenGrabber()) {
            VideoMode vm = _grabber.DetectVideoMode();
            if (vm != null) {
                if (vm.IsValid()) {
                    Frame frame = _grabber.GrabFrame(); // Default GrabFrame(V2U_GRABFRAME_FORMAT_BGR24, null);
                    if (frame != null) {
                        //Bitmap bitmap = frame.GetBitmap();
                        //if (bitmap != null) {
                        //    Debug.Log("Frame captured (" + vm + ")");
                        //    PictureBox.Image = bitmap;
                        //} else {
                        //    TextOutputAdd("Capture format error");
                        //    SetGrabber(null);
                        //}
                    } else {
                        Debug.Log("Capture failed");
                        SetGrabber(null);
                    }
                } else {
                    Debug.Log("No signal detected");
                }
            } else {
                Debug.Log("Failed to detect video mode");
                SetGrabber(null);
            }
        }
    }

    private void CaptureFrame_Click(object sender, EventArgs e)
    {
        GrabFrame();
    }

    //private void ConfigureButton_Click(object sender, EventArgs e)
    //{
    //    if (OpenGrabber()) {
    //        GrabParams gp = _grabber.GetGrabParams();
    //        CaptureSettings dialog = new CaptureSettings(gp);
    //        if (dialog.ShowDialog(this) == DialogResult.OK) {
    //            _grabber.SetGrabParams(gp);
    //            GrabFrame();
    //        }
    //        dialog.Dispose();
    //    }
    //}
}
