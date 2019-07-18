USE JAC
GO
-- EPS标定结果信息表
IF OBJECT_ID(N'JAC.dbo.CaliProcResult') IS NOT NULL
    DROP TABLE JAC.dbo.CaliProcResult
GO
CREATE TABLE JAC.dbo.CaliProcResult
(
	ID int IDENTITY PRIMARY KEY NOT NULL, -- ID, 自增, 主键
    VIN varchar(17) NOT NULL, -- VIN码
    CaliDate DATE NULL, -- 标定日期
	CaliTime TIME NULL, -- 标定时间
    ECU VARCHAR(6) NULL, -- ECU类型
    Result VARCHAR(1) NULL, -- 标定总结果
    NRCOrResult VARCHAR(5) NULL, -- NRC或错误代码
    DTC VARCHAR(120) NULL, -- DTC错误码
)
GO

-- 插入字段备注
EXEC sp_addextendedproperty N'MS_Description', N'ID', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'ID'
EXEC sp_addextendedproperty N'MS_Description', N'VIN码', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'VIN'
EXEC sp_addextendedproperty N'MS_Description', N'标定日期', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'CaliDate'
EXEC sp_addextendedproperty N'MS_Description', N'标定时间', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'CaliTime'
EXEC sp_addextendedproperty N'MS_Description', N'ECU类型', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'ECU'
EXEC sp_addextendedproperty N'MS_Description', N'标定总结果', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'Result'
EXEC sp_addextendedproperty N'MS_Description', N'NRC或错误代码', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'NRCOrResult'
EXEC sp_addextendedproperty N'MS_Description', N'DTC错误码', N'USER', N'dbo', N'TABLE', N'CaliProcResult', N'COLUMN', N'DTC'
GO

-- 测试: 插入数据
INSERT JAC.dbo.CaliProcResult
    VALUES ('testvincode012345', '2018-06-07', '10:41:08', '7S_EPS', 'X', 'NRC82', '511F00,01')
GO
-- 测试: 修改数据
UPDATE JAC.dbo.CaliProcResult
    SET Result = 'O'
    WHERE VIN = 'testvincode012345'
GO
-- 测试: 查询数据
SELECT *
    FROM JAC.dbo.CaliProcResult
GO


-- 标定状态表
IF OBJECT_ID(N'JAC.dbo.CaliProcStatus') IS NOT NULL
    DROP TABLE JAC.dbo.CaliProcStatus
GO
CREATE TABLE JAC.dbo.CaliProcStatus
(
	ID int IDENTITY PRIMARY KEY NOT NULL, -- ID, 自增, 主键
    VIN varchar(17) NOT NULL, -- VIN码
    Status int NOT NULL default(0), -- 标定状态，0：不需要标定，1：未开始标定，2：开始标定，3：结束标定
)
GO

-- 插入字段备注
EXEC sp_addextendedproperty N'MS_Description', N'ID', N'USER', N'dbo', N'TABLE', N'CaliProcStatus', N'COLUMN', N'ID'
EXEC sp_addextendedproperty N'MS_Description', N'VIN码', N'USER', N'dbo', N'TABLE', N'CaliProcStatus', N'COLUMN', N'VIN'
EXEC sp_addextendedproperty N'MS_Description', N'标定状态', N'USER', N'dbo', N'TABLE', N'CaliProcStatus', N'COLUMN', N'Status'
GO

-- 测试: 插入数据
INSERT JAC.dbo.CaliProcStatus
    VALUES ('testvincode012345', 2)
GO
-- 测试: 修改数据
UPDATE JAC.dbo.CaliProcStatus
    SET Status = 1
    WHERE VIN = 'testvincode012345'
GO
-- 测试: 查询数据
SELECT *
    FROM JAC.dbo.CaliProcStatus
GO
