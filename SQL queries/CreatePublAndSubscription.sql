DECLARE @repl_db sysname;
SET @repl_db = N'triovist';--N'triovist';

DECLARE @publ_name nvarchar(30);
SET @publ_name = 'triovist_table_repl_microservices';--'triovist_table_repl_microservices';

DECLARE @subscr_db nvarchar(30);
SET @subscr_db = 'triovist_repl';--'triovist_repl';

DECLARE @subscr_server nvarchar(30);
SET @subscr_server = 'SERVER-SQLNETMS';

DECLARE @login nvarchar(30);
SET @login = 'exampleDomain\exampleLogin';
DECLARE @pass nvarchar(30);
SET @pass = 'examplePass';


use triovist

/*Checking and creating primary keys*/
IF OBJECT_ID('dbo.[PK__Reference23294_VT26527]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Reference23294_VT26527 ADD CONSTRAINT
	PK__Reference23294_VT26527 PRIMARY KEY NONCLUSTERED 
	(
	_Reference23294_IDRRef,
	_KeyField
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo._Reference23294_VT26527 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg24088]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._InfoRg24088 ADD CONSTRAINT
	PK__InfoRg24088 PRIMARY KEY CLUSTERED 
	(
	_Period
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRg24088 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg27183]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE [dbo].[_InfoRg27183] ADD  CONSTRAINT [PK__InfoRg27183] PRIMARY KEY CLUSTERED 
(
	[_Period] ASC,
	[_Fld27184RRef] ASC
) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRg27183 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg28348]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE [dbo].[_InfoRg28348] ADD  CONSTRAINT [PK__InfoRg28348] PRIMARY KEY CLUSTERED 
(
	[_Period] ASC,
	[_Fld28349RRef] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
ALTER TABLE dbo._InfoRg28348 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Const21579]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Const21579 ADD CONSTRAINT
	PK__Const21579 PRIMARY KEY NONCLUSTERED 
	(
	_RecordKey
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Const21579 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Const21336]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Const21336 ADD CONSTRAINT
	PK__Const21336 PRIMARY KEY NONCLUSTERED 
	(
	_RecordKey
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Const21336 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Const21338]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Const21338 ADD CONSTRAINT
	PK__Const21338 PRIMARY KEY NONCLUSTERED 
	(
	_RecordKey
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Const21338 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Const21165]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Const21165 ADD CONSTRAINT
	PK__Const21165 PRIMARY KEY NONCLUSTERED 
	(
	_RecordKey
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo._Const21165 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Const21167]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Const21167 ADD CONSTRAINT
	PK__Const21167 PRIMARY KEY NONCLUSTERED 
	(
	_RecordKey
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Const21167 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg22353]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._InfoRg22353 ADD CONSTRAINT
	PK__InfoRg22353 PRIMARY KEY NONCLUSTERED 
	(
	_Fld22354
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRg22353 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__AccumRg25104]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._AccumRg25104 ADD CONSTRAINT
	PK__AccumRg25104 PRIMARY KEY NONCLUSTERED 
	(
	_Period,
	_RecorderTRef,
	_RecorderRRef,
	_LineNo,
	_Active,
	_RecordKind,
	_Fld25105RRef
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._AccumRg25104 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__AccumRg25110]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._AccumRg25110 ADD CONSTRAINT
	PK__AccumRg25110 PRIMARY KEY NONCLUSTERED 
	(
	_Period,
	_RecorderTRef,
	_RecorderRRef,
	_LineNo,
	_Active,
	_RecordKind,
	_Fld25111RRef,
	_Fld25112RRef,
	_Fld25202,
	_Fld25203
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._AccumRg25110 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg23830]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._InfoRg23830 ADD CONSTRAINT
	PK__InfoRg23830 PRIMARY KEY NONCLUSTERED 
	(
	_Fld23831RRef,
	_Fld23832,
	_Fld23833RRef,
	_Fld23834
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRg23830 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__AccumRg21407]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._AccumRg21407 ADD CONSTRAINT
	PK__AccumRg21407 PRIMARY KEY NONCLUSTERED 
	(
	_Period,
	_RecorderTRef,
	_RecorderRRef,
	_LineNo,
	_Active,
	_RecordKind,
	_Fld21408RRef,
	_Fld21410_TYPE,
	_Fld21410_RTRef,
	_Fld21410_RRRef,
	_Fld23568RRef,
	_Fld21424
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._AccumRg21407 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRgSL26678]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._InfoRgSL26678 ADD CONSTRAINT
	PK__InfoRgSL26678 PRIMARY KEY NONCLUSTERED 
	(
	_Period,
	_Fld14558RRef
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRgSL26678 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Reference23294_VT23309]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Reference23294_VT23309 ADD CONSTRAINT
	PK__Reference23294_VT23309 PRIMARY KEY NONCLUSTERED 
	(
	_Reference23294_IDRRef,
	_KeyField
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Reference23294_VT23309 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Reference114_VT23370]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Reference114_VT23370 ADD CONSTRAINT
	PK__Reference114_VT23370 PRIMARY KEY NONCLUSTERED 
	(
	_Reference114_IDRRef,
	_KeyField
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Reference114_VT23370 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__AccumRgT21444]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._AccumRgT21444 ADD CONSTRAINT
	PK__AccumRgT21444 PRIMARY KEY NONCLUSTERED 
	(
	_Period,
	_Fld21408RRef,
	_Fld21410_TYPE,
	_Fld21410_RTRef,
	_Fld21410_RRRef,
	_Fld23568RRef,
	_Fld21424,
	_Splitter
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._AccumRgT21444 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Reference23612_VT27054]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Reference23612_VT27054 ADD CONSTRAINT
	PK__Reference23612_VT27054 PRIMARY KEY NONCLUSTERED 
	(
	_Reference23612_IDRRef,
	_KeyField,
	_LineNo27055
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Reference23612_VT27054 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg23320]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._InfoRg23320 ADD CONSTRAINT
	PK__InfoRg23320 PRIMARY KEY NONCLUSTERED 
	(
	_Fld23321,
	_Fld23322RRef,
	_Fld23383RRef,
	_Fld23323RRef,
	_Fld23324RRef,
	_Fld23325RRef,
	_Fld25515,
	_Fld23665RRef
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRg23320 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__InfoRg21711]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._InfoRg21711 ADD CONSTRAINT
	PK__InfoRg21711 PRIMARY KEY NONCLUSTERED 
	(
	_Period,
	_Fld21712,
	_Fld21713,
	_Fld21714,
	_Fld21715,
	_Fld25549
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._InfoRg21711 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Reference23612_VT23613]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Reference23612_VT23613 ADD CONSTRAINT
	PK__Reference23612_VT23613 PRIMARY KEY NONCLUSTERED 
	(
	_Reference23612_IDRRef,
	_KeyField,
	_LineNo23614
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Reference23612_VT23613 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Reference114_VT25126]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Reference114_VT25126 ADD CONSTRAINT
	PK__Reference114_VT25126 PRIMARY KEY NONCLUSTERED 
	(
	_Reference114_IDRRef,
	_KeyField
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Reference114_VT25126 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

IF OBJECT_ID('dbo.[PK__Document317_VT8273]','PK') IS NULL 
Begin
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
ALTER TABLE dbo._Document317_VT8273 ADD CONSTRAINT
	PK__Document317_VT8273 PRIMARY KEY NONCLUSTERED 
	(
	_Document317_IDRRef,
	_KeyField
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo._Document317_VT8273 SET (LOCK_ESCALATION = TABLE)
COMMIT
End

-- Enabling the replication database
use master
exec sp_replicationdboption @dbname = @repl_db, @optname = N'publish', @value = N'true'
--GO

exec [triovist].sys.sp_addlogreader_agent @job_login = @login, @job_password = @pass, @publisher_security_mode = 1
--GO
-- Adding the transactional publication
use [triovist]
exec sp_addpublication @publication = @publ_name, @description = N'Transactional publication of database ''triovist'' from Publisher ''SERVER-UT05''.', @sync_method = N'concurrent', @retention = 0, @allow_push = N'true', @allow_pull = N'true', @allow_anonymous = N'true', @enabled_for_internet = N'false', @snapshot_in_defaultfolder = N'true', @post_snapshot_script = N'\\SERVER-UT04\SQL_ReplData\Scripts\CustomIndexesFoReplication.sql', @compress_snapshot = N'false', @ftp_port = 21, @ftp_login = N'anonymous', @allow_subscription_copy = N'false', @add_to_active_directory = N'false', @repl_freq = N'continuous', @status = N'active', @independent_agent = N'true', @immediate_sync = N'true', @allow_sync_tran = N'false', @autogen_sync_procs = N'false', @allow_queued_tran = N'false', @allow_dts = N'false', @replicate_ddl = 0, @allow_initialize_from_backup = N'false', @enabled_for_p2p = N'false', @enabled_for_het_sub = N'false'
--GO


exec sp_addpublication_snapshot @publication = @publ_name, @frequency_type = 4, @frequency_interval = 1, @frequency_relative_interval = 1, @frequency_recurrence_factor = 0, @frequency_subday = 1, @frequency_subday_interval = 1, @active_start_time_of_day = 040000, @active_end_time_of_day = 235959, @active_start_date = 0, @active_end_date = 0, @job_login = @login, @job_password = @pass, @publisher_security_mode = 1
exec sp_grant_publication_access @publication = @publ_name, @login = N'sa'
--GO
exec sp_grant_publication_access @publication = @publ_name, @login = N'exampleDomain\exampleLogin'
--GO
exec sp_grant_publication_access @publication = @publ_name, @login = N'21VEK\a.borodavko'
--GO
--exec sp_grant_publication_access @publication = @publ_name, @login = N'21VEK\Матвейчик Юрий'
--GO
--exec sp_grant_publication_access @publication = @publ_name, @login = N'21VEK\i.bastrikin'
----GO
--exec sp_grant_publication_access @publication = @publ_name, @login = N'21VEK\yu.zherdeckij'
----GO
--exec sp_grant_publication_access @publication = @publ_name, @login = N'NT SERVICE\Winmgmt'
----GO
--exec sp_grant_publication_access @publication = @publ_name, @login = N'NT SERVICE\SQLWriter'
--GO
exec sp_grant_publication_access @publication = @publ_name, @login = N'NT SERVICE\SQLSERVERAGENT'
--GO
exec sp_grant_publication_access @publication = @publ_name, @login = N'NT Service\MSSQLSERVER'
--GO
exec sp_grant_publication_access @publication = @publ_name, @login = N'distributor_admin'
--GO

-- Adding the transactional articles
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_AccumRg21407', @source_owner = N'dbo', @source_object = N'_AccumRg21407', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_AccumRg21407', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_AccumRg21407]', @del_cmd = N'CALL [sp_MSdel_dbo_AccumRg21407]', @upd_cmd = N'SCALL [sp_MSupd_dbo_AccumRg21407]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_AccumRg25104', @source_owner = N'dbo', @source_object = N'_AccumRg25104', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_AccumRg25104', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_AccumRg25104]', @del_cmd = N'CALL [sp_MSdel_dbo_AccumRg25104]', @upd_cmd = N'SCALL [sp_MSupd_dbo_AccumRg25104]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_AccumRg25110', @source_owner = N'dbo', @source_object = N'_AccumRg25110', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_AccumRg25110', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_AccumRg25110]', @del_cmd = N'CALL [sp_MSdel_dbo_AccumRg25110]', @upd_cmd = N'SCALL [sp_MSupd_dbo_AccumRg25110]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_AccumRgT21444', @source_owner = N'dbo', @source_object = N'_AccumRgT21444', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_AccumRgT21444', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_AccumRgT21444]', @del_cmd = N'CALL [sp_MSdel_dbo_AccumRgT21444]', @upd_cmd = N'SCALL [sp_MSupd_dbo_AccumRgT21444]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Const21165', @source_owner = N'dbo', @source_object = N'_Const21165', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Const21165', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Const21165]', @del_cmd = N'CALL [sp_MSdel_dbo_Const21165]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Const21165]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Const21167', @source_owner = N'dbo', @source_object = N'_Const21167', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Const21167', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Const21167]', @del_cmd = N'CALL [sp_MSdel_dbo_Const21167]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Const21167]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Const21336', @source_owner = N'dbo', @source_object = N'_Const21336', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Const21336', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Const21336]', @del_cmd = N'CALL [sp_MSdel_dbo_Const21336]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Const21336]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Const21338', @source_owner = N'dbo', @source_object = N'_Const21338', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Const21338', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Const21338]', @del_cmd = N'CALL [sp_MSdel_dbo_Const21338]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Const21338]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Const21579', @source_owner = N'dbo', @source_object = N'_Const21579', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Const21579', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Const21579]', @del_cmd = N'CALL [sp_MSdel_dbo_Const21579]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Const21579]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Document317', @source_owner = N'dbo', @source_object = N'_Document317', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Document317', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Document317]', @del_cmd = N'CALL [sp_MSdel_dbo_Document317]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Document317]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Date_Time', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Number', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld8205RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld8241RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld8243RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld8244', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld8245', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld8260RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld21917RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld21650', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld25158', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317', @column = N'_Fld25159', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article filter
exec sp_articlefilter @publication = @publ_name, @article = N'_Document317', @filter_name = N'FLTR__Document317_1__3762', @filter_clause = N'[_Date_Time]  > ''4021-07-01T00:00:00''', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Document317', @view_name = N'syncobj_0x4531443345414435', @filter_clause = N'[_Date_Time]  > ''4021-07-01T00:00:00''', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Document317_VT8273', @source_owner = N'dbo', @source_object = N'_Document317_VT8273', @type = N'logbased', @description = null, @creation_script = null, @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'manual', @destination_table = N'_Document317_VT8273', @destination_owner = N'dbo', @vertical_partition = N'true', @ins_cmd = N'CALL sp_MSins_dbo_Document317_VT8273', @del_cmd = N'CALL sp_MSdel_dbo_Document317_VT8273', @upd_cmd = N'SCALL sp_MSupd_dbo_Document317_VT8273', @filter_clause = N'[_Fld25264] > ''4021-07-01T00:00:00'' OR  [_Fld25264] = ''2001-01-01T00:00:00'''

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317_VT8273', @column = N'_Document317_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317_VT8273', @column = N'_KeyField', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317_VT8273', @column = N'_Fld8276RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317_VT8273', @column = N'_Fld8280', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Document317_VT8273', @column = N'_Fld25264', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article filter
exec sp_articlefilter @publication = @publ_name, @article = N'_Document317_VT8273', @filter_name = N'FLTR__Document317_VT8273_1__3762', @filter_clause = N'[_Fld25264] > ''4021-07-01T00:00:00'' OR  [_Fld25264] = ''2001-01-01T00:00:00''', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Document317_VT8273', @view_name = N'SYNC__Document317_VT8273_1__3762', @filter_clause = N'[_Fld25264] > ''4021-07-01T00:00:00'' OR  [_Fld25264] = ''2001-01-01T00:00:00''', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg21711', @source_owner = N'dbo', @source_object = N'_InfoRg21711', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg21711', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg21711]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg21711]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg21711]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg22353', @source_owner = N'dbo', @source_object = N'_InfoRg22353', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg22353', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg22353]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg22353]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg22353]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg23320', @source_owner = N'dbo', @source_object = N'_InfoRg23320', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg23320', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg23320]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg23320]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg23320]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg23830', @source_owner = N'dbo', @source_object = N'_InfoRg23830', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg23830', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg23830]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg23830]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg23830]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg24088', @source_owner = N'dbo', @source_object = N'_InfoRg24088', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg24088', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg24088]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg24088]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg24088]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg27183', @source_owner = N'dbo', @source_object = N'_InfoRg27183', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803509F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg27183', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg27183]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg27183]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg27183]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRg28348', @source_owner = N'dbo', @source_object = N'_InfoRg28348', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803509F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRg28348', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRg28348]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRg28348]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRg28348]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_InfoRgSL26678', @source_owner = N'dbo', @source_object = N'_InfoRgSL26678', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_InfoRgSL26678', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_InfoRgSL26678]', @del_cmd = N'CALL [sp_MSdel_dbo_InfoRgSL26678]', @upd_cmd = N'SCALL [sp_MSupd_dbo_InfoRgSL26678]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference112', @source_owner = N'dbo', @source_object = N'_Reference112', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference112', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference112]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference112]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference112]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Code', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld2784RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld2785RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld2786', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld2787', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld2788', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld2789', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21147', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21212', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21213', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21214', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21215', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21216', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21217', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21218', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21228', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21229', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21230RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21231', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21315', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21901', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld21950RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld22802', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld22803', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld25155', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld25156', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference112', @column = N'_Fld25552', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference112', @view_name = N'syncobj_0x3937443737454546', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference114', @source_owner = N'dbo', @source_object = N'_Reference114', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference114', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference114]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference114]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference114]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_ParentIDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Code', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld2846RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld2847RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld20923', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld21097', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld21098', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld21249', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld23104RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld23369', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld25204', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld25205', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld25720RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference114', @column = N'_Fld25721RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference114', @view_name = N'syncobj_0x3038364234443236', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference114_VT23370', @source_owner = N'dbo', @source_object = N'_Reference114_VT23370', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference114_VT23370', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Reference114_VT23370]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference114_VT23370]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference114_VT23370]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference114_VT25126', @source_owner = N'dbo', @source_object = N'_Reference114_VT25126', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference114_VT25126', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Reference114_VT25126]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference114_VT25126]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference114_VT25126]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference149', @source_owner = N'dbo', @source_object = N'_Reference149', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference149', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference149]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference149]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference149]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_ParentIDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Folder', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Code', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3479', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3480', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3481RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3482', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3483', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3484', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3485RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3486RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3487RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3488RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3489RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3490RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3491', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3492', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3493RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3494', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3495', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3496', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3497RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3498', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3499RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3500RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3501', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3502RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3503', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3504_TYPE', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3504_RTRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3504_RRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3505', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3506', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3507', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3508', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3509RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3510RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3511RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3512', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3513RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3514RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3515RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3516RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3517RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3518RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3519RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3520RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3521RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3522', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3523RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3524RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19010RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19011_TYPE', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19011_RTRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19011_RRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19012_TYPE', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19012_RTRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19012_RRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19013RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3525', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3526RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3527', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3528RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3529RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3530', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3531', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld3532', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld17888RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19889', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19890RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld20852', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld20853', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld20929', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld20930', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld20931', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21171', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21172_TYPE', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21172_RTRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21172_RRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21173', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21174', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21175', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21558', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21559RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld21822RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld22874', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld22987RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld22988', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23105', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23156', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23668', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23753', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23767', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23804RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23819', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld19849', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld20849', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference149', @column = N'_Fld23803', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference149', @view_name = N'syncobj_0x3045373835343133', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference226', @source_owner = N'dbo', @source_object = N'_Reference226', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference226', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference226]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference226]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference226]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_ParentIDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Folder', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Code', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5273RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5274RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5275', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5276', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5277', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5278', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5279', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5280', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5281', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5282', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5283RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5284', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5285RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5286RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5287RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5288', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5289', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5290RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5291RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5292RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5293RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5294', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5295RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5296RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5297', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5298RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5299', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5300', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5301RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5302', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5303', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5304', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5305', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5306', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5307', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5308', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5309', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5310', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld5311RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld22932RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld17912', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld19544', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld19584', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld19899RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld21394RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld21565RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld22115', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23130', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23131', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23239', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23240RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23343RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23350', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23554RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23620RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23650', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld23887', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld25723', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld25724', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld25855', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld26668', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld26669', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference226', @column = N'_Fld26874', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference226', @view_name = N'syncobj_0x3931394439304235', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference23294', @source_owner = N'dbo', @source_object = N'_Reference23294', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference23294', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference23294]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference23294]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference23294]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_ParentIDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Folder', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Code', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld23299RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld23300', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld23301RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld23302RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld23747', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25130', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25131', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25132', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25133', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25134', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25135', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25136', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25137', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25138', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25139', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25140', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25141', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25142', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25143', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25164', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25478', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25519', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld25520', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld26525', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23294', @column = N'_Fld26526RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference23294', @view_name = N'syncobj_0x3233363330363936', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference23294_VT23309', @source_owner = N'dbo', @source_object = N'_Reference23294_VT23309', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference23294_VT23309', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Reference23294_VT23309]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference23294_VT23309]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference23294_VT23309]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference23294_VT26527', @source_owner = N'dbo', @source_object = N'_Reference23294_VT26527', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference23294_VT26527', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Reference23294_VT26527]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference23294_VT26527]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference23294_VT26527]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference23612', @source_owner = N'dbo', @source_object = N'_Reference23612', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference23612', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference23612]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference23612]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference23612]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Code', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Fld23641', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Fld23643', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference23612', @column = N'_Fld23674', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference23612', @view_name = N'syncobj_0x3630324631454144', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference23612_VT23613', @source_owner = N'dbo', @source_object = N'_Reference23612_VT23613', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference23612_VT23613', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Reference23612_VT23613]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference23612_VT23613]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference23612_VT23613]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference23612_VT27054', @source_owner = N'dbo', @source_object = N'_Reference23612_VT27054', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference23612_VT27054', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'false', @ins_cmd = N'CALL [sp_MSins_dbo_Reference23612_VT27054]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference23612_VT27054]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference23612_VT27054]'
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference256', @source_owner = N'dbo', @source_object = N'_Reference256', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference256', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference256]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference256]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference256]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_OwnerID_TYPE', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_OwnerID_RTRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_OwnerID_RRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_ParentIDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld5999', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6000', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6001', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6002', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6003RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6004', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6005', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6006', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6007RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6008RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6009', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6010', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6011', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6012', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6013', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6014RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6015RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld6016', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld19573', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference256', @column = N'_Fld23560', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference256', @view_name = N'syncobj_0x3244333543313030', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'_Reference99', @source_owner = N'dbo', @source_object = N'_Reference99', @type = N'logbased', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x000000000803508F, @identityrangemanagementoption = N'none', @destination_table = N'_Reference99', @destination_owner = N'dbo', @status = 24, @vertical_partition = N'true', @ins_cmd = N'CALL [sp_MSins_dbo_Reference99]', @del_cmd = N'CALL [sp_MSdel_dbo_Reference99]', @upd_cmd = N'SCALL [sp_MSupd_dbo_Reference99]'

-- Adding the article's partition column(s)
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_IDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_Version', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_Marked', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_PredefinedID', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_ParentIDRRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_Folder', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_Description', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_Fld2649', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
exec sp_articlecolumn @publication = @publ_name, @article = N'_Reference99', @column = N'_Fld21949RRef', @operation = N'add', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1

-- Adding the article synchronization object
exec sp_articleview @publication = @publ_name, @article = N'_Reference99', @view_name = N'syncobj_0x3330423441334635', @filter_clause = N'', @force_invalidate_snapshot = 1, @force_reinit_subscription = 1
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'DelCountRows', @source_owner = N'dbo', @source_object = N'DelCountRows', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'DelCountRows', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'DelCountRows_RetailPrices', @source_owner = N'dbo', @source_object = N'DelCountRows_RetailPrices', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'DelCountRows_RetailPrices', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'fn_diagramobjects', @source_owner = N'dbo', @source_object = N'fn_diagramobjects', @type = N'func schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'fn_diagramobjects', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'fn_split_string', @source_owner = N'dbo', @source_object = N'fn_split_string', @type = N'func schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'fn_split_string', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_alterdiagram', @source_owner = N'dbo', @source_object = N'sp_alterdiagram', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_alterdiagram', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_creatediagram', @source_owner = N'dbo', @source_object = N'sp_creatediagram', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_creatediagram', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_dropdiagram', @source_owner = N'dbo', @source_object = N'sp_dropdiagram', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_dropdiagram', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_getid', @source_owner = N'dbo', @source_object = N'sp_getid', @type = N'func schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_getid', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_helpdiagramdefinition', @source_owner = N'dbo', @source_object = N'sp_helpdiagramdefinition', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_helpdiagramdefinition', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_helpdiagrams', @source_owner = N'dbo', @source_object = N'sp_helpdiagrams', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_helpdiagrams', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_renamediagram', @source_owner = N'dbo', @source_object = N'sp_renamediagram', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_renamediagram', @destination_owner = N'dbo', @status = 16
--GO
use [triovist]
exec sp_addarticle @publication = @publ_name, @article = N'sp_upgraddiagrams', @source_owner = N'dbo', @source_object = N'sp_upgraddiagrams', @type = N'proc schema only', @description = N'', @creation_script = N'', @pre_creation_cmd = N'drop', @schema_option = 0x0000000008000001, @destination_table = N'sp_upgraddiagrams', @destination_owner = N'dbo', @status = 16
--GO

-- Adding the transactional subscriptions
use [triovist]
exec sp_addsubscription @publication = @publ_name, @subscriber = @subscr_server, @destination_db = @subscr_db, @subscription_type = N'Push', @sync_type = N'automatic', @article = N'all', @update_mode = N'read only', @subscriber_type = 0
exec sp_addpushsubscription_agent @publication = @publ_name, @subscriber = @subscr_server, @subscriber_db = @subscr_db, @job_login = @login, @job_password = @pass, @subscriber_security_mode = 0, @subscriber_login = N'exampleLogin', @subscriber_password = N'examplePass', @frequency_type = 64, @frequency_interval = 1, @frequency_relative_interval = 1, @frequency_recurrence_factor = 0, @frequency_subday = 4, @frequency_subday_interval = 5, @active_start_time_of_day = 0, @active_end_time_of_day = 235959, @active_start_date = 0, @active_end_date = 0, @dts_package_location = N'Distributor'
--GO


use [triovist]
exec sp_addsubscription @publication = @publ_name, @subscriber = N'SERVER-1C-SQL-1', @destination_db = N'triovist_repl', @subscription_type = N'Push', @sync_type = N'automatic', @article = N'all', @update_mode = N'read only', @subscriber_type = 0
exec sp_addpushsubscription_agent @publication = @publ_name, @subscriber = N'SERVER-1C-SQL-1', @subscriber_db = N'triovist_repl', @job_login = @login, @job_password = @pass, @subscriber_security_mode = 0, @subscriber_login = N'exampleLogin', @subscriber_password = N'examplePass', @frequency_type = 64, @frequency_interval = 1, @frequency_relative_interval = 1, @frequency_recurrence_factor = 0, @frequency_subday = 4, @frequency_subday_interval = 5, @active_start_time_of_day = 0, @active_end_time_of_day = 235959, @active_start_date = 0, @active_end_date = 0, @dts_package_location = N'Distributor'
GO
