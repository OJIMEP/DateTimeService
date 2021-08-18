USE [triovist_repl]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/* Создаем таблицы аггрегации */
IF  EXISTS (SELECT *
FROM sys.objects
WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryPowerAggregate]') AND type in (N'U'))
DROP TABLE [dbo].[DeliveryPowerAggregate]
GO

CREATE TABLE [dbo].[DeliveryPowerAggregate]
(
    [Период] [datetime] NOT NULL,
    [ЗонаДоставки] [binary](16) NOT NULL,
    [МассаОборот] [numeric](10, 3) NOT NULL,
    [ОбъемОборот] [numeric](10, 3) NOT NULL,
    [ВремяНаОбслуживаниеОборот] [numeric](10, 3) NOT NULL,
    CONSTRAINT [PK_DeliveryPowerAggregate] PRIMARY KEY CLUSTERED 
(
	[Период] ASC,
	[ЗонаДоставки] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT *
FROM sys.objects
WHERE object_id = OBJECT_ID(N'[dbo].[IntervalsAggregate]') AND type in (N'U'))
DROP TABLE [dbo].[IntervalsAggregate]
GO

CREATE TABLE [dbo].[IntervalsAggregate]
(
    [Период] [datetime] NOT NULL,
    [ГруппаПланирования] [binary](16) NOT NULL,
    [Геозона] [binary](16) NOT NULL,
    [ВремяНачала] [datetime] NOT NULL,
    [ВремяОкончания] [datetime] NOT NULL,
    [КоличествоЗаказовЗаИнтервалВремени] [numeric](10, 0) NOT NULL,
    CONSTRAINT [PK_IntervalsAggregate] PRIMARY KEY CLUSTERED 
(
	[Период] ASC,
	[ГруппаПланирования] ASC,
	[Геозона] ASC,
	[ВремяНачала] ASC,
	[ВремяОкончания] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/* Создаем буферные таблицы */
IF  EXISTS (SELECT *
FROM sys.objects
WHERE object_id = OBJECT_ID(N'[dbo].[buffering_table_deliverypower]') AND type in (N'U'))
DROP TABLE [dbo].[buffering_table_deliverypower]
GO

CREATE TABLE [dbo].[buffering_table_deliverypower]
(
    [id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
    [Период] [date] NOT NULL,
    [ЗонаДоставки] [binary](16) NOT NULL,
    [МассаОборот] [numeric](10, 3) NOT NULL,
    [ОбъемОборот] [numeric](10, 3) NOT NULL,
    [ВремяНаОбслуживаниеОборот] [numeric](10, 3) NOT NULL,
    PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT *
FROM sys.objects
WHERE object_id = OBJECT_ID(N'[dbo].[buffering_table_intervals]') AND type in (N'U'))
DROP TABLE [dbo].[buffering_table_intervals]
GO

CREATE TABLE [dbo].[buffering_table_intervals]
(
    [id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
    [Период] [datetime] NOT NULL,
    [ГруппаПланирования] [binary](16) NOT NULL,
    [Геозона] [binary](16) NOT NULL,
    [ВремяНачала] [datetime] NOT NULL,
    [ВремяОкончания] [datetime] NOT NULL,
    [КоличествоЗаказовЗаИнтервалВремени] [numeric](10, 0) NOT NULL,
    PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO




/* Создаем хранимки обновления аггрегаций */
CREATE OR ALTER   procedure [dbo].[spUpdateAggregateDeliveryPower]
as

begin
    set nocount on;
    set xact_abort on;

    create table #t
    (
        [Период] [date] ,
        [ЗонаДоставки] [binary](16) ,
        [МассаОборот] [numeric](10, 3) ,
        [ОбъемОборот] [numeric](10, 3) ,
        [ВремяНаОбслуживаниеОборот] [numeric](10, 3)
    );

    if @@trancount > 0
  begin
        raiserror('Outer transaction detected', 16, 1);
        return;
    end;

    begin tran;

    declare @result int;
    exec @result = sys.sp_getapplock @Resource = N'[dbo].[buffering_table_deliverypower]', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 100;
    if @result < 0
	THROW 51000, 'Cant get lock for delete', 1;

    delete from [dbo].[buffering_table_deliverypower] 
	output deleted.Период,deleted.ЗонаДоставки,deleted.МассаОборот,deleted.ОбъемОборот,deleted.ВремяНаОбслуживаниеОборот into #t;

    with
        s (Период, ЗонаДоставки, МассаОборот_delta, ОбъемОборот_delta, ВремяНаОбслуживаниеОборот_delta)
        as
        (
            select
                Период, ЗонаДоставки, sum(МассаОборот), sum(ОбъемОборот), sum(ВремяНаОбслуживаниеОборот)
            from
                #t
            Group by
	Период,
	ЗонаДоставки
        )
 merge into [dbo].[DeliveryPowerAggregate] t
 using s on s.Период = t.Период and s.ЗонаДоставки = t.ЗонаДоставки
 when not matched then insert (Период,ЗонаДоставки,МассаОборот,ОбъемОборот,ВремяНаОбслуживаниеОборот) 
	values (s.Период,s.ЗонаДоставки, s.МассаОборот_delta, s.ОбъемОборот_delta,s.ВремяНаОбслуживаниеОборот_delta)
 when matched then update set МассаОборот += s.МассаОборот_delta, ОбъемОборот += s.ОбъемОборот_delta, ВремяНаОбслуживаниеОборот += s.ВремяНаОбслуживаниеОборот_delta;

    commit;
end;
GO

CREATE OR ALTER   procedure [dbo].[spUpdateAggregateIntervals]
as
begin
    set nocount on;
    set xact_abort on;

    create table #t
    (
        [Период] [datetime] NOT NULL,
	    [ГруппаПланирования] [binary](16) NOT NULL,
	    [Геозона] [binary](16) NOT NULL,
	    [ВремяНачала] [datetime] NOT NULL,
	    [ВремяОкончания] [datetime] NOT NULL,
	    [КоличествоЗаказовЗаИнтервалВремени] [numeric](10, 0) NOT NULL
    );

    if @@trancount > 0
  begin
        raiserror('Outer transaction detected', 16, 1);
        return;
    end;

    begin tran;

    declare @result int;
    exec @result = sys.sp_getapplock @Resource = N'[dbo].[buffering_table_intervals]', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 100;
    if @result < 0
	THROW 51000, 'Cant get lock for delete', 1;

    delete from [dbo].[buffering_table_intervals] 
	output deleted.Период,deleted.ГруппаПланирования,deleted.Геозона,deleted.ВремяНачала,deleted.ВремяОкончания, deleted.КоличествоЗаказовЗаИнтервалВремени into #t;

    with
        s (Период, ГруппаПланирования, Геозона, ВремяНачала, ВремяОкончания, КоличествоЗаказовЗаИнтервалВремени_delta )
        as
        (
            select
                Период, ГруппаПланирования, Геозона, ВремяНачала, ВремяОкончания, sum(КоличествоЗаказовЗаИнтервалВремени)
            from
                #t
            Group by
                Период, ГруппаПланирования, Геозона, ВремяНачала, ВремяОкончания
        )
 merge into [dbo].[IntervalsAggregate] t
 using s on s.Период = t.Период 
    and s.ГруппаПланирования = t.ГруппаПланирования
    and s.Геозона = t.Геозона
    and s.ВремяНачала = t.ВремяНачала
    and s.ВремяОкончания = t.ВремяОкончания
 when not matched then insert (Период, ГруппаПланирования, Геозона, ВремяНачала, ВремяОкончания, КоличествоЗаказовЗаИнтервалВремени) 
	values (s.Период,s.ГруппаПланирования, s.Геозона, s.ВремяНачала,s.ВремяОкончания, s.КоличествоЗаказовЗаИнтервалВремени_delta)
 when matched then update set КоличествоЗаказовЗаИнтервалВремени += s.КоличествоЗаказовЗаИнтервалВремени_delta;

    commit;
end;
GO


/*Создаем триггеры */
CREATE OR ALTER trigger [dbo].[_AccumRg25104_aggregate_trigger]
on [dbo].[_AccumRg25104]
after insert, update, delete
as
begin
 set nocount on;

 declare @result int;
exec @result = sys.sp_getapplock @Resource = N'[dbo].[buffering_table_deliverypower]', @LockMode = 'Shared', @LockOwner = 'Transaction', @LockTimeout = -1;
if @result < 0
	THROW 51000, 'Cant get lock for insert', 1; 

 insert into [dbo].[buffering_table_deliverypower] (Период,ЗонаДоставки,МассаОборот,ОбъемОборот,ВремяНаОбслуживаниеОборот) 
 select   
       CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME) AS Период, 
	   МощностиДоставки._Fld25105RRef As ЗонаДоставки,
		SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25107
                ELSE -(МощностиДоставки._Fld25107)
        END        
    ) AS МассаОборот,    
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25108
                ELSE -(МощностиДоставки._Fld25108)
        END        
    ) AS ОбъемОборот,    
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25201
                ELSE -(МощностиДоставки._Fld25201)
        END        
    ) AS ВремяНаОбслуживаниеОборот	 
FROM
    inserted As МощностиДоставки With (READCOMMITTED)
GROUP BY
    CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME),
	МощностиДоставки._Fld25105RRef 
;

 insert into [dbo].[buffering_table_deliverypower] (Период,ЗонаДоставки,МассаОборот,ОбъемОборот,ВремяНаОбслуживаниеОборот) 
 select   
       CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME) AS Период, 
	   МощностиДоставки._Fld25105RRef As ЗонаДоставки,
		-1*SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25107
                ELSE -(МощностиДоставки._Fld25107)
        END        
    ) AS МассаОборот,    
        -1*SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25108
                ELSE -(МощностиДоставки._Fld25108)
        END        
    ) AS ОбъемОборот,    
        -1*SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25201
                ELSE -(МощностиДоставки._Fld25201)
        END        
    ) AS ВремяНаОбслуживаниеОборот	 
