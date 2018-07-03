using Newtonsoft.Json;
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
        public DBConfig DBCfg;
        readonly LogBox Log;

        public Model(LogBox Log) {
            this.Log = Log;
            this.StrConn = "";
            this.StrConfigFile = "./DB_config.json";
            DBCfg = new DBConfig();
            ReadConfig();
        }

        public Model(string StrConfig, LogBox Log) {
            this.Log = Log;
            this.StrConn = "";
            this.StrConfigFile = StrConfig;
            DBCfg = new DBConfig();
            ReadConfig();
        }

        void ReadConfig() {
            try {
                using (StreamReader file = File.OpenText(StrConfigFile)) {
                    JsonSerializer serializer = new JsonSerializer();
                    DBCfg = serializer.Deserialize(file, typeof(DBConfig)) as DBConfig;
                }
            } catch (Exception e) {
                Log.ShowLog("===> ERROR: " + e.Message, LogBox.Level.error);
            }
            StrConn = "user id=" + DBCfg.DB_UserID + ";";
            StrConn += "password=" + DBCfg.DB_Pwd + ";";
            StrConn += "database=" + DBCfg.DB_Name + ";";
            StrConn += "data source=" + DBCfg.DB_IP + "," + DBCfg.DB_Port;
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

    public class DBConfig {
        public string DB_IP { get; set; }
        public string DB_Port { get; set; }
        public string DB_Name { get; set; }
        public string DB_UserID { get; set; }
        public string DB_Pwd { get; set; }
    }

}
