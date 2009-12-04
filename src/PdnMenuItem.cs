/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet
{
    internal class PdnMenuItem
        : ToolStripMenuItem,
          IFormAssociate
    {
        private string textResourceName = null;
        private const char noMnemonicChar = (char)0;
        private const char mnemonicPrefix = '&';
        private bool iconsLoaded = false;
        private bool namesLoaded = false;
        private AppWorkspace appWorkspace;
        private Keys registeredHotKey = Keys.None;

        [Browsable(false)]
        public AppWorkspace AppWorkspace
        {
            get
            {
                return this.appWorkspace;
            }

            set
            {
                if (value != this.appWorkspace)
                {
                    OnAppWorkspaceChanging();
                    this.appWorkspace = value;
                    OnAppWorkspaceChanged();
                }
            }
        }

        public Form AssociatedForm
        {
            get
            {
                if (this.appWorkspace == null)
                {
                    return null;
                }
                else
                {
                    return this.appWorkspace.FindForm();
                }
            }
        }

        public new Keys ShortcutKeys
        {
            get
            {
                return base.ShortcutKeys;
            }

            set
            {
                if (ShortcutKeys != Keys.None)
                {
                    PdnBaseForm.UnregisterFormHotKey(ShortcutKeys, OnShortcutKeyPressed);
                }

                PdnBaseForm.RegisterFormHotKey(value, OnShortcutKeyPressed);

                base.ShortcutKeys = value;
            }
        }

        public bool HasMnemonic
        {
            get
            {
                return (Mnemonic != noMnemonicChar);
            }
        }

        public char Mnemonic
        {
            get
            {
                if (string.IsNullOrEmpty(this.Text))
                {
                    return noMnemonicChar;
                }

                int mnemonicPrefixIndex = this.Text.IndexOf(mnemonicPrefix);

                if (mnemonicPrefixIndex >= 0 && mnemonicPrefixIndex < this.Text.Length - 1)
                {
                    return this.Text[mnemonicPrefixIndex + 1];
                }
                else
                {
                    return noMnemonicChar;
                }
            }
        }

        public void PerformClickAsync()
        {
            this.Owner.BeginInvoke(new Procedure(this.PerformClick));
        }

        protected virtual void OnAppWorkspaceChanging()
        {
            foreach (ToolStripItem item in this.DropDownItems)
            {
                PdnMenuItem asPMI = item as PdnMenuItem;

                if (asPMI != null)
                {
                    asPMI.AppWorkspace = null;
                }
            }
        }

        protected virtual void OnAppWorkspaceChanged()
        {
            foreach (ToolStripItem item in this.DropDownItems)
            {
                PdnMenuItem asPMI = item as PdnMenuItem;

                if (asPMI != null)
                {
                    asPMI.AppWorkspace = AppWorkspace;
                }
            }
        }       

        public PdnMenuItem(string name, Image image, EventHandler eventHandler)
            : base(name, image, eventHandler)
        {
            Constructor();
        }

        public PdnMenuItem()
        {
            Constructor();
        }

        private void Constructor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // HACK: For some reason, ToolStripMenuItem does not have an OnDropDownOpening method.
            this.DropDownOpening += new EventHandler(PdnMenuItem_DropDownOpening);
        }

        private bool OnShortcutKeyPressed(Keys keys)
        {
            PerformClick();
            return true;
        }

        private bool OnAccessHotKeyPressed(Keys keys)
        {
            ShowDropDown();
            return true;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this.registeredHotKey != Keys.None)
            {
                PdnBaseForm.UnregisterFormHotKey(this.registeredHotKey, OnAccessHotKeyPressed);
            }

            char mnemonic = this.Mnemonic;

            if (mnemonic != noMnemonicChar && !IsOnDropDown)
            {
                Keys hotKey = Utility.LetterOrDigitCharToKeys(mnemonic);
                PdnBaseForm.RegisterFormHotKey(Keys.Alt | hotKey, OnAccessHotKeyPressed);
            }

            base.OnTextChanged(e);
        }

        private void PdnMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            OnDropDownOpening(e);
        }

        protected virtual void OnDropDownOpening(EventArgs e)
        {
            if (!this.namesLoaded)
            {
                LoadNames(this.Name);
            }

            if (!this.iconsLoaded)
            {
                LoadIcons();
            }
        }

        public void LoadNames(string baseName)
        {
            foreach (ToolStripItem item in this.DropDownItems)
            {
                string itemNameBase = baseName + "." + item.Name;
                string itemNameText = itemNameBase + ".Text";
                string text = PdnResources.GetString(itemNameText);

                if (text != null)
                {
                    item.Text = text;
                }

                PdnMenuItem pmi = item as PdnMenuItem;
                if (pmi != null)
                {
                    pmi.textResourceName = itemNameText;
                    pmi.LoadNames(itemNameBase);
                }
            }

            this.namesLoaded = true;
        }

        public void SetIcon(string imageName)
        {
            this.ImageTransparentColor = Utility.TransparentKey;
            this.Image = PdnResources.GetImageResource(imageName).Reference;
        }

        public void SetIcon(ImageResource image)
        {
            this.ImageTransparentColor = Utility.TransparentKey;
            this.Image = image.Reference;
        }

        public void LoadIcons()
        {
            Type ourType = this.GetType();

            FieldInfo[] fields = ourType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType.IsSubclassOf(typeof(PdnMenuItem)) || 
                    fi.FieldType == typeof(PdnMenuItem))
                {
                    string iconFileName = "Icons." + fi.Name[0].ToString().ToUpper() + fi.Name.Substring(1) + "Icon.png";
                    PdnMenuItem mi = (PdnMenuItem)fi.GetValue(this);
                    Stream iconStream = PdnResources.GetResourceStream(iconFileName);

                    if (iconStream != null)
                    {
                        iconStream.Dispose();
                        mi.SetIcon(iconFileName);
                    }
                    else
                    {
                        Tracing.Ping(iconFileName + " not found");
                    }
                }
            }

            this.iconsLoaded = true;
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            foreach (ToolStripItem item in this.DropDownItems)
            {
                item.Enabled = true;
            }

            base.OnDropDownClosed(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (Form.ActiveForm != null)
            {
                Form.ActiveForm.BeginInvoke(new Procedure(PdnBaseForm.UpdateAllForms));
            }

            string featureName = this.Name ?? this.Text;

            Tracing.LogFeature(featureName);

            base.OnClick(e);
        }
    }
}
