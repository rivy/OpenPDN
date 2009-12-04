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
    /// <summary>
    /// A strongly typed version of FileType.
    /// </summary>
    public abstract class FileType<TToken, TWidget>
        : FileType
          where TToken : SaveConfigToken
          where TWidget : SaveConfigWidget
    {
        public FileType(string name, FileTypeFlags flags, string[] extensions)
            : base(name, flags, extensions)
        {
        }

        protected virtual bool IsReflexive(TToken token)
        {
            return false;
        }

        public override sealed bool IsReflexive(SaveConfigToken token)
        {
            return IsReflexive((TToken)token);
        }

        protected abstract void OnSaveT(Document input, Stream output, TToken token, Surface scratchSurface, ProgressEventHandler progressCallback);

        protected override sealed void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            OnSaveT(input, output, (TToken)token, scratchSurface, callback);
        }

        public void Save(Document input, Stream output, TToken token, Surface scratchSurface, ProgressEventHandler callback, bool rememberToken)
        {
            base.Save(input, output, token, scratchSurface, callback, rememberToken);
        }

        protected abstract TWidget OnCreateSaveConfigWidgetT();

        public override sealed SaveConfigWidget CreateSaveConfigWidget()
        {
            return OnCreateSaveConfigWidgetT();
        }

        protected abstract TToken OnCreateDefaultSaveConfigTokenT();

        protected override sealed SaveConfigToken OnCreateDefaultSaveConfigToken()
        {
            return OnCreateDefaultSaveConfigTokenT();
        }

        public new TToken CreateDefaultSaveConfigToken()
        {
            return (TToken)base.CreateDefaultSaveConfigToken();
        }

        protected virtual TToken GetSaveConfigTokenFromSerializablePortionT(object portion)
        {
            return (TToken)portion;
        }

        protected override sealed SaveConfigToken GetSaveConfigTokenFromSerializablePortion(object portion)
        {
            return GetSaveConfigTokenFromSerializablePortionT(portion);
        }

        protected virtual object GetSerializablePortionOfSaveConfigToken(TToken token)
        {
            return token;
        }

        protected override sealed object GetSerializablePortionOfSaveConfigToken(SaveConfigToken token)
        {
            return GetSerializablePortionOfSaveConfigToken((TToken)token);
        }
    }
}
