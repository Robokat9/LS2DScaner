using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using System.Runtime;
using System.IO;
using System.Threading;

namespace LSD_CLIENT_AQUA
{
    public class ScanerLsd
    {
        public bool  enableLSD;
        private const int Port = 11681;
        readonly IPAddress _scanerAddress = IPAddress.Parse("10.245.8.135");
        Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public UdpClient _udpClient = new UdpClient(Port);
        //private IPEndPoint _scanerEndPoint = new IPEndPoint(IPAddress.Parse("10.245.8.135"), Port);
        private IPEndPoint _remoteIpEndPoint = null;
        byte[] _buf = new byte[4];
        byte[] _bufData = new byte[255];
        private int _resultCmd;
        private int _dOffset = 8;
        uint _icalEn;
        bool _calEn;
        bool on = true;
        public int countV1 = 0;
        public int countV2 = 0;
        public PointF[] points = new PointF[1024];
        public PointF[] pointsV1 = new PointF[1024];
        public PointF[] pointsV2 = new PointF[1024];
        public Vector2[] v1_arr = new Vector2[1024];
        public Vector2[] v2_arr = new Vector2[1024];

        public Vector2 vect_v1 = new Vector2();
        public Vector2 vect_v2 = new Vector2();
        public double[] massH_v1;
        public double[] massH_v2;
        public double m_v1;
        public double m_v2;
        public double b_v1;
        public double b_v2;
        public PointF DotPoint;
        public PointF DotPoint1_v1;
        public PointF DotPoint2_v1;
        public PointF DotPoint1_v2;
        public PointF DotPoint2_v2;
        public double angl_v1 = 0;
        double angleBtw1 = 0;
        double angleBtw2 = 0;
        private string namefile;
        public void ConnectLsd()
        {
            try

            {
               _udpClient.Connect(_scanerAddress, Port);
                if (_udpClient != null)
                {
                    Console.Write("Scaner is ON");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }          
            
        }

        public void DisconnectLsd()
        {
            _udpClient.Dispose();
            _udpClient.Close();
        }

        public void EnableLsd()
        {
            byte[] _bufe = new byte[4];
            _bufe[0] = 0x81;////
            _bufe[1] = 0x20;
            _bufe[2] = 0x01;
            _bufe[3] = 0x01; //включение  сканера
            
                _resultCmd = _udpClient.Send(_bufe, _bufe.Length);
            enableLSD = true;
                Console.Write("Scaner is ON");
           
        }

        public void DisableLsd()
        {
        byte[] _bufd = new byte[4];
            _bufd[0] = 0x81;////
            _bufd[1] = 0x20;
            _bufd[2] = 0x01;
            _bufd[3] = 0x00; //выключение  сканера
            _resultCmd = _udpClient.Send(_bufd, _bufd.Length);
            enableLSD = false;
            Console.Write("Scaner is OFF");
        }

        public  double StartScan()
        {
            angl_v1 = 0;


            try
            {
             _buf = GetSetting();
                if (_buf != null)
                {
                    _buf[0] = 0x2D;
                    _buf[1] = 0xA8; ////

                    _resultCmd = _udpClient.Send(_buf, _buf.Length);
                    if (_resultCmd == 0)
                    {

                    }
                    for (int i = 0; i < _bufData.Length; i++)
                    {
                        _bufData[i] = 0;
                    }
                    _bufData = _udpClient.Receive(ref _remoteIpEndPoint);

                    string results = Encoding.UTF8.GetString(_bufData);

                    int frac_bits = 5;
                    _icalEn = _bufData[0x50];
                    if (_icalEn == 0)
                    {
                        _calEn = true;
                    }
                    else
                    {
                        _calEn = false;
                    }

                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].X = 0;
                        points[i].Y = 0;
                    }

                    double denom = _calEn ? Math.Pow(2, frac_bits) : 64.0; //5
                    var ins = BitConverter.GetBytes(_bufData[0]);
                    var ins1 = BitConverter.GetBytes(_bufData[4]);
                    //listBox1.Items.Clear();
                    double y = 0.0;
                    int count = 0;
                    //CleanList();

                    for (int i = 0; i < 1024; i++)
                    {
                        int pos_0 = _dOffset + i * 4 + 0;
                        int pos_1 = _dOffset + i * 4 + 1;
                        int pos_2 = _dOffset + i * 4 + 2;
                        int pos_3 = _dOffset + i * 4 + 3;

                        var valuez = BitConverter.GetBytes((_bufData[pos_0] << 8) | _bufData[pos_1]);
                        var valuex = BitConverter.GetBytes((_bufData[pos_2] << 8) | _bufData[pos_3]);
                        var zl = BitConverter.ToInt16(valuez, 0) / denom;
                        var xl = BitConverter.ToInt16(valuex, 0) / denom;

                        if (zl != -0.015625 && xl != -0.015625)
                        {
                            points[count].X = (float)xl;
                            points[count].Y = (float)zl;
                           
                            count++;
                        }
                    }

                    //WriteToFile();
                    int cellmin = 0;
                    var min = points[0].Y;
                     cellmin = GetMinElement(points, count);
                    countV1 = 0;
                    pointsV1 = SetListPointV1(cellmin, count, points);
                    pointsV2 = SetListPointV2(cellmin, 0, points);

                    angl_v1 = CreateVectors(pointsV1, pointsV2);
                }
                else
                {
                    Console.WriteLine("Error Connection LSD 1");
                }
                return angl_v1;
            }
            catch (Exception e) when (_bufData.Length < 4104)
            {

                return 0;
                Console.WriteLine(e.Message);

            }
            catch (Exception e) 
            {
                return 0;
                Console.WriteLine(e.Message);
            }
            return angl_v1;
     
        }

