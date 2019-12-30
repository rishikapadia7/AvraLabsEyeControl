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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

/*
 * NOTE: most of the code in this file is unused.
 * 
 * It was written with the intention that if the software is aware of the position and size of every
 * clickable object on the current screen, with each item known as a clickable "box"
 * then perform smart logic to predict the user's intention as to which box
 * the user actually wants to look at.
 * This would help reduce the flickering and bouncing back and forth between adjacent clickable boxes.
 * 
 * The experiment project was to use Selenium in Google Chrome to test this code, since Selenium
 * has access to all clickable objects within the web browser.
 * 
 * To truly identify the position and size of all elements on the screen, refer to AutoHotKey source code.
 * There are some functions in AutoHotkey that are capable of retrieving the size and position of elements on the screen.
 * That is my hint.
 * 
 */

namespace PrecisionGazeMouse
{
    public class Box
    {
        public int top, bottom, left, right;
        public enum SIDE { NONE, TOP, BOTTOM, LEFT, RIGHT };

        public Box(int t, int b, int l, int r)
        {
            top = t;
            bottom = b;
            left = l;
            right = r;
        }

        public Box()
        {
            top = 0;
            bottom = 0;
            left = 0;
            right = 0;
        }
        public Point center()
        {
            return new Point((left + right) / 2, (top + bottom) / 2);
        }

        public int width() { return right - left; }
        public int height() { return bottom - top; }

        public bool ContainsPoint(Point p)
        {
            return (p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom);
        }

        
        public SIDE GetClosestSideToSrcBoxX(Box srcBox)
        {
            int xdiff; //temp variable
            SIDE sideX; //the closest side
            if (this.left - srcBox.right > srcBox.left - this.right)
            {
                xdiff = this.left - srcBox.right;
                sideX = (xdiff >= 0) ? SIDE.LEFT : SIDE.NONE;
            }
            else
            {
                xdiff = srcBox.left - this.right;
                sideX = (xdiff >= 0) ? SIDE.RIGHT : SIDE.NONE;
            }

            return sideX;
        }

        public SIDE GetClosestSideToSrcBoxY(Box srcBox)
        {
            SIDE sideY;
            int ydiff;
            if (this.top - srcBox.bottom > srcBox.top - this.bottom)
            {
                ydiff = this.top - srcBox.bottom;
                sideY = (ydiff >= 0) ? SIDE.TOP : SIDE.NONE;
            }
            else
            {
                ydiff = srcBox.top - this.bottom;
                sideY = (ydiff >= 0) ? SIDE.BOTTOM : SIDE.NONE;
            }
            return sideY;
        }

        public bool ContainsPointPixelsWithin(Box srcBox, Point p, int pixels)
        {
            if (!ContainsPoint(p)) return false;
            //Figure out which sides on the current box (destination) are closest to the srcBox
            SIDE sideX = GetClosestSideToSrcBoxX(srcBox);
            SIDE sideY = GetClosestSideToSrcBoxY(srcBox);

            //Calculate the distance the current gaze point p is from the closest side
            //diff is the distance from the closest side to the gazepoint.
            int xdiff, ydiff;
            xdiff = (sideX == SIDE.LEFT) ? p.X - this.left : 0;
            xdiff = (sideX == SIDE.RIGHT) ? this.right - p.X : xdiff;

            ydiff = (sideY == SIDE.TOP) ? p.Y - this.top : 0;
            ydiff = (sideY == SIDE.BOTTOM) ? this.bottom - p.Y : ydiff;

            return xdiff > pixels || ydiff > pixels;
        }

        public bool ContainsPointPercentageWithin(Box srcBox, Point p, int perc)
        {
            if (!ContainsPoint(p)) return false;

            //Figure out which sides on the current box (destination) are closest to the srcBox
            SIDE sideX = GetClosestSideToSrcBoxX(srcBox);
            SIDE sideY = GetClosestSideToSrcBoxY(srcBox);

            //Calculate the distance the current gaze point p is from the closest side
            //diff is the distance from the closest side to the gazepoint.
            int xdiff, ydiff;
            xdiff = (sideX == SIDE.LEFT) ? p.X - this.left : 0;
            xdiff = (sideX == SIDE.RIGHT) ? this.right - p.X: xdiff;

            ydiff = (sideY == SIDE.TOP) ? p.Y - this.top : 0;
            ydiff = (sideY == SIDE.BOTTOM) ? this.bottom - p.Y : ydiff;
            
            int xdiffperc = xdiff * 100 / (width() / 2);
            int ydiffperc = ydiff * 100 / (height() / 2);
            return xdiffperc > perc || ydiff > perc;
        }
    }

