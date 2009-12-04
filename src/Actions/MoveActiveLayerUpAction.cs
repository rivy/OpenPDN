/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.HistoryFunctions;
using PaintDotNet.HistoryMementos;
using System;

namespace PaintDotNet.Actions
{
    internal sealed class MoveActiveLayerUpAction
        : DocumentWorkspaceAction
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MoveLayerUp.HistoryMementoName");
            }
        }

        public static ImageResource StaticImage
        {
            get
            {
                return PdnResources.GetImageResource("Icons.MenuLayersMoveLayerUpIcon.png");
            }
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            HistoryMemento hm = null;
            int index = documentWorkspace.ActiveLayerIndex;

            if (index != documentWorkspace.Document.Layers.Count - 1)
            {
                SwapLayerFunction slf = new SwapLayerFunction(index, index + 1);
                HistoryMemento slfhm = slf.Execute(documentWorkspace);

                hm = new CompoundHistoryMemento(
                    StaticName,
                    StaticImage,
                    new HistoryMemento[] { slfhm });

                documentWorkspace.ActiveLayer = (Layer)documentWorkspace.Document.Layers[index + 1];
            }

            return hm;
        }

        public MoveActiveLayerUpAction()
            : base(ActionFlags.None)
        {
        }
    }
}
