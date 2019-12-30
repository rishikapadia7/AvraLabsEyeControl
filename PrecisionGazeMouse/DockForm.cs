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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

/*
 * The dock for using eye-gaze only with no clicking device.
 * Currently coded so that it appears on the right side of the screen.
 */

namespace PrecisionGazeMouse
{
    public partial class DockForm : Form
    {
        public CursorOverlayForm cursorOverlayForm;

        //TODO: replace with the windows encodings that they use in Optikey keyboard
        public readonly string[] gazeButtonTextStrings = { "NONE", "Left Click", "Double Click", "Right Click", "Keyboard", "Up", "Down" };


        public enum GAZE_BUTTON_TYPE
        {
            NONE = 0,
            LEFTCLICK = 1,
            DOUBLECLICK = 2,
            RIGHTCLICK = 3,
            KEYBOARD = 4,
            UP = 5,
            DOWN = 6
        }

        private GAZE_BUTTON_TYPE _activeGazeButton = GAZE_BUTTON_TYPE.NONE;
        private readonly Object _activeGazeButtonLock = new object();

        public GAZE_BUTTON_TYPE activeGazeButton
        {
            get { lock (_activeGazeButtonLock) { return _activeGazeButton; } }
            set { lock (_activeGazeButtonLock) { _activeGazeButton = value; } }
        }

        public List<ButtonBase> gazeButtonsList = new List<ButtonBase>();

        public Color translucentWhite = Color.FromArgb(0x66, 0x66, 0x66); //off-white gray
        public Color DarkGray = Color.FromArgb(0x33, 0x33, 0x33);

        public DockForm()
        {
            InitializeComponent();
            activeGazeButton = GAZE_BUTTON_TYPE.NONE;
        }

        private void DockForm_Load(object sender, EventArgs e)
        {
            //1. Setup dock so that form window takes calculated user-amount of space on right side
            int dockWidth = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.dockFormMaxWidthMm);

            //Calculate dock height
            Rectangle wa = ScreenPixelHelper.GetScreenWorkingArea();
            int dockHeight = wa.Height;

            //calculate dock location
            Point dockLocation = new Point(wa.Right - dockWidth, wa.Top);
            this.Location = dockLocation;
            this.Size = new Size(dockWidth, dockHeight);
            this.BackColor = Color.Black;

            //Add all the buttons for the dock
            AddGazeButtons();
            SetActiveGazeButton(GAZE_BUTTON_TYPE.NONE);

            Logger.WriteMsg("About to register dockbar.");
            //Now let's dock the window
            RegisterDockbar();
        }

