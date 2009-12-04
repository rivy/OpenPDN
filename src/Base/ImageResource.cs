/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    public abstract class ImageResource
        : Resource<Image>
    {
        private sealed class FromImageResource
            : ImageResource
        {
            protected override Image Load()
            {
                return (Image)Reference.Clone();
            }

            public FromImageResource(Image image)
                : base(image)
            {
            }
        }

        public static ImageResource FromImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            ImageResource resource = new FromImageResource(image);
            return resource;
        }

        protected ImageResource()
            : base()
        {
        }

        protected ImageResource(Image image)
            : base(image)
        {
        }
    }
}
