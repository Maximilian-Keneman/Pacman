using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pacman
{
    public class Player : Movable, IDamagable
    {
        public int MaxHealth { get; }
        public int Health { get; private set; }
        public int Score;

        private bool _energetic = false;
        public bool Energetic
        {
            get => _energetic;
            set
            {
                _energetic = value;
                Owner.Ghosts.Act(G => G.IsUnderAttack = value);
                if (value)
                    Counter = 50;
            }
        }

        public Player(GameTable owner, PlayerArgs args) : base(owner)
        {
            Health = MaxHealth = args.MaxHealth;
            Speed.MaxValue = Owner.PParametrs.PacmanSpeed;
            Bounds = new RectangleF(new PointF(0, 0), new SizeF(Owner.SectorScaleValue * 3 / 4, Owner.SectorScaleValue * 3 / 4));
            Animations = new()
            {
                {
                    Direction.None, new Image[1]
                    {
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 401), new(43, 44)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Up, new Image[4]
                    {
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 445), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 401), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 445), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 489), new(43, 44)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Right, new Image[4]
                    {
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 49), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 5), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 49), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 93), new(43, 44)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Down, new Image[4]
                    {
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 181), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 137), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 181), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 225), new(43, 44)), Bounds.Size.ToSize())
                    }
                },
                {
                    Direction.Left, new Image[4]
                    {
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 313), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 269), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 313), new(43, 44)), Bounds.Size.ToSize()),
                        Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(4, 357), new(43, 44)), Bounds.Size.ToSize())
                    }
                }
            };
            DeadAnimation = new Image[11]
            {
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 5), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 49), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 93), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 137), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 181), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 225), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 269), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 313), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 357), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 401), new(43, 44)), Bounds.Size.ToSize()),
                Images.GetFragment(Properties.Resources.Pacman, new Rectangle(new(49, 445), new(43, 44)), Bounds.Size.ToSize())
            };
            Texture = Animations[Direction.None].Last();
            EatAnimator = new(GetEatAnimations, () => Animations[Speed.Direction].Last());
            DeadAnimator = new(GetDeadAnimation, () => Animations[Direction.None].Last());
            DeadAnimator.AnimationEnd += (sender, e) =>
            {
                if (Health <= 0)
                    Dead();
                else
                {
                    Owner.PlayerToStart();
                    Owner.GameContinue();
                }
            };
            Owner.PaintEvent += (sender, e) => Animate();
            Owner.UpdateEvent += (sender, e) =>
            {
                Elapsed();
                CheckCoins();
            };
        }

        public bool IsEat
        {
            get => EatAnimator.Condition;
            set => EatAnimator.Condition = value;
        }
        private Animator EatAnimator;
        private Dictionary<Direction, Image[]> Animations;
        private IEnumerator<Image> GetEatAnimations()
        {
            for (int i = 0; i < Animations[Speed.Direction].Length; i++)
                yield return Animations[Speed.Direction][i];
            yield break;
        }
        private Animator DeadAnimator;
        private Image[] DeadAnimation;
        private IEnumerator<Image> GetDeadAnimation()
        {
            for (int i = 0; i < DeadAnimation.Length; i++)
                yield return DeadAnimation[i];
            yield break;
        }
        private void Animate()
        {
            Texture = (IsDead ? DeadAnimator : EatAnimator).Animate();
        }

        private int Counter = 0;
        private void Elapsed()
        {
            if (Energetic)
            {
                if (Counter > 0)
                    Counter--;
                if (Counter == 0)
                    Energetic = false;
            }
        }

        private void CheckCoins()
        {
            Sector sector = Owner.GetPositionSector(Center).sector;
            CheckSector(sector);
            CheckSector(sector?.NeighborSector[Speed.Direction]);

            void CheckSector(Sector sector)
            {
                if (sector == null)
                    return;
                for (int i = 0; i < sector.Coins.Length; i++)
                {
                    if (!sector.Coins[i].geted && Intersect(sector.Coins[i].actBounds))
                    {
                        IsEat = true;
                        sector.Coins[i].geted = true;
                        if (sector.Coins[i].energetic)
                            Energetic = true;
                        else
                        {
                            Score++;
                            Owner.CheckFinish();
                        }
                    }
                }
            }
        }

        public bool IsDead
        {
            get => DeadAnimator.Condition;
            set => DeadAnimator.Condition = value;
        }
        public void ApllyDamage(int damage)
        {
            Owner.GamePause();
            Health--;
            IsDead = true;
        }
        public void Dead()
        {
            Owner.GameEnd();
        }

        public override string Debug() => $"Score {Score}\n{base.Debug()}\nLive {Health}";

        public struct PlayerArgs
        {
            public int MaxHealth { get; }

            public PlayerArgs(int maxHealth)
            {
                MaxHealth = maxHealth;
            }
        }
    }
}