        public void WriteToFile()
        {
            var rnd = new Random();
            int val = rnd.Next(1, 1000);
            StreamWriter strw1 = new StreamWriter("D:\\scandata\\V1_" + val + ".txt");
            StreamWriter strw2 = new StreamWriter("D:\\scandata\\V2_" + val + ".txt");
            StreamWriter strw3 = new StreamWriter("D:\\scandata\\V_.txt");
            for (int i = 0; i < pointsV1.Length; i++)
            {
                strw1.WriteLine(pointsV1[i].ToString());
            }
            for (int i = 0; i < pointsV2.Length; i++)
            {
                strw2.WriteLine(pointsV2[i].ToString());
            }
            for (int i = 0; i < points.Length; i++)
            {
                strw3.WriteLine(points[i].ToString());
            }
            strw1.Close();
            strw2.Close();
            strw3.Close();

        }
        public static double AngleBetweenVectors(Vector2 vector1, Vector2 vector2)
        {
            try
            {
                // Calculate the dot product of the two vectors
                double dotProduct = (vector1.X * vector2.X) + (vector1.Y * vector2.Y);

                // Calculate the magnitudes of the two vectors
                double magnitude1 = Math.Sqrt((vector1.X * vector1.X) + (vector1.Y * vector1.Y));
                double magnitude2 = Math.Sqrt((vector2.X * vector2.X) + (vector2.Y * vector2.Y));

                // Calculate the angle between the two vectors in radians
                double angleInRadians = Math.Acos(dotProduct / (magnitude1 * magnitude2));

                // Convert the angle to degrees and return it
                return angleInRadians * (180 / Math.PI);
            }
            catch (Exception e)
            {
                // Log the error and return 0
                Console.WriteLine($"Error: {e.Message}");
                return 0;
            }
        }
        public PointF[] SetListPointV1(int start, int end, PointF[] points)
        {
            pointsV1 = new PointF[1024];
            int countV1 = 0;
            for (int k = start; k < end; k++)
            {
                if (points[k].X != 0 && points[k].Y != 0)
                {
                    countV1++;
                }

            }
            pointsV1 = new PointF[countV1];
            countV1 = 0;
            for (int k = start; k < end; k++)
            {
               // if (points[k].IsEmpty == false)
                     if (points[k].X != 0 && points[k].Y != 0)
                {
                    pointsV1[countV1].X = (float)points[k].X;
                    pointsV1[countV1].Y = (float)points[k].Y;
                    countV1++;
                }
            }            
           
            return pointsV1;
        }

