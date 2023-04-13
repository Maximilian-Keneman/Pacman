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
        GameTable Game;
        List<Level> Levels;

        public MainForm()
        {
            InitializeComponent();
            Levels = new List<Level>();
            var intLevel = new (int RW, int DW)[18, 17]
            {
                { (0,0), (0,1), (0,1), (0,0), (0,1), (0,1), (0,1), (1,0), (1,0), (0,0), (0,1), (0,1), (0,1), (0,0), (0,1), (0,1), (1,0) },
                { (1,0), (0,0), (1,0), (1,0), (0,0), (0,0), (1,0), (1,0), (1,0), (1,0), (0,0), (0,0), (1,0), (1,0), (0,0), (1,0), (1,0) },
                { (1,0), (0,1), (1,1), (1,0), (0,1), (0,1), (1,1), (1,0), (1,1), (1,0), (0,1), (0,1), (1,1), (1,0), (0,1), (1,1), (1,0) },
                { (0,0), (0,1), (0,1), (0,0), (0,1), (0,0), (0,1), (0,1), (0,1), (0,1), (0,1), (0,0), (0,1), (0,0), (0,1), (0,1), (1,0) },
                { (1,0), (0,1), (1,1), (1,0), (1,0), (1,0), (0,1), (0,1), (0,0), (0,1), (1,1), (1,0), (1,0), (1,0), (0,1), (1,1), (1,0) },
                { (0,1), (0,1), (0,1), (1,0), (1,0), (0,1), (0,1), (1,0), (1,0), (0,0), (0,1), (1,1), (1,0), (0,0), (0,1), (0,1), (1,1) },
                { (0,0), (0,0), (1,0), (1,0), (0,0), (0,1), (1,1), (1,0), (1,1), (1,0), (0,1), (0,1), (1,0), (1,0), (0,0), (0,0), (1,0) },
                { (0,1), (0,1), (1,1), (1,0), (1,1), (0,0), (0,1), (0,1), (0,1), (0,1), (0,1), (1,0), (1,1), (1,0), (0,1), (0,1), (1,1) },
                { (0,1), (0,1), (0,1), (0,0), (0,1), (1,0), (0,1), (0,1), (0,1), (0,1), (1,1), (0,0), (0,1), (0,0), (0,1), (0,1), (0,1) },
                { (0,0), (0,0), (1,0), (1,0), (1,0), (0,0), (0,1), (0,1), (0,1), (0,1), (0,1), (1,0), (1,0), (1,0), (0,0), (0,0), (1,0) },
                { (0,1), (0,1), (1,1), (1,0), (1,1), (1,0), (0,1), (0,1), (0,0), (0,1), (1,1), (1,0), (1,1), (1,0), (0,1), (0,1), (1,1) },
                { (0,0), (0,1), (0,1), (0,0), (0,1), (0,1), (0,1), (1,0), (1,0), (0,0), (0,1), (0,1), (0,1), (0,0), (0,1), (0,1), (1,0) },
                { (1,0), (0,1), (1,0), (1,0), (0,1), (0,1), (1,1), (1,0), (1,1), (1,0), (0,1), (0,1), (1,1), (1,0), (0,0), (1,1), (1,0) },
                { (0,1), (1,0), (1,0), (0,0), (0,1), (0,0), (0,1), (0,1), (0,1), (0,1), (0,1), (0,0), (0,1), (1,0), (1,0), (0,0), (1,1) },
                { (1,1), (1,0), (1,1), (1,0), (1,0), (1,0), (0,1), (0,1), (0,0), (0,1), (1,1), (1,0), (1,0), (1,0), (1,1), (1,0), (1,1) },
                { (0,0), (0,1), (0,1), (1,1), (1,0), (0,1), (0,1), (1,0), (1,0), (0,0), (0,1), (1,1), (1,0), (0,1), (0,1), (0,1), (1,0) },
                { (1,0), (0,1), (0,1), (0,1), (0,1), (0,1), (1,1), (1,0), (1,1), (1,0), (0,1), (0,1), (0,1), (0,1), (0,1), (1,1), (1,0) },
                { (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (1,1) }
            };
            (bool RW, bool DW)[,] boolLevel = new (bool RW, bool DW)[18, 17];
            for (int x = 0; x < intLevel.GetLength(0); x++)
                for (int y = 0; y < intLevel.GetLength(1); y++)
                    boolLevel[x, y] = intLevel[x, y].ToBool();
            Levels.Add(new Level(boolLevel, new Point(13, 8)));
            intLevel = new (int RW, int DW)[11, 12]
            {
                { (0,0), (0,1), (0,0), (0,1), (0,1), (1,0), (0,0), (0,1), (0,1), (0,0), (0,1), (1,0) },
                { (1,0), (1,1), (1,0), (0,1), (1,1), (1,0), (1,0), (0,1), (1,1), (1,0), (1,1), (1,0) },
                { (0,0), (0,1), (0,0), (0,0), (0,1), (0,1), (0,1), (0,1), (0,0), (0,0), (0,1), (1,0) },
                { (0,1), (0,1), (1,0), (0,1), (0,1), (1,0), (0,0), (0,1), (1,1), (0,0), (0,1), (1,1) },
                { (0,1), (1,1), (1,0), (0,0), (0,1), (0,1), (0,1), (0,1), (1,0), (1,0), (0,1), (1,1) },
                { (0,1), (0,1), (0,0), (1,0), (0,1), (0,1), (0,1), (1,1), (0,0), (0,0), (0,1), (0,1) },
                { (0,1), (1,1), (1,0), (0,0), (0,1), (0,1), (0,1), (0,1), (1,0), (1,0), (0,1), (1,1) },
                { (0,0), (0,1), (0,0), (0,1), (0,1), (1,0), (0,0), (0,1), (0,1), (0,0), (0,1), (1,0) },
                { (0,1), (1,0), (0,0), (0,0), (0,1), (0,1), (0,1), (0,1), (0,0), (1,0), (0,0), (1,1) },
                { (0,0), (0,1), (1,1), (0,1), (0,1), (1,0), (0,0), (0,1), (1,1), (0,1), (0,1), (1,0) },
                { (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (0,1), (1,1) }
            };
            boolLevel = new (bool RW, bool DW)[11, 12];
            for (int x = 0; x < intLevel.GetLength(0); x++)
                for (int y = 0; y < intLevel.GetLength(1); y++)
                    boolLevel[x, y] = intLevel[x, y].ToBool();
            Levels.Add(new Level(boolLevel, new Point(8, 5)));
        }

        private void NewGameTool_Click(object sender, EventArgs e)
        {
            Game?.CheckFinish();
            Game = new GameTable(Levels[1], new Player.PlayerArgs(1), GameBox.Size, new ScreenBox(GameBox));
            Game.OnGameOver += (sender, e) => { GameBox.Image.Dispose(); Game = null; };
            Game.GameStart();
            DebugTimer.Start();
        }

        private void DebugTimer_Tick(object sender, EventArgs e)
        {
            DebugBox.Text = $"Game\n{Game?.Debug() ?? ""}\nPlayer\n{Game?.Player.Debug(Game) ?? ""}";
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
                    Game.CheckFinish();
                    break;
            }
        }
    }
    public class ScreenBox : IScreen
    {
        private readonly PictureBox Box;

        public ScreenBox(PictureBox box) => Box = box;

        public Image Image => Box.Image;

        public void UpdateImage(Image img)
        {
            Box.Image?.Dispose();
            Box.Image = img.Clone() as Image;
        }
    }
}
