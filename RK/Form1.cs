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
            
            public decimal x;
            public decimal y;
            public decimal h;
            public decimal c;//just for debug, it is contrast

            public H_point(decimal x, decimal y,decimal h)
            {
                this.x = x;
                this.y = y;
                this.h = h;
            }

            public H_point(decimal x, decimal y, decimal h, decimal c)
            {
                this.x = x;
                this.y = y;
                this.h = h;
                this.c = c;
            }

        }

        public List<H_point> Line(decimal ym, decimal xm, decimal h,decimal epsilon)
        {
            List<H_point> points = new List<H_point>();
            //оно рисуется на промежутке от 0 до 0.5
            int n = (int)(0.5f / (double)h);
            int max_t = 0;



            decimal old_y = ym;
            decimal old_x = xm;

            decimal normal_y = ym;
            decimal decimal_y = ym;

            decimal contrast = 0;

            //just for debug
            int e = 0;
            int nn = 0;

            for (int i = 0; i < n; i++)
            {
                label3.Text = "\n old_x" + old_x.ToString()+" "+ old_y.ToString();
                points.Add(new H_point((old_x), old_y, h,contrast));

                //h = (decimal)0.1f;

                bool CountNeeded = true;
                int t = 0;
                while (CountNeeded && t<1500)
                {
                    t++;
                    /*Мы строим график, пытаемся делать с шагом h и с шагом 0.5h,
                     сравниваем точность, которая там написана. Если она не достаточна, 
                    то мы принимаем за новый шаг, шаг в два раза меньший.

                     */
                    
                    normal_y = GetNextY(old_y, old_x, h);
                    decimal_y = GetNextY(old_y, old_x, (decimal)0.5 * h);
                    decimal_y = GetNextY(decimal_y, old_x+(decimal)0.5*h, (decimal)0.5 * h);

                    contrast = normal_y - decimal_y;
                    if (contrast < 0) contrast *= -1;

                    old_y = normal_y;

                    if (contrast > epsilon)
                    {
                        //old_y = decimal_y;
                        h = h * (decimal)0.5;
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
                label3.Text += "\n t=" +t.ToString() ;
                label3.Text += "\n max_t=" + max_t.ToString() ;
                old_x += h;
                //label1.Text += "\n " + t.ToString() ;
            }
            label1.Text += "\nenough=" + e.ToString() + " not" + nn.ToString();
            return points;
        }

        


        decimal my_func(decimal x, decimal y)
        {
            //Из подсказки на доске следует, что f(y,x) - это y'

            return (decimal)(x * x + y * y);
        }


        decimal GetNextY(decimal ym, decimal xm, decimal h)
        {

            //По какой-то причине в примере представлено несколько способов, но не указано в каких
            // случаях их допустимо применять, так что видимо можно взять любой (в них формулы для кашек отличаются)

            decimal k0 = h * my_func(xm, ym);
            decimal k1 = h * my_func(xm + h / 2, ym + k0 / 2);
            decimal k2 = h * my_func(xm + h / 2, ym + k1 / 2);
            decimal k3 = h * my_func(xm + h, ym + k2);

            decimal delta_ym = (decimal)(((decimal)1.0f / (decimal)6.0f) * (k0 + (decimal)2 * k1 + (decimal)2 * k2 + k3));

            decimal next_y = ym + delta_ym;
            return next_y;
        }

        private void Calculate()
        {
            //Убираем старые штуки
            chart1.Series["Series1"].Points.Clear();
            chart2.Series["Series1"].Points.Clear();
            chart3.Series["Series1"].Points.Clear();

            chart3.Series["Series1"].ChartType =chart2.Series["Series1"].ChartType = chart1.Series["Series1"].ChartType = SeriesChartType.Line;

            //y'=x^2+y^2
            //y(0)=0.4       x принадлежит [0,1]

            //для начала мы ище нужные нам k
            decimal h = (decimal)0.1f;

            //Судя по примеру k0=h*f(xm,ym)

            //Судя по примеру xm и ym это значения из частного кейса ym=0.4f, xm = 0

            decimal ym = (decimal)0.4f;
            decimal xm = 0;



            decimal epsilon = (decimal)0.00001;
            try
            {
                epsilon =(decimal) Math.Pow(10,-Convert.ToDouble(epsilon_box.Text));
                label1.Text += "epsilon=" + epsilon.ToString();
            }
            catch
            {
                label1.Text += "default epsilon=" + epsilon.ToString();
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
                h = h / (decimal)2.0f;

                decimal y1_h1 = GetNextY(ym, xm, h);
                //label1.Text += "\n" + h.ToString() + " ->     y1_h1=" + y1_h1.ToString();
                decimal y2_h1 = GetNextY(y1_h1, h, h);
                //label1.Text += "\n" + h.ToString() + " ->     y2_h1=" + y2_h1.ToString();

                decimal y1_h2 = GetNextY(ym, xm, 2 * h);
                //label1.Text += "\n" + h.ToString() + " ->     y1_h2=" + y1_h2.ToString();

                decimal contrast = y1_h2 - y2_h1;
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
            List<H_point> points= Line(ym,xm,h,epsilon);
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
