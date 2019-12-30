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

/* This file is unused in avra labs code. */

namespace PrecisionGazeMouse.PrecisionPointers
{

    public class HeadTranslation
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    /*
     * Using https://github.com/medsouz/Unity-TrackIR-Plugin-DLL
     */
    class TrackIRPrecisionPointer : PrecisionPointer
    {
        HeadTranslation trans;
        double sensitivity;
        bool started;

        //EyePositionStream stream;
        //FixationDataStream stream;

        int warpThreshold;

        public TrackIRPrecisionPointer(int warpThreshold, double sensitivity)
        {
            this.warpThreshold = warpThreshold;
            this.sensitivity = sensitivity;
            started = true;
        }

        public bool IsStarted()
        {
            return started;
        }

        public override String ToString()
        {
            if (trans == null)
            {
                return "";
            }
            else {
                return String.Format("({0:0}, {1:0})", trans.x, trans.y);
            }
        }

        public Point GetNextPoint(Point warpPoint)
        {
            System.Drawing.Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            trans = this.getTranslation();
            if (trans != null)
            {
                warpPoint.Offset((int)(trans.x * sensitivity), (int)(trans.y * sensitivity));
            }
            return warpPoint;
        }

        ~TrackIRPrecisionPointer()
        {
            Dispose();
        }

        public void warpOccurred()
        {

        }

        public void Dispose()
        {
            started = false;
        }

        HeadTranslation getTranslation()
        {
            HeadTranslation o = new HeadTranslation();
            //TODO get translation via eyes

            //Record the current head position (resets when a warp occurs)
            //Ensure movement exceeds initiation threshold (once per warp)


            //

            return o;
        }
    }
}