FROM
    deleted As МощностиДоставки With (READCOMMITTED)
GROUP BY
    CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME),
	МощностиДоставки._Fld25105RRef
;

end;
GO


CREATE OR ALTER trigger [dbo].[_AccumRg25110_aggregate_trigger]
on [dbo].[_AccumRg25110]
after insert, update, delete
as
begin
 set nocount on;

 declare @result int;
exec @result = sys.sp_getapplock @Resource = N'[dbo].[buffering_table_intervals]', @LockMode = 'Shared', @LockOwner = 'Transaction', @LockTimeout = -1;
if @result < 0
	THROW 51000, 'Cant get lock for insert', 1; 

 insert into [dbo].[buffering_table_intervals] (Период, ГруппаПланирования, Геозона, ВремяНачала, ВремяОкончания, КоличествоЗаказовЗаИнтервалВремени) 
 select   
    T5._Period AS Период,
    T5._Fld25112RRef As ГруппаПланирования, 
	T5._Fld25111RRef As Геозона,
	T5._Fld25202 As ВремяНачала,
	T5._Fld25203 As ВремяОкончания,
	SUM(
                CASE
                    WHEN (T5._RecordKind = 0.0) THEN T5._Fld25113
                    ELSE -(T5._Fld25113)
                END
            ) AS КоличествоЗаказовЗаИнтервалВремени	 
