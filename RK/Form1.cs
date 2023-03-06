using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading; //Именно это пространство имен поддерживает многопоточность

namespace RK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public List<Point> Line(double ym, double xm, double h)
        {
            List<Point> points = new List<Point>();
            //оно рисуется на промежутке от 0 до 0.5
            int n = (int)(0.5f / h);


            //label1.Text += "\n old_x" + xm.ToString();

            double old_y = ym;
            double old_x = xm;

            float koef_x = 900 / 0.5f; //Я забыл, что промежуток на котором мы строим функцию константа


            for (int i = 0; i < n; i++)
            {
                points.Add(new Point((int)(old_x * koef_x), (int)(400 - old_y * 100 * 5)));
                old_y = GetNextY(old_y, old_x, h);
                old_x += h;


                //points.Add(new Point((int)(old_x* koef_x), (int)(400-old_y*100* 5)));

                //400 высота поля для рисования графика, там начинается подсчёт сверху, но нам привычнее, чтобы он начинался снизу

            }

            return points;
        }

        public void DrawPlot(PictureBox box, List<Point> points)
        {



            Graphics graphics_h = box.CreateGraphics();
            Pen pen = new Pen(Color.Blue, 3f);





            graphics_h.DrawLines(pen, points.ToArray());
        }


        double my_func(double x, double y)
        {
            //Из подсказки на доске следует, что f(y,x) - это y'

            return x * x + y * y;
        }


        double GetNextY(double ym, double xm, double h)
        {

            //По какой-то причине в примере представлено несколько способов, но не указано в каких
            // случаях их допустимо применять, так что видимо можно взять любой (в них формулы для кашек отличаются)

            double k0 = h * my_func(xm, ym);
            double k1 = h * my_func(xm + h / 2, ym + k0 / 2);
            double k2 = h * my_func(xm + h / 2, ym + k1 / 2);
            double k3 = h * my_func(xm + h, ym + k2);

            double delta_ym = (1.0f / 6.0f) * (k0 + 2 * k1 + 2 * k2 + k3);

            double next_y = ym + delta_ym;
            return next_y;
        }

        private void Calculate()
        {
            //y'=x^2+y^2
            //y(0)=0.4       x принадлежит [0,1]

            //для начала мы ище нужные нам k
            double h = 0.1f;

            //Судя по примеру k0=h*f(xm,ym)

            //Судя по примеру xm и ym это значения из частного кейса ym=0.4f, xm = 0

            double ym = 0.4f;
            double xm = 0;



            double epsilon = 0.00001;
            try
            {
                epsilon = Convert.ToDouble(epsilon_box.Text);
                label1.Text += "epsilon=" + epsilon_box.Text;
            }
            catch
            {
                label1.Text += "default epsilon=" + epsilon_box.Text;
            }


            int count = 50;
            bool NotEnoughAccurate = true;
            while (NotEnoughAccurate && count > 0)
            {
                //Чтобы нельзя было мататься вечно в цикле
                count--;
                /*Пример снабжает меня не полной информацией, но судя по всему
                 
                 сначала мы вычисляем дельта игрек для нормального шага и ym и xm

                Потом мы берём и считаем дельта игрек для нормального шага и
                вместо ym берём y1 (который считается из предыдущего дельта игрек),
                откуда взялась замена xm в примере не до конца понятно. Но заметно, что новый x совпадает
                с h. Надеюсь, что это именно часть алгоритма, а не простое совпадение. 

                P.S. Я понял почему так позднее.


                Третий подсчёт ведётся с удвоенным шагом, использует ym и xm. Что 
                интересно, тут уже не используется никакой y2. А результат второго подсчёта с h сравнивается с результатом первого с h2
                 
                 */
                h = h / 2.0f;

                double y1_h1 = GetNextY(ym, xm, h);
                label1.Text += "\n" + h.ToString() + " ->     y1_h1=" + y1_h1.ToString();
                double y2_h1 = GetNextY(y1_h1, h, h);
                label1.Text += "\n" + h.ToString() + " ->     y2_h1=" + y2_h1.ToString();

                double y1_h2 = GetNextY(ym, xm, 2 * h);
                label1.Text += "\n" + h.ToString() + " ->     y1_h2=" + y1_h2.ToString();

                double contrast = y1_h2 - y2_h1;
                if (contrast < 0) contrast *= -1;

                label1.Text += "\n\n contrast=" + contrast.ToString();

                if (contrast < epsilon)
                {
                    NotEnoughAccurate = false;
                }

            }
            if (count < 1)
            {
                label1.Text += "too much";
            }

            //Что интересно первый график из-за другого шага выглядит длинее. Кажется там не прорисовывается последняя точка.
            //Будь это комерческий проект, я бы занёс это в баг трекер



            //void f1()
            //{

            //}
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            

            //Thread myThread = new Thread(f1); //Создаем новый объект потока (Thread)
            //myThread.Start();
            DrawPlot(plot_h1, Line(ym, xm, h));
            DrawPlot(plot_h2, Line(ym, xm, 2 * h));
            stopwatch.Stop();

            label1.Text += "\n" + stopwatch.ElapsedMilliseconds.ToString() + " ms";


        }
        private void button1_Click(object sender, EventArgs e)
        {
            //
            label1.Text = "Count\n";
            Calculate();
        }
    }
}
