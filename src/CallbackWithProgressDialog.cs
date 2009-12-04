/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal abstract class CallbackWithProgressDialog
    {
        private ProgressDialog dialog;
        private string dialogTitle;
        private string dialogDescription;
        private int progress;
        private Thread thread;
        private Control owner;
        private ThreadStart threadCallback;
        private Exception exception = null;
        private Point startPos = Point.Empty;
        private bool setStartPos = false;
        private Icon icon = null;
        private bool cancelled = false;

        /// <summary>
        /// Used to define the top center of the dialog window when it is created.
        /// If this property is not set, then a Windows-chosen location will be used.
        /// </summary>
        public Point StartPos
        {
            get
            {
                return startPos;
            }

            set
            {
                setStartPos = true;
                startPos = value;
            }
        }

        public Icon Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
            }
        }

        protected Control Owner
        {
            get
            {
                return this.owner;
            }
        }

        protected int Progress
        {
            get
            {
                return progress;
            }

            set
            {
                progress = value;

                if (dialog.IsHandleCreated && dialog.InvokeRequired)
                {
                    dialog.BeginInvoke(new Procedure(DoProgressUpdate), null);
                }
                else if (dialog.IsHandleCreated && !dialog.InvokeRequired)
                {
                    DoProgressUpdate();
                }
            }
        }

        public bool Cancelled
        {
            get
            {
                return this.dialog.Cancelled;
            }
        }

        public string Description
        {
            get
            {
                return this.dialogDescription;
            }

            set
            {
                this.dialogDescription = value;

                if (this.dialog != null)
                {
                    this.dialog.Description = value;
                }
            }
        }

        public bool MarqueeMode
        {
            get
            {
                return this.dialog.MarqueeMode;
            }

            set
            {
                this.dialog.MarqueeMode = value;
            }
        }

        private void DoProgressUpdate()
        {
            dialog.Value = progress;
            dialog.Update();
        }

        private void BackgroundCallback()
        {
            this.exception = null;

            try
            {
                threadCallback();
            }

            catch (Exception ex)
            {
                this.exception = ex;
            }

            finally
            {
                try
                {
                    dialog.BeginInvoke(new Procedure(dialog.ExternalFinish), null);
                }

                catch (Exception)
                {
                }
            }
        }

        public CallbackWithProgressDialog(Control owner, string dialogTitle, string dialogDescription)
        {
            this.owner = owner;
            this.dialogTitle = dialogTitle;
            this.dialogDescription = dialogDescription;
        }

        protected DialogResult ShowDialog(bool cancellable, bool marqueeProgress, ThreadStart callback)
        {
            this.threadCallback = callback;
            DialogResult dr = DialogResult.Cancel;
            
            using (this.dialog = new ProgressDialog())
            {
                dialog.Text = dialogTitle;
                dialog.Description = dialogDescription;

                if (marqueeProgress)
                {
                    dialog.PercentTextVisible = false;
                    dialog.MarqueeMode = true;
                }

                if (icon != null)
                {
                    dialog.Icon = icon;
                }

                EventHandler leh = new EventHandler(dialog_Load);
                dialog.Load += leh;
                dialog.Cancellable = cancellable;

                if (cancellable)
                {
                    dialog.CancelClick += new EventHandler(dialog_CancelClick);
                }

                thread = new Thread(new ThreadStart(BackgroundCallback));
                this.Progress = 0;
            
                if (setStartPos)
                {
                    dialog.Location = new Point(StartPos.X - (dialog.Width / 2), StartPos.Y);;
                    dialog.StartPosition = FormStartPosition.Manual;
                }
                else
                {
                    dialog.StartPosition = FormStartPosition.CenterParent;
                }

                dr = dialog.ShowDialog(owner);
                dialog.Load -= leh;

                if (cancellable)
                {
                    this.cancelled = dialog.Cancelled;
                    dialog.CancelClick -= new EventHandler(dialog_CancelClick);
                }

                if (exception != null)
                {
                    throw new WorkerThreadException("Worker thread threw an exception", exception);
                }
            }

            return dr;
        }

        private void dialog_Load(object sender, EventArgs e)
        {
            thread.Start();
        }

        private void dialog_CancelClick(object sender, EventArgs e)
        {
            OnCancelClick();

            using (new WaitCursorChanger(this.dialog))
            {
                thread.Join();
            }
        }

        protected virtual void OnCancelClick()
        {
        }
    }
}
