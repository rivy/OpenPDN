/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    // TODO: Remove
#if false
    [Obsolete("Use Function<bool, object> instead.", true)]
    public delegate bool BoolObjectDelegate(object o);

    [Obsolete("Use Procedure<bool> instead.", true)]
    public delegate bool BoolVoidDelegate();

    [Obsolete("Use EventArgs<T> instead", true)]
    public class DataEventArgs<T>
        : EventArgs
    {
        private T data;
        public T Data
        {
            get
            {
                return data;
            }
        }

        public DataEventArgs(T data)
        {
            this.data = data;
        }
    }

    [Obsolete("Use Procedure<object> instead", true)]
    public delegate void VoidObjectDelegate(object obj);

    [Obsolete("Use Procedure instead", true)]
    public delegate void VoidVoidDelegate();

    [Obsolete("Use Procedure instead", true)]
    public delegate void ProcedureDelegate();

    [Obsolete("Use Procedure`1 instead", true)]
    public delegate void UnaryProcedureDelegate<T>(T parameter);

    [Obsolete("Use Procedure`2 instead", true)]
    public delegate void BinaryProcedureDelegate<T, U>(T first, U second);

    [Obsolete("Use Function`2 instead", true)]
    public delegate R UnaryFunctionDelegate<R, T>(T parameter);

    [Obsolete("Use Function`3 instead", true)]
    public delegate R BinaryFunctionDelegate<R, T, U>(T first, U second);
#endif
}