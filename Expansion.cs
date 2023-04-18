using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Pacman
{
    public static class Images
    {
        // Отрегулируйте прозрачность изображения.
        public static Image AdjustAlpha(this Image image, float translucency)
        {
            // Создаем ColorMatrix.
            float t = translucency;
            ColorMatrix cm = new(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, t, 0},
                new float[] {0, 0, 0, 0, 1},
            });
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(cm);

            // Нарисуем изображение на новом растровом изображении
            // применение новой ColorMatrix.
            Point[] points =
            {
                new(0, 0),
                new(image.Width, 0),
                new(0, image.Height),
            };
            Rectangle rect = new(0, 0, image.Width, image.Height);

            // Создаем растровое изображение результата.
            Image img = new Bitmap(image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(img))
                gr.DrawImage(image, points, rect, GraphicsUnit.Pixel, attributes);

            // Вернуть результат.
            return img;
        }

        public static Image GetFragment(Image source, RectangleF sourceRect)
        {
            Image img = new Bitmap(sourceRect.Width.Round(), sourceRect.Height.Round());
            using (Graphics g = Graphics.FromImage(img))
                g.DrawImage(source, 0, 0, sourceRect, GraphicsUnit.Pixel);
            return img;
        }
        public static Image GetFragment(Image source, RectangleF sourceRect, Size needSize)
            => new Bitmap(GetFragment(source, sourceRect), needSize);
        public static Image GetRectangle(Color color, SizeF sizeF)
        {
            Size size = Size.Truncate(sizeF);
            Image img = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(img))
                g.FillRectangle(new SolidBrush(color), 0, 0, sizeF.Width, sizeF.Height);
            return img;
        }
        public static Image GetRound(Color color, float diameter)
        {
            Image img = new Bitmap(diameter.Floor(), diameter.Floor());
            using (Graphics g = Graphics.FromImage(img))
                g.FillRound(new SolidBrush(color), new(0, 0), diameter);
            return img;
        }

        public static void FillRound(this Graphics g, Brush brush, PointF center, float diameter)
        {
            RectangleF rect = new(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
            g.FillEllipse(brush, rect);
        }
        public static void DrawArrow(this Graphics g, Color color, RectangleF rect, float delta, float width, Direction direction)
        {
            var (p1, p2, p3, p4) = direction switch
            {
                Direction.Up => (new PointF(rect.Left + rect.Width / 2, rect.Top),
                                 new PointF(rect.Left + rect.Width / 2, rect.Bottom),
                                 new PointF(rect.Left, rect.Top + delta),
                                 new PointF(rect.Right, rect.Top + delta)),
                Direction.Right => (new PointF(rect.Right, rect.Top + rect.Height / 2),
                                    new PointF(rect.Left, rect.Top + rect.Height / 2),
                                    new PointF(rect.Right - delta, rect.Top),
                                    new PointF(rect.Right - delta, rect.Bottom)),
                Direction.Down => (new PointF(rect.Left + rect.Width / 2, rect.Bottom),
                                   new PointF(rect.Left + rect.Width / 2, rect.Top),
                                   new PointF(rect.Left, rect.Bottom - delta),
                                   new PointF(rect.Right, rect.Bottom - delta)),
                Direction.Left => (new PointF(rect.Left, rect.Top + rect.Height / 2),
                                   new PointF(rect.Right, rect.Top + rect.Height / 2),
                                   new PointF(rect.Left + delta, rect.Top),
                                   new PointF(rect.Left + delta, rect.Bottom)),
                _ => throw direction.ThrowInvalidDirectionException()
            };
            g.DrawLine(new(color, width), p1, p2);
            g.DrawLine(new(color, width), p1, p3);
            g.DrawLine(new(color, width), p1, p4);
            g.FillRound(new SolidBrush(color), p1, width);
        }
    }

    public static partial class Expansion
    {
        public static PointF Center(this RectangleF rect) => new(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        public static double Length(this Size vector) => Math.Sqrt(Math.Pow(vector.Width, 2) + Math.Pow(vector.Height, 2));
        public static double Length(this SizeF vector) => Math.Sqrt(Math.Pow(vector.Width, 2) + Math.Pow(vector.Height, 2));
        public static Size Multiple(this Size size, int multiple) => new(size.Width * multiple, size.Height * multiple);
        public static SizeF Multiple(this SizeF size, float multiple) => new(size.Width * multiple, size.Height * multiple);
        public static SizeF ToSize(this PointF point) => new(point.X, point.Y);
        public static Rectangle Round(this RectangleF rect) => new(rect.X.Round(), rect.Y.Round(), rect.Width.Round(), rect.Height.Round());
        public static Rectangle Floor(this RectangleF rect) => new(rect.X.Floor(), rect.Y.Floor(), rect.Width.Floor(), rect.Height.Floor());

        public static (bool, bool, bool) ToBool(this (int n1, int n2, int n3) v) => (v.n1 != 0, v.n2 != 0, v.n3 != 0);
        public static int Round(this float f) => (int)Math.Round(f);
        public static int Floor(this float f) => (int)Math.Floor(f);
        public static void Act<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }
        public static string Encrypt(this string data)
        {
            return string.Join(" 0x", Encoding.Default.GetBytes(data).Select(i => i.ToString("X2")));
        }
        public static string Decrypt(this string crypt)
        {
            return crypt.Split(' ')
                        .Select(s => Convert.ToInt32(s.StartsWith("0x") ? s.Substring(2) : s, 16))
                        .Select(char.ConvertFromUtf32)
                        .Aggregate((S, s) => S + s);
        }
        public static string GetFileName(this string path)
        {
            string name = path.Split('\\').Last();
            int i = name.LastIndexOf('.');
            return name.Substring(0, i);
        }

        public static Exception ThrowInvalidDirectionException(this Direction direction)
            => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction));
        public static Size ToSize(this Direction direction) => direction switch
        {
            Direction.Up => new(-1, 0),
            Direction.Right => new(0, -1),
            Direction.Down => new(1, 0),
            Direction.Left => new(0, 1),
            _ => throw direction.ThrowInvalidDirectionException()
        };
        public static Point ToPoint(this Direction direction) => direction switch
        {
            Direction.Up => new(-1, 0),
            Direction.Right => new(0, -1),
            Direction.Down => new(1, 0),
            Direction.Left => new(0, 1),
            _ => throw direction.ThrowInvalidDirectionException()
        };
        public static Size ToSizeOrEmpty(this Direction direction) => direction switch
        {
            Direction.Up => new(-1, 0),
            Direction.Right => new(0, -1),
            Direction.Down => new(1, 0),
            Direction.Left => new(0, 1),
            _ => Size.Empty
        };
        public static Point ToPointOrEmpty(this Direction direction) => direction switch
        {
            Direction.Up => new(-1, 0),
            Direction.Right => new(0, -1),
            Direction.Down => new(1, 0),
            Direction.Left => new(0, 1),
            _ => Point.Empty
        };
        public static Size ToSizeOrEmpty(this Direction direction, Size empty) => direction switch
        {
            Direction.Up => new Size(-1, 0),
            Direction.Right => new Size(0, -1),
            Direction.Down => new Size(1, 0),
            Direction.Left => new Size(0, 1),
            _ => empty
        };
        public static Point ToPointOrEmpty(this Direction direction, Point empty) => direction switch
        {
            Direction.Up => new(-1, 0),
            Direction.Right => new(0, -1),
            Direction.Down => new(1, 0),
            Direction.Left => new(0, 1),
            _ => empty
        };
        public static Direction Inverse(this Direction direction) => direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => Direction.None
        };
    }
}