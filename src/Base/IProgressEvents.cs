////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet
{
    public interface IProgressEvents<TItemInfo>
    {
        /// <summary>
        /// This method is called at the very beginning of the operation.
        /// </summary>
        /// <param name="owner">
        /// If the operation has a modal window open, this represents it. Otherwise, this is null and you should 
        /// use your own current modal window.</param>
        /// <param name="callWhenUIShown">
        /// You must call this method after initializing any UI you want to display. 
        /// If you will not be displaying UI, you must call this anyway.</param>
        /// <param name="cancelSink">
        /// An object that can be used to cancel the operation at any time after callWhenUIShown is invoked, 
        /// and any time before EndOperation() is called.
        /// </param>
        /// <remarks>
        /// This method must not return until EndOperation() is called. Note that EndOperation() will
        /// be called from a separate thread. 
        /// </remarks>
        void BeginOperation(IWin32Window owner, EventHandler callWhenUIShown, ICancelable cancelSink);

        /// <summary>
        /// Called to report the total number of work items that are part of the operation.
        /// </summary>
        /// <param name="itemCount">The total number of work items that are part of the operation.</param>
        void SetItemCount(int itemCount);

        /// <summary>
        /// Called to change which work item of the operation is in progress.
        /// </summary>
        /// <param name="itemOrdinal"></param>
        /// <remarks>
        /// This method is called after BeginOperation() but before BeginItem(),
        /// or after EndItem() but before BeginItem(). 
        /// </remarks>
        void SetItemOrdinal(int itemOrdinal);

        /// <summary>
        /// Called to report information about the current work item.
        /// </summary>
        /// <param name="itemInfo">Operation-dependent information about the current work item.</param>
        void SetItemInfo(TItemInfo itemInfo);

        /// <summary>
        /// Reports the total number of work units that are involved in completing the current work item.
        /// </summary>
        /// <param name="totalWork">The total number of work units that comprise the current work item.</param>
        void SetItemWorkTotal(long totalWork);

        /// <summary>
        /// Called when a work item is starting.
        /// </summary>
        /// <param name="itemName"></param>
        void BeginItem();

        /// <summary>
        /// Reports the total amount of progress for the current work item.
        /// </summary>
        /// <param name="totalProgress">The total number of work units that have completed for the current work item.</param>
        void SetItemWorkProgress(long totalProgress);

        /// <summary>
        /// Called when there is an error while completing the current work item.
        /// </summary>
        /// <param name="ex">The exception that was encountered while completing the current work item.</param>
        /// <returns>You must return a value from the ItemFailedUserChoice enumeration.</returns>
        WorkItemFailureAction ReportItemFailure(Exception ex);

        /// <summary>
        /// Called after a work item is finished.
        /// </summary>
        void EndItem(WorkItemResult result);

        /// <summary>
        /// Called after the operation is complete.
        /// </summary>
        /// <param name="result">Indicates whether the operation finished, or was cancelled.</param>
        /// <remarks>
        /// Even if the operation finished, individual work items may not have succeeded.
        /// You will need to track the data passed to to EndItem() ReportItemError() to be able to monitor this.
        /// You must close any UI shown during BeginOperation(), and then return from that method.
        /// </remarks>
        void EndOperation(OperationResult result);
    }
}
