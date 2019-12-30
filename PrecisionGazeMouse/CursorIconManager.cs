/*
 * Copyright 2019 Rishi Kapadia
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 * http://www.apache.org/licenses/LICENSE-2.0|http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

/*
 * When eye gaze mode is enabled, the mouse cursor is hidden by showing transparent icons.
 * When the user moves a physical mouse the mouse cursor icon
 * changes back to the system icons.
 */
namespace PrecisionGazeMouse
{
    //NOTE: if user uses their own customized mouse icons, they will not be usable
    //if they attempt to use this application.
    public class CursorIconManager
    {
        public enum CURSOR_ICON_SCHEME {
            EYEGAZE,
            EYEGAZE_ZOOM,
            MOUSE,
            COUNT
        };


        CURSOR_ICON_SCHEME currentScheme = CURSOR_ICON_SCHEME.MOUSE;

        const string hcFileStringBaseName = "cursors\\HiddenCursor_";

        const int pathKeysCount = 16;
        string[] pathKeys = new string[] {

            "Arrow", "Help", "AppStarting", //Standard arrow, arrow with question mark, arrow with hourglass whenever app is launched
            "Wait", "Crosshair", "IBeam",   //Hourglass, Precision selct cross, Text select icon "I"
            "NWPen", "No", "SizeNS", "SizeWE", //Show a pen sign, no-smoking looking symbol, vertical resize, horizontal resize
            "SizeNWSE", "SizeNESW",             //diagonal resize 1, diagonal resize 2
            "SizeAll", "UpArrow", "Hand", "" }; //Move/drag sign, Up arrow called alternate select, Hand/Link select, blank

        string[] eyegazeSchemeStrings = new String[] {
            hcFileStringBaseName, hcFileStringBaseName, "",
            "", hcFileStringBaseName, hcFileStringBaseName,
            "", "", "", "",
            "", "",
            "", "", hcFileStringBaseName, ""};

        //shows the I input symbol inside zoombox
        string[] eyegazeZoomSchemeStrings = new String[] {
            hcFileStringBaseName, hcFileStringBaseName, hcFileStringBaseName, 
            "", hcFileStringBaseName, hcFileStringBaseName,//hidden for ibeam so that custom overlay can be shown
            "", "", "", "",
            "", "",
            "", "", hcFileStringBaseName, ""}; //todo: experiment if we want to show hand

        string[] mouseSchemeStrings = new String[] {"", "", "",
            "", "", "", "", "", "", "",
            "", "", "", "", "", ""};

        string[][] schemeStringsList;

        public CursorIconManager()
        {
            schemeStringsList = new String[(int)CURSOR_ICON_SCHEME.COUNT][];
            schemeStringsList[(int)CURSOR_ICON_SCHEME.EYEGAZE] = eyegazeSchemeStrings;
            schemeStringsList[(int)CURSOR_ICON_SCHEME.EYEGAZE_ZOOM] = eyegazeZoomSchemeStrings;
            schemeStringsList[(int)CURSOR_ICON_SCHEME.MOUSE] = mouseSchemeStrings;
        }

        public void SetCursorScheme(CURSOR_ICON_SCHEME newScheme)
        {
            if(newScheme == currentScheme)
            {
                //Already set
                return;
            }

            //Do not change scheme from MOUSE to EYEGAZE_ZOOM, as not a valid transition
            if(currentScheme == CURSOR_ICON_SCHEME.MOUSE && newScheme == CURSOR_ICON_SCHEME.EYEGAZE_ZOOM )
            {
                return;
            }
            Logger.WriteVar(nameof(currentScheme), currentScheme);
            Logger.WriteVar(nameof(newScheme), newScheme);
            ApplyCurrentScheme(currentScheme, newScheme);
            currentScheme = newScheme;
        }

        private void ApplyCurrentScheme(CURSOR_ICON_SCHEME prevScheme, CURSOR_ICON_SCHEME newScheme)
        {
            List<int> indicesToChange = new List<int>();
            for(int i = 0; i < mouseSchemeStrings.Length; i++)
            {
                if(! schemeStringsList[(int) prevScheme][i].Equals(schemeStringsList[(int) newScheme][i]))
                {
                    indicesToChange.Add(i);
                }
            }

            using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey(@"Control Panel\Cursors", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                //if we couldn't open the registry, tell the user and exit
                if (regKey == null)
                {
                    Logger.WriteError("Unable to get write permissions to Current User\\Control Panel\\Cursors");
                    return;
                }

                //Show message box to user if icon cannot be found.
                if (!File.Exists(hcFileStringBaseName + "Arrow.cur"))
                {
                    Logger.WriteError("Unable to find " + hcFileStringBaseName + "Arrow.cur");
                }

                foreach(int i in indicesToChange)
                {
                    //Check if we should use hidden cursor here
                    if (schemeStringsList[(int)newScheme][i].Length > 0)
                    {
                        string executingDirectory = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        string hiddenCursorFullPath =  executingDirectory + "\\" + hcFileStringBaseName + pathKeys[i] + ".cur"; //the filename is named accordingly
                        regKey.SetValue(pathKeys[i], hiddenCursorFullPath, RegistryValueKind.String);
                    }
                    else //we want to set back to regular mouse scheme
                    {
                        regKey.SetValue(pathKeys[i], mouseSchemeStrings[i], RegistryValueKind.String);
                    }
                }
            }

            //tell windows we changed the cursor so it gets updated
            User32.SystemParametersInfo(User32.SPI_SETCURSORS, 0, IntPtr.Zero, 0);
        }
    }
}
