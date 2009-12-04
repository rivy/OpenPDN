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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.Tools
{
    internal abstract class MoveToolBase
        : Tool
    {
        protected Cursor moveToolCursor;
        protected bool dontDrop = false; // so that OnSelectionChanging() can tell who is raising the event ... don't drop the pixels if WE caused the event
        protected float angleDelta;
        protected MoveNubRenderer[] moveNubs;
        protected RotateNubRenderer rotateNub;
        protected bool tracking;
        protected Context context;
        protected bool hostShouldShowAngle;
        protected float hostAngle;
        protected List<HistoryMemento> currentHistoryMementos = new List<HistoryMemento>();
        protected bool deactivateOnLayerChange = true;
        protected bool enableOutline = true;

        public override bool DeactivateOnLayerChange
        {
            get
            {
                return this.deactivateOnLayerChange;
            }
        }
        
        protected enum Mode
        {
            Translate,
            Scale,
            Rotate
        }

        // Corresponds to array positions in this.moveNubs for easy mapping between the two
        protected enum Edge
        {
            TopLeft = 0,
            Top = 1,
            TopRight = 2,
            Right = 3,
            BottomRight = 4,
            Bottom = 5,
            BottomLeft = 6,
            Left = 7,
            None = 99
        }

        [Serializable]
        protected class Context
            : ICloneable,
              ISerializable,
              IDisposable
        {
            public bool lifted;
            public Guid seriesGuid;
            public Matrix baseTransform;       // a copy of the selection's interim transform at the time of mouse-down
            public Matrix liftTransform;       // a copy of the selection's interim transform at the time of lifting
            public Matrix deltaTransform;      // the transformations made since lifting
            public RectangleF liftedBounds;
            public RectangleF startBounds;
            public float startAngle;
            public PdnGraphicsPath startPath;
            public Mode currentMode;
            public Edge startEdge;
            public Point startMouseXY;
            public Point offset;

            private float[] GetMatrixElements(Matrix m)
            {
                if (m == null)
                {
                    return null;
                }
                else
                {
                    return m.Elements;
                }
            }

            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("lifted", this.lifted);
                info.AddValue("seriesGuid", this.seriesGuid);
                info.AddValue("baseTransform", GetMatrixElements(this.baseTransform));
                info.AddValue("deltaTransform", GetMatrixElements(this.deltaTransform));
                info.AddValue("liftTransform", GetMatrixElements(this.liftTransform));
                info.AddValue("liftedBounds", this.liftedBounds);
                info.AddValue("startBounds", this.startBounds);
                info.AddValue("startAngle", this.startAngle);
                info.AddValue("startPath", this.startPath);
                info.AddValue("currentMode", this.currentMode);
                info.AddValue("startEdge", this.startEdge);
                info.AddValue("startMouseXY", this.startMouseXY);
                info.AddValue("offset", this.offset);
            }

            private Matrix ReadMatrix(SerializationInfo info, StreamingContext context, string name)
            {
                Matrix m;
                float[] e = (float[])info.GetValue(name, typeof(float[]));

                if (e == null)
                {
                    m = null;
                }
                else
                {
                    m = new Matrix(e[0], e[1], e[2], e[3], e[4], e[5]);
                }

                return m;
            }

            public Context(SerializationInfo info, StreamingContext context)
            {
                this.lifted = (bool)info.GetValue("lifted", typeof(bool));
                this.seriesGuid = (Guid)info.GetValue("seriesGuid", typeof(Guid));
                this.baseTransform = ReadMatrix(info, context, "baseTransform");
                this.deltaTransform = ReadMatrix(info, context, "deltaTransform");
                this.liftTransform = ReadMatrix(info, context, "liftTransform");
                this.liftedBounds = (RectangleF)info.GetValue("liftedBounds", typeof(RectangleF));
                this.startBounds = (RectangleF)info.GetValue("startBounds", typeof(RectangleF));
                this.startAngle = (float)info.GetValue("startAngle", typeof(float));
                this.startPath = (PdnGraphicsPath)info.GetValue("startPath", typeof(PdnGraphicsPath));
                this.currentMode = (Mode)info.GetValue("currentMode", typeof(Mode));
                this.startEdge = (Edge)info.GetValue("startEdge", typeof(Edge));
                this.startMouseXY = (Point)info.GetValue("startMouseXY", typeof(Point));
                this.offset = (Point)info.GetValue("offset", typeof(Point));
            }

            public Context()
            {
            }

            public Context(Context cloneMe)
            {
                this.lifted = cloneMe.lifted;
                this.seriesGuid = cloneMe.seriesGuid;

                if (cloneMe.baseTransform != null)
                {
                    this.baseTransform = cloneMe.baseTransform.Clone();
                }

                if (cloneMe.deltaTransform != null)
                {
                    this.deltaTransform = cloneMe.deltaTransform.Clone();
                }

                if (cloneMe.liftTransform != null)
                {
                    this.liftTransform = cloneMe.liftTransform.Clone();
                }

                this.liftedBounds = cloneMe.liftedBounds;
                this.startBounds = cloneMe.startBounds;
                this.startAngle = cloneMe.startAngle;

                if (cloneMe.startPath != null)
                {
                    this.startPath = cloneMe.startPath.Clone();
                }

                this.currentMode = cloneMe.currentMode;
                this.startEdge = cloneMe.startEdge;

                this.startMouseXY = cloneMe.startMouseXY;
                this.offset = cloneMe.offset;
            }

            ~Context()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.baseTransform != null)
                    {
                        this.baseTransform.Dispose();
                        this.baseTransform = null;
                    }

                    if (this.deltaTransform != null)
                    {
                        this.deltaTransform.Dispose();
                        this.deltaTransform = null;
                    }

                    if (this.liftTransform != null)
                    {
                        this.liftTransform.Dispose();
                        this.liftTransform = null;
                    }

                    if (this.startPath != null)
                    {
                        this.startPath.Dispose();
                        this.startPath = null;
                    }
                }
            }

            public virtual object Clone()
            {
                return new Context(this);
            }
        }

        protected class CompoundToolHistoryMemento
            : ToolHistoryMemento
        {
            private CompoundHistoryMemento compoundHistoryMemento;

            public CompoundHistoryMemento CompoundHistoryMemento
            {
                get
                {
                    return this.compoundHistoryMemento;
                }
            }

            protected override HistoryMemento OnToolUndo()
            {
                CompoundHistoryMemento chm = (CompoundHistoryMemento)this.compoundHistoryMemento.PerformUndo();
                CompoundToolHistoryMemento cthm = new CompoundToolHistoryMemento(chm, DocumentWorkspace, this.Name, this.Image);
                return cthm;
            }

            public CompoundToolHistoryMemento(CompoundHistoryMemento chm, DocumentWorkspace documentWorkspace, string name, ImageResource image)
                : base(documentWorkspace, name, image)
            {
                this.compoundHistoryMemento = chm;
            }
        }

        public bool HostShouldShowAngle
        {
            get
            {
                return this.hostShouldShowAngle;
            }
        }

        public float HostAngle
        {
            get
            {
                return this.hostAngle;
            }
        }

        protected void DestroyNubs()
        {
            if (this.moveNubs != null)
            {
                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    this.RendererList.Remove(this.moveNubs[i]);
                    this.moveNubs[i].Dispose();
                    this.moveNubs[i] = null;
                }

                this.moveNubs = null;
            }

            if (this.rotateNub != null)
            {
                this.RendererList.Remove(this.rotateNub);
                this.rotateNub.Dispose();
                this.rotateNub = null;
            }
        }

        protected PointF GetEdgeVector(Edge edge)
        {
            PointF u;
            switch (edge)
            {
                case Edge.TopLeft:
                    u = new PointF(-1, -1);
                    break;

                case Edge.Top:
                    u = new PointF(0, -1);
                    break;

                case Edge.TopRight:
                    u = new PointF(1, -1);
                    break;

                case Edge.Left:
                    u = new PointF(-1, 0);
                    break;

                case Edge.Right:
                    u = new PointF(1, 0);
                    break;

                case Edge.BottomLeft:
                    u = new PointF(-1, 1);
                    break;

                case Edge.BottomRight:
                    u = new PointF(1, 1);
                    break;

                case Edge.Bottom:
                    u = new PointF(0, 1);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            return u;
        }

        protected void DetermineMoveMode(MouseEventArgs e, out Mode mode, out Edge edge)
        {
            mode = Mode.Translate;
            edge = Edge.None;

            if (e.Button == MouseButtons.Right)
            {
                mode = Mode.Rotate;
            }
            else
            {
                float minDistance = float.MaxValue;
                Point mousePt = new Point(e.X, e.Y);

                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    MoveNubRenderer nub = this.moveNubs[i];

                    if (nub.IsPointTouching(mousePt, true))
                    {
                        float distance = Utility.Distance((PointF)mousePt, nub.Location);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            mode = Mode.Scale;
                            edge = (Edge)i;
                        }
                    }
                }
            }

            return;
        }

        protected override void OnPulse()
        {
            if (this.moveNubs != null)
            {
                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    // Oscillate between 25% and 100% alpha over a period of 2 seconds
                    // Alpha value of 100% is sustained for a large duration of this period
                    const int period = 10000 * 2000; // 10000 ticks per ms, 2000ms per period
                    long tick = (DateTime.Now.Ticks % period) + (i * (period / this.moveNubs.Length));
                    double sin = Math.Sin(((double)tick / (double)period) * (2.0 * Math.PI));
                    // sin is [-1, +1]

                    sin = Math.Min(0.5, sin);
                    // sin is [-1, +0.5]

                    sin += 1.0;
                    // sin is [0, 1.5]

                    sin /= 2.0;
                    // sin is [0, 0.75]

                    sin += 0.25;
                    // sin is [0.25, 1]

                    int newAlpha = (int)(sin * 255.0);
                    int clampedAlpha = Utility.Clamp(newAlpha, 0, 255);
                    this.moveNubs[i].Alpha = clampedAlpha;
                }
            }

            base.OnPulse();
        }

        protected void PositionNubs(Mode currentMode)
        {
            if (this.moveNubs == null)
            {
                this.moveNubs = new MoveNubRenderer[8];

                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    this.moveNubs[i] = new MoveNubRenderer(this.RendererList);
                    this.RendererList.Add(this.moveNubs[i], false);
                }

                RectangleF bounds = Selection.GetBoundsF(false);

                this.moveNubs[(int)Edge.TopLeft].Location = new PointF(bounds.Left, bounds.Top);
                this.moveNubs[(int)Edge.TopLeft].Shape = MoveNubShape.Circle;

                this.moveNubs[(int)Edge.Top].Location = new PointF((bounds.Left + bounds.Right) / 2.0f, bounds.Top);

                this.moveNubs[(int)Edge.TopRight].Location = new PointF(bounds.Right, bounds.Top);
                this.moveNubs[(int)Edge.TopRight].Shape = MoveNubShape.Circle;

                this.moveNubs[(int)Edge.Left].Location = new PointF(bounds.Left, (bounds.Top + bounds.Bottom) / 2.0f);
                this.moveNubs[(int)Edge.Right].Location = new PointF(bounds.Right, (bounds.Top + bounds.Bottom) / 2.0f);

                this.moveNubs[(int)Edge.BottomLeft].Location = new PointF(bounds.Left, bounds.Bottom);
                this.moveNubs[(int)Edge.BottomLeft].Shape = MoveNubShape.Circle;

                this.moveNubs[(int)Edge.Bottom].Location = new PointF((bounds.Left + bounds.Right) / 2.0f, bounds.Bottom);

                this.moveNubs[(int)Edge.BottomRight].Location = new PointF(bounds.Right, bounds.Bottom);
                this.moveNubs[(int)Edge.BottomRight].Shape = MoveNubShape.Circle;
            }

            if (this.rotateNub == null)
            {
                this.rotateNub = new RotateNubRenderer(this.RendererList);
                rotateNub.Visible = false;
                this.RendererList.Add(this.rotateNub, false);
            }

            if (Selection.IsEmpty)
            {
                foreach (SurfaceBoxRenderer nub in this.moveNubs)
                {
                    nub.Visible = false;
                }

                this.rotateNub.Visible = false;
            }
            else
            {
                foreach (MoveNubRenderer nub in this.moveNubs)
                {
                    nub.Visible = !tracking || currentMode == Mode.Scale;
                    nub.Transform = Selection.GetInterimTransformReadOnly();
                }
            }
        }

        protected void HideNubs()
        {
            if (this.moveNubs != null)
            {
                foreach (SurfaceBoxRenderer sbr in this.moveNubs)
                {
                    sbr.Visible = false;
                }
            }

            if (this.rotateNub != null)
            {
                this.rotateNub.Visible = false;
            }
        }

        protected Edge FlipEdgeVertically(Edge flipMe)
        {
            Edge flippedEdge;

            switch (flipMe)
            {
                default:
                    throw new InvalidEnumArgumentException();

                case Edge.Bottom:
                    flippedEdge = Edge.Top;
                    break;

                case Edge.BottomLeft:
                    flippedEdge = Edge.TopLeft;
                    break;
                
                case Edge.BottomRight:
                    flippedEdge = Edge.TopRight;
                    break;
                
                case Edge.Left:
                    flippedEdge = Edge.Left;
                    break;
                
                case Edge.None:
                    flippedEdge = Edge.None;
                    break;
                
                case Edge.Right:
                    flippedEdge = Edge.Right;
                    break;
                
                case Edge.Top:
                    flippedEdge = Edge.Bottom;
                    break;
                
                case Edge.TopLeft:
                    flippedEdge = Edge.BottomLeft;
                    break;
                
                case Edge.TopRight:
                    flippedEdge = Edge.BottomRight;
                    break;
            }

            return flippedEdge;
        }


        // Constrains the given width and height to the aspect ratio of this.liftedBounds
        protected void ConstrainScaling(RectangleF liftedBounds, float startWidth, float startHeight, 
            float newWidth, float newHeight, out float newXScale, out float newYScale)
        {
            float hRatio = newWidth / (float)liftedBounds.Width;
            float vRatio = newHeight / (float)liftedBounds.Height;

            float bestScale = Math.Min(hRatio, vRatio);
            float bestWidth = (float)liftedBounds.Width * bestScale;
            float bestHeight = (float)liftedBounds.Height * bestScale;

            newXScale = bestWidth / startWidth;
            newYScale = bestHeight / startHeight;
        }

        // Constrains to nearest 15 degree angle
        protected float ConstrainAngle(float angle)
        {
            while (angle < 0)
            {
                angle += 360.0f;
            }

            int iangle = (int)angle;
            int lowerBound = (iangle / 15) * 15;
            int upperBound = lowerBound + 15;
            float lowerDiff = Math.Abs(angle - (float)lowerBound);
            float upperDiff = Math.Abs(angle - (float)upperBound);

            float newAngle;

            if (lowerDiff < upperDiff)
            {
                newAngle = (float)lowerBound;
            }
            else
            {
                newAngle = (float)upperBound;
            }

            if (newAngle > 180.0f)
            {
                newAngle -= 360.0f;
            }

            return newAngle;
        }

        protected override void OnKeyPress(Keys key)
        {
            if (!tracking)
            {
                int dx = 0;
                int dy = 0;

                if ((key & Keys.KeyCode) == Keys.Left)
                {
                    dx = -1;
                }
                else if ((key & Keys.KeyCode) == Keys.Right)
                {
                    dx = +1;
                } 
                else if ((key & Keys.KeyCode) == Keys.Up)
                {
                    dy = -1;
                }
                else if ((key & Keys.KeyCode) == Keys.Down) 
                {
                    dy = +1;
                }

                if ((key & Keys.Control) != Keys.None)
                {
                    dx *= 10;
                    dy *= 10;
                }

                // Simulate moving the selection
                if (dx != 0 || dy != 0)
                {
                    Point pos = Cursor.Position;
                    Point docPos = new Point(-70000, -70000);
                    Point newDocPos = new Point(docPos.X + dx, docPos.Y + dy);
                    OnMouseDown(new MouseEventArgs(MouseButtons.Left, 0, docPos.X, docPos.Y, 0));
                    OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, newDocPos.X, newDocPos.Y, 0));
                    OnMouseUp(new MouseEventArgs(MouseButtons.Left, 0, newDocPos.X, newDocPos.Y, 0));
                }
            } 
            else
            {
                base.OnKeyPress(key);
            }
        }

        protected abstract void OnLift(MouseEventArgs e);
        protected abstract void Drop();
        protected abstract void PreRender();
        protected abstract void Render(Point newOffset, bool useNewOffset);
        protected abstract void PushContextHistoryMemento();

        protected void Lift(MouseEventArgs e)
        {
            this.PushContextHistoryMemento();

            this.context.seriesGuid = Guid.NewGuid();
            DetermineMoveMode(e, out this.context.currentMode, out this.context.startEdge);

            // lift!
            this.context.startBounds = this.context.liftedBounds;
            this.context.liftedBounds = Selection.GetBoundsF(false);
            this.context.startMouseXY = new Point(e.X, e.Y);
            this.context.offset = new Point(0, 0);
            this.context.startAngle = 0.0f;
            this.context.lifted = true;
            this.context.liftTransform = Selection.GetCumulativeTransformCopy();

            OnLift(e);

            PositionNubs(this.context.currentMode);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (tracking)
            {
                return;
            }

            bool determinedMoveMode = false;
            Mode newMode = Mode.Translate;
            Edge newEdge = Edge.None;

            if (Selection.IsEmpty)
            {
                SelectionHistoryMemento shm = new SelectionHistoryMemento(
                    HistoryFunctions.SelectAllFunction.StaticName,
                    PdnResources.GetImageResource("Icons.MenuEditSelectAllIcon.png"),
                    DocumentWorkspace);

                DocumentWorkspace.History.PushNewMemento(shm);

                DocumentWorkspace.Selection.PerformChanging();
                DocumentWorkspace.Selection.Reset();
                DocumentWorkspace.Selection.SetContinuation(Document.Bounds, CombineMode.Replace);
                DocumentWorkspace.Selection.CommitContinuation();
                DocumentWorkspace.Selection.PerformChanged();

                if (e.Button == MouseButtons.Right)
                {
                    newMode = Mode.Rotate;
                }
                else
                {
                    newMode = Mode.Translate;
                }

                newEdge = Edge.None;

                determinedMoveMode = true;
            }

            DocumentWorkspace.EnableSelectionOutline = this.enableOutline;

            if (!context.lifted)
            {
                Lift(e);
            }

            PushContextHistoryMemento();

            if (!determinedMoveMode)
            {
                DetermineMoveMode(e, out newMode, out newEdge);
                determinedMoveMode = true;
            }

            if (this.context.deltaTransform != null)
            {
                this.context.deltaTransform.Dispose();
                this.context.deltaTransform = null;
            }

            this.context.deltaTransform = new Matrix();
            this.context.deltaTransform.Reset();

            if (newMode == Mode.Translate ||
                newMode == Mode.Scale ||
                newMode != this.context.currentMode ||
                newMode == Mode.Rotate)
            {
                this.context.startBounds = Selection.GetBoundsF();
                this.context.startMouseXY = new Point(e.X, e.Y);
                this.context.offset = new Point(0, 0);

                if (this.context.baseTransform != null)
                {
                    this.context.baseTransform.Dispose();
                    this.context.baseTransform = null;
                }

                this.context.baseTransform = Selection.GetInterimTransformCopy();
            }

            this.context.startEdge = newEdge;
            this.context.currentMode = newMode;
            PositionNubs(this.context.currentMode);

            tracking = true;
            this.rotateNub.Visible = (this.context.currentMode == Mode.Rotate);

            if (this.context.startPath != null)
            {
                this.context.startPath.Dispose();
                this.context.startPath = null;
            }

            this.context.startPath = Selection.CreatePath();
            this.context.startAngle = Utility.GetAngleOfTransform(Selection.GetInterimTransformReadOnly());

            SelectionHistoryMemento sha1 = new SelectionHistoryMemento(this.Name, this.Image, this.DocumentWorkspace);
            this.currentHistoryMementos.Add(sha1);

            OnMouseMove(e);

            if (this.enableOutline)
            {
                DocumentWorkspace.ResetOutlineWhiteOpacity();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            StringBuilder sbLogger = new StringBuilder();

            try
            {
                OnMouseMoveImpl(e, sbLogger);
            }

            catch (Exception ex)
            {
                throw new ApplicationException("Tracing data: " + sbLogger.ToString(), ex);
            }
        }

        private void OnMouseMoveImpl(MouseEventArgs e, StringBuilder sbLogger)
        {
            if (!this.tracking)
            {
                sbLogger.Append("1 ");
                Cursor cursor = this.moveToolCursor;

                for (int i = 0; i < this.moveNubs.Length; ++i)
                {
                    sbLogger.Append("2 ");
                    MoveNubRenderer nub = this.moveNubs[i];
                    sbLogger.Append("3 ");

                    if (nub.Visible && nub.IsPointTouching(new Point(e.X, e.Y), true))
                    {
                        sbLogger.Append("4 ");
                        cursor = this.handCursor;
                        break;
                    }
                }

                this.Cursor = cursor;
                sbLogger.Append("5 ");
            }
            else
            {
                sbLogger.Append("6 ");
                if (this.context.currentMode != Mode.Translate)
                {
                    sbLogger.Append("7 ");
                    this.Cursor = this.handCursorMouseDown;
                }

                sbLogger.Append("8 ");
                Point newMouseXY = new Point(e.X, e.Y);
                Point newOffset = new Point(newMouseXY.X - context.startMouseXY.X, newMouseXY.Y - context.startMouseXY.Y);

                PreRender();

                this.dontDrop = true;

                sbLogger.Append("9 ");
                Selection.PerformChanging();

                using (Matrix translateMatrix = new Matrix())
                {
                    RectangleF rect;
                    translateMatrix.Reset();

                    if (this.context.baseTransform != null)
                    {
                        Selection.SetInterimTransform(this.context.baseTransform);
                    }

                    Matrix interim = Selection.GetInterimTransformCopy();

                    switch (this.context.currentMode)
                    {
                        case Mode.Translate:
                            translateMatrix.Translate((float)newOffset.X, (float)newOffset.Y, MatrixOrder.Append);
                            break;

                        case Mode.Rotate:
                            rect = this.context.liftedBounds;
                            PointF center = new PointF(rect.X + (rect.Width / 2.0f), rect.Y + (rect.Height / 2.0f));
                            center = Utility.TransformOnePoint(interim, center);
                            double theta1 = Math.Atan2(context.startMouseXY.Y - center.Y, context.startMouseXY.X - center.X);
                            double theta2 = Math.Atan2(e.Y - center.Y, e.X - center.X);
                            double thetaDelta = theta2 - theta1;
                            this.angleDelta = (float)(thetaDelta * (180.0f / Math.PI));
                            float angle = this.context.startAngle + this.angleDelta;

                            if ((ModifierKeys & Keys.Shift) != 0)
                            {
                                angle = ConstrainAngle(angle);
                                angleDelta = angle - this.context.startAngle;
                            }

                            translateMatrix.RotateAt(angleDelta, center, MatrixOrder.Append);
                            this.rotateNub.Location = center;
                            this.rotateNub.Angle = this.context.startAngle + angleDelta;
                            break;

                        case Mode.Scale:
                            PointF xyAxes = GetEdgeVector(this.context.startEdge);
                            PointF xAxis = new PointF(xyAxes.X, 0);
                            PointF yAxis = new PointF(0, xyAxes.Y);
                            PointF edgeX = Utility.TransformOneVector(interim, xAxis);
                            PointF edgeY = Utility.TransformOneVector(interim, yAxis);
                            PointF edgeXN = Utility.NormalizeVector2(edgeX);
                            PointF edgeYN = Utility.NormalizeVector2(edgeY);

                            PointF xu;
                            float xulen;
                            PointF xv;
                            Utility.GetProjection((PointF)newOffset, edgeXN, out xu, out xulen, out xv);

                            PointF yu;
                            float yulen;
                            PointF yv;
                            Utility.GetProjection((PointF)newOffset, edgeYN, out yu, out yulen, out yv);

                            PdnGraphicsPath startPath2 = this.context.startPath.Clone();
                            RectangleF sp2Bounds = startPath2.GetBounds();

                            PointF sp2BoundsCenter = new PointF((sp2Bounds.Left + sp2Bounds.Right) / 2.0f,
                                (sp2Bounds.Top + sp2Bounds.Bottom) / 2.0f);

                            float tAngle = Utility.GetAngleOfTransform(interim);
                            bool isFlipped = Utility.IsTransformFlipped(interim);

                            using (Matrix spm = new Matrix())
                            {
                                spm.Reset();
                                spm.RotateAt(-tAngle, sp2BoundsCenter, MatrixOrder.Append);
                                translateMatrix.RotateAt(-tAngle, sp2BoundsCenter, MatrixOrder.Append);
                                startPath2.Transform(spm);
                            }

                            RectangleF spBounds2 = startPath2.GetBounds();

                            startPath2.Dispose();
                            startPath2 = null;

                            float xTranslate;
                            float yTranslate;
                            bool allowConstrain;

                            Edge theEdge = this.context.startEdge;

                            // If the transform is flipped, then GetTransformAngle will return 180 degrees
                            // even though no rotation has actually taken place. Thus we have to scratch
                            // our head and go "hmm, let's make some adjustments to this." Otherwise stretching
                            // the top and bottom nubs goes in the wrong direction.
                            if (isFlipped)
                            {
                                theEdge = FlipEdgeVertically(theEdge);
                            }

                            switch (theEdge)
                            {
                                default:
                                    throw new InvalidEnumArgumentException();

                                case Edge.TopLeft:
                                    allowConstrain = true;
                                    xTranslate = -spBounds2.X - spBounds2.Width;
                                    yTranslate = -spBounds2.Y - spBounds2.Height;
                                    break;

                                case Edge.Top:
                                    allowConstrain = false;
                                    xTranslate = 0;
                                    yTranslate = -spBounds2.Y - spBounds2.Height;
                                    break;

                                case Edge.TopRight:
                                    allowConstrain = true;
                                    xTranslate = -spBounds2.X;
                                    yTranslate = -spBounds2.Y - spBounds2.Height;
                                    break;

                                case Edge.Left:
                                    allowConstrain = false;
                                    xTranslate = -spBounds2.X - spBounds2.Width;
                                    yTranslate = 0;
                                    break;

                                case Edge.Right:
                                    allowConstrain = false;
                                    xTranslate = -spBounds2.X;
                                    yTranslate = 0;
                                    break;

                                case Edge.BottomLeft:
                                    allowConstrain = true;
                                    xTranslate = -spBounds2.X - spBounds2.Width;
                                    yTranslate = -spBounds2.Y;
                                    break;

                                case Edge.Bottom:
                                    allowConstrain = false;
                                    xTranslate = 0;
                                    yTranslate = -spBounds2.Y;
                                    break;

                                case Edge.BottomRight:
                                    allowConstrain = true;
                                    xTranslate = -spBounds2.X;
                                    yTranslate = -spBounds2.Y;
                                    break;
                            }

                            translateMatrix.Translate(xTranslate, yTranslate, MatrixOrder.Append);

                            float newWidth = spBounds2.Width + xulen;
                            float newHeight = spBounds2.Height + yulen;
                            float xScale = newWidth / spBounds2.Width;
                            float yScale = newHeight / spBounds2.Height;

                            if (allowConstrain && (this.ModifierKeys & Keys.Shift) != 0)
                            {
                                ConstrainScaling(this.context.liftedBounds, spBounds2.Width, spBounds2.Height,
                                    newWidth, newHeight, out xScale, out yScale);
                            }

                            translateMatrix.Scale(xScale, yScale, MatrixOrder.Append);
                            translateMatrix.Translate(-xTranslate, -yTranslate, MatrixOrder.Append);
                            translateMatrix.RotateAt(+tAngle, sp2BoundsCenter, MatrixOrder.Append);

                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }

                    this.context.deltaTransform.Reset();
                    this.context.deltaTransform.Multiply(this.context.liftTransform, MatrixOrder.Append);
                    this.context.deltaTransform.Multiply(translateMatrix, MatrixOrder.Append);

                    translateMatrix.Multiply(this.context.baseTransform, MatrixOrder.Prepend);

                    Selection.SetInterimTransform(translateMatrix);

                    interim.Dispose();
                    interim = null;
                }

                // advertise our angle of rotation to any host (i.e. mainform) that might want to use that information
                this.hostShouldShowAngle = this.rotateNub.Visible;
                this.hostAngle = -this.rotateNub.Angle;

                Selection.PerformChanged();
                dontDrop = false;

                Render(newOffset, true);
                Update();

                sbLogger.Append("a ");
                this.context.offset = newOffset;

                sbLogger.Append("b ");

                if (this.enableOutline)
                {
                    DocumentWorkspace.ResetOutlineWhiteOpacity();
                }

                sbLogger.Append("c ");
            }

            sbLogger.Append("d ");
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            DocumentWorkspace.EnableSelectionOutline = true;
            base.OnMouseUp (e);
        }

        public MoveToolBase(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, 
            string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems)
            : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, toolBarConfigItems)
        {
        }
    }
}
