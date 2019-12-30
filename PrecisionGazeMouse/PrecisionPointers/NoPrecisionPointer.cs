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

namespace PrecisionGazeMouse.PrecisionPointers
{
    class NoPrecisionPointer : PrecisionPointer
    {

        public override String ToString()
        {
            return "";
        }

        public bool IsStarted()
        {
            return true;
        }

        public Point GetNextPoint(Point warpPoint)
        {
            return warpPoint;
        }

        public void warpOccurred()
        {

        }

        public void Dispose()
        {
        }
    }
}
