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
        readonly LogBox Log;

        public EPB(string StrDirTrace, string StrDirXML, string StrVIN, bool IsClearCali, bool IsRepeatCali, int RetrialTimes, LogBox Log) {
            Vci = new VciClient(StrDirTrace, StrDirXML, Log);
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
                        return iRet;
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

        int DiagSessionControl(byte mode) {
            int iRet = vciApp.AppServer.EV6S_EPB_DiagSessionControl(mode);
            Log.ShowLog(string.Format("===> EV6S_EPB_DiagSessionControl({0}), iRet = {1}", mode, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_DiagSessionControl({0}) success", mode));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_DiagSessionControl({0}) failed", mode), LogBox.Level.error);
            }
            return iRet;
        }

        int TestPresent() {
            int iRet = vciApp.AppServer.EV6S_EPB_TestPresent();
            Log.ShowLog(string.Format("===> EV6S_EPB_TestPresent(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_TestPresent() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_TestPresent() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ReadDataByID(int did, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EV6S_EPB_ReadDataByIdentifier(did, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_ReadDataByIdentifier(), did = 0x{0:X4}, iRet = {1}", did, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadDataByIdentifier() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadDataByIdentifier() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int UnlockECU() {
            int iRet = vciApp.AppServer.EV6S_EPB_UnlockECU();
            Log.ShowLog(string.Format("===> EV6S_EPB_UnlockECU(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_UnlockECU() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_UnlockECU() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int RoutineControl(byte option, int rid, byte[] Data, int dataLen, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EV6S_EPB_RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_RoutineControl(), option = 0x{0:X2}, rid = 0x{1:X4}, iRet = {2}", option, rid, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_RoutineControl() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_RoutineControl() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ClearDTC() {
            int iRet = vciApp.AppServer.EV6S_EPB_ClearDTC();
            Log.ShowLog(string.Format("===> EV6S_EPB_ClearDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_ClearDTC() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_ClearDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        int ReadAllDTC(ref int NumOfDTC, ref byte[] RecvData, ref int RetLen) {
            int iRet = vciApp.AppServer.EV6S_EPB_ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_ReadAllDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadAllDTC() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadAllDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        void EPBCaliRun() {
            int iRet = 0;
            byte[] RecvData = new byte[200];
            int RetLen = 0;

            // step 1
            iRet = DiagSessionControl(1);

            // step 2
            iRet = TestPresent();
            iRet = DiagSessionControl(3);

            // step 3
            //int did = 0xF18C;
            //string ECU_SN = "";
            //iRet = ReadDataByID(did, ref RecvData, ref RetLen);
            //if (iRet == 0) {
            //    for (int i = 0; i < RetLen; i++) {
            //        ECU_SN += (char)RecvData[i];
            //    }
            //    ShowLog(string.Format("===> ECU_SN = {0}", ECU_SN));
            //}

            // step 4
            iRet = UnlockECU();
            iRet = TestPresent();

            byte option = 0;
            int rid = 0;
            byte[] Data = new byte[20];
            int dataLen = 0;

            // undefined
            if (IsClearCali) {
                option = 0x01;
                rid = 0x0101;
                iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            }

            // step 5
            option = 0x01;
            rid = 0x0100;
            iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);

            int times = 0;
            while (times < RetrialTimes) {
                // step 6
                Thread.Sleep(300);

                // step 7
                option = 0x03;
                rid = 0x0100;
                iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);

                // step 8
                if (RetLen == 1) {
                    if (RecvData[0] == 0x01) {
                        // the calibration is done
                        // step 9
                        option = 0x02;
                        rid = 0x0100;
                        iRet = RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);

                        // step 11
                        iRet = ClearDTC();

                        // step 12
                        int NumOfDTC = 0;
                        int DTC = 0;
                        byte Status = 0;
                        int j = 0;
                        iRet = ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);

                        // step 13
                        if (NumOfDTC > 0) {
                            string strData = "===> Number of DTC = {0}" + NumOfDTC.ToString();
                            for (int i = 0; i < NumOfDTC; i++) {
                                DTC = ((RecvData[i * 4] << 16) + (RecvData[(i * 4) + 1] << 8) + RecvData[(i * 4) + 2]);
                                Status = RecvData[(i * 4) + 3];
                                j = i + 1;
                                strData += ", DTC[" + j.ToString() + "] = 0x" + DTC.ToString("X6") + " - 0x" + Status.ToString("X2");
                            }
                            Log.ShowLog(strData);
                            DataBase.WriteResult("EPSCaliProc", StrVIN, 1, Vci.DTCToString(NumOfDTC, RecvData));
                        } else {
                            DataBase.WriteResult("EPSCaliProc", StrVIN, 1, "");
                        }
                        break;
                    } else if (RecvData[0] > 0x80) {
                        // an error occurred
                        //ShowLog(string.Format("===> {0}", GetErrorMessage(RecvData[0])), LogBox.Level.error);
                        DataBase.WriteResult("EPSCaliProc", StrVIN, RecvData[0], "");
                        if (IsRepeatCali) {
                            // step 20
                            Thread.Sleep(2400);
                            continue;
                        }
                    }
                    // If RecvData[0] > 0x01 and < 0x80
                    // the ECU is still working on the calibration stage and not finished yet
                } else {
                    // an error occurred
                    //ShowLog(string.Format("===> {0}", GetErrorMessage(RecvData[0])), LogBox.Level.error);
                    DataBase.WriteResult("EPSCaliProc", StrVIN, RecvData[0], "");
                    break;
                }
            }
        }

    }
}
