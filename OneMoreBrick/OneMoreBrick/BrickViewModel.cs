using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OneMoreBrick {
    class BrickViewModel : ViewModelBase {

        public BrickViewModel() {

        }

        private Point pos;
        public Point Pos {
            get { return pos; }
            set {
                if (SetProperty(value, ref pos, () => Pos)) {
                    OnPosChanged();
                    NotifyPropertyChanged(() => TopLeftPos);
                }
            }
        }

        private Size size = new Size(30, 30);
        public Size Size {
            get {
                return size;
            }
        }

        public Point TopLeftPos { get { return TopLeft; } }

        public Point TopLeft { get; private set; }
        public Point TopRight { get; private set; }
        public Point BottomRight { get; private set; }
        public Point BottomLeft { get; private set; }

        protected virtual void OnPosChanged() {
            UpdateProjectedCornerPositions();
        }

        private void UpdateProjectedCornerPositions() {
            var left = Pos.X - Size.Width / 2;
            var right = Pos.X + Size.Width / 2;
            var top = Pos.Y - Size.Height / 2;
            var bottom = Pos.Y + Size.Height / 2;

            TopLeft = new Point(left, top);
            TopRight = new Point(right, top);
            BottomRight = new Point(right, bottom);
            BottomLeft = new Point(left, bottom);
        }
    }
}
