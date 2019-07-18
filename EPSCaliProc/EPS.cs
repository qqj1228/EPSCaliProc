using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace EPSCaliProc {
    public class EPS {
        public string StrVIN { get; set; }
        bool IsClientRun { get; set; }
        bool IsSuccess { get; set; }
        public Model DataBase { get; set; }
        VciClient Vci;
        readonly LogBox Log;
        readonly Config Cfg;

        public EPS(string StrDirTrace, string StrDirXML, Config Cfg, LogBox Log) {
            Vci = new VciClient(StrDirTrace, StrDirXML, Log);
            IsClientRun = false;
            IsSuccess = false;
            this.Log = Log;
            this.Cfg = Cfg;
            DataBase = new Model(Cfg, Log);
        }

        ~EPS() {
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
        /// 运行EPS标定
        /// iType = 1：7S EPS标定，2：7L EPS标定，其余值为自动判断
        /// strVIN, VIN号
        /// </summary>
        /// <param name="iType"></param>
        /// <param name="strVIN"></param>
        /// <returns></returns>
        public bool Run(int iType, string strVIN) {
            Log.ShowLog("====== 开始 EPS 标定程序 ======");

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
                    iRet = EPS7SCaliRun();
                    break;
                case 2:
                    iRet = EPS7LCaliRun();
                    break;
                default:
                    if (strVehicleType.StartsWith("X41")) {
                        Log.ShowLog("=> 车型代码为：\"" + strVehicleType + "\"，调用 7S EPS 标定程序");
                        iRet = EPS7SCaliRun();
                    } else if (strVehicleType.StartsWith("X11")) {
                        Log.ShowLog("=> 车型代码为：\"" + strVehicleType + "\"，调用 7L EPS 标定程序");
                        iRet = EPS7LCaliRun();
                    } else {
                        Log.ShowLog("=> 未知车型代码：\"" + strVehicleType + "\"，默认调用 7S EPS 标定程序", LogBox.Level.error);
                        iRet = EPS7SCaliRun();
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
            int iRet = vciApp.AppServer.EPS_DiagSessionControl(mode);
            Log.ShowLog(string.Format("=> EPS_DiagSessionControl({0}), iRet = {1}", mode, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_DiagSessionControl({0}) success", mode));
            } else {
                Log.ShowLog(string.Format("=> EPS_DiagSessionControl({0}) failed", mode), LogBox.Level.error);
            }
            return iRet;
        }

        int TestPresent() {
            int iRet = vciApp.AppServer.EPS_TestPresent();
            Log.ShowLog(string.Format("=> EPS_TestPresent(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_TestPresent() success"));
            } else {
                Log.ShowLog(string.Format("=> EPS_TestPresent() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ReadDataByID(int did, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EPS_ReadDataByIdentifier(did, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("=> EPS_ReadDataByIdentifier(), did = 0x{0:X4}, iRet = {1}", did, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_ReadDataByIdentifier() success"));
            } else {
                Log.ShowLog(string.Format("=> EPS_ReadDataByIdentifier() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int UnlockECU() {
            int iRet = vciApp.AppServer.EPS_UnlockECU();
            Log.ShowLog(string.Format("=> EPS_UnlockECU(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_UnlockECU() success"));
            } else {
                Log.ShowLog(string.Format("=> EPS_UnlockECU() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int RoutineControl(byte option, int rid, byte[] Data, int dataLen, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EPS_RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("=> EPS_RoutineControl(), option = 0x{0:X2}, rid = 0x{1:X4}, iRet = {2}", option, rid, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_RoutineControl() success"));
            } else {
                Log.ShowLog(string.Format("=> EPS_RoutineControl() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ClearDTC() {
            int iRet = vciApp.AppServer.EPS_ClearDTC();
            Log.ShowLog(string.Format("=> EPS_ClearDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_ClearDTC() success"));
            } else {
                Log.ShowLog(string.Format("=> EPS_ClearDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ReadAllDTC(ref int NumOfDTC, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EPS_ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("=> EPS_ReadAllDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("=> EPS_ReadAllDTC() success"));
            } else {
                Log.ShowLog(string.Format("=> EPS_ReadAllDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int EPS7SCaliRun() {
            int iRet = 0;
            byte[] RecvData = new byte[200];
            int RetLen = 0;

            // 进入默认模式
            iRet = DiagSessionControl(1);

            // 进入扩展模式
            iRet = DiagSessionControl(3);

#if !DEBUG
            if (iRet != 0) {
                Log.ShowLog(string.Format("==> 7S EPS can't be accessed!"), LogBox.Level.error);
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }
#endif

            // 获取ECU SN号
            int did = 0xF18C;
            string ECU_SN = "";
            iRet = ReadDataByID(did, ref RecvData, ref RetLen);
            if (iRet == 0) {
                for (int i = 0; i < RetLen; i++) {
                    ECU_SN += (char)RecvData[i];
                }
                Log.ShowLog(string.Format("=> ECU_SN = {0}", ECU_SN));
            }

            // 进入安全访问
            iRet = UnlockECU();
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            byte option = 0;
            int rid = 0;
            byte[] Data = new byte[20];
            int dataLen = 0;

            // 开始标定例程
            option = 0x01;
            rid = 0x0100;
            iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            if (iRet != 0) {
                if (iRet < 256) {
                    // 负反馈
                    Log.ShowLog(string.Format("=> EPS_RoutineControl() returns negative response, NRC = {0}", GetNRCMessage((byte)iRet)), LogBox.Level.error);
                    DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", "NRC" + iRet.ToString("X2"), "-");
                }
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 停止标定例程
            Thread.Sleep(300);
            option = 0x02;
            rid = 0x0100;
            iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 获取标定结果
            option = 0x03;
            rid = 0x0100;
            iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            if (iRet != 0 || RetLen < 1) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }
            if (RecvData[0] != 0x01) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 清除DTC
            iRet = ClearDTC();
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 读取DTC
            int NumOfDTC = 0;
            int DTC = 0;
            byte Status = 0;
            int j = 0;
            iRet = ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }
            if (NumOfDTC > 0) {
                string strData = "=> Number of DTC: " + NumOfDTC.ToString();
                for (int i = 0; i < NumOfDTC; i++) {
                    DTC = ((RecvData[i * 4] << 16) + (RecvData[(i * 4) + 1] << 8) + RecvData[(i * 4) + 2]);
                    Status = RecvData[(i * 4) + 3];
                    j = i + 1;
                    strData += ", DTC[" + j.ToString() + "]: 0x" + DTC.ToString("X6") + " - 0x" + Status.ToString("X2");
                }
                Log.ShowLog(strData, LogBox.Level.error);
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "X", "-", Vci.DTCToString(NumOfDTC, RecvData));
            } else {
                // 没有DTC则标定成功
                Log.ShowLog("=> 7S EPS Calibration Procedure Success");
                DataBase.WriteCaliResult(StrVIN, "7S_EPS", "O", "-", "-");
                this.IsSuccess = true;
            }
            return iRet;
        }

        int EPS7LCaliRun() {
            int iRet = 0;
            byte[] RecvData = new byte[200];
            int RetLen = 0;

            // 进入默认模式
            iRet = DiagSessionControl(1);

            // 进入扩展模式
            iRet = DiagSessionControl(3);

#if !DEBUG
            if (iRet != 0) {
                Log.ShowLog(string.Format("==> 7L EPS can't be accessed!"), LogBox.Level.error);
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }
#endif

            // 读ECU序列号
            int did = 0xF18C;
            string ECU_SN = "";
            iRet = ReadDataByID(did, ref RecvData, ref RetLen);
            if (iRet == 0) {
                for (int i = 0; i < RetLen; i++) {
                    ECU_SN += (char)RecvData[i];
                }
                Log.ShowLog(string.Format("=> ECU_SN = {0}", ECU_SN));
            }

            // 进入安全访问
            iRet = UnlockECU();
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            byte option = 0;
            int rid = 0;
            byte[] Data = new byte[20];
            int dataLen = 0;

            // 开始标定例程
            option = 0x01;
            rid = 0x0100;
            iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 延时一段时间后，停止标定例程
            Thread.Sleep(300);
            option = 0x03;
            rid = 0x0100;
            iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 对返回结果进行分析
            if (RetLen >= 2) {
                if (RecvData[0] == 0x01) {
                    // 返回的例程状态为0x01表示标定例程执行成功
                    Log.ShowLog("=> RoutineStatus: Function active");
                    Log.ShowLog(string.Format("==> RoutineResult: {0}", GetErrorMessage(RecvData[1])));
                    if (RecvData[1] == 0x81) {
                        Log.ShowLog("=> RoutineControl() Success");
                    }
                } else {
                    Log.ShowLog("=> RoutineStatus: Function not active", LogBox.Level.error);
                    Log.ShowLog(string.Format("=> RoutineResult: {0}", GetErrorMessage(RecvData[1])), LogBox.Level.error);
                    DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", "Res" + RecvData[1].ToString("X2"), "-");
                    return iRet;
                }
            }

            // 清除DTC
            iRet = ClearDTC();
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", iRet.ToString(), "-");
                return iRet;
            }

            // 读取DTC
            int NumOfDTC = 0;
            int DTC = 0;
            byte Status = 0;
            int j = 0;
            iRet = ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            if (iRet != 0) {
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", iRet.ToString(), "-");
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
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "X", "-", Vci.DTCToString(NumOfDTC, RecvData));
            } else {
                // 没有DTC则标定成功
                Log.ShowLog("=> 7L EPS Calibration Procedure Success");
                DataBase.WriteCaliResult(StrVIN, "7L_EPS", "O", "-", "-");
                this.IsSuccess = true;
            }
            return iRet;
        }

        string GetErrorMessage(byte code) {
            string error;
            switch (code) {
                case 0x81:
                    error = "方向对中成功";
                    break;
                case 0x82:
                    error = "中位对中失败，P信号异常，检查传感器线束";
                    break;
                case 0x83:
                    error = "中位对中失败，S信号异常，检查传感器线束";
                    break;
                case 0x84:
                    error = "中位对中失败，P、S信号异常，检查传感器线束";
                    break;
                case 0x85:
                    error = "中位对中失败，P、S信号不匹配，检查传感器线束";
                    break;
                case 0x86:
                    error = "中位对中失败，传感器供电异常，检查传感器线束";
                    break;
                case 0x87:
                    error = "中位对中失败，操作方向盘力矩过大";
                    break;
                case 0x8F:
                    error = "中位对中数据存储失败";
                    break;
                default:
                    error = "方向对中未知错误";
                    break;
            }
            return error;
        }

        string GetNRCMessage(byte NRC) {
            string strNRC = "";
            switch (NRC) {
                case 0x83:
                    strNRC = "发动机必须停止运行";
                    break;
                case 0x88:
                    strNRC = "车速不能大于0km/h";
                    break;
                case 0x91:
                    strNRC = "点火按钮必须处于ON位置";
                    break;
                case 0x92:
                    strNRC = "电池电压不能高于17.5V";
                    break;
                case 0x93:
                    strNRC = "电池电压不能低于9V";
                    break;
                case 0x12:
                    strNRC = "$31服务不支持的子功能";
                    break;
                case 0x13:
                    strNRC = "$31服务的数据长度或格式不正确";
                    break;
                case 0x22:
                    strNRC = "$31服务条件有误";
                    break;
                case 0x24:
                    strNRC = "$31服务的请求顺序错误";
                    break;
                case 0x31:
                    strNRC = "$31服务的请求超出范围";
                    break;
                case 0x33:
                    strNRC = "$31服务的安全访问被拒绝";
                    break;
                default:
                    strNRC = "未知NRC代码";
                    break;
            }
            return strNRC;
        }
    }
}
