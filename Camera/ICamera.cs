using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PylonC.NET;
using HalconDotNet;
using ThridLibray;
using CLIDelegate;
namespace CameraSet
{
   public enum CameraName{
       CamUpL,
        CamDownL,
        CamUpR,
        CamDownR,
        CamUpBarrel,


    }
    public class CameraInfo {
       public string unique_name = "";
       public string user_name = "";
       public string ip_address = "";
       public string computerIp = "";
       public string serial = "";
    
   }
   public abstract class ICamera
    {
       public  static string Brand = "DaHua";
       public static uint CameraNum = 0;//�������
      // public CameraName cameraName;
       public string strDeviceName = "";
       public CameraInfo cameraInfo = null;
       public bool bTrigger = true;//����ģʽ
       public  Action<HObject> ProcessImage;//����ͼ���ί��
       public static List<CameraInfo> lstCamera = new List<CameraInfo>();//�����Ϣ
       public static Dictionary<CameraName, CameraInfo> dic_CameraInfo = new Dictionary<CameraName, CameraInfo>();
       public static Action<string> ShowInfo;
       //�ع�
       public double dExposure = 1000;
       public double dMaxExposure = 0;
       public double dMinExposure = 0;
       //����
       public double dGain = 300;
       public double dMaxGain = 0;
       public double dMinGain = 0;

       public bool bInit = false;//��ʼ�����״̬


       //public static Dictionary<CameraName,
       /// <summary>
       /// ��ȡ�������
       /// </summary>
       public static string[] getCameras()
       {
           if (Brand == "DaHua")
           {
               return GetCameraName();
           }
           try
           {

          
              HTuple aa;
               lstCamera.Clear();
               dic_CameraInfo.Clear();
               HTuple info,boardValues;

             // HOperatorSet.SetSystem("filename_encoding", "utf8");
               HOperatorSet.GetSystem("filename_encoding", out aa);
               //HSystem.SetSystem("filename_encoding", "locale");
               HOperatorSet.InfoFramegrabber("GigEVision", "info_boards", out info, out boardValues);

               //string boardValues1 = "";
               //boardValues1 = boardValues.S;
               int count = boardValues.Length;
               for (int i = 0; i < count; i++)
               {
                   string[] msg = boardValues[i].S.Split('|');
                   if (msg.Length > 10)
                   {
                       CameraInfo c = new CameraInfo();
                       c.unique_name = msg[0].Split(':')[1];
                       c.user_name = ToUtf8(msg[1].Split(':')[1].Trim());
                       c.ip_address = msg[2].Split(':')[1];
                       c.computerIp = msg[7].Split(':')[1];
                       c.serial = msg[10].Split(':')[1];
                       lstCamera.Add(c);
                       try
                       {
                           CameraName name = (CameraName)Enum.Parse(typeof(CameraName), c.user_name);
                           dic_CameraInfo.Add(name, c);
                       }
                       catch (Exception)
                       {
                       
                      
                       }
                   }
               
               }
           }
           catch (Exception)
           {

               
           }
           
           return null;
       }
      
         public static string[] GetCameraName()
        {
            List<IDeviceInfo> li = Enumerator.EnumerateDevices();
            
            List<string> lst = new List<string>();
            foreach (IDeviceInfo info in li)
            { 
                lst.Add(info.Name);
                
            }
            return lst.ToArray();
        }
      
       private static string ToUtf8(string str)
       {
          // return str;
          
            //Encoding utf8;  
            //Encoding gb2312;  
            //utf8 = Encoding.GetEncoding("UTF-8");  
            //gb2312 = Encoding.GetEncoding("GB2312"); 
         
            //byte[] gb = gb2312.GetBytes(str);
          // Encoding e = Encoding.Default;
           byte[] b = ASCIIEncoding.GetEncoding("GB2312").GetBytes(str);
          // byte[] b2 = ASCIIEncoding.UTF8.GetBytes(str);
           
          // byte[] b4 = ASCIIEncoding.Unicode.GetBytes(str);
          // byte[] b6 = ASCIIEncoding.GetEncoding("GBK").GetBytes(str);
          // byte[] b8 = ASCIIEncoding.ASCII.GetBytes(str);
          // byte[] b9 = ASCIIEncoding.GetEncoding("iso-8859-1").GetBytes(str);
          // // byte[] b = ASCIIEncoding.Unicode.GetBytes(str);
          // byte[] b1 = ASCIIEncoding.UTF8.GetBytes("�����1");
          // byte[] b3 = ASCIIEncoding.GetEncoding("gb2312").GetBytes("�����1");
          // byte[] b5 = ASCIIEncoding.Unicode.GetBytes("�����1");
          // byte[] b7 = ASCIIEncoding.GetEncoding("GBK").GetBytes("�����1");
          //// byte[] b5 = ASCIIEncoding.Unicode.GetBytes("�����1");
          // string str1 = b1.ToString() + b2.ToString() + b3.ToString() + b4.ToString() + b5.ToString() + b6.ToString() + b7.ToString() + b8.ToString()+ b9.ToString();
          // // gb = Encoding.Convert(gb2312,utf8,gb);  
           string strUtf = Encoding.UTF8.GetString(b);

           return strUtf;
         
       }
       public static CameraInfo getCameraInfoByName(string deviceName)
       {
           foreach (CameraInfo info in lstCamera)
           {
               if (info.user_name.Equals(deviceName))
               {
                   return info;
               }
           }
           return null;
       }
       /// <summary>
       /// �Ƴ�����ͼ���ί��
       /// </summary>
       public abstract void RemoveDelegate();
       /// <summary>
       /// ��ʼ�����
       /// </summary>
       ///  <param name="DeviceName">�����</param>
       /// <returns>�ɹ�����true,ʧ�ܷ���false</returns>
       public abstract bool InitCamera(string DeviceName);
       /// <summary>
       /// �����ع�
       /// </summary>
       /// <param name="value">�ع�ֵ</param>
       public abstract void SetExposure(double value);
       /// <summary>
       /// ��ȡ�����С�ع�ֵ
       /// </summary>
       /// <param name="minExposure">��С�ع�</param>
       /// <param name="maxExposure">����ع�</param>
       public abstract void GetMinMaxExposure(ref double minExposure, ref double maxExposure);
       /// <summary>
       /// ��������
       /// </summary>
       /// <param name="value">����ֵ</param>
       public abstract void SetGain(double value);
       /// <summary>
       /// ��ȡ�����С����ֵ
       /// </summary>
       /// <param name="minGain">��С����</param>
       /// <param name="maxGain">�������</param>
       public abstract void GetMinMaxGain(ref double minGain, ref double maxGain);

       /// <summary>
       /// ����
       /// </summary>
       public abstract void SoftTrigger();
       /// <summary>
       /// ���ô���ģʽ
       /// </summary>
       /// <param name="bTrigger">trueΪ����ģʽ��falseΪʵʱģʽ</param>
       public abstract void SetTriggerMode(bool bTrigger);
       /// <summary>
       /// �ر����
       /// </summary>
       public abstract void CloseCamera();

       public abstract void Release();
      
    }
}
