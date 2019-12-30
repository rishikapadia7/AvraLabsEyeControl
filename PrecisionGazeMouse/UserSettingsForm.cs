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

/*
 * Basic settings form window with options and selections.
 */

namespace PrecisionGazeMouse
{
    public partial class UserSettingsForm : Form
    {
        List<ButtonBase> buttonsList;
        PrecisionGazeMouseForm pgmf;

        public UserSettingsForm(PrecisionGazeMouseForm pgmf)
        {
            InitializeComponent();
            this.pgmf = pgmf;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            ConfigManager.SaveToDisk();
            this.Close();
        }

        //TODO
        private void ClickMethod_CheckedChanged(object sender, EventArgs e)
        {
            if(standardMouseButton.Checked)
            {
                ConfigManager.dwellingEnabled = false;
                tabControl1.SelectedTab = mainPage;
                Logger.WriteMsg("standardMouseButton is checked");
            }
            else if(singleSwitchButton.Checked)
            {
                Logger.WriteMsg("singleSwitchButton is checked");
            }
            else if(dwellButton.Checked)
            {
                ConfigManager.dwellingEnabled = true;
                tabControl1.SelectedTab = dwellPage;
                Logger.WriteMsg("dwellButton is checked");
            }

            IpcServerData.keySelectionMethod = ConfigManager.dwellingEnabled ? KEY_SELECTION_METHOD.DWELLING : KEY_SELECTION_METHOD.CLICKING;

            //Also want to enable or disable dockbar accordingly
            if(ConfigManager.dwellingEnabled)
            {
                if(pgmf.dockForm == null || pgmf.dockForm.IsDisposed)
                {
                    pgmf.dockForm = new DockForm();
                    pgmf.dockForm.Show();
                }
                //otherwise it's already showing
            }
            else
            {
                //let's unregister the dockbar
                if (pgmf.dockForm != null && !pgmf.dockForm.IsDisposed)
                {
                    pgmf.dockForm.Close();
                }
            }
            ApplyCheckedButtonConstraints();
        }

        private void TypingMethod_CheckedChanged(object sender, EventArgs e)
        {
            if (standardKeyboardButton.Checked)
            {
                ConfigManager.onscreenKeyboardEnabled = false;
                Logger.WriteMsg("standardKeyboardButton is checked");
            }
            else if (voiceRecognitionButton.Checked)
            {
                //not supported
                ConfigManager.onscreenKeyboardEnabled = true;
                Logger.WriteMsg("voiceRecognitionButton is checked");
            }
            else if (onscreenKeyboardButton.Checked)
            {
                ConfigManager.onscreenKeyboardEnabled = true;
                Logger.WriteMsg("onscreenKeyboardButton is checked");
            }

            ApplyCheckedButtonConstraints();
        }

        private void DockbarSelection_CheckedChanged(object sender, EventArgs e)
        {
            if (noDockbarButton.Checked)
            {
                ConfigManager.dockEnabled = false;
                Logger.WriteMsg("noDockbarButton is checked");
            }
            else if (quickLookButton.Checked)
            {
                ConfigManager.dockEnabled = true;
                ConfigManager.dockFixateToSelectEnabled = true;
                Logger.WriteMsg("quickLookButton is checked");
            }
            else if (clickButton.Checked)
            {
                ConfigManager.dockEnabled = true;
                ConfigManager.dockFixateToSelectEnabled = false;
                Logger.WriteMsg("clickButton is checked");
            }

            ApplyCheckedButtonConstraints();
        }

        private void DwellSpeed_CheckedChanged(object sender, EventArgs e)
        {
            if (dwellSpeedSlow.Checked)
            {
                ConfigManager.dwellingZoomboxInitMs = ConfigManager.DWELL_ZOOMBOX_SLOW;
                Logger.WriteMsg("DWELL_ZOOMBOX_SLOW is checked");
            }
            else if (dwellSpeedMedium.Checked)
            {
                ConfigManager.dwellingZoomboxInitMs = ConfigManager.DWELL_ZOOMBOX_MEDIUM;
                Logger.WriteMsg("DWELL_ZOOMBOX_SLOW is checked");
            }
            else if (dwellSpeedFast.Checked)
            {
                ConfigManager.dwellingZoomboxInitMs = ConfigManager.DWELL_ZOOMBOX_FAST;
                Logger.WriteMsg("DWELL_ZOOMBOX_SLOW is checked");
            }

            ApplyCheckedButtonConstraints();
        }

