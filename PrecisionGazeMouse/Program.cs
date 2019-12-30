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

namespace PrecisionGazeMouse
{
    using System;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;

    static class Program
    {


        /// <summary>
        /// Gets the singleton EyeX host instance.
        /// </summary>
       
        static PrecisionGazeMouseForm pgmf;

        private enum ProcessDPIAwareness
        {
            ProcessDPIUnaware = 0,
            ProcessSystemDPIAware = 1,
            ProcessPerMonitorDPIAware = 2
        }

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

        
        private static void SetDpiAwareness(ProcessDPIAwareness val)
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDpiAwareness(val);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            pgmf = new PrecisionGazeMouseForm();

            // Handle the ApplicationExit event to know when the application is exiting.
            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            //Whenever a new version of the application is made, this ensures that user's previous settings are not lost
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Reload();

            //Set process dpi awareness
            SetDpiAwareness(ProcessDPIAwareness.ProcessSystemDPIAware);

            Application.Run(pgmf);
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.Save();
                pgmf.PrecisionGameMouseForm_FormClosing(null, null);
            }
            catch { }
        }
    }
}