        public void DockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterDockbar();
        }

        public void RegisterDockbar()
        {
            if (!fBarRegistered)
            {
                RegisterUnregisterBar();
            }
        }
        public void UnregisterDockbar()
        {
            if (fBarRegistered)
            {
                RegisterUnregisterBar();
            }
        }

        private void AddGazeButtons()
        {
            //int buttonHeight = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.dockButtonHeightMm);

            int buttonHeightOffsetPx = ScreenPixelHelper.ConvertMmToPixels(5);
            int buttonHeight = (this.Height / gazeButtonTextStrings.Length) - buttonHeightOffsetPx;

            //Calculate vertical spacing between each of the buttons
            int heightStep = (this.Height - buttonHeight) / (gazeButtonTextStrings.Length - 1);

            for (int i = 0; i < gazeButtonTextStrings.Length; i++)
            {
                //create a new button
                Button b = new Button();
                b.AutoSize = false;
                b.Width = this.Width;
                //TODO: replace with just height
                b.Height = buttonHeight;
                //Location of the button is relative to the form
                b.Location = new Point(this.Width - b.Width, i * heightStep);
                b.Text = gazeButtonTextStrings[i];
                b.Font = new Font(FontFamily.GenericSansSerif, 14);
                b.TextAlign = ContentAlignment.MiddleRight;
                b.ForeColor = Color.White;
                b.BackColor = Color.Black;
                //remove border
                b.TabStop = false;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;

                //Apply padding to enlarge the "gazeable/clickable" area of the button
                //b.Padding = new Padding(buttonLeftPadding, buttonVerticalPadding, 0, buttonVerticalPadding);
                //b.Margin = new Padding(buttonLeftPadding, buttonVerticalPadding, 0, buttonVerticalPadding);

                b.Click += gazeButton_Click;
                //b.Paint += new System.Windows.Forms.PaintEventHandler(this.gazeButton_Paint);
                b.MouseEnter += gazeButton_MouseEnter;
                b.MouseLeave += gazeButton_MouseLeave;

                //TODO: ButtonBorderStyle.None
                gazeButtonsList.Add(b);
                this.Controls.Add(b);
            }
        }

        long keyboardSelectedTimestamp = 0;

        private void gazeButton_Click(object sender, EventArgs e)
        {
            Logger.WriteEvent();

            Button clickedButton = (Button)sender;
            GAZE_BUTTON_TYPE gb = GAZE_BUTTON_TYPE.NONE;

            if(clickedButton.Text.Equals(gazeButtonTextStrings[(int)GAZE_BUTTON_TYPE.UP]))
            {
                PrecisionGazeMouseForm.nextClickType = PrecisionGazeMouseForm.NEXTCLICKTYPE.SCROLLUP;
                gb = GAZE_BUTTON_TYPE.UP;
            }
            else if (clickedButton.Text.Equals(gazeButtonTextStrings[(int)GAZE_BUTTON_TYPE.DOWN]))
            {
                PrecisionGazeMouseForm.nextClickType = PrecisionGazeMouseForm.NEXTCLICKTYPE.SCROLLDOWN;
                gb = GAZE_BUTTON_TYPE.DOWN;
            }
            else if (clickedButton.Text.Equals(gazeButtonTextStrings[(int)GAZE_BUTTON_TYPE.LEFTCLICK]))
            {
                PrecisionGazeMouseForm.nextClickType = PrecisionGazeMouseForm.NEXTCLICKTYPE.LEFT;
                gb = GAZE_BUTTON_TYPE.LEFTCLICK;
            }
            else if (clickedButton.Text.Equals(gazeButtonTextStrings[(int)GAZE_BUTTON_TYPE.DOUBLECLICK]))
            {
                PrecisionGazeMouseForm.nextClickType = PrecisionGazeMouseForm.NEXTCLICKTYPE.DOUBLECLICK;
                gb = GAZE_BUTTON_TYPE.DOUBLECLICK;
            }
            else if (clickedButton.Text.Equals(gazeButtonTextStrings[(int)GAZE_BUTTON_TYPE.RIGHTCLICK]))
            {
                PrecisionGazeMouseForm.nextClickType = PrecisionGazeMouseForm.NEXTCLICKTYPE.RIGHT;
                gb = GAZE_BUTTON_TYPE.RIGHTCLICK;
            }
            else if (clickedButton.Text.Equals(gazeButtonTextStrings[(int)GAZE_BUTTON_TYPE.KEYBOARD]))
            {
                long currentTimestamp = PrecisionGazeMouseForm.GetCurrentTimestamp();
                long timestampDiff = currentTimestamp - keyboardSelectedTimestamp;

                //Want to give at least a second before it can be toggled again so that user has a chance to move their eyes away from the button
                if(timestampDiff > 1000)
                {
                    //We essentially toggle the keyboard
                    if (IpcServerData.showKeyboard == KEYBOARD_SHOWN_STATUS.NOT_SHOWN)
                    {
                        IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.SHOW_BOTTOM;
                        gb = GAZE_BUTTON_TYPE.KEYBOARD;
                    }
                    else //we hide the keyboard since it was already active
                    {
                        IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
                        gb = GAZE_BUTTON_TYPE.LEFTCLICK; //maintain left click status
                    }
                    keyboardSelectedTimestamp = currentTimestamp;
                }
            }
            else //default to NONE
            {
                PrecisionGazeMouseForm.nextClickType = PrecisionGazeMouseForm.NEXTCLICKTYPE.NONE;
            }

            Logger.WriteVar(nameof(gb), gb);
            SetActiveGazeButton(gb);
        }

        private void gazeButton_MouseEnter(object sender, EventArgs e)
        {
            //only cause gray background to change if near the right 2/3rds of the gaze button
            if (CursorMagnet.previousGazePoint.X > this.Left + (this.Width / 3))
            {
                Button b = (Button)sender;
                if (b != gazeButtonsList[(int)activeGazeButton])
                {
                    b.BackColor = DarkGray;
                }
            }
        }

        private void gazeButton_MouseLeave(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b != gazeButtonsList[(int)activeGazeButton])
            {
                b.BackColor = Color.Black;
            }
        }

        public void SetActiveGazeButton(GAZE_BUTTON_TYPE gb)
        {
            gazeButtonsList[(int)gb].BackColor = translucentWhite;

            if (gb != activeGazeButton)
            {
                gazeButtonsList[(int)activeGazeButton].BackColor = Color.Black;

                //set gb as active
                activeGazeButton = gb;
                gazeButtonsList[(int)activeGazeButton].Select();

                Logger.WriteVar(nameof(activeGazeButton), activeGazeButton);

                this.Invalidate(true);
            }
        }

        

        public bool AnyDockButtonContainsPoint(Point p)
        {
            if (p.X > this.Left + (this.Width / 3))
            {
                return ButtonHelper.GetButtonContainingPoint(gazeButtonsList, p) != null;
            }
            return false;
        }

        //This function gets called pretty much every precision gaze mouse form refresh
        public void ShowCursorOverlay(Point p)
        {
            if (activeGazeButton == GAZE_BUTTON_TYPE.DOWN || activeGazeButton == GAZE_BUTTON_TYPE.UP)
            {
                if (cursorOverlayForm == null || cursorOverlayForm.IsDisposed)
                {
                    Logger.WriteMsg("New Cursor Overlay Form being created.");
                    cursorOverlayForm = new CursorOverlayForm();
                }

                //Only update the overlay location if there is a change by more than 5 mm
                if (PointHelper.GetPointDistance(p, cursorOverlayForm.currentPoint) > ScreenPixelHelper.ConvertMmToPixels(5))
                {
                    cursorOverlayForm.currentPoint = p;

                    if (activeGazeButton == GAZE_BUTTON_TYPE.DOWN)
                    {
                        cursorOverlayForm.UpdateOverlayType(CursorOverlayForm.OVERLAY_TYPE.DOWNARROW);

                    }
                    else if (activeGazeButton == GAZE_BUTTON_TYPE.UP)
                    {
                        cursorOverlayForm.UpdateOverlayType(CursorOverlayForm.OVERLAY_TYPE.UPARROW);
                    }

                    if (!cursorOverlayForm.Visible)
                    {
                        Logger.WriteMsg("Cursor OverlyForm about to be shown.");
                        cursorOverlayForm.Show();
                    }
                    else
                    {
                        cursorOverlayForm.Invalidate();
                    }
                }
            }
            else
            {
                 HideCursorOverlayForm();
            }
        }

        public void HideCursorOverlayForm()
        {
            if (cursorOverlayForm != null && !cursorOverlayForm.IsDisposed && cursorOverlayForm.Visible)
            {
                Logger.WriteMsg("Cursor OverlyForm being hidden.");
                cursorOverlayForm.UpdateOverlayType(CursorOverlayForm.OVERLAY_TYPE.NONE);
                cursorOverlayForm.Hide();
            }
        }

        public void RefreshGazeButtonsWithGazePoint(Point gp)
        {
            if (ConfigManager.dockFixateToSelectEnabled)
            {
                //Check if the current gaze point is within the docked bar (right 2/3rds)
                if (gp.X > this.Left + (this.Width / 3))
                {
                    ButtonBase b = ButtonHelper.GetButtonContainingPoint(gazeButtonsList, gp);
                    if (b == null) return;

                    //if yes, then see if a moving fixation has occurred
                    if (FixationDetection.IsMovingFixation(ConfigManager.dockButtonFixationMs, 3))
                    {
                        //Set this button as active
                        gazeButton_Click(b, null);
                    }
                }
            }
        }
        
        //code from: https://www.codeproject.com/Articles/6741/AppBar-using-C
        #region APPBAR

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE = 1,
            ABM_QUERYPOS = 2,
            ABM_SETPOS = 3,
            ABM_GETSTATE = 4,
            ABM_GETTASKBARPOS = 5,
            ABM_ACTIVATE = 6,
            ABM_GETAUTOHIDEBAR = 7,
            ABM_SETAUTOHIDEBAR = 8,
            ABM_WINDOWPOSCHANGED = 9,
            ABM_SETSTATE = 10
        }

        enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        private bool fBarRegistered = false;

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        [DllImport("USER32")]
        static extern int GetSystemMetrics(int Index);
        [DllImport("User32.dll", ExactSpelling = true,
            CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool MoveWindow
            (IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern int RegisterWindowMessage(string msg);
        private int uCallBack;

        //If the Bar is registered, then it will get unregistered from the system
        private void RegisterUnregisterBar()
        {
            try
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = this.Handle;
                if (!fBarRegistered)
                {
                    uCallBack = RegisterWindowMessage("AppBarMessage");
                    abd.uCallbackMessage = uCallBack;

                    uint ret = SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
                    fBarRegistered = true;

                    ABSetPos();
                }
                else
                {
                    SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
                    fBarRegistered = false;
                }
            }
            catch(Exception ex)
            {
                Logger.WriteError(ex.ToString());
            }
        }

        //It is hardcoded to set to the right side
        private void ABSetPos()
        {
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            try
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(abd);
                abd.hWnd = this.Handle;
                abd.uEdge = (int)ABEdge.ABE_RIGHT;

                if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = screenSize.Height;
                    if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                    {
                        abd.rc.left = 0;
                        abd.rc.right = Size.Width;
                    }
                    else
                    {
                        abd.rc.right = screenSize.Width;
                        abd.rc.left = abd.rc.right - Size.Width;
                    }

                }
                else
                {
                    abd.rc.left = 0;
                    abd.rc.right = screenSize.Width;
                    if (abd.uEdge == (int)ABEdge.ABE_TOP)
                    {
                        abd.rc.top = 0;
                        abd.rc.bottom = Size.Height;
                    }
                    else
                    {
                        abd.rc.bottom = screenSize.Height;
                        abd.rc.top = abd.rc.bottom - Size.Height;
                    }
                }

                // Query the system for an approved size and position. 
                SHAppBarMessage((int)ABMsg.ABM_QUERYPOS, ref abd);

                // Adjust the rectangle, depending on the edge to which the 
                // appbar is anchored. 
                switch (abd.uEdge)
                {
                    case (int)ABEdge.ABE_LEFT:
                        abd.rc.right = abd.rc.left + Size.Width;
                        break;
                    case (int)ABEdge.ABE_RIGHT:
                        abd.rc.left = abd.rc.right - Size.Width;
                        break;
                    case (int)ABEdge.ABE_TOP:
                        abd.rc.bottom = abd.rc.top + Size.Height;
                        break;
                    case (int)ABEdge.ABE_BOTTOM:
                        abd.rc.top = abd.rc.bottom - Size.Height;
                        break;
                }

                // Pass the final bounding rectangle to the system. 
                SHAppBarMessage((int)ABMsg.ABM_SETPOS, ref abd);

                // Move and size the appbar so that it conforms to the 
                // bounding rectangle passed to the system. 
                MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top,
                    abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
            }
            catch(Exception ex)
            {
                Logger.WriteError(ex.ToString());
            }

        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == uCallBack)
            {
                switch (m.WParam.ToInt32())
                {
                    case (int)ABNotify.ABN_POSCHANGED:
                        ABSetPos();
                        break;
                }
            }

            try
            {
                base.WndProc(ref m);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e + "\n\n");
            }
            
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= (~0x00C00000); // WS_CAPTION
                cp.Style &= (~0x00800000); // WS_BORDER
                cp.ExStyle = 0x00000080 | 0x08000000; // WS_EX_TOOLWINDOW | WS_DISABLED
                return cp;
            }
        }


        #endregion

    }
}
