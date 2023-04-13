using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer
{
    public abstract class Movable
    {
        protected GameTable Owner;
        public (int vertical, int horizontal) Speed;
        protected RectangleF Bounds;
        protected InvertVectorF Vector = InvertVectorF.Empty;
        private (float up, float right, float down, float left) Distance;

        protected Movable(GameTable owner)
        {
            Owner = owner;
            Speed = (0, 0);
        }

        public Image Texture { get; protected set; }

        public PointF ImgLocation => Bounds.Location;
        public Point Center => Point.Round(Bounds.Center());
        public void TpTo(PointF position) => Bounds.Location = position - new SizeF(Bounds.Width / 2, Bounds.Height / 2);

        public PointF Bottom => Bounds.Location + new SizeF(Bounds.Width / 2, Bounds.Height);

        public bool Intersect(RectangleF bounds)
        {
            return Bounds.IntersectsWith(bounds);
        }
        private void UpdateDistanceToWalls(Sector sector)
        {
            float up, right, down, left;
            if (sector == null)
                (up, right, down, left) = (1, 1, 1, 1);
            else
            {
                up = sector.CanGo[Direction.Up] ? 1 : Bounds.Top - sector.WallBounds[Direction.Up].Bottom;
                right = sector.CanGo[Direction.Right] ? 1 : sector.WallBounds[Direction.Right].Left - Bounds.Right;
                down = sector.CanGo[Direction.Down] ? 1 : sector.WallBounds[Direction.Down].Top - Bounds.Bottom;
                left = sector.CanGo[Direction.Left] ? 1 : Bounds.Left - sector.WallBounds[Direction.Left].Right;
            }
            Distance = (up, right, down, left);
        }

        public void Update(object sender, EventArgs e)
        {


            var (sector, position) = Owner.GetPositionSector(Center);
            UpdateDistanceToWalls(sector);
            float wallBound = Owner.BParametrs.WallBound;
            if (Distance.right < 1 - wallBound && Vector.X > 0 || Distance.left < 1 - wallBound && Vector.X < 0)
                Vector.X = 0;
            if (Distance.up < 1 - wallBound && Vector.Y > 0 || Distance.down < 1 - wallBound && Vector.Y < 0)
                Vector.Y = 0;
            Bounds.Location += Vector;
            InvertVectorF ejection = new InvertVectorF();
            if (Distance.up < -wallBound)
                ejection.Y = Distance.up + wallBound;
            else if (Distance.down < -wallBound)
                ejection.Y = -Distance.down - wallBound;
            if (Distance.right < -wallBound)
                ejection.X = Distance.right + wallBound;
            else if (Distance.left < -wallBound)
                ejection.X = -Distance.left - wallBound;
            Bounds.Location += ejection;
            if (sector != null)
                if (sector.CornerBounds[Direction.Up].ToArray().Any(B => B.Contains(Center)))
                {
                    Bounds.Location += new InvertVectorF() { X = 0, Y = -Bounds.Height / 2 };
                    Vector.Y = 0;
                }
                else if (sector.CornerBounds[Direction.Down].ToArray().Any(B => B.Contains(Center)))
                {
                    Bounds.Location += new InvertVectorF() { X = 0, Y = Bounds.Height / 2 };
                    Vector.Y = 0;
                }
        }

        public abstract void Render();

        public string Debug(GameTable game)
        {
            Point position = game.GetPositionSector(Center).position;
            var (up, right, down, left) = Distance;
            return $"Sector {position}\nSpeed H={Vector.X}, V={Vector.Y}\nUp {up}\nDown {down}\nLeft {left}\nRight {right}";
        }
    }

    public interface IDamagable
    {
        public int MaxHealth { get; }
        public int Health { get; }

        public void ApllyDamage(int damage);
        public void Dead();
    }

    public class Player : Movable, IDamagable
    {
        public int MaxHealth { get; }
        public int Health { get; private set; }

        public Player(GameTable owner, PlayerArgs args) : base(owner)
        {
            MaxHealth = args.MaxHealth;
        }
        public override void Render()
        {
            float scale = Owner.SectorScaleValue;
            Bounds = new RectangleF(new PointF(0, 0), new SizeF(scale / 40 * 11, scale / 4 * 3));
            //int ImgSectorSize = 200;
            //Size ImgSize = new Size(ImgSectorSize / 40 * 11, ImgSectorSize / 4 * 3);
            //Texture = new Bitmap(Properties.Resources.StandartPlayer, Size.Truncate(Bounds.Size));
            //Images.GetFragment(Properties.Resources.StandartPlayer, ImgSize, new Rectangle(new Point(0, 0), ImgSize), Bounds.Size);
        }

        public void ApllyDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Dead();
            }
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

    public struct InvertVectorF
    {
        public static InvertVectorF Empty => new InvertVectorF();

        public float X;
        public float Y;

        public static PointF operator +(PointF p, InvertVectorF v) => new PointF(p.X + v.X, p.Y - v.Y);

        public override string ToString() => $"X = {X}, Y = {Y}";
    }
}