/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class SaveConfigWidget 
        : UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        protected SaveConfigToken token;

        [Browsable(false)]
        public SaveConfigToken Token
        {
            get
            {
                return this.token;
            }

            set
            {
                this.token = value;
                OnTokenChanged();

                if (this.token != null)
                {
                    InitWidgetFromToken((SaveConfigToken)this.token.Clone());
                }
            }
        }

        public event EventHandler TokenChanged;
        protected virtual void OnTokenChanged()
        {
            if (TokenChanged != null)
            {
                TokenChanged(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        protected FileType fileType;
        public FileType FileType
        {
            get
            {
                return fileType;
            }
        }

        internal SaveConfigWidget(FileType fileType)
        {
            InitializeComponent();
            this.fileType = fileType;
        }

        public SaveConfigWidget()
        {
            InitializeComponent();
            InitFileType();
        }

        public void UpdateToken()
        {
            InitTokenFromWidget();
            OnTokenChanged();
        }

        /// <summary>
        /// This method msut be overriden in derived classes.
        /// In this method you must initialize the protected fileType field.
        /// </summary>
        protected virtual void InitFileType()
        {
            //throw new InvalidOperationException("InitFileType was not implemented, or the derived method called the base method");
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the given EffectToken
        /// and use them to properly initialize the dialog's user interface elements.
        /// </summary>
        protected virtual void InitWidgetFromToken(SaveConfigToken sourceToken)
        {
            //throw new InvalidOperationException("InitWidgetFromToken was not implemented, or the derived method called the base method");
        }

        protected void InitWidgetFromToken()
        {   
            // If we don't check for null, we get awful errors in the designer.
            // Good idea to check for that anyway, yeah?
            if (token != null)
            {
                InitWidgetFromToken((SaveConfigToken)token.Clone());
            }
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the dialog box
        /// and use them to properly initialize theEffectToken.
        /// </summary>
        protected virtual void InitTokenFromWidget()
        {
            //throw new InvalidOperationException("InitTokenFromWidget was not implemented, or the derived method called the base method");
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
            InitWidgetFromToken();
            UpdateToken();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            components = new System.ComponentModel.Container();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ResumeLayout(false);
        }
        #endregion
    }
}