    public class CursorMagnet
    {
        public static Point previousGazePoint;

        public Point previousCursorPosition;
        public bool winzoomAvailable = true;
        public volatile bool boxesInitComplete = false;

        volatile List<Box> boxes;

        const string startUrl = "http://www.NBA.com";

        public string previousBrowserUrl = "";
        public int previousTabsCount = 0;

        Point[] recordedPoints;

        public Thread BoxesTaskPeriodic;
        public volatile bool scrollClickHappened = false;

        const string csvPath = "gazePointLog.csv";

        MouseController mc;
        CalibrationAdjuster calibrationAdjuster;

        public CursorMagnet(MouseController mousecontroller, CalibrationAdjuster calibrationAdjuster)
        {
            previousGazePoint = new Point(0, 0);
            previousCursorPosition = new Point(0, 0);
            recordedPoints = new Point[60];
            boxes = new List<Box>();
            mc = mousecontroller;
            this.calibrationAdjuster = calibrationAdjuster;
        }

        int rpi = 0;

        void recordPoint(Point p)
        {
            if (rpi < recordedPoints.Length)
            {
                recordedPoints[rpi] = p;
                rpi++;
            }
            else
            {
                Logger.WriteMsg("Starting to play.");
                //ConfigManager.playGazeRecording = true;
                playRecordedPoints(recordedPoints);
            }

        }

        void playRecordedPoints(Point[] points)
        {
            foreach (Point a in points)
            {

                Cursor.Position = a;
            }
            /*   Cursor.Position = recordedPoints[rpi2];
               previousCursorPosition = recordedPoints[rpi2];
               rpi2++;
           }
           else
           {
               rpi2 = 0;
           }*/
        }

        //Initiatialization (not for constructor) but rather when the page loads
        public void Init()
        {
            string repoPath = "C:\\repos\\precisionGazeMouse\\";
            string driverPath = "packages\\Selenium.WebDriver.3.6.0";


            BoxesTaskPeriodic.Start();

            //Currently we don't need periodically cursor selection.  We can evaluate on a per-incoming-point basis
            //setup timer
            /*
            int timerIntervalMs;
            uint refreshRate;
            
            cursorExploreTimer = new System.Timers.Timer();
            refreshRate = PrecisionGazeMouseForm.getRefreshFps(); //we have 8 points to check for each form refresh
            timerIntervalMs = Convert.ToInt32((double)1000 / (double)refreshRate);
            cursorExploreTimer.Interval = timerIntervalMs;
            cursorExploreTimer.Elapsed += (sender, e) => clipTo(sender, e, boxes);
            cursorExploreTimer.Start();
            */
        }

        long startTime = 0;
        public void profileStartTimer()
        {
            startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public long profileStopGetDuration()
        {
            long stoptime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return (stoptime - startTime);
        }

        public void setGazePoint(Point p)
        {
            previousGazePoint = p;
        }

        private bool csvHeaderWritten = false;
        long previousMillisecondsStart = 0;

        public void writeGazePointToCsv()
        {
            long millisecondsStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            try
            {
                if (!csvHeaderWritten)
                {
                    //The very first time
                    //Overwrites any files that exists
                    File.WriteAllText(csvPath, "gpx,gpy,cpx,cpy,startms,duration\n");

                    //We omit the duration on the very first one
                    var newLine = string.Format("{0},{1},{2},{3},{4}",
                    previousGazePoint.X,
                    previousGazePoint.Y,
                    previousCursorPosition.X,
                    previousCursorPosition.Y,
                    millisecondsStart);

                    csvHeaderWritten = true;
                }
                else
                {
                    //Duration will be on the same line as it's original timestamp start
                    var duration = millisecondsStart - previousMillisecondsStart;

                    var newLine = string.Format(",{0}\n{1},{2},{3},{4},{5}",
                        duration,
                        previousGazePoint.X,
                        previousGazePoint.Y,
                        previousCursorPosition.X,
                        previousCursorPosition.Y,
                        millisecondsStart);


                    previousMillisecondsStart = millisecondsStart;

                    //if the file already exists, then it is overwritten
                    File.AppendAllText(csvPath, newLine);
                }
            }
            catch (Exception e)
            {
                Logger.WriteError(e.ToString());
            }

        }

        public void setCursorPosition(Point p, bool zbShown)
        {
            ConfigManager.precisionGazeMouseFormRefreshCount++;
            //if the cursor magnet is not actually running, set the cursor location to the gaze point directly
            if (ConfigManager.cursorMagnetEnabled == false || zbShown || !boxesInitComplete)
            {
                winzoomAvailable = true;
                Cursor.Position = p;
                previousCursorPosition = p;

                //recordPoint(p);
            }
            else //else run the actual cursor magnet
            {
                //We clip to box if only 1 is around, otherwise we use the same gaze point
                //Point clippedPosition = ClipToBox(boxes, p);
                Point clippedPosition = ClipToBox(p);
                Cursor.Position = clippedPosition;
                previousCursorPosition = clippedPosition;
            }
        }

        Box ClippedBox = null;

        public List<Box> getNearBoxes(Point gp)
        {
            List<Box> nearby = new List<Box>();
            int nearThreshold = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.cursorMagnetClippingThresholdMm);
            foreach (Box b in boxes)
            {
                //should only work if the box have height and width 

                //if box is valid and the point is near a box defined by threshold, then we add it to the working set
                if ((isNearBox(gp, b, nearThreshold)) && (b.top != b.bottom && b.left != b.right))
                {
                    nearby.Add(b);
                    // Debug.WriteLine("Found Box nearby" + currentBox[0] + "," + currentBox[1] + "  " + Cursor.Position);
                }

            }
            return nearby;
        }

