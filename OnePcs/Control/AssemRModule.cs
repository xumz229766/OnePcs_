using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion;
using HalconDotNet;
namespace _OnePcs
{
    public class AssemRModule:ActionModule
    {
        private double dVelResetXY = 200;
        private double dVelResetZ = 100;
        private string strOut = ",右组装模块,Action,";

        private Point currentPoint = null;
        private double dDestPosX = 0;
        private double dDestPosY = 0;
        private double dDestPosZ = 0;
        private double dDestC = 0;
        private double dDestAssemPosX = 0;

        public static bool bAssembFlag = false;//是否在组装标志
        public static int iResetStep = 0;
        public static double dDiffX = 0;//组装时镜筒X移动的距离
        public static double dDiffY = 0;//组装时镜筒Y移动的距离
        public static int iCamTimes = 0;
        public static double dAssemCenterRow = 0;//组装时中心行
        public static double dAssemCenterCol = 0;//组装时中心列
        public static double dRotateAngle = 0;//旋转角度
        public AssemRModule()
        {
            lstAction.Clear();
            lstAction.Add(ActionName._10开始取料);
            lstAction.Add(ActionName._10X轴到取料位);
            lstAction.Add(ActionName._10XY到取料位);
            lstAction.Add(ActionName._10XY到位完成);
            lstAction.Add(ActionName._10X轴到位完成);
            lstAction.Add(ActionName._10Z轴到取料位);
            lstAction.Add(ActionName._10Z轴到位吸真空);
            lstAction.Add(ActionName._10Z轴到安全位);
            //Z轴上升过程中判断真空,如果真空检测OK,在Z轴到安全位之前移动X，否则跳到抛料流程
            lstAction.Add(ActionName._10Z轴上升时吸真空检测);
            lstAction.Add(ActionName._10X轴到下相机拍照位完成);
            lstAction.Add(ActionName._10下相机拍照);
            //如果拍照NG则到抛料流程
            lstAction.Add(ActionName._10下相机拍照完成);
            lstAction.Add(ActionName._10C轴旋转);
            lstAction.Add(ActionName._10等待镜筒拍照完成);
            lstAction.Add(ActionName._10镜筒XY轴到组装位);
            lstAction.Add(ActionName._10X轴到组装位);
            //判断X轴到位，C轴旋转到位,镜筒XY轴到位
            lstAction.Add(ActionName._10X轴组装位到位判断);
            lstAction.Add(ActionName._10判断使用二次组装);
            lstAction.Add(ActionName._10Z轴到组装准备位);
            lstAction.Add(ActionName._10Z轴到位);
            lstAction.Add(ActionName._10Z轴到组装位);
            lstAction.Add(ActionName._10Z轴到位);
            lstAction.Add(ActionName._10Z轴到安全位);
            //判断取料拍照是否OK,如果OK,则到取料位,否则到下相机拍照位等待
            lstAction.Add(ActionName._10X轴离开组装位判断);
            lstAction.Add(ActionName._10X轴到位完成);
            lstAction.Add(ActionName._10组装完成);//返回到取料


            //lstAction.Add(ActionName._10取料完成);

            //抛料流程
            lstAction.Add(ActionName._10X轴到抛料位);
            lstAction.Add(ActionName._10X轴到位完成);
            lstAction.Add(ActionName._10吸笔破真空);
            lstAction.Add(ActionName._10X轴到下相机拍照位);
            lstAction.Add(ActionName._10X轴到位完成);
            lstAction.Add(ActionName._10抛料完成);


        }

