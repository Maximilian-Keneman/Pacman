using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman
{
    public partial class MainForm : Form
    {
        ScreenBox Screen;
        GameTable Game;
        List<Level> Levels;

        public MainForm()
        {
            InitializeComponent();
            Screen = new ScreenBox(GameBox);
            var blinkyEvents = new (int timer, Ghost.Behaviour newBehaviour)[]
            {
                (50, Ghost.Behaviour.Scatter),
                (20, Ghost.Behaviour.Chase)
            };
            var pinkyEvents = new (int timer, Ghost.Behaviour newBehaviour)[]
            {
                (30, Ghost.Behaviour.GetOut),
                (50, Ghost.Behaviour.Scatter),
                (20, Ghost.Behaviour.Chase)
            };
            var inkyEvents = new (int timer, Ghost.Behaviour newBehaviour)[]
            {
                (80, Ghost.Behaviour.GetOut),
                (50, Ghost.Behaviour.Scatter),
                (20, Ghost.Behaviour.Chase)
            };
            var clydeEvents = new (int timer, Ghost.Behaviour newBehaviour)[]
            {
                (150, Ghost.Behaviour.GetOut),
                (50, Ghost.Behaviour.Scatter),
                (20, Ghost.Behaviour.Chase)
            };
            Levels = new List<Level>();
            var intLevel = new (int RightWall, int DownWall, int NoCoin)[18, 17]
            {
                { (0,0, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (1,0, 0), (1,0, 1), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (1,0, 0) },
                { (1,0, 0), (0,0, 1), (1,0, 1), (1,0, 0), (0,0, 1), (0,0, 1), (1,0, 1), (1,0, 0), (1,0, 1), (1,0, 0), (0,0, 1), (0,0, 1), (1,0, 1), (1,0, 0), (0,0, 1), (1,0, 1), (1,0, 0) },
                { (1,0, 0), (0,1, 1), (1,1, 1), (1,0, 0), (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,1, 1), (1,0, 0), (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (0,1, 1), (1,1, 1), (1,0, 0) },
                { (0,0, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (1,0, 0) },
                { (1,0, 0), (0,1, 1), (1,1, 1), (1,0, 0), (1,0, 1), (1,0, 0), (0,1, 1), (0,1, 1), (0,0, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,0, 1), (1,0, 0), (0,1, 1), (1,1, 1), (1,0, 0) },
                { (0,1, 0), (0,1, 0), (0,1, 0), (1,0, 0), (1,0, 1), (0,1, 0), (0,1, 0), (1,0, 0), (1,0, 1), (0,0, 0), (0,1, 0), (1,1, 0), (1,0, 1), (0,0, 0), (0,1, 0), (0,1, 0), (1,1, 0) },
                { (0,0, 1), (0,0, 1), (1,0, 1), (1,0, 0), (0,0, 1), (0,1, 1), (1,1, 1), (1,0, 1), (1,1, 1), (1,0, 1), (0,1, 1), (0,1, 1), (1,0, 1), (1,0, 0), (0,0, 1), (0,0, 1), (1,0, 1) },
                { (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,1, 1), (0,0, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,0, 1), (1,1, 1), (1,0, 0), (0,1, 1), (0,1, 1), (1,1, 1) },
                { (0,1, 1), (0,1, 1), (0,1, 1), (0,0, 0), (0,1, 1), (1,0, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,1, 1), (0,0, 1), (0,1, 1), (0,0, 0), (0,1, 1), (0,1, 1), (0,1, 1) },
                { (0,0, 1), (0,0, 1), (1,0, 1), (1,0, 0), (1,0, 1), (0,0, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,0, 1), (1,0, 1), (1,0, 0), (0,0, 1), (0,0, 1), (1,0, 1) },
                { (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,1, 1), (1,0, 1), (0,1, 1), (0,1, 1), (0,0, 1), (0,1, 1), (1,1, 1), (1,0, 1), (1,1, 1), (1,0, 0), (0,1, 1), (0,1, 1), (1,1, 1) },
                { (0,0, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (1,0, 0), (1,0, 1), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (1,0, 0) },
                { (1,0, 0), (0,1, 1), (1,0, 1), (1,0, 0), (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,1, 1), (1,0, 0), (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (0,0, 1), (1,1, 1), (1,0, 0) },
                { (0,1, 0), (1,0, 0), (1,0, 1), (0,0, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (1,0, 0), (1,0, 1), (0,0, 0), (1,1, 0) },
                { (1,1, 1), (1,0, 0), (1,1, 1), (1,0, 0), (1,0, 1), (1,0, 0), (0,1, 1), (0,1, 1), (0,0, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,0, 1), (1,0, 0), (1,1, 1), (1,0, 0), (1,1, 1) },
                { (0,0, 0), (0,1, 0), (0,1, 0), (1,1, 0), (1,0, 1), (0,1, 0), (0,1, 0), (1,0, 0), (1,0, 1), (0,0, 0), (0,1, 0), (1,1, 0), (1,0, 1), (0,1, 0), (0,1, 0), (0,1, 0), (1,0, 0) },
                { (1,0, 0), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0), (1,1, 1), (1,0, 0), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,1, 1), (1,0, 0) },
                { (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (1,1, 0) }
            };
            (bool RightWall, bool DownWall, bool NoCoin)[,] boolLevel = new (bool RightWall, bool DownWall, bool NoCoin)[18, 17];
            for (int x = 0; x < intLevel.GetLength(0); x++)
                for (int y = 0; y < intLevel.GetLength(1); y++)
                    boolLevel[x, y] = intLevel[x, y].ToBool();
            Levels.Add(new Level(boolLevel, new Point(13, 8), (8, Direction.Right), new Point[] { new(1, 0), new(1, 16), new(13, 0), new(13, 16) }, (new Point(8, 8), Direction.Up),
                blinkyEvents, pinkyEvents, inkyEvents, clydeEvents));
            intLevel = new (int RightWall, int DownWall, int NoCoin)[11, 12]
            {
                { (0,0, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (1,0, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (1,0, 0) },
                { (1,0, 0), (1,1, 1), (1,0, 0), (0,1, 1), (1,1, 1), (1,0, 0), (1,0, 0), (0,1, 1), (1,1, 1), (1,0, 0), (1,1, 1), (1,0, 0) },
                { (0,0, 0), (0,1, 0), (0,0, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,0, 0), (0,1, 0), (1,0, 0) },
                { (0,1, 0), (0,1, 0), (1,0, 0), (0,1, 0), (0,1, 0), (1,0, 0), (0,0, 0), (0,1, 0), (1,1, 0), (0,0, 0), (0,1, 0), (1,1, 0) },
                { (0,1, 1), (1,1, 1), (1,0, 0), (0,0, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,0, 1), (1,0, 0), (0,1, 1), (1,1, 1) },
                { (0,1, 1), (0,1, 1), (0,0, 0), (1,0, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,1, 1), (0,0, 1), (0,0, 0), (0,1, 1), (0,1, 1) },
                { (0,1, 1), (1,1, 1), (1,0, 0), (0,0, 1), (0,1, 1), (0,1, 1), (0,1, 1), (0,1, 1), (1,0, 1), (1,0, 0), (0,1, 1), (1,1, 1) },
                { (0,0, 0), (0,1, 0), (0,0, 0), (0,1, 0), (0,1, 0), (1,0, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,0, 0), (0,1, 0), (1,0, 0) },
                { (0,1, 0), (1,0, 0), (0,0, 0), (0,0, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,0, 0), (1,0, 0), (0,0, 0), (1,1, 0) },
                { (0,0, 0), (0,1, 0), (1,1, 0), (0,1, 0), (0,1, 0), (1,0, 0), (0,0, 0), (0,1, 0), (1,1, 0), (0,1, 0), (0,1, 0), (1,0, 0) },
                { (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (0,1, 0), (1,1, 0) }
            };
            boolLevel = new (bool RightWall, bool DownWall, bool NoCoin)[11, 12];
            for (int x = 0; x < intLevel.GetLength(0); x++)
                for (int y = 0; y < intLevel.GetLength(1); y++)
                    boolLevel[x, y] = intLevel[x, y].ToBool();
            Levels.Add(new Level(boolLevel, new Point(8, 5), (5, Direction.Right), new Point[] { new(1, 0), new(1, 11), new(8, 0), new(8, 11) }, (new Point(5, 5), Direction.Up),
                blinkyEvents, pinkyEvents, inkyEvents, clydeEvents));
        }

        private void NewGameTool_Click(object sender, EventArgs e)
        {
            Game?.GameEnd();
            Game = new GameTable(Levels[1], new Player.PlayerArgs(3), GameBox.Size, Screen);
            Game.OnGameOver += (sender, e) =>
            {
                MessageBox.Show(e.Score.ToString());
                GameBox.Image.Dispose();
                Game = null;
            };
            Game.GameStart();
            DebugTimer.Start();
        }

        private void DebugTimer_Tick(object sender, EventArgs e)
        {
            DebugBox.Text = $"Game\n{Game?.Debug() ?? ""}\n" +
                $"\nPlayer\n{Game?.Player.Debug() ?? ""}\n" +
                $"\nBlinky\n{Game?.Ghosts[0].Debug()}\n" +
                $"\nPinky\n{Game?.Ghosts[1].Debug()}\n" +
                $"\nInky\n{Game?.Ghosts[2].Debug()}\n" +
                $"\nClyde\n{Game?.Ghosts[3].Debug()}";
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (Game == null)
                return;
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A:
                    if (Game.Started)
                        Game.Player.ChangeDirection(Direction.Left);
                    break;
                case Keys.Up:
                case Keys.W:
                    if (Game.Started)
                        Game.Player.ChangeDirection(Direction.Up);
                    break;
                case Keys.Right:
                case Keys.D:
                    if (Game.Started)
                        Game.Player.ChangeDirection(Direction.Right);
                    break;
                case Keys.Down:
                case Keys.S:
                    if (Game.Started)
                        Game.Player.ChangeDirection(Direction.Down);
                    break;
                case Keys.P:
                    if (Game.Started)
                        Game.GamePause();
                    else
                        Game.GameContinue();
                    break;
                case Keys.Escape:
                    Game.GameEnd();
                    break;
            }
        }

        private void DebugModeTool_Click(object sender, EventArgs e)
        {
            Screen.DebugMode.WallDistance = WallDistanceTool.Checked;
            Screen.DebugMode.GhostGoal = GhostsGoalTool.Checked;
            if (DebugBoxTool.Checked)
            {
                DebugBox.Visible = true;
                DebugTimer.Start();
            }
            else
            {
                DebugBox.Visible = false;
                DebugTimer.Stop();
            }
        }
    }
    public class ScreenBox : IScreen
    {
        private readonly PictureBox Box;

        public ScreenBox(PictureBox box)
        {
            Box = box;
            DebugMode = new();
        }

        public Image Image => Box.Image;

        public void UpdateImage(Image img)
        {
            Box.Image?.Dispose();
            Box.Image = img.Clone() as Image;
        }

        public PacmanDebugMode DebugMode { get; }
        DebugMode IScreen.DebugMode => DebugMode;
    }
    public class PacmanDebugMode : DebugMode
    {
        public bool GhostGoal;
    }
}
