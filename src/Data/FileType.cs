/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Data.Quantize;
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PaintDotNet
{
    /// <summary>
    /// Represents one type of file that PaintDotNet can load or save.
    /// </summary>
    public abstract class FileType
    {
        private string[] extensions;
        private string name;
        private FileTypeFlags flags;

        // should be of the format ".ext" ... like ".bmp" or ".jpg"
        // The first extension in this list is the default extension (".jpg" for JPEG, 
        // for instance, as ".jfif" etc. are not seen very often)
        public string[] Extensions
        {
            get
            {
                return (string[])this.extensions.Clone();
            }
        }

        /// <summary>
        /// Gets the default extension for the FileType.
        /// </summary>
        /// <remarks>
        /// This is always the first extension that is supported
        /// </remarks>
        public string DefaultExtension
        {
            get
            {
                return this.extensions[0];
            }
        }

        /// <summary>
        /// Returns the friendly name of the file type, such as "Bitmap" or "JPEG".
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public FileTypeFlags Flags
        {
            get
            {
                return this.flags;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether this FileType supports layers.
        /// </summary>
        /// <remarks>
        /// If a FileType is asked to save a Document that has more than one layer,
        /// it will flatten it before it saves it.
        /// </remarks>
        public bool SupportsLayers
        {
            get
            {
                return (this.flags & FileTypeFlags.SupportsLayers) != 0;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether this FileType supports custom headers.
        /// </summary>
        /// <remarks>
        /// If this returns false, then the Document's CustomHeaders will be discarded
        /// on saving.
        /// </remarks>
        public bool SupportsCustomHeaders
        {
            get
            {
                return (this.flags & FileTypeFlags.SupportsCustomHeaders) != 0;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether this FileType supports the Save() method.
        /// </summary>
        /// <remarks>
        /// If this property returns false, calling Save() will throw a NotSupportedException.
        /// </remarks>
        public bool SupportsSaving
        {
            get
            {
                return (this.flags & FileTypeFlags.SupportsSaving) != 0;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether this FileType supports the Load() method.
        /// </summary>
        /// <remarks>
        /// If this property returns false, calling Load() will throw a NotSupportedException.
        /// </remarks>
        public bool SupportsLoading
        {
            get
            {
                return (this.flags & FileTypeFlags.SupportsLoading) != 0;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether this FileType reports progress while saving.
        /// </summary>
        /// <remarks>
        /// If false, then the callback delegate passed to Save() will be ignored.
        /// </remarks>
        public bool SavesWithProgress
        {
            get
            {
                return (this.flags & FileTypeFlags.SavesWithProgress) != 0;
            }
        }

        [Obsolete("Use the FileType(string, FileTypeFlags, string[]) overload instead", true)]
        public FileType(string name, bool supportsLayers, bool supportsCustomHeaders, string[] extensions)
            : this(name, supportsLayers, supportsCustomHeaders, true, true, false, extensions)
        {
        }

        [Obsolete("Use the FileType(string, FileTypeFlags, string[]) overload instead", true)]
        public FileType(string name, bool supportsLayers, bool supportsCustomHeaders, bool supportsSaving, 
            bool supportsLoading, bool savesWithProgress, string[] extensions)
            : this(name, 
                   (supportsLayers ? FileTypeFlags.SupportsLayers : 0) |
                       (supportsCustomHeaders ? FileTypeFlags.SupportsCustomHeaders : 0) |
                       (supportsSaving ? FileTypeFlags.SupportsSaving : 0) |
                       (supportsLoading ? FileTypeFlags.SupportsLoading : 0) |
                       (savesWithProgress ? FileTypeFlags.SavesWithProgress : 0),
                   extensions)
        {
        }

        public FileType(string name, FileTypeFlags flags, string[] extensions)
        {
            this.name = name;
            this.flags = flags;
            this.extensions = (string[])extensions.Clone();
        }

        public bool SupportsExtension(string ext)
        {
            foreach (string ext2 in extensions)
            {
                if (0 == string.Compare(ext2, ext, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        
        [Obsolete("This method is retained for compatibility with older plugins. Please use the other overload of Quantize().")]
        protected Bitmap Quantize(Surface quantizeMe, int ditherAmount, int maxColors, ProgressEventHandler progressCallback)
        {
            // This is the old version of the function took maxColors=255 to mean 255 colors + 1 slot for transparency.
            // The new version expects maxColors=256 and enableTransparency=true for this.

            if (maxColors < 2 || maxColors > 255)
            {
                throw new ArgumentOutOfRangeException(
                    "maxColors",
                    maxColors,
                    "Out of bounds. Must be in the range [2, 255]");
            }

            return Quantize(quantizeMe, ditherAmount, maxColors + 1, true, progressCallback);
        }

        /// <summary>
        /// Takes a Surface and quantizes it down to an 8-bit bitmap.
        /// </summary>
        /// <param name="quantizeMe">The Surface to quantize.</param>
        /// <param name="ditherAmount">How strong should dithering be applied. 0 for no dithering, 8 for full dithering. 7 is generally a good default to use.</param>
        /// <param name="maxColors">The maximum number of colors to use. This may range from 2 to 256.</param>
        /// <param name="enableTransparency">If true, then one color slot will be reserved for transparency. Any color with an alpha value less than 255 will be transparent in the output.</param>
        /// <param name="progressCallback">The progress callback delegate.</param>
        /// <returns>An 8-bit Bitmap that is the same size as quantizeMe.</returns>
        protected Bitmap Quantize(Surface quantizeMe, int ditherAmount, int maxColors, bool enableTransparency, ProgressEventHandler progressCallback)
        {
            if (ditherAmount < 0 || ditherAmount > 8)
            {
                throw new ArgumentOutOfRangeException(
                    "ditherAmount",
                    ditherAmount,
                    "Out of bounds. Must be in the range [0, 8]");
            }

            if (maxColors < 2 || maxColors > 256)
            {
                throw new ArgumentOutOfRangeException(
                    "maxColors",
                    maxColors,
                    "Out of bounds. Must be in the range [2, 256]");
            }

            // TODO: detect if transparency is needed? or take another argument

            using (Bitmap bitmap = quantizeMe.CreateAliasedBitmap(quantizeMe.Bounds, true))
            {
                OctreeQuantizer quantizer = new OctreeQuantizer(maxColors, enableTransparency);
                quantizer.DitherLevel = ditherAmount;
                Bitmap quantized = quantizer.Quantize(bitmap, progressCallback);
                return quantized;
            }
        }

        [Obsolete("Use the other Save() overload instead", true)]
        public void Save(Document input, Stream output, SaveConfigToken token, ProgressEventHandler callback, bool rememberToken)
        {
            using (Surface scratch = new Surface(input.Width, input.Height))
            {
                Save(input, output, token, callback, rememberToken);
            }
        }

        public void Save(
            Document input, 
            Stream output, 
            SaveConfigToken token, 
            Surface scratchSurface, 
            ProgressEventHandler callback, 
            bool rememberToken)
        {
            Tracing.LogFeature("Save(" + GetType().FullName + ")");

            if (!this.SupportsSaving)
            {
                throw new NotImplementedException("Saving is not supported by this FileType");
            }
            else
            {
                Surface disposeMe = null;

                if (scratchSurface == null)
                {
                    disposeMe = new Surface(input.Size);
                    scratchSurface = disposeMe;
                }
                else if (scratchSurface.Size != input.Size)
                {
                    throw new ArgumentException("scratchSurface.Size must equal input.Size");
                }

                if (rememberToken)
                {
                    Type ourType = this.GetType();
                    string savedTokenName = "SaveConfigToken." + ourType.Namespace + "." + ourType.Name + ".BinaryFormatter";

                    MemoryStream ms = new MemoryStream();

                    BinaryFormatter formatter = new BinaryFormatter();
                    DeferredFormatter deferredFormatter = new DeferredFormatter(false, null);
                    StreamingContext streamingContext = new StreamingContext(formatter.Context.State, deferredFormatter);
                    formatter.Context = streamingContext;

                    object tokenSubset = GetSerializablePortionOfSaveConfigToken(token);

                    formatter.Serialize(ms, tokenSubset);
                    deferredFormatter.FinishSerialization(ms);

                    byte[] bytes = ms.GetBuffer();
                    string base64Bytes = Convert.ToBase64String(bytes);

                    Settings.CurrentUser.SetString(savedTokenName, base64Bytes);
                }

                try
                {
                    OnSave(input, output, token, scratchSurface, callback);
                }

                catch (OnSaveNotImplementedException)
                {
                    OldOnSaveTrampoline(input, output, token, callback);
                }

                if (disposeMe != null)
                {
                    disposeMe.Dispose();
                    disposeMe = null;
                }
            }
        }

        protected virtual SaveConfigToken GetSaveConfigTokenFromSerializablePortion(object portion)
        {
            return (SaveConfigToken)portion;
        }

        protected virtual object GetSerializablePortionOfSaveConfigToken(SaveConfigToken token)
        {
            return token;
        }

        private sealed class OnSaveNotImplementedException
            : Exception
        {
            public OnSaveNotImplementedException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// Because the old OnSave() method is obsolete, we must use reflection to call it.
        /// This is important for legacy FileType plugins. It allows us to ensure that no
        /// new plugins can be compiled using the old OnSave() overload.
        /// </summary>
        private void OldOnSaveTrampoline(Document input, Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
            MethodInfo onSave = GetType().GetMethod(
                "OnSave",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
                Type.DefaultBinder,
                new Type[] 
                { 
                    typeof(Document), 
                    typeof(Stream), 
                    typeof(SaveConfigToken), 
                    typeof(ProgressEventHandler)
                },
                null);

            onSave.Invoke(
                this,
                new object[]
                {
                    input,
                    output,
                    token,
                    callback
                });
        }

        [Obsolete("Use the other OnSave() overload. It provides a scratch rendering surface that may enable your plugin to conserve memory usage.")]
        protected virtual void OnSave(Document input, Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
        }

        protected virtual void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            throw new OnSaveNotImplementedException("Derived classes must implement this method. It is virtual instead of abstract in order to maintain compatibility with legacy plugins.");
        }

        /// <summary>
        /// Determines if saving with a given SaveConfigToken would alter the image
        /// in any way. Put another way, if the document is saved with these settings
        /// and then immediately loaded, would it have exactly the same pixel values?
        /// Any lossy codec should return 'false'.
        /// This value is used to optimizing preview rendering memory usage, and as such
        /// flattening should not be taken in to consideration. For example, the codec
        /// for PNG returns true, even though it flattens the image.
        /// </summary>
        /// <param name="token">The SaveConfigToken to determine reflexiveness for.</param>
        /// <returns>true if the save would be reflexive, false if not</returns>
        /// <remarks>If the SaveConfigToken is for another FileType, the result is undefined.</remarks>
        public virtual bool IsReflexive(SaveConfigToken token)
        {
            return false;
        }

        public virtual SaveConfigWidget CreateSaveConfigWidget()
        {
            return new NoSaveConfigWidget();
        }

        [Serializable]
        private sealed class NoSaveConfigToken
            : SaveConfigToken
        {
        }

        /// <summary>
        /// Gets a flag indicating whether or not the file type supports configuration
        /// via a SaveConfigToken and SaveConfigWidget.
        /// </summary>
        /// <remarks>
        /// Implementers of FileType derived classes don't need to do anything special
        /// for this property to be accurate. If your FileType implements
        /// CreateDefaultSaveConfigToken, this will correctly return true.
        /// </remarks>
        public bool SupportsConfiguration
        {
            get
            {
                SaveConfigToken token = CreateDefaultSaveConfigToken();
                return !(token is NoSaveConfigToken);
            }
        }

        public SaveConfigToken GetLastSaveConfigToken()
        {
            Type ourType = this.GetType();
            string savedTokenName = "SaveConfigToken." + ourType.Namespace + "." + ourType.Name + ".BinaryFormatter";
            string savedToken = Settings.CurrentUser.GetString(savedTokenName, null);
            SaveConfigToken saveConfigToken = null;

            if (savedToken != null)
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(savedToken);

                    MemoryStream ms = new MemoryStream(bytes);

                    BinaryFormatter formatter = new BinaryFormatter();
                    DeferredFormatter deferred = new DeferredFormatter();
                    StreamingContext streamingContext = new StreamingContext(formatter.Context.State, deferred);
                    formatter.Context = streamingContext;

                    SerializationFallbackBinder sfb = new SerializationFallbackBinder();
                    sfb.AddAssembly(this.GetType().Assembly);
                    sfb.AddAssembly(typeof(FileType).Assembly);
                    formatter.Binder = sfb;

                    object obj = formatter.Deserialize(ms);
                    deferred.FinishDeserialization(ms);

                    ms.Close();
                    ms = null;

                    //SaveConfigToken sct = new SaveConfigToken();
                    //saveConfigToken = (SaveConfigToken)obj;
                    saveConfigToken = GetSaveConfigTokenFromSerializablePortion(obj);
                }

                catch (Exception)
                {
                    // Ignore erros and revert to default
                    saveConfigToken = null;
                }
            }

            if (saveConfigToken == null)
            {
                saveConfigToken = CreateDefaultSaveConfigToken();
            }

            return saveConfigToken;
        }

        public SaveConfigToken CreateDefaultSaveConfigToken()
        {
            return OnCreateDefaultSaveConfigToken();
        }

        /// <summary>
        /// Creates a SaveConfigToken for this FileType with the default values.
        /// </summary>
        protected virtual SaveConfigToken OnCreateDefaultSaveConfigToken()
        {
            return new NoSaveConfigToken();
        }

        public Document Load(Stream input)
        {
            Tracing.LogFeature("Load(" + GetType().FullName + ")");

            if (!this.SupportsLoading)
            {
                throw new NotSupportedException("Loading not supported for this FileType");
            }
            else
            {
                return OnLoad(input);
            }
        }

        protected abstract Document OnLoad(Stream input);

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FileType))
            {
                return false;
            }

            return this.name.Equals(((FileType)obj).Name);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        /// <summary>
        /// Returns a string that can be used for populating a *FileDialog common dialog.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Append(" (");

            for (int i = 0; i < extensions.Length; ++i)
            {
                sb.Append("*");
                sb.Append(extensions[i]);

                if (i != extensions.Length - 1)
                {
                    sb.Append("; ");
                }
                else
                {
                    sb.Append(")");
                }
            }

            sb.Append("|");

            for (int i = 0; i < extensions.Length; ++i)
            {
                sb.Append("*");
                sb.Append(extensions[i]);

                if (i != extensions.Length - 1)
                {
                    sb.Append(";");
                }
            }

            return sb.ToString();
        }
    }
}
