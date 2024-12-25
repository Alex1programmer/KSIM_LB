using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;




namespace muraschka_w
{
    public partial class Form1 : Form
    {
        private int[,] matrix_buf;
        private bool flag_ant=false, poprav = false;
        // Параметри алгоритму
        private double alpha;                //1.0; 
        private double beta;                 // 2.0;  Вплив зони досяжності
        private double tau0;                //= 1.0Початкове значенне феромона
        private double evaporationRate;      //0.5;  Коєфіцієнт випаровування феромона
        private double Q;                   //= 100 Константа для обновлення феромона
                                            // Результат обчислень
        private int bestLength = int.MaxValue;
        private int[] bestRoute;
        public Form1()
        {
            InitializeComponent();
            this.Resize += Form1_Resize;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
            //pictureBoxGraph.Invalidate();
            if (matrix_buf != null) 
            {
                if (!flag_ant)
                    DisplayGraph(matrix_buf);
                else
                {
                    if (!poprav)
                    {
                        poprav = true;
                        for (int itk = 0; itk < bestRoute.Length; itk++)
                            bestRoute[itk] -= 1;
                    }

                    DisplayAntsPath1(matrix_buf, bestRoute, bestRoute.Length); 
                }
                
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            label3.Visible = false;
            textBox2.Visible = false;
            label2.Visible = false;
            textBox1.Visible = false;
            button2.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    GenerateSymmetricMatrix();
                    break;
                case 1:
                    ReadMatrixFromFile();
                    break;
                case 2:
                    OpenMatrixInputForm();
                    break;
                default:
                    MessageBox.Show("Будь ласка, оберіть варіант з випадаючого списку.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }



        // ====================== генерація матриці випадкових чисел ======================

        private void GenerateSymmetricMatrix()
        {
            if (int.TryParse(textBox1.Text, out int n) && n > 0)
            {
                matrix_buf = new int[n, n];
                // int[,] matrix = new int[n, n];
                Random random = new Random();

                for (int i = 0; i < n; i++)
                {
                    for (int j = i; j < n; j++)
                    {
                        int value = random.Next(1, 100); // Рандомне значення

                        if (i == j)
                        {
                            //  matrix[i, j] = 0; 
                            matrix_buf[i, j] = 0;
                        }
                        else
                        {
                            //  matrix[i, j] = value;
                            //  matrix[j, i] = value;

                            matrix_buf[i, j] = value;
                            matrix_buf[j, i] = value;
                        }
                    }
                }

                DisplayMatrix(matrix_buf);
                flag_ant = false;
            }
            else
            {
                MessageBox.Show("Некоректне значення розміру матриці.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //===============================  Виведення матриці ========================================
        /*       private void DisplayMatrix(int[,] matrix)
               {
                   string result = "";
                   int n = matrix.GetLength(0);

                   for (int i = 0; i < n; i++)
                   {
                       for (int j = 0; j < n; j++)
                       {
                           result += matrix[i, j] + "\t";
                       }
                       result += "\n";
                   }

                   MessageBox.Show(result, "Матриця", MessageBoxButtons.OK, MessageBoxIcon.Information);
               } */

        //**********************************************************************************************

        //============================= Зчитування матриці з файлу ============================

        private void ReadMatrixFromFile()
        {
            string filePath = textBox2.Text;

            if (File.Exists(filePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    int n = lines.Length;
                    matrix_buf = new int[n, n];

                    for (int i = 0; i < n; i++)
                    {
                        string[] elements = lines[i].Split(' ');
                        for (int j = 0; j < n; j++)
                        {
                            matrix_buf[i, j] = int.Parse(elements[j]);
                        }
                    }

                    DisplayMatrix(matrix_buf);
                    flag_ant = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при зчитуванні файлу: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Файл не знайдено.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //=============================== Введення з клавіатури =============================
        private void OpenMatrixInputForm()
        {
            if (int.TryParse(textBox1.Text, out int n) && n > 0)
            {
                Form inputForm = new Form
                {
                    Text = "Введення матриці",
                    Size = new System.Drawing.Size(400, 400)
                };

                DataGridView gridView = new DataGridView
                {
                    ColumnCount = n,
                    RowCount = n,
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                Button saveButton = new Button
                {
                    Text = "Зберегти",
                    Dock = DockStyle.Bottom
                };

                saveButton.Click += (s, e) =>
                {
                    matrix_buf = new int[n, n];

                    try
                    {
                        for (int i = 0; i < n; i++)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                matrix_buf[i, j] = int.Parse(gridView[j, i].Value?.ToString() ?? "0");
                            }
                        }

                        DisplayMatrix(matrix_buf);
                        flag_ant = false;
                        inputForm.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Помилка введення даних.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                inputForm.Controls.Add(gridView);
                inputForm.Controls.Add(saveButton);
                inputForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Некоректне значення розміру матриці.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        //==============================================================================

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void maskedTextBoxERate_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: // Якщо обрано 
                    label2.Visible = true;
                    textBox1.Visible = true;
                    label3.Visible = false;
                    textBox2.Visible = false;
                    button2.Visible = true;
                    break;

                case 1: // Якщо обрано 
                    label3.Visible = true;
                    textBox2.Visible = true;
                    label2.Visible = false;
                    textBox1.Visible = false;
                    button2.Visible = true;
                    break;
                case 2: // Якщо обрано 
                    label2.Visible = true;
                    textBox1.Visible = true;
                    label3.Visible = false;
                    textBox2.Visible = false;
                    button2.Visible = true;
                    break;

                default:
                    label3.Visible = false;
                    textBox2.Visible = false;
                    label2.Visible = false;
                    textBox1.Visible = false;
                    button2.Visible = false;

                    break;

            }
        }

        //Ініціалізація параметрів алгоритму
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Зчитування параметрів з TextBox
                alpha = double.Parse(textBox3.Text);
                beta = double.Parse(textBox4.Text);
                tau0 = double.Parse(textBox5.Text);
                evaporationRate = double.Parse(textBox6.Text);
                Q = double.Parse(textBox7.Text);

                MessageBox.Show("Параметри збережені успішно.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("Некоректний формат введених параметрів. Будь ласка, перевірте введені значення.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //====================================================================================

        private void DisplayMatrix(int[,] matrix)
        {
            int n = matrix.GetLength(0);

            // Видаляємо попередній DataGridView, якщо він існує
            var existingGrid = Controls["matrixGrid"];
            if (existingGrid != null)
            {
                Controls.Remove(existingGrid);
            }

            // Створення нового DataGridView
            DataGridView gridView = new DataGridView
            {
                Name = "matrixGrid",
                /*                ColumnCount = n,
                                RowCount = n,
                                Dock = DockStyle.Bottom,
                                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                                Height = 200*/

                //**
                ColumnCount = n,
                RowCount = n,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Size = new System.Drawing.Size(600, 200), // Розміри таблиці
                Location = new System.Drawing.Point(250, 10), // Координати X=500, Y=30
                                                              //**

            };

            // Заповнення DataGridView даними матриці
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {

                    gridView[j, i].Value = matrix[i, j];



                }
            }

            // Додаємо DataGridView на форму
            Controls.Add(gridView);

            // Відображення графу
            DisplayGraph(matrix);
        }


        private void DisplayAntsPath1(int[,] matrix, int[] path, int size)
        {
            if (pictureBoxGraph.Image != null)
            {
                pictureBoxGraph.Image.Dispose();
            }

            int n = matrix.GetLength(0); // Кількість вузлів
            int padding = 20; // Відступи для країв
            int graphWidth = pictureBoxGraph.Width - 2 * padding;
            int graphHeight = pictureBoxGraph.Height - 2 * padding;
            int radius = Math.Min(graphWidth, graphHeight) / 2 - 10; // Радіус кола для вузлів
            int centerX = padding + graphWidth / 2;
            int centerY = padding + graphHeight / 2;

            Bitmap bitmap = new Bitmap(pictureBoxGraph.Width, pictureBoxGraph.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);

            // Координати вузлів
            Point[] nodePositions = new Point[n];
            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));
                nodePositions[i] = new Point(x, y);

                // Малюємо вузли
                graphics.FillEllipse(Brushes.LightBlue, x - 15, y - 15, 30, 30);
                graphics.DrawEllipse(Pens.Black, x - 15, y - 15, 30, 30);

                // Підписуємо вузли
                graphics.DrawString((i + 1).ToString(), DefaultFont, Brushes.Black, x - 10, y - 10);
            }

            // Малювання ребер
           /* for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (matrix[i, j] > 0) // Якщо є зв’язок
                    {
                        Point start = nodePositions[i];
                        Point end = nodePositions[j];
                        graphics.DrawLine(Pens.Gray, start, end);

                        // Відображення ваги ребра
                        int midX = (start.X + end.X) / 2;
                        int midY = (start.Y + end.Y) / 2;
                        graphics.DrawString(matrix[i, j].ToString(), DefaultFont, Brushes.Black, midX, midY);
                    }
                }
            }*/

            // Малювання шляху
            for (int i = 0; i < size - 1; i++)
            {

                Point start = nodePositions[path[i]];
                Point end;


                if ((i + 1) == size - 1)
                    end = nodePositions[path[0]];
                else
                    end = nodePositions[path[i + 1]];

                graphics.DrawLine(Pens.Red, start, end);
            }

            pictureBoxGraph.Image = bitmap;
        }
        private void DisplayAntsPath(int[,] matrix, int[] path, int size)
        {
            if (pictureBoxGraph.Image != null)
            {
               // pictureBoxGraph.Image.Dispose();
            }

            int n = matrix.GetLength(0); // Кількість вузлів
            //n -= 1;
            int padding = 20; // Відступи для країв
            int graphWidth = pictureBoxGraph.Width - 2 * padding;
            int graphHeight = pictureBoxGraph.Height - 2 * padding;
            int radius = Math.Min(graphWidth, graphHeight) / 2 - 10; // Радіус кола для вузлів
            int centerX = padding + graphWidth / 2;
            int centerY = padding + graphHeight / 2;

            Bitmap bitmap = new Bitmap(pictureBoxGraph.Width, pictureBoxGraph.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
           // graphics.Clear(Color.White);

            // Координати вузлів
            Point[] nodePositions = new Point[n];
            for (int i = 0; i < n; i++)
            {
                 double angle = 2 * Math.PI * i / n;
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));
                nodePositions[i] = new Point(x, y);

                // Малюємо вузли
               // graphics.FillEllipse(Brushes.LightBlue, x - 15, y - 15, 30, 30);
               // graphics.DrawEllipse(Pens.Black, x - 15, y - 15, 30, 30);

                // Підписуємо вузли
               // graphics.DrawString((i + 1).ToString(), DefaultFont, Brushes.Black, x - 10, y - 10);
            }

            // Малювання шляху
            for (int i = 0; i < size-1; i++)
            {
                    
                Point start = nodePositions[i];
                Point end;


                if ((i+1)==size-1)
                    end = nodePositions[path[0]];
                else
                    end = nodePositions[path[i+1]];
                       
                graphics.DrawLine(Pens.Red, start, end);
            }

            pictureBoxGraph.Image = bitmap;
        }

        private void DisplayGraph(int[,] matrix)
        {
            if (pictureBoxGraph.Image != null)
            {
                pictureBoxGraph.Image.Dispose();
            }

            int n = matrix.GetLength(0); // Кількість вузлів
            int padding = 20; // Відступи для країв
            int graphWidth = pictureBoxGraph.Width - 2 * padding;
            int graphHeight = pictureBoxGraph.Height - 2 * padding;
            int radius = Math.Min(graphWidth, graphHeight) / 2 - 10; // Радіус кола для вузлів
            int centerX = padding + graphWidth / 2;
            int centerY = padding + graphHeight / 2;

            Bitmap bitmap = new Bitmap(pictureBoxGraph.Width, pictureBoxGraph.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);

            // Координати вузлів
            Point[] nodePositions = new Point[n];
            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;
                int x = centerX + (int)(radius * Math.Cos(angle));
                int y = centerY + (int)(radius * Math.Sin(angle));
                nodePositions[i] = new Point(x, y);

                // Малюємо вузли
                graphics.FillEllipse(Brushes.LightBlue, x - 15, y - 15, 30, 30);
                graphics.DrawEllipse(Pens.Black, x - 15, y - 15, 30, 30);

                // Підписуємо вузли
                graphics.DrawString((i + 1).ToString(), DefaultFont, Brushes.Black, x - 10, y - 10);
            }

            // Малювання ребер
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (matrix[i, j] > 0) // Якщо є зв’язок
                    {
                        Point start = nodePositions[i];
                        Point end = nodePositions[j];
                        graphics.DrawLine(Pens.Gray, start, end);

                        // Відображення ваги ребра
                        int midX = (start.X + end.X) / 2;
                        int midY = (start.Y + end.Y) / 2;
                        graphics.DrawString(matrix[i, j].ToString(), DefaultFont, Brushes.Black, midX, midY);
                    }
                }
            }

            pictureBoxGraph.Image = bitmap;
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Перевірка, чи задана матриця відстаней
                if (matrix_buf == null)
                {
                    MessageBox.Show("Матриця відстаней не задана. Спочатку введіть дані!", "Помилка");
                    return;
                }

                int n = matrix_buf.GetLength(0);
                int m = n;

                double[,] eta = new double[n, n];
                double[,] tau = new double[n, n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i != j)
                        {
                            eta[i, j] = 1.0 / matrix_buf[i, j];
                            tau[i, j] = tau0;
                        }
                        else
                        {
                            tau[i, j] = 0;
                        }
                    }
                }

                int[] antPositions = new int[m];
               // Random rand = new Random();
                for (int k = 0; k < m; k++)
                {
                    //antPositions[k] = rand.Next(n);
                    antPositions[k] = 0;
              }

                bestLength = int.MaxValue;
                bestRoute = new int[n+1];
                int tmax = 100;

                for (int t = 0; t < tmax; t++)
                {
                    for (int k = 0; k < m; k++)
                    {
                        int[] route = BuildRoute(n, antPositions[k], eta, tau, alpha, beta);
                        int length = CalculateRouteLength(route, matrix_buf);

                        if (length < bestLength)
                        {
                            bestLength = length;
                            Array.Copy(route, bestRoute, n+1);
                        }

                        UpdatePheromone(tau, route, length, Q);
                    }

                    EvaporatePheromone(tau, evaporationRate);
                }
                
                
                DisplayAntsPath1(matrix_buf, bestRoute, n + 1);
                flag_ant = true;
                poprav = false;

                for (int kr = 0; kr <= n; kr++)
                    bestRoute[kr] += 1;

                txtResults.Text = "Найкоротший маршрут: " + string.Join(" -> ", bestRoute) + "\t";
                txtResults.Text += "Його довжина: " + bestLength;

                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Сталася помилка: " + ex.Message, "Помилка");
            }
        }


