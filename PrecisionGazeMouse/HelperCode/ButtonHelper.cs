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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PrecisionGazeMouse
{
    public static class ButtonHelper
    {
        public static ButtonBase GetButtonContainingPoint(List<ButtonBase> buttonList, Point p)
        {
            foreach (ButtonBase b in buttonList)
            {
                if (DoesButtonContainPoint(b, p))
                {
                    return b;
                }
            }
            return null;
        }

        public static bool DoesButtonContainPoint(ButtonBase b, Point p)
        {
            Rectangle bRect = b.RectangleToScreen(b.ClientRectangle);
            return bRect.Contains(p);
        }

        public static void AddButtonsFromFormToList(Control.ControlCollection controls, List<ButtonBase> buttonsList)
        {
            foreach (Control c in controls)
            {
                if (c.GetType() == typeof(Button) || c.GetType() == typeof(RadioButton))
                {
                    ButtonBase b = (ButtonBase)c;
                    buttonsList.Add(b);
                }
                else if(c.GetType() == typeof(Panel) || c.GetType() == typeof(TabControl)) //recursively add corresponding buttons to the list
                {
                    AddButtonsFromFormToList(c.Controls, buttonsList);
                }
            }
        }

        public static void ApplyRoundedButtonStyle(ButtonBase b, float fontPt)
        {
            b.TabStop = false;
            b.ForeColor = Color.Black;
            b.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            b.FlatAppearance.BorderSize = 0;
            
            //flat can't be used for RadioButton
            if(b.GetType() == typeof(Button))
            {
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            }
            
            b.Font = new System.Drawing.Font("Century Gothic", fontPt);
        }

        public static GraphicsPath GetRoundPath(RectangleF Rect, int radius)
        {
            radius = 10;
            float r2 = radius / 2f;
            GraphicsPath GraphPath = new GraphicsPath();

            GraphPath.AddArc(Rect.X, Rect.Y, radius, radius, 180, 90);
            GraphPath.AddLine(Rect.X + r2, Rect.Y, Rect.Width - r2, Rect.Y);
            GraphPath.AddArc(Rect.X + Rect.Width - radius, Rect.Y, radius, radius, 270, 90);
            GraphPath.AddLine(Rect.Width, Rect.Y + r2, Rect.Width, Rect.Height - r2);
            GraphPath.AddArc(Rect.X + Rect.Width - radius,
                             Rect.Y + Rect.Height - radius, radius, radius, 0, 90);
            GraphPath.AddLine(Rect.Width - r2, Rect.Height, Rect.X + r2, Rect.Height);
            GraphPath.AddArc(Rect.X, Rect.Y + Rect.Height - radius, radius, radius, 90, 90);
            GraphPath.AddLine(Rect.X, Rect.Height - r2, Rect.X, Rect.Y + r2);

            GraphPath.CloseFigure();
            return GraphPath;
        }

        public static void PaintRoundedButton(object sender, PaintEventArgs e)
        {
            ButtonBase b = (ButtonBase)sender;
            RectangleF Rect = new RectangleF(0, 0, b.Width, b.Height);
            GraphicsPath GraphPath = GetRoundPath(Rect, 50);

            b.Region = new Region(GraphPath);
            using (Pen pen = new Pen(Color.LightGray, 2f))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }
        }
    }
}
