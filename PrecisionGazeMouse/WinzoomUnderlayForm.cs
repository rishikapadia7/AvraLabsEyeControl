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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


/*
 * For operations such as text selection, it's important for the user to see where the mouse cursor
 * is located. So this will paint an indicator similar to capital "I" beneath the zoombox.
 * Thus, it gets magnified by the actual zoombox window. 
 */

namespace PrecisionGazeMouse
{
    public partial class WinzoomUnderlayForm : Form
    {
        WarpPointers.WarpPointer warpPointer;
        CalibrationAdjuster calibrationAdjuster;
        Winzoom wz;
        public WinzoomUnderlayForm(WarpPointers.WarpPointer warpPointer, CalibrationAdjuster caladjuster, Winzoom wz)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;

            this.warpPointer = warpPointer;
            this.calibrationAdjuster = caladjuster;
            this.wz = wz;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            User32.SetFormTransparent(this.Handle);

            //Convert the current warp point a point on the screen
            Point wp = warpPointer.GetWarpPoint();
            wp.Offset(calibrationAdjuster.GetCalibrationAdjustment(wp));
            Point screenPoint = wz.ConvertToScreenPoint(wp);
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();

            if (ConfigManager.zoomboxGrid)
            {
                //Draw a vertical line
                e.Graphics.DrawLine(Pens.Black, new Point(screenPoint.X, screenSize.Top), new Point(screenPoint.X, screenSize.Bottom));

                //Draw a horizontal line
                e.Graphics.DrawLine(Pens.Black, new Point(screenSize.Left, screenPoint.Y), new Point(screenSize.Right, screenPoint.Y));
            }
            else
            {
                DrawApplicableCursorSymbol(e, screenPoint);
            }
        }

        Point lastIbeamPoint;

        public bool NearLastIbeamPoint(Point p)
        {
            if(lastIbeamPoint == null) { return false; }

            int nearbyMm = 7; //mm
            int nearbyPixels = ScreenPixelHelper.ConvertMmToPixels(nearbyMm);
            return PointHelper.GetPointDistance(p, lastIbeamPoint) <= nearbyPixels;
        }


        private void DrawApplicableCursorSymbol(PaintEventArgs e, Point p)
        {
            IntPtr currentHandle = User32.GetCursorHandle();
            IntPtr ibeamHandle = Cursors.IBeam.Handle;
            if (currentHandle == ibeamHandle)
            {
                lastIbeamPoint = p;
                //Draw our version of I

                //vertical line (measured with pixel ruler to be a total of 16 pixels tall)
                //Choosing Blue, because Windows alternates between white and black based on background darkness
                e.Graphics.DrawLine(Pens.Blue, new Point(p.X, p.Y + 8), new Point(p.X, p.Y - 8));
                e.Graphics.DrawLine(Pens.Blue, new Point(p.X - 1, p.Y), new Point(p.X + 1, p.Y));
                //Don't bother with drawing top and bottom horizontals, they don't really help user position anything
            }
            else if(NearLastIbeamPoint(p)) //draw it even if it still near the last ibeam point
            {
                e.Graphics.DrawLine(Pens.Blue, new Point(p.X, p.Y + 8), new Point(p.X, p.Y - 8));
                e.Graphics.DrawLine(Pens.Blue, new Point(p.X - 1, p.Y), new Point(p.X + 1, p.Y));
            }
        }

        //Ensures that the form does not show up in the alt-tab menu
        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
    }
}