        public PointF[] SetListPointV2(int start, int end, PointF[] points)
        {
            int countV2 = 0;
            for (int k = start; k >= end; k--)
            {
                if (points[k].X != 0 && points[k].Y != 0)
                {
                    countV2++;
                }


            }
            pointsV2 = new PointF[countV2];
            countV2 = 0;
            for (int k = start; k >= end; k--)
            {

                if (points[k].X != 0 && points[k].Y != 0)
                {
                    pointsV2[countV2].X = (float)points[k].X;
                    pointsV2[countV2].Y = (float)points[k].Y;
                    countV2++;
                }
            }
            return pointsV2;
        }

        public void getLines(PointF point1, PointF point2, int selectV)
        {
            double k, b;
            k = (point2.Y - point1.Y) / (point2.X - point1.X);
            b = -(point1.X * point2.Y - point2.X * point1.Y) / (point2.X - point1.X);
            switch (selectV)
            {
                case 1:
                {
                    b_v1 = b;
                    m_v1 = k;
                }
                    ; break;
                case 2:
                {
                    b_v2 = b;
                    m_v2 = k;
                }
                    ;
                    break;

            }

        }

        public byte[] GetSetting()
        {
            Setting setting = new Setting();

            _buf[0]= 0x83;
            _resultCmd = _udpClient.Send(_buf, 1);
                _buf= _udpClient.Receive(ref _remoteIpEndPoint);
            setting.SettingFrom(_buf);
            return _buf;
        }

        public double CreateVectors(PointF[] _points_v1, PointF[] _points_v2)
        {
            PointF[] _points_buf1 = new PointF[1024];
            PointF[] _points_buf2 = new PointF[1024];
            int iteration_koef = 0;
            double angleBtw = 0;
            double max_y_v1 = 0;
            double max_y_v2 = 0;
            int countIter = 0;
            int countEnd = 2;
            Vector2 vect2 = new Vector2(100, 0);


            if ((_points_v1.Length >= 10) && (_points_v2.Length >= 10))
            {
                while (iteration_koef < 60)
                {

                    _points_buf1 = GetDistance(_points_v1[countIter], _points_v1[_points_v1.Length - countEnd],
                        _points_v1);
                    countIter += 1;

                    iteration_koef = _points_buf1.Length * 100 / _points_v1.Length;

                    getMNK(_points_buf1, _points_buf1.Length, 1);
                    if (countIter > _points_buf1.Length / 2)
                    {
                        countEnd += 1;
                        countIter = 0;
                    }

                    if (countEnd == _points_v1.Length - 2)
                    {
                        Console.WriteLine("Panel_not_found");
                        return 0;
                    }
                }

                Console.WriteLine("iteration_koef v1 = " + iteration_koef);
                Console.WriteLine("_points_buf1.Len v1 = " + _points_buf1.Length);
                max_y_v1 = _points_buf1[GetMaxElement(_points_buf1, _points_buf1.Length)].Y;
                DotPoint.X = _points_buf1[GetMinElement(_points_buf1, _points_buf1.Length)].X;
                DotPoint.Y = _points_buf1[GetMinElement(_points_buf1, _points_buf1.Length)].Y;
                DotPoint1_v1.X = (float)(DotPoint.X); //min element X,Y
                DotPoint1_v1.Y = (float)DotPoint.Y;
                DotPoint2_v1.X = (float)((max_y_v1 - b_v1) / m_v1);
                DotPoint2_v1.Y = (float)max_y_v1;
                vect_v1.X = DotPoint2_v1.X - DotPoint1_v1.X;
                vect_v1.Y = DotPoint2_v1.Y - DotPoint1_v1.Y;
                angleBtw = Math.Round(AngleBetweenVectors(vect_v1, vect2), 3);

                countIter = 0;
                countEnd = 2;
                iteration_koef = 0;
                while (iteration_koef < 60)
                {

                    _points_buf2 = GetDistance(_points_v2[countIter], _points_v2[_points_v2.Length - countEnd],
                        _points_v2);
                    countIter += 1;

                    iteration_koef = _points_buf2.Length * 100 / _points_v2.Length;

                    getMNK(_points_buf2, _points_buf2.Length, 2);

                    if (countIter > _points_buf2.Length / 2)
                    {
                        countEnd += 1;
                        countIter = 0;

                    }

                    if (countEnd == _points_v2.Length - 2)
                    {
                        Console.WriteLine("Panel_not_found");
                         return 0;
                        
                    }
                }
                Console.WriteLine("iteration_koef v2 " + iteration_koef);
                Console.WriteLine("_points_buf2.Len v2 = " + _points_buf2.Length);
                max_y_v2 = _points_buf2[GetMaxElement(_points_buf2, _points_buf2.Length)].Y;
                DotPoint.X = _points_buf2[GetMinElement(_points_buf2, _points_buf2.Length)].X;
                DotPoint.Y = _points_buf2[GetMinElement(_points_buf2, _points_buf2.Length)].Y;
                DotPoint1_v2.X = (float)(DotPoint.X); //min element X,Y
                DotPoint1_v2.Y = (float)DotPoint.Y;
                DotPoint2_v2.X = (float)((max_y_v2 - b_v2) / m_v2);
                DotPoint2_v2.Y = (float)max_y_v2;
                vect_v2.X = DotPoint2_v2.X - DotPoint1_v2.X;
                vect_v2.Y = DotPoint2_v2.Y - DotPoint1_v2.Y;
                angleBtw1 = Math.Round(AngleBetweenVectors(vect_v2, vect2), 3);

                angleBtw2 = AngleBetweenVectors(vect_v1, vect_v2);
                //DotPoint = dotPoi(b_v1, m_v1, 0, 0);
                DotPoint = dotPoi(b_v1, m_v1, b_v2, m_v2);
                angleBtw2 = AngleBetweenVectors(vect_v1, vect_v2);
                
                Console.WriteLine("angle v1 = " + angleBtw);
                Console.WriteLine("angle v2 = " + angleBtw1);
                Console.WriteLine("angle v1&v2 = " + angleBtw2);

            }

            return angleBtw;
        }

