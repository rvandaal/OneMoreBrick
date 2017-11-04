using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OneMoreBrick {
    /// <summary>
    /// Reponsibilities:
    /// 1. Create and position nodes for each DiagramNode
    /// 2. Drawing relations and force vectors
    /// 3. Provide trigger for simulation
    /// 4. Pass mouse events to Diagram
    /// </summary>
    public class GameCanvas : Canvas {

        private bool started;
        private DateTime previousTime;

        private GameViewModel gameViewModel;

        private Dictionary<UIElement, BallViewModel> control2model = new Dictionary<UIElement, BallViewModel>();


        public GameCanvas() {
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, EventArgs e) {
            CompositionTargetEx.FrameUpdating += OnCompositionTargetRendering;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            gameViewModel = (GameViewModel)DataContext;
            //
            // Instead of using a binding to ItemsSource, we listen to the CollectionChanged event.
            // So, this still has to happen in DiagramCanvas.
            //
            ((INotifyCollectionChanged)gameViewModel.BallViewModels).CollectionChanged += OnCollectionChanged;
            InitNodes();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            SyncNodes();
        }

        private void InitNodes() {
            if (IsLoaded) {
                SyncNodes();
            } else {
                Loaded += (sender, args) => SyncNodes();
            }
        }

        private void SyncNodes() {
            Children.Clear();
            control2model.Clear();
            foreach (var ballViewModel in gameViewModel.BallViewModels) {
                AddNode(ballViewModel);
            }
        }

        private void AddNode(BallViewModel ballViewModel) {
            Ellipse e = new Ellipse() { Width = ballViewModel.Size.Width, Height = ballViewModel.Size.Height, Fill = Brushes.White };
            control2model[e] = ballViewModel;
            Children.Add(e);
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e) {
            //
            // Gebruik floats ipv doubles, veel sneller:
            // https://evanl.wordpress.com/category/graphics/
            //
            foreach (var child in Children) {
                var element = (UIElement)child;
                var ballViewModel = control2model[element];
                SetLeft(element, ballViewModel.TopLeft.X);
                SetTop(element, ballViewModel.TopLeft.Y);
            }
            UpdateLayout();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            var now = DateTime.Now;
            if (started) {
                TimeSpan deltaTime = now - previousTime;
                SimulateStep(deltaTime, dc);
            }
            started = true;
            previousTime = now;
        }

        private void SimulateStep(TimeSpan timeStep, DrawingContext dc) {
            TimeSpan deltaTime = timeStep;
            gameViewModel.Simulate(
                Math.Min(deltaTime.TotalSeconds, 0.1),
                ActualWidth,
                ActualHeight
            );
        }

        //private void DrawForces(DrawingContext dc) {
        //    foreach (var node in gameViewModel.Nodes.Where(n => n.IsVisible)) {
        //        dc.PushOpacity(0.5);
        //        DrawForce(node, ForceType.Repulsion, dc, redPen);
        //        DrawForce(node, ForceType.NodeAttraction, dc, bluePen);
        //        DrawForce(node, ForceType.DiscreteAngles, dc, greenPen);
        //        DrawForce(node, ForceType.NeighbourConnectorRepulsion, dc, redPen);
        //        DrawForce(node, ForceType.Node2LinkRepulsion, dc, brownPen);
        //        dc.Pop();
        //        DrawTotalForce(node, dc, blackPen);
        //    }
        //}

        //private void DrawTotalForce(DiagramNode node, DrawingContext dc, Pen pen) {
        //    //Vector3D force = new Vector3D(0,0,0);
        //    //foreach (var value in node.Forces.Values) {
        //    //    force += value;
        //    //}
        //    DrawForce(node, node.Force, dc, pen);
        //}

        //private void DrawForce(DiagramNode node, ForceType type, DrawingContext dc, Pen pen) {
        //    if (!node.Forces.ContainsKey(type)) {
        //        return;
        //    }
        //    var force = node.Forces[type];
        //    DrawForce(node, force, dc, pen);
        //}

        //private void DrawForce(DiagramNode node, Vector force, DrawingContext dc, Pen pen) {
        //    var pos0 = node.Pos;
        //    var vec1 = new Vector(force.X, force.Y);
        //    var vec2 = vec1 * rotation45Matrix;
        //    var vec3 = vec1 * rotationMin45Matrix;
        //    vec2.Normalize();
        //    vec3.Normalize();
        //    var pos4 = pos0 + vec1 * 10;
        //    var pos5 = pos4 - vec2 * 4;
        //    var pos6 = pos4 - vec3 * 4;

        //    dc.DrawLine(pen, pos0, pos4);
        //    dc.DrawLine(pen, pos4, pos5);
        //    dc.DrawLine(pen, pos4, pos6);
        //}

        //private bool isMouseLeftButtonDown;
        //private Point startDraggingPoint;

        //public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        //    var pos = e.GetPosition(this);
        //    startDraggingPoint = pos;
        //    e.Handled = gameViewModel.UmlDiagramInteractor.HandleMouseLeftButtonDown(pos);
        //    isMouseLeftButtonDown = true;
        //    Mouse.Capture(this);
        //}

        //public void OnMouseMove(object sender, MouseEventArgs e) {
        //    if (isMouseLeftButtonDown) {
        //        var pos = e.GetPosition(this);
        //        e.Handled = gameViewModel.UmlDiagramInteractor.HandleMouseMove(pos, startDraggingPoint);
        //    }
        //}

        //public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        //    var pos = e.GetPosition(this);
        //    if (isMouseLeftButtonDown) {
        //        e.Handled = gameViewModel.UmlDiagramInteractor.HandleMouseLeftButtonUp(pos);
        //        isMouseLeftButtonDown = false;
        //    }
        //    Mouse.Capture(null);
        //}
    }
}