        public bool BoxLargeEnough(Box b)
        {
            if(b == null) { return false; }

            int threshold = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.cursorMagnetClippingMinBoxWidthMm);
            return (b.width() >= threshold && b.height() >= threshold);
        }

        public bool BoxTooSmall(Box b)
        {
            return !BoxLargeEnough(b);
        }

        public bool CheckAllBoxesLargeEnough(List<Box> boxes)
        {
            foreach(Box b in boxes)
            {
                if(BoxTooSmall(b))
                {
                    return false;
                }
            }
            return true;
        }


        public Point ClipToBox(Point gp)
        {
            //Check if LinkDict is not populated
            if (ClippedBox == null)
            {
                if (!boxesInitComplete || boxes.Count < 1)
                    return gp;
                //We also want to make sure that the gp is not near the Windows Taskbar, otherwise have difficulty using start menu or switching applications
                Rectangle workingArea = ScreenPixelHelper.GetScreenWorkingArea();

                //We also want to make sure browser navigation buttons can be clicked comfortably
                int topBar = 105;

                if (gp.X < workingArea.Left || gp.X > workingArea.Right || gp.X < (workingArea.Top + topBar - 15) || gp.Y > workingArea.Bottom)
                {
                    winzoomAvailable = true;
                    return gp;
                }
                List<Box> nearby = new List<Box>();
                nearby.AddRange(getNearBoxes(gp));

                if (nearby.Count == 0)
                {
                    winzoomAvailable = true;
                    return gp;
                }
                if (nearby.Count == 1)//if no box around, we keep the cursor where it is
                {
                    //We clip to the center of the clickable element
                    winzoomAvailable = false;
                    ClippedBox = nearby[0];
                    return ClippedBox.center();
                }
                else
                {
                    if(!CheckAllBoxesLargeEnough(nearby))
                    {
                        winzoomAvailable = true;
                        ClippedBox = null;
                        return gp;
                    }

                    ClippedBox = findClosestBox(gp, nearby);//closest Box
                    winzoomAvailable = false;
                    return ClippedBox.center();
                }
            }
            else //box was previously clipped
            {

                /*
                 * if(no box around)
                 *      ruturn ClippedBox.center or (-100,-100)either exit or stay
                 * else 
                 *      if GP is in direction of anotherBox 
                 *         return  changeBox()
                 *      else 
                 *          either exit or stay
                 */


                //no other box around, //exit

                List<Box> otherNearby = getNearBoxes(gp);
                

                if (otherNearby.Contains(ClippedBox)) { otherNearby.Remove(ClippedBox); } //remove the clipped box as it is itself
                if (otherNearby.Count == 0)
                {
                    return StayOrExitBox(gp);
                }
                else
                {
                    if (!CheckAllBoxesLargeEnough(otherNearby))
                    {
                        //must use winzoom and exit since we've approached a really small box, force exit from box
                        winzoomAvailable = true;
                        ClippedBox = null;
                        return gp;
                    }

                    if (ClippedBox.ContainsPoint(gp))
                    {
                        return ClippedBox.center();
                    }

                    //Check if gp is contained by any of the otherNearby boxes, and if so is it at least changeBoxPercThresh into it
                    foreach (Box b in otherNearby)
                    {
                        //If it's a large box then a few pixels should be enough to enter it... and for a small box use a small percentage of it
                        if (b.ContainsPointPercentageWithin(ClippedBox, gp, ConfigManager.cursorMagnetClippingChangeBoxIntoPerc)
                            || b.ContainsPointPixelsWithin(ClippedBox, gp, ScreenPixelHelper.ConvertMmToPixels(ConfigManager.cursorMagnetClippingExitThresholdMm))
                            )
                        {
                            winzoomAvailable = false;
                            ClippedBox = b;
                            return b.center();
                        }
                    }

                    //the gazepoint has not met the criteria for changing to a different box, see if it should stay or exit
                    return StayOrExitBox(gp);

                }
            }
        }


