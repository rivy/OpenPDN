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

namespace PaintDotNet
{
    public sealed class TaskButton
    {
        private static TaskButton cancel = null;
        public static TaskButton Cancel
        {
            get
            {
                if (cancel == null)
                {
                    cancel = new TaskButton(
                        PdnResources.GetImageResource("Icons.CancelIcon.png").Reference,
                        PdnResources.GetString("TaskButton.Cancel.ActionText"),
                        PdnResources.GetString("TaskButton.Cancel.ExplanationText"));
                }

                return cancel;
            }
        }

        private Image image;
        private string actionText;
        private string explanationText;

        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        public string ActionText
        {
            get
            {
                return this.actionText;
            }
        }

        public string ExplanationText
        {
            get
            {
                return this.explanationText;
            }
        }

        public TaskButton(Image image, string actionText, string explanationText)
        {
            this.image = image;
            this.actionText = actionText;
            this.explanationText = explanationText;
        }
    }
}
