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
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public static class TaskDialog
    {
        public static int DefaultPixelWidth96Dpi = 300;

        public static TaskButton Show(
            IWin32Window owner,
            Icon formIcon,
            string formTitle,
            Image taskImage,
            string introText,
            TaskButton[] taskButtons,
            TaskButton acceptTaskButton,
            TaskButton cancelTaskButton)
        {
            return Show(owner, formIcon, formTitle, taskImage, true, introText,
                taskButtons, acceptTaskButton, cancelTaskButton);
        }

        public static TaskButton Show(
            IWin32Window owner,
            Icon formIcon,
            string formTitle,
            Image taskImage,
            bool scaleTaskImageWithDpi,
            string introText,
            TaskButton[] taskButtons,
            TaskButton acceptTaskButton,
            TaskButton cancelTaskButton)
        {
            return Show(owner, formIcon, formTitle, taskImage, scaleTaskImageWithDpi, introText, 
                taskButtons, acceptTaskButton, cancelTaskButton, DefaultPixelWidth96Dpi);
        }

        public static TaskButton Show(
            IWin32Window owner,
            Icon formIcon,
            string formTitle,
            Image taskImage,
            bool scaleTaskImageWithDpi,
            string introText,
            TaskButton[] taskButtons,
            TaskButton acceptTaskButton,
            TaskButton cancelTaskButton,
            int pixelWidth96Dpi)
        {
            return Show(owner, formIcon, formTitle, taskImage, scaleTaskImageWithDpi, introText, 
                taskButtons, acceptTaskButton, cancelTaskButton, pixelWidth96Dpi, null, null);
        }

        public static TaskButton Show(
            IWin32Window owner,
            Icon formIcon,
            string formTitle,
            Image taskImage,
            bool scaleTaskImageWithDpi,
            string introText,
            TaskButton[] taskButtons,
            TaskButton acceptTaskButton,
            TaskButton cancelTaskButton,
            int pixelWidth96Dpi,
            string auxButtonText,
            EventHandler auxButtonClickHandler)
        {
            using (TaskDialogForm form = new TaskDialogForm())
            {
                form.Icon = formIcon;
                form.IntroText = introText;
                form.Text = formTitle;
                form.TaskImage = taskImage;
                form.ScaleTaskImageWithDpi = scaleTaskImageWithDpi;
                form.TaskButtons = taskButtons;
                form.AcceptTaskButton = acceptTaskButton;
                form.CancelTaskButton = cancelTaskButton;

                if (auxButtonText != null)
                {
                    form.AuxButtonText = auxButtonText;
                }

                if (auxButtonClickHandler != null)
                {
                    form.AuxButtonClick += auxButtonClickHandler;
                }

                int pixelWidth = UI.ScaleWidth(pixelWidth96Dpi);
                form.ClientSize = new Size(pixelWidth, form.ClientSize.Height);

                DialogResult dr = form.ShowDialog(owner);
                TaskButton result = form.DialogResult;

                return result;
            }
        }
        
        private sealed class TaskDialogForm
            : PdnBaseForm
        {
            private PictureBox taskImagePB;
            private bool scaleTaskImageWithDpi;
            private Label introTextLabel;
            private TaskButton[] taskButtons;
            private CommandButton[] commandButtons;
            private HeaderLabel separator;
            private TaskButton acceptTaskButton;
            private TaskButton cancelTaskButton;
            private TaskButton dialogResult;

            private Button auxButton;

            public string AuxButtonText
            {
                get
                {
                    return this.auxButton.Text;
                }

                set
                {
                    this.auxButton.Text = value;
                    PerformLayout();
                }
            }

            public event EventHandler AuxButtonClick;
            private void OnAuxButtonClick()
            {
                if (AuxButtonClick != null)
                {
                    AuxButtonClick(this, EventArgs.Empty);
                }
            }

            public new TaskButton DialogResult
            {
                get
                {
                    return this.dialogResult;
                }
            }

            public Image TaskImage
            {
                get
                {
                    return this.taskImagePB.Image;
                }

                set
                {
                    this.taskImagePB.Image = value;
                    PerformLayout();
                    Invalidate(true);
                }
            }

            public bool ScaleTaskImageWithDpi
            {
                get
                {
                    return this.scaleTaskImageWithDpi;
                }

                set
                {
                    this.scaleTaskImageWithDpi = value;
                    PerformLayout();
                    Invalidate(true);
                }
            }

            public string IntroText
            {
                get
                {
                    return this.introTextLabel.Text;
                }

                set
                {
                    this.introTextLabel.Text = value;
                    PerformLayout();
                    Invalidate(true);
                }
            }

            public TaskButton[] TaskButtons
            {
                get
                {
                    return (TaskButton[])this.taskButtons.Clone();
                }

                set
                {
                    this.taskButtons = (TaskButton[])value.Clone();
                    InitCommandButtons();
                    PerformLayout();
                    Invalidate(true);
                }
            }

            public TaskButton AcceptTaskButton
            {
                get
                {
                    return this.acceptTaskButton;
                }

                set
                {
                    this.acceptTaskButton = value;

                    IButtonControl newAcceptButton = null;

                    for (int i = 0; i < this.commandButtons.Length; ++i)
                    {
                        TaskButton asTaskButton = this.commandButtons[i].Tag as TaskButton;

                        if (this.acceptTaskButton == asTaskButton)
                        {
                            newAcceptButton = this.commandButtons[i];
                        }
                    }

                    AcceptButton = newAcceptButton;
                }
            }

            public TaskButton CancelTaskButton
            {
                get
                {
                    return this.cancelTaskButton;
                }

                set
                {
                    this.cancelTaskButton = value;

                    IButtonControl newCancelButton = null;

                    for (int i = 0; i < this.commandButtons.Length; ++i)
                    {
                        TaskButton asTaskButton = this.commandButtons[i].Tag as TaskButton;

                        if (this.cancelTaskButton == asTaskButton)
                        {
                            newCancelButton = this.commandButtons[i];
                        }
                    }

                    CancelButton = newCancelButton;
                }
            }

            public TaskDialogForm()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                SuspendLayout();
                this.introTextLabel = new Label();
                this.auxButton = new Button();
                this.taskImagePB = new PictureBox();
                this.separator = new HeaderLabel();
                //
                // introTextLabel
                //
                this.introTextLabel.Name = "introTextLabel";
                //
                // taskImagePB
                //
                this.taskImagePB.Name = "taskImagePB";
                this.taskImagePB.SizeMode = PictureBoxSizeMode.StretchImage;
                //
                // auxButton
                //
                this.auxButton.Name = "auxButton";
                this.auxButton.AutoSize = true;
                this.auxButton.FlatStyle = FlatStyle.System;
                this.auxButton.Visible = false;
                this.auxButton.Click +=
                    delegate(object sender, EventArgs e)
                    {
                        OnAuxButtonClick();
                    };
                //
                // separator
                //
                this.separator.Name = "separator";
                this.separator.RightMargin = 0;
                //
                // TaskDialogForm
                //
                this.Name = "TaskDialogForm";
                this.ClientSize = new Size(300, 100);
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.CenterParent;
                this.Controls.Add(this.introTextLabel);
                this.Controls.Add(this.taskImagePB);
                this.Controls.Add(this.auxButton);
                this.Controls.Add(this.separator);
                ResumeLayout();
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                int leftMargin = UI.ScaleWidth(8);
                int rightMargin = UI.ScaleWidth(8);
                int topMargin = UI.ScaleHeight(8);
                int bottomMargin = UI.ScaleHeight(8);
                int imageToIntroHMargin = UI.ScaleWidth(8);
                int topSectionToLinksVMargin = UI.ScaleHeight(8);
                int commandButtonVMargin = UI.ScaleHeight(0);
                int afterCommandButtonsVMargin = UI.ScaleHeight(8);
                int insetWidth = ClientSize.Width - leftMargin - rightMargin;

                if (this.taskImagePB.Image == null)
                {
                    this.taskImagePB.Location = new Point(0, topMargin);
                    this.taskImagePB.Size = new Size(0, 0);
                    this.taskImagePB.Visible = false;
                }
                else
                {
                    this.taskImagePB.Location = new Point(leftMargin, topMargin);

                    if (this.scaleTaskImageWithDpi)
                    {
                        this.taskImagePB.Size = UI.ScaleSize(this.taskImagePB.Image.Size);
                    }
                    else
                    {
                        this.taskImagePB.Size = this.taskImagePB.Image.Size;
                    }

                    this.taskImagePB.Visible = true;
                }

                this.introTextLabel.Location = new Point(this.taskImagePB.Right + imageToIntroHMargin, this.taskImagePB.Top);
                this.introTextLabel.Width = ClientSize.Width - this.introTextLabel.Left - rightMargin;
                this.introTextLabel.Height = this.introTextLabel.GetPreferredSize(new Size(this.introTextLabel.Width, 1)).Height;

                int y = Math.Max(this.taskImagePB.Bottom, this.introTextLabel.Bottom);
                y += topSectionToLinksVMargin;

                if (!string.IsNullOrEmpty(this.auxButton.Text))
                {
                    this.auxButton.Visible = true;
                    this.auxButton.Location = new Point(leftMargin, y);
                    this.auxButton.PerformLayout();
                    y += this.auxButton.Height;
                    y += topSectionToLinksVMargin;
                }
                else
                {
                    this.auxButton.Visible = false;
                }

                if (this.commandButtons != null)
                {
                    this.separator.Location = new Point(leftMargin, y);
                    this.separator.Width = insetWidth;
                    y += this.separator.Height;

                    for (int i = 0; i < this.commandButtons.Length; ++i)
                    {
                        this.commandButtons[i].Location = new Point(leftMargin, y);
                        this.commandButtons[i].Width = insetWidth;
                        this.commandButtons[i].PerformLayout();
                        y += this.commandButtons[i].Height + commandButtonVMargin;
                    }

                    y += afterCommandButtonsVMargin;
                }

                this.ClientSize = new Size(ClientSize.Width, y);
                base.OnLayout(levent);
            }

            private void InitCommandButtons()
            {
                SuspendLayout();

                if (this.commandButtons != null)
                {
                    foreach (CommandButton commandButton in this.commandButtons)
                    {
                        Controls.Remove(commandButton);
                        commandButton.Tag = null;
                        commandButton.Click -= CommandButton_Click;
                        commandButton.Dispose();
                    }

                    this.commandButtons = null;
                }

                this.commandButtons = new CommandButton[this.taskButtons.Length];

                IButtonControl newAcceptButton = null;
                IButtonControl newCancelButton = null;

                for (int i = 0; i < this.commandButtons.Length; ++i)
                {
                    TaskButton taskButton = this.taskButtons[i];
                    CommandButton commandButton = new CommandButton();

                    commandButton.ActionText = taskButton.ActionText;
                    commandButton.ActionImage = taskButton.Image;
                    commandButton.AutoSize = true;
                    commandButton.ExplanationText = taskButton.ExplanationText;
                    commandButton.Tag = taskButton;
                    commandButton.Click += CommandButton_Click;

                    this.commandButtons[i] = commandButton;
                    Controls.Add(commandButton);

                    if (this.acceptTaskButton == taskButton)
                    {
                        newAcceptButton = commandButton;
                    }

                    if (this.cancelTaskButton == taskButton)
                    {
                        newCancelButton = commandButton;
                    }
                }

                AcceptButton = newAcceptButton;
                CancelButton = newCancelButton;

                if (newAcceptButton != null && newAcceptButton is Control)
                {
                    ((Control)newAcceptButton).Select();
                }

                ResumeLayout();
            }

            private void CommandButton_Click(object sender, EventArgs e)
            {
                CommandButton commandButton = (CommandButton)sender;
                this.dialogResult = (TaskButton)commandButton.Tag;
                Close();
            }
        }
    }
}