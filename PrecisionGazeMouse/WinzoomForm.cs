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
using Karna.Magnification;
using System.Runtime.InteropServices;


/*
 * The window that shows the zoombox.
 */

namespace PrecisionGazeMouse
{
    public partial class WinzoomForm : Form
    {
        
        public Magnifier mag;
        public float magnification = 2.0f;
        Winzoom wz;

        public WinzoomForm(Winzoom winz)
        {
            InitializeComponent();
            mag = new Magnifier(this, ref magnification);
            Load += new EventHandler(WinzoomForm_Load);
            KeyDown += new KeyEventHandler(WinzoomForm_KeyDown);
            wz = winz;
            //FormBorderStyle = FormBorderStyle.None;
        }

        private void WinzoomForm_Load(object sender, EventArgs e)
        {
            Logger.WriteEvent();
            FormBorderStyle = FormBorderStyle.None;
            //TopMost = true;
            //ShowInTaskbar = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //FormBorderStyle = FormBorderStyle.None;
            User32.SetFormTransparent(this.Handle);
        }

        private void WinzoomForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Logger.WriteEvent();
            //Pressing any keyboard key should cause winzoom to disappear
            wz.Hide();
            /*
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    wz.Hide();
                    break;
                default:
                    break;
            }
            */
        }

        //Ensures that the form does not show up in the alt-tab menu
        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
    }
}
