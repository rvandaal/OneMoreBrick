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

        private int numberOfBallsToShoot = 30;
        private int numberOfBallsShot;
        private double currentDeltaTime;
        private double deltaTimeBetweenShots = 0.05;

        private Vector shootingDirection;
        private double shootingVel = 400;

        Size viewportSize;

        double ballDiameter = 10;

        public GameViewModel() {
            ballViewModels = new ObservableCollection<BallViewModel>();
            BallViewModels = new ReadOnlyObservableCollection<BallViewModel>(ballViewModels);
            brickViewModels = new ObservableCollection<BrickViewModel>();
            BrickViewModels = new ReadOnlyObservableCollection<BrickViewModel>(brickViewModels);
        }

        public void SetViewportSize(Size s) {
            viewportSize = s;
            StartNewGame();
        }

        private void StartNewGame() {
            ballViewModels.Clear();
            State = State.PlacingTarget;
            ShootingPoint = new Point(viewportSize.Width / 2, viewportSize.Height - ballDiameter / 2 - 50);
            AddBrickViewModel(new BrickViewModel() { Pos = new Point(120, 120) });
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

        #region BrickViewModels

        public ReadOnlyObservableCollection<BrickViewModel> BrickViewModels { get; private set; }
        private readonly ObservableCollection<BrickViewModel> brickViewModels;

        public void AddBrickViewModel(BrickViewModel brickViewModel) {
            if (!brickViewModels.Contains(brickViewModel)) {
                brickViewModels.Add(brickViewModel);
            }
        }

        public void RemoveBrickViewModel(BrickViewModel brickViewModel) {
            if (brickViewModels.Contains(brickViewModel)) {
                brickViewModels.Remove(brickViewModel);
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
                UpdateAccVelPos(dt, 0, 0, viewportWidth, viewportHeight);
                if (State == State.ShootingBalls) {
                    ShootBalls(dt);
                }
            }
        }

        private void ShootBalls(double dt) {
            if (currentDeltaTime > 0) {
                currentDeltaTime -= dt;
            } else {
                var ballViewModel = new BallViewModel(ShootingPoint, shootingDirection * shootingVel) { Size = new Size(ballDiameter, ballDiameter) };
                AddBallViewModel(ballViewModel);
                numberOfBallsShot += 1;
                if (numberOfBallsShot == numberOfBallsToShoot) {
                    State = State.WaitingForBallsToFinish;
                } else {
                    currentDeltaTime = deltaTimeBetweenShots;
                }
            }
        }

        private void UpdateAccVelPos(double deltaTime, double x0, double y0, double x1, double y1) {
            List<BallViewModel> ballsToDelete = new List<BallViewModel>();
            foreach (var bvm in BallViewModels) {
                bvm.Pos += bvm.Vel * deltaTime;

                if(!CheckCollisions(bvm, deltaTime)) {
                    ballsToDelete.Add(bvm);
                }
            }

            foreach (var ballViewModel in ballsToDelete) {
                ballViewModels.Remove(ballViewModel);
            }

            if (State == State.WaitingForBallsToFinish && !BallViewModels.Any()) {
                State = State.PlacingTarget;
            }
        }

        private bool CheckCollisions(BallViewModel bvm, double deltaTime) {
            if(!CheckCollisionWithCanvas(bvm, 0, 0, viewportSize.Width, viewportSize.Height)) {
                return false;
            }
            foreach (var brickViewModel in BrickViewModels) {
                CheckCollisionWithBrick(bvm, brickViewModel);
            }
            return true;
        }

        private bool CheckCollisionWithCanvas(BallViewModel bvm, double x0, double y0, double x1, double y1) {
            
            if (bvm.BottomLeft.Y > y1) {
                //return false;
            }
            Vector normalizedVel = bvm.Vel;
            normalizedVel.Normalize();

            if (bvm.TopLeft.X < x0) {
                // Reken terug waar de bal de kant raakte                        
                double a = (bvm.Pos.X - x0 - bvm.Size.Width / 2) / normalizedVel.X;
                bvm.Pos = new Point(x0 + bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
                bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
            } else if (bvm.TopRight.X > x1) {
                // Reken terug waar de bal de kant raakte
                double a = (bvm.Pos.X - x1 + bvm.Size.Width / 2) / normalizedVel.X;
                bvm.Pos = new Point(x1 - bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
                bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
            } else if (bvm.TopLeft.Y < y0) {
                // Reken terug waar de bal de kant raakte
                double a = (bvm.Pos.Y - y0 - bvm.Size.Height / 2) / normalizedVel.Y;
                bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, y0 + bvm.Size.Height / 2);
                bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
            } else if (bvm.BottomLeft.Y > y1) {
                // Reken terug waar de bal de kant raakte
                double a = (bvm.Pos.Y - y1 + bvm.Size.Height / 2) / normalizedVel.Y;
                bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, y1 - bvm.Size.Height / 2);
                bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
            }
            return true;
        }

        private void CheckCollisionWithBrick(BallViewModel bvm, BrickViewModel brickViewModel) {
            Vector normalizedVel = bvm.Vel;
            normalizedVel.Normalize();

            var xl = brickViewModel.TopLeft.X;
            var xr = brickViewModel.TopRight.X;
            var yt = brickViewModel.TopLeft.Y;
            var yb = brickViewModel.BottomLeft.Y;

            bool hasOverlapWithBrick = false;

            // cache waarde onderste rand bricks zit. Daaronder hoef je alleen maar de Y te checken

            // TODO: nieuwe pos kan buiten brick zitten als deltaTime te groot is. Kijk naar oude en nieuwe pos en bereken het snijpunt met de brick.
            hasOverlapWithBrick = bvm.TopRight.X >= xl && bvm.TopLeft.X <= xr && bvm.BottomLeft.Y >= yt && bvm.TopLeft.Y <= yb;

            if(!hasOverlapWithBrick) {
                return;
            }

            // Nu zou 1 van de 4 cases hieronder true moeten zijn, maar toch zie ik gebeuren dat dit niet zo is.

            var x = bvm.Pos.X;
            var y = bvm.Pos.Y;
            var vx = normalizedVel.X;
            var vy = normalizedVel.Y;

            var aleft = (x - xl) / vx;
            var yleft = y - aleft * vy;
            if(aleft >= -bvm.Size.Width / 2 && yt <= yleft && yleft <= yb) {
                // bal is aan linkerkant erin gegaan
                double a = (bvm.Pos.X - xl + bvm.Size.Width / 2) / normalizedVel.X;
                bvm.Pos = new Point(xl - bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
                bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
                return;
            }

            var aright = (x - xr) / vx;
            var yright = y - aright * vy;
            if (aright >= -bvm.Size.Width / 2 && yt <= yright && yright <= yb) {
                // bal is aan rechterkant erin gegaan
                double a = (bvm.Pos.X - xr - bvm.Size.Width / 2) / normalizedVel.X;
                bvm.Pos = new Point(xr + bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
                bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
                return;
            }

            var atop = (y - yt) / vy;
            var xtop = x - atop * vx;
            if (atop >= -bvm.Size.Width / 2 && xl <= xtop && xtop <= xr) {
                // bal is aan bovenkant erin gegaan
                double a = (bvm.Pos.Y - yt + bvm.Size.Height / 2) / normalizedVel.Y;
                bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, yt - bvm.Size.Height / 2);
                bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
                return;
            }

            // Let op: abottom en xbottom worden alleen maar gebruikt om te kijken of de shooting line kruist met de onderste line van de brick.
            // De size van de ball speelt hier dus geen rol.
            var abottom = (y - yb) / vy;
            var xbottom = x - abottom * vx;
            if (abottom >= -bvm.Size.Width/2 && xl <= xbottom && xbottom <= xr) {
                // bal is aan onderkant erin gegaan
                // Nu speelt de ball size wél een rol
                double a = (bvm.Pos.Y - yb - bvm.Size.Height / 2) / normalizedVel.Y;
                bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, yb + bvm.Size.Height / 2);
                bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
                return;
            }

            //// if isInBrick, bereken de hoeken van alle hoekpunten naar de huidige positie
            //var angleTopLeft = Vector.AngleBetween(bvm.Pos - brickViewModel.TopLeft, normalizedVel);
            //var angleTopRight = Vector.AngleBetween(bvm.Pos - brickViewModel.TopRight, normalizedVel);
            //var angleBottomRight = Vector.AngleBetween(bvm.Pos - brickViewModel.BottomRight, normalizedVel);
            //var angleBottomLeft = Vector.AngleBetween(bvm.Pos - brickViewModel.BottomLeft, normalizedVel);

            //// als je van de onderkant nadert ben je links van xr en inside brick. Dit klopt dus niet.

            //if (isInBrick && bvm.TopLeft.X < xr) {
            //    // Reken terug waar de bal de kant raakte                        
            //    double a = (bvm.Pos.X - xr - bvm.Size.Width / 2) / normalizedVel.X;
            //    bvm.Pos = new Point(xr + bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
            //    bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
            //} else if (isInBrick && bvm.TopLeft.X > xl) {
            //    // Reken terug waar de bal de kant raakte
            //    double a = (bvm.Pos.X - xl + bvm.Size.Width / 2) / normalizedVel.X;
            //    bvm.Pos = new Point(xl - bvm.Size.Width / 2, bvm.Pos.Y - a * normalizedVel.Y);
            //    bvm.Vel = new Vector(-bvm.Vel.X, bvm.Vel.Y);
            //} else if (isInBrick && bvm.TopLeft.Y < yb) {
            //    // Reken terug waar de bal de kant raakte
            //    double a = (bvm.Pos.Y - yb - bvm.Size.Height / 2) / normalizedVel.Y;
            //    bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, yb + bvm.Size.Height / 2);
            //    bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
            //} else if (isInBrick && bvm.TopLeft.Y > yt) {
            //    // Reken terug waar de bal de kant raakte
            //    double a = (bvm.Pos.Y - yt + bvm.Size.Height / 2) / normalizedVel.Y;
            //    bvm.Pos = new Point(bvm.Pos.X - a * normalizedVel.X, yt - bvm.Size.Height / 2);
            //    bvm.Vel = new Vector(bvm.Vel.X, -bvm.Vel.Y);
            //}
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
