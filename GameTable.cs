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

        public (Image texture, RectangleF imgBounds, RectangleF actBounds, bool geted, bool energetic)[] Coins;

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
            Background = Images.GetFragment(Properties.Resources.StandartWalls, new RectangleF(new PointF(D * ImgSectorSize, (4 - W) * ImgSectorSize), ImgSize), System.Drawing.Size.Truncate(Size));
        }
        public void Draw(Graphics g)
        {
            g.DrawImage(new Bitmap(Background, Bounds.Floor().Size), Bounds.Floor());
            foreach (var (texture, coinBounds, _, coinGeted, _) in Coins)
                if (!coinGeted)
                    g.DrawImage(texture, coinBounds);
        }

        public Sector(GameTable table, Point tblPosition, bool rightWall, bool downWall, bool noCoins, bool energetic, Direction voidPassage = Direction.None)
        {
            Owner = table;
            TblPosition = tblPosition;
            RightWall = rightWall;
            DownWall = downWall;
            VoidPassage = voidPassage;
            if (noCoins)
                Coins = new (Image texture, RectangleF imgBounds, RectangleF actBounds, bool geted, bool energetic)[0];
            else
            {
                RectangleF GetActBounds(RectangleF bounds, float proportion) =>
                    new(bounds.Location + bounds.Size.Multiple(0.5f) - bounds.Size.Multiple(proportion / 2), bounds.Size.Multiple(proportion));
                var coinSize = new SizeF(Scale / 4, Scale / 4);
                var energeticSize = new SizeF(Scale * 3 / 8, Scale * 3 / 8);
                var coins = new List<RectangleF>
                {
                    energetic ? new(new PointF(Scale / 2, Scale / 2) - energeticSize.Multiple(0.5f), energeticSize) :
                                new(new PointF(Scale / 2, Scale / 2) - coinSize.Multiple(0.5f), coinSize)
                };
                if (CanGo[Direction.Up] && NeighborSector[Direction.Up]?.Coins.Length != 0)
                    coins.Add(new(new(Scale / 2 - coinSize.Width / 2, -coinSize.Height / 2), coinSize));
                if (CanGo[Direction.Left] && NeighborSector[Direction.Left]?.Coins.Length != 0)
                    coins.Add(new(new(-coinSize.Width / 2, Scale / 2 - coinSize.Height / 2), coinSize));
                Coins = coins.Select(C => { C.Offset(ImgLocation); return C; })
                             .Select(C => (texture: Images.GetFragment(Properties.Resources.Coins, energetic ? new(new(8, 101), new(26, 25)) : new(new(11, 60), new(18, 17)), C.Size.ToSize()),
                                           imgBounds: C,
                                           actBounds: GetActBounds(C, 0.5f),
                                           geted: false,
                                           energetic: false)).ToArray();
                Coins[0].energetic = energetic;
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
                //PaintUpdate?.Change(started ? 0 : -1, 100);
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
                    bool energetic = level.Energetics.Contains(new(x, y));
                    Sectors[x, y] = new(this, new(x, y), level.Structure[x, y].RightWall, level.Structure[x, y].DownWall, level.Structure[x, y].NoCoin, energetic, voidPassage);
                }
            Player = new(this, player);
            UpdateEvent += Player.Update;
            Ghosts = new Ghost[4];
            Ghosts[0] = new Blinky(this, new(-1, TblSize.Height), level.BlinkyEvents);
            Ghosts[1] = new Pinky(this, new(-1, -1), level.PinkyEvents);
            Ghosts[2] = new Inky(this, new(TblSize.Width, TblSize.Height), (Blinky)Ghosts[0], level.InkyEvents);
            Ghosts[3] = new Clyde(this, new(TblSize.Width, -1), level.ClydeEvents);
            for (int i = 0; i < Ghosts.Length; i++)
                UpdateEvent += Ghosts[i].Update;
            OnPaint(box);
            PaintUpdate = new(OnPaint, box, -1, 100);
            PhisicUpdate = new(Update, null, -1, 100);

            SyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        protected SynchronizationContext SyncContext;

        protected Timer PaintUpdate;
        public event EventHandler PaintEvent;
        private bool PaintFrameStart = false;
        protected void OnPaint(object state)
        {
            if (!PaintFrameStart)
            {
                PaintFrameStart = true;
                PaintEvent?.Invoke(this, EventArgs.Empty);
                IScreen screen = state as IScreen;
                Image img = PaintProcess(screen.DebugMode);
                screen.UpdateImage(img);
                img.Dispose();
                PaintFrameStart = false;
            }
        }
        protected virtual Image PaintProcess(DebugMode debugMode)
        {
            Image img = new Bitmap(ImgSize.Width, ImgSize.Height);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.Clear(Color.Gray);
                for (int x = 0; x < TblSize.Width; x++)
                    for (int y = 0; y < TblSize.Height; y++)
                        Sectors[x, y].Draw(g);
                for (int i = 0; i < Ghosts.Length; i++)
                    Ghosts[i].Draw(g, debugMode);
                Player?.Draw(g, debugMode);
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
            PaintUpdate?.Change(started ? 0 : -1, 100);
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
            if (Sectors.OfType<Sector>().SelectMany(S => S.Coins).All(C => C.geted || C.energetic))
            {
                SyncContext.Post(GameOver, null);
            }
        }
        public void GameEnd() => SyncContext.Post(GameOver, null);
        private void GameOver(object state)
        {
            Started = false;
            PaintUpdate?.Change(started ? 0 : -1, 100);
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
        public Point[] Energetics { get; }
        public (Point Location, Direction Exit) GhostHome { get; }
        public (int timer, Ghost.Behaviour newBehaviour)[] BlinkyEvents { get; }
        public (int timer, Ghost.Behaviour newBehaviour)[] PinkyEvents { get; }
        public (int timer, Ghost.Behaviour newBehaviour)[] InkyEvents { get; }
        public (int timer, Ghost.Behaviour newBehaviour)[] ClydeEvents { get; }

        public Level((bool RightWall, bool DownWall, bool NoCoin)[,] structure, Point StartSector,
                     (int Index, Direction Direction) portal, Point[] energetics,
                     (Point Location, Direction Exit) ghostHome,
                     (int timer, Ghost.Behaviour newBehaviour)[] blinkyEvents,
                     (int timer, Ghost.Behaviour newBehaviour)[] pinkyEvents,
                     (int timer, Ghost.Behaviour newBehaviour)[] inkyEvents,
                     (int timer, Ghost.Behaviour newBehaviour)[] clydeEvents)
        {
            GUID = Guid.NewGuid();
            Structure = structure;
            Start = StartSector;
            Portal = portal;
            Energetics = energetics;
            GhostHome = ghostHome;
            BlinkyEvents = blinkyEvents;
            PinkyEvents = pinkyEvents;
            InkyEvents = inkyEvents;
            ClydeEvents = clydeEvents;
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

        private enum ObjectNames
        {
            GUID,
            Width,
            Height,
            Start,
            Portal,
            GhostHome,
            EnergeticsCount,
            Energetics,
            BlinkyCount,
            PinkyCount,
            InkyCount,
            ClydeCount,
            Blinky,
            Pinky,
            Inky,
            Clyde
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue($"{ObjectNames.GUID}", GUID.ToString());
            info.AddValue($"{ObjectNames.Width}", Size.Width);
            info.AddValue($"{ObjectNames.Height}", Size.Height);
            for (int x = 0; x < Size.Width; x++)
                for (int y = 0; y < Size.Height; y++)
                    info.AddValue($"{x},{y}", (Structure[x, y].RightWall ? 100 : 0) + (Structure[x, y].DownWall ? 10 : 0) + (Structure[x, y].NoCoin ? 1 : 0));
            info.AddValue($"{ObjectNames.Start}", $"{Start.X},{Start.Y}");
            info.AddValue($"{ObjectNames.Portal}", Portal.Index * 10 + (int)Portal.Direction);
            info.AddValue($"{ObjectNames.EnergeticsCount}", Energetics.Length);
            for (int i = 0; i < Energetics.Length; i++)
                info.AddValue($"{ObjectNames.Energetics}{i}", $"{Energetics[i].X},{Energetics[i].Y}");
            info.AddValue($"{ObjectNames.GhostHome}", $"{GhostHome.Location.X},{GhostHome.Location.Y},{(int)GhostHome.Exit}");
            info.AddValue($"{ObjectNames.BlinkyCount}", BlinkyEvents.Length);
            for (int i = 0; i < BlinkyEvents.Length; i++)
                info.AddValue($"{ObjectNames.Blinky}{i}", BlinkyEvents[i].timer * 10 + (int)BlinkyEvents[i].newBehaviour);
            info.AddValue($"{ObjectNames.PinkyCount}", PinkyEvents.Length);
            for (int i = 0; i < PinkyEvents.Length; i++)
                info.AddValue($"{ObjectNames.Pinky}{i}", PinkyEvents[i].timer * 10 + (int)PinkyEvents[i].newBehaviour);
            info.AddValue($"{ObjectNames.InkyCount}", InkyEvents.Length);
            for (int i = 0; i < InkyEvents.Length; i++)
                info.AddValue($"{ObjectNames.Inky}{i}", InkyEvents[i].timer * 10 + (int)InkyEvents[i].newBehaviour);
            info.AddValue($"{ObjectNames.ClydeCount}", ClydeEvents.Length);
            for (int i = 0; i < ClydeEvents.Length; i++)
                info.AddValue($"{ObjectNames.Clyde}{i}", ClydeEvents[i].timer * 10 + (int)ClydeEvents[i].newBehaviour);
        }
        private Level(SerializationInfo info, StreamingContext context)
        {
            GUID = Guid.Parse(info.GetString($"{ObjectNames.GUID}"));
            int width = info.GetInt32($"{ObjectNames.Width}");
            int height = info.GetInt32($"{ObjectNames.Height}");
            int[,] structure = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    structure[x, y] = info.GetInt32($"{x},{y}");
            Structure = new (bool RightWall, bool DownWall, bool NoCoin)[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Structure[x, y] = (RightWall: structure[x, y] / 100 != 0, DownWall: structure[x, y] / 10 % 10 != 0, NoCoin: structure[x, y] % 10 != 0);
            int[] start = info.GetString($"{ObjectNames.Start}").Split(',').Select(int.Parse).ToArray();
            Start = new Point(start[0], start[1]);
            int portal = info.GetInt32($"{ObjectNames.Portal}");
            Portal = (portal / 10, (Direction)(portal % 10));
            int[][] energetics = new int[info.GetInt32($"{ObjectNames.EnergeticsCount}")][];
            for (int i = 0; i < energetics.Length; i++)
                energetics[i] = info.GetString($"{ObjectNames.Energetics}{i}").Split(',').Select(int.Parse).ToArray();
            Energetics = energetics.Select(E => new Point(E[0], E[1])).ToArray();
            int[] ghostHome = info.GetString($"{ObjectNames.GhostHome}").Split(',').Select(int.Parse).ToArray();
            GhostHome = (new(ghostHome[0], ghostHome[1]), (Direction)ghostHome[2]);
            int[] blinkyEvents = new int[info.GetInt32($"{ObjectNames.BlinkyCount}")];
            for (int i = 0; i < blinkyEvents.Length; i++)
                blinkyEvents[i] = info.GetInt32($"{ObjectNames.Blinky}{i}");
            BlinkyEvents = blinkyEvents.Select(E => (E / 10, (Ghost.Behaviour)(E % 10))).ToArray();
            int[] pinkyEvents = new int[info.GetInt32($"{ObjectNames.PinkyCount}")];
            for (int i = 0; i < pinkyEvents.Length; i++)
                pinkyEvents[i] = info.GetInt32($"{ObjectNames.Pinky}{i}");
            PinkyEvents = pinkyEvents.Select(E => (E / 10, (Ghost.Behaviour)(E % 10))).ToArray();
            int[] inkyEvents = new int[info.GetInt32($"{ObjectNames.InkyCount}")];
            for (int i = 0; i < inkyEvents.Length; i++)
                inkyEvents[i] = info.GetInt32($"{ObjectNames.Inky}{i}");
            InkyEvents = inkyEvents.Select(E => (E / 10, (Ghost.Behaviour)(E % 10))).ToArray();
            int[] clydeEvents = new int[info.GetInt32($"{ObjectNames.ClydeCount}")];
            for (int i = 0; i < clydeEvents.Length; i++)
                clydeEvents[i] = info.GetInt32($"{ObjectNames.Clyde}{i}");
            ClydeEvents = clydeEvents.Select(E => (E / 10, (Ghost.Behaviour)(E % 10))).ToArray();
        }
    }

    public interface IScreen
    {
        public Image Image {get;}
        public void UpdateImage(Image img);
        public DebugMode DebugMode { get; }
    }
    public class DebugMode
    {
        private string[] Modes =>
            GetType().GetFields().Where(P => P.FieldType == typeof(bool)).Select(P => P.Name).ToArray();
        public void SetAll(bool value)
        {
            foreach (var name in Modes)
                GetType().InvokeMember(name, System.Reflection.BindingFlags.SetField, null, this, new object[] { value });
        }
        public bool WallDistance;
    }
}