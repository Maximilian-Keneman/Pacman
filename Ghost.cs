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
        public bool IsUnderAttack { get; set; } = false;
        private bool _isDead = false;
        public bool IsDead
        {
            get => _isDead;
            private set
            {
                _isDead = value;
                Speed.MaxValue = value ? Owner.PParametrs.EyesSpeed : Owner.PParametrs.GhostSpeed;
            }
        }

        public Ghost(GameTable owner, Point escapeGoal,
            Behaviour startBehaviour, (int timer, Behaviour newBehaviour)[] events) : base(owner)
        {
            Speed.MaxValue = Owner.PParametrs.GhostSpeed;
            EscapeGoal = escapeGoal;
            Bounds = new RectangleF(new PointF(0, 0), new SizeF(Owner.SectorScaleValue * 3 / 4, Owner.SectorScaleValue * 3 / 4));
            StartBehaviour = startBehaviour;
            UnderAttackAnimations = new()
            {
                {
                    Direction.None, new Image[1]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 270), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Up, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 270), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 314), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Right, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 6), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 50), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Down, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 94), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 138), new(45, 49)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Left, new Image[2]
                    {
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 182), new(45, 49)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Ghosts, new(new(183, 226), new(45, 49)), Bounds.Size.ToSize())
                    }
                }
            };
            Eyes = new()
            {
                {
                    Direction.None,
                    Images.GetFragment(Properties.Resources.Ghosts, new(new(228, 138), new(45, 49)), Bounds.Size.ToSize())
                },
                {
                    Direction.Up,
                    Images.GetFragment(Properties.Resources.Ghosts, new(new(228, 138), new(45, 49)), Bounds.Size.ToSize())
                },
                {
                    Direction.Right,
                    Images.GetFragment(Properties.Resources.Ghosts, new(new(228, 6), new(45, 49)), Bounds.Size.ToSize())
                },
                {
                    Direction.Down,
                    Images.GetFragment(Properties.Resources.Ghosts, new(new(228, 50), new(45, 49)), Bounds.Size.ToSize())
                },
                {
                    Direction.Left,
                    Images.GetFragment(Properties.Resources.Ghosts, new(new(228, 94), new(45, 49)), Bounds.Size.ToSize())
                }
            };
            WalkAnimator = new(GetWalkAnimations, () => (IsUnderAttack ? UnderAttackAnimations : Animations)[Speed.Direction].Last());
            WalkAnimator.Condition = true;
            Owner.PaintEvent += (sender, e) => Texture = WalkAnimator.Animate();
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

        private Animator WalkAnimator;
        protected Dictionary<Direction, Image[]> Animations;
        private Dictionary<Direction, Image[]> UnderAttackAnimations;
        private Dictionary<Direction, Image> Eyes;
        private IEnumerator<Image> GetWalkAnimations()
        {
            while (true)
            {
                if (IsDead)
                    yield return Eyes[Speed.Direction];
                else
                    for (int i = 0; i < (IsUnderAttack ? UnderAttackAnimations : Animations)[Speed.Direction].Length; i++)
                        yield return (IsUnderAttack ? UnderAttackAnimations : Animations)[Speed.Direction][i];
            }
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
                    {
                        Owner.Player.IsEat = true;
                        ApllyDamage(1);
                    }
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

        public void ApllyDamage(int damage)
        {
            Dead();
            Owner.Player.Score += 50;
        }

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
