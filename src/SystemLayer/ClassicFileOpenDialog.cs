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
    internal sealed class ClassicFileOpenDialog
        : ClassicFileDialog,
          IFileOpenDialog
    {
        private OpenFileDialog OpenFileDialog
        {
            get
            {
                return this.FileDialog as OpenFileDialog;
            }
        }

        public bool CheckFileExists
        {
            get
            {
                return this.OpenFileDialog.CheckFileExists;
            }

            set
            {
                this.OpenFileDialog.CheckFileExists = value;
            }
        }

        public bool Multiselect
        {
            get
            {
                return this.OpenFileDialog.Multiselect;
            }

            set
            {
                this.OpenFileDialog.Multiselect = value;
            }
        }

        public string[] FileNames
        {
            get
            {
                return this.OpenFileDialog.FileNames;
            }
        }

        public ClassicFileOpenDialog()
            : base(new System.Windows.Forms.OpenFileDialog())
        {
        }
    }
}
