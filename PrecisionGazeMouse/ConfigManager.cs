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
using System.Xml;
using System.Xml.Serialization;
using System.Resources;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

/*
 * Contains advanced user settings and configurable constants
 * which affect the experience of the application.
 */
namespace PrecisionGazeMouse
{
    public static class ConfigManager
    {
        public static string xmlExportFileName = "configManagerExport.xml";

        static ConfigManager()
        {
            // Initialize.
            InitToDefaults();
        }

        //whether clicking will yield a zoombox or if the click will go straight onto the screen without zoombox
        public static bool zoomboxEnabled;

        //Note: Cursor Magnet is not currently used.
        public static bool cursorMagnetEnabled;

        //how close the gaze location must be to the clickable item to qualify clipping to it
        public static int cursorMagnetClippingThresholdMm;
        public static int cursorMagnetClippingExitThresholdMm;
        public static int cursorMagnetClippingExitDurationMs;
        public static int cursorMagnetClippingChangeBoxIntoPerc;
        public static int cursorMagnetClippingMinBoxWidthMm;
        
        //Suppose external mouse movement is detected, then we show the arrow cursor and we wait x milliseconds before
        //resuming eye gaze usage and making the cursor invisible again
        public static int mouseMovementEyeGazeResumeMilliseconds;

        //Suppose external keyboard movement is detected, number of milliseconds to wait before resuming eye gaze usage
        public static int keyboardEyeGazeResumeMilliseconds;

        //zoomboxSticky is dead-code now.
        public static bool zoomboxSticky;

        //percentage of the screen to occupy with the zoombox
        public static int zoomWindowPctScreen;

        //magnification percentage
        public static int zoomWindowMagnificationPct;

        //mostly a debug/experimental feature where it will draw a vertical and horizontal line across the screen indicating the current gaze point
        //location when doing a text selection on zoombox underlay form
        public static bool zoomboxGrid;

        //if enabled, when the user fixates inside the zoombox, the zoombox will increase it's magnification for greater zoom. Pixelation may result.
        public static bool zoomboxZoomAgain;
        public static int zoomboxZoomAgainAfterMs;

        //These are parameters used in classifying whether gaze movement correlates to a fixation or not
        public static int fixationRadiusClickAndHoldMm;
        public static int fixationMinDurationMs;
        public static int fixationDurationClickAndHoldZoombox;

        //when the start point and end point are just vertically separated, then the end point is covered up and then another zoombox shows up immediately 
        //at the wrong but same location because fixation is present there from before. 
        //Solution: add a min time from mousedown before fixation measurement begins occurring
        public static int fixationInitTimeClickAndHoldMs; //min time that must elapse before a zoombox can be shown even if fixation occurs
        public static int fixationDurationForHoverMs;
        public static int fixationRadiusForHoverMm;
        public static int fixationMinCursorActiveMs; //the amount of time the cursor stays after a fixation hover occurs, before going back to start menu location

        //when ON, the cursor follows gaze and hover behavior will show up all the time (may be annoying when reading an article with hyperlinks)
        public static bool cursorAlwaysFollowsGaze;
        //useful for determining if a click and hold can follow a fixation ... good for text selection
        public static bool fixationClickAndHoldEnabled;

        //whether cursor becomes invisible during eye gaze usage
        public static bool hideCursorDuringGaze;

        //a counter for how often a certain function is called in CursorMagnet.cs
        public static int precisionGazeMouseFormRefreshCount;

        //the amount of mm for eye gaze to move to disqualify previous gaze data from contributing to current smoothed gaze point location
        public static int largeSaccadeThresholdMm;

        //parameters used for shrinking circles calibration
        public static int calibrationCircleSizeMm;
        public static int calibrationIterationTimeMs;
        public static int calibrationIterationCountMax;
        public static int calibrationCompletedThresholdMm;

        public static bool automaticErrorNotification;

        //whether dockbar is intended to be shown
        public static bool dockEnabled;
        //determines if fixating good enough to select a dock button, or is a click necessary (derived from user settings form)
        public static bool dockFixateToSelectEnabled;
        public static int dockButtonFixationMs;
        public static int dockFormMaxWidthMm;
        public static int dockButtonHeightMm;

        //configures onscreen keyboard
        public static bool onscreenKeyboardEnabled;
        public static int onscreenKeyWidthMm;
        public static int onscreenKeyHeightMm;


        public static bool dwellingEnabled;
        public static int dwellingZoomboxInitMs;
        public static int dwellingKeySelectionMs;
        public static int dwellingKeyLockOnMs;

