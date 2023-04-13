using System;
using System.Drawing;

namespace Pacman
{
    public class Player : Movable, IDamagable
    {
        public int MaxHealth { get; }
        public int Health { get; private set; }

        public Player(GameTable owner, PlayerArgs args) : base(owner)
        {
            MaxHealth = args.MaxHealth;
            Speed.MaxValue = Owner.PParametrs.PacmanSpeed;
            Bounds = new RectangleF(new PointF(0, 0), new SizeF(Owner.SectorScaleValue * 11 / 40, Owner.SectorScaleValue * 3 / 4));
        }
        public override void Render()
        {
            //int ImgSectorSize = 200;
            //Size ImgSize = new Size(ImgSectorSize / 40 * 11, ImgSectorSize / 4 * 3);
            Texture = Images.Pacman(Owner.SectorScaleValue * 3 / 4, Speed.Direction);
            //Images.GetFragment(Properties.Resources.StandartPlayer, ImgSize, new Rectangle(new Point(0, 0), ImgSize), Bounds.Size);
        }

        public void ApllyDamage(int damage)
        {
            Health--;
            if (Health <= 0)
                Dead();
        }

        public void Dead()
        {
            throw new NotImplementedException();
        }

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