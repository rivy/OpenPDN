/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace PaintDotNet
{
    public sealed class CallbackOnDispose
        : IDisposable
    {
        private Procedure callback;

        public CallbackOnDispose(Procedure callback)
        {
            this.callback = callback;
        }

        ~CallbackOnDispose()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Procedure callback2 = Interlocked.Exchange(ref this.callback, null);

            if (callback2 != null)
            {
                callback2();
            }
        }
    }
}
