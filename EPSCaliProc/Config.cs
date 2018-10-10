using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EPSCaliProc {
    public class Config {
        public struct DBConfig {
            public string IP { get; set; }
            public string Port { get; set; }
            public string Name { get; set; }
            public string UserID { get; set; }
            public string Pwd { get; set; }
        }

        public struct MainConfig {
            public bool ClearEPS { get; set; }
            public bool Retry { get; set; }
            public int RetryTimes { get; set; }
            public bool AutoRun { get; set; }
        }

        public DBConfig DB;
        public MainConfig Main;
        readonly LogBox Log;
        string ConfigFile { get; set; }

        public Config(LogBox Log, string strConfigFile = "config.xml") {
            this.Log = Log;
            this.ConfigFile = strConfigFile;
            LoadConfig();
        }

        ~Config() {
            SaveConfig();
        }

        void LoadConfig() {
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ConfigFile);
                XmlNode xnRoot = xmlDoc.SelectSingleNode("Config");
                XmlNodeList xnl = xnRoot.ChildNodes;

                foreach (XmlNode node in xnl) {
                    XmlNodeList xnlChildren = node.ChildNodes;
                    if (node.Name == "Main") {
                        foreach (XmlNode item in xnlChildren) {
                            if (item.Name == "ClearEPS") {
                                bool.TryParse(item.InnerText, out bool result);
                                Main.ClearEPS = result;
                            } else if (item.Name == "Retry") {
                                bool.TryParse(item.InnerText, out bool result);
                                Main.Retry = result;
                            } else if (item.Name == "RetryTimes") {
                                int.TryParse(item.InnerText, out int result);
                                Main.RetryTimes = result;
                            } else if (item.Name == "AutoRun") {
                                bool.TryParse(item.InnerText, out bool result);
                                Main.AutoRun = result;
                            }
                        }
                    } else if (node.Name == "DB") {
                        foreach (XmlNode item in xnlChildren) {
                            if (item.Name == "IP") {
                                DB.IP = item.InnerText;
                            } else if (item.Name == "Port") {
                                DB.Port = item.InnerText;
                            } else if (item.Name == "Name") {
                                DB.Name = item.InnerText;
                            } else if (item.Name == "UserID") {
                                DB.UserID = item.InnerText;
                            } else if (item.Name == "Pwd") {
                                DB.Pwd = item.InnerText;
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Log.ShowLog("ERROR: " + e.Message, LogBox.Level.error);
            }
        }

        void SaveConfig() {
            try {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(ConfigFile);
                XmlNode xnRoot = xmlDoc.SelectSingleNode("Config");
                XmlNodeList xnl = xnRoot.ChildNodes;

                foreach (XmlNode node in xnl) {
                    XmlNodeList xnlChildren = node.ChildNodes;
                    // 只操作了只会在程序中被修改的配置项
                    if (node.Name == "Main") {
                        foreach (XmlNode item in xnlChildren) {
                            if (item.Name == "ClearEPS") {
                                item.InnerText = Main.ClearEPS.ToString();
                            } else if (item.Name == "Retry") {
                                item.InnerText = Main.Retry.ToString();
                            } else if (item.Name == "RetryTimes") {
                                item.InnerText = Main.RetryTimes.ToString();
                            }
                        }
                    }
                }

                xmlDoc.Save(ConfigFile);
            } catch (Exception e) {
                Log.ShowLog("ERROR: " + e.Message, LogBox.Level.error);
            }
        }
    }
}
