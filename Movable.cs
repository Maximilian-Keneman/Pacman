using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
    public abstract class Movable
    {
        protected GameTable Owner;
        public (InvertVectorF Value, int MaxValue, Direction Direction) Speed;
        protected RectangleF Bounds;
        private (float up, float right, float down, float left) Distance;

        protected Movable(GameTable owner)
        {
            Owner = owner;
            Speed = (new(0, 0), 0, Direction.None);
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
            switch (Speed.Direction)
            {
                case Direction.None:
                    Speed.Value = new();
                    break;
                case Direction.Up:
                    Speed.Value = new(0, Speed.MaxValue);
                    break;
                case Direction.Right:
                    Speed.Value = new(Speed.MaxValue, 0);
                    break;
                case Direction.Down:
                    Speed.Value = new(0, -Speed.MaxValue);
                    break;
                case Direction.Left:
                    Speed.Value = new(-Speed.MaxValue, 0);
                    break;
            }
            var (sector, _) = Owner.GetPositionSector(Center);
            UpdateDistanceToWalls(sector);
            float wallBound = Owner.BParametrs.WallBound;
            if (Distance.right < 1 - wallBound && Speed.Value.X > 0 || Distance.left < 1 - wallBound && Speed.Value.X < 0)
                Speed.Value.X = 0;
            if (Distance.up < 1 - wallBound && Speed.Value.Y > 0 || Distance.down < 1 - wallBound && Speed.Value.Y < 0)
                Speed.Value.Y = 0;
            Bounds.Location += Speed.Value;
            InvertVectorF ejection = new();
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
                if (sector.CornerBounds[Direction.Up].right.Contains(Center))
                    Bounds.Location += new SizeF() { Width = -Bounds.Width / 2, Height = -Bounds.Height / 2 };
                else if (sector.CornerBounds[Direction.Up].left.Contains(Center))
                    Bounds.Location += new SizeF() { Width = Bounds.Width / 2, Height = -Bounds.Height / 2 };
                else if (sector.CornerBounds[Direction.Down].right.Contains(Center))
                    Bounds.Location += new SizeF() { Width = -Bounds.Width / 2, Height = Bounds.Height / 2 };
                else if (sector.CornerBounds[Direction.Down].left.Contains(Center))
                    Bounds.Location += new SizeF() { Width = Bounds.Width / 2, Height = Bounds.Height / 2 };
        }

        public abstract void Render();

        public string Debug(GameTable game)
        {
            Point position = game.GetPositionSector(Center).position;
            var (up, right, down, left) = Distance;
            return $"Sector {position}\nSpeed V={Speed.Value}\nUp {up}\nDown {down}\nLeft {left}\nRight {right}";
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

    public struct InvertVectorF
    {
        public static InvertVectorF Empty => new();

        public float X;
        public float Y;

        public InvertVectorF(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static PointF operator +(PointF p, InvertVectorF v) => new(p.X + v.X, p.Y - v.Y);

        public override string ToString() => $"X = {X}, Y = {Y}";
    }
}