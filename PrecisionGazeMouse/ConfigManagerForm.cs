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
using System.Diagnostics;

/*
 * User interface for adjusting the configurable values
 * specified in ConfigManager
 */
namespace PrecisionGazeMouse
{
    public partial class ConfigManagerForm : Form
    {
        public ConfigManagerForm()
        {
            InitializeComponent();
            Load += new EventHandler(ConfigManagerForm_Load);
        }

        private void ConfigManagerForm_Load(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            LoadConfigManagerValuesIntoGUI();
        }

        private void LoadConfigManagerValuesIntoGUI()
        {
            try
            {
                cursorMagnetEnabledCheckbox.Checked = ConfigManager.cursorMagnetEnabled;
                cursorMagnetClippingThreshold.Value = (decimal)ConfigManager.cursorMagnetClippingThresholdMm;
                zoomboxStickyCheckBox.Checked = ConfigManager.zoomboxSticky;
                mouseMovementEyeGazeResumeUpDown.Value = ((decimal)ConfigManager.mouseMovementEyeGazeResumeMilliseconds) / 1000;
                keyboardActiveGazeResumeSecondsNumeric.Value = ((decimal)ConfigManager.keyboardEyeGazeResumeMilliseconds) / 1000;
                zoomWindowPctScreenNumeric.Value = (decimal)ConfigManager.zoomWindowPctScreen;
                zoomWindowMagnificationNumeric.Value = (decimal)ConfigManager.zoomWindowMagnificationPct;

                fixationMinDurationNumeric.Value = (decimal)ConfigManager.fixationMinDurationMs;

                fixationRadiusClickAndHoldMmNumeric.Value = (decimal)ConfigManager.fixationRadiusClickAndHoldMm;
                fixationDurationClickAndHoldZoomboxNumeric.Value = ConfigManager.fixationDurationClickAndHoldZoombox;
                fixationInitTimeClickAndHoldMsNumeric.Value = ConfigManager.fixationInitTimeClickAndHoldMs;

                fixationRadiusForHoverMmNumeric.Value = ConfigManager.fixationRadiusForHoverMm;
                fixationDurationForHoverMsNumeric.Value = (decimal)ConfigManager.fixationDurationForHoverMs;
                fixationMinCursorActiveMs.Value = ConfigManager.fixationMinCursorActiveMs;

                cursorAlwaysFollowsGazeCheckbox.Checked = ConfigManager.cursorAlwaysFollowsGaze;
                fixationClickAndHoldCheckbox.Checked = ConfigManager.fixationClickAndHoldEnabled;

                hideCursorGazeCheckbox.Checked = ConfigManager.hideCursorDuringGaze;
                largeSaccadeMmNumeric.Value = ConfigManager.largeSaccadeThresholdMm;
                zoomboxEnabledCheckbox.Checked = ConfigManager.zoomboxEnabled;
                zoomboxGridCheckbox.Checked = ConfigManager.zoomboxGrid;
                zoomboxZoomAgainAfterMsNumeric.Value = ConfigManager.zoomboxZoomAgainAfterMs;
                zoomboxZoomAgainCheckbox.Checked = ConfigManager.zoomboxZoomAgain;
                zoomboxZoomAgainAfterMsNumeric.Enabled = zoomboxZoomAgainCheckbox.Checked;

                cursorMagnetClippingExitThresholdMmNumeric.Value = ConfigManager.cursorMagnetClippingExitThresholdMm;
                CursorMagnetClippingExitDurationMsNumeric.Value = ConfigManager.cursorMagnetClippingExitDurationMs;
                CursorMagnetClippingChangeBoxIntoPercNumeric.Value = ConfigManager.cursorMagnetClippingChangeBoxIntoPerc;

                CalibrationCircleSizeMmNumeric.Value = ConfigManager.calibrationCircleSizeMm;
                CalibrationSingleIterationTimeMsNumeric.Value = ConfigManager.calibrationIterationTimeMs;
                CalibrationIterationCountMaxNumeric.Value = ConfigManager.calibrationIterationCountMax;
                CalibrationCompletedThresholdMmNumeric.Value = ConfigManager.calibrationCompletedThresholdMm;

                CursorMagnetClippingMinBoxWidthMm.Value = ConfigManager.cursorMagnetClippingMinBoxWidthMm;
                fixationDurationClickAndHoldZoomboxNumeric.Value = ConfigManager.fixationDurationClickAndHoldZoombox;

                dockEnabledCheckbox.Checked = ConfigManager.dockEnabled;
                dockFixateToSelectButtonCheckbox.Checked = ConfigManager.dockFixateToSelectEnabled;
                dockButtonFixationMsNumeric.Value = ConfigManager.dockButtonFixationMs;
                dockFormMaxWidthMmNumeric.Value = ConfigManager.dockFormMaxWidthMm;
                dockButtonHeightMmNumeric.Value = ConfigManager.dockButtonHeightMm;

                onscreenKeyboardEnabledCheckbox.Checked = ConfigManager.onscreenKeyboardEnabled;
                onscreenKeyWidthMmNumeric.Value = ConfigManager.onscreenKeyWidthMm;
                onscreenKeyHeightMmNumeric.Value = ConfigManager.onscreenKeyHeightMm;
            }
            catch(Exception e)
            {
                Logger.WriteError(e.ToString());
                //Revert to defaults and try again
                ConfigManager.RestoreDefaults();
                LoadConfigManagerValuesIntoGUI();
            }
            
        }

