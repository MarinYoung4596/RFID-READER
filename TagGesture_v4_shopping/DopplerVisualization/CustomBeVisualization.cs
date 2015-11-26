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
using System.Collections;


namespace DopplerVisualization
{
    class CustomBeVisualization
    {
        private int width;//要生成的曲线图的宽度  
        private int height;//要生成的曲线图的高度  


        private Random random = new Random();//用于生成随机数  
        private Bitmap currentImage;//当前要绘制的图片  
        private Color backColor;//图片背景色  
        private Color foreColor;//图片前景色  
        byte[] types;

        int TagsCount = -1;
        List<String> tagNames = new List<String>();
        int margin = 10;
        int absMinV = 0;
        int absMaxV = 8;
        int PointCount = 3;
        private List<PhaseReguList> PhaseReguData = new List<PhaseReguList>();

        private List<PointF[]> PointList = new List<PointF[]>();


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
        ///// <summary>  
        ///// 构造函数，指定生成的曲线图的宽度和高度  
        ///// </summary>  
        ///// <param name="width">要生成的曲线图的宽度  
        ///// <param name="height">要生成的曲线图的高度  
        //public CustomBeVisualization(int width, int height, Hashtable tagsEPC)
        //    : this(width, height, TagsEPC, Color.Gray, Color.Blue)
        //{

        //}
        /// <summary>  
        /// 构造函数，指定生成的曲线图的宽度、高度及背景色和前景色  
        /// </summary>  
        /// <param name="width">要生成的曲线图的宽度  
        /// <param name="height">要生成的曲线图的高度  
        /// <param name="backColor">曲线图背景色  
        /// <param name="foreColor">曲线图前景色  
        /// 



        public CustomBeVisualization(int width, int height, List<String> tagsEPC, Color backColor, Color foreColor)
        {
            this.width = width;
            this.height = height;
            this.backColor = backColor;
            this.foreColor = foreColor;
            this.tagNames = tagsEPC;
            TagsCount = tagsEPC.Count;

            for (int tagId = 0; tagId < TagsCount; tagId++)
            {

                PointF[] pointListF = new PointF[width - 2 * margin];
                PointF tempPointF;
                float regionCenterY = GetRegionCenterLineY(tagId);
                for (int i = 0; i < width - 2 * margin; i++)
                {
                    tempPointF = new PointF();
                    tempPointF.X = i + margin;
                    tempPointF.Y = regionCenterY;
                    pointListF[i] = tempPointF;
                }
                PointList.Add(pointListF);
            }
            InitPathPointType();
            InitPhaseReguData();

        }

        void InitPhaseReguData()
        {
            for (int i = 0; i < TagsCount; i++)
            {
                PhaseReguList list = new PhaseReguList();
                for (int j = 0; j < PointCount; j++)
                {
                    DateTime time = new DateTime();
                    PlotTagInfo tagInfo = new PlotTagInfo(i, 0, time);
                    list.Insert(tagInfo);
                }
                PhaseReguData.Add(list);
            }
        }

        void InitPathPointType()
        {
            int regionWidth = GetRegionWidth();
            types = new byte[regionWidth];
            types[0] = Convert.ToByte(PathPointType.Start);
            for (int i = 1; i < regionWidth; i++)
            {
                types[i] = Convert.ToByte(PathPointType.Line);
            }
        }


        /// <summary>  
        /// 获取当前依次连接曲线上每个点绘制成的曲线  
        /// </summary>  
        /// <returns></returns>  
        public Image GetCurrentCurve()
        {
            int regionWidth = GetRegionWidth();
            //currentImage = historyImage.Clone(new Rectangle(1, 0, width - 1, height), PixelFormat.Format24bppRgb);  
            currentImage = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(currentImage);
            //坐标轴
            Graphics g1 = Graphics.FromImage(currentImage);


            g.Clear(backColor);
            for (int tagId = 0; tagId < TagsCount; tagId++)
            {
                PointF[] points = new PointF[regionWidth];
                points = PointList[tagId];
                GraphicsPath path = new GraphicsPath(points, types);

                //绘制曲线图  
                g.DrawPath(new Pen(Color.Red), path);
            }
            //g.DrawLines(new Pen(foreColor), pointList);
            //dinghan
            PaintCoordinates(g, g1);

            g.Dispose();
            return currentImage;
        }

