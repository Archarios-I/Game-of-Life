using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        #region переменные
        private Graphics graphics;
        private int resolution;   //разрешение(маштаб)
        private int rows;         //строки
        private int columns;     //столбцы
        private bool[,] field;   //массив координат, позже в нем же будут данные жива клетка или нет                 
        uint currentGen;
        private int seconds = 0;
        private int minutes = 0;
        private int hours = 0;
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        private void StartGame()            //старт
        {
            if (newGenTimer.Enabled)
                return;

            currentGen = 0;


            nudResol.Enabled = false;                      //выключаются кнопки для изменения плотности населения и разрешения
            nudDen.Enabled = false;
            resolution = (int)nudResol.Value;            //разрешение записывается в переменную

            rows = pictureBox2.Height / resolution;              //расчет того сколько клеток на поле
            columns = pictureBox2.Width / resolution;
            field = new bool[columns, rows];

            Random random = new Random();
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    field[x, y] = random.Next((int)nudDen.Value) == 0;      //рандомные числа чтобы клетки были живыми или мертвыми
                }
            }

            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            graphics = Graphics.FromImage(pictureBox2.Image);
            newGenTimer.Start();
        }

        private void NextGen()         //поколения
        {
            graphics.Clear(Color.Black);
            Text = $"Поколение + {++currentGen}";

            var newField = new bool[columns, rows];

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var neighboursCount = countNeighbours(x, y);
                    var hasLife = field[x, y];

                    if (!hasLife && neighboursCount == 3)
                    {
                        newField[x, y] = true;
                    }
                    else if (hasLife && (neighboursCount < 2 || neighboursCount > 3))
                    {
                        newField[x, y] = false;
                    }
                    else
                    {
                        newField[x, y] = field[x, y];
                    }

                    if (hasLife)
                    {
                        graphics.FillRectangle(Brushes.Crimson, x * resolution, y * resolution, resolution - 1, resolution - 1);
                    }
                }
            }
            field = newField;
            pictureBox2.Refresh();         //перезагрузка картики
        }

        private int countNeighbours(int x, int y) //соседи
        {
            int count = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    var col = (x + i + columns) % columns;
                    var row = (y + j + rows) % rows;
                    var isSelfChecking = col == x && row == y;
                    var hasLife = field[col, row];
                    if (hasLife && !isSelfChecking)
                        count++;
                }
            }

            return count;
        }

        private void StopGame()       //стоп
        {
            if (!newGenTimer.Enabled)
            {
                return;
            }
            else
            {
                newGenTimer.Stop();
                nudResol.Enabled = true;                      //включаются кнопки для изменения плотности населения и разрешения
                nudDen.Enabled = true;
            }
        }

        private void bStop_Click(object sender, EventArgs e)        //кнопка стоп
        {
            gameTimer.Stop();
            StopGame();
        }

        private void newGenTimer_Tick(object sender, EventArgs e)  //таймер поколений
        {
            NextGen();
        }

        private void bStart_Click(object sender, EventArgs e)     //кнопка старт
        {
            gameTimer.Start();
            StartGame();
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)  //давайте рисовать!
        {
            if (!newGenTimer.Enabled)
                return;

            if (e.Button == MouseButtons.Left)
            {
                var x = e.Location.X / resolution;
                var y = e.Location.Y / resolution;
                var validationPassed = ValidateMousePos(x, y);
                if (validationPassed)
                    field[x, y] = true;
            }

            if (e.Button == MouseButtons.Right)
            {
                var x = e.Location.X / resolution;
                var y = e.Location.Y / resolution;
                var validationPassed = ValidateMousePos(x, y);
                if (validationPassed)
                    field[x, y] = false;
            }
        }

        private bool ValidateMousePos(int x, int y)
        {
            return x >= 0 && y >= 0 && x < columns && y < rows;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            newGenTimer.Interval = (int)numericUpDown1.Value;
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            seconds++;
            if (seconds == 60)
            {
                seconds = 0;
                minutes++;
                if (minutes == 60)
                {
                    minutes = 0;
                    hours++;
                }
            }
            labelTime.Text = hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }
    }
}