FROM
    inserted As T5 With (READCOMMITTED)
GROUP BY
   T5._Period,
    T5._Fld25112RRef,
    T5._Fld25111RRef,
    T5._Fld25202,
	T5._Fld25203
;

 insert into [dbo].[buffering_table_intervals] (Период, ГруппаПланирования, Геозона, ВремяНачала, ВремяОкончания, КоличествоЗаказовЗаИнтервалВремени) 
 select   
    T5._Period AS Период,
    T5._Fld25112RRef As ГруппаПланирования, 
	T5._Fld25111RRef As Геозона,
	T5._Fld25202 As ВремяНачала,
	T5._Fld25203 As ВремяОкончания,
	-1*SUM(
                CASE
                    WHEN (T5._RecordKind = 0.0) THEN T5._Fld25113
                    ELSE -(T5._Fld25113)
                END
            ) AS КоличествоЗаказовЗаИнтервалВремени	 
FROM
    deleted As T5 With (READCOMMITTED)
GROUP BY
    T5._Period,
    T5._Fld25112RRef,
    T5._Fld25111RRef,
    T5._Fld25202,
	T5._Fld25203
;

end;
GO

/*Инициализируем таблицы аггрегации, очистив их и буферные таблицы перед этим*/
begin tran
alter table [dbo].[_AccumRg25104] disable trigger [_AccumRg25104_aggregate_trigger]
alter table [dbo].[_AccumRg25110] disable trigger [_AccumRg25110_aggregate_trigger]

