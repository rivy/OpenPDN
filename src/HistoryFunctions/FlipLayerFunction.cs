/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryMementos;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintDotNet.HistoryFunctions
{
    internal abstract class FlipLayerFunction
        : HistoryFunction
    {
        private string historyName;
        private FlipType flipType;
        private ImageResource undoImage;
        private int layerIndex;

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            CompoundHistoryMemento chm = new CompoundHistoryMemento(this.historyName, this.undoImage);

            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction df = new DeselectFunction();
                EnterCriticalRegion();
                HistoryMemento hm = df.Execute(historyWorkspace);
                chm.PushNewAction(hm);
            } 
            
            FlipLayerHistoryMemento flha = new FlipLayerHistoryMemento(
                null, 
                null, 
                historyWorkspace, 
                this.layerIndex, 
                this.flipType);

            EnterCriticalRegion();
            HistoryMemento flha2 = flha.PerformUndo();
            chm.PushNewAction(flha);

            return chm;
        }

        public FlipLayerFunction(string historyName, ImageResource image, FlipType flipType, int layerIndex)
            : base(ActionFlags.None)
        {
            this.historyName = historyName;
            this.flipType = flipType;
            this.undoImage = image;
            this.layerIndex = layerIndex;
        }
    }
}
