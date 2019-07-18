## 江淮青州项目IEV6E车型EPS模块下线标定程序说明

1. `EPSCaliProc.exe` 为主程序

2. `Config.xml` 为主配置文件，用于对主程序数据库功能进行配置：
	- `Main/ClearEPS` ：主界面 `清除上次EPS标定结果`，选项: `True` 、`False`
	- `Main/Retry` ：主界面 `EPS/EPB标定出错后重试`，选项: `True` 、`False`
	- `Main/RetryTimes` ：主界面 `重试次数` 输入框
	- `Main/AutoRun` ：程序是否自动运行，选项: `True` 、`False`，若为 `True` 的话，程序打开后会自动进入EPS标定过程，标定完成后会自动关闭无需人工干预
	- `DB/IP` ：SQL Server服务器IP地址
	- `DB/Port` ：SQL Server服务器端口号
	- `DB/Name` ：主程序操作的SQL Server数据库名称
	- `DB/UserID` ：SQL Server登录账号用户名（生产环境下不建议使用默认管理员账号 `sa` ）
	- `DB/Pwd` ：SQL Server登录账号密码

3. `vci_config` 目录为澳洲MVTU操作DLL的配置目录，内有澳洲DLL的配置文件。

4. `vci_trace` 目录为澳洲MVTU操作DLL的log文件目录，内有澳洲DLL的log文件。

5. 使用方法为在命令行中调用主程序：主程序名后跟VIN码，通过命令行参数将VIN码传递给主程序。例如：

	```dos
	EPSCaliProc.exe VINTESTCODE012345
	```

	- 其中 `VINTESTCODE012345` 为VIN码, 也可以不加VIN码参数直接启动主程序后，在左侧输入控件内输入VIN码。

6. 主程序左侧为功能区，右侧为执行结果显示区。

7. 左侧第一个输入控件为VIN码输入，若在启动时已经带有VIN码参数的话该控件内会显示命令行输入的VIN码参数。

8. 左侧第二、三个控件是标定程序执行参数。

9. 在EPS标定前若需要清除上一次标定结果的话就勾选 `清除上次EPS标定结果`

10. 默认情况下，主程序会每隔3秒循环执行EPS标定例程，直到EPS例程返回正确结束结果为止（除非EPS例程明确返回例程执行错误）。若需要控制循环次数的话，就勾选 `EPS/EPB标定出错后重试` ，下方的输入控件会变成可用状态，可输入重试次数，默认为3次。

11. 在 `开始EPS标定` 按钮之下 `清除记录` 按钮之上，是EPB标定功能，本项目暂时用不到此功能。

12. 在标定过程中产生的相关结果会存入数据库内，需保证 `Config.xml/DB` 配置内容的正确。