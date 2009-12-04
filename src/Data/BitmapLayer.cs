/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Threading;

namespace PaintDotNet
{
    [Serializable]
    public class BitmapLayer
        : Layer,
          IDeserializationCallback
    {
        public override Surface RenderThumbnail(int maxEdgeLength)
        {
            Size thumbSize = Utility.ComputeThumbnailSize(this.Size, maxEdgeLength);
            Surface thumb = new Surface(thumbSize);

            thumb.SuperSamplingFitSurface(this.surface);

            Surface thumb2 = new Surface(thumbSize);
            thumb2.ClearWithCheckboardPattern();
            UserBlendOps.NormalBlendOp nbop = new UserBlendOps.NormalBlendOp();
            nbop.Apply(thumb2, thumb);

            thumb.Dispose();
            thumb = null;

            return thumb2;
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                try
                {
                    if (disposing)
                    {
                        if (surface != null)
                        {
                            surface.Dispose();
                            surface = null;
                        }
                    }
                }
                    
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        [NonSerialized]
        private BinaryPixelOp compiledBlendOp = null;

        private void CompileBlendOp()
        {
            bool isDefaultOp = (properties.blendOp.GetType() == UserBlendOps.GetDefaultBlendOp());

            if (this.Opacity == 255)
            {
                this.compiledBlendOp = properties.blendOp;
            }
            else
            {
                this.compiledBlendOp = properties.blendOp.CreateWithOpacity(this.Opacity);
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            compiledBlendOp = null;
            base.OnPropertyChanged (propertyName);
        }

        [Serializable]
        internal sealed class BitmapLayerProperties
            : ICloneable,
              ISerializable
        {
            public UserBlendOp blendOp;
            internal int opacity; // this is ONLY used when loading older version PDN files! should normally equal -1

            private const string blendOpTag = "blendOp";
            private const string opacityTag = "opacity";

            public static string BlendOpName
            {
                get
                {
                    return PdnResources.GetString("BitmapLayer.Properties.BlendOp.Name");
                }
            }

            public BitmapLayerProperties(UserBlendOp blendOp)
            {
                this.blendOp = blendOp;
                this.opacity = -1;
            }

            public BitmapLayerProperties(BitmapLayerProperties cloneMe)
            {
                this.blendOp = cloneMe.blendOp;
                this.opacity = -1;
            }

            public object Clone()
            {
                return new BitmapLayerProperties(this);
            }

            public BitmapLayerProperties(SerializationInfo info, StreamingContext context)
            {
                this.blendOp = (UserBlendOp)info.GetValue(blendOpTag, typeof(UserBlendOp));

                // search for 'opacity' and load it if it exists
                this.opacity = -1;

                foreach (SerializationEntry entry in info)
                {
                    if (entry.Name == opacityTag)
                    {
                        this.opacity = (int)((byte)entry.Value);
                        break;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(blendOpTag, this.blendOp);
            }
        }

        private BitmapLayerProperties properties;
        private Surface surface;

        public override object SaveProperties()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            object baseProperties = base.SaveProperties();
            return new List(properties.Clone(), new List(baseProperties, null));
        }

        public override void LoadProperties(object oldState, bool suppressEvents)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            List list = (List)oldState;

            // Get the base class' state, and our state
            LayerProperties baseState = (LayerProperties)list.Tail.Head;
            BitmapLayerProperties blp = (BitmapLayerProperties)(((List)oldState).Head);

            // Opacity is only couriered for compatibility with PDN v2.0 and v1.1
            // files. It should not be present in v2.1+ files (well, it'll be
            // part of the base class' serialization)
            if (blp.opacity != -1)
            {
                baseState.opacity = (byte)blp.opacity;
                blp.opacity = -1;
            }            

            // Have the base class load its properties
            base.LoadProperties(baseState, suppressEvents);

            // Now load our properties, and announce them to the world
            bool raiseBlendOp = false;

            if (blp.blendOp.GetType() != properties.blendOp.GetType())
            {
                if (!suppressEvents)
                {
                    raiseBlendOp = true;
                    OnPropertyChanging(BitmapLayerProperties.BlendOpName);
                }
            }

            this.properties = (BitmapLayerProperties)blp.Clone();
            this.compiledBlendOp = null;

            Invalidate();

            if (raiseBlendOp)
            {
                OnPropertyChanged(BitmapLayerProperties.BlendOpName);
            }
        }

        public void SetBlendOp(UserBlendOp blendOp)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (blendOp.GetType() != properties.blendOp.GetType())
            {
                OnPropertyChanging(BitmapLayerProperties.BlendOpName);
                properties.blendOp = blendOp;
                compiledBlendOp = null;
                Invalidate();
                OnPropertyChanged(BitmapLayerProperties.BlendOpName);
            }
        }

        public override object Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            return (object)new BitmapLayer(this);
        }

        public Surface Surface
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return surface;
            }
        }

        public UserBlendOp BlendOp
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return properties.blendOp;
            }
        }

        public BitmapLayer(int width, int height)
            : this(width, height, ColorBgra.FromBgra(255, 255, 255, 0))
        {
        }

        public BitmapLayer(int width, int height, ColorBgra fillColor)
            : base(width, height)
        {
            this.surface = new Surface(width, height);
            // clear to see-through white, 0x00ffffff
            this.Surface.Clear(fillColor);
            this.properties = new BitmapLayerProperties(UserBlendOps.CreateDefaultBlendOp());
        }

        /// <summary>
        /// Creates a new BitmapLayer of the same size as the given Surface, and copies the 
        /// pixels from the given Surface.
        /// </summary>
        /// <param name="surface">The Surface to copy pixels from.</param>
        public BitmapLayer(Surface surface)
            : this(surface, false)
        {
        }

        /// <summary>
        /// Creates a new BitmapLayer of the same size as the given Surface, and either
        /// copies the pixels of the given Surface or takes ownership of it.
        /// </summary>
        /// <param name="surface">The Surface.</param>
        /// <param name="takeOwnership">
        /// true to take ownership of the surface (make sure to Dispose() it yourself), or
        /// false to copy its pixels
        /// </param>
        public BitmapLayer(Surface surface, bool takeOwnership)
            : base(surface.Width, surface.Height)
        {
            if (takeOwnership)
            {
                this.surface = surface;
            }
            else
            {
                this.surface = surface.Clone();
            }

            this.properties = new BitmapLayerProperties(UserBlendOps.CreateDefaultBlendOp());
        }

        protected BitmapLayer(BitmapLayer copyMe)
            : base(copyMe)
        {
            this.surface = copyMe.Surface.Clone();
            this.properties = (BitmapLayerProperties)copyMe.properties.Clone();
        }

        protected unsafe override void RenderImpl(RenderArgs args, Rectangle roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (Opacity == 0)
            {
                return;
            }

            if (compiledBlendOp == null)
            {
                CompileBlendOp();
            }

            for (int y = roi.Top; y < roi.Bottom; ++y)
            {
                ColorBgra *dstPtr = args.Surface.GetPointAddressUnchecked(roi.Left, y);
                ColorBgra *srcPtr = this.surface.GetPointAddressUnchecked(roi.Left, y);

                this.compiledBlendOp.Apply(dstPtr, srcPtr, roi.Width);
            }
        }

        protected unsafe override void RenderImpl(RenderArgs args, Rectangle[] rois, int startIndex, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (Opacity == 0)
            {
                return;
            }

            if (compiledBlendOp == null)
            {
                CompileBlendOp();
            }

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra *dstPtr = args.Surface.GetPointAddressUnchecked(roi.Left, y);
                    ColorBgra *srcPtr = this.surface.GetPointAddressUnchecked(roi.Left, y);

                    this.compiledBlendOp.Apply(dstPtr, srcPtr, roi.Width);
                }
            }
        }

        public override PdnBaseForm CreateConfigDialog()
        {
            BitmapLayerPropertiesDialog blpd = new BitmapLayerPropertiesDialog();
            blpd.Layer = this;
            return blpd;
        }

        public void OnDeserialization(object sender)
        {
            if (this.properties.opacity != -1)
            {
                this.PushSuppressPropertyChanged();
                base.Opacity = (byte)this.properties.opacity;
                this.properties.opacity = -1;
                this.PopSuppressPropertyChanged();
            }

            this.compiledBlendOp = null;
        }
    }
}
