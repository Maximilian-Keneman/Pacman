namespace Pacman
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.GameBox = new System.Windows.Forms.PictureBox();
            this.DebugBox = new System.Windows.Forms.RichTextBox();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.NewGameTool = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugModeTool = new System.Windows.Forms.ToolStripMenuItem();
            this.WallDistanceTool = new System.Windows.Forms.ToolStripMenuItem();
            this.GhostsGoalTool = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.DebugBoxTool = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.DebugTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.GameBox)).BeginInit();
            this.MainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // GameBox
            // 
            this.GameBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameBox.Location = new System.Drawing.Point(0, 24);
            this.GameBox.Name = "GameBox";
            this.GameBox.Size = new System.Drawing.Size(625, 426);
            this.GameBox.TabIndex = 0;
            this.GameBox.TabStop = false;
            // 
            // DebugBox
            // 
            this.DebugBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.DebugBox.Enabled = false;
            this.DebugBox.Location = new System.Drawing.Point(625, 24);
            this.DebugBox.Name = "DebugBox";
            this.DebugBox.Size = new System.Drawing.Size(175, 426);
            this.DebugBox.TabIndex = 1;
            this.DebugBox.Text = "";
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewGameTool,
            this.DebugModeTool});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(800, 24);
            this.MainMenu.TabIndex = 2;
            this.MainMenu.Text = "menuStrip1";
            // 
            // NewGameTool
            // 
            this.NewGameTool.Name = "NewGameTool";
            this.NewGameTool.Size = new System.Drawing.Size(77, 20);
            this.NewGameTool.Text = "New Game";
            this.NewGameTool.Click += new System.EventHandler(this.NewGameTool_Click);
            // 
            // DebugModeTool
            // 
            this.DebugModeTool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WallDistanceTool,
            this.GhostsGoalTool,
            this.toolStripSeparator1,
            this.DebugBoxTool,
            this.toolStripSeparator2});
            this.DebugModeTool.Name = "DebugModeTool";
            this.DebugModeTool.Size = new System.Drawing.Size(88, 20);
            this.DebugModeTool.Text = "Debug Mode";
            // 
            // WallDistanceTool
            // 
            this.WallDistanceTool.CheckOnClick = true;
            this.WallDistanceTool.Name = "WallDistanceTool";
            this.WallDistanceTool.Size = new System.Drawing.Size(180, 22);
            this.WallDistanceTool.Text = "Wall distance";
            this.WallDistanceTool.Click += new System.EventHandler(this.DebugModeTool_Click);
            // 
            // GhostsGoalTool
            // 
            this.GhostsGoalTool.CheckOnClick = true;
            this.GhostsGoalTool.Name = "GhostsGoalTool";
            this.GhostsGoalTool.Size = new System.Drawing.Size(180, 22);
            this.GhostsGoalTool.Text = "Ghost\'s goal";
            this.GhostsGoalTool.Click += new System.EventHandler(this.DebugModeTool_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // DebugBoxTool
            // 
            this.DebugBoxTool.CheckOnClick = true;
            this.DebugBoxTool.Name = "DebugBoxTool";
            this.DebugBoxTool.Size = new System.Drawing.Size(180, 22);
            this.DebugBoxTool.Text = "Box";
            this.DebugBoxTool.Click += new System.EventHandler(this.DebugModeTool_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // DebugTimer
            // 
            this.DebugTimer.Tick += new System.EventHandler(this.DebugTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.GameBox);
            this.Controls.Add(this.DebugBox);
            this.Controls.Add(this.MainMenu);
            this.MainMenuStrip = this.MainMenu;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.GameBox)).EndInit();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox GameBox;
        private System.Windows.Forms.RichTextBox DebugBox;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem NewGameTool;
        private System.Windows.Forms.Timer DebugTimer;
        private System.Windows.Forms.ToolStripMenuItem DebugModeTool;
        private System.Windows.Forms.ToolStripMenuItem WallDistanceTool;
        private System.Windows.Forms.ToolStripMenuItem GhostsGoalTool;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem DebugBoxTool;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

