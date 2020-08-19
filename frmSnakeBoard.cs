using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class frmSnakeBoard : Form
    {
        #region Enum
        private enum eLevelGame
        {
            BeginnerMode = 0,
            MediumMode = 1,
            ExpertMode = 2,
        }
        private enum eSnakePostion
        {
            Left,
            Right,
            Up,
            Down

        };
        #endregion

        #region Consts
        private const string c_BoomImageName = "Boom.png";
        private const string c_SnakeImageName = "HeadSnake.png";
        private const string c_AppleImageName = "Apple.png";
        private const string c_ImageFolderName = "image";

        private const int c_Begginer_Boom_Interval = 7000;
        private const int c_Medium_Boom_Interval = 3000;
        private const int c_Expert_Boom_Interval = 1000;

        private const int c_Begginer_KeepMove_Interval = 300;
        private const int c_Medium_KeepMove_Interval = 100;
        private const int c_Expert_KeepMove_Interval = 50;

        #endregion

        #region Members
        private List<PictureBox> AppleList;
        private List<PictureBox> BoombList;
        private PictureBox Snake;
        private Image SnakeImageRight;
        private Image SnakeImageLeft;
        private Image SnakeImageUp;
        private Image SnakeImageDown;
        private eSnakePostion m_CurrentPosttion = eSnakePostion.Right;
        private DateTime m_StartTime;
        private Timer m_UpdateTimeLblTimer;
        private Timer m_KeepMoveTimer;
        private Timer m_SetAppleOnBoardTimer;
        private Timer m_SetBoombsOnBoardTimer;
        private bool m_IsPlayGame = true;
        private int m_Score = 0;
        private eLevelGame m_SpeedGame = eLevelGame.BeginnerMode;
        private Point m_SnakeStartPoint;

        #endregion

        #region Init
        public frmSnakeBoard(string NamePlayer)
        {
            InitializeComponent();
            InitComboBoxLevel();

            m_SnakeStartPoint = new Point(350, 150);
            lblNameValue.Text = NamePlayer;
            this.KeyPress += MoveHandler;
            btnNewGame.Click += BtnNewGame_Click;

            m_UpdateTimeLblTimer = new Timer();
            m_UpdateTimeLblTimer.Interval = 1000;
            m_UpdateTimeLblTimer.Tick += (sender, e) =>
            {
                lblTimerValue.Text = GetGameTime();
            };// lambda expression with action 


            m_KeepMoveTimer = new Timer();
            m_KeepMoveTimer.Interval = GetKeepMovingInterval();
            m_KeepMoveTimer.Tick += (sender, e) =>
            {
                KeepMoving(m_IsPlayGame);
            };// lambda expression with action 

            m_SetAppleOnBoardTimer = new Timer();
            m_SetAppleOnBoardTimer.Interval = 10000;
            m_SetAppleOnBoardTimer.Tick += (sender, e) =>
            {
                AddItemToPnl(m_IsPlayGame, c_AppleImageName, AppleList);
            };// lambda expression with action 

            m_SetBoombsOnBoardTimer = new Timer();
            m_SetBoombsOnBoardTimer.Interval = GetBoombInterval();
            m_SetBoombsOnBoardTimer.Tick += (sender, e) =>
            {
                AddItemToPnl(m_IsPlayGame, c_BoomImageName, BoombList);
            };

            StartGame();
        }
        private void StartGame()
        {
            InitImage(); 
            m_IsPlayGame = true;
            m_StartTime = default;
            m_Score = 0;
            AppleList = new List<PictureBox>();
            BoombList = new List<PictureBox>();
            Snake.Location = m_SnakeStartPoint; // Snake start location
            lblScoreValue.Text = m_Score.ToString();

            //Init interval by level game
            m_SetBoombsOnBoardTimer.Interval = GetBoombInterval();
            m_KeepMoveTimer.Interval = GetKeepMovingInterval();

        }
        private void InitComboBoxLevel()
        {
            cbLevel.Items.AddRange(Enum.GetNames(typeof(eLevelGame)));
            cbLevel.DropDownStyle = ComboBoxStyle.DropDownList;

            cbLevel.MaxDropDownItems = 3;
            cbLevel.TabIndex = 0;
            cbLevel.SelectedIndex = (int)eLevelGame.BeginnerMode;
            cbLevel.BackColor = System.Drawing.Color.BlanchedAlmond;
            cbLevel.ForeColor = System.Drawing.Color.Black;
            cbLevel.SelectedIndexChanged += CbLevel_SelectedIndexChanged;

        }

        private void InitImage()
        {
            if (Snake != null)
                return;

            Snake = new PictureBox();
            SnakeImageRight = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c_ImageFolderName, c_SnakeImageName));
            SnakeImageLeft = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c_ImageFolderName, c_SnakeImageName));
            SnakeImageUp = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c_ImageFolderName, c_SnakeImageName));
            SnakeImageDown = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c_ImageFolderName, c_SnakeImageName));
            Snake.Image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c_ImageFolderName, c_SnakeImageName));
            Snake.Size = new Size(40, 40);
            Snake.Location = m_SnakeStartPoint;
            Snake.SizeMode = PictureBoxSizeMode.StretchImage;

            SnakeImageLeft = RotateImage(eSnakePostion.Left, SnakeImageLeft);
            SnakeImageUp = RotateImage(eSnakePostion.Up, SnakeImageUp);
            SnakeImageDown = RotateImage(eSnakePostion.Down, SnakeImageDown);
            pnlBoard.Controls.Add(Snake);
        }

        #endregion


        #region EventListner
        /// <summary>
        /// Event on new game click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            RemoveAllItemsFromBoard();
            StopTimer();
            StartGame();
        }

        /// <summary>
        /// Event On key down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveHandler(object sender, KeyPressEventArgs e)
        {
            if (!m_IsPlayGame)
                return;

            var isCurrectKey = false;

            if ((e.KeyChar == 'D' || e.KeyChar == 'd') && (m_CurrentPosttion != eSnakePostion.Left))
            {
                MoveRight(m_IsPlayGame);
                m_CurrentPosttion = eSnakePostion.Right;
                isCurrectKey = true;
            }
            if ((e.KeyChar == 'A' || e.KeyChar == 'a') && (m_CurrentPosttion != eSnakePostion.Right))
            {
                MoveLeft(m_IsPlayGame);
                m_CurrentPosttion = eSnakePostion.Left;
                isCurrectKey = true;
            }
            if ((e.KeyChar == 'W' || e.KeyChar == 'w') && (m_CurrentPosttion != eSnakePostion.Down))
            {
                MoveUp(m_IsPlayGame);
                m_CurrentPosttion = eSnakePostion.Up;
                isCurrectKey = true;
            }
            if ((e.KeyChar == 'X' || e.KeyChar == 'x') && (m_CurrentPosttion != eSnakePostion.Up))
            {
                MoveDown(m_IsPlayGame);
                m_CurrentPosttion = eSnakePostion.Down;
                isCurrectKey = true;
            }

            // ======= START GAME on click========
            if (isCurrectKey && m_StartTime == default) // defualt == Date time null
            {
                m_StartTime = DateTime.Now;
                m_KeepMoveTimer.Start();
                m_SetAppleOnBoardTimer.Start();
                m_SetBoombsOnBoardTimer.Start();
                m_UpdateTimeLblTimer.Start();
            }
        }
        
        /// <summary>
        /// Event On combobox index change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            int selectedIndex = cb.SelectedIndex;
            m_SpeedGame = (eLevelGame)selectedIndex;

            RemoveAllItemsFromBoard();
            StopTimer();
            StartGame();
        }

        #endregion

        /// <summary>
        /// This func return timer iterval for boomb timer by game level
        /// </summary>
        private int GetBoombInterval()
        {
            var interval = c_Begginer_Boom_Interval;
            switch (m_SpeedGame)
            {
                case eLevelGame.BeginnerMode:
                    interval= c_Begginer_Boom_Interval;
                    break;
                case eLevelGame.MediumMode:
                    interval = c_Medium_Boom_Interval;
                    break;
                case eLevelGame.ExpertMode:
                    interval = c_Expert_Boom_Interval;
                    break;
            }

            return interval;
        }
        
        /// <summary>
        ///This func return keep moving inerval for Snake by game level 
        /// </summary>
        /// <returns></returns>
        private int GetKeepMovingInterval()
        {
            var interval = c_Begginer_KeepMove_Interval;
            switch (m_SpeedGame)
            {
                case eLevelGame.BeginnerMode:
                    interval = c_Begginer_KeepMove_Interval;
                    break;
                case eLevelGame.MediumMode:
                    interval = c_Medium_KeepMove_Interval;
                    break;
                case eLevelGame.ExpertMode:
                    interval = c_Expert_KeepMove_Interval;
                    break;
            }

            return interval;
        }

        /// <summary>
        /// Do action when game is over
        /// </summary>
        private void GameOver()
        {
            StopTimer();

            DialogResult result = MessageBox.Show("Your Score " + m_Score + "\nDo you want to play again?", "Game Over", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {

                RemoveAllItemsFromBoard();
                StartGame();
            }
            else
            {
                m_KeepMoveTimer.Stop();
                m_SetAppleOnBoardTimer.Stop();
                m_UpdateTimeLblTimer.Stop();
                m_IsPlayGame = false;
            }
        }

        /// <summary>
        /// func stop all timers
        /// </summary>
        private void StopTimer()
        {
            m_UpdateTimeLblTimer.Stop();
            m_KeepMoveTimer.Stop();
            m_SetAppleOnBoardTimer.Stop();
            m_SetBoombsOnBoardTimer.Stop();

            m_StartTime = default;
        }

        /// <summary>
        /// Return String of game timer by pattern
        /// </summary>
        private string GetGameTime()
        {
            if (m_StartTime == default)
                return "00:00:00";
            return (DateTime.Now - m_StartTime).ToString(@"hh\:mm\:ss");

        }
        /// <summary>
        /// Func that called from timer tick
        /// Moveing the snake in current direction
        /// </summary>
        /// <param name="m_IsPlayGame"></param>
        private void KeepMoving(bool m_IsPlayGame)
        {
            if (!m_IsPlayGame)
                return;

            switch (m_CurrentPosttion)
            {
                case eSnakePostion.Left:
                    MoveLeft(m_IsPlayGame);
                    break;
                case eSnakePostion.Right:
                    MoveRight(m_IsPlayGame);
                    break;
                case eSnakePostion.Up:
                    MoveUp(m_IsPlayGame);
                    break;
                case eSnakePostion.Down:
                    MoveDown(m_IsPlayGame);
                    break;
            }
        }
        /// <summary>
        /// Check if snake is on an apple and remove it from on the board if needed
        /// </summary>
        private void RemoveAppleFromPnl()
        {
            PictureBox AppleToRemove = null;
            var cloneAppleList = AppleList.ToList();
            foreach (var Apple in cloneAppleList)
            {
                if (IsSnakeOnItem(Apple.Location.X, Apple.Location.Y))
                {
                    AppleToRemove = Apple;
                    break;
                }
            }
            if (AppleToRemove == null)
                return;
            //Stop timer in order to prevent removing an apple while timer is adding apples to the same list
            m_SetAppleOnBoardTimer.Stop(); 
            AppleList.Remove(AppleToRemove);
            pnlBoard.Controls.Remove(AppleToRemove);
            lblScoreValue.Text = (++m_Score).ToString();
            m_SetAppleOnBoardTimer.Start();
        }
        /// <summary>
        /// Return if snake is on boomb
        /// </summary>
        /// <returns></returns>
        private bool IsSnakeOnBoomb()
        {
            var cloneBoomList = BoombList.ToList();
            foreach (var boom in cloneBoomList)
            {
                if (IsSnakeOnItem(boom.Location.X, boom.Location.Y))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get new location to snake , check if game is over else set snake new location
        /// </summary>
        /// <param name="point"></param>
        private bool SetSnakeLocation(Point point)
        {
            if (IsSnakeOnBoomb() || !IsInFormBound(point.X, point.Y))
            {
                GameOver();
                return false;
            }

            Snake.Location = point;
            return true;
        }
        /// <summary>
        /// func get image path and find new point for it with out any items and add it to panel board 
        /// and to the list of items
        /// </summary>
        /// <param name="m_IsPlayGame"></param>
        /// <param name="imageToAdd"></param>
        /// <param name="listToAdd"></param>
        private void AddItemToPnl(bool m_IsPlayGame,string imageToAdd,List<PictureBox> listToAdd)
        {
            if (!m_IsPlayGame)
                return;


            var rnd = new Random();
            var pointX = rnd.Next(0, pnlBoard.Width);
            var pointY = rnd.Next(0, pnlBoard.Height);

            var allItems = AppleList.ToList();
            allItems.AddRange(BoombList);
            //TODO: Check x and y is not on snake or another apples
            foreach (var itemOnBoard in allItems)
            {
                if (itemOnBoard.Location.X == pointX || itemOnBoard.Location.Y == pointY || IsInFormBound(pointX, pointY))
                {
                    pointX = rnd.Next(0, pnlBoard.Width);
                    pointY = rnd.Next(0, pnlBoard.Height);
                }

            }
            var ImageIcon = new PictureBox();
            ImageIcon.Image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, c_ImageFolderName, imageToAdd));

            ImageIcon.Size = new Size(25, 25);
            ImageIcon.Location = new Point(pointX, pointY);
            ImageIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            listToAdd.Add(ImageIcon);
            pnlBoard.Controls.Add(ImageIcon);

        }
        /// <summary>
        ///  return if snake on given x,y 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsSnakeOnItem(int x, int y)
        {
            if (x > Snake.Location.X &&
               x < Snake.Location.X + Snake.Size.Width &&
               y > Snake.Location.Y &&
               y < Snake.Location.Y + Snake.Size.Height)
                return true;

            return false;
        }

        private void MoveDown(bool m_IsPlayGame)
        {
            if (!m_IsPlayGame)
                return;

            var isSuccess = SetSnakeLocation(new Point(Snake.Location.X, Snake.Location.Y + 5));
            if (!isSuccess)
                return;

            if (m_CurrentPosttion != eSnakePostion.Down)
                Snake.Image = SnakeImageDown;

            RemoveAppleFromPnl();

        }

        private void MoveUp(bool m_IsPlayGame)
        {
            if (!m_IsPlayGame)
                return;

            var isSuccess = SetSnakeLocation(new Point(Snake.Location.X, Snake.Location.Y - 5));
            if (!isSuccess)
                return;

            if (m_CurrentPosttion != eSnakePostion.Up)
            {
                Snake.Image = SnakeImageUp;
            }
            RemoveAppleFromPnl();

        }

        private void MoveLeft(bool m_IsPlayGame)
        {
            if (!m_IsPlayGame)
                return;

            var isSuccess = SetSnakeLocation(new Point(Snake.Location.X - 5, Snake.Location.Y));
            if (!isSuccess)
                return;

            if (m_CurrentPosttion != eSnakePostion.Left)
            {
                Snake.Image = SnakeImageLeft;
            }


            RemoveAppleFromPnl();

        }

        private void MoveRight(bool m_IsPlayGame)
        {
            if (!m_IsPlayGame)
                return;

            var isSuccess = SetSnakeLocation(new Point(Snake.Location.X + 5, Snake.Location.Y));
            if (!isSuccess)
                return;

            if (m_CurrentPosttion != eSnakePostion.Right)
            {
                Snake.Image = SnakeImageRight;
            }
            RemoveAppleFromPnl();


        }

        /// <summary>
        /// check if point is in the board panel bounds
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsInFormBound(int x, int y)
        {
            return x < pnlBoard.Width && x > 0 &&
                    y < pnlBoard.Height && y > 0;
        }

        /// <summary>
        /// func get postion and return a rotated image
        /// </summary>
        /// <param name="newPosstion"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        private Image RotateImage(eSnakePostion newPosstion, Image img)
        {
            Image flipImage = img;
            switch (newPosstion)
            {
                case eSnakePostion.Left:
                    flipImage.RotateFlip(RotateFlipType.Rotate180FlipY);

                    break;
                case eSnakePostion.Right:
                    break;
                case eSnakePostion.Up:
                    flipImage.RotateFlip(RotateFlipType.Rotate90FlipXY);

                    break;
                case eSnakePostion.Down:
                    flipImage.RotateFlip(RotateFlipType.Rotate270FlipY);

                    break;
            }
            return flipImage;

        }

        /// <summary>
        /// Remove all boombs and apple from board
        /// </summary>
        private void RemoveAllItemsFromBoard()
        {
            foreach (var apple in AppleList)
            {
                pnlBoard.Controls.Remove(apple);
            }

            foreach (var boomb in BoombList)
            {
                pnlBoard.Controls.Remove(boomb);
            }
        }

    }

}