delete  from [dbo].[buffering_table_deliverypower] with (tablock)
delete  from [dbo].[buffering_table_intervals] with (tablock)

delete  from [dbo].[IntervalsAggregate] with (tablock)

Insert into [dbo].[DeliveryPowerAggregate]
SELECT   
       CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME) AS Период, 
	   МощностиДоставки._Fld25105RRef As ЗонаДоставки,
		SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25107
                ELSE -(МощностиДоставки._Fld25107)
        END        
    ) AS МассаОборот,    
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25108
                ELSE -(МощностиДоставки._Fld25108)
        END        
    ) AS ОбъемОборот,    
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25201
                ELSE -(МощностиДоставки._Fld25201)
        END        
    ) AS ВремяНаОбслуживаниеОборот
    
	 
FROM
    dbo._AccumRg25104 МощностиДоставки With (READCOMMITTED)
GROUP BY
    CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME),
	МощностиДоставки._Fld25105RRef

Insert into [dbo].[IntervalsAggregate]
SELECT
    T5._Period AS Период,
    T5._Fld25112RRef As ГруппаПланирования, 
	T5._Fld25111RRef As Геозона,
	T5._Fld25202 As ВремяНачала,
	T5._Fld25203 As ВремяОкончания,
	SUM(
                CASE
                    WHEN (T5._RecordKind = 0.0) THEN T5._Fld25113
                    ELSE -(T5._Fld25113)
                END
            ) AS КоличествоЗаказовЗаИнтервалВремени

FROM
    dbo._AccumRg25110 T5 With (READCOMMITTED)
GROUP BY
    T5._Period,
    T5._Fld25112RRef,
    T5._Fld25111RRef,
    T5._Fld25202,
	T5._Fld25203

alter table [dbo].[_AccumRg25104] enable trigger [_AccumRg25104_aggregate_trigger]
alter table [dbo].[_AccumRg25110] enable trigger [_AccumRg25110_aggregate_trigger]
commit

/* Создадим джоб для обновления аггрегаций*/
USE [msdb]
GO

/****** Object:  Job [UpdateAggregates]    Script Date: 18.08.2021 16:31:51 ******/
EXEC msdb.dbo.sp_delete_job @job_id=N'7403ec1f-4359-46ea-81e2-e4a99fdf415c', @delete_unused_schedule=1
GO

/****** Object:  Job [UpdateAggregates]    Script Date: 18.08.2021 16:31:51 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [Data Collector]    Script Date: 18.08.2021 16:31:52 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'Data Collector' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Data Collector'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'UpdateAggregates', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Описание недоступно.', 
		@category_name=N'Data Collector', 
		@owner_login_name=N'21VEK\a.borodavko', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [UpdateDeliveryPower]    Script Date: 18.08.2021 16:31:52 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'UpdateDeliveryPower', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'while 1 = 1
begin
exec spUpdateAggregateDeliveryPower
exec spUpdateAggregateIntervals
waitfor delay ''00:00:01''
end
', 
		@database_name=N'triovist_repl', 
		@flags=4
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'autostart schedule', 
		@enabled=1, 
		@freq_type=64, 
		@freq_interval=0, 
		@freq_subday_type=0, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20210818, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'4f5faf5d-a1cc-4bcc-b473-f513546274a0'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'recurring schedule', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=2, 
		@freq_subday_interval=10, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20210818, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'c2fa3a84-6176-4c19-88b8-febd8ef63a2a'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