        public override void Reset()
        {
            switch (iResetStep)
            {
                case 0:
                    if (bResetFinish)
                        break;
                    IStep = 0;
                    Action(ActionName._10Z轴到安全位, ref iResetStep);

                    break;
                case 1:
                    Action(ActionName._10Z轴到位, ref iResetStep);

                    break;
                case 2:
                    Action(ActionName._10X轴到抛料位, ref iResetStep);
                    break;
                case 3:
                    Action(ActionName._10X轴到位完成, ref iResetStep);

                    break;
                case 4:
                    Action(ActionName._10吸笔破真空, ref iResetStep);
                    break;
                case 5:
                    if(sw.WaitSetTime(500))
                        Action(ActionName._10X轴到下相机拍照位, ref iResetStep);
                    break;
                case 6:
                    Action(ActionName._10X轴到位完成, ref iResetStep);

                    break;
                case 7:
                    iCamTimes = 0;
                    iResetStep = 0;
                    bResetFinish = true;
                    break;
                default:

                    break;
            }
        }

        public override void Action(ActionName action, ref int step)
        {
            try
            {
                switch (action) { 
                    case ActionName._10开始取料:
                        if (CameraRModule.bImageResult)
                        {
                            iCamTimes = 0;
                            WriteOutputInfo(strOut + "开始取料");
                            step = step + 1;
                        }

                        break;
                   
                    case ActionName._10X轴到取料位:
                        if (!mc.dic_Axis[AXIS.组装X2轴].INP)
                            break;
                        if (Run.runMode == 0)
                            mc.SetEncoderValue(AXIS.C2轴, 0);
                        bAssembFlag = false;
                        dDestAssemPosX = ModelManager.SuctionRParam.DGetPosX;
                        mc.AbsMove(AXIS.组装X2轴, dDestAssemPosX, ModelManager.VelAssemX.Vel, ModelManager.VelAssemX.ACCAndDec, ModelManager.VelAssemX.ACCAndDec);
                        WriteOutputInfo(strOut + "X轴到取料位");
                        step = step + 1;
                        break;
                    case ActionName._10X轴到抛料位:
                        if (!mc.dic_Axis[AXIS.组装X2轴].INP)
                            break;
                        double dVelX = dVelResetXY;
                        if (!Run.bReset)
                            dVelX = ModelManager.VelAssemX.Vel;
                        dDestAssemPosX = ModelManager.SuctionRParam.DDiscardX;
                        mc.AbsMove(AXIS.组装X2轴, dDestAssemPosX, dVelX, ModelManager.VelAssemX.ACCAndDec, ModelManager.VelAssemX.ACCAndDec);
                        WriteOutputInfo(strOut + "X轴到抛料位");
                        step = step + 1;
                        break;
                    case ActionName._10X轴到组装位:
                        if (!mc.dic_Axis[AXIS.组装X2轴].INP)
                            break;
                        dDestAssemPosX = ModelManager.SuctionRParam.DPutPosX;
                          mc.AbsMove(AXIS.组装X2轴, dDestAssemPosX, ModelManager.VelAssemX.Vel, ModelManager.VelAssemX.ACCAndDec, ModelManager.VelAssemX.ACCAndDec);
                        WriteOutputInfo(strOut + "X轴到组装位");
                        step = step + 1;
                        break;
                    case ActionName._10X轴到下相机拍照位:
                        if (!mc.dic_Axis[AXIS.组装X2轴].INP)
                            break;
                        double dVelX1 = dVelResetXY;
                        if (!Run.bReset)
                            dVelX1 = ModelManager.VelAssemX.Vel;
                        dDestAssemPosX = ModelManager.SuctionRParam.DPosCameraDownX;
                        mc.AbsMove(AXIS.组装X2轴, dDestAssemPosX, dVelX1, ModelManager.VelAssemX.ACCAndDec, ModelManager.VelAssemX.ACCAndDec);
                        WriteOutputInfo(strOut + "X轴到下相机拍照位");
                        step = step + 1;
                        break;
                    case ActionName._10X轴到位完成:
                        if (IsAxisINP(dDestAssemPosX, AXIS.组装X2轴, 0.2))
                        {
                            WriteOutputInfo(strOut + "X轴到位完成");
                            step = step + 1;
                        }
                        break;
                    case ActionName._10X轴到下相机拍照位完成:
                        if (IsAxisINP2(dDestAssemPosX, AXIS.组装X2轴, 0.003))
                        {
                            //复位拍照模块标志，上相机拍照开始
                            CameraRModule.bImageResult = false;
                            WriteOutputInfo(strOut + "X轴到下相机拍照位完成");
                            step = step + 1;
                        }
                        break;
                    case ActionName ._10XY到取料位:
                        if (Run.runMode == RunMode.取料测试)
                        {
                            double dMoveX1 = mc.dic_Axis[AXIS.取料X2轴].dPos + CalibrationR.dDiffX;
                            double dMoveY1 = mc.dic_Axis[AXIS.取料Y2轴].dPos - CalibrationR.dDiffY;
                            dDestPosX = dMoveX1 - (CalibrationR.dRoateCenterColumn - ModelManager.SuctionRParam.imgResultUp.CenterColumn) * CalibrationR.GetUpResulotion();
                            dDestPosY = dMoveY1 + (ModelManager.SuctionRParam.imgResultUp.CenterRow - CalibrationR.dRoateCenterRow) * CalibrationR.GetUpResulotion();
                            mc.AbsMove(AXIS.取料X2轴, dDestPosX, 20);
                            mc.AbsMove(AXIS.取料Y2轴, dDestPosY, 20);
                            WriteOutputInfo(strOut + "XY到取料位");
                            step = step + 1;
                            break;
                        }
                        if (CommonSet.bTestRun)
                        {
                            WriteOutputInfo(strOut + "XY到取料位");
                            step = step + 1;
                            break;
                        }
                        double dMoveX2 = mc.dic_Axis[AXIS.取料X2轴].dPos + CalibrationR.dDiffX;
                        double dMoveY2 = mc.dic_Axis[AXIS.取料Y2轴].dPos - CalibrationR.dDiffY;
                        dDestPosX = dMoveX2 - (CalibrationR.dRoateCenterColumn - ModelManager.SuctionRParam.imgResultUp.CenterColumn) * CalibrationR.GetUpResulotion();
                        dDestPosY = dMoveY2 + (ModelManager.SuctionRParam.imgResultUp.CenterRow - CalibrationR.dRoateCenterRow) * CalibrationR.GetUpResulotion();
                        mc.AbsMove(AXIS.取料X2轴, dDestPosX, 20);
                        mc.AbsMove(AXIS.取料Y2轴, dDestPosY, 20);
                        WriteOutputInfo(strOut + "XY到取料位");
                        step = step + 1;
                        break;
                    case ActionName._10XY到位完成:
                        if (CommonSet.bTestRun || IsAxisINP2(dDestPosX, AXIS.取料X2轴, 0.02) && IsAxisINP2(dDestPosY, AXIS.取料Y2轴, 0.02))
                        {
                            WriteOutputInfo(strOut + "XY到位完成");
                            step = step + 1;
                        }

                        break;
                    case ActionName._10Z轴到安全位:
                        double vel = dVelResetZ;
                        if (!Run.bReset)
                            vel = ModelManager.VelZ.Vel;
                        dDestPosZ = ModelManager.SuctionRParam.pSafeXYZ.Z;
                        mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, vel, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                        WriteOutputInfo(strOut + "Z轴到安全位");
                        step = step + 1;
                        break;
                    case ActionName._10Z轴到取料位:
                        dDestPosZ = ModelManager.SuctionRParam.DGetPosZ;
                        mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, ModelManager.VelZ.Vel, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                        WriteOutputInfo(strOut + "Z轴到取料位");
                        step = step + 1;
                        break;
                    case ActionName._10Z轴到取标定块位:
                        if (Run.runMode == RunMode.取料标定)
                            dDestPosZ = CalibrationR.DGetCalibPosZ;
                        else if (Run.runMode == RunMode.组装标定)
                            dDestPosZ = CalibrationBR.DGetCalibPosZ;
                        
                        mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, ModelManager.VelZ.Vel, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                        WriteOutputInfo(strOut + "Z轴到取标定块位");
                        step = step + 1;
                        break;
                    case ActionName._10Z轴到标定块拍照高度:
                        if (Run.runMode == RunMode.取料标定)
                            dDestPosZ = CalibrationR.DCalibDownPosZ;
                        else if (Run.runMode == RunMode.组装标定)
                            dDestPosZ = CalibrationBR.DCalibDownPosZ;
                        //dDestPosZ = CalibrationR.DCalibDownPosZ;
                        mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, ModelManager.VelZ.Vel, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                        WriteOutputInfo(strOut + "Z轴到标定块拍照高度");
                        step = step + 1;
                        break;
                    case ActionName._10Z轴到组装位:
                        if (Run.runMode == RunMode.组装验证)
                        {
                            dDestPosZ = ModelManager.SuctionRParam.DPutPosZ;
                          
                            mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, ModelManager.VelZ.Vel, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                            WriteOutputInfo(strOut + "Z轴到组装位");
                            step = step + 1;
                            break;
                        }
                        if (Run.runMode == RunMode.运行)
                        {
                            Run.lAssemTime = Run.swCircleTime.ElapsedMilliseconds;
                            Run.swCircleTime.Restart();
                            int iCurrentTrayIndex = ModelManager.BarrelParam.iCurrentTrayIndex - 1;
                            int pos = Barrel.lstTray[iCurrentTrayIndex].CurrentPos;
                            Barrel.lstTray[iCurrentTrayIndex].setNumColor(pos, CommonSet.ColorOK);
                            Barrel.lstTray[iCurrentTrayIndex].updateColor();
                            Barrel.lstTray[iCurrentTrayIndex].CurrentPos++;
                        }
                        dDestPosZ = ModelManager.SuctionRParam.DPutPosZ;
                        double v = ModelManager.VelZ.Vel;
                        if (ModelManager.SuctionRParam.BTwo)
                            v = ModelManager.SuctionRParam.VelZ2.Vel;
                         mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, v, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                        WriteOutputInfo(strOut + "Z轴到组装位");
                        step = step + 1;
                        break;
                    case ActionName._10Z轴到组装准备位:
                       dDestPosZ = ModelManager.SuctionRParam.DPutPosZ2;
                       mc.AbsMove(AXIS.组装Z2轴, dDestPosZ, ModelManager.VelZ.Vel, ModelManager.VelZ.ACCAndDec, ModelManager.VelZ.ACCAndDec);
                        WriteOutputInfo(strOut + "Z轴到组装准备位");
                        step = step + 1;
                        break;
                    case ActionName._10Z轴到位:
                        if (IsAxisINP(dDestPosZ, AXIS.组装Z2轴, 0.02))
                        {
                            WriteOutputInfo(strOut + "Z轴到位完成");
                            step = step + 1;
                        }
                        break;
                    case ActionName._10C轴旋转:
                        if (CommonSet.bTestRun)
                        {
                            dDestC = mc.dic_Axis[AXIS.C2轴].dPos + 180;
                        }
                        else
                        {
                            if(Run.runMode == RunMode.组装验证)
                            {
                                dDestC = mc.dic_Axis[AXIS.C2轴].dPos + dRotateAngle;

                            }else
                            {
                                dDestC = mc.dic_Axis[AXIS.C2轴].dPos + ModelManager.SuctionRParam.imgResultDown.dAngle;
                            }
                           
                        }
                        
                        mc.AbsMove(AXIS.C2轴, dDestC, ModelManager.VelC.Vel, ModelManager.VelC.ACCAndDec, ModelManager.VelC.ACCAndDec);
                        WriteOutputInfo(strOut + "C2轴旋转");
                        step = step + 1;
                        break;
                    case ActionName._10等待镜筒拍照完成:
                        if (!BarrelModule.bBarrelResult)
                        {
                            break;
                        }
                        if (AssemLModule.bAssembFlag)
                        {
                            break;
                        }
                        bAssembFlag = true;
                        step = step + 1;
                        break;
                    case ActionName._10镜筒XY轴到组装位:
                        double dMoveX3 = mc.dic_Axis[AXIS.镜筒X轴].dPos - CalibrationBR.dDiffX;
                        double dMoveY3 = mc.dic_Axis[AXIS.镜筒Y轴].dPos - CalibrationBR.dDiffY;
                        if (Run.runMode == RunMode.运行)
                        {
                            dAssemCenterRow = ModelManager.BarrelParam.imgResultUp.CenterRow;
                            dAssemCenterCol = ModelManager.BarrelParam.imgResultUp.CenterColumn;
                        }
                        HTuple hmat, outR, outC;
                        HOperatorSet.HomMat2dIdentity(out hmat);

                        HOperatorSet.HomMat2dRotate(hmat, new HTuple(-(double)dRotateAngle).TupleRad().D, CalibrationR.dRoateCenterRow, CalibrationR.dRoateCenterColumn, out hmat);

                        HOperatorSet.AffineTransPoint2d(hmat, ModelManager.SuctionRParam.imgResultDown.CenterRow, ModelManager.SuctionRParam.imgResultDown.CenterColumn, out outR, out outC);
                        dMoveX3 = dMoveX3 + (outC - 1224) * CalibrationR.GetDownResulotion();
                        dMoveY3 = dMoveY3 - (outR - 1024) * CalibrationR.GetDownResulotion();
                        dDestPosX = dMoveX3 - (dAssemCenterCol - 1224) * CalibrationBL.GetUpResulotion();
                        dDestPosY = dMoveY3 + (dAssemCenterRow - 1024) * CalibrationBL.GetUpResulotion();
                        mc.AbsMove(AXIS.镜筒X轴, dDestPosX, ModelManager.VelBarrelX.Vel);
                        mc.AbsMove(AXIS.镜筒Y轴, dDestPosY, ModelManager.VelBarrelY.Vel);
                        WriteOutputInfo(strOut + "镜筒XY轴到组装位");
                        step = step + 1;
                        break;
                    case ActionName._10镜筒XY轴到组装补偿位:
                        double dMoveX = mc.dic_Axis[AXIS.镜筒X轴].dPos + dDiffX;
                        double dMoveY = mc.dic_Axis[AXIS.镜筒Y轴].dPos + dDiffY;
                        dMoveX = dMoveX + (1224- ModelManager.SuctionRParam.imgResultDown.CenterColumn) * CalibrationR.GetDownResulotion();
                        dMoveY = dMoveY + ( ModelManager.SuctionRParam.imgResultDown.CenterRow-1024) * CalibrationR.GetDownResulotion();
                        mc.AbsMove(AXIS.镜筒X轴, dMoveX, 20);
                        mc.AbsMove(AXIS.镜筒Y轴, dMoveY, 20);
                        WriteOutputInfo(strOut + "镜筒XY轴到组装补偿位");
                        step = step + 1;

                        break;
                    case ActionName._10X轴组装位到位判断:
                        if (!IsAxisINP2(dDestC, AXIS.C2轴, 10))
                        {
                            break;
                        }
                        if (!IsAxisINP2(dDestAssemPosX, AXIS.组装X2轴, 0.2))
                        {
                            break;
                        }
                      
                        WriteOutputInfo(strOut + "X轴到组装位");
                        step = step + 1;
                        break;
                    case ActionName._10X轴离开组装位判断:
                        //当Z轴上升2/3的距离时判断
                        double dDist = Math.Abs((ModelManager.SuctionRParam.DPutPosZ - ModelManager.SuctionRParam.pSafeXYZ.Z) / 3);
                        if (IsAxisINP2(dDestPosZ, AXIS.组装Z2轴, dDist))
                        {
                            //如果检测有料，则移动组装X轴
                            if (CameraRModule.bImageResult)
                            {
                                BarrelModule.bBarrelResult = false;
                                WriteOutputInfo(strOut + "X轴离开组装位判断去取料位");
                                step = lstAction.IndexOf(ActionName._10X轴到取料位);
                                
                            }
                            else
                            {
                                bAssembFlag = false;
                                BarrelModule.bBarrelResult = false;
                                dDestAssemPosX = ModelManager.SuctionRParam.DPosCameraDownX;
                                mc.AbsMove(AXIS.组装X2轴, dDestAssemPosX, ModelManager.VelAssemX.Vel, ModelManager.VelAssemX.ACCAndDec, ModelManager.VelAssemX.ACCAndDec);
                                WriteOutputInfo(strOut + "X轴离开组装位判断去下相机拍照位");
                                step = step + 1;
                            }

                        }
                        break;
                    case ActionName._10判断使用二次组装:
                        //if (CommonSet.bTestRun)
                        //{
                        //    step = step + 3;
                        //    break;
                        //}
                        if (ModelManager.SuctionRParam.BTwo)
                        {
                            step = step + 1;
                        }
                        else
                        {
                            step = step + 3;
                        }

                        break;
                    case ActionName._10吸笔破真空:
                        
                         mc.setDO(DO.右吸嘴吸气, false);
                         mc.setDO(DO.右吸嘴吹气, true);
                         long lTime1 =(long)( ModelManager.SuctionRParam.DPutTime * 1000);
                        if (Run.runMode != RunMode.运行)
                            lTime1 = 1000;
                        if (sw.WaitSetTime(lTime1))
                         {
                             mc.setDO(DO.右吸嘴吹气, false);
                             WriteOutputInfo(strOut + "右吸笔破真空");
                             step = step + 1;
                         }

                        break;
                    case ActionName._10Z轴到位吸真空:
                        //当Z轴离取料位差0.2mm时，打开吸真空
                        if (IsAxisINP2(dDestPosZ, AXIS.组装Z2轴, 0.2))
                        {
                            mc.setDO(DO.右吸嘴吸气, true);
                            mc.setDO(DO.右吸嘴吹气, false);
                            long lGetTime = (long)(ModelManager.SuctionRParam.DGetTime * 1000);
                            if (Run.runMode != RunMode.运行)
                                lGetTime = 1000;
                            if (sw.WaitSetTime(lGetTime)&&IsAxisINP(AXIS.组装Z2轴))
                            {
                                WriteOutputInfo(strOut + "Z轴到位吸真空");
                                step = step + 1;
                            }
                        }

                        break;
                    case ActionName._10吸笔真空检测:
                        if (mc.dic_DI[DI.右吸嘴负压感应1] || (Run.runMode == RunMode.取料测试))
                        {
                            WriteOutputInfo(strOut + "右吸笔真空检测");
                            step = step + 1;
                        }


                        break;
                    case ActionName._10Z轴上升时吸真空检测:
                        //当Z轴上升2/3的距离时，真空检测
                        double dDist2 = Math.Abs((ModelManager.SuctionRParam.DGetPosZ - ModelManager.SuctionRParam.pSafeXYZ.Z)/3);
                        if (IsAxisINP2(dDestPosZ, AXIS.组装Z2轴, dDist2))
                        {
                            //如果检测有料，则移动组装X轴
                            if (mc.dic_DI[DI.右吸嘴负压感应1] || CommonSet.bTestRun)
                            {
                                dDestAssemPosX = ModelManager.SuctionRParam.DPosCameraDownX;
                                mc.AbsMove(AXIS.组装X2轴, dDestAssemPosX, ModelManager.VelAssemX.Vel,ModelManager.VelAssemX.ACCAndDec, ModelManager.VelAssemX.ACCAndDec);
                                WriteOutputInfo(strOut + "Z轴上升时吸真空检测有料,组装X2轴到下相机拍照位");
                                step = step + 1;
                            }
                            else
                            {

                                WriteOutputInfo(strOut + "Z轴上升时吸真空检测无料,进入抛料流程，重新拍照");
                                step = lstAction.IndexOf(ActionName._10X轴到抛料位);
                            }
                        
                        }
                        break;
                    case ActionName._10下相机拍照:
                        iCamTimes++;
                        if (Run.runMode == RunMode.取料标定)
                        {
                            ModelManager.SuctionRParam.InitImageDown(CalibrationR.strPicDownCalibName);
                            ModelManager.CamDownR.SetExposure(CalibrationR.ICalibDownExposure);
                        }
                        else if (Run.runMode == RunMode.组装标定)
                        {
                            ModelManager.SuctionRParam.InitImageDown(CalibrationBR.strPicDownCalibName);
                            ModelManager.CamDownR.SetExposure(CalibrationBR.ICalibDownExposure);
                        }
                        else
                        {
                            ModelManager.SuctionRParam.InitImageDown(ModelManager.SuctionRParam.strPicDownProduct);
                            ModelManager.CamDownR.SetExposure(ModelManager.SuctionRParam.IProductDownExposure);
                        }
                        ModelManager.CamDownR.SoftTrigger();
                        WriteOutputInfo(strOut + "相机拍照");
                        step = step + 1;
                        break;
                    case ActionName._10下相机拍照完成:
                        if (CommonSet.bTestRun)
                        {
                            WriteOutputInfo(strOut + "下相机拍照完成");
                            step = step + 1;
                            break;
                        }
                        if (ModelManager.SuctionRParam.imgResultDown.bStatus)
                        {
                            if (Run.runMode != RunMode.运行)
                            {
                                if (!sw.WaitSetTime(500))
                                    break;
                                if (ModelManager.SuctionRParam.imgResultDown.bImageResult)
                                {
                                    WriteOutputInfo(strOut + "相机拍照完成");

                                    step = step + 1;
                                }
                                else
                                {
                                   // Run.runMode = RunMode.手动;
                                    step = step - 1;
                                }

                                break;
                            }
                            if (ModelManager.SuctionRParam.imgResultDown.bImageResult)
                            {
                                WriteOutputInfo(strOut + "下相机拍照完成");
                                step = step + 1;
                            }

                        }else
                        {
                            if (sw.WaitSetTime(500))
                            {
                                if (iCamTimes < 2)
                                {
                                    step = step - 1;
                                    WriteOutputInfo(strOut + "第二次拍照");
                                }
                                else
                                {
                                    if (Run.runMode != RunMode.运行)
                                    {
                                        Run.runMode = RunMode.手动;
                                    }
                                    else
                                    {
                                        Alarminfo.AddAlarm(Alarm.右下相机处理图像超时);
                                        CommonSet.WriteInfo(strOut + "右下相机处理图像超时");
                                        Run.runMode = RunMode.暂停;
                                    }

                                }
                            }

                        }


                       // step = step + 1;

                        break;
                    case ActionName._10组装完成:
                        WriteOutputInfo(strOut + "组装完成");
                        step = 0;
                        break;
                    case ActionName._10取料完成:
                        WriteOutputInfo(strOut + "取料完成");
                        step = 0;
                        break;

                    case ActionName._10抛料完成:
                        WriteOutputInfo(strOut + "抛料完成");
                        step = 0;
                        break;

                }


            }
            catch (Exception ex)
            {
                
                WriteOutputInfo(strOut + "右取料模块动作执行异常："+ex.ToString());
            }
        }

        public override void Action2()
        {
           
        }
    }
}
