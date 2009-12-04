using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet.Data
{
    [Serializable]
    public sealed class HDPhotoSaveConfigToken
        : SaveConfigToken
    {
        private int quality;
        public int Quality
        {
            get
            {
                return this.quality;
            }

            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("quality must be in the range [0, 100]");
                }

                this.quality = value;
            }
        }

        private int bitDepth;
        public int BitDepth
        {
            get
            {
                return this.bitDepth;
            }

            set
            {
                if (value == 24 || value == 32)
                {
                    this.bitDepth = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("BitDepth may only be 24 or 32");
                }
            }
        }

        public HDPhotoSaveConfigToken(int quality, int bitDepth)
        {
            this.quality = quality;
            this.bitDepth = bitDepth;
        }

        public override void Validate()
        {
            if (this.quality < 0 || this.quality > 100)
            {
                throw new ArgumentOutOfRangeException("quality must be in the range [0, 100]");
            }

            if (!(this.bitDepth == 24 || this.bitDepth == 32))
            {
                throw new ArgumentOutOfRangeException("BitDepth may only be 24 or 32");
            }

            base.Validate();
        }

        public override object Clone()
        {
            return new HDPhotoSaveConfigToken(this.quality, this.bitDepth);
        }
    }
}
