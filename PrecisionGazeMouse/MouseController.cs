/*
 * Copyright 2019 Rishi Kapadia
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * http://www.apache.org/licenses/LICENSE-2.0|http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 * This is a derivative work of:
 * https://github.com/PrecisionGazeMouse/PrecisionGazeMouse
 * 
 * Modifications are stated throughout the file.
 */
 
using System;
using System.Drawing;
using PrecisionGazeMouse.PrecisionPointers;
using PrecisionGazeMouse.WarpPointers;
using System.Windows.Forms;
using System.Runtime.InteropServices;

/*
 * Wrapper class which will have the useful functionality implemented by
 * WarpPointer class so that it can get eye gaze data from the eye-tracker.
 */

namespace PrecisionGazeMouse
{
    public class MouseController
    {
        WarpPointer warp;
        PrecisionPointer prec;
        Point finalPoint;
        DateTime pauseTime;
        Point lastCursorPosition;
        PrecisionGazeMouseForm form;
        bool updatedAtLeastOnce;

        public enum Mode
        {
            EYEX_AND_TRACKIR,
            EYEX_AND_SMARTNAV,
            EYEX_ONLY,
            TRACKIR_ONLY
        };
        Mode mode;

        public enum Movement
        {
            CONTINUOUS,
            HOTKEY
        };
        Movement movement;
        bool hotKeyDown = false;
        bool dragging = false;
        DateTime? timeSinceKeyUp;
        Point? lastClick;

        public enum TrackingState
        {
            STARTING,
            PAUSED,
            RUNNING,
            ERROR
        };
        public TrackingState state;
        public TrackingState previousState = TrackingState.STARTING;

        public MouseController(PrecisionGazeMouseForm form)
        {
            this.form = form;
        }

        public void setMovement(Movement movement)
        {
            this.movement = movement;
        }

        public void setMode(Mode mode)
        {
            if (warp != null)
                warp.Dispose();
            if (prec != null)
                prec.Dispose();

            this.mode = mode;
            switch(mode)
            {
                case Mode.EYEX_AND_TRACKIR:
                    warp = new EyeXWarpPointer(ScreenPixelHelper.ConvertMmToPixels(25));
                    prec = new TrackIRPrecisionPointer(ScreenPixelHelper.ConvertMmToPixels(25), 0.3);
                    break;
                case Mode.EYEX_AND_SMARTNAV:
                    warp = new EyeXWarpPointer(ScreenPixelHelper.ConvertMmToPixels(1));
                    prec = new NoPrecisionPointer();
                    state = TrackingState.RUNNING;
                    break;
                case Mode.EYEX_ONLY:
                    warp = new EyeXWarpPointer(ScreenPixelHelper.ConvertMmToPixels(5));
                    prec = new NoPrecisionPointer();
                    break;
            }

            Logger.WriteVar(nameof(mode), mode);

            if (!warp.IsStarted())
                state = TrackingState.ERROR;

            if (!prec.IsStarted())
                state = TrackingState.ERROR;
        }
        
        public void HotKeyDown()
        {
            if (movement != Movement.HOTKEY || state == TrackingState.ERROR || state == TrackingState.PAUSED)
                return;

            if (!hotKeyDown)
            {
                if (!dragging && timeSinceKeyUp != null && System.DateTime.Now.Subtract(timeSinceKeyUp.Value).TotalMilliseconds < 250)
                {
                    // it's a drag so click down and hold
                    int X = lastClick.Value.X;
                    int Y = lastClick.Value.Y;
                    User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
                    dragging = true;
                }
                else if(!dragging)
                {
                    warp.RefreshTracking();
                    state = TrackingState.STARTING;
                    updatedAtLeastOnce = false;
                }
            }
            hotKeyDown = true;
        }

