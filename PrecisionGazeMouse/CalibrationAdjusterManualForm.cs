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
using System.IO;

/*
 * This provides calibration adjustment through the use of keyboard keys.
 * The real-time adjusted gaze position is shown. The user can use arrow keys
 * to adjust their offset from the shown calibration stimulus.
 * Press the Enter key to advance to the next calibration point.
 */

namespace PrecisionGazeMouse
{
    public partial class CalibrationAdjusterManualForm : Form
    {
        Graphics graphics;
        int numCircles;
        int currentCircle;
        CalibrationAdjuster ca;
        MouseController controller;

        public CalibrationAdjusterManualForm(CalibrationAdjuster caladjust, MouseController mc)
        {
            ca = caladjust;
            controller = mc;
            numCircles = ca.adjGrid.Length;
            currentCircle = 0;
            graphics = CreateGraphics();
            Load += new EventHandler(CalibrationAdjusterManualForm_Load);
            FormClosing += CalibrationAdjusterManualForm_FormClosing;
            KeyDown += new KeyEventHandler(CalibrationAdjusterManualForm_KeyDown);
        }

        private void CalibrationAdjusterManualForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PrecisionGazeMouseForm.calibrationInProgress = false;
        }

        private void CalibrationAdjusterManualForm_Load(object sender, EventArgs e)
        {
            PrecisionGazeMouseForm.calibrationInProgress = true;

            //maximizes the form
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
        }

        //whenever this method is called, it paints from scratch and does not append to what's already on the screen
        //this method is typically called whenever RefreshScreen is called (60 fps)
        protected override void OnPaint(PaintEventArgs e)
        {
            int radius = ScreenPixelHelper.ConvertMmToPixels(1.25); //radius
            Point p = ca.adjGrid[currentCircle];
            Rectangle rec = new Rectangle(p.X - radius, p.Y - radius, radius * 2, radius * 2);
            e.Graphics.DrawEllipse(Pens.White, rec);

            //draw gray gaze point
            p = controller.WarpPointer.GetNextPoint(controller.WarpPointer.GetGazePoint());
            p.Offset(ca.adjGridOffset[currentCircle]);

            rec = new Rectangle(p.X - radius, p.Y - radius, radius * 2, radius * 2);
            e.Graphics.FillEllipse(Brushes.Gray, rec);
            
        }

        private void CalibrationAdjusterManualForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            int s = ScreenPixelHelper.ConvertMmToPixels(1); //step size of keyboard arrow input to control the gaze point adjustment offset
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    currentCircle++;
                    this.Invalidate();// This forces the form to repaint
                    break;
                case Keys.Up:
                    ca.adjGridOffset[currentCircle].Offset(0, -1 * s);
                    break;
                case Keys.Down:
                    ca.adjGridOffset[currentCircle].Offset(0, s);
                    break;
                case Keys.Left:
                    ca.adjGridOffset[currentCircle].Offset(-1 * s, 0);
                    break;
                case Keys.Right:
                    ca.adjGridOffset[currentCircle].Offset(s, 0);
                    break;
                case Keys.Alt:
                //no break stmt
                case Keys.Escape:
                    ca.writeCalibrationAdjustmentCsv();
                    this.Close(); //prevent altf4 crash
                    break;
                case Keys.Space:
                    //record adjustment
                    Point wp = controller.WarpPointer.GetWarpPoint();
                    Point calpoint = ca.adjGrid[currentCircle];
                    ca.adjGridOffset[currentCircle] = new Point((calpoint.X - wp.X), (calpoint.Y - wp.Y));
                    this.Invalidate();// This forces the form to repaint
                    break;
                default:
                    break;
            }

            //see if done all adjustment points
            if (currentCircle == 9)
            {
                ca.writeCalibrationAdjustmentCsv();
                currentCircle = 0;
                this.Close();
            }

        }
    }
}