        public double CreateVectors1(PointF[] _points_v1, PointF[] _points_v2)
        {
            PointF[] _points_buf1 = new PointF[1024];
            PointF[] _points_buf2 = new PointF[1024];
            int iteration_koef = 0;
            double angleBtw = 0;
            double max_y_v1 = 0;
            double max_y_v2 = 0;
            int countIter = 0;
            int countEnd = 2;
            Vector2 vect2 = new Vector2(100, 0);



            while (iteration_koef < 70)
            {
               
                    _points_buf1 = GetDistance(_points_v1[countIter], _points_v1[_points_v1.Length - countEnd],
                        _points_v1);
                    countIter += 1;
                    
                    iteration_koef = _points_buf1.Length * 100 / _points_v1.Length;
                    
                    getMNK(_points_buf1, _points_buf1.Length, 1);
                    if (countIter > _points_buf1.Length / 2)
                    {
                        countEnd += 1;
                        countIter = 0;

                    }

                    if (countEnd == _points_v1.Length - 2)
                    {
                        return 0;
                    }


                

            }
            Console.WriteLine("iteration_koef v1 = " + iteration_koef);
            Console.WriteLine("_points_buf1.Len v1 = " + _points_buf1.Length);
            max_y_v1 = _points_buf1[GetMaxElement(_points_buf1, _points_buf1.Length)].Y;


           // DotPoint.X = _points_buf1[GetMinElement(_points_buf1, _points_buf1.Length)].X;
            //DotPoint.Y = _points_buf1[GetMinElement(_points_buf1, _points_buf1.Length)].Y;
            DotPoint1_v1.X = (float)(DotPoint.X); //min element X,Y
            DotPoint1_v1.Y = (float)DotPoint.Y;
            DotPoint2_v1.X = (float)((max_y_v1 - b_v1) / m_v1);
            DotPoint2_v1.Y = (float)max_y_v1;
            vect_v1.X = DotPoint2_v1.X - DotPoint1_v1.X;
            vect_v1.Y = DotPoint2_v1.Y - DotPoint1_v1.Y;
            angleBtw = Math.Round(AngleBetweenVectors(vect_v1, vect2), 3);

            countIter = 0;
            countEnd = 2;
            iteration_koef = 0;
            while (iteration_koef < 70)
            {

                _points_buf2 = GetDistance(_points_v2[countIter], _points_v2[_points_v2.Length - countEnd],
                    _points_v2);
                countIter += 1;

                iteration_koef = _points_buf2.Length * 100 / _points_v2.Length;

                getMNK(_points_buf2, _points_buf2.Length, 2);

                if (countIter > _points_buf2.Length / 2)
                {
                    countEnd += 1;
                    countIter = 0;

                }

                if (countEnd == _points_v2.Length - 2)
                {
                    return 0;
                }


            }

            Console.WriteLine("iteration_koef v2 " + iteration_koef);
            Console.WriteLine("_points_buf2.Len v2 = " + _points_buf2.Length);
            max_y_v2 = _points_buf2[GetMaxElement(_points_buf2, _points_buf2.Length)].Y;
            //DotPoint.X = _points_buf2[GetMinElement(_points_buf2, _points_buf2.Length)].X;
            //DotPoint.Y = _points_buf2[GetMinElement(_points_buf2, _points_buf2.Length)].Y;
            DotPoint1_v2.X = (float)(DotPoint.X); //min element X,Y
            DotPoint1_v2.Y = (float)DotPoint.Y;
            DotPoint2_v2.X = (float)((max_y_v2 - b_v2) / m_v2);
            DotPoint2_v2.Y = (float)max_y_v2;
            vect_v2.X = DotPoint2_v2.X - DotPoint1_v2.X;
            vect_v2.Y = DotPoint2_v2.Y - DotPoint1_v2.Y;
            angleBtw1 = Math.Round(AngleBetweenVectors(vect_v2, vect2), 3);
            angleBtw2 = AngleBetweenVectors(vect_v1, vect_v2);
            //if (angleBtw2 < 95 && angleBtw2 > 85)
            //{
                DotPoint = dotPoi(b_v1, m_v1, b_v2, m_v2);
                angleBtw2 = AngleBetweenVectors(vect_v1, vect_v2);
                

           // }
            Console.WriteLine("angle v1 = " + angleBtw);
            Console.WriteLine("angle v2 = " + angleBtw1);
            Console.WriteLine("angle v1&v2 = " + angleBtw2);

            //_points_v2 = GetDistance(_points_v2[1], _points_v2[5], _points_v2);
            //getMNK(_points_v1, _points_v1.Length, 1);
            //getMNK(_points_v2, _points_v2.Length, 2);
            //if ((_points_v1.Length > 20) || (_points_v2.Length > 20))
            //{
            //    if (_points_v1.Length > 10)
            //    {
            //        getMNK(_points_v1, _points_v1.Length, 1);

            //    }
            //    else
            //    {
            //        getLines(_points_v1[0], _points_v1[_points_v1.Length - 1], 1);
            //    }

            //max_y_v1 = _points_v1[GetMaxElement(_points_v1, _points_v1.Length)].Y;
            //    DotPoint1_v1.X = (float)(DotPoint.X); //min element X,Y
            //    DotPoint1_v1.Y = (float)DotPoint.Y;
            //    DotPoint2_v1.X = (float)((max_y_v1 - b_v1) / m_v1);
            //    DotPoint2_v1.Y = (float)max_y_v1;
            //    vect_v1.X = DotPoint2_v1.X - DotPoint1_v1.X;
            //    vect_v1.Y = DotPoint2_v1.Y - DotPoint1_v1.Y;
            //    angleBtw = Math.Round(AngleBetweenVectors(vect_v1, vect2), 3);


            //    if (_points_v2.Length > 10)
            //    {
            //        getMNK(_points_v2, _points_v2.Length, 2);
            //    }
            //    else
            //    {
            //        getLines(_points_v2[0], _points_v2[_points_v2.Length - 1], 2);
            //    }

            //     max_y_v2 = _points_v2[GetMaxElement(_points_v2, _points_v2.Length)].Y;
            //    DotPoint1_v2.X = (float)(DotPoint.X); //min element X,Y
            //    DotPoint1_v2.Y = (float)DotPoint.Y;
            //    DotPoint2_v2.X = (float)((max_y_v2 - b_v2) / m_v2);
            //    DotPoint2_v2.Y = (float)max_y_v2;
            //    vect_v2.X = DotPoint2_v2.X - DotPoint1_v2.X;
            //    vect_v2.Y = DotPoint2_v2.Y - DotPoint1_v2.Y;
            //    angleBtw1 = Math.Round(AngleBetweenVectors(vect_v2, vect2), 3);




            //    DotPoint = dotPoi(b_v1, m_v1, b_v2, m_v2);

            //    angleBtw2 = AngleBetweenVectors(vect_v1, vect_v2);

                //_points_v1 = GetDistance(DotPoint1_v1, DotPoint2_v1, _points_v1);
                //_points_v2 = GetDistance(DotPoint1_v2, DotPoint2_v2, _points_v2);
               // DrawLinePoints(_points_v1, _points_v2, 2);
               
                // DrawLinePoints(_points_buf1, _points_buf2, 1);

        //}
            //else
            //{

            //    double c = 2;
            //}

            return angleBtw;
        }