        private void cursorMagnetEnabledCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorMagnetEnabled = cursorMagnetEnabledCheckbox.Checked;
        }

        private void zoomboxStickyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomboxSticky = zoomboxStickyCheckBox.Checked;
        }

        private void mouseMovementEyeGazeResumeUpDown_ValueChanged(object sender, EventArgs e)
        {
            decimal val = mouseMovementEyeGazeResumeUpDown.Value; //in seconds
            ConfigManager.mouseMovementEyeGazeResumeMilliseconds = (int)(val * 1000);
        }

        private void zoomWindowPctScreenNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomWindowPctScreen = (int) zoomWindowPctScreenNumeric.Value;
        }

        private void zoomWindowMagnificationNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomWindowMagnificationPct = (int)zoomWindowMagnificationNumeric.Value;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            ConfigManager.SaveToDisk();
            this.Hide();
        }

        private void cursorMagnetClippingThreshold_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorMagnetClippingThresholdMm = (int)cursorMagnetClippingThreshold.Value;
        }


        private void hideCursorGazeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.hideCursorDuringGaze = hideCursorGazeCheckbox.Checked;
        }

 
        private void largeSaccadeMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.largeSaccadeThresholdMm = (int)largeSaccadeMmNumeric.Value;
        }

        private void zoomboxEnabledCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomboxEnabled = zoomboxEnabledCheckbox.Checked;
        }


        private void cursorMagnetClippingExitThresholdPixelsNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorMagnetClippingExitThresholdMm = (int)cursorMagnetClippingExitThresholdMmNumeric.Value;
        }

        private void CursorMagnetClippingExitDurationMsNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorMagnetClippingExitDurationMs = (int)CursorMagnetClippingExitDurationMsNumeric.Value;
        }

        private void CursorMagnetClippingChangeBoxIntoPercNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorMagnetClippingChangeBoxIntoPerc = (int)CursorMagnetClippingChangeBoxIntoPercNumeric.Value;
        }

        private void CalibrationCircleSizeMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.calibrationCircleSizeMm = (int)CalibrationCircleSizeMmNumeric.Value;
        }

        private void CalibrationSingleIterationTimeMsNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.calibrationIterationTimeMs = (int)CalibrationSingleIterationTimeMsNumeric.Value;
        }

        private void CalibrationIterationCountMaxNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.calibrationIterationCountMax = (int)CalibrationIterationCountMaxNumeric.Value;
        }

        private void CalibrationCompletedThresholdMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.calibrationCompletedThresholdMm = (int)CalibrationCompletedThresholdMmNumeric.Value;
        }

        private void CursorMagnetClippingMinBoxWidth_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorMagnetClippingMinBoxWidthMm = (int)CursorMagnetClippingMinBoxWidthMm.Value;
        }

        private void restoreDefaultsButton_Click(object sender, EventArgs e)
        {
            ConfigManager.RestoreDefaults();
            LoadConfigManagerValuesIntoGUI();
        }

        private void keyboardActiveGazeResumeSecondsNumeric_ValueChanged(object sender, EventArgs e)
        {
            decimal val = keyboardActiveGazeResumeSecondsNumeric.Value; //in seconds
            ConfigManager.keyboardEyeGazeResumeMilliseconds = (int)(val * 1000);
        }

        private void fixationMinDurationNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationMinDurationMs = (int)fixationMinDurationNumeric.Value;
        }

        private void fixationDurationClickAndHoldZoomboxNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationDurationClickAndHoldZoombox = (int)fixationDurationClickAndHoldZoomboxNumeric.Value;
        }

        private void fixationRadiusClickAndHoldMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationRadiusClickAndHoldMm = (int)fixationRadiusClickAndHoldMmNumeric.Value;
        }

        private void zoomboxGridCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomboxGrid = zoomboxGridCheckbox.Checked;
        }

        private void zoomboxZoomAgainCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomboxZoomAgain = zoomboxZoomAgainCheckbox.Checked;
            zoomboxZoomAgainAfterMsNumeric.Enabled = zoomboxZoomAgainCheckbox.Checked;
        }

        private void zoomboxZoomAgainAfterMsNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.zoomboxZoomAgainAfterMs = (int)zoomboxZoomAgainAfterMsNumeric.Value;
        }

        private void debugTab_Click(object sender, EventArgs e)
        {

        }

        private void fixationInitTimeClickAndHoldMsNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationInitTimeClickAndHoldMs = (int)fixationInitTimeClickAndHoldMsNumeric.Value;
        }

        private void fixationRadiusForHoverMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationRadiusForHoverMm = (int)fixationRadiusForHoverMmNumeric.Value;
        }

        private void fixationDurationForHoverMsNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationDurationForHoverMs = (int)fixationDurationForHoverMsNumeric.Value;
        }


        private void generalTab_Click(object sender, EventArgs e)
        {

        }

        private void fixationMinCursorActiveMs_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationMinCursorActiveMs = (int)fixationMinCursorActiveMs.Value;
        }

        private void fixationClickAndHoldCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.fixationClickAndHoldEnabled = fixationClickAndHoldCheckbox.Checked;
        }

        private void cursorAlwaysFollowsGazeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.cursorAlwaysFollowsGaze = cursorAlwaysFollowsGazeCheckbox.Checked;
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void dockEnabledCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.dockEnabled = dockEnabledCheckbox.Checked;
        }

        private void dockFixateToSelectButtonCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.dockFixateToSelectEnabled = dockFixateToSelectButtonCheckbox.Checked;
        }

        private void label19_Click(object sender, EventArgs e)
        {
            ConfigManager.dockButtonFixationMs = (int)dockButtonFixationMsNumeric.Value;
        }

        private void dockFormMaxWidthMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.dockFormMaxWidthMm = (int)dockFormMaxWidthMmNumeric.Value;
        }

        
        private void dockButtonHeightMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.dockButtonHeightMm = (int)dockButtonHeightMmNumeric.Value;
        }

        private void onscreenKeyboardEnabledCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.onscreenKeyboardEnabled = onscreenKeyboardEnabledCheckbox.Checked;
        }

        private void onscreenKeyWidthMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.onscreenKeyWidthMm = (int)onscreenKeyWidthMmNumeric.Value;
        }

        private void onscreenKeyHeightMmNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.onscreenKeyHeightMm = (int)onscreenKeyHeightMmNumeric.Value;
        }
    }
}
