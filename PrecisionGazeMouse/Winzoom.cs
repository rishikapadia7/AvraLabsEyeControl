/*
 * Copyright 2019 Rishi Kapadia
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * http://www.apache.org/licenses/LICENSE-2.0|http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

/*
 * The logic behind showing and configuring the zoombox form.
 */

namespace PrecisionGazeMouse
{
    public class Winzoom
    {
        public enum ZOOMBOX_RECT_T
        {
            SRC,
            ACTIVE,
            PADDED
        };

        private bool active; //whether the zoom box is currently being displayed on the screen or not
        public bool enabled; //whether the zoombox feature is enabled or not
        public Rectangle activeRect, activeRectPadded, activeRectSrc;
        public Point activeRectCenter, activeRectSrcCenter;
        float zoomPct, activeRectPctScreen, activeRectPaddedPctScreen, aciveRectSrcPctScreen;

        WinzoomForm wzf;
        public WinzoomUnderlayForm wzuf;

        WarpPointers.WarpPointer warpPointer;
        CalibrationAdjuster calibrationAdjuster;

        public Winzoom(WarpPointers.WarpPointer warpPointer, CalibrationAdjuster calibrationAdjuster)
        {
            active = false;
            enabled = true;
            activeRect = new Rectangle(0, 0, 0, 0);
            activeRectPadded = new Rectangle(0, 0, 0, 0);
            activeRectSrc = new Rectangle(0, 0, 0, 0);
            activeRectCenter = new Point(0, 0);
            activeRectSrcCenter = new Point(0, 0);

            this.warpPointer = warpPointer;
            this.calibrationAdjuster = calibrationAdjuster;
        }

        public bool Active()
        {
            return enabled && active;
        }

        //Input: c is the proposed center, w is width of rectangle, h is the height
        //Output: returns a new center point so that the entire rectangle fits the screen
        public Point CalculateRectCenterToFitScreen(Point c, float w, float h)
        {
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            Point r = new Point(c.X, c.Y);
            
            if (c.X - w/2 < 0)
                //We need to shift to right
                r.X = c.X - (c.X - Convert.ToInt32(Math.Round(w / 2)));
            if (c.X + w / 2 > screenSize.Width)
                //shift to the left
                r.X = c.X - (c.X + Convert.ToInt32(Math.Round(w / 2)) - screenSize.Width);
            if (c.Y - h / 2 < 0)
                //We need to shift down
                r.Y = c.Y - (c.Y - Convert.ToInt32(Math.Round(h / 2)));
            if (c.Y + h / 2 > screenSize.Height)
                //shift up
                r.Y = c.Y - (c.Y + Convert.ToInt32(Math.Round(h / 2)) - screenSize.Height);
                
            return r;
        }

        public void Show(Point p, float addedMagnification = 0)
        {
            if (active) return;
            Logger.WriteEvent();
            active = true;

            
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();

            p = BoundPointToRect(p, screenSize, 5);

            zoomPct = ConfigManager.zoomWindowMagnificationPct + addedMagnification;
            activeRectPctScreen = ConfigManager.zoomWindowPctScreen;
            activeRectPaddedPctScreen = activeRectPctScreen + 15;
            aciveRectSrcPctScreen = (float) activeRectPctScreen * 100 / (float)zoomPct;

            //Calculate the size and position of the source rectangle
            float w = (float)screenSize.Width * aciveRectSrcPctScreen / 100;
            float h = (float)screenSize.Height * aciveRectSrcPctScreen / 100;
            //Check based on the height, width, and p if we need to shift to fit on screen
            activeRectSrcCenter = CalculateRectCenterToFitScreen(p, w, h);
            int xScreen = activeRectSrcCenter.X - Convert.ToInt32(w/2);
            int yScreen = activeRectSrcCenter.Y - Convert.ToInt32(h / 2);
            activeRectSrc = new Rectangle(xScreen, yScreen, Convert.ToInt32(w), Convert.ToInt32(h));


            float activeRectWidth = (float)screenSize.Width * activeRectPctScreen / 100;
            float activeRectHeight = (float)screenSize.Height * activeRectPctScreen / 100;
            activeRectCenter = CalculateRectCenterToFitScreen(p, activeRectWidth, activeRectHeight);
            xScreen = activeRectCenter.X - Convert.ToInt32((activeRectWidth / 2));
            yScreen = activeRectCenter.Y - Convert.ToInt32((activeRectHeight / 2));
            activeRect = new Rectangle(xScreen, yScreen, Convert.ToInt32(activeRectWidth), Convert.ToInt32(activeRectHeight));

            //Calculate the padded activeRect so that within this padding the zoombox does not cancel when user looks away
            float activeRectWidthPadded = screenSize.Width * activeRectPaddedPctScreen / 100;
            float activeRectHeightPadded = screenSize.Height * activeRectPaddedPctScreen / 100;
            xScreen = activeRectCenter.X - Convert.ToInt32((activeRectWidthPadded / 2));
            yScreen = activeRectCenter.Y - Convert.ToInt32((activeRectHeightPadded / 2));
            activeRectPadded = new Rectangle(xScreen,yScreen ,Convert.ToInt32(activeRectWidthPadded), Convert.ToInt32(activeRectHeightPadded));

            //Allows drawing cursor icons underneath the WinzoomForm.
            wzuf = new WinzoomUnderlayForm(warpPointer, calibrationAdjuster, this);
            wzuf.StartPosition = FormStartPosition.Manual;
            wzuf.WindowState = FormWindowState.Maximized;
            wzuf.AllowTransparency = true;
            wzuf.TransparencyKey = wzuf.BackColor;
            wzuf.TopMost = true;
            wzuf.Show();

            //zbf.BackColor = Color.FromArgb(216,249,107); //a very unusual light green color
            wzf = new WinzoomForm(this);
            wzf.magnification = (float) zoomPct / 100;
            wzf.mag.magnification = (float)zoomPct / 100;
            wzf.Width = Convert.ToInt32(activeRectWidth);
            wzf.Height = Convert.ToInt32(activeRectHeight);
            wzf.StartPosition = FormStartPosition.Manual;
            wzf.Location = new Point(activeRect.X, activeRect.Y);
            wzf.mag.SetSourceRect(activeRectSrc);
            wzf.mag.SetActiveRect(activeRect);
            wzf.Show();
        }

