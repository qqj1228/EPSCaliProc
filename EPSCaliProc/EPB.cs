using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace EPSCaliProc {
    public class EPB {
        public string StrVIN { get; set; }
        bool IsClientRun { get; set; }
        public bool IsClearCali { get; set; }
        public bool IsRepeatCali { get; set; }
        public Model DataBase { get; set; }
        public string StrSoftwareVer { get; set; }
        public string StrHardwareVer { get; set; }

        private int retrialTimes;
        public int RetrialTimes {
            get { return retrialTimes; }
            set {
                if (value < 0) {
                    value = 0;
                }
                retrialTimes = value;
            }
        }

        VciClient Vci;
        protected VciEPBApp EPBApp;
        readonly LogBox Log;

        public EPB(string StrDirTrace, string StrDirXML, string StrVIN, bool IsClearCali, bool IsRepeatCali, int RetrialTimes, LogBox Log) {
            Vci = new VciClient(StrDirTrace, StrDirXML, Log);
            EPBApp = new VciEPBApp(Log);

            IsClientRun = false;
            this.StrVIN = StrVIN;
            this.IsClearCali = IsClearCali;
            this.IsRepeatCali = IsRepeatCali;
            this.RetrialTimes = RetrialTimes;
            this.Log = Log;
            DataBase = new Model(Log);
            //DataBase.ShowDB("EPSCaliProc");
            //byte[] temp = new byte[] { 0x11, 0x22, 0x33, 0xAA, 0x44, 0x55, 0x66, 0xBB, 0x77, 0x88, 0x99, 0xCC };
            //DataBase.WriteResult("EPSCaliProc", "testvincode999345", 1, Vci.DTCToString(3, temp));
        }

        ~EPB() {
            if (IsClientRun) {
                Close();
            }
        }

        public int Close() {
            int iRet = 0;
            iRet = Vci.Close();
            return iRet;
        }

        public void Run() {
            int iRet = 0;
            iRet = Vci.StartService();
            if (iRet == 0) {
                Log.ShowLog("=> StartService success");
                iRet = Vci.StartDevice();
                if (iRet == 0) {
                    IsClientRun = true;
                    Log.ShowLog("==> StartDevice success");
                } else {
                    Thread.Sleep(200);
                    iRet = Vci.StartDevice();
                    if (iRet == 0) {
                        Log.ShowLog("==> StartDevice success");
                    } else {
                        Log.ShowLog("==> StartDevice failed", LogBox.Level.error);
#if !DEBUG
                        Close();
                        return;
#endif
                    }
                }
            } else {
                Log.ShowLog("=> StartService failed",LogBox.Level.error);
                return;
            }
            EPBCaliRun();
            Close();
            IsClientRun = false;
        }

        int DoCail(int rid, byte[] Data, int iDataLen, ref byte[] RecvData, ref int RetLen) {
            int iRet = 0;

            iRet = EPBApp.DiagSessionControl(3);
            iRet = EPBApp.RoutineControl(0x01, rid, Data, iDataLen, ref RecvData, ref RetLen);

            int times = 0;
            while (times <= RetrialTimes) {
                // 等待300ms
                Thread.Sleep(300);

                // 请求例程结果
                Data[0] = 0;
                iDataLen = 0;
                iRet = EPBApp.RoutineControl(0x03, rid, Data, iDataLen, ref RecvData, ref RetLen);

                // 判断例程结果
                if (RetLen >= 1) {
                    if (RecvData[0] == 0x10) {
                        // 例程正确结束, 退出标定
                        iRet = 0;
                        Log.ShowLog(string.Format("===> Finished Routine:0x{0:X4} with success", rid));
                        break;
                    } else if (RecvData[0] == 0xC0) {
                        // 例程仍在运行, 等待其结束
                        Log.ShowLog(string.Format("===> 0x{0:X4}:RoutineRunning", rid));
                        continue;
                    } else if (RecvData[0] == 0x40) {
                        // 例程错误结束
                        iRet = -1;
                        Log.ShowLog(string.Format("===> 0x{0:X4}:RoutineFinishedWithFailure", rid), LogBox.Level.error);
                        if (IsRepeatCali) {
                            // 若需要重试的话, 等待三秒后重试
                            ++times;
                            Log.ShowLog(string.Format("===> Retry Routine:0x{0:X4} {1} time(s)", rid, times), LogBox.Level.error);
                            Thread.Sleep(2400);
                            continue;
                        } else {
                            // 不需要重试的话, 退出标定
                            Log.ShowLog(string.Format("===> Finished Routine:0x{0:X4} with failure", rid), LogBox.Level.error);
                            break;
                        }
                    } else {
                        // 未知错误, 退出标定
                        iRet = -1;
                        Log.ShowLog(string.Format("===> Finished Routine:0x{0:X4} with failure", rid), LogBox.Level.error);
                        break;
                    }
                } else {
                    // 例程出错未运行, 退出标定
                    iRet = -1;
                    Log.ShowLog(string.Format("===> 0x{0:X4}:RoutineNotRunning", rid), LogBox.Level.error);
                    break;
                }
            }
            return iRet;
        }

        void EPBCaliRun() {
            int iRet = 0;
            byte[] RecvData = new byte[500];
            int RetLen = 0;
            string[] strResult = new string[5] { "X", "X", "X", "X", "X" };

            iRet = EPBApp.DiagSessionControl(1);
            iRet = EPBApp.TestPresent();
            iRet = EPBApp.DiagSessionControl(3);

            // 获取EPB软件版本号
            int did = 0xF195;
            string SoftwareVer = "";
            iRet = EPBApp.ReadDataByID(did, ref RecvData, ref RetLen);
            if (iRet == 0) {
                for (int i = 0; i < RetLen; i++) {
                    SoftwareVer += (char)RecvData[i];
                }
                Log.ShowLog(string.Format("===> Software Version(from EPB): {0}", SoftwareVer));
                if (SoftwareVer == StrSoftwareVer) {
                    strResult[0] = "O";
                } else {
                    Log.ShowLog(string.Format("===> Software Version isn't match", LogBox.Level.error));
                }
            } else {
                Log.ShowLog(string.Format("===> Get Software Version failed", LogBox.Level.error));
            }

            // 获取EPB硬件版本号
            did = 0xF192;
            string HardwareVer = "";
            iRet = EPBApp.ReadDataByID(did, ref RecvData, ref RetLen);
            if (iRet == 0) {
                for (int i = 0; i < RetLen; i++) {
                    HardwareVer += (char)RecvData[i];
                }
                Log.ShowLog(string.Format("===> Hardware Version: {0}", HardwareVer));
                if (HardwareVer == StrHardwareVer) {
                    strResult[1] = "O";
                } else {
                    Log.ShowLog(string.Format("===> Hardware Version isn't match", LogBox.Level.error));
                }
            } else {
                Log.ShowLog(string.Format("===> Get Hardware Version failed", LogBox.Level.error));
            }

            // 设置EPB系统安全访问模式
            iRet = EPBApp.UnlockECU();
            iRet = EPBApp.TestPresent();

            int rid = 0;
            byte[] Data = new byte[20];
            int iDataLen = 0;

            // G-sensor倾角标定
            rid = 0x2008;
            Data[0] = 1;
            iDataLen = 1;
            iRet = DoCail(rid, Data, iDataLen, ref RecvData, ref RetLen);
            if (0 == iRet) {
                strResult[2] = "O";
            }

            // Assembly check 测试
            rid = 0x200A;
            Data[0] = 0;
            iDataLen = 0;
            iRet = DoCail(rid, Data, iDataLen, ref RecvData, ref RetLen);
            if (0 == iRet) {
                strResult[3] = "O";
            }

            // 清除系统故障
            iRet = EPBApp.ClearDTC();

            // 读取DTC故障码
            int NumOfDTC = 0;
            int DTC = 0;
            byte Status = 0;
            int j = 0;
            iRet = EPBApp.ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);

            if (NumOfDTC > 0) {
                string strData = "===> Number of DTC: {0}" + NumOfDTC.ToString();
                for (int i = 0; i < NumOfDTC; i++) {
                    DTC = ((RecvData[i * 4] << 16) + (RecvData[(i * 4) + 1] << 8) + RecvData[(i * 4) + 2]);
                    Status = RecvData[(i * 4) + 3];
                    j = i + 1;
                    strData += ", DTC[" + j.ToString() + "]: 0x" + DTC.ToString("X6") + " - 0x" + Status.ToString("X2");
                }
                Log.ShowLog(strData);
            }

            // 设置出厂模式
            iRet = EPBApp.UnlockECU();
            did = 0x0110;
            Data[0] = 0x00;
            iDataLen = 1;
            iRet = EPBApp.WriteDataByID(did, Data, iDataLen);
            if (iRet == 0) {
                iRet = EPBApp.ReadDataByID(did, ref RecvData, ref RetLen);
                if (iRet == 0 && RetLen == 1 && RecvData[0] == 0x00) {
                    Log.ShowLog("===> 设置出厂模式成功");
                    strResult[4] = "O";
                } else {
                    Log.ShowLog("===> 读取出厂模式失败", LogBox.Level.error);
                }
            } else {
                Log.ShowLog("===> 设置出厂模式失败", LogBox.Level.error);
            }

            // 写结果到数据库里
            DataBase.WriteEPBResult("EPBCaliProc", StrVIN, strResult, Vci.DTCToString(NumOfDTC, RecvData));
        }

    }
}