        public Box findClosestBox(Point gp, List<Box> nearby)
        {
            int distance = 10000000;
            Box result = null;
            foreach (Box box in nearby)
            {
                int distanceComp = distanceToBox(gp, box);
                if (distanceComp < distance)
                {
                    distance = distanceComp;
                    result = box;
                }
            }
            return result;

        }

        public Point StayOrExitBox(Point gp)
        {
            //iterate through all the most recent raw gaze points and check that they all are at least thresholdPixels away
            int threshold = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.cursorMagnetClippingExitThresholdMm);

            List<Point> rawGazePoints = mc.WarpPointer.GetGazeHistory();
            if (rawGazePoints == null || rawGazePoints.Count == 0 || ClippedBox == null)
            {
                winzoomAvailable = true;
                return gp; //todo: out of bound?
            }

            int sampleRate = mc.WarpPointer.GetSampleRate();

            int recentDurationMs = ConfigManager.cursorMagnetClippingExitDurationMs; //todo: config
            int lookbackSampleCount = sampleRate * recentDurationMs / 1000;
            int startIndex = Math.Max(0, rawGazePoints.Count - lookbackSampleCount);

            //all points have to leave the box by at least exitThreshold amount in order to consider exit
            //equivalent to having 1 point not leave the box by exitThreshold to continue staying

            for (int i = startIndex; i < rawGazePoints.Count; i++)
            {
                //use the calibration adjusted value for the raw data
                Point adjustedGp = new Point(rawGazePoints[i].X, rawGazePoints[i].Y);
                adjustedGp.Offset(calibrationAdjuster.GetCalibrationAdjustment(adjustedGp));
                if (isNearBox(adjustedGp, ClippedBox, ScreenPixelHelper.ConvertMmToPixels(ConfigManager.cursorMagnetClippingExitThresholdMm)))
                {
                    //stay in box, since 1 point is still threshold/2 near the perimeter of the currently clipped box
                    return ClippedBox.center();
                }
            }

            //We went through the most recent gaze points and all of them are more than threshold away
            //Exit this box
            ClippedBox = null;
            winzoomAvailable = true;
            return gp; //todo: out of bound
        }

      

        public static int distanceToBox(Point gp, Box b)
        {
            int xclose = gp.X;
            xclose = (gp.X < b.left) ? b.left : xclose;
            xclose = (gp.X > b.right) ? b.right : xclose;

            int yclose = gp.Y;
            yclose = (gp.Y < b.top) ? b.top : yclose;
            yclose = (gp.Y > b.bottom) ? b.bottom : yclose;

            return PointHelper.GetPointDistance(gp, new Point(xclose, yclose));

        }

        public static bool isNearBox(Point gp, Box b, int threshold)
        {
            return distanceToBox(gp, b) < threshold;
        }
        
        //returns true if A is subset of B (aka B covers over all of A)
        public bool IsBoxSubset(Box a, Box b)
        {
            if (a.left >= b.left && a.right <= b.right && a.top >= b.top && a.bottom <= b.bottom)
                return true;

            return false;
        }

        void RemoveOverlappingDuplicateBoxes()
        {
            //Perform a pairwise comparison and determine if one box is a subset of another
            int i = 0;
            int j = 0;
            while (i < boxes.Count)
            {
                j = i + 1;
                while (j < boxes.Count)
                {
                    if (IsBoxSubset(boxes[i], boxes[j]))
                    {
                        //Remove box at i
                        boxes.RemoveAt(i);
                    }
                    else if (IsBoxSubset(boxes[j], boxes[i]))
                    {
                        boxes.RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }
                i++;
            }

        }
        

    }

}