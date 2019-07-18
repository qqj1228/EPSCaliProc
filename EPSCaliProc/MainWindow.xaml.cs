using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EPSCaliProc {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow {
        LogBox Log { get; set; }
        public Config Cfg { get; set; }
        public EPS EPSCali { get; set; }
        public EPB EPBCali { get; set; }
        public bool IsManualVIN { get; set; }

        private string strVIN;
        public string StrVIN {
            get { return strVIN; }
            set {
                if (value.Length > 17) {
                    strVIN = value.Substring(0, 17);
                    Log.ShowLog("VIN码长度大于17，将会截断长度大于17的字符", LogBox.Level.error);
                    Log.ShowLog("原VIN码：" + value + ", 新VIN码：" + StrVIN);
                } else {
                    strVIN = value;
                    Log.ShowLog("VIN码：" + StrVIN);
                }
            }
        }

        public MainWindow() {
            InitializeComponent();
            // ckbxVIN绑定的必要前提
            this.ckbxVIN.DataContext = this;

            Log = new LogBox(this.rbxLog, this.rbxDoc);
            Cfg = new Config(Log);

            // 获取程序第一个参数中传进来的VIN码
            if (Application.Current.Properties["VIN"] != null) {
                StrVIN = Application.Current.Properties["VIN"].ToString();
            }

            EPSCali = new EPS("vci_trace", "vci_config", Cfg, Log);
            EPBCali = new EPB("vci_trace", "vci_config", Cfg, Log);

            if (Cfg.Main.AutoRun) {
                this.IsManualVIN = false;
                Task.Factory.StartNew(() => {
                    CaliSignalListener();
                });
                Task.Factory.StartNew(() => {
                    AutoRunCali();
                });
            } else {
                this.IsManualVIN = true;
            }
        }

        private void CaliSignalListener() {
            string[] status = new string[2];
            Model db = new Model(Cfg, Log);
            while (true) {
                if (!IsManualVIN) {
                    status = db.GetCaliStatus();
                    strVIN = status[0];
                    // 跨UI控件线程调用需使用：Diapatcher + Action + lambda
                    this.tbxVIN.Dispatcher.Invoke(new Action(() => {
                        this.tbxVIN.Text = strVIN;
                    }));
                }
                Thread.Sleep(Cfg.Main.Interval);
            }
        }

        private void AutoRunCali() {
            Log.ShowLog("等待标定信号。。。");
            Model db = new Model(Cfg, Log);
            while (true) {
                if (StrVIN != null && StrVIN != "") {
                    Log.ClearLog();
                    // EPS标定
                    bool bResult = false;
                    while (!bResult) {
                        bResult = EPSCali.Run(0, StrVIN);
                        if (!bResult) {
                            MessageBoxResult res = MessageBox.Show("EPS标定失败，是否重试？\n若要重试的话需要将车辆下电至少5秒后再重试。", "标定失败", MessageBoxButton.YesNo, MessageBoxImage.Error);
                            if (res == MessageBoxResult.No) {
                                break;
                            }
                        }
                    }
                    if (bResult) {
                        Log.ShowLog("VIN号为 \"" + StrVIN + "\" 的车辆 EPS 标定成功\n");
                    } else {
                        Log.ShowLog("VIN号为 \"" + StrVIN + "\" 的车辆 EPS 标定失败\n");
                    }

                    // EPB标定
                    bResult = false;
                    while (!bResult) {
                        bResult = EPBCali.Run(0, StrVIN);
                        if (!bResult) {
                            MessageBoxResult res = MessageBox.Show("EPB标定失败，是否重试？\n若要重试的话需要将车辆下电至少5秒后再重试。", "标定失败", MessageBoxButton.YesNo, MessageBoxImage.Error);
                            if (res == MessageBoxResult.No) {
                                break;
                            }
                        }
                    }
                    if (bResult) {
                        Log.ShowLog("VIN号为 \"" + StrVIN + "\" 的车辆 EPB 标定成功\n");
                    } else {
                        Log.ShowLog("VIN号为 \"" + StrVIN + "\" 的车辆 EPB 标定失败\n");
                    }
                    db.DeleteCaliStatus();
                    Log.ShowLog("等待标定信号。。。");

                }
                Thread.Sleep(Cfg.Main.Interval);
            }
        }

        private void Btn7SEPSStart_Click(object sender, RoutedEventArgs e) {
            if (this.tbxVIN.Text == "") {
                MessageBox.Show("VIN号不能为空", "VIN号出错");
                return;
            }
            if (this.tbxVIN.Text.Length > 17) {
                MessageBox.Show("VIN号长度不能大于17", "VIN号出错");
                return;
            }
            if (this.tbxVIN.Text.Length < 17) {
                MessageBox.Show("VIN号长度不能小于17", "VIN号出错");
                return;
            }
            Log.ShowLog("====== 手动 7S EPS 标定开始 ======");
            string strVIN = this.tbxVIN.Text;
            Task.Factory.StartNew(() => {
                EPSCali.Run(1, strVIN);
            });
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) {
            Log.ClearLog();
        }

        private void LogBox_Loaded(object sender, RoutedEventArgs e) {
            this.tbxVIN.Text = StrVIN;
        }

        private void Btn7SEPBStart_Click(object sender, RoutedEventArgs e) {
            if (this.tbxVIN.Text == "") {
                MessageBox.Show("VIN号不能为空", "VIN号出错");
                return;
            }
            if (this.tbxVIN.Text.Length > 17) {
                MessageBox.Show("VIN号长度不能大于17", "VIN号出错");
                return;
            }
            if (this.tbxVIN.Text.Length < 17) {
                MessageBox.Show("VIN号长度不能小于17", "VIN号出错");
                return;
            }
            Log.ShowLog("====== 手动 7S EPB标定开始 ======");
            string strVIN = this.tbxVIN.Text;
            Task.Factory.StartNew(() => {
                EPBCali.Run(1, strVIN);
            });
        }

        private void Btn7LEPSStart_Click(object sender, RoutedEventArgs e) {
            if (this.tbxVIN.Text == "") {
                MessageBox.Show("VIN号不能为空", "VIN号出错");
                return;
            }
            if (this.tbxVIN.Text.Length > 17) {
                MessageBox.Show("VIN号长度不能大于17", "VIN号出错");
                return;
            }
            if (this.tbxVIN.Text.Length < 17) {
                MessageBox.Show("VIN号长度不能小于17", "VIN号出错");
                return;
            }
            Log.ShowLog("====== 手动 7L EPS标定开始 ======");
            string strVIN = this.tbxVIN.Text;
            Task.Factory.StartNew(() => {
                EPSCali.Run(2, strVIN);
            });
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e) {
            // 默认contextmenu只能右键弹出，加入以下代码使之按下左键也能弹出
            this.menu.PlacementTarget = this.btnMenu;
            this.menu.IsOpen = true;
        }

        private void MenuDark_Checked(object sender, RoutedEventArgs e) {
            ThemeManager.ChangeAppTheme(Application.Current, "BaseDark");
        }

        private void MenuDark_Unchecked(object sender, RoutedEventArgs e) {
            ThemeManager.ChangeAppTheme(Application.Current, "BaseLight");
        }

        private void CbxVIN_Checked(object sender, RoutedEventArgs e) {
            //this.IsManualVIN = (bool)this.ckbxVIN.IsChecked;
        }
    }

    public class LogBox {
        readonly RichTextBox rbxLog;
        readonly FlowDocument rbxDoc;
        readonly Dispatcher dp;
        Paragraph Para { get; set; }
        Logger Log;

        public enum Level {
            error,
            info,
        }

        public LogBox(RichTextBox rbxLog, FlowDocument rbxDoc) {
            this.rbxLog = rbxLog;
            this.rbxDoc = rbxDoc;
            this.dp = this.rbxLog.Dispatcher;
            this.Para = new Paragraph();
            this.rbxDoc.Blocks.Add(Para);
            Log = new Logger("./log", EnumLogLevel.LogLevelAll, false, 100);
        }

        public void ShowLog(string strLog, Level lv = Level.info) {
            this.dp.Invoke(new Action(() => {
                Run run = new Run(strLog + "\n");
                if (lv == Level.error) {
                    run.Foreground = new SolidColorBrush(Colors.Red);
                    Log.TraceError(strLog);
                } else {
                    Log.TraceInfo(strLog);
                }
                this.Para.Inlines.Add(run);
                this.rbxLog.ScrollToEnd();
                if (strLog.Contains("Close!")) {
                    Application.Current.Shutdown();
                }
            }));
        }

        public void ClearLog() {
            this.dp.Invoke(new Action(() => {
                this.rbxDoc.Blocks.Clear();
                // 调用Blocks.Clear()会移除Block内原有的Paragraph（原Paragraph仍然存在，只是不在Block内了）
                // 若需要写入新内容的话，需要新建Paragraph, 再加入Block中
                // 若不新建Paragraph, 仍旧使用原来的Paragraph加入Block中的话,原先的内容还会显示在RichTextBox内
                this.Para = new Paragraph();
                this.rbxDoc.Blocks.Add(Para);
            }));
        }
    }
}
