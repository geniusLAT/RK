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
using System.Windows.Forms.DataVisualization.Charting; //Именно это пространство имен поддерживает многопоточность

namespace RK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class H_point
        {

            public double x;
            public double y;
            public double h;
            public double c;//just for debug, it is contrast

            public H_point(double x, double y, double h)
            {
                this.x = x;
                this.y = y;
                this.h = h;
            }

            public H_point(double x, double y, double h, double c)
            {
                this.x = x;
                this.y = y;
                this.h = h;
                this.c = c;
            }

        }

        public List<H_point> Line(double ym, double xm, double xfinish,double h, double epsilon,double def_h)
        {
            //h = def_h;
            List<H_point> points = new List<H_point>();
            //оно рисуется на промежутке от 0 до 0.5
            int n = (int)(0.5f / (double)h);
            int max_t = 0;



            double old_y = ym;
            double old_x = xm;

            double normal_y = ym;
            double double_y = ym;

            double contrast = 0;

            //just for debug
            int e = 0;
            int nn = 0;

            //while(old_x<0.5f)
            while(old_x< xfinish)
            {
                label3.Text = "\n old_x" + old_x.ToString() + " " + old_y.ToString();
                points.Add(new H_point((old_x), old_y, h, contrast));

                //h = (double)0.1f;

                bool CountNeeded = true;
                int t = 0;
                while (CountNeeded && t < 1500)
                {
                    t++;
                    /*Мы строим график, пытаемся делать с шагом h и с шагом 0.5h,
                     сравниваем точность, которая там написана. Если она не достаточна, 
                    то мы принимаем за новый шаг, шаг в два раза меньший.

                     */

                    normal_y = GetNextY(old_y, old_x, h);
                    double_y = GetNextY(old_y, old_x, (double)0.5 * h);
                    double_y = GetNextY(double_y, old_x + (double)0.5 * h, (double)0.5 * h);

                    contrast = normal_y - double_y;
                    if (contrast < 0) contrast *= -1;

                    old_y = normal_y;

                    if (contrast > epsilon)
                    {
                        //old_y = double_y;
                        h = h * (double)0.5;
                        //недостаточная точность

                        nn++;
                    }
                    else
                    {
                        e++;
                        CountNeeded = false;
                        //достаточная точность
                    }



                }

                if (t > max_t) max_t = t;
                label3.Text += "\n t=" + t.ToString();
                label3.Text += "\n max_t=" + max_t.ToString();
                old_x += h;
                //label1.Text += "\n " + t.ToString() ;
            }
            label1.Text += "\nenough=" + e.ToString() + " not" + nn.ToString();
            return points;
        }




        double my_func(double x, double y)
        {
            //Из подсказки на доске следует, что f(y,x) - это y'

            return (double)(x * x + y * y);
        }


        double GetNextY(double ym, double xm, double h)
        {

            //По какой-то причине в примере представлено несколько способов, но не указано в каких
            // случаях их допустимо применять, так что видимо можно взять любой (в них формулы для кашек отличаются)

            double k0 = h * my_func(xm, ym);
            double k1 = h * my_func(xm + h / 2, ym + k0 / 2);
            double k2 = h * my_func(xm + h / 2, ym + k1 / 2);
            double k3 = h * my_func(xm + h, ym + k2);

            double delta_ym = (double)(((double)1.0f / (double)6.0f) * (k0 + (double)2 * k1 + (double)2 * k2 + k3));

            double next_y = ym + delta_ym;
            return next_y;
        }

        private void Calculate()
        {
            //Убираем старые штуки
            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();
            chart3.Series["Series1"].Points.Clear();

            chart3.Series["Series1"].ChartType = chart2.Series["Series1"].ChartType = chart1.Series["Series1"].ChartType = SeriesChartType.Line;

            //y'=x^2+y^2
            //y(0)=0.4       x принадлежит [0,1]

            //для начала мы ище нужные нам k
            double h = (double)0.1f;

            //Судя по примеру k0=h*f(xm,ym)

            //Судя по примеру xm и ym это значения из частного кейса ym=0.4f, xm = 0

            double ym = (double)0.4f;
            double xm = 0;
            
            
            double x_finish = (double)0.5f;


            double epsilon = (double)0.00001;
            try
            {
                epsilon = (double)Math.Pow(10, -Convert.ToDouble(epsilon_box.Text));
                label1.Text += "epsilon=" + epsilon.ToString();
            }
            catch
            {
                label1.Text += "default epsilon=" + epsilon.ToString();
            }

            double def_h = (double)0.00001;
            try
            {
                def_h = Convert.ToDouble(def_h_box.Text);
                label1.Text += "def_h=" + def_h.ToString();
            }
            catch
            {
                label1.Text += "default def_h=" + def_h.ToString();
            }

            try
            {
                xm = Convert.ToDouble(startX_box.Text);
                label1.Text += "xm=" + xm.ToString();
            }
            catch
            {
                label1.Text += "default xm=" + xm.ToString();
            }

            try
            {
                x_finish = Convert.ToDouble(finishX_box.Text);
                label1.Text += "x_finish=" + x_finish.ToString();
            }
            catch
            {
                label1.Text += "default x_finish=" + x_finish.ToString();
            }

            try
            {
                ym = Convert.ToDouble(y0_box.Text);
                label1.Text += "ym=" + ym.ToString();
            }
            catch
            {
                label1.Text += "default ym=" + ym.ToString();
            }


            int count = 50;
            bool NotEnoughAccurate = false;
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
                h = h / (double)2.0f;

                double y1_h1 = GetNextY(ym, xm, h);
                //label1.Text += "\n" + h.ToString() + " ->     y1_h1=" + y1_h1.ToString();
                double y2_h1 = GetNextY(y1_h1, h, h);
                //label1.Text += "\n" + h.ToString() + " ->     y2_h1=" + y2_h1.ToString();

                double y1_h2 = GetNextY(ym, xm, 2 * h);
                //label1.Text += "\n" + h.ToString() + " ->     y1_h2=" + y1_h2.ToString();

                double contrast = y1_h2 - y2_h1;
                if (contrast < 0) contrast *= -1;

                //label1.Text += "\n\n contrast=" + contrast.ToString();

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
            //DrawPlot(plot_h1, Line(ym, xm, h));
            List<H_point> points = Line(ym, xm, x_finish, h, epsilon, def_h);
            //DrawPlot(plot_h2, Line(ym, xm, 2 * h));

            label1.Text += "\n" + points.Count + " points";
            for (int i = 0; i < points.Count; i++)
            {
                chart1.Series["Series1"].Points.AddXY(points[i].x, points[i].y);
                chart2.Series["Series1"].Points.AddXY(points[i].x, points[i].h);
                chart3.Series["Series1"].Points.AddXY(points[i].x, points[i].c);

                label3.Text += "\n" + points[i].h;
            }


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