        //============================
        private int[] BuildRoute(int n, int start, double[,] eta, double[,] tau, double alpha, double beta)
        {
            int[] route = new int[n+1];    // int[] route = new int[n];
            bool[] visited = new bool[n];
            route[0] = start;
            visited[start] = true;

            for (int step = 1; step < n; step++)
            {
                int current = route[step - 1];
                double[] probabilities = new double[n];
                double sum = 0;

                for (int j = 0; j < n; j++)
                {
                    if (!visited[j])
                    {
                        probabilities[j] = Math.Pow(tau[current, j], alpha) * Math.Pow(eta[current, j], beta);
                        sum += probabilities[j];
                    }
                }

                double random = new Random().NextDouble() * sum;
                for (int j = 0; j < n; j++)
                {
                    if (!visited[j])
                    {
                        random -= probabilities[j];
                        if (random <= 0)
                        {
                            route[step] = j;
                            visited[j] = true;
                            break;
                        }
                    }
                }
            }

            route[n] = route[0]; // Повертаємося в початкове місто (замикаємо коло)
            return route;
        }

        private int CalculateRouteLength(int[] route, int[,] matrix_buf)
        {
            int length = 0;
            for (int i = 0; i < route.Length - 1; i++)
            {
                length += matrix_buf[route[i], route[i + 1]];
            }
            length += matrix_buf[route[route.Length - 1], route[0]];
            return length;
        }

        private void UpdatePheromone(double[,] tau, int[] route, double length, double Q)
        {
            double deposit = Q / length;
            for (int i = 0; i < route.Length - 1; i++)
            {
                tau[route[i], route[i + 1]] += deposit;
                tau[route[i + 1], route[i]] += deposit;
            }
            tau[route[route.Length - 1], route[0]] += deposit;
            tau[route[0], route[route.Length - 1]] += deposit;
        }

        private void EvaporatePheromone(double[,] tau, double evaporationRate)
        {
            int n = tau.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    tau[i, j] *= (1 - evaporationRate);
                }
            }
        }

    }



    /*       private void MainForm_Resize(object sender, EventArgs e)
           {
               if (currentMatrix != null) // Якщо матриця існує
               {
                   DisplayGraph(currentMatrix);
               }
           }
    */

}
    
