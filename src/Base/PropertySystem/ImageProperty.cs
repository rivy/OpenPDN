/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Drawing;

namespace PaintDotNet.PropertySystem
{
    public sealed class ImageProperty
        : Property<ImageResource>
    {
        public ImageProperty(object propertyName)
            : this(propertyName, null, false)
        {
        }

        public ImageProperty(object propertyName, ImageResource image)
            : this(propertyName, image, false)
        {
        }

        public ImageProperty(object propertyName, ImageResource image, bool readOnly)
            : base(propertyName, image, readOnly, ValueValidationFailureResult.Ignore)
        {
        }

        private ImageProperty(ImageProperty cloneMe, ImageProperty sentinelNotUsed)
            : base(cloneMe, sentinelNotUsed)
        {
        }

        public override Property Clone()
        {
            return new ImageProperty(this, this);
        }

        protected override bool ValidateNewValueT(ImageResource newValue)
        {
            return true;
        }

        protected override ImageResource OnClampNewValueT(ImageResource newValue)
        {
            return newValue;
        }
    }
}
