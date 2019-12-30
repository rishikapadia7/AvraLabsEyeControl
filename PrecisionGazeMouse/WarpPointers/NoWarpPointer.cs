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
using System.Collections.Generic;

namespace PrecisionGazeMouse.WarpPointers
{
    class NoWarpPointer : WarpPointer
    {
        Point warpPoint;

        public NoWarpPointer(Point centerOfScreen)
        {
            warpPoint = centerOfScreen;
        }

        public bool IsStarted()
        {
            return true;
        }

        public bool IsWarpReady()
        {
            return true;
        }

        public Point calculateSmoothedPoint()
        {
            return warpPoint;
        }

        public override String ToString()
        {
            return String.Format("({0:0}, {1:0})", warpPoint.X, warpPoint.Y);
        }

        public Point GetGazePoint()
        {
            return warpPoint;
        }

        public int GetSampleCount()
        {
            return 1;
        }

        public int GetWarpTreshold()
        {
            return 0;
        }

        public Point GetWarpPoint()
        {
            return warpPoint;
        }

        public Point GetNextPoint(Point currentPoint)
        {
            return warpPoint;
        }

        public void Dispose()
        {
        }

        public void RefreshTracking()
        {
        }

        public List<Point> GetGazeHistory()
        {
            return null;
        }

        public int GetSampleRate()
        {
            return 0;
        }
    }
}