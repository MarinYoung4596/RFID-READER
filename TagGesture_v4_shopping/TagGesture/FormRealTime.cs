using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace TagGesture
{

    public partial class FormRealTime : Form
    {
        Thread thread;
        RealTimeImageMaker rti;
        Color backColor = Color.Black;//指定绘制曲线图的背景色  


        public FormRealTime()
        {


            InitializeComponent();
            rti = new RealTimeImageMaker(Width, Height, backColor, Color.Green);
            thread = new Thread(new ThreadStart(Run));
            thread.Start();

        }

        private void Run()
        {
            Thread.Sleep(2000);
            while (true)
            {
                Image image = rti.GetCurrentCurve();
                Graphics g = CreateGraphics();
                //
                Graphics g1 = CreateGraphics();

                //用指定背景色清除当前窗体上的图象  
                g.Clear(backColor);
                g.DrawImage(image, 0, 0);
                g.Dispose();
                //每0.1秒刷新一次  
                Thread.Sleep(100);
            }
        }

        private void FormRealTime_FormClosing(object sender, FormClosingEventArgs e)
        {
            //在窗体即将关闭之前中止线程  
            thread.Abort();
        }


        public void updateTaginfo(DataRow row)
        {
            rti.updatePointY(row);
        }





    }

}
