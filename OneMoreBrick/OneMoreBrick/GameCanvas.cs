using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OneMoreBrick {
    /// <summary>
    /// Responsible to draw all game elements on screen.
    /// </summary>
    public class GameCanvas : Canvas {

        private bool started;
        private DateTime previousTime;

        private GameViewModel gameViewModel;
        private bool isMouseLeftButtonDown;

        private readonly Pen whitePen = new Pen(new SolidColorBrush(Colors.White), 1);

        public GameCanvas() {
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, EventArgs e) {
            CompositionTargetEx.FrameUpdating += OnCompositionTargetRendering;
            gameViewModel.SetViewportSize(RenderSize);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            gameViewModel = (GameViewModel)DataContext;            
        }



        private void OnCompositionTargetRendering(object sender, EventArgs e) {
            //
            // Gebruik floats ipv doubles, veel sneller:
            // https://evanl.wordpress.com/category/graphics/
            //
            if(gameViewModel.State == State.PlacingTarget) {
                MouseLeftButtonDown += OnMouseLeftButtonDown;
                MouseMove += OnMouseMove;
                MouseLeftButtonUp += OnMouseLeftButtonUp;
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

            DrawBalls(dc);

            if (isMouseLeftButtonDown) {
                DrawShootingLine(dc);
            }
        }

        private void SimulateStep(TimeSpan timeStep, DrawingContext dc) {
            TimeSpan deltaTime = timeStep;
            gameViewModel.Simulate(
                Math.Min(deltaTime.TotalSeconds, 0.1),
                ActualWidth,
                ActualHeight
            );
        }

        private void DrawBalls(DrawingContext dc) {
            foreach (var ballViewModel in gameViewModel.BallViewModels) {
                dc.DrawEllipse(Brushes.White, null, ballViewModel.Pos, ballViewModel.Size.Width / 2, ballViewModel.Size.Height / 2);
            }
        }

        private void DrawShootingLine(DrawingContext dc) {
            dc.DrawLine(whitePen, gameViewModel.ShootingPoint, mousePosition);
        }

        Point mousePosition;

        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            isMouseLeftButtonDown = true;
            Mouse.Capture(this);
        }

        public void OnMouseMove(object sender, MouseEventArgs e) {
            if (isMouseLeftButtonDown) {
                mousePosition = e.GetPosition(this);
            }
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            isMouseLeftButtonDown = false;
            gameViewModel.ShootAtTarget(e.GetPosition(this));
            Mouse.Capture(null);
            MouseLeftButtonDown -= OnMouseLeftButtonDown;
            MouseMove -= OnMouseMove;
            MouseLeftButtonUp -= OnMouseLeftButtonUp;
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
        //private Point startDraggingPoint;
    }
}
