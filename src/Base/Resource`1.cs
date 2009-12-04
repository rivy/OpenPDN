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
    public abstract class Resource<T>
    {
        protected T resource;

        public T Reference
        {
            get
            {
                if (this.resource == null)
                {
                    this.resource = Load();
                }

                return this.resource;
            }
        }

        public T GetCopy()
        {
            return Load();
        }

        protected abstract T Load();

        protected Resource()
        {
            this.resource = default(T);
        }

        protected Resource(T resource)
        {
            this.resource = resource;
        }
    }
}
