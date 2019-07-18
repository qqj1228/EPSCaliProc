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
                sqlConn.Close();
            }
        }

        public void WriteCaliResult(string StrVIN, string strECU, string StrResult, string NRCOrResult, string StrDTC) {
            string StrSQL = "insert CaliProcResult values ('";
            StrSQL += StrVIN + "', '";
            StrSQL += DateTime.Now.ToString("yyyy-MM-dd") + "', '";
            StrSQL += DateTime.Now.ToLongTimeString() + "', '";
            StrSQL += strECU + "', '";
            StrSQL += StrResult + "', '";
            StrSQL += NRCOrResult + "', '";
            StrSQL += StrDTC + "')";

            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                try {
                    sqlConn.Open();
                    Log.ShowLog(string.Format("==> T-SQL: {0}", StrSQL));
                    Log.ShowLog(string.Format("==> Writed calibration result. Insert {0} record(s)", sqlCmd.ExecuteNonQuery()));
                } catch (Exception e) {
                    Log.ShowLog("==> SQL ERROR: " + e.Message, LogBox.Level.error);
                    Log.ShowLog("==> Wrong SQL: " + StrSQL, LogBox.Level.error);
                } finally {
                    sqlConn.Close();
                }
            }
        }

        public string GetVehicleType(string strVIN) {
            string ret = "";
            string StrSQL = "select VehicleType from VehicleInfo where VIN = '" + strVIN + "'";
            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                try {
                    sqlConn.Open();
                    SqlDataReader sqlData = sqlCmd.ExecuteReader();
                    if (sqlData.Read()) {
                        ret = sqlData.GetString(0);
                    }
                    Log.ShowLog(string.Format("==> T-SQL: {0}", StrSQL));
                    Log.ShowLog(string.Format("==> Get vehicle type: {0}", ret));
                } catch (Exception e) {
                    Log.ShowLog("==> SQL ERROR: " + e.Message, LogBox.Level.error);
                    Log.ShowLog("==> Wrong SQL: " + StrSQL, LogBox.Level.error);
                } finally {
                    sqlConn.Close();
                }
                return ret;
            }
        }

        /// <summary>
        /// 获取标定状态，返回值为2个元素的string数组，[0]：VIN号，[1]：status
        /// </summary>
        /// <returns></returns>
        public string[] GetCaliStatus() {
            string[] ret = new string[2];
            string StrSQL = "select VIN, Status from CaliProcStatus";
            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                try {
                    sqlConn.Open();
                    SqlDataReader sqlData = sqlCmd.ExecuteReader();
                    while (sqlData.Read()) {
                        ret[0] = sqlData.GetString(0);
                        ret[1] = sqlData.GetInt32(1).ToString();
                    }
                } catch (Exception e) {
                    Log.ShowLog("==> SQL ERROR: " + e.Message, LogBox.Level.error);
                    Log.ShowLog("==> Wrong SQL: " + StrSQL, LogBox.Level.error);
                } finally {
                    sqlConn.Close();
                }
            }
            return ret;
        }

        public void DeleteCaliStatus() {
            string StrSQL = "delete from CaliProcStatus";
            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                SqlCommand sqlCmd = new SqlCommand(StrSQL, sqlConn);
                try {
                    sqlConn.Open();
                    Log.ShowLog(string.Format("==> T-SQL: {0}", StrSQL));
                    Log.ShowLog(string.Format("==> Delete {0} record(s) in CaliProcStatus.", sqlCmd.ExecuteNonQuery()));
                } catch (Exception e) {
                    Log.ShowLog("==> SQL ERROR: " + e.Message, LogBox.Level.error);
                    Log.ShowLog("==> Wrong SQL: " + StrSQL, LogBox.Level.error);
                } finally {
                    sqlConn.Close();
                }
            }
        }
    }
}
