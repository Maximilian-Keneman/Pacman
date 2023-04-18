using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
    public class Gift
    {
        public enum Type
        {
            Cherry,
            Strawberry,
            Orange,
            Apple,
            Mango,
            Banana,
            Peach,
            Heart,
            Bell,
            Diamond,
            Coffee,
            Cake,
            Clover
        }
        private Action<GameTable> Action;
        private RectangleF Bounds;
        private Image Texture;

        private Gift(GameTable owner, Type type)
        {
            SizeF size = new(owner.SectorScaleValue * 2 / 3, owner.SectorScaleValue * 2 / 3);
            Bounds = new RectangleF(owner[owner.GiftPosition].Bounds.Location + (owner.SectorScale - size).Multiple(0.5f), size);
            (Texture, Action) = type switch
            {
                Type.Cherry => (Images.GetFragment(Properties.Resources.Fruits, new(new(3, 6), new(50, 42)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 20)),
                Type.Strawberry => (Images.GetFragment(Properties.Resources.Fruits, new(new(3, 45), new(46, 49)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 20)),
                Type.Orange => (Images.GetFragment(Properties.Resources.Fruits, new(new(5, 93), new(42, 43)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 20)),
                Type.Apple => (Images.GetFragment(Properties.Resources.Fruits, new(new(5, 134), new(41, 46)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 20)),
                Type.Mango => (Images.GetFragment(Properties.Resources.Fruits, new(new(5, 179), new(42, 44)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 50)),
                Type.Banana => (Images.GetFragment(Properties.Resources.Fruits, new(new(4, 224), new(42, 44)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 50)),
                Type.Peach => (Images.GetFragment(Properties.Resources.Fruits, new(new(5, 268), new(47, 43)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 50)),
                Type.Cake => (Images.GetFragment(Properties.Resources.Fruits, new(new(5, 485), new(42, 40)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 100)),
                Type.Diamond => (Images.GetFragment(Properties.Resources.Fruits, new(new(4, 395), new(45, 45)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 100)),
                Type.Clover => (Images.GetFragment(Properties.Resources.Fruits, new(new(7, 526), new(40, 44)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Score += 100)),
                Type.Heart => (Images.GetFragment(Properties.Resources.Fruits, new(new(5, 310), new(44, 38)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.ApllyDamage(-1))),
                Type.Bell => (Images.GetFragment(Properties.Resources.Fruits, new(new(4, 352), new(45, 44)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Ghosts.Act(G => G.ApllyDamage(1)))),
                Type.Coffee => (Images.GetFragment(Properties.Resources.Fruits, new(new(4, 443), new(43, 41)), size.ToSize()), new Action<GameTable>((GameTable owner) => owner.Player.Energetic = true)),
                _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(Type))
            };
            owner.UpdateEvent += IntersectionCheck;
        }
        public static Gift SpawnGift(GameTable owner)
        {
            return (new Random().NextDouble() * 100) switch
            {
                < 10 => new Gift(owner, Type.Cherry),
                < 20 => new Gift(owner, Type.Strawberry),
                < 30 => new Gift(owner, Type.Orange),
                < 40 => new Gift(owner, Type.Apple),
                < 48 => new Gift(owner, Type.Mango),
                < 56 => new Gift(owner, Type.Banana),
                < 64 => new Gift(owner, Type.Peach),
                < 70 => new Gift(owner, Type.Heart),
                < 76 => new Gift(owner, Type.Bell),
                < 82 => new Gift(owner, Type.Diamond),
                < 88 => new Gift(owner, Type.Coffee),
                < 94 => new Gift(owner, Type.Cake),
                < 100 => new Gift(owner, Type.Clover),
                _ => null
            };
        }

        public void Draw(Graphics g) => g.DrawImage(Texture, Bounds);
        private void IntersectionCheck(object sender, EventArgs e)
        {
            GameTable owner = sender as GameTable;
            if (owner.Player.Intersect(Bounds))
            {
                owner.UpdateEvent -= IntersectionCheck;
                owner.Player.IsEat = true;
                owner.RemoveGift();
                Action(owner);
            }
        }
    }
}
