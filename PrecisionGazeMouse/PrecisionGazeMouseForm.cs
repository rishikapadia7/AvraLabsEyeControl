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


using System.Windows.Forms;
using System;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using WindowsInput;
using System.Globalization;
using System.Threading.Tasks;

/*
 * This is the core logic of the application.
 * It has the main window application that you can use to click on the other items such as
 * calibrate, settings, etc.
 * It also has all the logic for handling mouse clicks, orchestrating the refresh of eye-gaze data
 * moving the mouse cursor, showing or hiding the dock, etc.
 */

namespace PrecisionGazeMouse
{
    public partial class PrecisionGazeMouseForm : Form
    {
        //NOTE: if you want to run optikey from visual studio, set this to TRUE!!!
        bool useVSOptikey = false;

        public enum MAG_SHOWN_STATUS
        {
            NOT_SHOWN,
            TO_BE_SHOWN,
            SHOWING,
            TO_DISAPPEAR
        };

        MouseController controller;
        GlobalKeyboardHook _globalKeyboardHook;
        Keys hotKey;

        CalibrationAdjuster calibrationAdjuster;
        CalibrationAdjusterAutoForm calibrationAdjusterAutoForm;
        CalibrationAdjusterManualForm calibrationAdjusterManualForm;
        CursorMagnet cursorMagnet;
        Winzoom wz;
        public CursorIconManager cursorIconManager;
        UserSettingsForm userSettingsForm;
        ConfigManagerForm configManagerForm; //remove when advancedSettingsFully integrated


        const bool useZoombox = false; //use winzoom instead


        //https://github.com/gmamaladze/globalmousekeyhook
        private IKeyboardMouseEvents m_GlobalHook;

        static uint refreshRate;
        static int timerIntervalMs;

        DateTime pauseTimeClickWait;
        DateTime pauseTimeMouseMovement;
        DateTime zoomboxTimeToStartDisappear;
        DateTime lastKeyboardPressTime;

        bool stickyZoombox = false;

        volatile MAG_SHOWN_STATUS magShownStatus;


        private readonly Object _mouseDownZoomboxShowingTimestampLock = new object();
        private long _mouseDownZoomboxShowingTimestamp = 0;

        long mouseDownZoomboxShowingTimestamp
        {
            get { lock (_mouseDownZoomboxShowingTimestampLock) { return this._mouseDownZoomboxShowingTimestamp; } }
            set { lock (_mouseDownZoomboxShowingTimestampLock) { this._mouseDownZoomboxShowingTimestamp = value; } }
        }


        System.Windows.Forms.Timer refreshTimer;

        public CursorHiderForm cursorHiderForm;


        private readonly Object _scrollWheelTimestampLock = new object();
        private long _scrollWheelTimestamp = 0;

        long scrollWheelTimestamp
        {
            get { lock (_scrollWheelTimestampLock) { return this._scrollWheelTimestamp; } }
            set { lock (_scrollWheelTimestampLock) { this._scrollWheelTimestamp = value; } }
        }

        bool zoomAlreadyBumped = false;
        int refreshScreenCount = 0;

        private readonly Object _zoomboxShownTimestampLock = new object();
        private long _zoomboxShownTimestamp = 0;

        long zoomboxShownTimestamp
        {
            get { lock (_zoomboxShownTimestampLock) { return this._zoomboxShownTimestamp; } }
            set { lock (_zoomboxShownTimestampLock) { this._zoomboxShownTimestamp = value; } }
        }

        public enum NEXTCLICKTYPE
        {
            NONE,
            LEFT,
            RIGHT,
            DOUBLECLICK,
            SCROLLUP,
            SCROLLDOWN
        }

        private static NEXTCLICKTYPE _nextClickType = NEXTCLICKTYPE.NONE;
        private static readonly Object _nextClickTypeLock = new object();
        private static long nextClickTypeTimestamp = 0;

        public static NEXTCLICKTYPE nextClickType
        {
            get { lock (_nextClickTypeLock) { return _nextClickType; } }
            set { lock (_nextClickTypeLock)
                {
                    _nextClickType = value;
                    Logger.WriteVar(nameof(nextClickType), value);
                    nextClickTypeTimestamp = GetCurrentTimestamp();
                }
            }
        }
        
        public bool trackingPaused = false;

        public static string optikeyWindowTitle = "optikey v2.11.0.0";

        //Shell_TrayWnd is start menu perhaps on older Windows

        //TODO: will need to implement a validation for "task view" as even if you hover over it's icon ... it becomes the handleFromPoint
        //Only foregroundWindow to be used for that
        //enum HANDLE_TO_USE { handleFromPoint, foregroundWindow, ANY, ALL }
        public static List<string> nonzoomboxApps = new List<string> { "Shell_TrayWnd", optikeyWindowTitle, "cortana", "start" };

        public DockForm dockForm;

        private IpcServerChannel channel;

        List<ButtonBase> buttonsList;


        private static readonly Object _calibrationInProgressLock = new object();
        private static bool _calibrationInProgress = false;

        public static bool calibrationInProgress
        {
            get { lock (_calibrationInProgressLock) { return _calibrationInProgress; } }
            set { lock (_calibrationInProgressLock) { _calibrationInProgress = value; } }
        }

        InputSimulator inputSimulator = new InputSimulator();

        public PrecisionGazeMouseForm()
        {
            InitializeComponent();
            magShownStatus = MAG_SHOWN_STATUS.NOT_SHOWN;

            // Set the default mode
            controller = new MouseController(this);
            controller.setMode(MouseController.Mode.EYEX_AND_SMARTNAV);
            controller.setMovement(MouseController.Movement.CONTINUOUS);
            hotKey = Keys.F3;

            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;

            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Tick += new EventHandler(RefreshScreen);
            refreshRate = 15;
            timerIntervalMs = Convert.ToInt32((double)1000 / (double)refreshRate);
            refreshTimer.Interval = timerIntervalMs;

            calibrationAdjuster = new CalibrationAdjuster();

            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            SubscribeGlobalMouseEventHandlers();

            pauseTimeMouseMovement = DateTime.Now.AddSeconds(-100); //set it in the past
            pauseTimeClickWait = DateTime.Now.AddSeconds(-100); //set it in the past
            lastKeyboardPressTime = DateTime.Now.AddSeconds(-100); //initialize to the past

            wz = new Winzoom(controller.WarpPointer, calibrationAdjuster);
            cursorMagnet = new CursorMagnet(controller, calibrationAdjuster);


            cursorIconManager = new CursorIconManager();
            FormClosed += new FormClosedEventHandler(PrecisionGameMouseForm_FormClosed);
            FormClosing += new FormClosingEventHandler(PrecisionGameMouseForm_FormClosing);

            Load += new EventHandler(PrecisionGameMouseForm_Load);
            FixationDetection.SetWarpPointer(controller.WarpPointer);

            //Instantiate our server channel.
            channel = new IpcServerChannel("ServerChannel");

            //Register the server channel.
            ChannelServices.RegisterChannel(channel, true);

            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                                        typeof(IpcSharedObject),
                                        "IpcSharedObject",
                                        WellKnownObjectMode.Singleton);

