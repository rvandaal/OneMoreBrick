using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OneMoreBrick {
    /// <summary>
    /// Voert physics uit, doet collision detection enz.
    /// </summary>
    class GameViewModel : ViewModelBase {

        public GameViewModel() {
            ballViewModels = new ObservableCollection<BallViewModel>();
            BallViewModels = new ReadOnlyObservableCollection<BallViewModel>(ballViewModels);

            ballViewModels.Add(new BallViewModel() { Pos = new Point(50, 50), Vel = new Vector(250, 150), Size = new Size(20,20) });
        }

        #region BallViewModels

        public ReadOnlyObservableCollection<BallViewModel> BallViewModels { get; private set; }
        private readonly ObservableCollection<BallViewModel> ballViewModels;

        public void AddNode(BallViewModel diagramNode) {
            if (!ballViewModels.Contains(diagramNode)) {
                ballViewModels.Add(diagramNode);
            }
        }

        public void RemoveNode(BallViewModel diagramNode) {
            if (ballViewModels.Contains(diagramNode)) {
                ballViewModels.Remove(diagramNode);
            }
        }

        #endregion

        public virtual void ClearDiagram() {
            ballViewModels.Clear();
        }

        private bool isSimulating = true;
        public bool IsSimulating {
            get { return isSimulating; }
            set { SetProperty(value, ref isSimulating, () => IsSimulating); }
        }

        public void Simulate(double dt, double viewportWidth, double viewportHeight) {

            CalculateFps(dt);

            if (isSimulating) {
                UpdateAccVelPos(dt, viewportWidth, viewportHeight);
            }
        }

        private void UpdateAccVelPos(double deltaTime, double viewportWidth, double viewportHeight) {
            foreach (var bvm in BallViewModels) {
                bvm.Pos += bvm.Vel * deltaTime;
                if(bvm.TopLeft.X < 0 || bvm.TopRight.X > viewportWidth) {
                    bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
                }
                if (bvm.TopLeft.Y < 0 || bvm.BottomLeft.Y > viewportHeight) {
                    bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
                }
            }
        }

        #region Fps

        private double fpsTime;
        private int fpsCount;
        private int lastFpsCount;

        private int fps;
        public int Fps {
            get { return fps; }
            set { SetProperty(value, ref fps, () => Fps); }
        }

        private void CalculateFps(double dt) {
            fpsTime += dt;
            fpsCount++;
            if (fpsTime > 1) {
                fpsTime = 0;
                lastFpsCount = fpsCount;
                fpsCount = 0;
            }
            Fps = lastFpsCount;
        }

        #endregion
    }
}
