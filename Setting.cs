using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LSD_CLIENT_AQUA
{
    internal class Setting
    {
        uint _icalEn;
        bool _calEn;
        ushort _info1;
        ushort _info2;
        ushort _x16;
        ushort _y16;
        byte _calBypass;
        byte _convBypass;
        private byte[] _ethMac = new byte[6];
        private int[] _ethIp = new int[4];
        byte _laser;
        byte _syncMode;
        byte _sensRestart;
        byte _imgSubtr;
        short _expo1;
        short _expo2;
        byte _sensMode;
        byte _sensGain;

        byte _subXs;
        byte _subXe;
        short _subYs;
        short _subYe;
        byte _subpixSideEn;
        byte _subpixSideThres;
        private byte[] _locmaxDist = new byte[3];     // тест на соответствие условию локального максимума, три пары расстояний (в пикселях [0-31], в каждую сторону)
        private byte[] _locmaxThres = new byte[3];    // и порогов (уровень, [0-255])
        byte _findMaxMult;      // множитель (todo: текущего?) максимума при поиске сигнала  0x00 = 1/16, ... 0x04 = 1, ... 0x08 = 16
        byte _detachWindow;      // фильтр, убирающий отдельностоящие точки, окно [3-31]
        byte _detachPoints;      // минимальное кол-во находящихся рядом точек
        short _detachDistance;   // максимальное расстояние, при котором точки считаются близкими, мм / (коэф (дробных бит))
        private byte _medianWindow;      // размер окна медианного фильтра [1-31]. 0x00 - допустимое значение, фильтр выключен
        private short[] _convCoef = new short[16];     // коэффициенты симметричной нечетной свертки, знаковое число в дополнительном коде [-32768, 32767]. [0] - центральный.
        private short[] _xmod = new short[8];
        int _period;
        private byte[] _buf = new byte[255];
       

        public Setting()
        {
           
        }
        
        public void SettingFrom(byte[]bufSetting)
        {
            for (int i = 0; i < 6; i++)
            {
                _ethMac[i] = bufSetting[i];
            }
            for (int i = 0; i < 4; i++) _ethIp[i] = Convert.ToInt32(bufSetting[i + 7]);
            _laser = bufSetting[0x20];
            _syncMode = bufSetting[0x22];
            _period = (bufSetting[0x25] << 16) + (bufSetting[0x26] << 8) + (bufSetting[0x27] << 0);
            _subXs = bufSetting[0x34];
            _subXe = bufSetting[0x35];
            _convBypass = bufSetting[0x40];
            _imgSubtr = bufSetting[0x41];
            _sensRestart = bufSetting[0x42];
            _calBypass = bufSetting[0x50];
            _subpixSideEn = bufSetting[0x51];
            _subpixSideThres = bufSetting[0x52];
            _findMaxMult = bufSetting[0x53];
            _locmaxDist[0] = bufSetting[0x54];
            _locmaxDist[1] = bufSetting[0x55];
            _locmaxDist[2] = bufSetting[0x56];
            _locmaxThres[0] = bufSetting[0x57];
            _locmaxThres[1] = bufSetting[0x58];
            _locmaxThres[2] = bufSetting[0x59];

            _medianWindow = bufSetting[0x60];
            _detachWindow = bufSetting[0x64];
            _detachPoints = bufSetting[0x65];

            for (int i = 0; i < 8; i++) _xmod[i] = Convert.ToInt16(bufSetting[0x70 + i * 2]);// make_uint16(_bufINFO[0x70 + i * 2]);
            for (int i = 0; i < 16; i++) _convCoef[i] = Convert.ToInt16(bufSetting[0xDE - i * 2]);
            


        }

        public void SettingTo(byte[] bufSetting)
        {
            for (int i = 0; i < 255; i++) bufSetting[i] = 0x00;
            for (int i = 0; i < 6; i++) bufSetting[0x00 + i] = _ethMac[i];
            bufSetting[0x06] = (byte)(_ethIp[3]);
            bufSetting[0x07] = (byte)(_ethIp[2]);
            bufSetting[0x08] = (byte)(_ethIp[1]);
            bufSetting[0x09] = (byte)(_ethIp[0]);

            bufSetting[0x20] = _laser;
            bufSetting[0x22] = _syncMode;
            bufSetting[0x25] = (byte)(_period >> 16);
            bufSetting[0x26] = (byte)(_period >> 8);
            bufSetting[0x27] = (byte)(_period >> 0);
            bufSetting[0x28] = (byte)(_expo1 >> 8);
            bufSetting[0x29] = (byte)(_expo1 >> 0);
            //bufSetting[0x2C] = _re;

            bufSetting[0x30] = _sensGain ;
            bufSetting[0x33] = _sensMode;
            bufSetting[0x34] = _subXs;
            bufSetting[0x35] = _subXe;
            bufSetting[0x36] = (byte)(_subYs >> 8);
            bufSetting[0x37] = (byte)(_subYs >> 0);
            bufSetting[0x38] = (byte)(_subYe >> 8);
            bufSetting[0x39] = (byte)(_subYe >> 0);

            bufSetting[0x40] = _convBypass;
            bufSetting[0x41] = _imgSubtr;
            bufSetting[0x42] = _sensRestart;

            bufSetting[0x50] = _calBypass;
            bufSetting[0x51] = _subpixSideEn;
            bufSetting[0x52] = _subpixSideThres;
            bufSetting[0x53] = _findMaxMult;
            bufSetting[0x54] = _locmaxDist[0];
            bufSetting[0x55] = _locmaxDist[1];
            bufSetting[0x56] = _locmaxDist[2];
            bufSetting[0x57] = _locmaxThres[0];
            bufSetting[0x58] = _locmaxThres[1];
            bufSetting[0x59] = _locmaxThres[2];

            bufSetting[0x60] = _medianWindow;
            bufSetting[0x64] = _detachWindow;
            bufSetting[0x65] = _detachPoints;
            bufSetting[0x66] = (byte)(_detachDistance >> 8);
            bufSetting[0x67] = (byte)(_detachDistance >> 0);

            for (int i = 0; i < 8; i++)
            {
                bufSetting[0x70 + i * 2 + 0] = (byte)(_xmod[i] >> 8);
                bufSetting[0x70 + i * 2 + 1] = (byte)(_xmod[i] >> 0);
            }

            for (int i = 0; i < 16; i++)
            {
                bufSetting[0xDE - i * 2 + 0] = (byte)(_convCoef[i] >> 8);
                bufSetting[0xDE - i * 2 + 1] = (byte)(_convCoef[i] >> 0);
            }
        }


    }
}
