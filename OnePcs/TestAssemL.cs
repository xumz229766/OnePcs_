using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _OnePcs
{
    public class TestAssemL :AssemLModule
    {
        private string strOut = ",左组装验证模块,Action,";

        private Point currentPoint = null;
        private double dDestPosX = 0;
        private double dDestPosY = 0;
        private double dDestPosZ = 0;
        private double dDestC = 0;
        private double dDestAssemPosX = 0;
        public static int pos = 1;
        public static int iTray = 1;
        public static string strType = "取标定块";
        public static bool bContinue = false;//取放连续测试
        public static int iTimes = 1;//连续取放测试次数
        public static int iCurrentTimes = 0;
        public static double dCenterRow = 1024;
        public static double dCenterCol = 1224;
        private BarrelModule camModule = new BarrelModule();
        public static bool bUseCamCenter = false;
        public TestAssemL()
        {
            lstAction.Clear();
            //放料
            lstAction.Add(ActionName._10Z轴到安全位);
            lstAction.Add(ActionName._10Z轴到位);
            lstAction.Add(ActionName._10X轴到下相机拍照位);
            lstAction.Add(ActionName._10X轴到位完成);
            //选择上相机组装基准位置
            lstAction.Add(ActionName._50动作选择);
            lstAction.Add(ActionName._20XY轴到拍照位);
            lstAction.Add(ActionName._20XY轴到位完成);
            lstAction.Add(ActionName._20相机拍照);
            lstAction.Add(ActionName._20相机拍照完成);
            lstAction.Add(ActionName._50选择结果);

            lstAction.Add(ActionName._10下相机拍照);
            lstAction.Add(ActionName._10下相机拍照完成);
            lstAction.Add(ActionName._10C轴旋转);

            lstAction.Add(ActionName._10镜筒XY轴到组装位);
            lstAction.Add(ActionName._10X轴到组装位);
            lstAction.Add(ActionName._10X轴到位完成);
            lstAction.Add(ActionName._10Z轴到组装位);
            lstAction.Add(ActionName._10Z轴到位);
            lstAction.Add(ActionName._10吸笔破真空);
            lstAction.Add(ActionName._10Z轴到安全位);
            lstAction.Add(ActionName._10Z轴到位);

            lstAction.Add(ActionName._10X轴到下相机拍照位);
            lstAction.Add(ActionName._10X轴到位完成);
            lstAction.Add(ActionName._20相机拍照);
            lstAction.Add(ActionName._20相机拍照完成);
            lstAction.Add(ActionName._50动作完成);
        }
        public override void Action(ActionName action, ref int step)
        {
            try
            {
                if (action.ToString().Contains("_20"))
                {

                    camModule.Action(action, ref step);

                }
                else if (action.ToString().Contains("_10"))
                    base.Action(action, ref step);
                else
                {
                    switch (action)
                    {
                        case ActionName._50动作选择:
                            BarrelModule.iCamTimes = 0;
                            CameraLModule.iCamTimes = 0;

                            if (bUseCamCenter)
                            {
                                AssemLModule.dAssemCenterRow = 1024;
                                AssemLModule.dAssemCenterCol = 1224;
                                step = lstAction.IndexOf(ActionName._50选择结果) + 1;
                                break;
                            }
                            else
                            {
                                camModule.currentPoint = ModelManager.BarrelParam.getCameraCoordinateByIndex(iTray, pos);
                                step = step + 1;
                            }
                            break;
                        case ActionName._50动作完成:

                            Run.runMode = RunMode.手动;
                            step = 0;

                            break;
                        case ActionName._50选择结果:
                            BarrelModule.iCamTimes = 0;
                            AssemLModule.dAssemCenterRow = ModelManager.BarrelParam.imgResultUp.CenterRow;
                            AssemLModule.dAssemCenterCol = ModelManager.BarrelParam.imgResultUp.CenterColumn;
                            step = step + 1;
                            break;

                    }

                }
            }
            catch (Exception)
            {


            }

        }

        public override void Action2()
        {

        }

        public override void Reset()
        {

        }
    }
}