        public void Update(Point p, float addedMagnification = 0)
        {
            if (!active) return;
            
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();

            p = BoundPointToRect(p, screenSize, 5);

            zoomPct = ConfigManager.zoomWindowMagnificationPct + addedMagnification;
            activeRectPctScreen = ConfigManager.zoomWindowPctScreen;
            activeRectPaddedPctScreen = activeRectPctScreen + 15;
            aciveRectSrcPctScreen = (float)activeRectPctScreen * 100 / (float)zoomPct;

            //Calculate the size and position of the source rectangle
            float w = (float)screenSize.Width * aciveRectSrcPctScreen / 100;
            float h = (float)screenSize.Height * aciveRectSrcPctScreen / 100;
            //Check based on the height, width, and p if we need to shift to fit on screen
            activeRectSrcCenter = CalculateRectCenterToFitScreen(p, w, h);
            int xScreen = activeRectSrcCenter.X - Convert.ToInt32(w / 2);
            int yScreen = activeRectSrcCenter.Y - Convert.ToInt32(h / 2);
            activeRectSrc = new Rectangle(xScreen, yScreen, Convert.ToInt32(w), Convert.ToInt32(h));


            float activeRectWidth = (float)screenSize.Width * activeRectPctScreen / 100;
            float activeRectHeight = (float)screenSize.Height * activeRectPctScreen / 100;
            activeRectCenter = CalculateRectCenterToFitScreen(p, activeRectWidth, activeRectHeight);
            xScreen = activeRectCenter.X - Convert.ToInt32((activeRectWidth / 2));
            yScreen = activeRectCenter.Y - Convert.ToInt32((activeRectHeight / 2));
            activeRect = new Rectangle(xScreen, yScreen, Convert.ToInt32(activeRectWidth), Convert.ToInt32(activeRectHeight));

            //Calculate the padded activeRect so that within this padding the zoombox does not cancel when user looks away
            float activeRectWidthPadded = screenSize.Width * activeRectPaddedPctScreen / 100;
            float activeRectHeightPadded = screenSize.Height * activeRectPaddedPctScreen / 100;
            xScreen = activeRectCenter.X - Convert.ToInt32((activeRectWidthPadded / 2));
            yScreen = activeRectCenter.Y - Convert.ToInt32((activeRectHeightPadded / 2));
            activeRectPadded = new Rectangle(xScreen, yScreen, Convert.ToInt32(activeRectWidthPadded), Convert.ToInt32(activeRectHeightPadded));

            //Allows drawing cursor icons underneath the WinzoomForm.
            //wzuf is already fine and needs no updating since all it's location info happens in OnPaint

            //zbf.BackColor = Color.FromArgb(216,249,107); //a very unusual light green color
            wzf.magnification = (float)zoomPct / 100;
            wzf.mag.magnification = (float)zoomPct / 100;
            wzf.Width = Convert.ToInt32(activeRectWidth);
            wzf.Height = Convert.ToInt32(activeRectHeight);
            wzf.StartPosition = FormStartPosition.Manual;
            wzf.Location = new Point(activeRect.X, activeRect.Y);
            wzf.mag.SetSourceRect(activeRectSrc);
            wzf.mag.SetActiveRect(activeRect);
            wzf.Invalidate();
        }

