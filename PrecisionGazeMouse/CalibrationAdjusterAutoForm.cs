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
 * This provides a screen for a 9-point calibration where there are shrinking circles.
 * 1 calibration stimulus is visible at a time (i.e. 1 of the 9 points)
 * Successful completion of 1 calibration point, leads to the circle appearing at the next location. Process repeats until all 9 completed.
 */

namespace PrecisionGazeMouse
{
    public partial class CalibrationAdjusterAutoForm : Form
    {
        Graphics graphics;
        int numCircles;
        int currentCircle;
        CalibrationAdjuster ca;
        MouseController controller;

        public enum CalState
        {
            WAITING_FOCUS,
            FOCUS_IN_PROGRESS,
            FOCUSED,
            COLLECTING_SAMPLES
        };

        int iteration = 0;

        CalState calstate;

        public CalibrationAdjusterAutoForm(CalibrationAdjuster caladjust, MouseController mc)
        {
            ca = caladjust;
            controller = mc;
            numCircles = ca.adjGrid.Length;
            currentCircle = 0;
            graphics = CreateGraphics();
            Load += new EventHandler(CalibrationAdjusterAutoForm_Load);
            FormClosing += CalibrationAdjusterAutoForm_FormClosing;
            KeyDown += new KeyEventHandler(CalibrationAdjusterAutoForm_KeyDown);

            calstate = CalState.WAITING_FOCUS;

        }

        private void CalibrationAdjusterAutoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PrecisionGazeMouseForm.calibrationInProgress = false;
        }

        private void CalibrationAdjusterAutoForm_Load(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            PrecisionGazeMouseForm.calibrationInProgress = true;
            //maximizes the form
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
        }


        long startTime = 0;
        public void StartTimer()
        {
            startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public long StopTimerGetDuration()
        {
            long stoptime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return (stoptime - startTime);
        }


        public bool DoneWaitingTime(int milliseconds)
        {
            if (StopTimerGetDuration() > milliseconds)
            {
                return true;
            }
            return false;
        }

        long sampleCollectionStartTime = 0;

        private void UpdateCalibrationAdjustmentWithCurrentGaze()
        {
            List<Point> rawGazePoints = controller.WarpPointer.GetGazeHistory();
            if (rawGazePoints == null || rawGazePoints.Count == 0)
            {
                //just use the previous warp point
                Point wp = controller.WarpPointer.GetWarpPoint();
                Point calpoint = ca.adjGrid[currentCircle];
                ca.adjGridOffset[currentCircle] = new Point((calpoint.X - wp.X), (calpoint.Y - wp.Y));
            }
            else
            {
                //Average out the most recent gaze points
                int sampleRate = controller.WarpPointer.GetSampleRate();

                int recentDurationMs = (int) (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - sampleCollectionStartTime);
                int lookbackSampleCount = sampleRate * recentDurationMs / 1000;
                int startIndex = Math.Max(0, rawGazePoints.Count - lookbackSampleCount);
                List<Point> recentSamples = new List<Point>();

                for (int i = startIndex; i < rawGazePoints.Count; i++)
                {
                    recentSamples.Add(rawGazePoints[i]);
                }

                ca.adjGridOffset[currentCircle] = ca.getAdjGridOffsetFromCalibrationSamples(ca.adjGrid[currentCircle], recentSamples.ToArray(), recentSamples.Count);
            }
        }

        //Calculates target circle radius based on number of iterations completed, max number of iterations, min and max circle radiuses
        private int GetTargetCircleRadius(int iter)
        {
            int maxIterations = ConfigManager.calibrationIterationCountMax;
            int minRadius = ScreenPixelHelper.ConvertMmToPixels(1);
            int maxRadius = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.calibrationCircleSizeMm);

            int rDiff = maxRadius - minRadius;
            double rStep = (double)rDiff / (double)(maxIterations - 1);
            int iterBounded = Math.Min(iter, maxIterations - 1); //note: deliberately capping at maxIterations -1 and therefore never reaches 0.2% screen
            return (int)(maxRadius - iterBounded * rStep);
        }
        