        public static int DWELL_ZOOMBOX_FAST, DWELL_ZOOMBOX_MEDIUM, DWELL_ZOOMBOX_SLOW;
        public static int DWELL_KEY_FAST, DWELL_KEY_MEDIUM, DWELL_KEY_SLOW;
        public static int DWELL_KEY_LOCK_ON_FAST, DWELL_KEY_LOCK_ON_MEDIUM, DWELL_KEY_LOCK_ON_SLOW;

        public static void InitToDefaults()
        {
            zoomboxEnabled = true;
            cursorMagnetEnabled = false;

            cursorMagnetClippingThresholdMm = 12;
            cursorMagnetClippingExitThresholdMm = 6;
            cursorMagnetClippingExitDurationMs = 300;
            cursorMagnetClippingChangeBoxIntoPerc = 10;
            cursorMagnetClippingMinBoxWidthMm = 13;

            mouseMovementEyeGazeResumeMilliseconds = 1500;
            keyboardEyeGazeResumeMilliseconds = 800;

            zoomboxSticky = false;

            zoomWindowPctScreen = 45;
            zoomWindowMagnificationPct = 350;
            zoomboxGrid = false;
            zoomboxZoomAgain = false;
            zoomboxZoomAgainAfterMs = 900;

            fixationMinDurationMs = 200;
            fixationRadiusClickAndHoldMm = 5;
            fixationDurationClickAndHoldZoombox = 400;
            fixationInitTimeClickAndHoldMs = 800;
            fixationRadiusForHoverMm = 2;
            fixationDurationForHoverMs = 5000; //basically do not follow
            fixationMinCursorActiveMs = 0; //basically do not follow

            cursorAlwaysFollowsGaze = true;
            fixationClickAndHoldEnabled = true;

            hideCursorDuringGaze = true;
            precisionGazeMouseFormRefreshCount = 0;
            largeSaccadeThresholdMm = 5;
            calibrationCircleSizeMm = 4;
            calibrationIterationTimeMs = 200;
            calibrationIterationCountMax = 3;
            calibrationCompletedThresholdMm = 19;

            automaticErrorNotification = true;

            dockEnabled = false;
            dockFixateToSelectEnabled = false;
            dockButtonFixationMs = 150;
            dockFormMaxWidthMm = 30;
            dockButtonHeightMm = 30;

            onscreenKeyboardEnabled = true;
            onscreenKeyWidthMm = 25;
            onscreenKeyHeightMm = 25;

            DWELL_ZOOMBOX_FAST = 700;
            DWELL_ZOOMBOX_MEDIUM = 1000;
            DWELL_ZOOMBOX_SLOW = 1500;

            //For optikey, the dwell times are added on top of the Lock on time
            /*
                FAST = 250 + 300
                MEDIUM = 350 + 500
                SLOW = 550 + 600
            */

            DWELL_KEY_LOCK_ON_FAST = 250;
            DWELL_KEY_LOCK_ON_MEDIUM = 350;
            DWELL_KEY_LOCK_ON_SLOW = 550;

            DWELL_KEY_FAST = 300;
            DWELL_KEY_MEDIUM = 500;
            DWELL_KEY_SLOW = 600;

            dwellingEnabled = false;
            dwellingZoomboxInitMs = DWELL_ZOOMBOX_MEDIUM;
            dwellingKeySelectionMs = DWELL_KEY_MEDIUM;
            dwellingKeyLockOnMs = DWELL_KEY_LOCK_ON_MEDIUM;
        }

    public static void SaveToDisk()
        {
            SerializeStatic.Save(typeof(ConfigManager), xmlExportFileName);
        }

        public static void LoadFromDisk()
        {
            InitToDefaults();
            if (File.Exists(xmlExportFileName))
            {
                try
                {
                    if (!SerializeStatic.Load(typeof(ConfigManager), xmlExportFileName))
                    {
                        RestoreDefaults();
                        Logger.WriteMsg("Lack of entries found in saved ConfigManagerExport, returned false;");
                    }
                }
                catch (Exception e)
                {
                    //using the defaults which were already set
                    Logger.WriteError("Unable to restore ConfigManager due to: " + e.ToString());
                }
            }
        }

        public static void RestoreDefaults()
        {
            Logger.WriteFunc(2);
            //Remove the xml exported file if it exists
            if(File.Exists(xmlExportFileName))
            {
                File.Delete(xmlExportFileName);
            }
            InitToDefaults();
        }

        
        
    }
}