            //make this debug text invisible and cursor selectable only
            displayInfoTextbox.BackColor = this.BackColor;
            displayInfoTextbox.ForeColor = this.BackColor;
            displayInfoTextbox.BorderStyle = BorderStyle.None;
            displayInfoTextbox.ReadOnly = true;
            displayInfoTextbox.TabStop = false;
        }

        private void SubscribeGlobalMouseEventHandlers()
        {
            //Want to pause the cursor movement for a little bit so that a click can actually register
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.MouseUpExt += GlobalHookMouseUpExt;
            m_GlobalHook.MouseClick += GlobalMouseAction;
            m_GlobalHook.MouseDoubleClick += GlobalMouseAction;
            m_GlobalHook.MouseWheel += GlobalMouseAction;
            m_GlobalHook.MouseWheelExt += GlobalHook_MouseWheelExt;
        }

        private void UnsubscribeGlobalMouseEventHandlers()
        {
            //Want to pause the cursor movement for a little bit so that a click can actually register
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.MouseUpExt -= GlobalHookMouseUpExt;
            m_GlobalHook.MouseClick -= GlobalMouseAction;
            m_GlobalHook.MouseDoubleClick -= GlobalMouseAction;
            m_GlobalHook.MouseWheel -= GlobalMouseAction;
            m_GlobalHook.MouseWheelExt -= GlobalHook_MouseWheelExt;
        }

        public static long GetCurrentTimestamp()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private void GlobalHook_MouseWheelExt(object sender, MouseEventExtArgs e)
        {
            Logger.WriteEvent();
            scrollWheelTimestamp = GetCurrentTimestamp();
            Logger.WriteVar(nameof(scrollWheelTimestamp), scrollWheelTimestamp);

            if(IsCursorHiderFormActive())
            {
                cursorMagnet.setCursorPosition(CursorMagnet.previousGazePoint, false);
                DisableCursorHiderForm();
            }
        }

        void SetMagShownStatus(MAG_SHOWN_STATUS status)
        {
            if (magShownStatus == MAG_SHOWN_STATUS.NOT_SHOWN && status == MAG_SHOWN_STATUS.TO_DISAPPEAR)
            {
                return; //invalid state transition, already not shown
            }
            if (magShownStatus == MAG_SHOWN_STATUS.SHOWING && status == MAG_SHOWN_STATUS.TO_BE_SHOWN)
            {
                Logger.WriteMsg("WARNING: Mag_show_status already showing, but now going to set to TO_BE_SHOWN");
            }

            if (magShownStatus == MAG_SHOWN_STATUS.TO_BE_SHOWN && (status == MAG_SHOWN_STATUS.TO_DISAPPEAR))
            {
                Logger.WriteMsg("WARNING: Mag_show_status TO_BE_SHOWN, but now going to be TO_DISAPPEAR");
            }
            if (magShownStatus == MAG_SHOWN_STATUS.TO_BE_SHOWN && (status == MAG_SHOWN_STATUS.NOT_SHOWN))
            {
                Logger.WriteMsg("WARNING: Mag_show_status TO_BE_SHOWN, but now going to be NOT_SHOWN");
            }
            //Logger.WriteFunc(3);
            Logger.WriteVar(nameof(magShownStatus), magShownStatus);
            magShownStatus = status;
            Logger.WriteVar(nameof(magShownStatus), magShownStatus);
        }

        MAG_SHOWN_STATUS GetMagShownStatus()
        {
            return magShownStatus;
        }

        bool CheckMagShownStatus(MAG_SHOWN_STATUS status)
        {
            return magShownStatus == status;
        }

        bool ReadyForClickAndHoldZoombox(long timestamp)
        {
            if (!ConfigManager.fixationClickAndHoldEnabled || !ClickAndHoldZoomboxShowingInProgress() )
            {
                return false;
            }

            Logger.WriteVar(nameof(mouseDownZoomboxShowingTimestamp), mouseDownZoomboxShowingTimestamp);
            long timestampDiff = timestamp - mouseDownZoomboxShowingTimestamp;

            if (timestampDiff < User32.GetDoubleClickTime() || timestampDiff < ConfigManager.fixationInitTimeClickAndHoldMs
                || !FixationDetection.IsMovingFixation(ConfigManager.fixationDurationClickAndHoldZoombox, ConfigManager.fixationRadiusClickAndHoldMm)
                || !CheckMagShownStatus(MAG_SHOWN_STATUS.NOT_SHOWN))

            {
                return false;
            }

            return true;
        }

        void RunMagShownStatusLogic()
        {
            long timestamp = GetCurrentTimestamp();

            if (CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING))
            {
                if (!wz.wzuf.IsDisposed)
                {
                    wz.wzuf.Invalidate();
                }

                //Check if fixation occurred within the zoombox for long time, then bump up the magnification
                if (ConfigManager.zoomboxZoomAgain
                    && !zoomAlreadyBumped
                    && (timestamp > zoomboxShownTimestamp + ConfigManager.zoomboxZoomAgainAfterMs)
                    && FixationDetection.IsMovingFixation(ConfigManager.fixationDurationClickAndHoldZoombox, ConfigManager.fixationRadiusClickAndHoldMm))
                {
                    Point screenPoint = wz.ConvertToScreenPoint(CursorMagnet.previousGazePoint);
                    wz.Update(screenPoint, 300);
                    zoomAlreadyBumped = true;
                    Logger.WriteVar(nameof(zoomAlreadyBumped), zoomAlreadyBumped);
                }
            }

            if (ReadyForClickAndHoldZoombox(timestamp))
            {
                //A hold has occurred after a click inside the first zoombox.  Show another zoombox to establish endpoint.
                Logger.WriteMsg("CLICKANDHOLD: fixation detected and going to show zoombox.");
                SetMagShownStatus(MAG_SHOWN_STATUS.TO_BE_SHOWN);
            }


