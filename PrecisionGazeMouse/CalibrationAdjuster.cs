/*
 * Copyright 2019 Rishi Kapadia
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * http://www.apache.org/licenses/LICENSE-2.0|http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System.Windows.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;

/*
 * 
 * 
 * 
CalibrationAdjuster Overview
Given an existing 9-point calibration for eye-tracking (such as Tobii's for the 4C), and also a second-stage calibration performed using a different software, this module will compute the resultant gaze point by applying an offset in x,y as determined by the second-stage calibration.

How to use and theory of operation
adj abbreviation is for adjuster.

Point[] adjGrid is an array of size 9, with each item representing the calibration point (stimulus) location. Given 1000x1000 demo screen, Index 0 for example on Tobii calibration is 10% away from the top left corner of the screen (x=100,y=100). Index 1 would be at top middle (x=500, y = 100) ...

Point[] adjGridOffset describes the shift in x and shift in y in pixels to be applied for a given adjGrid[] coordinate. It describes the discrepancy between the Tobii original calibration and the second stage calibration for each adjGrid coordinate and stores that difference.

Method GetCalibrationAdjustment(Point p). p is the gaze point determined by Tobii SDK, and the method returns p shifted (adjusted) by a weighted sum of relevant elements in adjGridOffset.

How weighted sum is calculated
For simplicity consider the 1 dimensional case, where you have two calibration points (stimuli location) at coordinates A and B. A and B are separated by 10 pixels, A at x= 0, and B at x=10. Gaze point p is located at x = 4. Consequently, the correct way to adjust point p is by applying (60% of the adjustment offered by A since closer) + (40% of adjustmented offered at B).

The same concept is extended into 2 dimensions. Three cases form:
p is at a corner of the screen, in which case it is only close to 1 reference point.
p is at a side of the screen, where it is influenced by the adjustment of 2 reference points.
p is within the interior (i.e. more than 10% away from any given edge of the screen), and it is affected by 4 reference points.


*/

namespace PrecisionGazeMouse
{
    public class CalibrationAdjuster
    {
        public Point[] adjGrid; //the points at which the adjustment grid offsets are recorded
        public Point[] adjGridOffset; //the actual offset applied at each of the adjGrid points
        string csvPath = @"caladjust.csv";

        public CalibrationAdjuster()
        {
            adjGrid = new Point[9];
            /*
            index corresponds to following visual locations
               0     1      2
               3     4      5
               6     7      8

                (point 0,0) is top left corner.
            */
int w = GetScreenSize().Width;
            int h = GetScreenSize().Height;
            adjGrid[0] = new Point(Convert.ToInt16(w * 0.1), Convert.ToInt16(h * 0.1));
            adjGrid[1] = new Point(Convert.ToInt16(w * 0.5), Convert.ToInt16(h * 0.1));
            adjGrid[2] = new Point(Convert.ToInt16(w * 0.9), Convert.ToInt16(h * 0.1));

            adjGrid[3] = new Point(Convert.ToInt16(w * 0.1), Convert.ToInt16(h * 0.5));
            adjGrid[4] = new Point(Convert.ToInt16(w * 0.5), Convert.ToInt16(h * 0.5));
            adjGrid[5] = new Point(Convert.ToInt16(w * 0.9), Convert.ToInt16(h * 0.5));

            adjGrid[6] = new Point(Convert.ToInt16(w * 0.1), Convert.ToInt16(h * 0.9));
            adjGrid[7] = new Point(Convert.ToInt16(w * 0.5), Convert.ToInt16(h * 0.9));
            adjGrid[8] = new Point(Convert.ToInt16(w * 0.9), Convert.ToInt16(h * 0.9));

            //initialize all offsets to zero at first
            adjGridOffset = new Point[9];
            for (int i = 0; i < adjGridOffset.Length; i++)
            {
                adjGridOffset[i] = new Point(0, 0);
            }

        }

        private Point GetCornerOffset(Point p, int a)
        {
            if (a < adjGridOffset.Length)
            {
                return adjGridOffset[a];
            }
            else
            {
                return new Point(0, 0);
            }

        }

        private Point GetSideOffset(Point p, int a, int b)
        {
            if (a >= adjGridOffset.Length || b >= adjGridOffset.Length)
            {
                return new Point(0,0);
            }

            double da, db, dtotal, wa, wb, wtotal;
            da = PointHelper.GetPointDistance(p, adjGrid[a]);
            db = PointHelper.GetPointDistance(p, adjGrid[b]);
            dtotal = da + db;

            wa = dtotal - da;
            wb = dtotal - db;
            wtotal = wa + wb;


            //Apply weighted sum of a and b offsets
            return new Point(Convert.ToInt16(adjGridOffset[a].X * (wa / wtotal) + adjGridOffset[b].X * (wb / wtotal)),
                Convert.ToInt16(adjGridOffset[a].Y * (wa / wtotal) + adjGridOffset[b].Y * (wb / wtotal)));
        }