        public int GetMaxElement(PointF[] _points, int _len)
        {
            var max = _points[0].Y;
            var cellmin = 0;
            for (int k = 0; k < _len; k++)
            {
                if (max < _points[k].Y)
                {
                    max = _points[k].Y;
                    cellmin = k;
                }
            }

            return cellmin;

        }
        public int GetMinElement(PointF[] _points, int _len)
        {
            var min = _points[0].Y;
            var cellmin = 0;
            
            for (int k = 0; k < _len; k++)
            {
                if (_points != null && _points.Length > 0) //02_11_2023
                {
                    if (min > _points[k].Y)
                    {

                        min = _points[k].Y;
                        cellmin = k;
                    }
                }
            }

            return cellmin;

        }
        public PointF[] GetDistance(PointF point1, PointF point2, PointF[] points)
        {
            double[] DistanceValues = new double[points.Length];
            double[] DistanceValues1 = new double[1024];
            PointF[] pointnew = new PointF[1024];
            int goodValues = 0;
            int count = 0;
            for (int k = 0; k < points.Length; k++)
            {

                if (CheckPoints(point1, point2, points[k]) <= 0.2)
                {
                    DistanceValues[k] = CheckPoints(point1, point2, points[k]);
                    if (points[k].IsEmpty==false)
                    {
                        pointnew[goodValues] = points[k];
                        goodValues++;
                    }

                }

            }
            PointF[] pointnew1 = new PointF[goodValues];
            DistanceValues1 = new double[goodValues];
            for (int i = 0; i < points.Length; i++)
            {
                if (DistanceValues[i] > 0.0)
                {
                    DistanceValues1[count] = DistanceValues[i];
                    pointnew1[count] = points[i];
                    count++;
                }
            }
            pointnew1 = new PointF[count];
            for (int i = 0; i < count; i++)
            {
                if (pointnew[i].IsEmpty == false)
                {
                    pointnew1[i] = pointnew[i];
                }
            }

            return pointnew1;
        }

