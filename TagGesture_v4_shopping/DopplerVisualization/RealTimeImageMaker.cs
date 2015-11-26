using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace DopplerVisualization
{
    class RealTimeImageMaker
    {
        private int width;//要生成的曲线图的宽度  
        private int height;//要生成的曲线图的高度  
        
        private PointF[] pointListF;//用来绘制曲线图的关键点，依次将这些点连接起来即得到曲线图
        private PointF[] pointListF2;
        private PointF[] pointListF3;
        private PointF[] pointListF4;

        private Random random = new Random();//用于生成随机数  
        private Bitmap currentImage;//当前要绘制的图片  
        private Color backColor;//图片背景色  
        private Color foreColor;//图片前景色  

        //新建一个dataTable
        static DataTable DataDS = new DataTable();
        static DataRow tagInfo;
        byte[] types;

        /// <summary>  
        /// 图片的高度  
        /// </summary>  
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>  
        /// 图片的宽度  
        /// </summary>  
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        /// <summary>  
        /// 构造函数，指定生成的曲线图的宽度和高度  
        /// </summary>  
        /// <param name="width">要生成的曲线图的宽度  
        /// <param name="height">要生成的曲线图的高度  
        public RealTimeImageMaker(int width, int height)
            : this(width, height, Color.Gray, Color.Blue)
        {

        }
        /// <summary>  
        /// 构造函数，指定生成的曲线图的宽度、高度及背景色和前景色  
        /// </summary>  
        /// <param name="width">要生成的曲线图的宽度  
        /// <param name="height">要生成的曲线图的高度  
        /// <param name="backColor">曲线图背景色  
        /// <param name="foreColor">曲线图前景色  
        /// 

   

        public RealTimeImageMaker(int width, int height, Color backColor, Color foreColor)
        {
            this.width = width;
            this.height = height;
            this.backColor = backColor;
            this.foreColor = foreColor;
           
            pointListF = new PointF[width];
            PointF tempPointF;
            pointListF2 = new PointF[width];
            pointListF3 = new PointF[width];
            pointListF4 = new PointF[width];
            

            types = new byte[width];
            types[0] = Convert.ToByte(PathPointType.Start);
            types[1] = Convert.ToByte(PathPointType.Line);

            //初始化曲线上的所有点坐标  
            for (int i = 0; i < width; i++)
            {
               
                tempPointF = new PointF();
                //曲线的横坐标沿x轴依次递增，在横向位置上每个像素都有一个点  
                tempPointF.X = i;
                //曲线上每个点的纵坐标  
                tempPointF.Y = height/2;
                pointListF[i] = tempPointF;
                pointListF2[i] = tempPointF;
                pointListF3[i] = tempPointF;
                pointListF4[i] = tempPointF;

            
                if (i < width - 2)
                {
                    types[i + 2] = types[i + 1];
                }
            }
        }
        /// <summary>  
        /// 获取当前依次连接曲线上每个点绘制成的曲线  
        /// </summary>  
        /// <returns></returns>  
        public Image GetCurrentCurve()
        {
            

            //currentImage = historyImage.Clone(new Rectangle(1, 0, width - 1, height), PixelFormat.Format24bppRgb);  
            currentImage = new Bitmap(width, height);
           
            Graphics g = Graphics.FromImage(currentImage);
            //坐标轴
            Graphics g1 = Graphics.FromImage(currentImage);
            

            PointF[] points = new PointF[width];
            points = pointListF;

            PointF[] points2 = new PointF[width];
            points2 = pointListF2;
            PointF[] points3 = new PointF[width];
            points3 = pointListF3;
            PointF[] points4 = new PointF[width];
            points4 = pointListF4;

    
            g.Clear(backColor);
            GraphicsPath path = new GraphicsPath(points, types);
            GraphicsPath path2 = new GraphicsPath(points2, types);
            GraphicsPath path3 = new GraphicsPath(points3, types);
            GraphicsPath path4 = new GraphicsPath(points4, types); 

            //绘制曲线图  
            g.DrawPath(new Pen(Color.Blue), path);

            g.DrawPath(new Pen(Color.Red), path2);
            g.DrawPath(new Pen(Color.Cyan), path3);
            g.DrawPath(new Pen(Color.Pink), path4);
            //g.DrawLines(new Pen(foreColor), pointList);
            //dinghan
            PaintCoordinates(g, g1);

            g.Dispose();
            return currentImage;
        }

        public void updatePointY(DataRow row)
        {
            // 生成Datarow
            tagInfo = DataDS.NewRow();
            tagInfo = row;
            //DataDS.Rows.Add(row);
            //Console.Out.WriteLine(tagInfo[1] + "--------------"); //tagInfo[1]为Doppler Shift(Hz);


            // 更新数据点
           
                PointF m_p;

                PointF m_p2; PointF m_p3; PointF m_p4;
                //将当前定位曲线图的坐标点前移，并且将横坐标减1，  
                //这样做的效果相当于移除当前第一个点  
                for (int i = 0; i < width - 1; i++)
                {
                    m_p = pointListF[i + 1];
                    pointListF[i] = new PointF(m_p.X - 1, m_p.Y);

                    m_p2 = pointListF2[i + 1];
                    pointListF2[i] = new PointF(m_p2.X - 1, m_p2.Y);
                    m_p3 = pointListF3[i + 1];
                    pointListF3[i] = new PointF(m_p3.X - 1, m_p3.Y);
                    m_p4 = pointListF4[i + 1];
                    pointListF4[i] = new PointF(m_p4.X - 1, m_p4.Y);
                }


                PointF tempPointRF = new PointF();
                PointF tempPointPF = new PointF();

                

                //新生成曲线图定位点的最后一个点的坐标 
                tempPointRF.X = width;
                tempPointPF.X = width;
                //曲线上每个点的纵坐标随机生成，但保证在显示区域之内 
                float tempRFY;
                float.TryParse(tagInfo[1].ToString(), out tempRFY);
                tempPointRF.Y = tempRFY * 5 + height / 4;

                float tempPhY;
                float.TryParse(tagInfo[7].ToString(), out tempPhY);
                tempPointPF.Y = tempPhY * 10+ height*3/4;


                if (tagInfo[3].ToString() == "1")  
                { 
                    //在最后再添加一个新坐标点  
                    pointListF[width - 1] = tempPointRF;
                    pointListF3[width - 1] = tempPointPF;
                }

                if (tagInfo[0].ToString() == "E102") 
                {
                    pointListF2[width - 1] = tempPointRF;
                    pointListF4[width - 1] = tempPointPF;
                }
          
        }



        private void PaintCoordinates(Graphics g, Graphics g1)//在panel上绘制坐标及坐标轴
        {
            #region 画坐标轴线
            //Graphics curveGph1 = curvePanel1.CreateGraphics();
            Pen curvePen1 = new Pen(Color.DimGray, 1);
            curvePen1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            Point point = new Point(40, 20);

            for (int locx = 40; locx <= 1000; )//画竖线
            {
                g.DrawLine(curvePen1, locx, point.Y - 15, locx, 500);
                locx += 25;
            }
            for (int locy = 0; locy <= 500; )//画横线
            {
                g.DrawLine(curvePen1, point.X - 45, locy, 1000, locy);
                locy += 50;
            }
            g.Dispose();
            curvePen1.Dispose();
            #endregion

            #region 画坐标值

            Font font1 = new Font("Times New Roman", 10, FontStyle.Bold);
            Font font2 = new Font("Times New Roman", 10, FontStyle.Bold);
            //g1.DrawString("0", font2, new SolidBrush(Color.DimGray), 10, 472);
            //for (int locx = 2; locx <= 30; locx += 2)
            //{
            //    g1.DrawString(locx.ToString(), font1, new SolidBrush(Color.DimGray), locx * 25 + 28, 472);
            //}

            for (int locy = 1; locy <= 10; locy++)
            {
                g1.DrawString(((locy-5)*10).ToString(), font2, new SolidBrush(Color.DimGray), 10, height - locy * 50);
            }

            g1.Dispose();
            #endregion
        }

       

      

    }






};