            if (CheckMagShownStatus(MAG_SHOWN_STATUS.TO_BE_SHOWN))
            {
                //Move the cursor immediately so that it does not accidentally create zoombox elsewhere
                if (IsCursorHiderFormActive())
                {
                    cursorMagnet.setCursorPosition(CursorMagnet.previousGazePoint, false);
                    DisableCursorHiderForm();
                }

                //below will set only if currntly already on Eyegaze cursor scheme.
                cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.EYEGAZE_ZOOM);
                wz.Show(CursorMagnet.previousGazePoint);
                zoomboxShownTimestamp = timestamp;
                SetMagShownStatus(MAG_SHOWN_STATUS.SHOWING);
            }

            if (CheckMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR))
            {
                //uint doubleClickTime = GetDoubleClickTime();
                if (System.DateTime.Now.CompareTo(zoomboxTimeToStartDisappear.AddMilliseconds(200)) > 0)
                {
                    wz.Hide();
                    if (zoomAlreadyBumped)
                    {
                        zoomAlreadyBumped = false;
                        Logger.WriteVar(nameof(zoomAlreadyBumped), zoomAlreadyBumped);
                    }
                    SetMagShownStatus(MAG_SHOWN_STATUS.NOT_SHOWN);
                }
            }
        }

        void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            Logger.WriteEvent();

            if (e.KeyboardData.VirtualCode == (int)hotKey)
            {
                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
                {
                    controller.HotKeyDown();
                }
                else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp)
                {
                    controller.HotKeyUp();
                }

                // don't type the hot key 
                e.Handled = true;
            }

            //if a non-system key is pressed, we count it as keyboarding
            if (e.KeyboardState != GlobalKeyboardHook.KeyboardState.SysKeyDown 
                && e.KeyboardState != GlobalKeyboardHook.KeyboardState.SysKeyUp)
            {
                lastKeyboardPressTime = DateTime.Now;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                _globalKeyboardHook?.Dispose();
            }
            base.Dispose(disposing);
        }

       
        private void RefreshScreen(Object o, EventArgs e)
        {
            refreshScreenCount++;

            if (!controller.UpdateMouse(Cursor.Position))
            {
                //Check for external mouse movement which would show the cursor icon again
                externalMouseMovementInProgress();
            }
            this.Invalidate();

            if (calibrationAdjusterAutoForm != null)
            {
                calibrationAdjusterAutoForm.Invalidate();
            }

            if (calibrationAdjusterManualForm != null)
            {
                calibrationAdjusterManualForm.Invalidate();
            }

            if(ConfigManager.dwellingEnabled)
            {
                DetectDwelling();
            }

            RunMagShownStatusLogic();

            if(IpcServerData.optikeyRequestsRestart)
            {
                IpcServerData.optikeyRequestsRestart = false;
                StartOrRestartOptikey();
            }

        }

        //Returns whether the left mouse button is currently held down
        private bool MouseLeftButtonIsDown()
        {
            return User32.GetKeyState(User32.VirtualKeyStates.VK_LBUTTON) < 0;
        }

        private bool ClickAndHoldZoomboxShowingInProgress()
        {
            if(mouseDownZoomboxShowingTimestamp != 0 && !MouseLeftButtonIsDown())
            {
                mouseDownZoomboxShowingTimestamp = 0;
                return false;
            }

            long timestampDiff = GetCurrentTimestamp() - mouseDownZoomboxShowingTimestamp;
            return mouseDownZoomboxShowingTimestamp != 0 && timestampDiff > User32.GetDoubleClickTime();
        }

        private bool IsCursorHiderFormActive()
        {
            return cursorHiderForm != null && !cursorHiderForm.IsDisposed;
        }

        private void DisableCursorHiderForm()
        {
            if (IsCursorHiderFormActive())
            {
                Logger.WriteMsg("Disabled cursor hider form.");
                cursorHiderForm.Close();
            }
        }

        public bool AnyFormContainsFocus()
        {
            foreach(Form f in Application.OpenForms)
            {
                if (f.ContainsFocus) { return true; }
            }
            return false;
        }

        private void EnableCursorHiderForm()
        {
            //Close a previous cursor hider session if it is already open
            if (cursorHiderForm != null && !cursorHiderForm.IsDisposed)
            {
                cursorHiderForm.Close();
            }

            //If the scroll wheel is in progress, we want to keep the cursor where it is for continued scrolling
            if(ScrollWheelIsInProgress() || ClickAndHoldZoomboxShowingInProgress() || AnyFormContainsFocus())
            {
                //do nothing, don't move the cursor to the default location
            }
            else
            {
                cursorHiderForm = new CursorHiderForm(cursorMagnet);
                cursorHiderForm.Show();
                Logger.WriteMsg("cursorHiderForm enabled.");
            }
        }

        private bool ScrollWheelIsInProgress()
        {
            long timestamp = GetCurrentTimestamp();
            return scrollWheelTimestamp != 0 && timestamp - scrollWheelTimestamp < 700;
        }

        public bool OptikeyIsShowing()
        {
            return IpcServerData.showKeyboard != KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
        }

        string[] optikeyPaths = new string[] {
            "..\\..\\..\\..\\OptiKey-master\\src\\JuliusSweetland.OptiKey\\bin\\Release\\OptiKey.exe",
            "..\\onscreenKeyboard\\OptiKey.exe",
            "..\\onScreenKeyboard\\OptiKey.exe",
            "..\\OptiKey\\OptiKey.exe",
            "..\\optikey\\OptiKey.exe"
        };

        string currentOptikeyPath = "";

        public void KillOptikey()
        {
            string[] optikeyNames = new string[] { "OptiKey", "OptiKey (32-bit)", "OptiKey (64-bit)", "OptiKey.exe" };
            foreach(string name in optikeyNames)
            {
                foreach (var process in Process.GetProcessesByName(name))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception e)
                    {
                        //can't do anything about it really
                    }
                    
                }
            }
        }

        public void StartOrRestartOptikey()
        {
            //Find optikey path
            foreach (string path in optikeyPaths)
            {
                if(File.Exists(path))
                {
                    currentOptikeyPath = path; //found the correct optikey path
                    break;
                }
            }

            if(currentOptikeyPath.Length > 0)
            {
                KillOptikey();
                Process.Start(currentOptikeyPath);
            }
        }

        int externalClickingProgressCount = 0;
        public bool clickingInProgress()
        {
            if(OptikeyIsShowing())
            {
                //It's a single click and no double clicking necessary
                return false;
            }
            else
            {
                uint doubleClickTime = User32.GetDoubleClickTime();

                if (System.DateTime.Now.CompareTo(pauseTimeClickWait.AddMilliseconds(doubleClickTime)) > 0)
                {
                    return false;
                }
                else
                {
                    externalClickingProgressCount++;
                    return true;
                }
            }
        }

        public bool externalKeyboardInProgress()
        {
            if(OptikeyIsShowing())
            {
                return false;
            }
            else if(System.DateTime.Now.CompareTo(lastKeyboardPressTime.AddMilliseconds(ConfigManager.keyboardEyeGazeResumeMilliseconds)) <= 0)
            {
                return true;
            }
            return false;
        }

        int externalMouseCount = 0;
        int externalMouseDuration = 0;
        bool sameExternalMovementSession = false;
        public bool externalMouseMovementInProgress()
        {
            Point prev = cursorMagnet.previousCursorPosition;
            Point cur = Cursor.Position;

            int xDiff = Math.Abs(cur.X - prev.X);
            int yDiff = Math.Abs(cur.Y - prev.Y);

            //See if cursor position has changed (there is a bug with trackpads that even when not using it somehow shifts the cursor slightly)
            //Therefore any noticeable mouse movement is sufficient here
            if (xDiff > 7 || yDiff > 7)
            {
                //Start the pause time for mouse movement
                cursorMagnet.previousCursorPosition = cur;
                pauseTimeMouseMovement = System.DateTime.Now;

                //Debug.WriteLine("FIRST: xdiff = " + xDiff + "; yDiff = " + yDiff);

                externalMouseCount++;

                //We want the mouse movement to continue from the gaze location rather than start menu location
                //It should not be the same external mouse movement session
                
                if (!sameExternalMovementSession)
                {
                    if (IsCursorHiderFormActive())
                    {
                        cursorMagnet.setCursorPosition(CursorMagnet.previousGazePoint, false);
                        DisableCursorHiderForm();
                    }
                    sameExternalMovementSession = true;
                    Logger.WriteVar(nameof(sameExternalMovementSession), sameExternalMovementSession);
                }


                //disable optikey
                IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;

                //Also show the default cursor icons that come with Windows
                cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.MOUSE);
                return true;
            }
            else if (System.DateTime.Now.CompareTo(pauseTimeMouseMovement.AddMilliseconds(ConfigManager.mouseMovementEyeGazeResumeMilliseconds)) <= 0)
            {
                //still waiting for the pause time to complete
                externalMouseDuration++;
                //Debug.WriteLine("PAUSED: xdiff = " + xDiff + "; yDiff = " + yDiff);

                if(MouseLeftButtonIsDown()) //extend the duration of external mouse movement when the left mouse button is held down
                {
                    pauseTimeMouseMovement = DateTime.Now;
                }

                SetMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR);
                return true;
            }
            else
            {
                //We can use eye gaze
                if (ConfigManager.hideCursorDuringGaze)
                {
                    if(CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING))
                    {
                        cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.EYEGAZE_ZOOM);
                    }
                    else
                    {
                        cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.EYEGAZE);
                    }
                }
                else
                {
                    cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.MOUSE);
                }

                if (sameExternalMovementSession != false)
                {
                    sameExternalMovementSession = false;
                    Logger.WriteVar(nameof(sameExternalMovementSession), sameExternalMovementSession);
                }
                
                return false;
            }
        }

        long lastFixationTimestamp = 0;

        public bool CursorAllowedToFollowGaze()
        {
            if(ConfigManager.cursorAlwaysFollowsGaze || wz.Active())
            {
                return true;
            }
           
            long timestamp = GetCurrentTimestamp();
            long timestampDelta = timestamp - lastFixationTimestamp;
            //if a fixation had recently occurred, keep the cursor active
            if (timestampDelta < ConfigManager.fixationMinCursorActiveMs)
            {
                //Logger.WriteMsg("CursorMovement already allowed.");
                return true;
            }
            //currently there is no init time, and we just allow cursor movement as soon as the fixation is detected
            else if (FixationDetection.IsStationaryFixation(ConfigManager.fixationDurationForHoverMs, ConfigManager.fixationRadiusForHoverMm))
            {
                lastFixationTimestamp = timestamp;
                Logger.WriteMsg("CursorMovement new fixation timestamp =" + lastFixationTimestamp);
                return true;
            }
            else
            {
                //Logger.WriteMsg("CursorMovement not allowed since no fixation of duration " + ConfigManager.fixationDurationForHoverMs + 
                //    " and radius of " + ConfigManager.fixationRadiusForHoverMm);
                return false;
            }
            
        }

        public bool IsGazeInUpperHalfOfScreen()
        {
            Rectangle screenSize = ScreenPixelHelper.GetScreenSize();
            Point gp = controller.WarpPointer.GetGazePoint();
            return gp.Y < screenSize.Height / 2;
        }

        int setMousePositionCalls = 0;
        int externalCount = 0;
        int onlyZoomboxCount = 0;
        int zoomboxInsideCount = 0;

        public void SetMousePosition(Point p)
        {
            setMousePositionCalls++;
            //Apply calibration adjustment offset to the point p
            p.Offset(calibrationAdjuster.GetCalibrationAdjustment(p));
            p = controller.limitToScreenBounds(p);
            cursorMagnet.setGazePoint(p);
            User32.POINT pt;
            pt.x = p.X;
            pt.y = p.Y;

            if (ConfigManager.dockEnabled && dockForm != null && !dockForm.IsDisposed)
            {
                dockForm.Enabled = !calibrationInProgress; //if calibration is in progress, disable dock from from receiving input
                if(dockForm.Enabled)
                {
                    dockForm.RefreshGazeButtonsWithGazePoint(p);
                    dockForm.ShowCursorOverlay(p);
                }
                else
                {
                    dockForm.HideCursorOverlayForm();
                }
            }

            //if (externalMouseMovementInProgress() || clickingInProgress() || !zb.Active())
            if (externalMouseMovementInProgress() || clickingInProgress() || externalKeyboardInProgress())
            {
                //don't set mouse position
                externalCount++;
                return;
            }
            else if (!CursorAllowedToFollowGaze()
                && CheckMagShownStatus(MAG_SHOWN_STATUS.NOT_SHOWN)
                && !ScrollWheelIsInProgress()
                && !ClickAndHoldZoomboxShowingInProgress()
                && !CurrentlyOnNonzoomableApp())
            {
                onlyZoomboxCount++;

                Point currentPosition = Cursor.Position;
                Point target = CursorHiderForm.GetStartMenuLocation();


                int xDiff = Math.Abs(currentPosition.X - target.X);
                int yDiff = Math.Abs(currentPosition.Y - target.Y);

                //As long as the current gaze position is more than 3 pixels away from start menu, we hide the cursor
                //Cursor is thus only shown when it is allowed to be shown
                if (xDiff > 3 || yDiff > 3)
                {
                    if(!IsCursorHiderFormActive())
                    {
                        Logger.WriteMsg("About to hide cursor to start menu.");
                        EnableCursorHiderForm();
                    }
                }

                return;
            }
            else
            {
                if (IsCursorHiderFormActive())
                {
                    DisableCursorHiderForm();
                }
                zoomboxInsideCount++;
                //Let's see if current gaze point is within zoombox
                if (CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING) && wz.WithinRect(p, Winzoom.ZOOMBOX_RECT_T.PADDED))
                {
                    //compensate for weird icon padding bug, where at 100% it would be off by 8 pixels
                    Point compensation = new Point(8 * ConfigManager.zoomWindowMagnificationPct / 100, 8 * ConfigManager.zoomWindowMagnificationPct / 100);
                    Point pOld = new Point(p.X, p.Y);
                    p.Offset(compensation);
                    p = wz.ConvertToScreenPoint(pOld);
                    cursorMagnet.setCursorPosition(p, CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING));
                }
                else //the general case
                {
                    //Logger.WriteMsg("About to set mouse position to p=" + p);
                    cursorMagnet.setCursorPosition(p, CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING));
                    
                    if (!stickyZoombox)
                    {
                        //if user looks away from zoombox, then it disappears
                        if (CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING))
                        {
                            SetMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR);
                            //also make the next click type none to cancel
                            nextClickType = NEXTCLICKTYPE.NONE;
                            if(dockForm != null)
                            {
                                dockForm.SetActiveGazeButton(DockForm.GAZE_BUTTON_TYPE.NONE);
                            }
                        }
                    }

                }

            }

        }

        

        protected override void OnPaint(PaintEventArgs e)
        {

        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void ModeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controller == null)
                return;

            System.Windows.Forms.ComboBox box = (System.Windows.Forms.ComboBox)sender;
            switch ((String)box.SelectedItem)
            {
                case "EyeX and TrackIR":
                    controller.setMode(MouseController.Mode.EYEX_AND_TRACKIR);
                    break;
                case "EyeX and SmartNav":
                    controller.setMode(MouseController.Mode.EYEX_AND_SMARTNAV);
                    break;
                case "EyeX Only":
                    controller.setMode(MouseController.Mode.EYEX_ONLY);
                    break;
                case "TrackIR Only":
                    controller.setMode(MouseController.Mode.TRACKIR_ONLY);
                    break;
                default:
                    break;
            }
        }

        private void ContinuousButton_Click(object sender, EventArgs e)
        {
            controller.setMovement(MouseController.Movement.CONTINUOUS);
        }



        public bool CurrentlyOnNonzoomableApp()
        {
            Point prevGp = CursorMagnet.previousGazePoint;
            //If dockbar is open, see if gaze point is located within a dock button
            if (ConfigManager.dockEnabled && dockForm != null && !dockForm.IsDisposed)
            {
                if(dockForm.AnyDockButtonContainsPoint(prevGp))
                {
                    return true;
                }
            }

            if(this.ContainsFocus)
            {
                if(ButtonHelper.GetButtonContainingPoint(buttonsList, CursorMagnet.previousGazePoint) != null)
                {
                    return true;
                }
            }

            if (userSettingsForm != null && !userSettingsForm.IsDisposed)
            {
                if (userSettingsForm.AnyButtonContainsPoint(prevGp))
                {
                    return true;
                }
            }

            return User32.GazeOnNonzoomboxApp(nonzoomboxApps, prevGp);
        }

        public bool ZoomboxAllowedToBeShown()
        {
            if(!ConfigManager.zoomboxEnabled || externalMouseMovementInProgress() || wz.Active() 
                || clickingInProgress() || CheckMagShownStatus(MAG_SHOWN_STATUS.TO_BE_SHOWN) 
                || !cursorMagnet.winzoomAvailable || 
                nextClickType == NEXTCLICKTYPE.SCROLLDOWN || nextClickType == NEXTCLICKTYPE.SCROLLUP)
            {
                return false;
            }

            //Also want to see if one of the non-zoomable applications are open (by title or classname)
            //If they are, then return false so that zoombox is not allowed to be shown
            
            return !CurrentlyOnNonzoomableApp();
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Logger.WriteEvent();

            long timestamp = GetCurrentTimestamp();

            if (CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING))
            {
                //re-enable when single switch is actually supported
                /*
                if(nextClickType != NEXTCLICKTYPE.LEFT)
                {
                    //Check if the system is operating in single-mouse left click mode, and if a right-click is intended
                    PerformCorrectMouseClickType(e);
                }
                else
                {
                */
                    mouseDownZoomboxShowingTimestamp = timestamp;

                    //If it's already showing then we should make it disappear on this click
                    SetMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR);
                    Logger.WriteMsg("MouseDown: status set to disappear since already showing. mouseDownZoomboxShowingTimestamp = " + mouseDownZoomboxShowingTimestamp);
                //}

            }
            else
            {
                if(IsCursorHiderFormActive())
                {
                    cursorMagnet.setCursorPosition(CursorMagnet.previousGazePoint, false);
                    DisableCursorHiderForm();
                }
                mouseDownZoomboxShowingTimestamp = 0; //reset since another click has occurred
                Logger.WriteVar(nameof(mouseDownZoomboxShowingTimestamp), mouseDownZoomboxShowingTimestamp);
            }

            //We want to extend the time for zoombox disappearing if a double click is in progress
            if (CheckMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR))
            {
                zoomboxTimeToStartDisappear = DateTime.Now;
            }

            //If a click occurs when zb/wz is active and the original gaze point is not within the padded rectangle
            //then suppress the click, and instead close the zoombox

            if (ConfigManager.zoomboxSticky && wz.Active() && !wz.WithinRect(CursorMagnet.previousGazePoint, Winzoom.ZOOMBOX_RECT_T.PADDED))
            {
                e.Handled = true;
                SetMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR);

                if (ConfigManager.zoomboxSticky)
                    Logger.WriteMsg(timestamp + " MouseDown suppressed: status set to disappear.");
            }
            else
            {
                if (ConfigManager.zoomboxSticky)
                {
                    Logger.WriteMsg(timestamp + " Mousedown not suppressed since outside active area, with magstatus of " + magShownStatus);
                    pauseTimeClickWait = DateTime.Now;
                }
            }

            if (ZoomboxAllowedToBeShown())
            {
                //supress the mouse click and show the zoombox
                e.Handled = true;
                SetMagShownStatus(MAG_SHOWN_STATUS.TO_BE_SHOWN);
                Logger.WriteMsg(timestamp + " Mousedown suppressed, with magstatus of " + magShownStatus);
            }
            else
            {
                pauseTimeClickWait = DateTime.Now;
                Logger.WriteMsg(timestamp + " Mousedown not suppressed, with magstatus of " + magShownStatus);
            }

            
            //If Zoombox is disabled or currently on a non-zoomable app, and we want to use the dock click type
            //Note: the right click will only come here IF on a zoombox is not allowed to be shown
            //SCROLL UP and DOWN will always come here and not to the one inside zoomboxIsShowing
            /*if(ConfigManager.dockEnabled && (!ConfigManager.zoomboxEnabled || CurrentlyOnNonzoomableApp() ||
                nextClickType == NEXTCLICKTYPE.SCROLLDOWN || nextClickType == NEXTCLICKTYPE.SCROLLUP))
            {
                PerformCorrectMouseClickType(e);
            }
            */
        }

        private void GlobalHookMouseUpExt(object sender, MouseEventExtArgs e)
        {
            Logger.WriteEvent();

            //if the e.Handled = true in the mouse down, then the corresponding mouse-up is never called - thus below is not a problem
            if (CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING))
            {
                SetMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR);
                zoomboxTimeToStartDisappear = DateTime.Now;

                //Want to pause eye tracking for some ms if a click occurs so that mouse movement does not occur immediately after
                pauseTimeClickWait = DateTime.Now;

                Logger.WriteMsg("MouseUp: status set to disappear.");
            }
            else
            {
                pauseTimeClickWait = DateTime.Now;
                Logger.WriteMsg("Mouseup pauseTimeClickWait Time extended with magshownStatus of " + magShownStatus);
            }
            //only going to do this in GlobalMouseAction()
            //mouseDownZoomboxShowingTimestamp = 0; //the click and "hold" has completed and thus no subsequent zoombox should be shown
            //Logger.WriteVar(nameof(mouseDownZoomboxShowingTimestamp), mouseDownZoomboxShowingTimestamp);
        }

        private void GlobalMouseAction(Object sender, MouseEventArgs e)
        {
            Logger.WriteEvent();
            cursorMagnet.scrollClickHappened = true;
            Logger.WriteVar(nameof(cursorMagnet.scrollClickHappened), cursorMagnet.scrollClickHappened);

            if(e.Button == MouseButtons.Left)
            {
                //Update on-screen keyboard visibility state if applicable
                UpdateOnscreenKeyboardVisibility();
            }

            mouseDownZoomboxShowingTimestamp = 0; //the click and "hold" has completed and thus no subsequent zoombox should be shown
            Logger.WriteVar(nameof(mouseDownZoomboxShowingTimestamp), mouseDownZoomboxShowingTimestamp);
        }

        public void UpdateOnscreenKeyboardVisibility()
        {
            if (!ConfigManager.onscreenKeyboardEnabled || trackingPaused)
            {
                if(IpcServerData.showKeyboard != KEYBOARD_SHOWN_STATUS.NOT_SHOWN)
                {
                    IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
                    Logger.WriteVar("IpcServerData.showKeyboard", IpcServerData.showKeyboard);
                }
                return;
            }
               
            bool currentClickOnIbeam = User32.IsCursorHandle(Cursors.IBeam.Handle);

            //if the keyboard is showing
            if (IpcServerData.showKeyboard != KEYBOARD_SHOWN_STATUS.NOT_SHOWN && ClickIsInKeyboard(Cursor.Position))
            {
                //continue showing the keyboard
            }
            else if (!ClickIsInKeyboard(Cursor.Position) || IpcServerData.showKeyboard == KEYBOARD_SHOWN_STATUS.NOT_SHOWN)
            {
                if (currentClickOnIbeam && !ClickAndHoldZoomboxShowingInProgress())
                {
                    int mmOffset = ConfigManager.dwellingEnabled ? 5 : 0; //make key's 5mm more bigger if using dwelling

                    IpcServerData.onscreenKeyWidthPx = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.onscreenKeyWidthMm + mmOffset);
                    IpcServerData.onscreenKeyHeightPx = ScreenPixelHelper.ConvertMmToPixels(ConfigManager.onscreenKeyHeightMm + mmOffset);

                    if (Cursor.Position.Y > ScreenPixelHelper.GetScreenSize().Height / 2)
                    {
                        IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.SHOW_TOP;
                    }
                    else
                    {
                        IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.SHOW_BOTTOM;
                    }
                }
                else if(!ClickIsInKeyboard(Cursor.Position) && IpcServerData.showKeyboard != KEYBOARD_SHOWN_STATUS.NOT_SHOWN)
                {
                    //Hide the keyboard
                    IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
                }
                Logger.WriteVar("IpcServerData.showKeyboard", IpcServerData.showKeyboard);
                Logger.WriteVar("IpcServerData.keyboardRectangle.", IpcServerData.keyboardRectangle);
            }
        }


        public bool ClickIsInKeyboard(System.Drawing.Point pos)
        {
            int overflowThresh = ScreenPixelHelper.ConvertMmToPixels(4); //px
            Rectangle area = IpcServerData.keyboardRectangle;
            Rectangle newArea = new Rectangle(area.Left - overflowThresh, area.Top - overflowThresh,
                area.Width + 2 * overflowThresh, area.Height + 2 * overflowThresh);

            return (pos.X > newArea.Left) && (pos.X < newArea.Right) && (pos.Y > newArea.Top) &&
                   (pos.Y < newArea.Bottom);
        }


        private void DetectDwelling()
        {
            if (ConfigManager.dockEnabled && dockForm != null && !dockForm.IsDisposed && nextClickType != NEXTCLICKTYPE.NONE)
            {
                //See if user's gaze has made it out of the originally clicked dock gaze button (at least 1/3rd lefwards)
                if (CursorMagnet.previousGazePoint.X < dockForm.Left + (dockForm.Width / 3))
                {
                    long timestamp = GetCurrentTimestamp();
                    long timestampDiff = timestamp - nextClickTypeTimestamp;


                    //Let's detect for a dwell (fixation) 

                    if ((CheckMagShownStatus(MAG_SHOWN_STATUS.SHOWING) && wz.WithinRect(CursorMagnet.previousGazePoint, Winzoom.ZOOMBOX_RECT_T.PADDED) )
                        || !ZoomboxAllowedToBeShown())
                    {
                        //Below, we will return early if the dwell requirements are not met

                        if(nextClickType == NEXTCLICKTYPE.SCROLLDOWN || nextClickType == NEXTCLICKTYPE.SCROLLUP)
                        {
                            //We will use a moving fixation for detecting the dwell
                            //if a fixation is detected here inside a zoombox (or equivalent), then the actual type of click is performed
                            if (timestampDiff < ConfigManager.fixationInitTimeClickAndHoldMs
                               || !FixationDetection.IsMovingFixation(ConfigManager.fixationDurationClickAndHoldZoombox, ConfigManager.fixationRadiusClickAndHoldMm))
                            {
                                return; // not ready to perform the scroll event
                            }
                        }
                        else //left, right, double click
                        {
                            //TODO: chamge to stationary fixation
                            if (timestampDiff < ConfigManager.dwellingZoomboxInitMs //the amount of time to wait before a click can occur inside a zoombox
                               || !FixationDetection.IsMovingFixation(ConfigManager.fixationDurationClickAndHoldZoombox, ConfigManager.fixationRadiusClickAndHoldMm))
                            {
                                return; // not ready to perform the click event
                            }
                        }

                        SimulateMouseEvent(nextClickType);
                        nextClickType = NEXTCLICKTYPE.NONE;
                        SetMagShownStatus(MAG_SHOWN_STATUS.TO_DISAPPEAR);
                        //We want to return to default of no click button after every action
                        if (ConfigManager.dockEnabled && dockForm != null && !dockForm.IsDisposed)
                        {
                            dockForm.SetActiveGazeButton(DockForm.GAZE_BUTTON_TYPE.NONE);
                        }
                        return;
                    }

                    if(CheckMagShownStatus(MAG_SHOWN_STATUS.NOT_SHOWN) && ZoomboxAllowedToBeShown())
                    {
                        //Now is the time to show the zoombox if a fixation is detected
                       
                        //To show the zoombox, the time used for a click and hold fixation is fine.
                        if (timestampDiff < ConfigManager.fixationInitTimeClickAndHoldMs
                            || !FixationDetection.IsMovingFixation(ConfigManager.fixationDurationClickAndHoldZoombox, ConfigManager.fixationRadiusClickAndHoldMm))
                        {
                            return; // not ready to show zoombox
                        }
                        else
                        {
                            SetMagShownStatus(MAG_SHOWN_STATUS.TO_BE_SHOWN);
                            nextClickTypeTimestamp = GetCurrentTimestamp(); //want to update timestamp for init time
                        }
                    }
                }
            }
        }

        private void SimulateMouseEvent(NEXTCLICKTYPE clickType)
        {
            Logger.WriteMsg("SimulateMouseEvent: " + clickType.ToString());

            switch(clickType)
            {
                case NEXTCLICKTYPE.DOUBLECLICK:
                    inputSimulator.Mouse.LeftButtonDoubleClick();
                    break;
                case NEXTCLICKTYPE.LEFT:

                    //User32.mouse_event(User32.MOUSEEVENTF_LEFTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    //User32.mouse_event(User32.MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    inputSimulator.Mouse.LeftButtonClick();
                    Logger.WriteMsg("SimulateMouseEvent: sent right click.");
                    break;
                case NEXTCLICKTYPE.RIGHT:
                    //User32.mouse_event(User32.MOUSEEVENTF_RIGHTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    //User32.mouse_event(User32.MOUSEEVENTF_RIGHTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    inputSimulator.Mouse.RightButtonClick();
                    Logger.WriteMsg("SimulateMouseEvent: sent right click.");
                    break;
                case NEXTCLICKTYPE.SCROLLDOWN:
                    dockForm.HideCursorOverlayForm();
                    //User32.mouse_event(User32.MOUSEEVENTF_WHEEL, 0, 0, -120, 0);
                    inputSimulator.Mouse.VerticalScroll(-1);
                    Logger.WriteMsg("SimulateMouseEvent: sent scroll down.");
                    break;
                case NEXTCLICKTYPE.SCROLLUP:
                    dockForm.HideCursorOverlayForm();
                    //User32.mouse_event(User32.MOUSEEVENTF_WHEEL, 0, 0, 120, 0);
                    inputSimulator.Mouse.VerticalScroll(1);
                    Logger.WriteMsg("SimulateMouseEvent: sent scroll up.");
                    break;
            }
        }

        private void PerformCorrectMouseClickType(MouseEventExtArgs e)
        {
            
            if (e.Button == MouseButtons.Left)
            {

                if (nextClickType == NEXTCLICKTYPE.RIGHT)
                {
                    e.Handled = true;
                    SimulateMouseEvent(nextClickType);
                }
                else if (nextClickType == NEXTCLICKTYPE.SCROLLUP)
                {
                    e.Handled = true;
                    SimulateMouseEvent(nextClickType);
                }
                else if (nextClickType == NEXTCLICKTYPE.SCROLLDOWN)
                {
                    e.Handled = true;
                    SimulateMouseEvent(nextClickType);
                }
            }

            if(ConfigManager.dwellingEnabled)
            {
                nextClickType = NEXTCLICKTYPE.NONE;
            }
            else
            {
                nextClickType = NEXTCLICKTYPE.LEFT;
            }
            

            //We want to return to default of left click gaze button after every action
            if (ConfigManager.dockEnabled && dockForm != null && !dockForm.IsDisposed)
            {
                if (ConfigManager.dwellingEnabled)
                {
                    dockForm.SetActiveGazeButton(DockForm.GAZE_BUTTON_TYPE.NONE);
                }
                else
                {
                    dockForm.SetActiveGazeButton(DockForm.GAZE_BUTTON_TYPE.LEFTCLICK);
                }
            }

        }

        public static uint getRefreshFps()
        {
            return refreshRate;
        }

        public static int getRefreshIntervalMs()
        {
            return timerIntervalMs;
        }

        private void CheckCalButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Use arrow keys to adjust gaze point (optional). [Enter] advances to next point. Press [Esc] to exit. Avoid staring too hard and take breaks by looking away for the comfort of your eyes.");
            calibrationAdjusterManualForm = new CalibrationAdjusterManualForm(calibrationAdjuster, controller);
            calibrationAdjusterManualForm.Show();
        }

        private void ClearCalButton_Click(object sender, EventArgs e)
        {
            calibrationAdjuster.clearCalibrationAdjustments();
            MessageBox.Show("Calibration adjustment cleared. Now using eye gaze data provided by eye tracker.");
        }

        private void calibrationAdjusterAutoButton_Click(object sender, EventArgs e)
        {

            calibrationAdjusterAutoForm = new CalibrationAdjusterAutoForm(calibrationAdjuster, controller);
            calibrationAdjusterAutoForm.Show();
        }

        
        private void PrecisionGameMouseForm_Load(object sender, EventArgs e)
        {
            Logger.WriteEvent();

            Logger.ReadKnownErrorsFromDisk("");
            ConfigManager.LoadFromDisk();

            IpcServerData.keySelectionMethod = ConfigManager.dwellingEnabled ? KEY_SELECTION_METHOD.DWELLING : KEY_SELECTION_METHOD.CLICKING;
            IpcServerData.dwellingKeySelectionMs = ConfigManager.dwellingKeySelectionMs;
            IpcServerData.optikeyLockOnMs = ConfigManager.dwellingKeyLockOnMs;
            
            //we show the progress pie on key selection for slow and medium speeds
            IpcServerData.optikeyShowPie = !(ConfigManager.dwellingKeySelectionMs == ConfigManager.DWELL_KEY_FAST);

            if (!useVSOptikey)
            {
                StartOrRestartOptikey();
            }
            
            calibrationAdjuster.readCalibrationAdjustmentCsv();

            //TODO: this selenium version of cursor magnet will only work if the checkbox is ticked during initial startup of program
            if (ConfigManager.cursorMagnetEnabled)
            {
                cursorMagnet.Init();
            }

            
            //Gather a list of all the buttons on the form
            //Also make them all have rounded corners by OnPaint method
            buttonsList = new List<ButtonBase>();
            ButtonHelper.AddButtonsFromFormToList(this.Controls, buttonsList);

            foreach (ButtonBase b in buttonsList)
            {
                b.Paint += ButtonHelper.PaintRoundedButton;
                ButtonHelper.ApplyRoundedButtonStyle(b, 20F);
            }

            if (false)
            {
                DisableProgramSinceExpired();
            }
            else //program is valid
            {
                if (ConfigManager.dockEnabled)
                {
                    dockForm = new DockForm();
                    dockForm.Show();
                }

                refreshTimer.Start();
            }
            

            //testing: replace zoomboxEnabled setting
            //ConfigManager.zoomboxEnabled = Settings.Default.zoomboxEnabled;
        }

        private void DisableProgramSinceExpired()
        {
            pauseTrackingButton.Enabled = false;
            pauseTrackingButton.Text = "Program expired. Contact Developers.";
            IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
            IpcServerData.quitKeyboard = true;
            trackingPaused = true;
            pictureBox1.Enabled = false;
            //Let's disable all the buttons
            foreach(ButtonBase b in buttonsList)
            {
                b.Enabled = false;
            }
        }


        public bool alreadyCalledQuit = false;
        public void PrecisionGameMouseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!alreadyCalledQuit && MessageBox.Show("Are you sure you want to Quit Avra?", "Confirm Quit",
                          MessageBoxButtons.YesNo) == DialogResult.No)
            {
                // Cancel the Closing event from closing the form.
                e.Cancel = true;
                return;
            }
            alreadyCalledQuit = true;

            cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.MOUSE);

            //Pause/Stop tracking for more stable exit
            if(!trackingPaused) pauseTrackingButton_Click(null, null);

            

            try
            {
                if (cursorMagnet.BoxesTaskPeriodic.IsAlive)
                {
                    cursorMagnet.BoxesTaskPeriodic.Abort();
                }

            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.ToString());
            }

            try
            { 
                if (Logger.errorReportThread.IsAlive)
                    {
                        Logger.errorReportThread.Abort();
                    }
                }
            catch(Exception ex)
            {
                //unhandled
                Debug.Write(ex.ToString());
            }


            //perform a second time just in case it didn't occur the first time
            cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.MOUSE);

            //Disable dockbar if enabled
            if (dockForm != null && !dockForm.IsDisposed)
            {
                dockForm.DockForm_FormClosing(null, null);
            }

            //tell optikey to close down
            IpcServerData.quitKeyboard = true;
            if(currentOptikeyPath.Length > 0)
            {
                KillOptikey();
            }

            //Close the IPC channel that communicates with Optikey
            if (channel.ChannelData != null)
            {
                ChannelServices.UnregisterChannel(channel);
            }
        }

        private void PrecisionGameMouseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.MOUSE);
        }

        private void configurationManagerOpenButton_Click(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            userSettingsForm = new UserSettingsForm(this);
            userSettingsForm.StartPosition = FormStartPosition.Manual;
            userSettingsForm.Location = this.Location;
            userSettingsForm.SetBounds(this.Left, this.Top,this.Width,this.Height);
            userSettingsForm.Show();
        }

      
        
        /*
        public struct TobiiVector2
        {
            public float x;
            public float y;

            public TobiiVector2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct tobii_calibration_point_data_t
        {
            public TobiiVector2 point_xy;
            public tobii_calibration_point_status_t left_status;
            public TobiiVector2 left_mapping_xy;
            public tobii_calibration_point_status_t right_status;
            public TobiiVector2 right_mapping_xy;
        }
        
        [DllImport("tobii_stream_engine", CallingConvention = CallingConvention.Cdecl)]
        public static extern tobii_error_t tobii_calibration_apply(IntPtr device, IntPtr data, IntPtr size);

        public static tobii_error_t tobii_calibration_apply(IntPtr device, byte[] calibration)
        {
            IntPtr num = Marshal.AllocHGlobal(calibration.Length);
            try
            {
                Marshal.Copy(calibration, 0, num, calibration.Length);
                return tobii_calibration_apply(device, data: num, size: new IntPtr(calibration.Length));
            }
            finally
            {
                Marshal.FreeHGlobal(num);
            }
        }
        */


        public static Bitmap ChangePixelFormat(Bitmap inputImage, PixelFormat newFormat)
        {
            Bitmap bmp = new Bitmap(inputImage.Width, inputImage.Height, newFormat);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(inputImage, 0, 0);
            }
            return bmp;
        }


        private void pauseTrackingButton_Click(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            trackingPaused = !trackingPaused;
            Logger.WriteVar(nameof(trackingPaused), trackingPaused);
            if(trackingPaused)
            {
                if(wz != null) { wz.Hide(); }
                SetMagShownStatus(MAG_SHOWN_STATUS.NOT_SHOWN);
                cursorIconManager.SetCursorScheme(CursorIconManager.CURSOR_ICON_SCHEME.MOUSE);
                refreshTimer.Stop();
                controller.state = MouseController.TrackingState.PAUSED;
                UnsubscribeGlobalMouseEventHandlers();
                pauseTrackingButton.Text = "Resume";
                pauseTrackingButton.BackColor = Color.Green;
                Point currentPosition = Cursor.Position;
                currentPosition.Offset(new Point(100, 200));
                Cursor.Position = currentPosition; //move the cursor away from the button so user doesn't accidentally click on it again

                IpcServerData.showKeyboard = KEYBOARD_SHOWN_STATUS.NOT_SHOWN;
            }
            else
            {
                refreshTimer.Start();
                controller.state = MouseController.TrackingState.STARTING;
                pauseTrackingButton.Text = "Stop";
                pauseTrackingButton.BackColor = Color.White;
                SubscribeGlobalMouseEventHandlers();
                SetMagShownStatus(MAG_SHOWN_STATUS.NOT_SHOWN);
            }
            this.Invalidate();
        }
        

        private void PrecisionGazeMouseForm_Activated(object sender, EventArgs e)
        {

            //Update screen size inches and resolution
            Rectangle screenSizeMm = ScreenPixelHelper.GetScreenSizeMm();
            double screenSizeDiagonalInches = ScreenPixelHelper.GetRectHypotenuse(screenSizeMm) / 25.4;
            string formattedString = string.Format("{0:N1} h={1:N1} diagonal={2:N1}", screenSizeMm.Width / 25.4, screenSizeMm.Height / 25.4, screenSizeDiagonalInches);


            Logger.WriteMsg("Screen Size (inches) w=" +  formattedString);

            Rectangle screenSizeGdi = ScreenPixelHelper.GetScreenSizeGdi();
            Logger.WriteMsg("Screen Resolution (Pixels): " + screenSizeGdi.Width + "x" + screenSizeGdi.Height);

            //Native screensize reported to windows forms
            Size monitorSizePixels = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            //If these 2 values are the same then there is no problem
            if (screenSizeGdi.Width == monitorSizePixels.Width)
            {
                displayHotplugTextbox.Visible = false;
            }
            else
            {
                displayHotplugTextbox.Visible = true;
                displayHotplugTextbox.ForeColor = Color.Red;
            }

            //Show display resolutions as debug info
            displayInfoTextbox.Text = "SystemInfo: " + monitorSizePixels.Width + "," + monitorSizePixels.Height +
                " gdi: " + screenSizeGdi.Width + "," + screenSizeGdi.Height + 
                " forms: " + Screen.PrimaryScreen.Bounds.Width + "," + Screen.PrimaryScreen.Bounds.Height;
        }

        //TODO: remove when advanced user settings fully integrated into user settings form and all invocations are there
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            configManagerForm = new ConfigManagerForm();
            configManagerForm.Show();
        }
    }
}