        public void HotKeyUp()
        {
            if (movement != Movement.HOTKEY || state == TrackingState.ERROR || state == TrackingState.PAUSED)
                return;

            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;

            if (dragging)
            {
                User32.mouse_event(User32.MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                dragging = false;
            }
            else
            {
                if (timeSinceKeyUp != null && System.DateTime.Now.Subtract(timeSinceKeyUp.Value).TotalMilliseconds < 500)
                {
                    // it's a double click so use the original click position
                    X = lastClick.Value.X;
                    Y = lastClick.Value.Y;
                }
                User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN | User32.MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                lastClick = Cursor.Position;
            }

            timeSinceKeyUp = System.DateTime.Now;
            hotKeyDown = false;
        }

        public WarpPointer WarpPointer
        {
            get { return warp; }
        }

        public PrecisionPointer PrecisionPointer
        {
            get { return prec; }
        }

        public Point GetFinalPoint()
        {
            return finalPoint;
        }

        public String GetTrackingStatus()
        {
            switch (state)
            {
                case TrackingState.STARTING:
                    return "Starting";
                case TrackingState.RUNNING:
                    return "Running";
                case TrackingState.PAUSED:
                    return "Paused";
                case TrackingState.ERROR:
                    if (!warp.IsStarted())
                        return "No EyeX connection";
                    if (!prec.IsStarted())
                        return "No TrackIR connection";
                    return "Error";
            }
            return "";
        }

        //returns true if a new update is made, false otherwise
        public bool UpdateMouse(Point currentPoint)
        {
            bool ret = false;
            switch (state)
            {
                case TrackingState.STARTING:
                    if (warp.IsWarpReady())
                    {
                        state = TrackingState.RUNNING;
                        finalPoint = currentPoint;
                    }
                    break;
                case TrackingState.RUNNING:
                    if(movement == Movement.HOTKEY)
                    {
                        if (updatedAtLeastOnce && !hotKeyDown)
                            break;
                    }
                    Point warpPoint = warp.GetNextPoint(currentPoint);
                    if (mode == Mode.EYEX_AND_SMARTNAV || mode == Mode.TRACKIR_ONLY) //warps in a split second (EyeX is 60 Hz)
                    {
                        if (warpPoint != finalPoint)
                        {
                            finalPoint = warpPoint;
                            form.SetMousePosition(finalPoint);
                            ret = true;
                        }
                    }
                    else //eyex only, this one takes about a 3 second dwell for it to warp
                        //as well eyeX and TrackIR which is with warp + head movement
                    {
                        if (PrecisionGazeMouseForm.MousePosition != finalPoint)
                        {
                            state = TrackingState.PAUSED;
                            pauseTime = System.DateTime.Now;
                        }
                        finalPoint = prec.GetNextPoint(warpPoint);
                        form.SetMousePosition(finalPoint);
                        ret = true;
                    }
                    updatedAtLeastOnce = true;
                    break;
                case TrackingState.PAUSED:
                    // Keep pausing if the user is still moving the mouse
                    if (lastCursorPosition != currentPoint)
                    {
                        lastCursorPosition = currentPoint;
                        pauseTime = System.DateTime.Now;
                    }
                    if (System.DateTime.Now.CompareTo(pauseTime.AddSeconds(1)) > 0)
                        state = TrackingState.STARTING;
                    break;
                case TrackingState.ERROR:
                    if (warp.IsStarted() && prec.IsStarted())
                        state = TrackingState.STARTING;
                    break;
            }

            if(previousState != state)
            {
                Logger.WriteVar(nameof(state), state);
                previousState = state;
            }
            return ret;
        }

        private Point getScreenCenter()
        {
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            return new Point(screenSize.Width / 2, screenSize.Height / 2);
        }

        public Point limitToScreenBounds(Point p)
        {
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            int margin = 5;

            if (p.X < margin)
                p.X = margin;
            if (p.Y < margin)
                p.Y = margin;
            if (p.X >= screenSize.Width - margin)
                p.X = screenSize.Width - margin;
            if (p.Y >= screenSize.Height - margin - 5)
                p.Y = screenSize.Height - margin - 5;

            return p;
        }
    }
}
