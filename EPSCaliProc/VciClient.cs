using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace EPSCaliProc {
    class VciClient {
        string StrDirTrace { get; set; }
        string StrDirXML { get; set; }
        bool IsServiceOpen { get; set; }
        bool IsDeviceOpen { get; set; }
        readonly LogBox Log;

        public VciClient(string StrDirTrace, string StrDirXML, LogBox Log) {
            this.StrDirTrace = StrDirTrace;
            this.StrDirXML = StrDirXML;
            this.IsServiceOpen = false;
            this.IsDeviceOpen = false;
            this.Log = Log;
        }

        public int Close() {
            int iRet = 0;
            iRet = StopDevice();
            iRet = StopService();
            return iRet;
        }

        public int StartService() {
            int iRet = vciApp.AppServer.startService(StrDirTrace, StrDirXML);
            if (iRet == 0) {
                IsServiceOpen = true;
            }
            return iRet;
        }

        public int StopService() {
            int iRet = 0;
            if (IsServiceOpen) {
                Log.ShowLog("=> StopService...\n");
                iRet = vciApp.AppServer.stopService();
            }
            return iRet;
        }

        public int StartDevice() {
            int iRet = 0;
            int iDeviceID = 0;
            int recvLen = 0;
            int timeout = 0;
            byte[] arrbtCMD = { 0xD0 };
            byte[] arrbtRecv = new byte[50];

            iDeviceID = 1;
            iRet = vciApp.AppServer.avtMcDevice("DEVICE_START", iDeviceID);
            if (iRet != 0) {
                return iRet;
            }

            timeout = 1400;
            iRet = SendCommandMC(iDeviceID, arrbtCMD, 1, timeout, arrbtRecv, 50, ref recvLen);
            if (iRet == 0) {
                IsDeviceOpen = true;
            }
            return iRet;
        }

        public int SendCommandMC(int devID, byte[] arrbtCMD, int numResp, int timeout, byte[] arrbtResp, int inRespLen, ref int outRespLen) {
            int iRet = 0;
            int iRecvLen = 0;
            byte[] arrbtRecvData = new byte[inRespLen];

            string strParam = devID.ToString() + "|" + numResp.ToString() + "|" + timeout.ToString();
            int cmdLen = arrbtCMD.Length;
            iRet = vciApp.AppServer.avtMcCommand("MC_SENDCOMMAND", strParam, arrbtCMD, cmdLen, ref arrbtRecvData, ref iRecvLen);

            if (iRet == 0) {
                if (iRecvLen > inRespLen) {
                    Log.ShowLog("RecvData缓冲区将会溢出", LogBox.Level.error);
                    return -1;
                }
                for (int i = 0; i < iRecvLen; i++) {
                    arrbtResp[i] = arrbtRecvData[i];
                }
                outRespLen = iRecvLen;
            }
            return iRet;
        }

        public int StopDevice() {
            int iRet = 0;
            if (IsDeviceOpen) {
                Log.ShowLog("==> StopDevice...\n");
                int iDeviceID = 1;
                iRet = vciApp.AppServer.avtMcDevice("DEVICE_CLOSE", iDeviceID);
            }
            return iRet;
        }

        public string DTCToString(int NumOfDTC, byte[] RecvData) {
            string strResult = "";
            int DTC = 0;
            byte Status = 0;

            for (int i = 0; i < NumOfDTC; i++) {
                DTC = ((RecvData[i * 4] << 16) + (RecvData[(i * 4) + 1] << 8) + RecvData[(i * 4) + 2]);
                Status = RecvData[(i * 4) + 3];
                strResult += DTC.ToString("X6") + "," + Status.ToString("X2") + "|";
            }
            return strResult.Remove(strResult.Length - 1, 1);
        }

    }
}
