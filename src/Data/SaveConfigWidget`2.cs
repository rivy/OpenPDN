/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    public abstract class SaveConfigWidget<TFileType, TToken>
        : SaveConfigWidget
          where TFileType : FileType
          where TToken : SaveConfigToken
    {
        private FileType fileTypeFromCtor;

        public new TToken Token
        {
            get
            {
                return (TToken)base.Token;
            }

            set
            {
                base.Token = value;
            }
        }

        public new TFileType FileType
        {
            get
            {
                return (TFileType)base.FileType;
            }
        }

        protected override void InitFileType()
        {
            // This method won't actually be called, but is implemented anyway.
            this.fileType = this.fileTypeFromCtor;
        }

        protected abstract void InitWidgetFromToken(TToken sourceToken);

        protected override sealed void InitWidgetFromToken(SaveConfigToken sourceToken)
        {
            InitWidgetFromToken((TToken)sourceToken);
        }

        protected abstract TToken CreateTokenFromWidget();

        protected override sealed void InitTokenFromWidget()
        {
            TToken token = CreateTokenFromWidget();
            this.Token = token;
            base.InitTokenFromWidget();
        }

        public SaveConfigWidget(FileType fileType)
            : base(fileType)
        {
            this.fileTypeFromCtor = fileType;
        }
    }
}