        public int IndexOfMinDistance(List<double> distances)
        {
            if (distances.Count < 1)
            { return 0; }

            int currentMinIndex = 0;
            double currentMinVal = distances[0];
            for (int i = 1; i < distances.Count; i++)
            {
                if (currentMinVal > distances[i])
                {
                    currentMinIndex = i;
                    currentMinVal = distances[i];
                }
            }
            return currentMinIndex;
        }


        //a and b are same horizontally, and     c and d are same horizontally
        private Point GetCentralOffset(Point p, int a, int b, int c, int d)
        {
            if (a >= adjGridOffset.Length || b >= adjGridOffset.Length || c >= adjGridOffset.Length || d >= adjGridOffset.Length)
            {
                return new Point(0, 0);
            }

            double da, db, dc, dd, dtotal, wa = 0, wb = 0, wc = 0, wd = 0;
            da = PointHelper.GetPointDistance(p, adjGrid[a]);
            db = PointHelper.GetPointDistance(p, adjGrid[b]);
            dc = PointHelper.GetPointDistance(p, adjGrid[c]);
            dd = PointHelper.GetPointDistance(p, adjGrid[d]);
            dtotal = da + db + dc + dd;

            Point A = adjGrid[a];
            Point B = adjGrid[b];
            Point C = adjGrid[c];
            Point D = adjGrid[d];

            List<Point> corners = new List<Point>() { A, B, C, D };

            List<double> distances = new List<double>();
            double distanceSum = 0;
            //compute distances
            for (int i = 0; i < corners.Count; i++)
            {
                Point corner = corners[i];
                double distance = PointHelper.GetPointDistance(p, corner);
                distances.Add(distance);
                distanceSum += distance;
            }

            double fractionLeft = 1; //100%
            while (corners.Count > 0)
            {
                int currentMinIndex = IndexOfMinDistance(distances);

                //1 - (di / dt) * (n - 1)
                double contribution = 1 - (distances[currentMinIndex] / distanceSum) * (distances.Count - 1);

                if (corners[currentMinIndex] == A)
                {
                    wa = fractionLeft * contribution;
                }
                else if (corners[currentMinIndex] == B)
                {
                    wb = fractionLeft * contribution;
                }
                else if (corners[currentMinIndex] == C)
                {
                    wc = fractionLeft * contribution;
                }
                else if (corners[currentMinIndex] == D)
                {
                    wd = fractionLeft * contribution;
                }

                fractionLeft -= contribution * fractionLeft;
                distanceSum -= distances[currentMinIndex];
                distances.RemoveAt(currentMinIndex);
                corners.RemoveAt(currentMinIndex);
            }

            //Apply weighted sum of offsets
            return new Point(
                Convert.ToInt16(adjGridOffset[a].X * wa
                + adjGridOffset[b].X * wb
                + adjGridOffset[c].X * wc
                + adjGridOffset[d].X * wd),
                Convert.ToInt16(
                    adjGridOffset[a].Y * wa
                    + adjGridOffset[b].Y * wb
                    + adjGridOffset[c].Y * wc
                    + adjGridOffset[d].Y * wd
            ));
        }

