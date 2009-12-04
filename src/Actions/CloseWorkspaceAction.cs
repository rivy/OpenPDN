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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet.Actions
{
    internal sealed class CloseWorkspaceAction
        : AppWorkspaceAction
    {
        private DocumentWorkspace closeMe;
        private bool cancelled;

        public bool Cancelled
        {
            get
            {
                return this.cancelled;
            }
        }

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            if (appWorkspace == null)
            {
                throw new ArgumentNullException("appWorkspace");
            }

            DocumentWorkspace dw;

            if (this.closeMe == null)
            {
                dw = appWorkspace.ActiveDocumentWorkspace;
            }
            else
            {
                dw = this.closeMe;
            }

            if (dw != null)
            {
                if (dw.Document == null)
                {
                    appWorkspace.RemoveDocumentWorkspace(dw);
                }
                else if (!dw.Document.Dirty)
                {
                    appWorkspace.RemoveDocumentWorkspace(dw);
                }
                else
                {
                    appWorkspace.ActiveDocumentWorkspace = dw;

                    TaskButton saveTB = new TaskButton(
                        PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference,
                        PdnResources.GetString("CloseWorkspaceAction.SaveButton.ActionText"),
                        PdnResources.GetString("CloseWorkspaceAction.SaveButton.ExplanationText"));

                    TaskButton dontSaveTB = new TaskButton(
                        PdnResources.GetImageResource("Icons.MenuFileCloseIcon.png").Reference,
                        PdnResources.GetString("CloseWorkspaceAction.DontSaveButton.ActionText"),
                        PdnResources.GetString("CloseWorkspaceAction.DontSaveButton.ExplanationText"));

                    TaskButton cancelTB = new TaskButton(
                        PdnResources.GetImageResource("Icons.CancelIcon.png").Reference,
                        PdnResources.GetString("CloseWorkspaceAction.CancelButton.ActionText"),
                        PdnResources.GetString("CloseWorkspaceAction.CancelButton.ExplanationText"));

                    string title = PdnResources.GetString("CloseWorkspaceAction.Title");
                    string introTextFormat = PdnResources.GetString("CloseWorkspaceAction.IntroText.Format");
                    string introText = string.Format(introTextFormat, dw.GetFriendlyName());

                    Image thumb = appWorkspace.GetDocumentWorkspaceThumbnail(dw);

                    if (thumb == null)
                    {
                        thumb = new Bitmap(32, 32);
                    }

                    Bitmap taskImage = new Bitmap(thumb.Width + 2, thumb.Height + 2, PixelFormat.Format32bppArgb);

                    using (Graphics g = Graphics.FromImage(taskImage))
                    {
                        g.Clear(Color.Transparent);

                        g.DrawImage(
                            thumb, 
                            new Rectangle(1, 1, thumb.Width, thumb.Height), 
                            new Rectangle(0, 0, thumb.Width, thumb.Height), 
                            GraphicsUnit.Pixel);

                        Utility.DrawDropShadow1px(g, new Rectangle(0, 0, taskImage.Width, taskImage.Height));
                    }

                    Form mainForm = appWorkspace.FindForm();
                    if (mainForm != null)
                    {
                        PdnBaseForm asPDF = mainForm as PdnBaseForm;

                        if (asPDF != null)
                        {
                            asPDF.RestoreWindow();
                        }
                    }

                    Icon warningIcon;
                    ImageResource warningIconImageRes = PdnResources.GetImageResource("Icons.WarningIcon.png");

                    if (warningIconImageRes != null)
                    {
                        Image warningIconImage = warningIconImageRes.Reference;
                        warningIcon = Utility.ImageToIcon(warningIconImage, false);
                    }
                    else
                    {
                        warningIcon = null;
                    }                     

                    TaskButton clickedTB = TaskDialog.Show(
                        appWorkspace,
                        warningIcon,
                        title,
                        taskImage,
                        false,
                        introText,
                        new TaskButton[] { saveTB, dontSaveTB, cancelTB },
                        saveTB,
                        cancelTB,
                        340);                        

                    if (clickedTB == saveTB)
                    {
                        if (dw.DoSave())
                        {
                            this.cancelled = false;
                            appWorkspace.RemoveDocumentWorkspace(dw);
                        }
                        else
                        {
                            this.cancelled = true;
                        }
                    }
                    else if (clickedTB == dontSaveTB)
                    {
                        this.cancelled = false;
                        appWorkspace.RemoveDocumentWorkspace(dw);
                    }
                    else
                    {
                        this.cancelled = true;
                    }
                }
            }

            Utility.GCFullCollect();
        }

        public CloseWorkspaceAction()
            : this(null)
        {
        }

        public CloseWorkspaceAction(DocumentWorkspace closeMe)
        {
            this.closeMe = closeMe;
            this.cancelled = false;
        }
    }
}
