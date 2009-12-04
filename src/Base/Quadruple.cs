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
    public static class Quadruple
    {
        public static Quadruple<T, U, V, W> Create<T, U, V, W>(T first, U second, V third, W fourth)
        {
            return new Quadruple<T, U, V, W>(first, second, third, fourth);
        }
    }
}
