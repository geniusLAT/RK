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
            public double z;
            public double h;
            public double c;//just for debug, it is contrast

            public H_point(double x, double y, double h, double z)
            {
                this.x = x;
                this.y = y;
                this.h = h;
                this.z = z;
            }

            public H_point(double x, double y, double h, double c, double z)
            {
                this.x = x;
                this.y = y;
                this.h = h;
                this.c = c;
                this.z = z;
            }

        }

        public List<H_point> Line(double zm, double ym, double xm, double xfinish,double h, double epsilon,double def_h)
        {

            progressBar1.Value = 0;

            //h = def_h;
            h = 0.00001f;
            List<H_point> points = new List<H_point>();
            //оно рисуется на промежутке от 0 до 0.5
            int n = (int)(0.5f / (double)h);
            int max_t = 0;



            double old_y = ym;
            double old_x = xm;
            double old_z = zm;

            double normal_y = ym;
            double normal_z = ym;
            double double_y = ym;
            double double_z = zm;

            double contrast_y = 0;
            double contrast_z = 0;

            //just for debug
            int e = 0;
            int nn = 0;

            int progress = 0;

            label1.Text += "\n\nY: " + old_y + "\n\n";

            //while(old_x<0.5f)
            while (old_x< xfinish)
            {

                progress= (int)(old_x / xfinish * 100);
                progresText.Text = progress.ToString()+"%";
                progressBar1.Value = progress;

                label3.Text = "\n old_x" + old_x.ToString() + " " + old_y.ToString();
                points.Add(new H_point((old_x), old_y, h, contrast_y,old_z));

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

                    normal_z = GetNextZ(old_z, old_x, h);
                    double firstDoubleZ = GetNextZ(old_z, old_x, (double)0.5 * h);
                    double_z = GetNextZ(firstDoubleZ, old_x + (double)0.5 * h, (double)0.5 * h);

                    normal_y = GetNextY(old_y, old_x, h, old_z);
                    double_y = GetNextY(old_y, old_x, (double)0.5 * h, old_z);
                    double_y = GetNextY(double_y, old_x + (double)0.5 * h, (double)0.5 * h, old_z);

                    contrast_y = normal_y - double_y;
                    if (contrast_y < 0) contrast_y *= -1; 
                    
                    contrast_z = normal_z - double_z;
                    if (contrast_z < 0) contrast_z *= -1;

                    old_y = normal_y;
                    old_z = normal_z;

                    //if ((contrast_y > epsilon) || (contrast_z > epsilon))
                    //{
                    //    //old_y = double_y;
                    //    h = h * (double)0.5;
                    //    //недостаточная точность

                    //    nn++;
                    //}
                    //else
                    //{
                    //    e++;
                    //    CountNeeded = false;
                    //    //достаточная точность
                    //}

                    CountNeeded = false;
                    //return new List<H_point>();//TODO delete me
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




        //double my_func(double x, double y)
        //{
        //    //Из подсказки на доске следует, что f(y,x) - это y'

        //    return (double)(x * x + y * y);
        //}


        double funcZ(double z, double x)
        {
            return -(z * z) + (2.5f * x) / (1 + x * x);
        }

        double funcY(double z, double x,double y)
        {
            //label3.Text += "\n z: " + z;
            //label3.Text += "\n x: " + x;
            //label3.Text += "\n y: " + y;
            //if (x == 0) x = 0.00001f;
            return -y*z+Math.Cos(x)/x;
        }

        double GetNextZ(double zm, double xm, double h)
        {

            double zk0 = h * funcZ(xm, zm);
            double zk1 = h * funcZ(xm + h / 2, zm + zk0 / 2);
            double zk2 = h * funcZ(xm + h / 2, zm + zk1 / 2);
            double zk3 = h * funcZ(xm + h, zm + zk2);

            double delta_zm = (double)(((double)1.0f / (double)6.0f) * (zk0 + (double)2 * zk1 + (double)2 * zk2 + zk3));

            double next_z = zm + delta_zm;
            return next_z;
        }

        double GetNextY(double ym, double xm, double h, double zm)
        {
            double zk0 = h * funcZ(xm, zm);

            label3.Text += "\n zk0: " + zk0;
            label3.Text += "\n h: " + h;

            double yk0 = h * funcY(zm, xm, ym);
            label3.Text += "\nyk0: " + yk0;

            double zk1 = h * funcZ(xm + h / 2, zm + zk0 / 2);
            double yk1 = h * funcY(zm + zk0 / 2, xm + h / 2, ym + yk0 / 2);

            label3.Text += "\n yk1: " + yk1;

            double zk2 = h * funcZ(xm + h / 2, zm + zk1 / 2);
            double yk2 = h * funcY(zm + zk1 / 2, xm + h / 2, ym + yk1 / 2);

            label3.Text += "\n yk2: " + yk2;

            double zk3 = h * funcZ(xm + h, zm + zk2);
            double yk3 = h * funcY(zm + zk2, xm + h, ym + yk2);

            label3.Text += "\n yk3: " + yk3;








            //double delta_zm = (double)(((double)1.0f / (double)6.0f) * (zk0 + (double)2 * zk1 + (double)2 * zk2 + zk3));

            //double next_z = zm + delta_zm;
            //return next_z;

            double delta_ym = (double)(((double)1.0f / (double)6.0f) * (yk0 + (double)2 * yk1 + (double)2 * yk2 + yk3));

            double next_y = ym + delta_ym;
            return next_y;
        }


        //double GetNextY(double ym, double xm, double h)
        //{

        //    //По какой-то причине в примере представлено несколько способов, но не указано в каких
        //    // случаях их допустимо применять, так что видимо можно взять любой (в них формулы для кашек отличаются)

        //    double k0 = h * my_func(xm, ym);
        //    double k1 = h * my_func(xm + h / 2, ym + k0 / 2);
        //    double k2 = h * my_func(xm + h / 2, ym + k1 / 2);
        //    double k3 = h * my_func(xm + h, ym + k2);

        //    double delta_ym = (double)(((double)1.0f / (double)6.0f) * (k0 + (double)2 * k1 + (double)2 * k2 + k3));

        //    double next_y = ym + delta_ym;
        //    return next_y;
        //}

        private void Calculate()
        {

            progresText.Text = "0%";

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

            double ym = (double)0;
            double zm = (double)-0.2f;
            double xm = 0;
            
            
            double x_finish = (double)1f;


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

            //try
            //{
            //    xm = Convert.ToDouble(startX_box.Text);
            //    label1.Text += "xm=" + xm.ToString();
            //}
            //catch
            //{
            //    label1.Text += "default xm=" + xm.ToString();
            //}

            //try
            //{
            //    x_finish = Convert.ToDouble(finishX_box.Text);
            //    label1.Text += "x_finish=" + x_finish.ToString();
            //}
            //catch
            //{
            //    label1.Text += "default x_finish=" + x_finish.ToString();
            //}

            //try
            //{
            //    ym = Convert.ToDouble(y0_box.Text);
            //    label1.Text += "ym=" + ym.ToString();
            //}
            //catch
            //{
            //    label1.Text += "default ym=" + ym.ToString();
            //}


            //int count = 50;
            //bool NotEnoughAccurate = false;
            //while (NotEnoughAccurate && count > 0)
            //{
            //    //Чтобы нельзя было мататься вечно в цикле
            //    count--;
            //    /*Пример снабжает меня не полной информацией, но судя по всему
                 
            //     сначала мы вычисляем дельта игрек для нормального шага и ym и xm

            //    Потом мы берём и считаем дельта игрек для нормального шага и
            //    вместо ym берём y1 (который считается из предыдущего дельта игрек),
            //    откуда взялась замена xm в примере не до конца понятно. Но заметно, что новый x совпадает
            //    с h. Надеюсь, что это именно часть алгоритма, а не простое совпадение. 

            //    P.S. Я понял почему так позднее.


            //    Третий подсчёт ведётся с удвоенным шагом, использует ym и xm. Что 
            //    интересно, тут уже не используется никакой y2. А результат второго подсчёта с h сравнивается с результатом первого с h2
                 
            //     */
            //    //h = h / (double)2.0f;


            //    double z1_h1 = GetNextZ(zm, xm, h);
            //    //label1.Text += "\n" + h.ToString() + " ->     y1_h1=" + y1_h1.ToString();
            //    double z2_h1 = GetNextZ(z1_h1, h, h);
            //    //label1.Text += "\n" + h.ToString() + " ->     y2_h1=" + y2_h1.ToString();

            //    double z1_h2 = GetNextZ(zm, xm, 2 * h);



            //    double y1_h1 = GetNextY(zm,ym, xm, h);
            //    //label1.Text += "\n" + h.ToString() + " ->     y1_h1=" + y1_h1.ToString();
            //    double y2_h1 = GetNextY(zm, y1_h1, h, h);
            //    //label1.Text += "\n" + h.ToString() + " ->     y2_h1=" + y2_h1.ToString();

            //    double y1_h2 = GetNextY(zm, ym, xm, 2 * h);
            //    //label1.Text += "\n" + h.ToString() + " ->     y1_h2=" + y1_h2.ToString();

            //    double contrast_y = y1_h2 - y2_h1;
            //    if (contrast_y < 0) contrast_y *= -1;

            //    //label1.Text += "\n\n contrast=" + contrast.ToString();

            //    if (contrast_y < epsilon)
            //    {
            //        NotEnoughAccurate = false;
            //    }

            //}
            //if (count < 1)
            //{
            //    label1.Text += "too much";
            //}

           


            //void f1()
            //{

            //}
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();



            //Thread myThread = new Thread(f1); //Создаем новый объект потока (Thread)
            //myThread.Start();
            //DrawPlot(plot_h1, Line(ym, xm, h));
            label1.Text += "\n\nY: " + ym +"\n\n";
            List<H_point> points = Line(zm, ym + 0.00001f, xm+0.00001f, x_finish, h, epsilon, def_h);
            //DrawPlot(plot_h2, Line(ym, xm, 2 * h));

            label1.Text += "\n" + points.Count + " points";
            for (int i = 0; i < points.Count; i++)
            {
                try
                {
                    chart1.Series["Series1"].Points.AddXY(points[i].x, points[i].y);
                    chart2.Series["Series1"].Points.AddXY(points[i].x, points[i].h);
                    chart3.Series["Series1"].Points.AddXY(points[i].x, points[i].z);

                    //label3.Text += "\nx: " + points[i].x+" y:"+ points[i].y;

                    //label3.Text += "\n" + points[i].h;
                }
                catch (Exception)
                {

                    //throw;
                }
                
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
