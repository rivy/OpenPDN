/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Actions;
using PaintDotNet.HistoryMementos;
using System;
using System.Drawing.Drawing2D;

namespace PaintDotNet.HistoryFunctions
{
    internal sealed class InvertSelectionFunction
        : HistoryFunction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("InvertSelectionAction.Name");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuEditInvertSelectionIcon.png");
            }
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            else
            {
                SelectionHistoryMemento sha = new SelectionHistoryMemento(
                    StaticName,
                    StaticImage,
                    historyWorkspace);

                //PdnGraphicsPath selectedPath = historyWorkspace.Selection.GetPathReadOnly();
                PdnGraphicsPath selectedPath = historyWorkspace.Selection.CreatePath();

                PdnGraphicsPath boundsOutline = new PdnGraphicsPath();
                boundsOutline.AddRectangle(historyWorkspace.Document.Bounds);

                PdnGraphicsPath clippedPath = PdnGraphicsPath.Combine(selectedPath, CombineMode.Intersect, boundsOutline);
                PdnGraphicsPath invertedPath = PdnGraphicsPath.Combine(clippedPath, CombineMode.Xor, boundsOutline);

                selectedPath.Dispose();
                selectedPath = null;

                clippedPath.Dispose();
                clippedPath = null;

                EnterCriticalRegion();
                historyWorkspace.Selection.PerformChanging();
                historyWorkspace.Selection.Reset();
                historyWorkspace.Selection.SetContinuation(invertedPath, CombineMode.Replace, true);
                historyWorkspace.Selection.CommitContinuation();
                historyWorkspace.Selection.PerformChanged();

                boundsOutline.Dispose();
                boundsOutline = null;

                return sha;
            }
        }
 
        public InvertSelectionFunction()
            : base(ActionFlags.None)
        {
        }
    }
}
