/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    internal sealed class ClassicFileSaveDialog
        : ClassicFileDialog,
          IFileSaveDialog
    {
        private SaveFileDialog SaveFileDialog
        {
            get
            {
                return this.FileDialog as SaveFileDialog;
            }
        }

        public bool AddExtension
        {
            get
            {
                return this.SaveFileDialog.AddExtension;
            }

            set
            {
                this.SaveFileDialog.AddExtension = value;
            }
        }

        public string FileName
        {
            get
            {
                return this.SaveFileDialog.FileName;
            }

            set
            {
                this.SaveFileDialog.FileName = value;
            }
        }

        public bool OverwritePrompt
        {
            get
            {
                return this.SaveFileDialog.OverwritePrompt;
            }

            set
            {
                this.SaveFileDialog.OverwritePrompt = value;
            }
        }

        public ClassicFileSaveDialog()
            : base(new SaveFileDialog())
        {
        }
    }
}
