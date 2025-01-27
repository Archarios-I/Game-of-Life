using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Graphics graphics;
        private int resolution;   //разрешение(маштаб)
        private int rows;         //строки
        private int columns;     //столбцы
        private bool[,] field;   //массив координат, позже в нем же будут данные жива клетка или нет                 
        uint currentGen;

        public Form1()
        {
            InitializeComponent();
        }

        private readonly Random random = new Random();

        private void StartGame() // первая генерация карты
        {
            if (newGenTimer.Enabled)
                return;

            currentGen = 0;

            nudResol.Enabled = false; // выключаются кнопки для изменения плотности населения и разрешения
            nudDen.Enabled = false;
            resolution = (byte)nudResol.Value; // разрешение записывается в переменную

            rows = pictureBox2.Height / resolution; // расчет того сколько клеток на поле
            columns = pictureBox2.Width / resolution;
            field = new bool[columns, rows];

            byte densityThreshold = (byte)nudDen.Value; // Сохраняем значение плотности в переменной

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    field[x, y] = random.Next(densityThreshold) == 0; // рандомные числа чтобы клетки были живыми или мертвыми
                }
            }

            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            graphics = Graphics.FromImage(pictureBox2.Image);
            newGenTimer.Start();
        }

        private void NextGen() // отрисовка
        {
            graphics.Clear(Color.Black);
            Text = $"Поколение: {++currentGen}";

            // Создаем временный массив для обновления состояния
            bool[,] newField = new bool[columns, rows];

            Parallel.For(0, columns, x =>
            {
                for (int y = 0; y < rows; y++)
                {
                    byte neighboursCount = CountNeighbours(x, y);
                    bool hasLife = field[x, y];

                    // Логика для определения состояния клетки в следующем поколении
                    if (!hasLife && neighboursCount == 3)
                    {
                        newField[x, y] = true; // Рождается новая жизнь
                    }
                    else if (hasLife && (neighboursCount < 2 || neighboursCount > 3))
                    {
                        newField[x, y] = false; // Клетка умирает
                    }
                    else
                    {
                        newField[x, y] = hasLife; // Сохраняем текущее состояние
                    }
                }
            });

            // Обновляем основное поле
            field = newField;

            // Отрисовка клеток
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (field[x, y]) // Если клетка живая
                    {
                        graphics.FillRectangle(Brushes.Green, x * resolution, y * resolution, resolution - 1, resolution - 1);
                    }
                }
            }

            // Обновление изображения
            pictureBox2.Refresh();
        }


        private byte CountNeighbours(int x, int y)
        {
            byte count = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int col = (x + i + columns) % columns;
                    int row = (y + j + rows) % rows;
                    bool isSelfChecking = col == x && row == y;
                    bool hasLife = field[col, row];
                    if (hasLife && !isSelfChecking)
                        count++;
                }
            }

            return count;
        }

        private void StopGame()
        {
            if (!newGenTimer.Enabled)
            {
                return;
            }
            else
            {
                newGenTimer.Stop();
                nudResol.Enabled = true; //включаются кнопки для изменения плотности населения и разрешения
                nudDen.Enabled = true;
            }
        }

        private void bStop_Click(object sender, EventArgs e) //кнопка стоп
        {
            StopGame();
        }

        private void NewGenTimer_Tick(object sender, EventArgs e) //тик таймера каждые пол секунды
        {
            NextGen();
        }

        private void bStart_Click(object sender, EventArgs e) //кнопка старт
        {
            StartGame();
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (!newGenTimer.Enabled)
                return;

            // Вычисляем координаты клетки
            int x = e.Location.X / resolution;
            int y = e.Location.Y / resolution;

            // Проверяем валидность координат
            if (!ValidateMousePos(x, y))
                return;

            // убить или родить
            switch (e.Button)
            {
                case MouseButtons.Left:
                    field[x, y] = true; // Жизнь
                    break;
                case MouseButtons.Right:
                    field[x, y] = false; // Смерть
                    break;
            }
        }

        private bool ValidateMousePos(int x, int y)
        {
            return x >= 0 && y >= 0 && x < columns && y < rows;
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            newGenTimer.Interval = (int)numericUpDown1.Value;
        }
    }
}