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
        bool IsSuccess { get; set; }
        public Model DataBase { get; set; }
        VciClient Vci;
        readonly LogBox Log;
        readonly Config Cfg;

        public EPB(string StrDirTrace, string StrDirXML, Config Cfg, LogBox Log) {
            Vci = new VciClient(StrDirTrace, StrDirXML, Log);
            IsClientRun = false;
            this.Log = Log;
            this.Cfg = Cfg;
            DataBase = new Model(Cfg, Log);
        }

        ~EPB() {
            Close();
        }

        public void Close() {
            int iRet = 0;
            if (IsClientRun) {
                iRet = Vci.Close();
                IsClientRun = false;
            }
        }

        /// <summary>
        /// 运行EPB标定
        /// iType = 1：7S EPB标定，其余值为自动判断
        /// strVIN, VIN号
        /// </summary>
        /// <param name="iType"></param>
        /// <param name="strVIN"></param>
        /// <returns></returns>
        public bool Run(int iType, string strVIN) {
            Log.ShowLog("====== 开始 EPB 标定程序 ======");

            this.StrVIN = strVIN;
            this.IsSuccess = false;
            // 获取与VIN号对应的车型代码
            string strVehicleType = DataBase.GetVehicleType(strVIN);
            if (strVehicleType == "") {
                Log.ShowLog("=> 未获取到车型代码，退出标定", LogBox.Level.error);
                return IsSuccess;
            }

            int iRet = 0;
            iRet = Vci.StartService();
            if (iRet == 0) {
                Log.ShowLog("=> StartService success");
                if (!DoubleStartDevice()) {
#if !DEBUG
                    Close();
                    return IsSuccess;
#endif
                }
            } else {
                Thread.Sleep(200);
                iRet = Vci.StartService();
                if (iRet == 0) {
                    Log.ShowLog("=> StartService success");
                    if (!DoubleStartDevice()) {
#if !DEBUG
                        Close();
                        return IsSuccess;
#endif
                    }
                } else {
                    Log.ShowLog("=> StartService failed", LogBox.Level.error);
#if !DEBUG
                    Close();
                    return IsSuccess;
#endif
                }
            }

            switch (iType) {
                case 1:
                    iRet = EPBCaliRun();
                    break;
                default:
                    if (strVehicleType.StartsWith("X41")) {
                        Log.ShowLog("=> 车型代码为：\"" + strVehicleType + "\"，调用 7S EPB 标定程序");
                        iRet = EPBCaliRun();
                    } else if (strVehicleType.StartsWith("X11")) {
                        Log.ShowLog("=> 车型代码为：\"" + strVehicleType + "\"，不进行 EPB 标定");
                        return true;
                    } else {
                        Log.ShowLog("=> 未知车型代码：\"" + strVehicleType + "\"，默认调用 7S EPB 标定程序", LogBox.Level.error);
                        iRet = EPBCaliRun();
                    }
                    break;
            }

            Close();
            return IsSuccess;
        }

        bool DoubleStartDevice() {
            int iRet = 0;
            iRet = Vci.StartDevice();
            IsClientRun = true;
            if (iRet == 0) {
                Log.ShowLog("=> StartDevice success");
            } else {
                Thread.Sleep(200);
                iRet = Vci.StartDevice();
                if (iRet == 0) {
                    Log.ShowLog("=> StartDevice success");
                } else {
                    Log.ShowLog("=> StartDevice failed", LogBox.Level.error);
#if !DEBUG
                    Close();
                    return false;
#endif
                }
            }
            return true;
        }

        int DiagSessionControl(byte mode) {
            int iRet = vciApp.AppServer.EPB_DiagSessionControl(mode);
            Log.ShowLog(string.Format("=> EPB_DiagSessionControl({0}), iRet = {1}", mode, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_DiagSessionControl({0}) success", mode));
            } else {
                Log.ShowLog(string.Format("=> EPB_DiagSessionControl({0}) failed", mode), LogBox.Level.error);
            }
            return iRet;
        }

        int TestPresent() {
            int iRet = vciApp.AppServer.EPB_TestPresent();
            Log.ShowLog(string.Format("=> EPB_TestPresent(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_TestPresent() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_TestPresent() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ReadDataByID(int did, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EPB_ReadDataByIdentifier(did, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("=> EPB_ReadDataByIdentifier(), did = 0x{0:X4}, iRet = {1}", did, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_ReadDataByIdentifier() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_ReadDataByIdentifier() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int WriteDataByID(int did, byte[] data, int dataLen) {
            int iRet = vciApp.AppServer.EPB_WriteDataByIdentifier(did, data, dataLen);
            Log.ShowLog(string.Format("=> EPB_WriteDataByIdentifier(), did = 0x{0:X4}, iRet = {1}", did, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_WriteDataByIdentifier() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_WriteDataByIdentifier() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int UnlockECU() {
            int iRet = vciApp.AppServer.EPB_UnlockECU();
            Log.ShowLog(string.Format("=> EPB_UnlockECU(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_UnlockECU() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_UnlockECU() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int RoutineControl(byte option, int rid, byte[] Data, int dataLen, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EPB_RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("=> EPB_RoutineControl(), option = 0x{0:X2}, rid = 0x{1:X4}, iRet = {2}", option, rid, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_RoutineControl() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_RoutineControl() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ClearDTC() {
            int iRet = vciApp.AppServer.EPB_ClearDTC();
            Log.ShowLog(string.Format("=> EPB_ClearDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_ClearDTC() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_ClearDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ReadAllDTC(ref int NumOfDTC, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EPB_ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("=> EPB_ReadAllDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPB_ReadAllDTC() success"));
            } else {
                Log.ShowLog(string.Format("=> EPB_ReadAllDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int DoCail(int rid, byte[] Data, int iDataLen, ref byte[] RecvData, ref int RetLen) {
            byte option = 0;
            int iRet = 0;

            option = 0x01;
            iRet = RoutineControl(option, rid, Data, iDataLen, ref RecvData, ref RetLen);

            int counter = 0;
            while (counter < 3) {
                // 等待一段时间
                if (rid == 0x2008) {
                    Thread.Sleep(300);
                } else if (rid == 0x200A) {
                    Thread.Sleep(3000);
                }

                // 请求例程结果
                Data[0] = 0;
                iDataLen = 0;
                option = 0x03;
                iRet = RoutineControl(option, rid, Data, iDataLen, ref RecvData, ref RetLen);

                // 判断例程结果
                if (RetLen >= 1) {
                    if (RecvData[0] == 0x10) {
                        // 例程正确结束
                        iRet = 0;
                        Log.ShowLog(string.Format("=> Finished Routine:0x{0:X4} with success", rid));
                        break;
                    } else if (RecvData[0] == 0xC0) {
                        // 例程仍在运行, 等待其结束
                        Log.ShowLog(string.Format("=> 0x{0:X4}:RoutineRunning", rid));
                        ++counter;
                    } else if (RecvData[0] == 0x40) {
                        // 例程错误结束
                        iRet = -1;
                        Log.ShowLog(string.Format("=> 0x{0:X4}:RoutineFinishedWithFailure", rid), LogBox.Level.error);
                        break;
                    } else {
                        // 未知错误, 退出例程
                        iRet = -1;
                        Log.ShowLog(string.Format("=> Finished Routine:0x{0:X4} with failure", rid), LogBox.Level.error);
                        break;
                    }
                } else {
                    // 例程出错未运行, 退出例程
                    iRet = -1;
                    Log.ShowLog(string.Format("=> 0x{0:X4}:RoutineNotRunning", rid), LogBox.Level.error);
                    break;
                }
            }
            return iRet;
        }

        int EPBCaliRun() {
            int iRet = 0;
            byte[] RecvData = new byte[500];
            int RetLen = 0;

            // 进入默认模式
            iRet = DiagSessionControl(1);
            // 进入扩展模式
            iRet = DiagSessionControl(3);
#if !DEBUG
            if (iRet != 0) {
                Log.ShowLog(string.Format("==> 7S EPB can't be accessed!"), LogBox.Level.error);
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }
#endif

            // 设置EPB系统安全访问模式
            iRet = UnlockECU();
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }

            int rid = 0;
            byte[] Data = new byte[3];
            int iDataLen = 0;

            // G-sensor倾角标定
            rid = 0x2008;
            Data[0] = 1;
            iDataLen = 1;
            iRet = DoCail(rid, Data, iDataLen, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }

            // Assembly check 测试
            rid = 0x200A;
            Data[0] = 0;
            iDataLen = 0;
            iRet = DoCail(rid, Data, iDataLen, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 清除系统故障
            iRet = ClearDTC();
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 读取DTC故障码
            int NumOfDTC = 0;
            int DTC = 0;
            byte Status = 0;
            int j = 0;
            iRet = ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }
            if (NumOfDTC > 0) {
                string strData = "=> Number of DTC: {0}" + NumOfDTC.ToString();
                for (int i = 0; i < NumOfDTC; i++) {
                    DTC = ((RecvData[i * 4] << 16) + (RecvData[(i * 4) + 1] << 8) + RecvData[(i * 4) + 2]);
                    Status = RecvData[(i * 4) + 3];
                    j = i + 1;
                    strData += ", DTC[" + j.ToString() + "]: 0x" + DTC.ToString("X6") + " - 0x" + Status.ToString("X2");
                }
                Log.ShowLog(strData, LogBox.Level.error);
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", "-", Vci.DTCToString(NumOfDTC, RecvData));
            }

            // 设置出厂模式
            int did = 0x0110;
            Data[0] = 0x00;
            iRet = WriteDataByID(did, Data, 1);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }
            iRet = ReadDataByID(did, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
                return iRet;
            }
            if (RetLen >= 1) {
                if (RecvData[0] == 0x00) {
                    Log.ShowLog("=> 7S EPS Calibration Procedure Success");
                    DataBase.WriteCaliResult(StrVIN, "7S_EPB", "O", "-", "-");
                    this.IsSuccess = true;
                } else {
                    Log.ShowLog("=> Fail to set Non-Manufactory mode, Receive data: \"" + RecvData[0].ToString("X2") + "\"");
                    DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", "Res" + RecvData[0].ToString("X2"), "-");
                }
            } else {
                Log.ShowLog("=> Fail to set Non-Manufactory mode, Reture data length < 1, RetLen: \"" + RetLen + "\"");
                iRet = -1;
                DataBase.WriteCaliResult(StrVIN, "7S_EPB", "X", iRet.ToString(), "-");
            }
            return iRet;
        }

    }
}
