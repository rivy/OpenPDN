/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace PaintDotNet.Actions
{
    internal sealed class SendFeedbackAction
        : AppWorkspaceAction
    {
        private string GetEmailLaunchString(string email, string subject, string body)
        {
            const string emailFormat = "mailto:{0}?subject={1}&body={2}";
            string bodyUE = body.Replace("\r\n", "%0D%0A");
            string launchString = string.Format(emailFormat, email, subject, bodyUE);
            return launchString;
        }

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            string email = InvariantStrings.FeedbackEmail;
            string subjectFormat = PdnResources.GetString("SendFeedback.Email.Subject.Format");
            string subject = string.Format(subjectFormat, PdnInfo.GetFullAppName());
            string body = PdnResources.GetString("SendFeedback.Email.Body");
            string launchMe = GetEmailLaunchString(email, subject, body);
            launchMe = launchMe.Substring(0, Math.Min(1024, launchMe.Length));

            try
            {
                Process.Start(launchMe);
            }
                 
            catch (Exception)
            {
                Utility.ErrorBox(appWorkspace, PdnResources.GetString("LaunchLink.Error"));
            }
        }

        public SendFeedbackAction()
        {
        }
    }
}
