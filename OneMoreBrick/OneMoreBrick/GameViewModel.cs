using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OneMoreBrick {

    enum State {
        PlacingTarget,
        MovingTarget,
        ShootingBalls,
        WaitingForBallsToFinish
    }
    /// <summary>
    /// Voert physics uit, doet collision detection enz.
    /// </summary>
    class GameViewModel : ViewModelBase {

        public State State;
        public Point ShootingPoint { get; set; }
        public Point TargetPoint { get; set; }

        private int numberOfBallsToShoot;
        private int numberOfBallsShot;
        private double currentDeltaTime;
        private double deltaTimeBetweenShots = 0.05;

        private Vector shootingDirection;
        private double shootingVel = 800;

        Size viewportSize;

        double ballDiameter = 10;

        public GameViewModel() {
            ballViewModels = new ObservableCollection<BallViewModel>();
            BallViewModels = new ReadOnlyObservableCollection<BallViewModel>(ballViewModels);
        }

        public void SetViewportSize(Size s) {
            viewportSize = s;
            StartNewGame();
        }

        private void StartNewGame() {
            ballViewModels.Clear();
            State = State.PlacingTarget;
            ShootingPoint = new Point(viewportSize.Width / 2, viewportSize.Height - ballDiameter / 2);
            numberOfBallsToShoot = 100;
        }

        #region BallViewModels

        public ReadOnlyObservableCollection<BallViewModel> BallViewModels { get; private set; }
        private readonly ObservableCollection<BallViewModel> ballViewModels;

        public void AddBallViewModel(BallViewModel ballViewModel) {
            if (!ballViewModels.Contains(ballViewModel)) {
                ballViewModels.Add(ballViewModel);
            }
        }

        public void RemoveBallViewModel(BallViewModel ballViewModel) {
            if (ballViewModels.Contains(ballViewModel)) {
                ballViewModels.Remove(ballViewModel);
            }
        }

        #endregion

        private bool isSimulating = true;
        public bool IsSimulating {
            get { return isSimulating; }
            set { SetProperty(value, ref isSimulating, () => IsSimulating); }
        }

        public void Simulate(double dt, double viewportWidth, double viewportHeight) {

            CalculateFps(dt);

            if (isSimulating) {
                UpdateAccVelPos(dt, viewportWidth, viewportHeight);
                if(State == State.ShootingBalls) {
                    ShootBalls(dt);
                }
            }            
        }

        private void ShootBalls(double dt) {
            if(currentDeltaTime > 0) {
                currentDeltaTime -= dt;
            } else {
                var ballViewModel = new BallViewModel(ShootingPoint, shootingDirection * shootingVel) { Size = new Size(ballDiameter, ballDiameter) };
                AddBallViewModel(ballViewModel);
                numberOfBallsShot += 1;
                if(numberOfBallsShot == numberOfBallsToShoot) {
                    State = State.WaitingForBallsToFinish;
                } else {
                    currentDeltaTime = deltaTimeBetweenShots;
                }
            }
        }

        private void UpdateAccVelPos(double deltaTime, double viewportWidth, double viewportHeight) {
            List<BallViewModel> ballsToDelete = new List<BallViewModel>();
            foreach (var bvm in BallViewModels) {
                bvm.Pos += bvm.Vel * deltaTime;
                if ( false && bvm.BottomLeft.Y > viewportHeight) {
                    ballsToDelete.Add(bvm);
                } else {
                    if (bvm.TopLeft.X < 0) {
                        // Reken terug waar de bal de kant raakte
                        Vector normalizedVel = bvm.Vel;
                        normalizedVel.Normalize();
                        double a = (bvm.Pos.X - bvm.Size.Width / 2) / normalizedVel.X;
                        bvm.Pos = new Point(bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
                        bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
                    }
                    if (bvm.TopRight.X > viewportWidth) {
                        // Reken terug waar de bal de kant raakte
                        Vector normalizedVel = bvm.Vel;
                        normalizedVel.Normalize();
                        double a = (bvm.Pos.X - viewportWidth + bvm.Size.Width / 2) / normalizedVel.X;
                        bvm.Pos = new Point(viewportWidth - bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
                        bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
                    }
                    if (bvm.TopLeft.Y < 0) {
                        // Reken terug waar de bal de kant raakte
                        Vector normalizedVel = bvm.Vel;
                        normalizedVel.Normalize();
                        double a = (bvm.Pos.Y - bvm.Size.Height / 2) / normalizedVel.Y;
                        bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, bvm.Size.Height / 2);
                        bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
                    }
                    if (bvm.BottomLeft.Y > viewportHeight) {
                        // Reken terug waar de bal de kant raakte
                        Vector normalizedVel = bvm.Vel;
                        normalizedVel.Normalize();
                        double a = (bvm.Pos.Y - viewportHeight + bvm.Size.Height / 2) / normalizedVel.Y;
                        bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, viewportHeight - bvm.Size.Height / 2);
                        bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
                    }
                }
            }
            foreach (var ballViewModel in ballsToDelete) {
                ballViewModels.Remove(ballViewModel);
            }
            if(State == State.WaitingForBallsToFinish && !BallViewModels.Any()) {
                State = State.PlacingTarget;
            }
        }

        internal void ShootAtTarget(Point point) {
            State = State.ShootingBalls;
            numberOfBallsShot = 0;
            currentDeltaTime = deltaTimeBetweenShots;
            shootingDirection = point - ShootingPoint;
            shootingDirection.Normalize();
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
