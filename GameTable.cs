using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Pacman
{
    public enum Direction
    {
        None = -1,
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
    public class Sector
    {
        private GameTable Owner;
        public Point TblPosition { get; }
        public Dictionary<Direction, Sector> NeighborSector => new()
        {
            { Direction.Up, TblPosition.X > 0 ? Owner[TblPosition + new Size(-1, 0)] : null },
            { Direction.Right, TblPosition.Y < Owner.TblSize.Height - 1 ? Owner[TblPosition + new Size(0, 1)] : null },
            { Direction.Down, TblPosition.X < Owner.TblSize.Width - 1 ? Owner[TblPosition + new Size(1, 0)] : null },
            { Direction.Left, TblPosition.Y > 0 ? Owner[TblPosition + new Size(0, -1)] : null }
        };
        protected bool RightWall;
        protected bool DownWall;
        public Direction VoidPassage { get; }
        public Dictionary<Direction, bool> CanGo => new()
        {
            { Direction.Up, !NeighborSector[Direction.Up]?.DownWall ?? VoidPassage == Direction.Up },
            { Direction.Right, !RightWall },
            { Direction.Down, !DownWall },
            { Direction.Left, !NeighborSector[Direction.Left]?.RightWall ?? VoidPassage == Direction.Left }
        };

        public Dictionary<Direction, RectangleF> WallBounds => new()
        {
            { Direction.Up, CanGo[Direction.Up] ? RectangleF.Empty : new RectangleF(ImgLocation.X, ImgLocation.Y, Scale, Scale / 10) },
            { Direction.Right, CanGo[Direction.Right] ? RectangleF.Empty : new RectangleF(ImgLocation.X + Scale * 9 / 10, ImgLocation.Y, Scale / 10, Scale) },
            { Direction.Down, CanGo[Direction.Down] ? RectangleF.Empty : new RectangleF(ImgLocation.X, ImgLocation.Y + Scale * 9 / 10, Scale, Scale / 10) },
            { Direction.Left, CanGo[Direction.Left] ? RectangleF.Empty : new RectangleF(ImgLocation.X, ImgLocation.Y, Scale / 10, Scale) }
        };
        public Dictionary<Direction, (RectangleF left, RectangleF right)> CornerBounds => new()
        {
            { Direction.Up, (new RectangleF(ImgLocation.X, ImgLocation.Y, Scale / 10, Scale / 10),
                             new RectangleF(ImgLocation.X + Scale * 9 / 10, ImgLocation.Y, Scale / 10, Scale / 10)) },
            { Direction.Down, (new RectangleF(ImgLocation.X, ImgLocation.Y + Scale * 9 / 10, Scale / 10, Scale / 10),
                               new RectangleF(ImgLocation.X + Scale * 9 / 10, ImgLocation.Y + Scale * 9 / 10, Scale / 10, Scale / 10)) }
        };

        public (RectangleF bounds, bool geted)[] Coins;

        private PointF ImgLocation => new(TblPosition.Y * Scale, TblPosition.X * Scale);
        private SizeF Size => Owner.SectorScale;
        public float Scale => Owner.SectorScaleValue;
        public RectangleF Bounds => new(ImgLocation, Size);
        public Image Background { get; private set; }

        public void PaintBackground()
        {
            (int W, int D) = GetWallDir();
            int ImgSectorSize = 200;
            Size ImgSize = new(ImgSectorSize, ImgSectorSize);
            Background = Images.GetFragment(Properties.Resources.StandartWalls, ImgSize, new RectangleF(new PointF(D * ImgSectorSize, (4 - W) * ImgSectorSize), ImgSize), System.Drawing.Size.Truncate(Size));
        }
        public void Draw(Graphics g)
        {
            g.DrawImage(Background, Bounds);
            foreach (var (coinBounds, coinGeted) in Coins)
                if (!coinGeted)
                    g.FillRectangle(new SolidBrush(Color.Black), coinBounds);
        }

        public Sector(GameTable table, Point tblPosition, bool rightWall, bool downWall, bool noCoins, Direction voidPassage = Direction.None)
        {
            Owner = table;
            TblPosition = tblPosition;
            RightWall = rightWall;
            DownWall = downWall;
            VoidPassage = voidPassage;
            if (noCoins)
                Coins = new (RectangleF bounds, bool geted)[0];
            else
            {
                var coinBounds = new RectangleF(9 * Scale / 20, 9 * Scale / 20, Scale / 10, Scale / 10);
                var coins = new List<RectangleF> { coinBounds };
                if (CanGo[Direction.Up] && NeighborSector[Direction.Up]?.Coins.Length != 0)
                {
                    coinBounds.Location = new(9 * Scale / 20, -Scale / 20);
                    coins.Add(coinBounds);
                }
                if (CanGo[Direction.Left] && NeighborSector[Direction.Left]?.Coins.Length != 0)
                {
                    coinBounds.Location = new(-Scale / 20, 9 * Scale / 20);
                    coins.Add(coinBounds);
                }
                Coins = coins.Select(C => { C.Offset(ImgLocation); return (bounds: C, geted: false); }).ToArray();
            }
            PaintBackground();
        }
        private (int W, int D) GetWallDir()
        {
            return string.Join("", CanGo.Select(b => b.Value ? 1 : 0)) switch
            {
                "1111" => (0, 0),
                "1101" => (1, 0),
                "1110" => (1, 1),
                "0111" => (1, 2),
                "1011" => (1, 3),
                "1100" => (2, 0),
                "0110" => (2, 1),
                "0011" => (2, 2),
                "1001" => (2, 3),
                "1010" => (2, 4),
                "0101" => (2, 5),
                "1000" => (3, 0),
                "0100" => (3, 1),
                "0010" => (3, 2),
                "0001" => (3, 3),
                "0000" => (4, 0),
                _ => throw new Exception()
            };
        }
    }
    public class GameTable
    {
        public PhisicParametrs PParametrs;
        public BoundsParametrs BParametrs;

        protected Sector[,] Sectors;
        protected Sector this[int X, int Y] => X < TblSize.Width && Y < TblSize.Height &&
                                       X >= 0 && Y >= 0 ?
                                       Sectors[X, Y] : null;
        public Sector this[Point p] => this[p.X, p.Y];
        public Size TblSize => new(Sectors.GetLength(0), Sectors.GetLength(1));
        public Size ImgSize { get; protected set; }
        public float SectorScaleValue { get; protected set; }
        public SizeF SectorScale => new(SectorScaleValue, SectorScaleValue);
        
        private bool started = false;
        public bool Started
        {
            get => started;
            private set
            {
                started = value;
                PaintUpdate?.Change(started ? 0 : -1, 100);
                PhisicUpdate?.Change(started ? 0 : -1, 100);
            }
        }

        public Guid Level { get; }

        public Player Player;
        public Ghost[] Ghosts;

        protected Point? StartSector;
        public (Point Location, Direction Exit) GhostHome { get; }

        protected GameTable() { }
        public GameTable(Level level, Player.PlayerArgs player, Size formSize, IScreen box)
        {
            Level = level.GUID;
            Sectors = new Sector[level.Size.Width, level.Size.Height];
            SectorScaleValue = GetSectorScale(formSize);
            PParametrs = new(SectorScaleValue);
            BParametrs = new(SectorScaleValue);
            ImgSize = Size.Truncate(new(TblSize.Height * SectorScaleValue, TblSize.Width * SectorScaleValue));
            box.UpdateImage(new Bitmap(ImgSize.Width, ImgSize.Height));
            StartSector = level.Start;
            GhostHome = level.GhostHome;
            for (int x = 0; x < TblSize.Width; x++)
                for (int y = 0; y < TblSize.Height; y++)
                {
                    Direction voidPassage = Direction.None;
                    switch (level.Portal.Direction)
                    {
                        case Direction.Up:
                        case Direction.Down:
                            if (y == level.Portal.Index)
                                if (x == 0)
                                    voidPassage = Direction.Up;
                                else if (x == TblSize.Width - 1)
                                    voidPassage = Direction.Down;
                            break;
                        case Direction.Right:
                        case Direction.Left:
                            if (x == level.Portal.Index)
                                if (y == 0)
                                    voidPassage = Direction.Left;
                                else if (y == TblSize.Height - 1)
                                    voidPassage = Direction.Right;
                            break;
                    }
                    Sectors[x, y] = new(this, new(x, y), level.Structure[x, y].RightWall, level.Structure[x, y].DownWall, level.Structure[x, y].NoCoin, voidPassage);
                }
            Player = new(this, player);
            Player.Render();
            UpdateEvent += Player.Update;
            Ghosts = new Ghost[4];
            Ghosts[0] = new Blinky(this, new(-1, TblSize.Height));
            Ghosts[1] = new Pinky(this, new(-1, -1));
            Ghosts[2] = new Inky(this, new(TblSize.Width, TblSize.Height), (Blinky)Ghosts[0]);
            Ghosts[3] = new Clyde(this, new(TblSize.Width, -1));
            for (int i = 0; i < Ghosts.Length; i++)
            {
                Ghosts[i].Render();
                UpdateEvent += Ghosts[i].Update;
            }
            OnPaint(box);
            PaintUpdate = new(OnPaint, box, -1, 100);
            PhisicUpdate = new(Update, null, -1, 100);

            SyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        protected SynchronizationContext SyncContext;

        protected Timer PaintUpdate;
        private bool PaintFrameStart = false;
        protected void OnPaint(object state)
        {
            if (!PaintFrameStart)
            {
                PaintFrameStart = true;
                Image img = PaintProcess();
                (state as IScreen).UpdateImage(img);
                img.Dispose();
                PaintFrameStart = false;
            }
        }
        protected virtual Image PaintProcess()
        {
            Image img = new Bitmap(ImgSize.Width, ImgSize.Height);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.Clear(Color.White);
                for (int x = 0; x < TblSize.Width; x++)
                    for (int y = 0; y < TblSize.Height; y++)
                        Sectors[x, y].Draw(g);
                Player?.Draw(g);
                for (int i = 0; i < Ghosts.Length; i++)
                    Ghosts[i].Draw(g);
            }
            return img;
        }

        protected Timer PhisicUpdate;
        public event EventHandler UpdateEvent;
        private bool PhisicFrameStart = false;

        protected void Update(object state)
        {
            if (!PhisicFrameStart)
            {
                PhisicFrameStart = true;
                EventHandler UpdateEnd = (sender, e) => PhisicFrameStart = false;
                UpdateEvent += UpdateEnd;
                UpdateEvent(this, EventArgs.Empty);
                UpdateEvent -= UpdateEnd;
            }
        }

        public void PlayerToStart()
        {
            RectangleF startWall = this[StartSector.Value].Bounds;
            Player.TpTo(new(startWall.Left + startWall.Width / 2, startWall.Top + startWall.Height / 2));
            Player.ChangeDirection(Direction.Left);
            startWall = this[GhostHome.Location].Bounds;
            for (int i = 0; i < Ghosts.Length; i++)
            {
                Ghosts[i].TpTo(new(startWall.Left + startWall.Width / 2, startWall.Top + startWall.Height / 2));
                Ghosts[i].ChangeDirection(Direction.Left);
                Ghosts[i].Restart();
            }
        }
        public void GameStart()
        {
            PlayerToStart();
            Started = true;
        }
        public void GamePause()
        {
            Started = false;
        }
        public  void GameContinue()
        {
            Started = true;
        }
        public void CheckFinish()
        {
            if (Sectors.OfType<Sector>().SelectMany(S => S.Coins).All(C => C.geted))
            {
                SyncContext.Post(GameOver, null);
            }
        }
        public void GameEnd() => SyncContext.Post(GameOver, null);
        private void GameOver(object state)
        {
            Started = false;
            OnGameOver?.Invoke(this, new(Player.Score));
        }
        public event EventHandler<GameOverEventArgs> OnGameOver;

        public class GameOverEventArgs : EventArgs
        {
            public int Score { get; }
            public GameOverEventArgs(int score) => Score = score;
        }

        protected float GetSectorScale(Size formSize)
        {
            float Y = (float)formSize.Height / TblSize.Height;
            float X = (float)formSize.Width / TblSize.Width;
            return X > Y ? Y : X;
        }
        public (Sector sector, Point position) GetPositionSector(Point imgLocation)
        {
            float sectorSize = SectorScaleValue;
            int x = imgLocation.Y / sectorSize.Round();
            int y = imgLocation.X / sectorSize.Round();
            if (imgLocation.Y < 0 && x == 0)
                x = -1;
            if (imgLocation.X < 0 && y == 0)
                y = -1;
            Point p = new(x, y);
            return (this[p], p);
        }

        public string Debug()
        {
            return $"PacmanSpeed {PParametrs.PacmanSpeed}\nGhostSpeed {PParametrs.GhostSpeed}\nEyesSpeed {PParametrs.EyesSpeed}\n" +
                   $"WallBound {BParametrs.WallBound}";
        }

        public struct PhisicParametrs
        {
            public int PacmanSpeed { get; }
            public int GhostSpeed { get; }
            public int EyesSpeed { get; }

            public PhisicParametrs(float sectorScale)
            {
                PacmanSpeed = (sectorScale * 0.2f).Round();
                GhostSpeed = (sectorScale * 0.15f).Round();
                EyesSpeed = (sectorScale * 0.3f).Round();
            }
        }
        public struct BoundsParametrs
        {
            public float WallBound { get; }

            public BoundsParametrs(float sectorScale)
            {
                WallBound = 0.5f;
            }
        }
    }

    [Serializable]
    public struct Level : ISerializable
    {
        public static readonly Level Empty = new();

        public Guid GUID { get; private set; }
        public void ChangeGUID(Guid guid) => GUID = guid;
        public (bool RightWall, bool DownWall, bool NoCoin)[,] Structure { get; }
        public Size Size => new(Structure.GetLength(0), Structure.GetLength(1));
        public Point Start { get; }
        public (int Index, Direction Direction) Portal { get; }
        public (Point Location, Direction Exit) GhostHome { get; }

        public Level((bool RightWall, bool DownWall, bool NoCoin)[,] structure, Point StartSector,
                     (int Index, Direction Direction) portal, (Point Location, Direction Exit) ghostHome)
        {
            GUID = Guid.NewGuid();
            Structure = structure;
            Start = StartSector;
            Portal = portal;
            GhostHome = ghostHome;
        }

        public void Save(string path)
        {
            BinaryFormatter F = new();
            using FileStream fs = new(path, FileMode.CreateNew, FileAccess.Write);
            F.Serialize(fs, this);
        }
        public static Level Load(string path)
        {
            BinaryFormatter F = new();
            using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
            return (Level)F.Deserialize(fs);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("GUID", GUID.ToString());
            info.AddValue("Width", Size.Width);
            info.AddValue("Height", Size.Height);
            for (int x = 0; x < Size.Width; x++)
                for (int y = 0; y < Size.Height; y++)
                    info.AddValue($"{x},{y}", (Structure[x, y].RightWall ? 100 : 0) + (Structure[x, y].DownWall ? 10 : 0) + (Structure[x, y].NoCoin ? 1 : 0));
            info.AddValue("Start", $"{Start.X},{Start.Y}");
            info.AddValue("Portal", Portal.Index * 10 + (int)Portal.Direction);
            info.AddValue("GhostHome", $"{GhostHome.Location.X},{GhostHome.Location.Y},{(int)GhostHome.Exit}");
        }
        private Level(SerializationInfo info, StreamingContext context)
        {
            GUID = Guid.Parse(info.GetString("GUID"));
            int width = info.GetInt32("Width");
            int height = info.GetInt32("Height");
            int[,] structure = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    structure[x, y] = info.GetInt32($"{x},{y}");
            Structure = new (bool RightWall, bool DownWall, bool NoCoin)[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Structure[x, y] = (RightWall: structure[x, y] / 100 != 0, DownWall: structure[x, y] / 10 % 10 != 0, NoCoin: structure[x, y] % 10 != 0);
            int[] start = info.GetString("Start").Split(',').Select(int.Parse).ToArray();
            Start = new Point(start[0], start[1]);
            int portal = info.GetInt32("Portal");
            Portal = (portal / 10, (Direction)(portal % 10));
            int[] ghostHome = info.GetString("GhostHome").Split(',').Select(int.Parse).ToArray();
            GhostHome = (new(ghostHome[0], ghostHome[1]), (Direction)ghostHome[2]);
        }
    }

    public interface IScreen
    {
        public Image Image {get;}
        public void UpdateImage(Image img);
    }
}