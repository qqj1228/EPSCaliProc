USE JAC
GO
-- EPS标定结果信息表
IF OBJECT_ID(N'JAC.dbo.EPSCaliProc') IS NOT NULL
    DROP TABLE JAC.dbo.EPSCaliProc
GO
CREATE TABLE JAC.dbo.EPSCaliProc
(
	ID int IDENTITY PRIMARY KEY NOT NULL, -- ID, 自增, 主键
    VIN varchar(17) NOT NULL UNIQUE, -- VIN码
    CaliDate DATE NULL, -- 标定日期
	CaliTime TIME NULL, -- 标定时间
    Result VARCHAR(1) NULL, -- 标定结果
    RecvData VARCHAR(2) NULL, -- 返回数据
    DTC VARCHAR(120) NULL, -- DTC错误码
)
GO

-- 插入字段备注
EXEC sp_addextendedproperty N'MS_Description', N'ID', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'ID'
EXEC sp_addextendedproperty N'MS_Description', N'VIN码', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'VIN'
EXEC sp_addextendedproperty N'MS_Description', N'标定日期', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'CaliDate'
EXEC sp_addextendedproperty N'MS_Description', N'标定时间', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'CaliTime'
EXEC sp_addextendedproperty N'MS_Description', N'标定结果', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'Result'
EXEC sp_addextendedproperty N'MS_Description', N'返回数据', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'RecvData'
EXEC sp_addextendedproperty N'MS_Description', N'DTC错误码', N'USER', N'dbo', N'TABLE', N'EPSCaliProc', N'COLUMN', N'DTC'
GO

-- 测试: 插入数据
INSERT JAC.dbo.EPSCaliProc
    VALUES ('testvincode012345', '2018-06-07', '10:41:08', 'X', '01', '511F00,01')
GO
-- 测试: 修改数据
UPDATE JAC.dbo.EPSCaliProc
    SET RecvData = '90'
    WHERE VIN = 'testvincode012345'
GO
-- 测试: 查询数据
SELECT *
    FROM JAC.dbo.EPSCaliProc
GO

-- EPB标定结果信息表
IF OBJECT_ID(N'JAC.dbo.EPBCaliProc') IS NOT NULL
    DROP TABLE JAC.dbo.EPBCaliProc
GO
CREATE TABLE JAC.dbo.EPBCaliProc
(
	ID int IDENTITY PRIMARY KEY NOT NULL, -- ID, 自增, 主键
    VIN varchar(17) NOT NULL UNIQUE, -- VIN码
    CaliDate DATE NULL, -- 标定日期
	CaliTime TIME NULL, -- 标定时间
    Result VARCHAR(1) NULL, -- 标定总结果
    SoftwareVerResult VARCHAR(1) NULL, -- 软件版本结果
    HardwareVerResult VARCHAR(1) NULL, -- 硬件版本结果
    GSensorResult VARCHAR(1) NULL, -- G-sensor结果
    AssemblyCheckResult VARCHAR(1) NULL, -- 装配检查结果
    ManufactoryModeResult VARCHAR(1) NULL, -- 工厂模式结果
    DTC VARCHAR(120) NULL, -- DTC错误码
)
GO

-- 插入字段备注
EXEC sp_addextendedproperty N'MS_Description', N'ID', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'ID'
EXEC sp_addextendedproperty N'MS_Description', N'VIN码', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'VIN'
EXEC sp_addextendedproperty N'MS_Description', N'标定日期', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'CaliDate'
EXEC sp_addextendedproperty N'MS_Description', N'标定时间', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'CaliTime'
EXEC sp_addextendedproperty N'MS_Description', N'标定结果', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'Result'
EXEC sp_addextendedproperty N'MS_Description', N'软件版本结果', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'SoftwareVerResult'
EXEC sp_addextendedproperty N'MS_Description', N'硬件版本结果', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'HardwareVerResult'
EXEC sp_addextendedproperty N'MS_Description', N'G-sensor结果', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'GSensorResult'
EXEC sp_addextendedproperty N'MS_Description', N'装配检查结果', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'AssemblyCheckResult'
EXEC sp_addextendedproperty N'MS_Description', N'工厂模式结果', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'ManufactoryModeResult'
EXEC sp_addextendedproperty N'MS_Description', N'DTC错误码', N'USER', N'dbo', N'TABLE', N'EPBCaliProc', N'COLUMN', N'DTC'
GO

-- 测试: 插入数据
INSERT JAC.dbo.EPBCaliProc
    VALUES ('testvincode012346', '2018-06-25', '11:27:12', 'X', 'O', 'O', 'O', 'O', 'O', '')
GO
-- 测试: 修改数据
UPDATE JAC.dbo.EPBCaliProc
    SET Result = 'O'
    WHERE VIN = 'testvincode012346'
GO
-- 测试: 查询数据
SELECT *
    FROM JAC.dbo.EPBCaliProc
GO
