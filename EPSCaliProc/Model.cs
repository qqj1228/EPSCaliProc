using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace EPSCaliProc {
    public class Model {
        public string StrConn { get; set; }
        public string StrConfigFile { get; set; }
        readonly LogBox Log;
        readonly Config Cfg;

        public Model(Config Cfg, LogBox Log) {
            this.Cfg = Cfg;
            this.Log = Log;
            this.StrConn = "";
            ReadConfig();
        }

        void ReadConfig() {
            StrConn = "user id=" + Cfg.DB.UserID + ";";
            StrConn += "password=" + Cfg.DB.Pwd + ";";
            StrConn += "database=" + Cfg.DB.Name + ";";
            StrConn += "data source=" + Cfg.DB.IP + "," + Cfg.DB.Port;
        }

        public void ShowDB(string StrTable) {
            string StrSQL = "select * from " + StrTable;

            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                SqlDataReader sqlData = sqlCmd.ExecuteReader();
                string str = "";
                int c = sqlData.FieldCount;
                while (sqlData.Read()) {
                    for (int i = 0; i < c; i++) {
                        object obj = sqlData.GetValue(i);
                        if (obj.GetType() == typeof(DateTime)) {
                            str += ((DateTime)obj).ToString("yyyy-MM-dd") + "\t";
                        } else {
                            str += obj.ToString() + "\t";
                        }
                    }
                    str += "\n";
                }
                Log.ShowLog(str);
            }
        }

        public void WriteResult(string StrTable, string StrVIN, string StrResult, int RoutineStatus, int RoutineResult, string StrDTC) {
            string StrSQL = "insert " + StrTable + " values ('";
            StrSQL += StrVIN + "', '";
            StrSQL += DateTime.Now.ToString("yyyy-MM-dd") + "', '";
            StrSQL += DateTime.Now.ToLongTimeString() + "', '";
            StrSQL += StrResult + "', '";
            StrSQL += RoutineStatus.ToString("X2") + "', '";
            StrSQL += RoutineResult.ToString("X2") + "', '";
            StrSQL += StrDTC + "')";

            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                try {
                    sqlConn.Open();
                    Log.ShowLog(string.Format("===> T-SQL: {0}", StrSQL));
                    Log.ShowLog(string.Format("===> EPSCaliProc Done. Insert {0} record(s)", sqlCmd.ExecuteNonQuery()));
                } catch (Exception e) {
                    Log.ShowLog("===> ERROR: " + e.Message, LogBox.Level.error);
                }
            }
        }

        public void WriteEPBResult(string StrTable, string StrVIN, string[] strResult, string strDTC) {
            string strAllResult = "O";
            string StrSQL = "insert " + StrTable + " values ('";
            StrSQL += StrVIN + "', '";
            StrSQL += DateTime.Now.ToString("yyyy-MM-dd") + "', '";
            StrSQL += DateTime.Now.ToLongTimeString() + "', '";
            for (int i = 0; i < 5; i++) {
                if (strResult[i] != "O") {
                    strAllResult = "X";
                    break;
                }
            }
            StrSQL += strAllResult + "', '";
            for (int i = 0; i < 5; i++) {
                StrSQL += strResult[i] + "', '";
            }
            StrSQL += strDTC + "')";

            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                try {
                    sqlConn.Open();
                    Log.ShowLog(string.Format("===> T-SQL: {0}", StrSQL));
                    Log.ShowLog(string.Format("===> EPBCaliProc Done. Insert {0} record(s)", sqlCmd.ExecuteNonQuery()));
                } catch (Exception e) {
                    Log.ShowLog("===> ERROR: " + e.Message, LogBox.Level.error);
                }
            }
        }
    }
}
