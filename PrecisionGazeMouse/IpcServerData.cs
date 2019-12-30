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

/*
 * The Windows interprocess communication channel is opened
 * and it exposes certain properties that are shared with the external
 * Optikey program. The optikey program can read the IpcSharedObject.
 */

namespace PrecisionGazeMouse
{


    public static class IpcServerData
    {
        public static DateTime lastModifiedServer = DateTime.Now;


        private static KEYBOARD_SHOWN_STATUS _showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
        public static KEYBOARD_SHOWN_STATUS showKeyboard
        {
            get { return _showKeyboard; }
            set
            {
                lastModifiedServer = DateTime.Now;
                _showKeyboard = value;
            }
        }

        public static System.Drawing.Rectangle keyboardRectangle = new System.Drawing.Rectangle(0,0,1000,500);


        private static bool _quitKeyboard = false;
        public static bool quitKeyboard
        {
            get { return _quitKeyboard; }
            set
            {
                lastModifiedServer = DateTime.Now;
                _quitKeyboard = value;
            }
        }

        public static int onscreenKeyWidthPx = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 25;
        public static int onscreenKeyHeightPx = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 25;

        public static int _dwellingKeySelectionMs = 400;
        public static int dwellingKeySelectionMs
        {
            get { return _dwellingKeySelectionMs; }
            set
            {
                lastModifiedServer = DateTime.Now;
                _dwellingKeySelectionMs = value;
            }
        }

        private static KEY_SELECTION_METHOD _keySelectionMethod = KEY_SELECTION_METHOD.CLICKING;
        public static KEY_SELECTION_METHOD keySelectionMethod
        {
            get { return _keySelectionMethod; }
            set { lastModifiedServer = DateTime.Now;
                _keySelectionMethod = value;
            }
        }

        private static bool _optikeyRequestsRestart = false;
        public static bool optikeyRequestsRestart
        {
            get { return _optikeyRequestsRestart; }
            set { _optikeyRequestsRestart = value; }
        }

        private static int _optikeyLockOnMs = 250; 
        public static int optikeyLockOnMs
        {
            get { return _optikeyLockOnMs; }
            set { _optikeyLockOnMs = value; }
        }

        private static bool _optikeyShowPie = false;
        public static bool optikeyShowPie
        {
            get { return _optikeyShowPie; }
            set { _optikeyShowPie = value; }
        }

        public static DateTime lastModifiedServerCalibration = DateTime.MinValue;
        public static System.Drawing.Point[] adjGrid;
        public static System.Drawing.Point[] adjGridOffset;
    }
}
