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
using Tobii.EyeX.Framework;
using Tobii.EyeX.Client;
using System.Collections.Generic;
using EyeXFramework;

namespace PrecisionGazeMouse.WarpPointers
{
    class EyeXWarpPointer : WarpPointer
    {
        public static EyeXHost EyeXHost = new EyeXHost();
        GazePointDataStream gazeDataStream;

        Point warpPoint;
        Point[] samples;
        public List<Point> samplesHistory;
        int sampleIndex;
        int sampleCount;
        bool setNewWarp;
        int warpThreshold;

        const int samplesArraySize = 5;

        const int sampleRate = 60;
        const int samplesHistorySize = sampleRate * 5; //keep most recent 5 second of gaze points

        public EyeXWarpPointer(int threshold)
        {
            samples = new Point[samplesArraySize];
            samplesHistory = new List<Point>();
            warpThreshold = threshold;


            if (!EyeXHost.IsStarted)
            {
                EyeXHost.Start(); // Start the EyeX host
            }

            //default is lightly filtered, just making it explicit
            gazeDataStream = EyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);
            if (gazeDataStream != null)
            {
                gazeDataStream.Next += UpdateGazePosition;
            }
        }

        public bool IsStarted()
        {
            return EyeXHost.GazeTracking.Value == GazeTracking.GazeTracked;
        }

        public bool IsWarpReady()
        {
            return sampleCount > samplesArraySize;
        }

        protected void UpdateGazePosition(object s, GazePointEventArgs data)
        {
            if (!double.IsNaN(data.X) && !double.IsNaN(data.Y))
            {
                System.Drawing.Point p = new System.Drawing.Point((int)data.X, (int)data.Y);
                sampleCount++;
                sampleIndex++;
                if (sampleIndex >= samples.Length)
                    sampleIndex = 0;
                samples[sampleIndex] = p;

                samplesHistory.Add(p);
                if (samplesHistory.Count > samplesHistorySize)
                {
                    samplesHistory.RemoveAt(0); //removes the oldest sample
                }
            }
        }

        //screenPercent is between 0 and 100
        public bool largeSaccadeDetected(double screenPercent)
        {
            Point currentPoint = samples[sampleIndex];
            //get screen percent in terms of pixels
            System.Drawing.Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            //find hypotenuse of distance in pixels for screenpercentage
            double pixelsThreshold = Math.Sqrt(Math.Pow(screenSize.Width, 2) + Math.Pow(screenSize.Height, 2)) * screenPercent / 100;

            Point saccadeDiff = new Point(Math.Abs(currentPoint.X - GetWarpPoint().X), Math.Abs(currentPoint.Y - GetWarpPoint().Y));
            double saccadeDiffPixels = Math.Sqrt(Math.Pow(saccadeDiff.X, 2) + Math.Pow(saccadeDiff.Y, 2));

            if (saccadeDiffPixels > pixelsThreshold)
            {
                return true;
            }

            return false;
        }

        public Point calculateSmoothedPoint()
        {
            //if current point is more than some percentage the screen away, then set to new warp point
            //and refresh tracking
            if (largeSaccadeDetected(ScreenPixelHelper.ConvertMmToPixels(Properties.Settings.Default.largeSaccadeThresholdMm)))
            {
                warpPoint = samples[sampleIndex];
                RefreshTracking();
                //Fill rest of array with the same item
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] = warpPoint;
                }
            }

            return calculateTrimmedMean();
        }



        private Point calculateTrimmedMean()
        {
            Point p = calculateMean();

            // Find the furthest point from the mean
            double maxDist = 0;
            int maxIndex = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                double dist = Math.Pow(samples[i].X - p.X, 2) + Math.Pow(samples[i].Y - p.Y, 2);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxIndex = i;
                }
            }

            // Calculate a new mean without the furthest point
            p = new Point(0, 0);
            for (int i = 0; i < samples.Length; i++)
            {
                if (i != maxIndex)
                {
                    p.X += samples[i].X;
                    p.Y += samples[i].Y;
                }
            }
            p.X /= (samples.Length - 1);
            p.Y /= (samples.Length - 1);

            return p;
        }

        private Point calculateMean()
        {
            Point p = new Point(0, 0);
            for (int i = 0; i < samples.Length; i++)
            {
                p.X += samples[i].X;
                p.Y += samples[i].Y;
            }
            p.X /= samples.Length;
            p.Y /= samples.Length;

            return p;
        }

        public override String ToString()
        {
            if (sampleIndex < samples.Length)
            {
                return String.Format("({0:0}, {1:0})", samples[sampleIndex].X, samples[sampleIndex].Y);
            }
            else
            {
                return "";
            }

        }

        public Point GetGazePoint()
        {
            return samples[sampleIndex];
        }

        public int GetSampleCount()
        {
            return sampleCount;
        }

        public int GetWarpTreshold()
        {
            return warpThreshold;
        }

        public Point GetWarpPoint()
        {
            return warpPoint;
        }

        public Point GetNextPoint(Point currentPoint)
        {
            Point smoothedPoint = calculateSmoothedPoint();
            Point delta = Point.Subtract(smoothedPoint, new System.Drawing.Size(warpPoint)); // whenever there is a big change from the past
            double distance = Math.Sqrt(Math.Pow(delta.X, 2) + Math.Pow(delta.Y, 2));
            if (!setNewWarp && distance > GetWarpTreshold()) //
            {
                sampleCount = 0;
                setNewWarp = true;
            }

            if (setNewWarp && IsWarpReady())
            {
                warpPoint = smoothedPoint;
                setNewWarp = false;
            }

            return warpPoint;
        }

        public void Dispose()
        {
            gazeDataStream.Dispose();
            EyeXHost.Dispose();
        }

        public void RefreshTracking()
        {
            sampleCount = 0;
            setNewWarp = true;
        }

        //*****unused code below ****

        private double calculateStdDev()
        {
            Point u = calculateMean();

            double o = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                Point delta = Point.Subtract(samples[i], new System.Drawing.Size(u));
                o += Math.Pow(delta.X, 2) + Math.Pow(delta.Y, 2);
            }
            return Math.Sqrt(o / samples.Length);
        }

        public List<Point> GetGazeHistory()
        {
            return samplesHistory;
        }

        public int GetSampleRate()
        {
            return sampleRate;
        }
    }
}
