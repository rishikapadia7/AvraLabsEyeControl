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

namespace PrecisionGazeMouse.PrecisionPointers
{
    public interface PrecisionPointer : System.IDisposable
    {
        // Whether it's started tracking
        bool IsStarted();

        // Get the next precision point based on the warp point
        Point GetNextPoint(Point warpPoint);

        // Let precision know that a new warp has occurred
        void warpOccurred();
    }
}
