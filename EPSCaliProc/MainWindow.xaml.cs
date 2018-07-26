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

            Log = new LogBox(this.rbxLog, this.rbxDoc);
            Cfg = new Config(Log);
            InitUI();

            EPSCali = new EPS("vci_trace", "vci_config", StrVIN, Cfg, Log);
            EPBCali = new EPB("vci_trace", "vci_config", StrVIN, Cfg, Log);
        }

        private void InitUI() {
            this.ckbxClearCali.IsChecked = Cfg.Main.ClearEPS;
            this.ckbxRepeatCali.IsChecked = Cfg.Main.Retry;
            this.tbxRetrialTimes.Text = Cfg.Main.RetryTimes.ToString();
        }

        private void BtnEPSStart_Click(object sender, RoutedEventArgs e) {
            EPSCali.StrVIN = this.tbxVIN.Text;
            Log.ShowLog("====== EPS标定开始 ======");
            Task.Factory.StartNew(() => {
                EPSCali.Run();
            });
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) {
            Log.ClearLog();
        }

        private void LogBox_Loaded(object sender, RoutedEventArgs e) {
            // 获取程序参数中传进来的VIN码
            if (Application.Current.Properties["VIN"] != null) {
                StrVIN = Application.Current.Properties["VIN"].ToString();
            }
            this.tbxVIN.Text = StrVIN;
        }

        private void BtnEPBStart_Click(object sender, RoutedEventArgs e) {
            EPBCali.StrVIN = this.tbxVIN.Text;
            EPBCali.StrSoftwareVer = this.tbxSoftwareVer.Text;
            EPBCali.StrHardwareVer = this.tbxHardwareVer.Text;
            Log.ShowLog("====== EPB标定开始 ======");
            Task.Factory.StartNew(() => {
                EPBCali.Run();
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

        private void CkbxClearCali_Click(object sender, RoutedEventArgs e) {
            Cfg.Main.ClearEPS = (bool)this.ckbxClearCali.IsChecked;
        }

        private void CkbxRepeatCali_Click(object sender, RoutedEventArgs e) {
            Cfg.Main.Retry = (bool)this.ckbxRepeatCali.IsChecked;
        }

        private void TbxRetrialTimes_LostFocus(object sender, RoutedEventArgs e) {
            int times = Cfg.Main.RetryTimes;
            try {
                times = Convert.ToInt32(this.tbxRetrialTimes.Text);
            } catch (Exception ex) {
                string str = "ERROR: \"" + this.tbxRetrialTimes.Text + "\" 输入错误, " + ex.Message + "\n忽略输入值，请重新输入";
                Log.ShowLog(str, LogBox.Level.error);
                MessageBox.Show(str, "RetialTimes输入错误");
                this.tbxRetrialTimes.Text = times.ToString();
            }
            Cfg.Main.RetryTimes = times;
        }
    }

    public class LogBox {
        readonly RichTextBox rbxLog;
        readonly FlowDocument rbxDoc;
        readonly Dispatcher dp;
        Paragraph Para { get; set; }

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
        }

        public void ShowLog(string strLog, Level le = Level.info) {
            this.dp.Invoke(new Action(() => {
                Run run = new Run(strLog + "\n");
                if (le == Level.error) {
                    run.Foreground = new SolidColorBrush(Colors.Red);
                }
                this.Para.Inlines.Add(run);
                this.rbxLog.ScrollToEnd();
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
