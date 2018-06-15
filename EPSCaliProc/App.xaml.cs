using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace EPSCaliProc
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            if (e.Args != null && e.Args.Count() > 0) {
                this.Properties["VIN"] = e.Args[0];
            }
        }
    }
}
