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
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class SaveProgressDialog
        : CallbackWithProgressDialog
    {
        private FileType fileType;
        private Document document;
        private Stream stream;
        private SaveConfigToken saveConfigToken;
        private Surface scratchSurface;

        private void SaveCallback()
        {
            fileType.Save(
                this.document, 
                this.stream, 
                this.saveConfigToken, 
                this.scratchSurface,
                new ProgressEventHandler(ProgressHandler), 
                true);
        }

        public SaveProgressDialog(Control owner)
            : base(owner, 
                   PdnResources.GetString("SaveProgressDialog.Title"), 
                   PdnResources.GetString("SaveProgressDialog.Description"))
        {
            this.Icon = Utility.ImageToIcon(PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference, Utility.TransparentKey);
        }

        public void Save(Stream dstStream, Document srcDocument, FileType dstFileType, SaveConfigToken parameters, Surface saveScratchSurface)
        {
            this.document = srcDocument;
            this.fileType = dstFileType;
            this.stream = dstStream;
            this.saveConfigToken = parameters;
            this.scratchSurface = saveScratchSurface;
            DialogResult dr = this.ShowDialog(false, !dstFileType.SavesWithProgress , new ThreadStart(SaveCallback));
        }

        private void ProgressHandler(object sender, ProgressEventArgs e)
        {
            if (!MarqueeMode)
            {
                Progress = (int)e.Percent;
            }
        }
    }
}
