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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;


/*
 * This is the interprocess communication outline of the data that gets communicated.
 * My modified Optikey program reads these fields, and some fields may be written to via the set {} methods.
 */
namespace PrecisionGazeMouse
{
    //https://www.codeproject.com/Articles/14791/NET-Remoting-with-an-easy-example
    //https://www.techrepublic.com/article/use-microsoft-message-queuing-in-c-for-inter-process-communication/

    /*
        DO NOT:
        public DateTime lastModifiedServer
        {
            get { return lastModifiedServer; }
        }

        //this is becomes a recursive call and leads to a stack overflow.  Always return something else that is initialized.

    */

    [Serializable]
    public enum KEYBOARD_SHOWN_STATUS
    {
        NOT_SHOWN,
        SHOW_TOP,
        SHOW_BOTTOM
    }

    [Serializable]
    public enum KEY_SELECTION_METHOD
    {
        CLICKING,
        DWELLING
    }

    public class IpcSharedObject : MarshalByRefObject
    {
        public IpcSharedObject() { }

        public DateTime lastModifiedServer
        {
            get { return IpcServerData.lastModifiedServer; }
        }
        

        //only read by client, not modified
        //This is an INT because there is a bug in remoting an enum!
        public int showKeyboard
        {
            get { return (int)IpcServerData.showKeyboard; }
            set { IpcServerData.showKeyboard = (KEYBOARD_SHOWN_STATUS) value; }
        }

        //This can be only set by the client
        public System.Drawing.Rectangle keyboardRectangle
        {
            get { return IpcServerData.keyboardRectangle; }
            set { IpcServerData.keyboardRectangle = value; }
        }

        public bool quitKeyboard {
            get { return IpcServerData.quitKeyboard; }
        }

        public int onscreenKeyWidthPx
        {
            get { return IpcServerData.onscreenKeyWidthPx; }
        }

        public int onscreenKeyHeightPx
        {
            get { return IpcServerData.onscreenKeyHeightPx; }
        }

        public DateTime lastModifiedServerCalibration
        {
            get { return IpcServerData.lastModifiedServerCalibration; }
        }

        public System.Drawing.Point[] adjGrid
        {
            get { return IpcServerData.adjGrid; }
        }

        public System.Drawing.Point[] adjGridOffset
        {
            get { return IpcServerData.adjGridOffset; }
        }

        public int dwellingKeySelectionMs
        {
            get { return IpcServerData.dwellingKeySelectionMs; }
        }

        public int keySelectionMethod
        {
            get { return (int)IpcServerData.keySelectionMethod; }
            set { IpcServerData.keySelectionMethod = (KEY_SELECTION_METHOD)value; }
        }

        public bool optikeyRequestsRestart
        {
            get { return IpcServerData.optikeyRequestsRestart; }
            set { IpcServerData.optikeyRequestsRestart = value; }
        }

        public int optikeyLockOnMs
        {
            get { return IpcServerData.optikeyLockOnMs; }
        }

        public bool optikeyShowPie
        {
            get { return IpcServerData.optikeyShowPie; }
        }
    }
}
