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

/*
 * There are 2 modes of eye gaze cursor movement:
 * 1) The mouse cursor always follows where the eyes are looking. 
 * This mode can be annoying as everything will highlight and flicker 
 * even though the user is just reading the screen.
 * 2) The mouse cursor only follows when a zoombox is active. 
 * Where the user actually is about to make a click and the highlighting
 * of elements on the screen is useful.
 * 
 * It's for the 2nd case this form has been designed.
 * It basically relocates the cursor to remain static at the location of the start button.
 */

namespace PrecisionGazeMouse
{
    public partial class CursorHiderForm : Form
    {
        CursorMagnet cursorMagnet;
        ScreenCapture sc = new ScreenCapture();
        static Image targetWindowImg = null;

        public CursorHiderForm(CursorMagnet cursorMagnet)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            this.Location = GetStartMenuRect().Location;
            this.Size = GetStartMenuRect().Size;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font; ;
            this.Bounds = GetStartMenuRect();
            this.Capture = true;
            this.DesktopBounds = GetStartMenuRect();
            this.MaximizedBounds = GetStartMenuRect();

            this.cursorMagnet = cursorMagnet;

            this.Shown += CursorHiderForm_Shown;
        }

        private void CursorHiderForm_Shown(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            Point target = GetStartMenuLocation();

            if(targetWindowImg == null && !GetStartMenuRect().Contains(CursorMagnet.previousGazePoint))
            {
                Image screenimg = sc.CaptureScreen();
                targetWindowImg = cropImage(screenimg, GetStartMenuRect());
            }

            if(targetWindowImg != null)
                this.BackgroundImage = targetWindowImg;
            
            cursorMagnet.previousCursorPosition = target; ; //must be set as well otherwise external mouse movement is detected
            Cursor.Position = target;
        }

        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        //NOTE: it doesn't really matter if user's taskbar is arranged elsewhere since we are using this hider form
        public static Point GetStartMenuLocation()
        {
            return new Point(ScreenPixelHelper.ConvertMmToPixels(3), ScreenPixelHelper.GetScreenSize().Height - ScreenPixelHelper.ConvertMmToPixels(4));
        }

        public static Rectangle GetStartMenuRect()
        {
            Rectangle screenRect = ScreenPixelHelper.GetScreenSize();
            Rectangle waRect = ScreenPixelHelper.GetScreenWorkingArea();

            //the taskbar might be located other than the bottom of the screen default
            //But it doesn't matter since we are showing this overlay form
            return new Rectangle(0, waRect.Bottom, 48, screenRect.Bottom - waRect.Bottom); //value measured with pixel ruler
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
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
