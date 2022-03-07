using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //true, если точка находится на старте (только начинается движение от датчика)
        bool[] start_position = new bool[3];

        //эти переменные для того, чтобы определить, достигнута ли граница контура
        bool[] is_border = new bool[3];

        bool captured = false; //для передвижения излучателя

        double x_shape, x_canvas, y_shape, y_canvas;
        UIElement source = null;

        Ellipse[] ray = new Ellipse[3]; //для отрисовки трёх лучей

        //скорость движения
        int speed = 1;

        //текущие координаты положения сигнала
        double[] x_ray = new double[3];
        double[] y_ray = new double[3];

        //коэффициенты уравнений для сторон
        double[] k_sides;
        double[] b_sides;

        //индекс стороны, от которой происходит движение
        int[] index = new int[3];

        //для очередей (стирание старого следа)
        int count1 = 0;
        int count2 = 0;

        //координаты точки пересечения прямой и стороны
        double[] intersect_x = new double[3];
        double[] intersect_y = new double[3];

        bool direction = true; //для того, чтобы корректно могла срабатать кнопка старт после нажатия на кнопку стоп

        //коэффициенты прямой, которая проходит через датчик под заданным углом
        double[] k = new double[3];
        double[] b = new double[3];

        //угол ввода
        double[] angle = new double[3];
        double teta = 0; //угол между лучами 

        //массив сторон многоугольника
        private List<Point[]> _sides = new List<Point[]>();

        //массив точек для построения многоугольника (на canvas, сами элементы)
        private List<Ellipse> ellipses = new List<Ellipse>();

        //очереди для лучей (чтобы стирать старый след)
        private Queue<List<Ellipse>> queue = new Queue<List<Ellipse>>();

        //таймер
        DispatcherTimer dT = new DispatcherTimer();

        PointCollection _points = new PointCollection();


        public MainWindow()
        {

            //for (int i = 0; i < 3; i++)
            //{
            //    min_distance[i] = 1000;
            //}

            for (int i = 0; i < 3; i++)
            {
               start_position[i] = true;
            }
            InitializeComponent();
            comboBox1.Items.Add(1);
            comboBox1.Items.Add(30);
            comboBox1.Items.Add(70);

            for (int i = 0; i < 3; i++)
            {
                is_border[i] = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //удаление с canvas точек, по которым строится многоугольник (не обязательно, но без них изображение лучше выглядит) 
            for (int i = 0; i < ellipses.Count; i++)
                canvas1.Children.Remove(ellipses[i]);

            //для построения многоугольника используется массив точек, где их по 3 экземпляра
            Polygon myPolygon = new Polygon();

            //using (StreamReader sr = new StreamReader(@"data.txt", System.Text.Encoding.Default))
            //{
            //    string line;
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        string[] coord = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            //        Point p = new Point();
            //        p.X = double.Parse(coord[0]);
            //        p.Y = double.Parse(coord[1]);
            //        _points.Add(p);
            //    }
            //}

            myPolygon.Points = _points;

            myPolygon.Fill = Brushes.PeachPuff;
            myPolygon.Stroke = Brushes.Black;
            myPolygon.StrokeThickness = 2;
            canvas1.Children.Add(myPolygon);
            sides();
            EquationCoeff();
        }

        //запуск анимации (старт)
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //если сигнал находится в начальном положении 
            if (direction)
            {
                double teta_text = double.Parse(textBox2.Text);
                teta = teta_text % 180.0;
                angle[0] = double.Parse(textBox1.Text) % 180.0;
                angle[1] = angle[0] + teta;
                angle[2] = angle[0] - teta;
                
                for (int i = 0; i < 3; i++)
                {
                    ray[i] = new Ellipse();
                    ray[i].Width = 5;
                    ray[i].Height = 5;
                    canvas1.Children.Add(ray[i]);
                }
            }

            //исходные координаты эллипса
            if (direction)
            {
                dT.Tick += new EventHandler(dT_Tick);
                dT.Interval = new TimeSpan(0, 0, 0, 0, speed);

            }

            button7.IsEnabled = false;
            dT.Start();
        }

        void dT_Tick(object sender, EventArgs e)
        {
            
            for (int i = 0; i < 3; i++)
            {
                //если сигнал еще не дошел до стороны
                if ((Math.Abs(x_ray[i] - intersect_x[i]) > 1 || Math.Abs(y_ray[i] - intersect_y[i]) > 1) && is_border[i] == true)
                {
                    is_border[i] = true;
                    x_ray[i] -= Math.Sin(GetAngle(i));
                    y_ray[i] -= Math.Cos(GetAngle(i));
                    Color();
                }
                else
                {
                    is_border[i] = false;
                    


                    Coeff(i);
                    Intersect(i);
                
                    is_border[i] = true;                   
                }
                
            }

        }

        //передвижение датчика
        private void rectangle1_MouseMove(object sender, MouseEventArgs e)
        {
            if (captured)
            {
                double x = e.GetPosition(this).X;
                double y = e.GetPosition(this).Y;
                x_shape += x - x_canvas;

                Canvas.SetLeft(source, x_shape);
                Canvas.SetLeft(ellipse1, x_shape + 11);
                x_canvas = x;
                y_shape += y - y_canvas;
                Canvas.SetTop(source, y_shape);

                Canvas.SetTop(ellipse1, y_shape + rectangle1.Height - ellipse1.Height);
                y_canvas = y;
            }
        }

        private void rectangle1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(this).X;
            y_shape = Canvas.GetTop(source);
            y_canvas = e.GetPosition(this).Y;
        }

        private void rectangle1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
        }

        private void rectangle1_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        //задание объекта с помощью точек на canvas
        private void canvas1_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            Point p = new Point();
            p.X = e.GetPosition(canvas1).X;
            p.Y = e.GetPosition(canvas1).Y;
            _points.Add(p);
            using (StreamWriter sw = new StreamWriter(@"data.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(p.ToString());
            }
        }

        //задание объекта с помощью точек на canvas
        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas1);
            Ellipse ell = new Ellipse();

            ell.Fill = Brushes.Black;

            ell.Width = 2;
            ell.Height = 2;

            ell.Margin = new Thickness(p.X, p.Y, 0, 0);

            canvas1.Children.Add(ell);
            ellipses.Add(ell);
        }

        //задание объекта с помощью точек на canvas
        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Point p = new Point();
            //p.X = e.GetPosition(canvas1).X;
            //p.Y = e.GetPosition(canvas1).Y;
            //_points3.Add(p);
        }

        //отсановка анимации
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            direction = false;
            dT.Stop();
            button7.IsEnabled = true;
        }


        //задание скорости (данные берутся из ComboBox)
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            speed = int.Parse(comboBox1.Text);
            dT.Interval = new TimeSpan(0, 0, 0, 0, speed);
        }

        private bool isSelfIntersected()
        {
            for (int i = 0; i < _sides.Count; i++)
            {
                for (int j = 0; j < _sides.Count; j++)
                {
                    if (i == j)
                    { }
                    else
                    {
                        double x = (double)(b_sides[j] - b_sides[i]) / (k_sides[i] - k_sides[j]);
                        double y = (double)(k_sides[j] * (b_sides[j] - b_sides[i])) / (k_sides[i] - k_sides[j]) + b_sides[j];
                        if (((x < _sides[i][0].X && x > _sides[i][1].X) || (x > _sides[i][0].X && x < _sides[i][1].X)) && ((y < _sides[i][0].Y && y > _sides[i][1].Y) || (y > _sides[i][0].Y && y < _sides[i][1].Y)) &&
                            ((x < _sides[j][0].X && x > _sides[j][1].X) || (x > _sides[j][0].X && x < _sides[j][1].X)) && ((y < _sides[j][0].Y && y > _sides[j][1].Y) || (y > _sides[j][0].Y && y < _sides[j][1].Y)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //"примагничивание" датчика
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            Sensor();
        }

        //очистить canvas
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            canvas1.Children.Clear();
            _sides.Clear();
            _points.Clear();
            //_points3.Clear();
            direction = true;
            for (int i = 0; i < 3; i++)
                start_position[i] = true;
        }

        //рассчет угла по двум точкам
        double GetAngle(int i)
        {
            return Math.Atan2((x_ray[i] - intersect_x[i]), (y_ray[i] - intersect_y[i]));
            //a %= 360;
            //if (a > 0)
            //{
            //    if (a > 180)
            //        return (a - 180);
            //    else return a;
            //}
            //else
            //{
            //    if (a > -180)
            //        return (a + 180);
            //    else return (a + 360);
            //}
        }

        //стороны многоугольника
        public void sides()
        {
            _sides.Clear();
            for (int i = 0; i < (_points.Count) - 1; i++)
            {
                Point[] pair = new Point[2];
                pair[0] = _points[i];
                pair[1] = _points[i + 1];
                _sides.Add(pair);
            }
            //запись в массив стороны, соединяющей первую и последнюю точки
            Point[] point = new Point[2];
            point[0] = _points[0];
            point[1] = _points.Last();
            _sides.Add(point);
        }

        //рассчет коэффициентов уравнения прямой, проходящей через датчик (угол дан)
        private void Coeff(int i)
        {
            //если это стартовая точка (от датчика)
            if (start_position[i])
            {
                //записать для всех лучей стартовой точкой ту, от которой исходит сигнал (от датчика)
                x_ray[i] = Canvas.GetLeft(ellipse1);
                y_ray[i] = Canvas.GetTop(ellipse1);

                //движение теперь будет происходить для каждого луча от своей координаты, текущей
                start_position[i] = false;
            }
            else
            {
            }
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                angle[0] = double.Parse(textBox1.Text);
                angle[1] = angle[0] + double.Parse(textBox2.Text);
                angle[2] = angle[0] - double.Parse(textBox2.Text);
            }

            //рассчет коэффициентов прямых для каждого луча
            k[i] = (double)Math.Tan((double)angle[i] * Math.PI / 180);
            b[i] = y_ray[i] - k[i] * x_ray[i];
            //очистить данные в TextBox
            textBox1.Text = "";
            textBox2.Text = "";
        }

        ////пересечение со сторонами многоугольника (передается номер луча)
        //private void Intersect(int j)
        //{
        //    double X = 0, Y = 0;
        //    int INDEX = 0;
        //    double min_distance = canvas1.Width;

        //    //проходим по всем сторонам
        //    for (int i = 0; i < _sides.Count; i++)
        //    {
        //        if (i == index[j])
        //        {
        //        }
        //        else
        //        {
        //            //точка пересечения прямой, являющейся траекторией движения и i-ой стороны
        //            double x = (double)(b_sides[i] - b[j]) / (k[j] - k_sides[i]);
        //            double y = (double)(k_sides[i] * (b_sides[i] - b[j])) / (k[j] - k_sides[i]) + b_sides[i];

        //            if (((x <= _sides[i][0].X && x > _sides[i][1].X) || (x > _sides[i][0].X && x <= _sides[i][1].X)) && ((y <= _sides[i][0].Y && y > _sides[i][1].Y) || (y > _sides[i][0].Y && y <= _sides[i][1].Y)))
        //            {
        //                X = x;
        //                Y = y;
        //                INDEX = i;
        //            }
        //        }
        //    }
        //    //в индекс записан номер стороны, с которой будет пересекаться луч 
        //    index[j] = INDEX;
        //    intersect_x[j] = X;
        //    intersect_y[j] = Y;
        //    angle[j] = AngleOfReflection(_sides[INDEX][0], _sides[INDEX][1], j);
        //}


        ////пересечение со сторонами многоугольника (передается номер луча)
        //private void Intersect(int j)
        //{
        //    double[] X = new double[3];
        //    double[] Y = new double[3];
        //    double[] X_middle = new double[3];
        //    double[] Y_middle = new double[3];
        //    double[] min = new double[3];
        //    double[] max = new double[3];
        //    double[] K_middle = new double[3];
        //    double[] B_middle = new double[3];
        //    double[] x_middle = new double[3];
        //    double[] y_middle = new double[3];
        //    int INDEX = 0;
        //    int[] intersection_count = new int[3];
        //    double[] min_distance = { 100000, 100000, 100000 };
        //    label4.Content = min_distance[j].ToString();


        //    //проходим по всем сторонам
        //    for (int i = 0; i < _sides.Count; i++)
        //    {
        //        if (i == index[j])
        //        {
        //        }
        //        else
        //        {
        //            //точка пересечения прямой, являющейся траекторией движения и i-ой стороны
        //            double x = (double)(b_sides[i] - b[j]) / (k[j] - k_sides[i]);
        //            double y = (double)(k_sides[i] * (b_sides[i] - b[j])) / (k[j] - k_sides[i]) + b_sides[i];

        //            if(x > 0 && y > 0  && x < canvas1.Width && y < canvas1.Height) {
        //                label4.Content = min_distance[j].ToString() + ", " + x.ToString() + ", " + y.ToString();

        //                if (x_ray[j] > x)
        //                {
        //                    max[j] = x_ray[j];
        //                    min[j] = x;
        //                }
        //                else
        //                {
        //                    max[j] = x;
        //                    min[j] = x_ray[j];
        //                }

        //                X_middle[j] = (double)(max[j] - min[j]) / 2 + min[j];
        //                Y_middle[j] = X_middle[j] * k[j] + b[j];

        //                K_middle[j] = k_sides.Min() - 1;
        //                B_middle[j] = Y_middle[j] - (K_middle[j] * X_middle[j]);

        //                //цикл по сторонам многоугольника для поиска числа пересечений
        //                for (int m = 0; m < _sides.Count; m++)
        //                {
        //                    x_middle[j] = (double)(b_sides[m] - B_middle[j]) / (K_middle[j] - k_sides[m]);
        //                    y_middle[j] = (double)(k_sides[m] * (b_sides[m] - B_middle[j])) / (K_middle[j] - k_sides[m]) + b_sides[m];

        //                    if (((x_middle[j] < _sides[m][0].X && x_middle[j] > _sides[m][1].X) || (x_middle[j] > _sides[m][0].X && x_middle[j] < _sides[m][1].X)) && ((y_middle[j] < _sides[m][0].Y && y_middle[j] > _sides[m][1].Y) || (y_middle[j] > _sides[m][0].Y && y_middle[j] < _sides[m][1].Y)) && x_middle[j] < X_middle[j])
        //                    {
        //                        intersection_count[j]++;
        //                    }
        //                }

        //                if (intersection_count[j] % 2 == 1)
        //                {
        //                    if (((x <= _sides[i][0].X && x > _sides[i][1].X) || (x > _sides[i][0].X && x <= _sides[i][1].X)) && ((y <= _sides[i][0].Y && y > _sides[i][1].Y) || (y > _sides[i][0].Y && y <= _sides[i][1].Y)))
        //                    {
        //                        if (Math.Sqrt(Math.Pow(x_ray[j] - x, 2) + Math.Pow(y_ray[j] - y, 2)) < min_distance[j])
        //                        {
        //                            min_distance[j] = Math.Sqrt(Math.Pow(x_ray[j] - x, 2) + Math.Pow(y_ray[j] - y, 2));
        //                            X[j] = x;
        //                            Y[j] = y;
        //                            INDEX = i;
        //                        }

        //                    }
        //                    intersection_count[j] = 0;
        //                }


        //                //}

        //            }
        //    }
        //        //min_distance[j] = 1000;
        //    }



        //    //в индекс записан номер стороны, с которой будет пересекаться луч 
        //    index[j] = INDEX;
        //    intersect_x[j] = X[j];
        //    intersect_y[j] = Y[j];

        //    angle[j] = AngleOfReflection(_sides[INDEX][0], _sides[INDEX][1], j);
        //}


        //пересечение со сторонами многоугольника (передается номер луча)
        private void Intersect(int j)
        {
            double[] X = new double[3];
            double[] Y = new double[3];
            double[] X_middle = new double[3];
            double[] Y_middle = new double[3];
            double[] min = new double[3];
            double[] max = new double[3];
            double[] K_middle = new double[3];
            double[] B_middle = new double[3];
            double[] x_middle = new double[3];
            double[] y_middle = new double[3];
            int INDEX = 0;
            int[] intersection_count = new int[3];
            int[] intersection_count2 = new int[3];
            double[] min_distance = { 100000, 100000, 100000 };
            label4.Content = min_distance[j].ToString();


            //проходим по всем сторонам
            for (int i = 0; i < _sides.Count; i++)
            {
                if (i == index[j])
                {
                }
                else
                {
                    //точка пересечения прямой, являющейся траекторией движения и i-ой стороны
                    double x = (double)(b_sides[i] - b[j]) / (k[j] - k_sides[i]);
                    double y = (double)(k_sides[i] * (b_sides[i] - b[j])) / (k[j] - k_sides[i]) + b_sides[i];

                    //bool answer = false;

                    if (x > 0 && y > 0 && x < canvas1.Width && y < canvas1.Height)
                    {
                        //label4.Content = min_distance[j].ToString() + ", " + x.ToString() + ", " + y.ToString();

                        if (x_ray[j] > x)
                        {
                            max[j] = x_ray[j];
                            min[j] = x;
                            //answer = false;
                        }
                        else
                        {
                            max[j] = x;
                            min[j] = x_ray[j];
                            //answer = true;
                        }

                        //X_middle[j] = (double)(max[j] - min[j])/100  + min[j];
                        X_middle[j] = min[j] + ((double)(max[j] - min[j])/2);
                        Y_middle[j] = X_middle[j] * k[j] + b[j];

                        K_middle[j] = k_sides.Min() - 1;
                        B_middle[j] = Y_middle[j] - (K_middle[j] * X_middle[j]);

                        //цикл по сторонам многоугольника для поиска числа пересечений
                        for (int m = 0; m < _sides.Count; m++)
                        {
                            x_middle[j] = (double)(b_sides[m] - B_middle[j]) / (K_middle[j] - k_sides[m]);
                            y_middle[j] = (double)(k_sides[m] * (b_sides[m] - B_middle[j])) / (K_middle[j] - k_sides[m]) + b_sides[m];

                            if (((x_middle[j] < _sides[m][0].X && x_middle[j] > _sides[m][1].X) || (x_middle[j] > _sides[m][0].X && x_middle[j] < _sides[m][1].X)) && ((y_middle[j] < _sides[m][0].Y && y_middle[j] > _sides[m][1].Y) || (y_middle[j] > _sides[m][0].Y && y_middle[j] < _sides[m][1].Y)) && x_middle[j] < X_middle[j])
                            {
                                intersection_count[j]++;
                            }
                        }



                       

                        if (intersection_count[j] % 2 == 1)
                        {
                            if (((x <= _sides[i][0].X && x > _sides[i][1].X) || (x > _sides[i][0].X && x <= _sides[i][1].X)) && ((y <= _sides[i][0].Y && y > _sides[i][1].Y) || (y > _sides[i][0].Y && y <= _sides[i][1].Y)))
                            {
                                if (Math.Sqrt(Math.Pow(x_ray[j] - x, 2) + Math.Pow(y_ray[j] - y, 2)) < min_distance[j])
                                {
                                    //if (intersection_count2[j] == 0)
                                    //{
                                        //label4.Content = intersection_count2[0];
                                        min_distance[j] = Math.Sqrt(Math.Pow(x_ray[j] - x, 2) + Math.Pow(y_ray[j] - y, 2));
                                        X[j] = x;
                                        Y[j] = y;
                                        INDEX = i;
                                   // }
                                }

                            }
                            intersection_count[j] = 0;
                            //intersection_count2[j] = 0;
                        }


                        //}

                    }
                }
                //min_distance[j] = 1000;
            }



            //в индекс записан номер стороны, с которой будет пересекаться луч 
            index[j] = INDEX;
            intersect_x[j] = X[j];
            intersect_y[j] = Y[j];

            angle[j] = AngleOfReflection(_sides[INDEX][0], _sides[INDEX][1], j);
        }








//угол отражения (передаются координаты точек для стороны и номер луча (по номеру можно определить, под каким углом луч движется в настоящий момент))
private double AngleOfReflection(Point p1, Point p2, int j)
        {
            double k;
            k = ((double)(p2.Y - p1.Y) / (p2.X - p1.X));
            double beta = (double)(Math.Atan(k) * 180) / Math.PI;
            return 180 - angle[j] + 2 * beta;
        }

        //поиск коэффициентов уравнения
        private void EquationCoeff()
        {
            k_sides = new double[_sides.Count];
            b_sides = new double[_sides.Count];

            for (int i = 0; i < _sides.Count; i++)
            {
                k_sides[i] = (double)(_sides[i][1].Y - _sides[i][0].Y) / (_sides[i][1].X - _sides[i][0].X);
                b_sides[i] = _sides[i][0].Y - ((double)(_sides[i][1].Y - _sides[i][0].Y) / (_sides[i][1].X - _sides[i][0].X)) * _sides[i][0].X;
            }
        }

        //определение цвета луча
        private void Color()
        {
            List<Ellipse> list = new List<Ellipse>();

            for (int i = 0; i < 3; i++)
            {
                ray[i].Margin = new Thickness(x_ray[i], y_ray[i], 0, 0);

                Ellipse ell = new Ellipse();

                if (i == 0)
                    ell.Fill = Brushes.Red;

                if (i == 1)
                    ell.Fill = Brushes.Green;

                if (i == 2)
                    ell.Fill = Brushes.Blue;

                ell.Width = 2;
                ell.Height = 2;

                ell.Margin = new Thickness(x_ray[i], y_ray[i], 0, 0);
                canvas1.Children.Add(ell);

                list.Add(ell);
            }

            queue.Enqueue(list);

            if (count1 >= 900)
            {
                queue.Peek()[0].Fill = Brushes.Red;
                queue.Peek()[1].Fill = Brushes.Green;
                queue.Peek()[2].Fill = Brushes.Blue;
            }
            else
                count1++;

            if (count2 >= 2000)
            {

                canvas1.Children.Remove(queue.Peek()[0]);
                canvas1.Children.Remove(queue.Peek()[1]);
                canvas1.Children.Remove(queue.Peek()[2]);
                queue.Peek().Clear();
                queue.Dequeue();

            }
            else count2++;
        }

        //"примагничивание" датчика
        public void Sensor()
        {
            //минимальное расстояние от датчика до стороны
            double min = canvas1.Width;

            double x = 0, y = 0;
            //координаты точки, к которой будет "примагничен" датчик
            double xrez = 0;
            double yrez = 0;

            for (int i = 0; i < _sides.Count; i++)
            {
                //угол, под которым проходит текущая сторона
                double alpha = (double)(Math.Atan(k_sides[i]) * 180) / Math.PI;
                //угол, соответствующий прямой, перпендикулярной к текущей стороне
                double beta = alpha - 90;

                //коэффициенты прямой, перпендикулярной текущей в цикле стороне
                double kort = (double)Math.Tan((double)(beta * Math.PI) / 180);
                double bort = Canvas.GetTop(ellipse1) - kort * Canvas.GetLeft(ellipse1);

                //точка пересечения текущей стороны и перпендикуляра, проведенного к ней от датчика
                x = (double)(bort - b_sides[i]) / (k_sides[i] - kort);
                y = (double)(kort * (bort - b_sides[i])) / (k_sides[i] - kort) + bort;
                //если расстояние меньше текущего минимума
                if (Math.Sqrt(Math.Pow(Math.Abs(x - Canvas.GetLeft(ellipse1)), 2) + Math.Pow(Math.Abs(y - Canvas.GetTop(ellipse1)), 2)) < min)
                {
                    if (((x < _sides[i][0].X && x > _sides[i][1].X) || (x > _sides[i][0].X && x < _sides[i][1].X)) && ((y < _sides[i][0].Y && y > _sides[i][1].Y) || (y > _sides[i][0].Y && y < _sides[i][1].Y)))
                    {
                        min = Math.Sqrt(Math.Pow(Math.Abs(x - Canvas.GetLeft(ellipse1)), 2) + Math.Pow(Math.Abs(x - Canvas.GetLeft(ellipse1)), 2));
                        for (int j = 0; j < 3; j++)
                        {
                            index[j] = i;
                            xrez = x;
                            yrez = y;
                        }
                    }
                }
            }
            Canvas.SetLeft(ellipse1, xrez);
            Canvas.SetTop(ellipse1, yrez);
            Canvas.SetLeft(rectangle1, xrez - 11);
            Canvas.SetTop(rectangle1, yrez - 22);
        }

        //проверка на самопересечение
        private void button4_Click_1(object sender, RoutedEventArgs e)
        {
            if (isSelfIntersected())
            {
                label4.Content = "Заданный контур является самопересекающимся.\n";
            }
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            Start wnd = new Start();
            wnd.Show();
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button8_Click_1(object sender, RoutedEventArgs e)
        {
            Start wnd = new Start();
            wnd.Show();
        }
    }
}
