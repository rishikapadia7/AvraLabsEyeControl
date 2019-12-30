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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrecisionGazeMouse
{
    public class ScreenPixelHelper
    {
        public enum SPI : uint
        {
            SPI_GETBEEP = 0x0001,
            SPI_GETWORKAREA = 0x0030
        }

        public enum SPIF
        {

            None = 0x00,
            SPIF_UPDATEINIFILE = 0x01,
            SPIF_SENDCHANGE = 0x02,
            SPIF_SENDWININICHANGE = 0x02
        }

        //Cannot move to User32.cs since it conflicts with another SystemParametersInfo declaration
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref RECT pvParam, SPIF fWinIni);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }


        public static Rectangle GetScreenSize()
        {
            //It is a requirement that the eyetracker runs on the primary display
            //Below code usually reports same as Screen.PrimaryScreen.Bounds 
            Size monitorSizePixels = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            /*Rectangle gdiRect = GetScreenSizeGdi(); //use gdi value if these 2 values do not match
            if(gdiRect.Height != monitorSizePixels.Height)
            {
                return gdiRect;
            }
            */
            return new Rectangle(new Point(0, 0), monitorSizePixels);
        }

        //This gets the actual screen resolution for the actual primary monitor - even if Windows internally may be screwed up
        public static Rectangle GetScreenSizeGdi()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            int screenWidth = Gdi32.GetDeviceCaps(desktop, (int)Gdi32.DeviceCap.DESKTOPHORZRES);
            int screenHeight = Gdi32.GetDeviceCaps(desktop, (int)Gdi32.DeviceCap.DESKTOPVERTRES);

            g.ReleaseHdc(desktop);

            return new Rectangle(0, 0, screenWidth, screenHeight);
        }

        public static Rectangle GetScreenWorkingArea()
        {
            //Below code is an alternative way, but leads to same value
            /*
            RECT rect = new RECT();
            SystemParametersInfo(SPI.SPI_GETWORKAREA, 0, ref rect, 0);

            return new Rectangle(0, 0, rect.right - rect.left, rect.bottom - rect.top);
            */

            return Screen.GetWorkingArea(new Point(0, 0));
        }

        public static Rectangle GetScreenSizeMm()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            int screenWidthInches = Gdi32.GetDeviceCaps(desktop, (int)Gdi32.DeviceCap.HORZSIZE);
            int screenHeightInches = Gdi32.GetDeviceCaps(desktop, (int)Gdi32.DeviceCap.VERTSIZE);
            g.ReleaseHdc(desktop);

            return new Rectangle(0, 0, screenWidthInches, screenHeightInches);
        }

        public static double GetRectHypotenuse(Rectangle rect)
        {
            return Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2));
        }

        public static double GetScreenPixelsPerMm()
        {
            return GetRectHypotenuse(GetScreenSize()) / GetRectHypotenuse(GetScreenSizeMm());
        }

        public static int ConvertMmToPixels(int mm)
        {
            return Math.Max((int)Math.Round(((double)mm * GetScreenPixelsPerMm())), 1);
        }

        public static int ConvertMmToPixels(double mm)
        {
            return Math.Max((int)Math.Round((mm * GetScreenPixelsPerMm())), 1);
        }

    }
}
