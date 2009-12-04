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
    public enum ExecutePrivilege
    {
        /// <summary>
        /// The process is started with default permissions: either the same as the invoker,
        /// or those required by the executable's manifest.
        /// </summary>
        AsInvokerOrAsManifest,

        /// <summary>
        /// The process is required to run with administrator privilege. If the user does not
        /// have administrator privilege, nor has the ability to obtain it, then the operation
        /// will fail.
        /// </summary>
        RequireAdmin,

        /// <summary>
        /// The process is required to run with normal privilege. On some systems this may
        /// not be possible, and as such this will have the same effect as AsInvokerOrAsManifest.
        /// </summary>
        /// <remarks>
        /// This flag only has an effect in Windows Vista from a process that already has
        /// administrator privilege.
        /// </remarks>
        RequireNonAdminIfPossible
    }
}
