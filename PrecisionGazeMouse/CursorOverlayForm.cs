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
 * On the scroll down and scroll up eye gaze operations,
 * we want to show an icon that follows the gaze of the eyes.
 * This allows showing the down or up arrow anywhere on the screen as an overlay
 */

namespace PrecisionGazeMouse
{
    public partial class CursorOverlayForm : Form
    {
        public enum OVERLAY_TYPE
        {
            NONE,
            UPARROW,
            DOWNARROW
        }

        public static OVERLAY_TYPE overlayType = OVERLAY_TYPE.NONE;

        public static Image UpArrowImage;
        public static Image DownArrowImage;

        public Point currentPoint = new Point(0, 0);

        public CursorOverlayForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;

            this.Size = ScreenPixelHelper.GetScreenWorkingArea().Size;

            User32.SetWindowPos(this.Handle, User32.HWND_TOPMOST, 0, 0, 0, 0, (User32.SWP_NOMOVE | User32.SWP_NOSIZE | User32.SWP_SHOWWINDOW));

            UpArrowImage = Properties.Resources.uparrow;
            DownArrowImage = Properties.Resources.downarrow;

            Logger.WriteMsg("CursorOverlayForm instantiated.");
        }

        public void UpdateOverlayType(OVERLAY_TYPE ot)
        {
            if(overlayType != ot)
            {
                overlayType = ot;
                if(this.Visible)
                {
                    this.Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Point showPoint = new Point(currentPoint.X - 50, currentPoint.Y - 50);
            Rectangle showRect = new Rectangle(showPoint, new Size(100, 100));

            if (overlayType == OVERLAY_TYPE.UPARROW)
            {
                e.Graphics.DrawImage(UpArrowImage, showRect);
            }
            else if(overlayType == OVERLAY_TYPE.DOWNARROW)
            {
                e.Graphics.DrawImage(DownArrowImage, showRect);
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
