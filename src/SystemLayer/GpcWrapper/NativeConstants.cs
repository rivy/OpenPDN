/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer.GpcWrapper
{
    internal static class NativeConstants
    {
        public enum gpc_op                                 
        {
            GPC_DIFF = 0,
            GPC_INT = 1,
            GPC_XOR = 2,
            GPC_UNION = 3
        }
    }
}
