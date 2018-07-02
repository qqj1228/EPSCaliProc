using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPSCaliProc {
    public class EPBApp : vciApp.EV6S_EPB {
        public EPBApp(int device, int ich, vciApp.VCI_Channel channel, vciApp.MessagePipe pipe) : base(device, ich, channel, pipe) {
            try {
                this.m_ecuID = 0x741;
                this.m_testerID = 0x749;
            } catch {
                base.Dispose(true);
                throw;
            }
        }

        public void ShowInfo() {
            Console.WriteLine("EPB_ID: 0x{0:X4}\nTest_ID: 0x{1:X4}", m_ecuID, m_testerID);
        }

        public int DiagSessionControl(byte mode) {
            int iRet = DiagnosticSessionControl(mode);
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_DiagSessionControl({0}), iRet = {1}", mode, iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }

        new public int TestPresent() {
            int iRet = base.TestPresent();
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_TestPresent(), iRet = {0}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }

        public unsafe int ReadDataByID(int did, ref byte[] RecvData, ref int RetLen) {
            int iRet = 0;
            RetLen = 0;
            byte[] RespData = new byte[500];
            fixed (byte * pResp = RespData) {
                iRet = ReadDataByIdentifier(did, pResp, 500, ref RetLen);
                if (iRet == 0) {
                    if (RecvData.Length < RetLen) {
                        iRet = 11013;
                    } else {
                        for (int i = 0; i < RetLen; i++) {
                            RecvData[i] = *(pResp + i);
                        }
                    }
                }
            }
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_ReadDataByIdentifier(), iRet = {0:d}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }

        public int WriteDataByID(int did, byte[] Data, int iDataLen) {
            int iRet = 0;
            iRet = WriteDataByIdentifier(did, Data, iDataLen);
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_WriteDataByIdentifier(), iRet = {0:d}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }


        new public int UnlockECU() {
            int iRet = base.UnlockECU();
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_UnlockECU(), iRet = {0:d}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }

        public unsafe int RoutineControl(byte option, int rid, byte[] Data, int dataLen, ref byte[] RecvData, ref int RetLen) {
            int iRet = 0;
            RetLen = 0;
            byte[] RespData = new byte[500];
            fixed (byte* pResp = RespData) {
                iRet = base.RoutineControl(option, rid, Data, dataLen, pResp, 500, ref RetLen);
                if (iRet == 0) {
                    if (RecvData.Length < RetLen) {
                        iRet = 11013;
                    } else {
                        for (int i = 0; i < RetLen; i++) {
                            RecvData[i] = *(pResp + i);
                        }
                    }
                }
            }
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_RoutineControl(), iRet = {0:d}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }

        public int ClearDTC() {
            int iRet = ClearDiagnosticInformation();
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6_EPS_ClearDTC(), iRet = {0:d}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }

        public unsafe int ReadAllDTC(ref int NumOfDTC, ref byte[] RecvData, ref int RetLen) {
            int iRet = 0;
            RetLen = 0;
            byte[] RespData = new byte[500];
            fixed (byte* pResp = RespData) {
                iRet = base.ReadAllDTC(ref NumOfDTC, pResp, 500, ref RetLen);
                if (iRet == 0) {
                    if (RecvData.Length < RetLen) {
                        iRet = 11013;
                    } else {
                        for (int i = 0; i < RetLen; i++) {
                            RecvData[i] = *(pResp + i);
                        }
                    }
                }
            }
            if (vciBase.VciServer.IsTraceOn()) {
                string info = string.Format("EV6S_EPB_ReadAllDTC(), iRet = {0:d}", iRet);
                vciBase.VciServer.OutputTrace(1, info);
                vciBase.VciServer.FlushTrace();
            }
            return iRet;
        }
    }

    public class VciEPBApp {
        readonly vciApp.MessagePipe _Pipe;
        readonly vciApp.VCI_Channel _Channel;
        readonly EPBApp app;
        readonly LogBox Log;

        public VciEPBApp(LogBox Log) {
            _Pipe = vciApp.MessagePipe.GetMessagePipe();
            _Channel = new vciApp.VCI_Channel(1, 0);
            app = new EPBApp(1, 0, _Channel, _Pipe);
            this.Log = Log;
            app.ShowInfo();
        }

        public int DiagSessionControl(byte mode) {
            int iRet = app.DiagnosticSessionControl(mode);
            Log.ShowLog(string.Format("===> EV6S_EPB_DiagSessionControl({0}), iRet = {1}", mode, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_DiagSessionControl({0}) success", mode));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_DiagSessionControl({0}) failed", mode), LogBox.Level.error);
            }
            return iRet;
        }

        public int TestPresent() {
            int iRet = app.TestPresent();
            Log.ShowLog(string.Format("===> EV6S_EPB_TestPresent(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_TestPresent() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_TestPresent() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        public int ReadDataByID(int did, ref byte[] RecvData, ref int RetLen) {
            int iRet = app.ReadDataByID(did, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_ReadDataByIdentifier(), did = 0x{0:X4}, iRet = {1}", did, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadDataByIdentifier() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadDataByIdentifier() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        public int WriteDataByID(int did, byte[] Data, int iDataLen) {
            int iRet = app.WriteDataByID(did, Data, iDataLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_WriteDataByIdentifier(), did = 0x{0:X4}, iRet = {1}", did, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_WriteDataByIdentifier() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_WriteDataByIdentifier() failed"), LogBox.Level.error);
            }
            return iRet;
        }


        public int UnlockECU() {
            int iRet = app.UnlockECU();
            Log.ShowLog(string.Format("===> EV6S_EPB_UnlockECU(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_UnlockECU() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_UnlockECU() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        public int RoutineControl(byte option, int rid, byte[] Data, int dataLen, ref byte[] RecvData, ref int RetLen) {
            int iRet = app.RoutineControl(option, rid, Data, dataLen, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_RoutineControl(), option = 0x{0:X2}, rid = 0x{1:X4}, iRet = {2}", option, rid, iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_RoutineControl() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_RoutineControl() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        public int ClearDTC() {
            int iRet = app.ClearDTC();
            Log.ShowLog(string.Format("===> EV6S_EPB_ClearDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_ClearDTC() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_ClearDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

        public int ReadAllDTC(ref int NumOfDTC, ref byte[] RecvData, ref int RetLen) {
            int iRet = app.ReadAllDTC(ref NumOfDTC, ref RecvData, ref RetLen);
            Log.ShowLog(string.Format("===> EV6S_EPB_ReadAllDTC(), iRet = {0}", iRet));
            if (iRet == 0) {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadAllDTC() success"));
            } else {
                Log.ShowLog(string.Format("===> EV6S_EPB_ReadAllDTC() failed"), LogBox.Level.error);
            }
            return iRet;
        }

    }
}
