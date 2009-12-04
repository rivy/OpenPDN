/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Data;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PaintDotNet.Data
{
    public sealed class HDPhotoFileType
        : FileType
    {
        public HDPhotoFileType()
            : base(Strings.HDPhotoFileType_Name,
                   FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving,
                   new string[] { ".wdp", ".hdp" })
        {
        }

        public override SaveConfigWidget CreateSaveConfigWidget()
        {
            return new HDPhotoSaveConfigWidget();
        }

        public override bool IsReflexive(SaveConfigToken token)
        {
            return false;
        }

        protected override SaveConfigToken OnCreateDefaultSaveConfigToken()
        {
            return new HDPhotoSaveConfigToken(90, 32);
        }
        
        protected override Document OnLoad(Stream input)
        {
            Document document;

            // WIC does not support MTA, so we must marshal this stuff to another thread
            // that is then guaranteed to be STA.
            switch (Thread.CurrentThread.GetApartmentState())
            {
                case ApartmentState.Unknown:
                case ApartmentState.MTA:
                    OnLoadArgs ola = new OnLoadArgs();
                    ola.input = input;
                    ola.output = null;

                    ParameterizedThreadStart pts = new ParameterizedThreadStart(OnLoadThreadProc);
                    Thread staThread = new Thread(pts);
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start(ola);
                    staThread.Join();

                    if (ola.exception != null)
                    {
                        throw new ApplicationException("OnLoadImpl() threw an exception", ola.exception);
                    }

                    document = ola.output;
                    break;

                case ApartmentState.STA:
                    document = OnLoadImpl(input);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            return document;
        }

        private sealed class OnLoadArgs
        {
            public Stream input;
            public Document output;

            public Exception exception;
        }

        private void OnLoadThreadProc(object context)
        {
            OnLoadArgs ola = (OnLoadArgs)context;

            try
            {
                ola.output = OnLoadImpl(ola.input);
            }

            catch (Exception ex)
            {
                ola.exception = ex;
            }
        }

        private Document OnLoadImpl(Stream input)
        {
            WmpBitmapDecoder wbd = new WmpBitmapDecoder(input, BitmapCreateOptions.None, BitmapCacheOption.None);
            BitmapFrame frame0 = wbd.Frames[0];

            Document output = new Document(frame0.PixelWidth, frame0.PixelHeight);
            output.DpuUnit = MeasurementUnit.Inch;
            output.DpuX = frame0.DpiX;
            output.DpuY = frame0.DpiY;

            BitmapLayer layer = Layer.CreateBackgroundLayer(output.Width, output.Height);
            MemoryBlock memoryBlock = layer.Surface.Scan0;
            IntPtr scan0 = memoryBlock.Pointer;

            FormatConvertedBitmap fcb = new FormatConvertedBitmap(frame0, System.Windows.Media.PixelFormats.Bgra32, null, 0);

            fcb.CopyPixels(Int32Rect.Empty, scan0, (int)memoryBlock.Length, layer.Surface.Stride);
            output.Layers.Add(layer);

            BitmapMetadata hdMetadata = (BitmapMetadata)frame0.Metadata;
            CopyMetadataTo(output.Metadata, hdMetadata);

            // WPF doesn't give us an IDisposable implementation on its types
            Utility.GCFullCollect();

            return output;
        }

        private void CopyStringTagTo(BitmapMetadata dst, string dstPropertyName, Metadata src, ExifTagID srcTagID)
        {
            PropertyItem[] pis = src.GetExifValues(srcTagID);

            if (pis.Length > 0)
            {
                PropertyInfo pi = dst.GetType().GetProperty(dstPropertyName);
                string piValue = Exif.DecodeAsciiValue(pis[0]);

                try
                {
                    pi.SetValue(dst, piValue, null);
                }

                catch (Exception)
                {
                    // *shrug*
                }
            }
        }

        private void CopyMetadataTo(BitmapMetadata dst, Metadata src)
        {
            // ApplicationName
            CopyStringTagTo(dst, "ApplicationName", src, ExifTagID.Software);

            // Author
            PropertyItem[] authorsPI = src.GetExifValues(ExifTagID.Artist);
            if (authorsPI.Length > 0)
            {
                List<string> authors = new List<string>();
                foreach (PropertyItem pi in authorsPI)
                {
                    string author = Exif.DecodeAsciiValue(pi);
                    authors.Add(author);
                }
                ReadOnlyCollection<string> authorsRO = new ReadOnlyCollection<string>(authors);
                dst.Author = authorsRO;
            }

            CopyStringTagTo(dst, "CameraManufacturer", src, ExifTagID.Make);
            CopyStringTagTo(dst, "CameraModel", src, ExifTagID.Model);
            CopyStringTagTo(dst, "Copyright", src, ExifTagID.Copyright);
            CopyStringTagTo(dst, "Title", src, ExifTagID.ImageDescription);

            PropertyItem[] dateTimePis = src.GetExifValues(ExifTagID.DateTime);
            if (dateTimePis.Length > 0)
            {
                string dateTime = Exif.DecodeAsciiValue(dateTimePis[0]);

                try
                {
                    dst.DateTaken = dateTime;
                }

                catch (Exception)
                {
                    try
                    {
                        string newDateTime = FixDateTimeString(dateTime);
                        dst.DateTaken = newDateTime;
                    }

                    catch (Exception)
                    {
                        // *shrug*
                    }
                }
            }
        }

        private string FixDateTimeString(string brokenDateTime)
        {
            // It may be in the form of YYYY:MM:YY HH:MM:SS
            // But we need those first two :'s to be -'s
            StringBuilder fixedDateTime = new StringBuilder(brokenDateTime);

            for (int i = 0; i < Math.Min(brokenDateTime.Length, 10); ++i)
            {
                if (fixedDateTime[i] == ':')
                {
                    fixedDateTime[i] = '-';
                }
            }

            return fixedDateTime.ToString();
        }

        private void CopyMetadataTo(Metadata dst, BitmapMetadata src)
        {
            dst.AddExifValues(new PropertyItem[1] { Exif.CreateAscii(ExifTagID.Software, src.ApplicationName) });

            ReadOnlyCollection<string> authors = src.Author;
            if (authors != null)
            {

                List<PropertyItem> piAuthors = new List<PropertyItem>();
                foreach (string author in authors)
                {
                    PropertyItem piAuthor = Exif.CreateAscii(ExifTagID.Artist, author);
                    piAuthors.Add(piAuthor);
                }

                dst.AddExifValues(piAuthors.ToArray());
            }

            dst.AddExifValues(new PropertyItem[1] { Exif.CreateAscii(ExifTagID.Make, src.CameraManufacturer) });
            dst.AddExifValues(new PropertyItem[1] { Exif.CreateAscii(ExifTagID.Model, src.CameraModel) });
            dst.AddExifValues(new PropertyItem[1] { Exif.CreateAscii(ExifTagID.Copyright, src.Copyright) });
            dst.AddExifValues(new PropertyItem[1] { Exif.CreateAscii(ExifTagID.DateTime, src.DateTaken) });
            dst.AddExifValues(new PropertyItem[1] { Exif.CreateAscii(ExifTagID.ImageDescription, src.Title) });
        }

        protected override void OnSave(
            Document input, 
            Stream output, 
            SaveConfigToken token, 
            Surface scratchSurface, 
            ProgressEventHandler callback)
        {
            switch (Thread.CurrentThread.GetApartmentState())
            {
                // WIC does not support MTA, so we must marshal this stuff to another thread that is guaranteed to be STA.
                case ApartmentState.Unknown:
                case ApartmentState.MTA:
                    ParameterizedThreadStart pts = new ParameterizedThreadStart(OnSaveThreadProc);

                    OnSaveArgs osa = new OnSaveArgs();
                    osa.input = input;
                    osa.output = output;
                    osa.token = token;
                    osa.scratchSurface = scratchSurface;
                    osa.callback = callback;

                    Thread staThread = new Thread(pts);
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start(osa);
                    staThread.Join();

                    if (osa.exception != null)
                    {
                        throw new ApplicationException("OnSaveImpl() threw an exception", osa.exception);
                    }
                    break;

                case ApartmentState.STA:
                    OnSaveImpl(input, output, token, scratchSurface, callback);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private sealed class OnSaveArgs
        {
            public Document input;
            public Stream output;
            public SaveConfigToken token;
            public Surface scratchSurface;
            public ProgressEventHandler callback;

            public Exception exception;
        }

        private void OnSaveThreadProc(object context)
        {
            OnSaveArgs osa = (OnSaveArgs)context;

            try
            {
                OnSave(osa);
            }

            catch (Exception ex)
            {
                osa.exception = ex;
            }
        }

        private void OnSave(OnSaveArgs args)
        {
            OnSaveImpl(args.input, args.output, args.token, args.scratchSurface, args.callback);
        }

        private void OnSaveImpl(
            Document input, 
            Stream output, 
            SaveConfigToken token, 
            Surface scratchSurface, 
            ProgressEventHandler callback)
        {
            HDPhotoSaveConfigToken hdToken = token as HDPhotoSaveConfigToken;
            WmpBitmapEncoder wbe = new WmpBitmapEncoder();

            using (RenderArgs ra = new RenderArgs(scratchSurface))
            {
                input.Render(ra, true);
            }

            MemoryBlock block = scratchSurface.Scan0;
            IntPtr scan0 = block.Pointer;

            double dpiX;
            double dpiY;

            switch (input.DpuUnit)
            {
                case MeasurementUnit.Centimeter:
                    dpiX = Document.DotsPerCmToDotsPerInch(input.DpuX);
                    dpiY = Document.DotsPerCmToDotsPerInch(input.DpuY);
                    break;

                case MeasurementUnit.Inch:
                    dpiX = input.DpuX;
                    dpiY = input.DpuY;
                    break;

                case MeasurementUnit.Pixel:
                    dpiX = Document.GetDefaultDpu(MeasurementUnit.Inch);
                    dpiY = Document.GetDefaultDpu(MeasurementUnit.Inch);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            BitmapSource bitmapSource = BitmapFrame.Create(
                scratchSurface.Width,
                scratchSurface.Height,
                dpiX,
                dpiY,
                System.Windows.Media.PixelFormats.Bgra32,
                null,
                scan0,
                (int)block.Length, // TODO: does not support >2GB images
                scratchSurface.Stride);

            FormatConvertedBitmap fcBitmap = new FormatConvertedBitmap(
                bitmapSource,
                hdToken.BitDepth == 24 ? PixelFormats.Bgr24 : PixelFormats.Bgra32,
                null,
                0);

            BitmapFrame outputFrame0 = BitmapFrame.Create(fcBitmap);

            wbe.Frames.Add(outputFrame0);
            wbe.ImageQualityLevel = (float)hdToken.Quality / 100.0f;

            string tempFileName = FileSystem.GetTempFileName();

            FileStream tempFileOut = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            wbe.Save(tempFileOut);
            tempFileOut.Close();
            tempFileOut = null;

            FileStream tempFileIn = new FileStream(tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            WmpBitmapDecoder wbd = new WmpBitmapDecoder(tempFileIn, BitmapCreateOptions.None, BitmapCacheOption.None);
            BitmapFrame ioFrame0 = wbd.Frames[0];
            InPlaceBitmapMetadataWriter metadata2 = ioFrame0.CreateInPlaceBitmapMetadataWriter();
            CopyMetadataTo(metadata2, input.Metadata);
            tempFileIn.Close();
            tempFileIn = null;

            FileStream tempFileIn2 = new FileStream(tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            Utility.CopyStream(tempFileIn2, output);
            tempFileIn2.Close();
            tempFileIn2 = null;

            try
            {
                File.Delete(tempFileName);
            }

            catch (Exception)
            {
            }

            // WPF doesn't give us an IDisposable implementation on its types
            Utility.GCFullCollect();
        }
    }
}