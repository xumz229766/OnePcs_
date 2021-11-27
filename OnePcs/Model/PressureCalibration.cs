using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigureFile;
using HalconDotNet;
namespace _OnePcs
{
    public class PressureCalibration
    {
        public static List<MeasurePressure> lstPressureAndVL = new List<MeasurePressure>();//左吸笔压力和电压数据
        public static List<MeasurePressure> lstPressureAndVR = new List<MeasurePressure>();//右吸笔压力和电压数据
        public static HTuple homatL = new HTuple();
        public static HTuple homatR = new HTuple();
        public static double dInitPL = 0;
        public static double dInitPR = 0;
        public static void InitParam(string file)
        {
            string section = "AssemblePressureRelation";
            lstPressureAndVL.Clear();
            lstPressureAndVR.Clear();
           
             
            for (int k = 0; k < 10; k++)
            {
                MeasurePressure m = new MeasurePressure();
                m.InitParam(file, section, "PressureV1"  + k.ToString());
                lstPressureAndVL.Add(m);
                m = new MeasurePressure();
                m.InitParam(file, section, "PressureV2" + k.ToString());
                lstPressureAndVR.Add(m);
            }
            dInitPL = Convert.ToDouble(IniOperate.INIGetStringValue(file,section, "dInitPL", "0"));
            dInitPR = Convert.ToDouble(IniOperate.INIGetStringValue(file, section, "dInitPR", "0"));
            homatL = null;
            homatR = null;
           
         

        }
        public static bool SaveParam(string file)
        {
            string section = "AssemblePressureRelation";
            bool bFlag = true;
           
            for (int k = 0; k < 10; k++)
            {
                bFlag = bFlag && lstPressureAndVL[k].SaveParam(file, section, "PressureV1" + k.ToString());
                bFlag = bFlag && lstPressureAndVR[k].SaveParam(file, section, "PressureV2" + k.ToString());
            }
            bFlag = bFlag && IniOperate.INIWriteValue(file, section, "dInitPL", dInitPL.ToString());
            bFlag = bFlag && IniOperate.INIWriteValue(file, section, "dInitPR", dInitPR.ToString());
            return bFlag;
        }
        public static float GetVBySuctionL(double pressure)
        {


            HTuple outV = 1;
            try
            {
                if (homatL == null)
                {
                    createFunL();
                }
                HOperatorSet.GetYValueFunct1d(homatL, pressure, "constant", out outV);
            }
            catch (Exception)
            {


            }

            return (float)outV.D;

        }
        public static void createFunL()
        {
            HTuple x = new HTuple();
            HTuple y = new HTuple();
            int count = lstPressureAndVL.Count;
            for (int i = 0; i < count; i++)
            {
                if (lstPressureAndVL[i].V != 0)
                {
                    x = x.TupleConcat(lstPressureAndVL[i].Pressure);
                    y = y.TupleConcat(lstPressureAndVL[i].V);
                }

            }
            HTuple func;
            HOperatorSet.CreateFunct1dPairs(x, y, out func);
            homatL = func;
        }
        public static float GetVBySuctionR(double pressure)
        {


            HTuple outV = 1;
            try
            {
                if (homatR == null)
                {
                    createFunR();
                }
                HOperatorSet.GetYValueFunct1d(homatR, pressure, "constant", out outV);
            }
            catch (Exception)
            {


            }

            return (float)outV.D;

        }
        public static void createFunR()
        {
            HTuple x = new HTuple();
            HTuple y = new HTuple();
            int count = lstPressureAndVR.Count;
            for (int i = 0; i < count; i++)
            {
                if (lstPressureAndVR[i].V != 0)
                {
                    x = x.TupleConcat(lstPressureAndVR[i].Pressure);
                    y = y.TupleConcat(lstPressureAndVR[i].V);
                }

            }
            HTuple func;
            HOperatorSet.CreateFunct1dPairs(x, y, out func);
            homatR = func;
        }
    }
    public class MeasurePressure
    {
        public double V { get; set; }
        public double Pressure { get; set; }

        public void InitParam(string file, string section, string keyHead)
        {
            V = Convert.ToDouble(IniOperate.INIGetStringValue(file, section, keyHead + "_V", "0"));
            Pressure = Convert.ToDouble(IniOperate.INIGetStringValue(file, section, keyHead + "_Pressure", "0"));
        }
        public bool SaveParam(string file, string section, string keyHead)
        {
            bool bFlag = true;
            bFlag = bFlag && (IniOperate.INIWriteValue(file, section, keyHead + "_V", V.ToString()));
            bFlag = bFlag && (IniOperate.INIWriteValue(file, section, keyHead + "_Pressure", Pressure.ToString()));
            return bFlag;
        }
    }
}
