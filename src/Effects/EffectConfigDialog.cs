/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class EffectConfigDialog
        : PdnBaseForm
    {
        private Surface effectSourceSurface;
        private PdnRegion effectSelection = null;

        /// <summary>
        /// This is the surface that will be used as the source for rendering.
        /// Its contents will not change for the lifetime of this dialog box
        /// ("lifetime" being defined as "until Close() is called")
        /// Treat this object as read-only. In your OnLoad method, feel free
        /// to do any analysis of this surface to populate the dialog box.
        /// </summary>
        [Browsable(false)]
        public Surface EffectSourceSurface
        {
            get
            {
                return effectSourceSurface;
            }

            set
            {
                effectSourceSurface = value;
            }
        }

        [Browsable(false)]
        public PdnRegion Selection
        {
            get
            {
                if (this.DesignMode)
                {
                    return null;
                }
                else
                {
                    if (effectSelection == null || effectSelection.IsEmpty()) 
                    {
                        effectSelection = new PdnRegion();
                        effectSelection.MakeInfinite();
                    }

                    return effectSelection;
                }
            }

            set
            {
                if (effectSelection != null)
                {
                    effectSelection.Dispose();
                    effectSelection = null;
                }

                effectSelection = value;
            }
        }

        internal virtual void OnBeforeConstructor(object context)
        {
        }

        internal EffectConfigDialog(object context)
        {
            OnBeforeConstructor(context);
            Constructor();
        }

        public EffectConfigDialog()
        {
            Constructor();
        }

        private void Constructor()
        {
            InitializeComponent();
            InitialInitToken();
            effectSelection = new PdnRegion();
            effectSelection.MakeInfinite();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // 
            // EffectConfigDialog
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EffectConfigDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ResumeLayout(false);
        }

        /// <summary>
        /// Overrides Form.OnLoad.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Derived classes MUST call this base method if they override it!
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                InitDialogFromToken();
                FinishTokenUpdate();
            }

            catch (Exception ex)
            {
                SystemLayer.Tracing.Ping(ex.ToString());
                throw;
            }
        }

        [Browsable(false)]
        public event EventHandler EffectTokenChanged;
        protected virtual void OnEffectTokenChanged()
        {
            if (EffectTokenChanged != null)
            {
                EffectTokenChanged(this, EventArgs.Empty);
            }
        }

        [Obsolete("Use FinishTokenUpdate() instead", true)]
        public void UpdateToken()
        {
            FinishTokenUpdate();
        }

        public void FinishTokenUpdate()
        {
            InitTokenFromDialog();
            OnEffectTokenChanged();
        }

        /// <summary>
        /// This method must be overriden in the derived classes.
        /// In this you initialize the default values for the token, and
        /// thus the default values for the dialog box.
        /// The job of this function is to initialize this.theEffectToken with
        /// a non-null reference.
        /// </summary>
        protected virtual void InitialInitToken()
        {
            //throw new InvalidOperationException("InitialInitToken was not implemented, or the derived method called the base method");
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the given EffectToken
        /// and use them to properly initialize the dialog's user interface elements.
        /// Make sure to read values from the passed-in effectToken
        /// </summary>
        protected virtual void InitDialogFromToken(EffectConfigToken effectTokenCopy)
        {
            //throw new InvalidOperationException("InitDialogFromToken was not implemented, or the derived method called the base method");
        }

        protected void InitDialogFromToken()
        {   
            // If we don't check for null, we get awful errors in the designer.
            // Good idea to check for that anyway, yeah?
            if (theEffectToken != null)
            {
                InitDialogFromToken((EffectConfigToken)theEffectToken.Clone());
            }
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the dialog box
        /// and use them to properly initialize theEffectToken.
        /// </summary>
        protected virtual void InitTokenFromDialog()
        {
            //throw new InvalidOperationException("InitTokenFromDialog was not implemented, or the derived method called the base method");
        }

        private Effect effect;

        [Browsable(false)]
        public Effect Effect
        {
            get
            {
                return effect;
            }

            set
            {
                effect = value;

                if (effect.Image != null)
                {
                    this.Icon = Utility.ImageToIcon(effect.Image, Utility.TransparentKey);
                }
                else
                {
                    this.Icon = null;
                }
            }
        }

        protected EffectConfigToken theEffectToken;

        [Browsable(false)]
        public EffectConfigToken EffectToken
        {
            get
            {
                return theEffectToken;
            }

            set
            {
                theEffectToken = value;
                OnEffectTokenChanged();
                InitDialogFromToken();
            }
        }
    }
}