        public void Hide()
        {
            if (!active) return;
            active = false;
            if (!wzuf.IsDisposed)
            {
                wzuf.Hide();
                wzuf.Dispose();
            }

            if (!wzf.IsDisposed)
            {
                wzf.Hide();
                wzf.Dispose();
            }
        }

        public Rectangle GetActiveRectFromType(ZOOMBOX_RECT_T rect_t)
        {
            Rectangle rect;
            switch (rect_t)
            {
                case ZOOMBOX_RECT_T.ACTIVE:
                    rect = activeRect;
                    break;
                case ZOOMBOX_RECT_T.PADDED:
                    rect = activeRectPadded;
                    break;
                case ZOOMBOX_RECT_T.SRC:
                    rect = activeRectSrc;
                    break;
                default:
                    rect = new Rectangle(0, 0, 0, 0);
                    //SHOULD NOT BE IN HERE
                    MessageBox.Show("Invalid entry in Zoombox.GetRectangleFromType()");
                    break;
            }
            return rect;
        }

        public bool WithinRect(Point p, ZOOMBOX_RECT_T rect_t)
        {
            if (!active)
            {
                return false;
            }

            Rectangle rect = GetActiveRectFromType(rect_t);

            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            p = BoundPointToRect(p, screenSize, 5);

            if (p.X >= rect.Left && p.X <= rect.Right && p.Y >= rect.Top && p.Y <= rect.Bottom)
            {
                return true;
            }
            return false;
        }

        public Point BoundPointToRect(Point p, Rectangle rect, int margin)
        {
            if (p.X < rect.Left)
                p.X = rect.Left + margin;
            if (p.Y < rect.Top)
                p.Y = rect.Top + margin;
            if (p.X >= rect.Right)
                p.X = rect.Right - margin;
            if (p.Y >= rect.Bottom)
                p.Y = rect.Bottom - margin;

            return p;
        }

        public Point ConvertToScreenPoint(Point p)
        {
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            p = BoundPointToRect(p, screenSize, 5);

            int delta = ScreenPixelHelper.ConvertMmToPixels(1);

            //We want to find the "center" which is the focal point of where magnified and system cursor intersect
            //on the winzoom box.  It is different whether edge or corner, or if winzoom box is in middle of screen
            int x = 0;
            if (Math.Abs(activeRect.Left - activeRectSrc.Left) < delta)
            {
                x = 0;
            }
            else if (Math.Abs(activeRect.Right - activeRectSrc.Right) < delta)
            {
                x = screenSize.Width;
            }
            else if(activeRect.Left < delta) //only activeRect is left aligned
            {
                //Experimentally and analytically determined this formula
                x = Convert.ToInt32((Convert.ToDouble(activeRectSrc.Left) * zoomPct / (zoomPct - 100) ));

            }
            else if (Math.Abs(activeRect.Right - screenSize.Width) < delta) //only activeRect is right aligned
            {
                int xdiff = (activeRect.Right - activeRectSrc.Right); //this will be a positive number
                int xDiffScaled = Convert.ToInt32((Convert.ToDouble(xdiff) * zoomPct / (zoomPct - 100)));
                x = activeRect.Right - xDiffScaled;
            }
            else
            {
                x = activeRectSrcCenter.X;
            }

            int y = 0;
            if (Math.Abs(activeRect.Top - activeRectSrc.Top) < delta)
            {
                y = 0;
            }
            else if (Math.Abs(activeRect.Bottom - activeRectSrc.Bottom) < delta)
            {
                y = screenSize.Height;
            }
            else if (activeRect.Top < delta) //only activeRect is left aligned
            {
                y = Convert.ToInt32((activeRectSrc.Top * zoomPct / (zoomPct - 100)));
            }
            else if (Math.Abs(activeRect.Bottom - screenSize.Width) < delta) //only activeRect is right aligned
            {
                int ydiff = (activeRect.Bottom - activeRectSrc.Bottom); //this will be a positive number
                int yDiffScaled = Convert.ToInt32((ydiff * zoomPct / (zoomPct - 100)));
                y = activeRect.Bottom - yDiffScaled;
            }
            else
            {
                y = activeRectSrcCenter.Y;
            }

            //This center is essentially the focal point of the magnified cursor and system cursor
            Point center = new Point(x, y);
            Point diff = new Point(p.X - center.X, p.Y - center.Y);

            //The magnified cursor and the actively controlled cursor are the same point in top left corner of activeRectSrc
            //As main cursor moves, magnified cursor moves away twice as fast if we return p and do no adjustments
            //Therefore we need to divide the distance of gaze point from top left corner by zoomFactor to get magnified and gaze point to be same

            return new Point(center.X + Convert.ToInt32((float)diff.X / ((float)zoomPct / (float)100)),
                             center.Y + Convert.ToInt32((float)diff.Y / ((float)zoomPct / (float)100))); ;
        }
    }
}
