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
        private Sector LastSector = null;

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
        private Direction SavedDirection = Direction.None;
        public void ChangeDirection(Direction direction)
        {
            if (direction == Direction.None || direction == Speed.Direction)
                return;
            var sector = Owner.GetPositionSector(Center).sector;
            if (sector.CanGo[direction] &&
                PointF.Subtract(sector.Bounds.Center(), (Size)Center).ToSize().Length() < Owner.SectorScaleValue / 8)
            {
                Speed.Direction = direction;
                Render();
                SavedDirection = Direction.None;
            }
            else
                SavedDirection = direction;
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
            if (Distance.right < 1 - wallBound && Speed.Direction == Direction.Right || Distance.left < 1 - wallBound && Speed.Direction == Direction.Left)
                Speed.Value.X = 0;
            if (Distance.up < 1 - wallBound && Speed.Direction == Direction.Up || Distance.down < 1 - wallBound && Speed.Direction == Direction.Down)
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
                    Bounds.Location += new SizeF(-Bounds.Width / 2, -Bounds.Height / 2);
                else if (sector.CornerBounds[Direction.Up].left.Contains(Center))
                    Bounds.Location += new SizeF(Bounds.Width / 2, -Bounds.Height / 2);
                else if (sector.CornerBounds[Direction.Down].right.Contains(Center))
                    Bounds.Location += new SizeF(-Bounds.Width / 2, Bounds.Height / 2);
                else if (sector.CornerBounds[Direction.Down].left.Contains(Center))
                    Bounds.Location += new SizeF(Bounds.Width / 2, Bounds.Height / 2);
            if (sector == null)
            {
                switch (LastSector.VoidPassage)
                {
                    case Direction.Up:
                        Bounds.Location += new SizeF(0, Owner.ImgSize.Height);
                        break;
                    case Direction.Right:
                        Bounds.Location -= new SizeF(Owner.ImgSize.Width, 0);
                        break;
                    case Direction.Down:
                        Bounds.Location -= new SizeF(0, Owner.ImgSize.Height);
                        break;
                    case Direction.Left:
                        Bounds.Location += new SizeF(Owner.ImgSize.Width, 0);
                        break;
                }
                sector = Owner.GetPositionSector(Center).sector;
                UpdateDistanceToWalls(sector);
            }
            if (LastSector != sector)
                LastSector = sector;
            ChangeDirection(SavedDirection);
        }

        public abstract void Render();
        public void Draw(Graphics g)
        {
            g.DrawImage(Texture, ImgLocation);
            //g.DrawLine(Pens.Red, new PointF(Center.X, Bounds.Top), new PointF(Center.X, Bounds.Top - Distance.up));
            //g.DrawLine(Pens.Red, new PointF(Bounds.Left, Center.Y), new PointF(Bounds.Left + Distance.left, Center.Y));
            //g.DrawLine(Pens.Red, new PointF(Center.X, Bounds.Bottom), new PointF(Center.X, Bounds.Bottom + Distance.down));
            //g.DrawLine(Pens.Red, new PointF(Bounds.Right, Center.Y), new PointF(Bounds.Right + Distance.right, Center.Y));
        }

        public virtual string Debug(GameTable game)
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