        public void updatePointY(int tagId, float value)
        {
            // 更新数据点
            PointF _pointF;
            int regionWidth = GetRegionWidth();
            //将当前定位曲线图的坐标点前移，并且将横坐标减1，  
            //这样做的效果相当于移除当前第一个点  
            for (int i = 0; i < regionWidth - 1; i++)
            {
                _pointF = PointList[tagId][i + 1];
                PointList[tagId][i] = new PointF(_pointF.X - 1, _pointF.Y);
            }
            PointF newPointF = new PointF();
            //新生成曲线图定位点的最后一个点的坐标 
            newPointF.X = regionWidth;
            //曲线上每个点的纵坐标随机生成，但保证在显示区域之内 
            newPointF.Y = GetRegionCenterLineY(tagId) + GetRegionRelativeValue(value);

            //在最后再添加一个新坐标点  
            PointList[tagId][regionWidth - 1] = newPointF;

        }

        public void updatePointData(PlotTagInfo tagInfo)
        {
            int tagIdx = tagInfo.TagIdx;
            PhaseReguData[tagIdx].UpdateData(tagInfo);
            float value = PhaseReguData[tagIdx].CalculatePointValue();
            updatePointY(tagIdx, value);
        }

        float GetRegionRelativeValue(float value)
        {
            float h = GetRegionHeight();
            return value * (h / 2) / absMaxV;
        }



        private void PaintCoordinates(Graphics g, Graphics g1)//在panel上绘制坐标及坐标轴
        {
            #region 画坐标轴线
            ////Graphics curveGph1 = curvePanel1.CreateGraphics();
            //Pen curvePen1 = new Pen(Color.DimGray, 1);
            //curvePen1.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            //Point point = new Point(40, 20);

            //for (int locx = 40; locx <= width; )//画竖线
            //{
            //    g.DrawLine(curvePen1, locx, point.Y - 15, locx, height);
            //    locx += 25;
            //}
            //for (int locy = 0; locy <= height; )//画横线
            //{
            //    g.DrawLine(curvePen1, point.X - 45, locy, width, locy);
            //    locy += 50;
            //}
            //g.Dispose();
            //curvePen1.Dispose();
            #endregion

            #region 画区域值

            Font font1 = new Font("Times New Roman", 10, FontStyle.Bold);
            //g1.DrawString("0", font2, new SolidBrush(Color.DimGray), 10, 472);
            //for (int locx = 2; locx <= 30; locx += 2)
            //{
            //    g1.DrawString(locx.ToString(), font1, new SolidBrush(Color.DimGray), locx * 25 + 28, 472);
            //}

            for (int tagId = 0; tagId < TagsCount; tagId++)
            {
                g1.DrawString(tagNames[tagId], font1, new SolidBrush(Color.DimGray), GetRegionPointX() + 10, GetRegionPointY(tagId));
            }

            g1.Dispose();
            #endregion
        }

        float GetRegionPointY(int regionId)
        {
            float regionHeight = (height - margin) / TagsCount;
            return regionHeight * regionId + margin;
        }


        float GetRegionPointX()
        {
            return margin;
        }


        float GetRegionHeight()
        {
            return (height - margin) / TagsCount;
        }

        int GetRegionWidth()
        {
            return width - 2 * margin;
        }

        float GetRegionCenterLineY(int tagId)
        {
            float pointY = GetRegionPointY(tagId);
            return pointY + GetRegionHeight() / 2;
        }


    }

};