        //whenever this method is called, it paints from scratch and does not append to what's already on the screen
        //this method is typically called whenever RefreshScreen is called (60 fps)
        protected override void OnPaint(PaintEventArgs e)
        {
            //Draw overall target circle (which shrinks with duration)
            int radius = GetTargetCircleRadius(iteration);
            Point calpoint = ca.adjGrid[currentCircle];
            Rectangle rec = new Rectangle(calpoint.X - radius, calpoint.Y - radius, radius * 2, radius * 2);

            if(iteration == 0)
            {
                e.Graphics.DrawEllipse(Pens.White, rec);
            }
            else if(iteration < ConfigManager.calibrationIterationCountMax - 1)
            {
                e.Graphics.DrawEllipse(Pens.DarkCyan, rec);
                //also draw inner circles to guide
                radius = GetTargetCircleRadius(iteration + 1);
                rec = new Rectangle(calpoint.X - radius, calpoint.Y - radius, radius * 2, radius * 2);
                e.Graphics.DrawEllipse(Pens.DarkMagenta, rec);
            }
            else
            {
                e.Graphics.FillEllipse(Brushes.DarkMagenta, rec);
                e.Graphics.DrawEllipse(Pens.DarkCyan, rec);
            }
            
            //Reset the iterations and calibrating this point if user has already been trying longer than expected
            if(iteration > ConfigManager.calibrationIterationCountMax + 1)
            {
                calstate = CalState.WAITING_FOCUS;
                iteration = 0;
            }
            

            Point gp = controller.WarpPointer.GetNextPoint(controller.WarpPointer.GetGazePoint());

            //detect whether gaze point is close to the calibration circle (at most 7cm of screen away)
            if (PointHelper.GetPointDistance(gp, calpoint) < ScreenPixelHelper.ConvertMmToPixels(70))
            {
                //draw gray gaze point as long as it's not the final iteration(s)
                int gazeRadius = ScreenPixelHelper.ConvertMmToPixels(1) / 2;
                Point p = controller.WarpPointer.GetNextPoint(controller.WarpPointer.GetGazePoint());            
                p.Offset(ca.adjGridOffset[currentCircle]);
                rec = new Rectangle(p.X - gazeRadius, p.Y - gazeRadius, gazeRadius * 2, gazeRadius * 2);

                if (iteration < ConfigManager.calibrationIterationCountMax)
                {
                    e.Graphics.FillEllipse(Brushes.Gray, rec);

                    if(iteration > 0)
                    {
                        //Get antipoint, which is basically located on other cide of currentCircle
                        int xDiff = p.X - ca.adjGrid[currentCircle].X;
                        int yDiff = p.Y - ca.adjGrid[currentCircle].Y;
                        Point ap = new Point(p.X - xDiff * 2, p.Y - yDiff * 2);

                        rec = new Rectangle(ap.X - gazeRadius, ap.Y - gazeRadius, gazeRadius * 2, gazeRadius * 2);
                        e.Graphics.FillEllipse(Brushes.Gray, rec);
                    }
                }
                    

                if (calstate == CalState.WAITING_FOCUS)
                {
                    StartTimer();
                    calstate = CalState.FOCUS_IN_PROGRESS;
                    return;
                }
                else if (calstate == CalState.FOCUS_IN_PROGRESS)
                {
                    if (DoneWaitingTime(150))
                    {
                        calstate = CalState.FOCUSED;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (calstate == CalState.FOCUSED)
                {
                    //Done waiting the focus time and eyes are focussed
                    UpdateCalibrationAdjustmentWithCurrentGaze();

                    StartTimer();
                    sampleCollectionStartTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    calstate = CalState.COLLECTING_SAMPLES;
                    iteration = 0;
                    return;
                }
                else if (calstate == CalState.COLLECTING_SAMPLES)
                {
                    //Done waiting time to start collecting samples

                    //We don't want the gaze to lag across the screen so we trick the user thinking it's centered already
                    if (iteration == 0 || iteration == 1)
                    {
                        UpdateCalibrationAdjustmentWithCurrentGaze();
                    }

                    if (DoneWaitingTime(ConfigManager.calibrationIterationTimeMs))
                    {
                        //Check if current gaze point is close to the center of the target circle
                        iteration++;
                        Point wp = controller.WarpPointer.GetWarpPoint();
                        double pointDistance = PointHelper.GetPointDistance(calpoint, wp);
                        int pointDistanceThreshold = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.calibrationCompletedThresholdMm);
                        UpdateCalibrationAdjustmentWithCurrentGaze();
                         if (iteration >= ConfigManager.calibrationIterationCountMax 
                            && pointDistance < pointDistanceThreshold)
                        {
                            //calibration for this point is successful
                            currentCircle++;
                            if (currentCircle == 9)
                            {
                                ca.writeCalibrationAdjustmentCsv();
                                currentCircle = 0;
                                Cursor.Show();
                                this.Close();
                            }
                        }
                        else
                        {
                            StartTimer();
                        }
                        this.Invalidate();// This forces the form to repaint
                    }
                }
            }
            else
            {
                iteration = 0;
                calstate = CalState.WAITING_FOCUS;
            }
        }

        private void CalibrationAdjusterAutoForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Alt:
                //no break stmt
                case Keys.Escape:
                    ca.writeCalibrationAdjustmentCsv();
                    this.Close(); //prevent altf4 crash
                    break;
                case Keys.Space:
                    //record adjustment
                    UpdateCalibrationAdjustmentWithCurrentGaze();
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
