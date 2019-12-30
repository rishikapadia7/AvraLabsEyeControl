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
 
using System.Drawing;
using System.Collections.Generic;

namespace PrecisionGazeMouse.WarpPointers
{
    public interface WarpPointer : System.IDisposable
    {
        // Whether it's started tracking gaze
        bool IsStarted();

        // Whether it's ready to warp to a new point
        bool IsWarpReady();

        // Smoothed point for drawing
        Point calculateSmoothedPoint();

        // Gaze point for drawing
        Point GetGazePoint();

        // Sample count for printing
        int GetSampleCount();

        // Warp threshold in pixels
        int GetWarpTreshold();

        // Warp point for drawing, no update made
        Point GetWarpPoint();

        // Get the next warp point based on the current pointer location and gaze
        Point GetNextPoint(Point currentPoint);

        // Refresh the tracking buffer for a fresh start
        void RefreshTracking();

        List<Point> GetGazeHistory();
        int GetSampleRate();
    }
}