        //Pertains to onscreen keyboard typing speed
        private void TypingSpeed_CheckedChanged(object sender, EventArgs e)
        {
            if (typingSpeedSlow.Checked)
            {
                ConfigManager.dwellingKeySelectionMs = ConfigManager.DWELL_KEY_SLOW;
                ConfigManager.dwellingKeyLockOnMs = ConfigManager.DWELL_KEY_LOCK_ON_SLOW;
                Logger.WriteMsg("DWELL_KEY_SLOW is checked");
            }
            else if (typingSpeedMedium.Checked)
            {
                ConfigManager.dwellingKeySelectionMs = ConfigManager.DWELL_KEY_MEDIUM;
                ConfigManager.dwellingKeyLockOnMs = ConfigManager.DWELL_KEY_LOCK_ON_MEDIUM;
                Logger.WriteMsg("DWELL_KEY_MEDIUM is checked");
            }
            else if (typingSpeedFast.Checked)
            {
                ConfigManager.dwellingKeySelectionMs = ConfigManager.DWELL_KEY_FAST;
                ConfigManager.dwellingKeyLockOnMs = ConfigManager.DWELL_KEY_LOCK_ON_FAST;
                Logger.WriteMsg("DWELL_KEY_FAST is checked");
            }
            IpcServerData.dwellingKeySelectionMs = ConfigManager.dwellingKeyLockOnMs;
            IpcServerData.dwellingKeySelectionMs = ConfigManager.dwellingKeySelectionMs;

            //we show the progress pie on key selection for slow and medium speeds
            IpcServerData.optikeyShowPie = !(ConfigManager.dwellingKeySelectionMs == ConfigManager.DWELL_KEY_FAST);

            ApplyCheckedButtonConstraints();
        }

        private void UserSettingsForm_Load(object sender, EventArgs e)
        {
            //Gather a list of all the buttons on the form
            //Also make them all have rounded corners by OnPaint method
            buttonsList = new List<ButtonBase>();
            ButtonHelper.AddButtonsFromFormToList(this.Controls, buttonsList);

            
            foreach (ButtonBase b in buttonsList)
            {
                if (b.GetType() == typeof(RadioButton))
                {
                    b.BackColor = Color.White;
                }
            }
            
            //We want to populate the selected buttons based on current user settings

            if (ConfigManager.dwellingEnabled)
            {
                dwellButton.Checked = true;
                ConfigManager.onscreenKeyboardEnabled = true;
                tabControl1.SelectedTab = dwellPage;
            }
            else
            {
                standardMouseButton.Checked = true;
                tabControl1.SelectedTab = mainPage;
            }

            if (ConfigManager.onscreenKeyboardEnabled)
            {
                onscreenKeyboardButton.Checked = true;
            }
            else
            {
                standardKeyboardButton.Checked = true;
            }

            if (ConfigManager.dockEnabled)
            {
                if (ConfigManager.dockFixateToSelectEnabled)
                {
                    quickLookButton.Checked = true;
                }
                else
                {
                    clickButton.Checked = true;
                }
            }
            else
            {
                noDockbarButton.Checked = true;
            }

            //Set dwelling speed
            if (ConfigManager.dwellingZoomboxInitMs == ConfigManager.DWELL_ZOOMBOX_SLOW)
            {
                dwellSpeedSlow.Checked = true;
            }   
            else if (ConfigManager.dwellingZoomboxInitMs == ConfigManager.DWELL_ZOOMBOX_FAST)
            {
                dwellSpeedFast.Checked = true;
            }
            else
            {
                dwellSpeedMedium.Checked = true;
            }

            //Set onscreen keyboard typing speed
            if (ConfigManager.dwellingKeySelectionMs == ConfigManager.DWELL_KEY_SLOW)
            {
                typingSpeedSlow.Checked = true;
            }
            else if (ConfigManager.dwellingKeySelectionMs == ConfigManager.DWELL_KEY_FAST)
            {
                typingSpeedFast.Checked = true;
            }
            else
            {
                typingSpeedMedium.Checked = true;
            }

            ApplyCheckedButtonConstraints();
        }

        //Chose to make non-applicable buttons invisible since disabling them looked crowded without purpose
        private void ApplyCheckedButtonConstraints()
        {
            voiceRecognitionButton.Enabled = false;//make non-enabled
            dwellButton.Enabled = true;
            singleSwitchButton.Enabled = false; //wait until a new setting created for it

            //Enforce that user must use dockbar if they are using singleSwitch, DwellButton, or OnscreenKeyboard
            if(singleSwitchButton.Checked || dwellButton.Checked)
            {
                noDockbarButton.Enabled = false;

                if(noDockbarButton.Checked)
                {
                    noDockbarButton.Checked = false;
                    clickButton.Checked = true;
                }
            }
            else
            {
                noDockbarButton.Enabled = true;
            }

            //Dockbar is disabled when using a standard mouse and currently limited to dwelling only (until single switch mode is made)
            if (standardMouseButton.Checked)
            {
                noDockbarButton.Enabled = true;
                noDockbarButton.Checked = true;

                clickButton.Enabled = false;
                quickLookButton.Enabled = false;
            }
            else if(dwellButton.Checked)
            {
                noDockbarButton.Enabled = false;
                clickButton.Enabled = false;
                if (clickButton.Checked)
                {
                    clickButton.Checked = false;
                    quickLookButton.Checked = true;
                }
            }

            //Update the colors of the buttons depending on their status
            foreach (ButtonBase b in buttonsList)
            {
                if (b.GetType() == typeof(RadioButton))
                {
                    RadioButton rb = (RadioButton)b;
                    if(!rb.Enabled)
                    {
                        rb.BackColor = Color.DarkGray;
                    }
                    else if(rb.Checked)
                    {
                        rb.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        rb.BackColor = Color.White;
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        public bool AnyButtonContainsPoint(Point p)
        {
            if (this.ContainsFocus)
            {
                return ButtonHelper.GetButtonContainingPoint(buttonsList, p) != null;
            }
            return false;
        }
    }
}
