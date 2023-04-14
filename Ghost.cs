using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
    public abstract class Ghost : Movable, IDamagable
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

        public int MaxHealth => 0;
        public int Health => 0;
        private bool _isUnderAttack = false;
        public bool IsUnderAttack
        {
            get => _isUnderAttack;
            set
            {
                _isUnderAttack = value;
                Render();
            }
        }
        private bool _isDead = false;
        public bool IsDead
        {
            get => _isDead;
            private set
            {
                _isDead = value;
                Render();
                Speed.MaxValue = value ? Owner.PParametrs.EyesSpeed : Owner.PParametrs.GhostSpeed;
            }
        }

        private Color Color;

        public Ghost(GameTable owner, Point escapeGoal, Color color,
            Behaviour startBehaviour, (int timer, Behaviour newBehaviour)[] events) : base(owner)
        {
            Speed.MaxValue = Owner.PParametrs.GhostSpeed;
            EscapeGoal = escapeGoal;
            Color = color;
            Bounds = new RectangleF(new PointF(0, 0), new SizeF(Owner.SectorScaleValue * 3 / 4, Owner.SectorScaleValue * 3 / 4));
            StartBehaviour = startBehaviour;
            Owner.UpdateEvent += (sender, e) =>
            {
                Elapsed();
                CheckPosition();
                GoalSector = SetGoal((Owner.GetPositionSector(Owner.Player.Center).position, Owner.Player.Speed.Direction));
            };
            SectorChanged += (sender, e) => SetDirection();
            Events = events;
            Counter = Events[0].timer;
        }

        public override void Draw(Graphics g, DebugMode debugMode)
        {
            base.Draw(g, debugMode);
            if ((debugMode as PacmanDebugMode).GhostGoal)
            {
                Pen pen = this switch
                {
                    Blinky => Pens.Red,
                    Pinky => Pens.Pink,
                    Inky => Pens.Blue,
                    Clyde => Pens.Orange,
                    _ => Pens.Brown,
                };
                g.DrawEllipse(pen, Owner[GoalSector]?.Bounds??RectangleF.Empty);
            }
        }
        public override void Render()
        {
            Texture = IsDead ? Images.Eyes(Owner.SectorScaleValue * 3 / 8) :
                               Images.Ghost(Owner.SectorScaleValue * 3 / 4, IsUnderAttack ? Color.DarkBlue : Color);
        }

        public void Restart()
        {
            IsDead = false;
            IsUnderAttack = false;
            Speed.MaxValue = Owner.PParametrs.GhostSpeed;
            CurrentBehaviour = StartBehaviour;
            Counter = Events[0].timer;
            EventIndex = 0;
        }

        private (int timer, Behaviour newBehaviour)[] Events;
        private int EventIndex = 0;
        private int Counter = 1;

        private void Elapsed()
        {
            if ((CurrentBehaviour == Behaviour.Wander ||
                 CurrentBehaviour == Behaviour.Chase ||
                 CurrentBehaviour == Behaviour.Scatter) &&
                 EventIndex < Events.Length)
            {
                if (Counter >= 0)
                    Counter--;
                if (Counter == 0)
                {
                    Counter = Events[EventIndex].timer;
                    CurrentBehaviour = Events[EventIndex].newBehaviour;
                    EventIndex++;
                }
            }
        }

        private void CheckPosition()
        {
            var position = Owner.GetPositionSector(Center).position;
            if (!IsDead)
            {
                if (position == Owner.GetPositionSector(Owner.Player.Center).position)
                    if (IsUnderAttack)
                        ApllyDamage(1);
                    else
                        Owner.Player.ApllyDamage(1);
                if (CurrentBehaviour == Behaviour.GetOut && position == Owner.GhostHome.Location + Owner.GhostHome.Exit.ToSizeOrEmpty())
                    CurrentBehaviour = Behaviour.Chase;
            }
            else
            {
                if (CurrentBehaviour == Behaviour.Frightened && position == Owner.GhostHome.Location)
                    Alive();
            }
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
                    if (position == Owner.GhostHome.Location + Owner.GhostHome.Exit.ToSize())
                        directions = directions.Append(new KeyValuePair<Direction, bool>(Owner.GhostHome.Exit.Inverse(), true));
                    break;
            }
            if (directions.Any())
            {
                var lengths = directions.Select(G => (length: ((Size)((sector.NeighborSector[G.Key]?.TblPosition - goal) ?? new Point(100, 100))).Length(), direction: G.Key)).ToList();
                double min = lengths.Min(L => L.length);
                ChangeDirection(lengths.Find(V => V.length == min).direction);
            }
        }

        public void ApllyDamage(int damage) => Dead();
        public void Dead()
        {
            IsDead = true;
            CurrentBehaviour = Behaviour.Frightened;
        }
        public void Alive()
        {
            IsDead = false;
            IsUnderAttack = false;
            CurrentBehaviour = Behaviour.GetOut;
        }
    }
}
