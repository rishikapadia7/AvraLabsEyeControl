using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace Karna.Magnification
{
    public class Magnifier : IDisposable
    {
        
        private Form form;
        private IntPtr hwndMag;
        public float magnification;
        private bool initialized;
        private RECT activeRect;
        private System.Windows.Forms.Timer timer;
        private RECT sourceRect;

        public Magnifier(Form form, ref float mag)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            if (mag > 1.0f)
            {
                magnification = mag;
            }
            else
            {
                magnification = 2.0f; //default 200%
            }
            
            this.form = form;
            this.form.Resize += new EventHandler(form_Resize);
            this.form.FormClosing += new FormClosingEventHandler(form_FormClosing);

            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);

            initialized = NativeMethods.MagInitialize();
            if (initialized)
            {
                SetupMagnifier();
                timer.Interval = NativeMethods.USER_TIMER_MINIMUM;
                timer.Enabled = true;
            }
        }

        void form_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Enabled = false;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateMagnifier();
        }

        void form_Resize(object sender, EventArgs e)
        {
            ResizeMagnifier();
        }

        ~Magnifier()
        {
            Dispose(false);
        }

        protected virtual void ResizeMagnifier()
        {
            if ( initialized && (hwndMag != IntPtr.Zero))
            {
                NativeMethods.GetClientRect(form.Handle, ref activeRect);
                // Resize the control to fill the window.
                NativeMethods.SetWindowPos(hwndMag, IntPtr.Zero,
                    activeRect.left, activeRect.top, activeRect.right, activeRect.bottom, 0);
            }
        }

        public void SetSourceRect(Rectangle rect)
        {
            sourceRect = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public void SetActiveRect(Rectangle rect)
        {
            activeRect = new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public virtual void UpdateMagnifier()
        {
            if ((!initialized) || (hwndMag == IntPtr.Zero))
                return;

            /*
            POINT mousePoint = new POINT();


            NativeMethods.GetCursorPos(ref mousePoint);

            int width = (int)((magWindowRect.right - magWindowRect.left) / magnification);
            int height = (int)((magWindowRect.bottom - magWindowRect.top) / magnification);

            sourceRect.left = mousePoint.x - width / 2;
            sourceRect.top = mousePoint.y - height / 2;


            // Don't scroll outside desktop area.
            if (sourceRect.left < 0)
            {
                sourceRect.left = 0;
            }
            if (sourceRect.left > NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width)
            {
                sourceRect.left = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width;
            }
            sourceRect.right = sourceRect.left + width;

            if (sourceRect.top < 0)
            {
                sourceRect.top = 0;
            }
            if (sourceRect.top > NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height)
            {
                sourceRect.top = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height;
            }
            sourceRect.bottom = sourceRect.top + height;
            */

            if (this.form == null)
            {
                timer.Enabled = false;
                return;
            }

            if (this.form.IsDisposed)
            {
                timer.Enabled = false;
                return;
            }

            // Set the magnification factor.
            Transformation matrix = new Transformation(magnification);
            NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);

            // Set the source rectangle for the magnifier control.
            NativeMethods.MagSetWindowSource(hwndMag, sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            NativeMethods.SetWindowPos(form.Handle, NativeMethods.HWND_TOPMOST, form.Top, form.Left, form.Width, form.Height,
                (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);

            //This call fails, no point trying.
            //bool ret = NativeMethods.MagSetInputTransform(true, ref sourceRect, ref magWindowRect);
            //if (!ret) MessageBox.Show("Input transform failed.");

            // Force redraw.
            NativeMethods.InvalidateRect(hwndMag, IntPtr.Zero, true);
        }

        public float Magnification
        {
            get { return magnification; }
            set
            {
                if (magnification != value)
                {
                    magnification = value;
                    // Set the magnification factor.
                    Transformation matrix = new Transformation(magnification);
                    NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
                }
            }
        }

        protected void SetupMagnifier()
        {
            if (!initialized)
                return;

            IntPtr hInst;

            hInst = NativeMethods.GetModuleHandle(null);

            // Make the window opaque.
            form.AllowTransparency = true;
            form.TransparencyKey = Color.Empty;
            //form.Opacity = 255;

            // Create a magnifier control that fills the client area.
            NativeMethods.GetClientRect(form.Handle, ref activeRect);
            hwndMag = NativeMethods.CreateWindow((int)ExtendedWindowStyles.WS_EX_TRANSPARENT, NativeMethods.WC_MAGNIFIER,
                "MagnifierWindow", (int)WindowStyles.WS_CHILD | (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR |
                //"MagnifierWindow", (int)WindowStyles.WS_CHILD |
                (int)WindowStyles.WS_VISIBLE,
                activeRect.left, activeRect.top, activeRect.right, activeRect.bottom, form.Handle, IntPtr.Zero, hInst, IntPtr.Zero);

            if (hwndMag == IntPtr.Zero)
            {
                return;
            }

            // Set the magnification factor.
            Transformation matrix = new Transformation(magnification);
            NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
        }

        protected void RemoveMagnifier()
        {
            if (initialized)
                NativeMethods.MagUninitialize();
        }

        protected virtual void Dispose(bool disposing)
        {
            timer.Enabled = false;
            if (disposing)
                timer.Dispose();
            timer = null;
            form.Resize -= form_Resize;
            RemoveMagnifier();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
