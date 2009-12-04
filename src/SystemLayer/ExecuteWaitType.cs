/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    public enum ExecuteWaitType
    {
        /// <summary>
        /// Returns immediately after executing without waiting for the task to finish.
        /// </summary>
        ReturnImmediately,

        /// <summary>
        /// Waits until the task exits before returning control to the calling method.
        /// </summary>
        WaitForExit,

        /// <summary>
        /// Returns immediately after executing without waiting for the task to finish.
        /// However, another task will be spawned that will wait for the requested task
        /// to finish, and it will then relaunch Paint.NET if the task was successful.
        /// This is only intended to be used by the Paint.NET updater so that it can
        /// relaunch Paint.NET with the same user and privilege-level that initiated
        /// the update.
        /// </summary>
        RelaunchPdnOnExit
    }
}
