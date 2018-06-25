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
    public partial class MainWindow : Window {
        LogBox Log { get; set; }
        public EPS EPSCali { get; set; }
        public EPB EPBCali { get; set; }
        const int RetrialTimes = 3;

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

            EPSCali = new EPS("vci_trace", "vci_config", StrVIN, (bool)this.ckbxClearCali.IsChecked, (bool)this.ckbxRepeatCali.IsChecked, RetrialTimes, Log);
            EPBCali = new EPB("vci_trace", "vci_config", StrVIN, (bool)this.ckbxClearCali.IsChecked, (bool)this.ckbxRepeatCali.IsChecked, RetrialTimes, Log);
        }

        private void BtnEPSStart_Click(object sender, RoutedEventArgs e) {
            EPSCali.StrVIN = this.tbxVIN.Text;
            EPSCali.IsClearCali = (bool)this.ckbxClearCali.IsChecked;
            EPSCali.IsRepeatCali = (bool)this.ckbxRepeatCali.IsChecked;
            if (int.TryParse(this.tbxRetrialTimes.Text, out int result)) {
                EPSCali.RetrialTimes = result;
                Log.ShowLog("====== EPS标定开始 ======");
                Task.Factory.StartNew(() => {
                    EPSCali.Run();
                });
            } else {
                string str = this.tbxRetrialTimes.Text + " 无法正确转换成整数，请重试\n";
                MessageBox.Show(str, "RetialTimes转换出错");
            }
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
            this.tbxRetrialTimes.Text = RetrialTimes.ToString();
        }

        private void BtnEPBStart_Click(object sender, RoutedEventArgs e) {
            EPBCali.StrVIN = this.tbxVIN.Text;
            EPBCali.IsClearCali = (bool)this.ckbxClearCali.IsChecked;
            EPBCali.IsRepeatCali = (bool)this.ckbxRepeatCali.IsChecked;
            EPBCali.StrSoftwareVer = this.tbxSoftwareVer.Text;
            EPBCali.StrHardwareVer = this.tbxHardwareVer.Text;
            if (int.TryParse(this.tbxRetrialTimes.Text, out int result)) {
                EPBCali.RetrialTimes = result;
                Log.ShowLog("====== EPB标定开始 ======");
                Task.Factory.StartNew(() => {
                    EPBCali.Run();
                });
        } else {
                string str = this.tbxRetrialTimes.Text + " 无法正确转换成整数，请重试\n";
                MessageBox.Show(str, "RetialTimes转换出错");
            }
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
