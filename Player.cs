using System;
using System.Drawing;

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
            Owner.UpdateEvent += (sender, e) =>
            {
                Elapsed();
                CheckCoins();
            };
        }
        public override void Render()
        {
            //int ImgSectorSize = 200;
            //Size ImgSize = new Size(ImgSectorSize / 40 * 11, ImgSectorSize / 4 * 3);
            Texture = Images.Pacman(Owner.SectorScaleValue * 3 / 4, Speed.Direction);
            //Images.GetFragment(Properties.Resources.StandartPlayer, ImgSize, new Rectangle(new Point(0, 0), ImgSize), Bounds.Size);
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
                    if (!sector.Coins[i].geted && Intersect(sector.Coins[i].bounds))
                    {
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

        public void ApllyDamage(int damage)
        {
            Health--;
            Owner.PlayerToStart();
            if (Health <= 0)
                Dead();
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