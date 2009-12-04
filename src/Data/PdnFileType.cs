/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace PaintDotNet
{
    public sealed class PdnFileType
        : FileType
    {
        public PdnFileType()
            : base(PdnInfo.GetBareProductName(),
                   FileTypeFlags.SavesWithProgress | 
                       FileTypeFlags.SupportsCustomHeaders |
                       FileTypeFlags.SupportsLayers |
                       FileTypeFlags.SupportsLoading |
                       FileTypeFlags.SupportsSaving,
                   new string[] { ".pdn" })
        {
        }

        protected override Document OnLoad(Stream input)
        {
            return Document.FromStream(input);
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            if (callback == null)
            {
                input.SaveToStream(output);
            }
            else
            {
                UpdateProgressTranslator upt = new UpdateProgressTranslator(ApproximateMaxOutputOffset(input), callback);
                input.SaveToStream(output, new IOEventHandler(upt.IOEventHandler));
            }
        }

        public override bool IsReflexive(SaveConfigToken token)
        {
            return true;
        }

        private sealed class UpdateProgressTranslator
        {
            private long maxBytes;
            private long totalBytes;
            private ProgressEventHandler callback;

            public void IOEventHandler(object sender, IOEventArgs e)
            {
                double percent;

                lock (this)
                {
                    totalBytes += (long)e.Count;
                    percent = Math.Max(0.0, Math.Min(100.0, ((double)totalBytes * 100.0) / (double)maxBytes));
                }

                callback(sender, new ProgressEventArgs(percent));
            }

            public UpdateProgressTranslator(long maxBytes, ProgressEventHandler callback)
            {
                this.maxBytes = maxBytes;
                this.callback = callback;
                this.totalBytes = 0;
            }
        }

        private long ApproximateMaxOutputOffset(Document measureMe)
        {
            return (long)measureMe.Layers.Count * (long)measureMe.Width * (long)measureMe.Height * (long)ColorBgra.SizeOf;
        }
    }
}
