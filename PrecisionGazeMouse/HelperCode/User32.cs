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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;


namespace PrecisionGazeMouse
{

    /// <summary>
    /// Helper class containing User32 API functions
    /// </summary>
    /// 
    public class User32
    {
        //this function lets us tell windows that we changed the cursor
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);


        public const uint SPI_SETCURSORS = 0x0057;


        public static IntPtr GetCursorHandle()
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out pci);

            return pci.hCursor;
        }

        //h example is Cursors.WaitCursor.Handle
        public static bool IsCursorHandle(IntPtr h)
        {
            return GetCursorHandle() == h;
        }

        

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
                                        // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
                                        //    0             The cursor is hidden.
                                        //    CURSOR_SHOWING    The cursor is showing.
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern uint GetDoubleClickTime();

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPhysicalPoint(POINT p);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT p);

        [DllImport("user32.dll")]
        public static extern bool LogicalToPhysicalPoint(IntPtr hwnd, ref POINT p);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);


        //Mouse actions
        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;




        //constants taken from: http://pinvoke.net/default.aspx/Constants.GWL%20-%20GetWindowLong

        //static readonly int GWL_WNDPROC = -4;
        //static readonly int GWL_HINSTANCE = -6;
        //static readonly int GWL_HWNDPARENT = -8;
        //static readonly int GWL_STYLE = -16;
        static readonly int GWL_EXSTYLE = -20;
        //static readonly int GWL_USERDATA = -21;
        //static readonly int GWL_ID = -12;
        

        //#if(_WIN32_WINNT >= 0x0500)
        public const uint WS_EX_LAYERED = 0x00080000;
        //#endif /* _WIN32_WINNT >= 0x0500 */

        public const uint WS_EX_TRANSPARENT = 0x00000020;
        public const uint WS_EX_TOPMOST = 0x00000008;
        public const uint WS_EX_DISABLED = 0x08000000; 

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public static void SetFormTransparent(IntPtr Handle)
        {
            int oldWindowLong = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, Convert.ToInt32(oldWindowLong | WS_EX_TRANSPARENT | WS_EX_LAYERED));
        }

        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_SHOWWINDOW = 0x0040;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        //http://pinvoke.net/default.aspx/user32/GetKeyState.html
        public enum VirtualKeyStates
        {
            VK_LBUTTON = 0x01
        }

        [DllImport("USER32.dll")]
        public static extern short GetKeyState(VirtualKeyStates nVirtKey);



        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = User32.GetForegroundWindow();

            if (User32.GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        static string previousFgWindowTitle = "";
        static string previousFgWindowClassname = "";

        public static bool AppsInForeground(List<string> apps)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            StringBuilder Buff2 = new StringBuilder(nChars);

            IntPtr handle = User32.GetForegroundWindow();

            string fgWindowTitle = "";
            //Get foreground window title
            if (User32.GetWindowText(handle, Buff, nChars) > 0)
            {
                fgWindowTitle = String.Format("{0}", Buff.ToString().ToLower());
            }

            string fgWindowClassName = "";

            //Get foreground window classname
            if (User32.GetClassName(handle, Buff2, nChars) > 0)
            {
                fgWindowClassName = String.Format("{0}", Buff2.ToString().ToLower());
            }

            if(!fgWindowClassName.Equals(previousFgWindowClassname))
            {
                Logger.WriteVar(nameof(fgWindowClassName), fgWindowClassName);
                previousFgWindowClassname = fgWindowClassName;
            }

            if (!fgWindowTitle.Equals(previousFgWindowTitle))
            {
                Logger.WriteVar(nameof(fgWindowTitle), fgWindowTitle);
                previousFgWindowTitle = fgWindowTitle;
            }

            for (int i = 0; i < apps.Count; i++)
            {
                string appname = apps[i].ToLower();
                if(fgWindowTitle.Length > 0)
                {
                    if (fgWindowTitle.Contains(appname))
                    {
                        return true;
                    }
                }
                if (fgWindowClassName.Length > 0)
                {
                    if (fgWindowClassName.Contains(appname))
                    {
                        return true;
                    }
                }
            }

            //Foreground window is not one of the listed apps
            return false;
        }

        static IntPtr[] previousHandles = new IntPtr[2] { (IntPtr)0, (IntPtr)0 };

        public static bool GazeOnNonzoomboxApp(List<string> apps, System.Drawing.Point p)
        {
            User32.POINT gp;
            gp.x = p.X;
            gp.y = p.Y;

            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            StringBuilder Buff2 = new StringBuilder(nChars);

            IntPtr[] handles = new IntPtr[2];
            handles[0] = WindowFromPoint(gp);
            handles[1] = User32.GetForegroundWindow();

            for(int k = 0; k < handles.Length; k++)
            {
                IntPtr handle = handles[k];
                string windowTitle = "";

                //Get foreground window title
                if (User32.GetWindowText(handle, Buff, nChars) > 0)
                {
                    windowTitle = String.Format("{0}", Buff.ToString().ToLower().Trim());
                }
                if(!previousHandles[k].Equals(handles[k]))
                {
                    Logger.WriteVar(nameof(windowTitle) + "[" + k + "]", windowTitle);
                    previousHandles[k] = handles[k];
                }

                for (int i = 0; i < apps.Count; i++)
                {
                    string appname = apps[i].ToLower();
                    if (windowTitle.Length > 0)
                    {
                        if (windowTitle.Equals(appname))
                        {
                            return true;
                        }
                    }
                }
            }

            //Foreground window is not one of the listed apps
            return false;
        }

    }
}
