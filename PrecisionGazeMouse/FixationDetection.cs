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

/*
 * Given the gaze data from the eye tracker, this module attempts to determine if 
 * a fixation has occurred.
 */

namespace PrecisionGazeMouse
{
    public static class FixationDetection
    {
        static WarpPointers.WarpPointer warpPointer;

        public static void SetWarpPointer(WarpPointers.WarpPointer wPtr)
        {
            warpPointer = wPtr;
        }

        /*
            Returns true if:
                The speed of eye movement does not exceed (radiusMM / ConfigManager.fixationMinDurationMs) millimeters/seconds
                measured over a total duration of minMs.

            This function should be called when it's acceptable for the eyes to continue moving slowly and still count it as fixation.
        */
        public static bool IsMovingFixation(int minMs, int radiusMm)
        {
            if (warpPointer == null) return false;

            List<Point> rawGazePoints = warpPointer.GetGazeHistory();
            if (rawGazePoints == null || rawGazePoints.Count == 0)
            {
                return false;
            }

            int sampleRate = warpPointer.GetSampleRate();

            //total number of samples that should be meet the fixation width criteria
            int lookbackSampleCount = (int)Math.Max(1, sampleRate * minMs / 1000);

            if (lookbackSampleCount > rawGazePoints.Count)
            {
                return false;
            }

            //moving window samples amount
            int windowSampleCount = Math.Max(sampleRate * ConfigManager.fixationMinDurationMs / 1000, 1);
            int stopIndex = Math.Max(0, rawGazePoints.Count - windowSampleCount);

            //Get average of the first window
            //We start at the most recent point in history

            Point runningSum = new Point(0, 0);
            Point runningAvg = new Point(0, 0);

            for (int i = rawGazePoints.Count - 1; i >= stopIndex; i--)
            {
                runningSum.X += rawGazePoints[i].X;
                runningSum.Y += rawGazePoints[i].Y;
            }
            runningAvg.X = runningSum.X / windowSampleCount;
            runningAvg.Y = runningSum.Y / windowSampleCount;

            //Check that each point in the current window meets the fixation width criteria
            for (int i = rawGazePoints.Count - 1; i >= stopIndex; i--)
            {
                if (PointHelper.GetPointDistance(runningAvg, rawGazePoints[i]) > ScreenPixelHelper.ConvertMmToPixels(radiusMm))
                {
                    return false; //this point is too far from the avg
                }
            }

            //slide window until we complete lookbackSampleCount many
            stopIndex = Math.Max(0, rawGazePoints.Count - lookbackSampleCount);
            int lowerIndex = rawGazePoints.Count -1 - windowSampleCount;
            while(lowerIndex >= stopIndex)
            {
                int upperIndex = lowerIndex + windowSampleCount;
                //Remove one point from window
                runningSum.X -= rawGazePoints[upperIndex].X;
                runningSum.Y -= rawGazePoints[upperIndex].Y;

                //Add another point to the window
                runningSum.X += rawGazePoints[lowerIndex].X;
                runningSum.Y += rawGazePoints[lowerIndex].Y;
                
                //Recalculate avg
                runningAvg.X = runningSum.X / windowSampleCount;
                runningAvg.Y = runningSum.Y / windowSampleCount;

                //See if newest point is within the average
                if (PointHelper.GetPointDistance(runningAvg, rawGazePoints[lowerIndex]) > ScreenPixelHelper.ConvertMmToPixels(radiusMm))
                {
                    return false; //this point is too far from the avg
                }

                lowerIndex--;
            }

            //Been through lookbackSampleCount and all of them according to the moving window are within fixation width requirement
            return true;
        }

        //TODO: refactor these methods to same one and use enums

        /*
            Returns true if:
                All gazepoints for the past minMs duration are within radiusMm of the first gaze point in the observed set.

            This function should be called when you want to limit the fixation region to a certain millimeters within the
            gazepoint at minMs milliseconds ago.
        */
        public static bool IsStationaryFixation(int minMs, int radiusMm)
        {
            if (warpPointer == null) return false;

            List<Point> rawGazePoints = warpPointer.GetGazeHistory();
            if (rawGazePoints == null || rawGazePoints.Count == 0)
            {
                return false;
            }

            int sampleRate = warpPointer.GetSampleRate();

            //total number of samples that should be meet the fixation width criteria
            int lookbackSampleCount = (int) Math.Max(1, sampleRate * minMs / 1000);

            if (lookbackSampleCount > rawGazePoints.Count)
            {
                return false;
            }

            //Find the average of all the lookbackSamples ...
            int startIndex = Math.Max(0, rawGazePoints.Count - lookbackSampleCount);
            Point runningSum = new Point(0, 0);
            Point runningAvg = new Point(0, 0);

            for (int i = startIndex; i < rawGazePoints.Count; i++)
            {
                runningSum.X += rawGazePoints[i].X;
                runningSum.Y += rawGazePoints[i].Y;
            }
            runningAvg.X = runningSum.X / lookbackSampleCount;
            runningAvg.Y = runningSum.Y / lookbackSampleCount;

            //Check if distances between all points are within radius
            for (int i = startIndex + 1; i < rawGazePoints.Count; i++)
            {
                if (PointHelper.GetPointDistance(runningAvg, rawGazePoints[i]) > ScreenPixelHelper.ConvertMmToPixels(radiusMm))
                {
                    return false; //this point is too far from the first point
                }
            }
            return true;
        }
    }
}