        public double CheckPoints(PointF point1, PointF point2, PointF dotpoint)
        {
            return Math.Abs(((point2.X - point1.X) * (dotpoint.Y - point1.Y) - (point2.Y - point1.Y) * (dotpoint.X - point1.X)) / Math.Sqrt(((Math.Pow((point2.X - point1.X), 2) + Math.Pow((point2.Y - point1.Y), 2)))));

        }
        public void getMNK(PointF[] points, int len, int selectV)
        {
            int countxy = 0;
            float X_ = 0;
            float Y_ = 0;
            double m1 = 0;
            double m2 = 0;
            double m3 = 0;
            double b = 0;
            for (int i = 0; i < len; i++)
            {
                if (points[i].IsEmpty==false)
                {
                    X_ = X_ + points[i].X;
                    Y_ = Y_ + points[i].Y;
                    countxy++;
                }
            }

            X_ = X_ / countxy;
            Y_ = Y_ / countxy;

            for (int i = 0; i < len; i++)
            {
                if (points[i].IsEmpty==false)
                {
                    m1 += (points[i].X - X_) * (points[i].Y - Y_);
                }
            }

            for (int i = 0; i < len; i++)
            {
                m2 += Math.Pow((points[i].X - X_), 2);
            }

            m3 = m1 / m2;
            b = Y_ - m3 * X_;

            if (selectV == 1)
            {
                m_v1 = m3;
                b_v1 = b;
            }
            else
            {
                m_v2 = m3;
                b_v2 = b;
            }

        }
        public PointF dotPoi(double b1, double m1, double b2, double m2)
        {
            PointF _dotpoi = new PointF();

            double x = -(b1 - b2) / (m1 - m2);
            double y = m1 * x + b1;

            x = Math.Round(x, 3);
            y = Math.Round(y, 3);
            _dotpoi.X = (float)x;
            _dotpoi.Y = (float)y;
            return _dotpoi;
        }
    }
}
