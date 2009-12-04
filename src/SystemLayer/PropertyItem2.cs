/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Re-implements System.Drawing.PropertyItem so that the data is serializable.
    /// </summary>
    [Serializable]
    internal sealed class PropertyItem2
    {
        private const string piElementName = "exif";
        private const string idPropertyName = "id";
        private const string lenPropertyName = "len";
        private const string typePropertyName = "type";
        private const string valuePropertyName = "value";

        private int id;
        private int len;
        private short type;
        private byte[] value;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public int Len
        {
            get
            {
                return len;
            }
        }

        public short Type
        {
            get
            {
                return type;
            }
        }

        public byte[] Value
        {
            get
            {
                return (byte[])value.Clone();
            }
        }

        public PropertyItem2(int id, int len, short type, byte[] value)
        {
            this.id = id;
            this.len = len;
            this.type = type;

            if (value == null)
            {
                this.value = new byte[0];
            }
            else
            {
                this.value = (byte[])value.Clone();
            }

            if (len != this.value.Length)
            {
                Tracing.Ping("len != value.Length: id=" + id + ", type=" + type);
            }
        }

        public string ToBlob()
        {
            string blob = string.Format("<{0} {1}=\"{2}\" {3}=\"{4}\" {5}=\"{6}\" {7}=\"{8}\" />",
                piElementName, 
                idPropertyName, this.id.ToString(CultureInfo.InvariantCulture),
                lenPropertyName, this.len.ToString(CultureInfo.InvariantCulture), 
                typePropertyName, this.type.ToString(CultureInfo.InvariantCulture),
                valuePropertyName, Convert.ToBase64String(this.value));

            return blob;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public PropertyItem ToPropertyItem()
        {
            PropertyItem pi = GetPropertyItem();

            pi.Id = this.Id;
            pi.Len = this.Len;
            pi.Type = this.Type;
            pi.Value = this.Value;

            return pi;
        }

        public static PropertyItem2 FromPropertyItem(PropertyItem pi)
        {
            return new PropertyItem2(pi.Id, pi.Len, pi.Type, pi.Value);
        }

        private static string GetProperty(string blob, string propertyName)
        {
            string findMe = propertyName + "=\"";
            int startIndex = blob.IndexOf(findMe) + findMe.Length;
            int endIndex = blob.IndexOf("\"", startIndex);
            string propertyValue = blob.Substring(startIndex, endIndex - startIndex);
            return propertyValue;
        }
        
        public static PropertyItem2 FromBlob(string blob)
        {
            PropertyItem2 pi2;

            if (blob.Length > 0 && blob[0] == '<')
            {
                string idStr = GetProperty(blob, idPropertyName);
                string lenStr = GetProperty(blob, lenPropertyName);
                string typeStr = GetProperty(blob, typePropertyName);
                string valueStr = GetProperty(blob, valuePropertyName);

                int id = int.Parse(idStr, CultureInfo.InvariantCulture);
                int len = int.Parse(lenStr, CultureInfo.InvariantCulture);
                short type = short.Parse(typeStr, CultureInfo.InvariantCulture);
                byte[] value = Convert.FromBase64String(valueStr);

                pi2 = new PropertyItem2(id, len, type, value);
            }
            else
            {
                // Old way of serializing: .NET serialized!
                byte[] bytes = Convert.FromBase64String(blob);
                MemoryStream ms = new MemoryStream(bytes);
                BinaryFormatter bf = new BinaryFormatter();
                SerializationFallbackBinder sfb = new SerializationFallbackBinder();
                sfb.AddAssembly(Assembly.GetExecutingAssembly());
                bf.Binder = sfb;
                pi2 = (PropertyItem2)bf.Deserialize(ms);
            }

            return pi2;
        }

        // System.Drawing.Imaging.PropertyItem does not have a public constructor
        // So, as per the documentation, we have to "steal" one.
        // Quite ridiculous.
        // This depends on PropertyItem.png being an embedded resource in this assembly.
        private static Image propertyItemImage;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static PropertyItem GetPropertyItem()
        {
            if (propertyItemImage == null)
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PaintDotNet.SystemLayer.PropertyItem.png");
                propertyItemImage = Image.FromStream(stream);
            }

            PropertyItem pi = propertyItemImage.PropertyItems[0];
            pi.Id = 0;
            pi.Len = 0;
            pi.Type = 0;
            pi.Value = new byte[0];

            return pi;
        }
    }
}