        public Point GetCalibrationAdjustment(Point p)
        {
            Point adj = new Point(0, 0);
            //assume user has done a 9 point fixed calibration
            //assume the corners that are calibrated to are fairly representative of the extremties towards the screen edge

            /*
           index corresponds to following visual locations
              0     1      2
              3     4      5
              6     7      8

               (point 0,0) is top left corner.
           */

            //horizontally before first column
            if (p.X <= adjGrid[0].X)
            {
                if (p.Y <= adjGrid[0].Y)
                {
                    //corner apply 0's offset
                    adj = GetCornerOffset(p, 0);
                }
                else if (p.Y < adjGrid[3].Y)
                {
                    //some of 0 and 3
                    adj = GetSideOffset(p, 0, 3);
                }
                else if (p.Y < adjGrid[6].Y)
                {
                    //some of 3 and 6
                    adj = GetSideOffset(p, 3, 6);
                }
                else
                {
                    //corner of 6
                    adj = GetCornerOffset(p, 6);
                }
            }
            //horizontally between first and second column
            else if (p.X <= adjGrid[1].X)
            {
                if (p.Y <= adjGrid[1].Y)
                {
                    //some of 0 and 1
                    adj = GetSideOffset(p, 0, 1);
                }
                else if (p.Y < adjGrid[4].Y)
                {
                    //some of 0,1,3,4
                    adj = GetCentralOffset(p, 0, 1, 3, 4);
                }
                else if (p.Y < adjGrid[7].Y)
                {
                    //some of 3,4,6,7
                    adj = GetCentralOffset(p, 3, 4, 6, 7);
                }
                else
                {
                    //some of 6,7
                    adj = GetSideOffset(p, 6, 7);
                }
            }
            //horizontally between second and third column
            else if (p.X < adjGrid[2].X)
            {
                if (p.Y < adjGrid[2].Y)
                {
                    //some of 1 and 2
                    adj = GetSideOffset(p, 1, 2);
                }
                else if (p.Y < adjGrid[5].Y)
                {
                    //some of 1,2,4,5
                    adj = GetCentralOffset(p, 1, 2, 4, 5);
                }
                else if (p.Y < adjGrid[8].Y)
                {
                    //some of 4,5,7,8
                    adj = GetCentralOffset(p, 4, 5, 7, 8);
                }
                else
                {
                    //some of 7,8
                    adj = GetSideOffset(p, 7, 8);

                }
            }
            //after 3rd column
            else
            {
                if (p.Y <= adjGrid[2].Y)
                {
                    //corner apply 2's offset
                    adj = GetCornerOffset(p, 2);
                }
                else if (p.Y < adjGrid[5].Y)
                {
                    //some of 2 and 5
                    adj = GetSideOffset(p, 2, 5);
                }
                else if (p.Y < adjGrid[8].Y)
                {
                    //some of 5 and 8
                    adj = GetSideOffset(p, 5, 8);

                }
                else
                {
                    //corner of 8
                    adj = GetCornerOffset(p, 8);
                }
            }

            return adj;
        }

        public static Rectangle GetScreenSize()
        {
            return ScreenPixelHelper.GetScreenSize();
        }


        //passes the grid location of the calibration point
        public Point getAdjGridOffsetFromCalibrationSamples(Point calpoint, Point[] calSamples, int calSamplesCount)
        {
            if (calSamples == null || calSamples.Length == 0 || calSamplesCount < 1)
            {
                return new Point(0, 0);
            }
            Point gp = calculateCalSamplesMean(calSamples, calSamplesCount); //the average of all the calSamples
            return new Point((calpoint.X - gp.X), (calpoint.Y - gp.Y));
        }


        private Point calculateCalSamplesMean(Point[] calSamples, int calSamplesCount)
        {
            Point p = new Point(0, 0);
            for (int i = 0; i < calSamplesCount; i++)
            {
                p.X += calSamples[i].X;
                p.Y += calSamples[i].Y;
            }
            p.X /= calSamplesCount;
            p.Y /= calSamplesCount;

            return p;
        }

        public void UpdateIpcServerCalibrationAdjustments()
        {
            int arrayLength = adjGrid.Length;
            IpcServerData.adjGrid = new System.Drawing.Point[arrayLength];
            IpcServerData.adjGridOffset = new System.Drawing.Point[arrayLength];

            for (int i = 0; i < arrayLength; i++)
            {
                IpcServerData.adjGrid[i] = new System.Drawing.Point(adjGrid[i].X, adjGrid[i].Y);
                IpcServerData.adjGridOffset[i] = new System.Drawing.Point(adjGridOffset[i].X, adjGridOffset[i].Y);
            }

            IpcServerData.lastModifiedServerCalibration = DateTime.Now;
        }

        public void writeCalibrationAdjustmentCsv()
        {
            Logger.WriteFunc(2);
            var csv = new StringBuilder();
            for (int i = 0; i < adjGridOffset.Length; i++)
            {
                var newLine = string.Format("{0},{1}", adjGridOffset[i].X, adjGridOffset[i].Y);
                csv.AppendLine(newLine);
            }

            //if the file already exists, then it is overwritten
            File.WriteAllText(csvPath, csv.ToString());

            //This is also a good time to update IPC server data for on screen keyboard usage
            UpdateIpcServerCalibrationAdjustments();
        }

        public void readCalibrationAdjustmentCsv()
        {
            Logger.WriteFunc(2);
            if (File.Exists(csvPath))
            {
                using (var reader = new StreamReader(csvPath))
                {
                    int i = 0;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.Length > 1)
                        {
                            var values = line.Split(',');
                            //MessageBox.Show(String.Format("{0},{1}\n", values[0], values[1]));
                            adjGridOffset[i].X = Convert.ToInt16(values[0]);
                            adjGridOffset[i].Y = Convert.ToInt16(values[1]);
                            i++;
                        }
                    }
                    UpdateIpcServerCalibrationAdjustments();
                }
            }
        }

        public void clearCalibrationAdjustments()
        {
            Logger.WriteFunc(2);
            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);

                //initialize all offsets to zero
                adjGridOffset = new Point[9];
                for (int i = 0; i < adjGridOffset.Length; i++)
                {
                    adjGridOffset[i] = new Point(0, 0);
                }
            }
        }
    }
}
