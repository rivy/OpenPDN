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
    /// <summary>
    /// Provides access to a cached group of boxed, commonly used constants.
    /// This helps to avoid boxing overhead, much of which consists of transferring
    /// the item to the heap. Unboxing, on the other hand, is quite cheap.
    /// This is commonly used to pass index values to worker threads.
    /// </summary>
    public sealed class BoxedConstants
    {
        private static object[] boxedInt32 = new object[1024];
        private static object boxedTrue = (object)true;
        private static object boxedFalse = (object)false;

        public static object GetInt32(int value)
        {
            if (value >= boxedInt32.Length || value < 0)
            {
                return (object)value;
            }

            if (boxedInt32[value] == null)
            {
                boxedInt32[value] = (object)value;
            }

            return boxedInt32[value];
        }

        public static object GetBoolean(bool value)
        {
            return value ? boxedTrue : boxedFalse;
        }

        static BoxedConstants()
        {
        }

        private BoxedConstants()
        {
        }
    }
}
