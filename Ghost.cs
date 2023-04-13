using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace Pacman
{
    public abstract class Ghost : Movable
    {
        public enum Behaviour
        {
            Wander,
            GetOut,
            Chase,
            Scatter,
            Frightened
        }

        private Point GoalSector;
        protected Point EscapeGoal;

        private Behaviour _currentBehaviour;
        protected Behaviour CurrentBehaviour
        {
            get => _currentBehaviour;
            set
            {
                _currentBehaviour = value;
                IgnorDirection = true;
            }
        }
        private Behaviour StartBehaviour;
        public bool IgnorDirection = false;

        private Color Color;

        public Ghost(GameTable owner, Point escapeGoal, Color color, Behaviour startBehaviour) : base(owner)
        {
            Speed.MaxValue = Owner.PParametrs.GhostSpeed;
            EscapeGoal = escapeGoal;
            Color = color;
            Bounds = new RectangleF(new PointF(0, 0), new SizeF(Owner.SectorScaleValue * 3 / 4, Owner.SectorScaleValue * 3 / 4));
            StartBehaviour = startBehaviour;
            Owner.UpdateEvent += (sender, e) =>
            {
                CheckPosition();
                GoalSector = SetGoal((Owner.GetPositionSector(Owner.Player.Center).position, Owner.Player.Speed.Direction));
            };
            SectorChanged += (sender, e) => SetDirection();
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            //Pen pen = Pens.Brown;
            //if (this is Blinky)
            //    pen = Pens.Red;
            //else if (this is Pinky)
            //    pen = Pens.Pink;
            //else if (this is Inky)
            //    pen = Pens.Blue;
            //else if (this is Clyde)
            //    pen = Pens.Orange;
            //g.DrawEllipse(pen, Owner[GoalSector]?.Bounds??RectangleF.Empty);
        }
        public override void Render()
        {
            Texture = Images.Ghost(Owner.SectorScaleValue * 3 / 4, Color);
        }

        public void Restart()
        {
            Speed.MaxValue = Owner.PParametrs.GhostSpeed;
            CurrentBehaviour = StartBehaviour;
        }

        private void CheckPosition()
        {
            var position = Owner.GetPositionSector(Center).position;
            if (CurrentBehaviour == Behaviour.GetOut && position == Owner.GhostHome.Location + Owner.GhostHome.Exit.ToSizeOrEmpty())
                CurrentBehaviour = Behaviour.Chase;
        }
        public bool GoHome(Direction direction)
        {
            var position = Owner.GetPositionSector(Center).position;
            return (position == Owner.GhostHome.Location || position == Owner.GhostHome.Location + Owner.GhostHome.Exit.ToSizeOrEmpty()) &&
                   (direction == Owner.GhostHome.Exit &&
                    CurrentBehaviour == Behaviour.GetOut) || (direction == Owner.GhostHome.Exit.Inverse() &&
                                                              CurrentBehaviour == Behaviour.Frightened);
        }
        protected override void BlockSpeed(float wallBound)
        {
            var position = Owner.GetPositionSector(Center).position;
            if (position == Owner.GhostHome.Location && Speed.Direction == Owner.GhostHome.Exit && CurrentBehaviour == Behaviour.GetOut)
                return;
            if (position == Owner.GhostHome.Location + Owner.GhostHome.Exit.ToSizeOrEmpty() && Speed.Direction == Owner.GhostHome.Exit.Inverse() && CurrentBehaviour == Behaviour.Frightened)
                return;
            base.BlockSpeed(wallBound);
        }

        public abstract Point SetGoal((Point position, Direction direction) player);
        private void SetDirection()
        {
            var (sector, position) = Owner.GetPositionSector(Center);
            var directions = sector.CanGo.Where(G => G.Value && (IgnorDirection || G.Key != Speed.Direction.Inverse()));
            if (CurrentBehaviour != Behaviour.GetOut)
                IgnorDirection = false;
            Size goal = Size.Empty;
            switch (CurrentBehaviour)
            {
                case Behaviour.Wander:
                    if (!sector.CanGo[Direction.Left])
                        ChangeDirection(Direction.Right);
                    if (!sector.CanGo[Direction.Right])
                        ChangeDirection(Direction.Left);
                    return;
                case Behaviour.GetOut:
                    goal = (Size)Owner[Owner.GhostHome.Location].NeighborSector[Owner.GhostHome.Exit].TblPosition;
                    if (position == Owner.GhostHome.Location)
                        directions = directions.Append(new KeyValuePair<Direction, bool>(Owner.GhostHome.Exit, true));
                    break;
                case Behaviour.Chase:
                    goal = (Size)GoalSector;
                    break;
                case Behaviour.Scatter:
                    goal = (Size)EscapeGoal;
                    break;
                case Behaviour.Frightened:
                    goal = (Size)Owner[Owner.GhostHome.Location].TblPosition;
                    break;
            }
            if (directions.Any())
            {
                var lengths = directions.Select(G => (length: ((Size)((sector.NeighborSector[G.Key]?.TblPosition - goal) ?? new Point(100, 100))).Length(), direction: G.Key)).ToList();
                double min = lengths.Min(L => L.length);
                ChangeDirection(lengths.Find(V => V.length == min).direction);
            }
        }
    }
}
