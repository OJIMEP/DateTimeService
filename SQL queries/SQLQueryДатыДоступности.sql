DECLARE @P_Article1 nvarchar(20);
DECLARE @P_Article2 nvarchar(20);
DECLARE @P_Article3 nvarchar(20);
DECLARE @P_Article4 nvarchar(20);
DECLARE @P_Article5 nvarchar(20);
DECLARE @P_Article6 nvarchar(20);
DECLARE @P_Article7 nvarchar(20);
DECLARE @P_Article8 nvarchar(20);

DECLARE @P_Code1 nvarchar(20);
DECLARE @P_Code2 nvarchar(20);
DECLARE @P_Code3 nvarchar(20);
DECLARE @P_Code4 nvarchar(20);
DECLARE @P_Code5 nvarchar(20);
DECLARE @P_Code6 nvarchar(20);
DECLARE @P_Code7 nvarchar(20);
DECLARE @P_Code8 nvarchar(20);

DECLARE @PickupPoint1 nvarchar(10);
DECLARE @PickupPoint2 nvarchar(10);
DECLARE @PickupPoint3 nvarchar(10);
DECLARE @PickupPoint4 nvarchar(10);
DECLARE @PickupPoint5 nvarchar(10);
DECLARE @PickupPoint6 nvarchar(10);
--DECLARE @PickupPointsAll nvarchar(50);

DECLARE @P_CityCode nvarchar(20);

DECLARE @P_DateTimeNow datetime;
DECLARE @P_DateTimePeriodBegin datetime;
DECLARE @P_DateTimePeriodEnd datetime;
DECLARE @P_TimeNow datetime;
DECLARE @P_EmptyDate datetime;
DECLARE @P_MaxDate datetime;

 SET @P_Article1 = '358649'; --артикулы
 SET @P_Article2 = '424941';
 SET @P_Article3 = '6445627';
 SET @P_Article4 = '5962720';
 SET @P_Article5 = '600017';
 SET @P_Article6 = '6167903';
 SET @P_Article7 = '380386';
 SET @P_Article8 = '358619';

 SET @P_Code1 = '00-00444697'; --коды для уценки
 SET @P_Code2 = '00-00527933';
 SET @P_Code3 = NULL;
 SET @P_Code4 = NULL;
 SET @P_Code5 = NULL;
 SET @P_Code6 = NULL;
 SET @P_Code7 = NULL;
 SET @P_Code8 = NULL;

SET @PickupPoint1 = '340';
SET @PickupPoint2 = '388';
SET @PickupPoint3 = '460';
SET @PickupPoint4 = '417';
SET @PickupPoint5 = '234';
SET @PickupPoint6 = '2';

 Set @P_CityCode = '17030' --код адреса

DECLARE @P_DaysToShow numeric(2);
Set @P_DaysToShow = 7;

DECLARE @P_ApplyShifting numeric(2);
set @P_ApplyShifting = 1;

DECLARE @P_DaysToShift numeric(2);
set @P_DaysToShift = 3;

 Set @P_DateTimeNow = '4022-01-04T22:30:00' 
 Set @P_DateTimePeriodBegin = '4022-01-04T00:00:00'
 Set @P_DateTimePeriodEnd = '4022-01-15T00:00:00'
 Set @P_TimeNow = '2001-01-01T22:30:00'
 Set @P_EmptyDate = '2001-01-01T00:00:00'
 Set @P_MaxDate = '5999-11-11T00:00:00'

Create Table #Temp_GoodsRaw   
(	
	Article nvarchar(20), 
	code nvarchar(20), 
    PickupPoint nvarchar(45),
    quantity int
)

INSERT INTO 
	#Temp_GoodsRaw ( 
		Article, code, PickupPoint, quantity 
	)
VALUES
('5781776',NULL,NULL,0),
 ('6470876',NULL,NULL,0),
 ('6653227',NULL,NULL,0),
 ('5756005',NULL,NULL,0),
 ('6910368',NULL,NULL,0),
 ('65527691',NULL,NULL,0),
 ('6274049',NULL,NULL,0),
 ('5733055',NULL,NULL,0),
 ('6259882',NULL,NULL,0),
 ('6470867',NULL,NULL,0),
 ('6473101',NULL,NULL,0),
 ('5946773',NULL,NULL,0),
 ('6167713',NULL,NULL,0),
 ('6387604',NULL,NULL,0),
 ('6519574',NULL,NULL,0),
 ('6324632',NULL,NULL,0),
 ('65527696',NULL,NULL,0),
 ('1018005',NULL,NULL,0),
 ('6489477',NULL,NULL,0),
 ('960889',NULL,NULL,0),
 ('6365327',NULL,NULL,0),
 ('6250462',NULL,NULL,0),
 ('6473097',NULL,NULL,0),
 ('6243617',NULL,NULL,0),
 ('6976685',NULL,NULL,0),
 ('65512970',NULL,NULL,0),
 ('6728766',NULL,NULL,0),
 ('7014536',NULL,NULL,0),
 ('654492',NULL,NULL,0),
 ('6377785',NULL,NULL,0),
 ('6488244',NULL,NULL,0),
 ('6425392',NULL,NULL,0),
 ('6129625',NULL,NULL,0),
 ('6497452',NULL,NULL,0),
 ('5744815',NULL,NULL,0),
 ('5764866',NULL,NULL,0),
 ('6519600',NULL,NULL,0),
 ('6473127',NULL,NULL,0),
 ('6279620',NULL,NULL,0),
 ('6488152',NULL,NULL,0),
 ('6519602',NULL,NULL,0),
 ('6945846',NULL,NULL,0),
 ('6489494',NULL,NULL,0),
 ('6279613',NULL,NULL,0),
 ('6129626',NULL,NULL,0),
 ('6488164',NULL,NULL,0),
 ('6473154',NULL,NULL,0),
 ('6079733',NULL,NULL,0),
 ('5946770',NULL,NULL,0),
 ('6489488',NULL,NULL,0),
 ('5804317',NULL,NULL,0),
 ('5744771',NULL,NULL,0),
 ('5850929',NULL,NULL,0),
 ('65601248',NULL,NULL,0),
 ('6945852',NULL,NULL,0),
 ('6488124',NULL,NULL,0),
 ('6488234',NULL,NULL,0),
 ('6180814',NULL,NULL,0),
 ('6425383',NULL,NULL,0),
 ('65567643',NULL,NULL,0),
('5781776',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6470876',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6653227',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5756005',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6910368',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('65527691',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6274049',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5733055',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6259882',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6470867',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6473101',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5946773',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6167713',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6387604',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6519574',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6324632',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('65527696',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('1018005',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6489477',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('960889',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6365327',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6250462',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6473097',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6243617',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6976685',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('65512970',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6728766',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('7014536',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('654492',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6377785',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6488244',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6425392',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6129625',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6497452',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5744815',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5764866',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6519600',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6473127',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6279620',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6488152',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6519602',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6945846',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6489494',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6279613',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6129626',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6488164',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6473154',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6079733',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5946770',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6489488',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5804317',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5744771',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('5850929',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('65601248',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6945852',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6488124',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6488234',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6180814',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('6425383',NULL,'2,234,340,388,417,460,529,548,551,574,580',0),
 ('65567643',NULL,'2,234,340,388,417,460,529,548,551,574,580',0)
	--(@P_Article1,@P_Code1,@PickupPoint3,0),
	--(@P_Article2,@P_Code2,@PickupPoint2,0),
	--(@P_Article1,@P_Code1,NULL,0),
	--(@P_Article3,@P_Code3,@PickupPoint3,0),
	--('843414',NULL,NULL,0),
	--(@P_Article5,NULL,NULL,0)--,
	--('5990264',NULL,NULL,0),
	--('586456',NULL,NULL,0),
	--('5990263',NULL,'340,388,460,417,234,2',0),
	--('586455',NULL,'340,388,460,417,2',0)--,
	--('5990263',NULL,'388',0),
	--('586455',NULL,'388',0),
	--('5990263',NULL,'460',0),
	--('586455',NULL,'460',0),
	--('5990263',NULL,'417',0),
	--('586455',NULL,'417',0),
	--('5990263',NULL,'234',0),
	--('586455',NULL,'234',0),
	--('5990263',NULL,'2',0),
	--('586455',NULL,'2',0)--,
	--(@P5,4),
	--(@P6,3),
	--(@P7,2),
	--(@P8,1)
	;

Select 
	Склады._IDRRef AS СкладСсылка,
	Склады._Fld19544 AS ERPКодСклада
Into #Temp_PickupPoints
From 
	dbo._Reference226 Склады 
Where Склады._Fld19544 in('2','234','340','388','417','460','529','548','551','574','580')
 


Select
	IsNull(_Reference114_VT23370._Fld23372RRef,Геозона._Fld23104RRef) As СкладСсылка,
	ЗоныДоставки._ParentIDRRef As ЗонаДоставкиРодительСсылка,
	Геозона._IDRRef As Геозона
Into #Temp_GeoData
From dbo._Reference114 Геозона With (NOLOCK)
	Inner Join _Reference114_VT23370 With (NOLOCK)
	on _Reference114_VT23370._Reference114_IDRRef = Геозона._IDRRef
	Inner Join _Reference99 ЗоныДоставки With (NOLOCK)
	on Геозона._Fld2847RRef = ЗоныДоставки._IDRRef
where Геозона._IDRRef IN (
	SELECT TOP 1
		T3._Fld26708RRef AS Fld26823RRef --геозона из рс векРасстоянияАВ
	FROM (SELECT
			T1._Fld25549 AS Fld25549_,
			MAX(T1._Period) AS MAXPERIOD_ 
		FROM dbo._InfoRg21711 T1 With (NOLOCK)
		WHERE T1._Fld26708RRef <> 0x00 and T1._Fld25549 = @P_CityCode
		GROUP BY T1._Fld25549) T2
	INNER JOIN dbo._InfoRg21711 T3 With (NOLOCK)
	ON T2.Fld25549_ = T3._Fld25549 AND T2.MAXPERIOD_ = T3._Period
	)
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With Temp_GoodsRawParsed AS
(
select 
	t1.Article, 
	t1.code, 
	value AS PickupPoint 
from #Temp_GoodsRaw t1
	cross apply 
		string_split(IsNull(t1.PickupPoint,'-'), ',')
)
Select 
	Номенклатура._IDRRef AS НоменклатураСсылка,
	Номенклатура._Code AS code,
	Номенклатура._Fld3480 AS article,
	Номенклатура._Fld3489RRef AS ЕдиницаИзмерения,
	Номенклатура._Fld3526RRef AS Габариты,
	#Temp_PickupPoints.СкладСсылка AS СкладПВЗСсылка,
	Упаковки._IDRRef AS УпаковкаСсылка,
	Упаковки._Fld6000 AS Вес,
	Упаковки._Fld6006 AS Объем,
	Номенклатура._Fld21822RRef as ТНВЭДСсылка,
	Номенклатура._Fld3515RRef as ТоварнаяКатегорияСсылка
INTO #Temp_GoodsBegin
From
	Temp_GoodsRawParsed T1
	Inner Join 	dbo._Reference149 Номенклатура With (NOLOCK) 
		ON T1.code is NULL and T1.Article = Номенклатура._Fld3480
	Left Join #Temp_PickupPoints  
		ON T1.PickupPoint = #Temp_PickupPoints.ERPКодСклада
    Inner Join dbo._Reference256 Упаковки With (NOLOCK)
		On 
		Упаковки._OwnerID_TYPE = 0x08  
		AND Упаковки.[_OwnerID_RTRef] = 0x00000095
		AND 
		Номенклатура._IDRRef = Упаковки._OwnerID_RRRef		
		And Упаковки._Fld6003RRef = Номенклатура._Fld3489RRef
		AND Упаковки._Marked = 0x00
union all
Select 
	Номенклатура._IDRRef,
	Номенклатура._Code,
	Номенклатура._Fld3480,
	Номенклатура._Fld3489RRef,
	Номенклатура._Fld3526RRef,
	#Temp_PickupPoints.СкладСсылка,
	Упаковки._IDRRef AS УпаковкаСсылка,
	Упаковки._Fld6000 AS Вес,
	Упаковки._Fld6006 AS Объем,
	Номенклатура._Fld21822RRef as ТНВЭДСсылка,
	Номенклатура._Fld3515RRef as ТоварнаяКатегорияСсылка
From 
	Temp_GoodsRawParsed T1
	Inner Join 	dbo._Reference149 Номенклатура With (NOLOCK) 
		ON T1.code is not NULL and T1.code = Номенклатура._Code
	Left Join #Temp_PickupPoints  
		ON T1.PickupPoint = #Temp_PickupPoints.ERPКодСклада
    Inner Join dbo._Reference256 Упаковки With (NOLOCK)
		On 
		Упаковки._OwnerID_TYPE = 0x08  
		AND Упаковки.[_OwnerID_RTRef] = 0x00000095
		AND 
		Номенклатура._IDRRef = Упаковки._OwnerID_RRRef		
		And Упаковки._Fld6003RRef = Номенклатура._Fld3489RRef
		AND Упаковки._Marked = 0x00
OPTION (KEEP PLAN, KEEPFIXED PLAN);

Select 
	Номенклатура.НоменклатураСсылка AS НоменклатураСсылка,
	Номенклатура.article AS article,
	Номенклатура.code AS code,
	Номенклатура.СкладПВЗСсылка AS СкладСсылка,
	Номенклатура.УпаковкаСсылка AS УпаковкаСсылка,--Упаковки._IDRRef AS УпаковкаСсылка,
	1 As Количество,
	Номенклатура.Вес AS Вес,--Упаковки._Fld6000 AS Вес,
	Номенклатура.Объем AS Объем,--Упаковки._Fld6006 AS Объем,
	Номенклатура.ТНВЭДСсылка as ТНВЭДСсылка,
	Номенклатура.ТоварнаяКатегорияСсылка as ТоварнаяКатегорияСсылка,
	10 AS ВремяНаОбслуживание,
	IsNull(ГруппыПланирования._IDRRef, 0x00000000000000000000000000000000) AS ГруппаПланирования,
	IsNull(ГруппыПланирования._Description, '') AS ГруппаПланированияНаименование,
	IsNull(ГруппыПланирования._Fld25519, @P_EmptyDate) AS ГруппаПланированияДобавляемоеВремя,
	IsNull(ГруппыПланирования._Fld23302RRef, 0x00000000000000000000000000000000) AS ГруппаПланированияСклад,
	1 AS Приоритет
INTO #Temp_Goods
From 
	#Temp_GoodsBegin Номенклатура
	Left Join dbo._Reference23294 ГруппыПланирования With (NOLOCK)
		Inner Join dbo._Reference23294_VT23309 With (NOLOCK)
			on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
			and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
		On 
		ГруппыПланирования._Fld23302RRef IN (Select СкладСсылка From #Temp_GeoData) --склад
		AND ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
		AND (ГруппыПланирования._Fld23301RRef = Номенклатура.Габариты OR (Номенклатура.Габариты = 0xAC2CBF86E693F63444670FFEB70264EE AND ГруппыПланирования._Fld23301RRef= 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) ) --габариты
		AND ГруппыПланирования._Marked = 0x00
        AND Номенклатура.СкладПВЗСсылка Is Null
UNION ALL
Select 
	Номенклатура.НоменклатураСсылка AS НоменклатураСсылка,
	Номенклатура.article AS article,
	Номенклатура.code AS code,
	Номенклатура.СкладПВЗСсылка AS СкладСсылка,
	Номенклатура.УпаковкаСсылка AS УпаковкаСсылка,--Упаковки._IDRRef AS УпаковкаСсылка,
	1 As Количество,
	Номенклатура.Вес AS Вес,--Упаковки._Fld6000 AS Вес,
	Номенклатура.Объем AS Объем,--Упаковки._Fld6006 AS Объем,
	Номенклатура.ТНВЭДСсылка as ТНВЭДСсылка,
	Номенклатура.ТоварнаяКатегорияСсылка as ТоварнаяКатегорияСсылка,
	10 AS ВремяНаОбслуживание,
	IsNull(ПодчиненнаяГП._IDRRef, 0x00000000000000000000000000000000) AS ГруппаПланирования,
	IsNull(ПодчиненнаяГП._Description, '') AS ГруппаПланированияНаименование,
	IsNull(ПодчиненнаяГП._Fld25519, @P_EmptyDate) AS ГруппаПланированияДобавляемоеВремя,
	IsNull(ПодчиненнаяГП._Fld23302RRef, 0x00000000000000000000000000000000) AS ГруппаПланированияСклад,
	0
From 
	#Temp_GoodsBegin Номенклатура
	Left Join dbo._Reference23294 ГруппыПланирования With (NOLOCK)
		Inner Join dbo._Reference23294_VT23309 With (NOLOCK)
			on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
			and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
		On 
		ГруппыПланирования._Fld23302RRef IN (Select СкладСсылка From #Temp_GeoData) --склад
		AND ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
		AND (ГруппыПланирования._Fld23301RRef = Номенклатура.Габариты OR (Номенклатура.Габариты = 0xAC2CBF86E693F63444670FFEB70264EE AND ГруппыПланирования._Fld23301RRef= 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) ) --габариты
		AND ГруппыПланирования._Marked = 0x00
		AND Номенклатура.СкладПВЗСсылка Is Null
	Inner Join dbo._Reference23294 ПодчиненнаяГП
			On  ГруппыПланирования._Fld26526RRef = ПодчиненнаяГП._IDRRef
Where 
	Номенклатура.СкладПВЗСсылка IS NULL
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With Temp_ExchangeRates AS (
SELECT
	T1._Period AS Период,
	T1._Fld14558RRef AS Валюта,
	T1._Fld14559 AS Курс,
	T1._Fld14560 AS Кратность
FROM _InfoRgSL26678 T1 With (NOLOCK)
	)
SELECT
    T2._Fld21408RRef AS НоменклатураСсылка,
    T2._Fld21410_TYPE AS Источник_TYPE,
	T2._Fld21410_RTRef AS Источник_RTRef,
	T2._Fld21410_RRRef AS Источник_RRRef,
	Цены._Fld21410_TYPE AS Регистратор_TYPE,
    Цены._Fld21410_RTRef AS Регистратор_RTRef,
    Цены._Fld21410_RRRef AS Регистратор_RRRef,
    T2._Fld23568RRef AS СкладИсточника,
    T2._Fld21424 AS ДатаСобытия,
    SUM(T2._Fld21411) - SUM(T2._Fld21412) AS Количество
Into #Temp_Remains
FROM
    dbo._AccumRgT21444 T2 With (READCOMMITTED)
	Left Join _AccumRg21407 Цены With (READCOMMITTED)
		Inner Join Temp_ExchangeRates With (NOLOCK)
			On Цены._Fld21443RRef = Temp_ExchangeRates.Валюта 
		On T2._Fld21408RRef = Цены._Fld21408RRef
		AND T2._Fld21410_RTRef = 0x00000153
		AND Цены._Fld21410_RTRef = 0x00000153 --Цены.Регистратор ССЫЛКА Документ.мегапрайсРегистрацияПрайса
		AND T2._Fld21410_RRRef = Цены._Fld21410_RRRef
        And Цены._Fld21442<>0 AND (Цены._Fld21442 * Temp_ExchangeRates.Курс / Temp_ExchangeRates.Кратность >= Цены._Fld21982 OR Цены._Fld21411 >= Цены._Fld21616)
		And Цены._Fld21408RRef IN(SELECT
                НоменклатураСсылка
            FROM
                #Temp_Goods)
WHERE
    T2._Period = '5999-11-01 00:00:00'
    AND (
        (
            (T2._Fld21424 = '2001-01-01 00:00:00')
            OR (Cast(T2._Fld21424 AS datetime)>= @P_DateTimeNow)
        )
        AND T2._Fld21408RRef IN (
            SELECT
                TNomen.НоменклатураСсылка
            FROM
                #Temp_Goods TNomen WITH(NOLOCK))) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0)
GROUP BY
    T2._Fld21408RRef,
    T2._Fld21410_TYPE,
    T2._Fld21410_RTRef,
    T2._Fld21410_RRRef,
	Цены._Fld21410_TYPE,
	Цены._Fld21410_RTRef,
	Цены._Fld21410_RRRef,
    T2._Fld23568RRef,
    T2._Fld21424
HAVING
    (SUM(T2._Fld21412) <> 0.0
    OR SUM(T2._Fld21411) <> 0.0)
	AND SUM(T2._Fld21411) - SUM(T2._Fld21412) > 0.0
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='4022-01-04T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);

SELECT Distinct
    T1._Fld23831RRef AS СкладИсточника,
    T1._Fld23832 AS ДатаСобытия,
    T1._Fld23834 AS ДатаПрибытия,
    T1._Fld23833RRef AS СкладНазначения
Into #Temp_WarehouseDates
FROM
    dbo._InfoRg23830 T1 With (READCOMMITTED)
	Inner Join #Temp_Remains With (NOLOCK)
	ON T1._Fld23831RRef = #Temp_Remains.СкладИсточника
	AND T1._Fld23832 = #Temp_Remains.ДатаСобытия
	AND T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData UNION ALL Select СкладСсылка From #Temp_PickupPoints)
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With SourceWarehouses AS
(
SELECT Distinct
	T2.СкладИсточника AS СкладИсточника
FROM
	#Temp_Remains T2 WITH(NOLOCK)
)
SELECT
	T1.СкладИсточника AS СкладИсточника,
	T1.СкладНазначения AS СкладНазначения,
	MIN(T1.ДатаПрибытия) AS ДатаПрибытия  
Into #Temp_MinimumWarehouseDates
FROM
    [dbo].[WarehouseDatesAggregate] T1 With (READCOMMITTED)
    Inner Join SourceWarehouses On T1.СкладИсточника = SourceWarehouses.СкладИсточника
WHERE
    T1.СкладНазначения IN (Select СкладСсылка From #Temp_GeoData UNION ALL Select СкладСсылка From #Temp_PickupPoints)
		AND	T1.ДатаСобытия BETWEEN @P_DateTimeNow AND DateAdd(DAY,6,@P_DateTimeNow)
GROUP BY T1.СкладИсточника,
T1.СкладНазначения
OPTION (OPTIMIZE FOR (@P_DateTimeNow='4022-01-04T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);


SELECT
    T1.НоменклатураСсылка,
    T1.Количество,
    T1.Источник_TYPE,
    T1.Источник_RTRef,
    T1.Источник_RRRef,
    T1.СкладИсточника,
    T1.ДатаСобытия,
    ISNULL(T3.ДатаПрибытия, T2.ДатаПрибытия) AS ДатаДоступности,
    1 AS ТипИсточника,
    ISNULL(T3.СкладНазначения, T2.СкладНазначения) AS СкладНазначения
INTO #Temp_Sources
FROM
    #Temp_Remains T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_WarehouseDates T2 WITH(NOLOCK)
    ON (T1.СкладИсточника = T2.СкладИсточника)
    AND (T1.ДатаСобытия = T2.ДатаСобытия)
    LEFT OUTER JOIN #Temp_MinimumWarehouseDates T3 WITH(NOLOCK)
    ON (T1.СкладИсточника = T3.СкладИсточника)
    AND (T1.ДатаСобытия = '2001-01-01 00:00:00')
WHERE
    T1.Источник_RTRef = 0x000000E2 OR T1.Источник_RTRef = 0x00000150

UNION
ALL
SELECT
    T4.НоменклатураСсылка,
    T4.Количество,
    T4.Источник_TYPE,
    T4.Источник_RTRef,
    T4.Источник_RRRef,
    T4.СкладИсточника,
    T4.ДатаСобытия,
    T5.ДатаПрибытия,
    2,
    T5.СкладНазначения
FROM
    #Temp_Remains T4 WITH(NOLOCK)
    INNER JOIN #Temp_WarehouseDates T5 WITH(NOLOCK)
    ON (T4.СкладИсточника = T5.СкладИсточника)
    AND (T4.ДатаСобытия = T5.ДатаСобытия)
WHERE
    T4.Источник_RTRef = 0x00000141

UNION
ALL
SELECT
    T6.НоменклатураСсылка,
    T6.Количество,
    T6.Источник_TYPE,
    T6.Источник_RTRef,
    T6.Источник_RRRef,
    T6.СкладИсточника,
    T6.ДатаСобытия,
    T7.ДатаПрибытия,
    3,
    T7.СкладНазначения
FROM
    #Temp_Remains T6 WITH(NOLOCK)
    INNER JOIN #Temp_WarehouseDates T7 WITH(NOLOCK)
    ON (T6.СкладИсточника = T7.СкладИсточника)
    AND (T6.ДатаСобытия = T7.ДатаСобытия)
WHERE
    NOT T6.Регистратор_RRRef IS NULL
	And T6.Источник_RTRef = 0x00000153
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With Temp_ExchangeRates AS (
SELECT
	T1._Period AS Период,
	T1._Fld14558RRef AS Валюта,
	T1._Fld14559 AS Курс,
	T1._Fld14560 AS Кратность
FROM _InfoRgSL26678 T1 With (NOLOCK)
)
SELECT
    T1.НоменклатураСсылка,
    T1.Источник_TYPE,
    T1.Источник_RTRef,
    T1.Источник_RRRef,
    T1.СкладИсточника,
    T1.СкладНазначения,
    T1.ДатаСобытия,
    T1.ДатаДоступности,
    CAST(
        (
            CAST(
                (Резервирование._Fld21442 * T3.Курс) AS NUMERIC(27, 8)
            ) / T3.Кратность
        ) AS NUMERIC(15, 2)
    )  AS Цена
Into #Temp_SourcesWithPrices
FROM
    #Temp_Sources T1 WITH(NOLOCK)
    INNER JOIN dbo._AccumRg21407 Резервирование WITH(READCOMMITTED)
    LEFT OUTER JOIN Temp_ExchangeRates T3 WITH(NOLOCK)
        ON (Резервирование._Fld21443RRef = T3.Валюта)
    ON 
	T1.НоменклатураСсылка = Резервирование._Fld21408RRef
    AND T1.Источник_TYPE = 0x08
		AND T1.Источник_RTRef = Резервирование._RecorderTRef
        AND T1.Источник_RRRef = Резервирование._RecorderRRef
    
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With Temp_SupplyDocs AS
(
SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.ДатаДоступности,
    DATEADD(DAY, 4.0, T1.ДатаДоступности) AS ДатаДоступностиПлюс, --это параметр КоличествоДнейАнализа
    MIN(T1.Цена) AS ЦенаИсточника,
    MIN(T1.Цена / 100.0 * (100 - 3.0)) AS ЦенаИсточникаМинус --это параметр ПроцентДнейАнализа
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
WHERE
    T1.Цена <> 0
    AND T1.Источник_RTRef = 0x00000153    
GROUP BY
    T1.НоменклатураСсылка,
    T1.ДатаДоступности,
    T1.СкладНазначения,
    DATEADD(DAY, 4.0, T1.ДатаДоступности)--это параметр КоличествоДнейАнализа
)
SELECT
    T2.НоменклатураСсылка,
    T2.ДатаДоступности,
    T2.СкладНазначения,
    T2.ДатаДоступностиПлюс,
    T2.ЦенаИсточника,
    T2.ЦенаИсточникаМинус,
    MIN(T1.ДатаДоступности) AS ДатаДоступности1,
    MIN(T1.Цена) AS Цена1
Into #Temp_BestPriceAfterClosestDate
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
    INNER JOIN Temp_SupplyDocs T2 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
    AND (T1.ДатаДоступности >= T2.ДатаДоступности)
    AND (T1.ДатаДоступности <= T2.ДатаДоступностиПлюс)
    AND (T1.Цена <= T2.ЦенаИсточникаМинус)
    AND (T1.Цена <> 0)
GROUP BY
    T2.НоменклатураСсылка,
    T2.СкладНазначения,
    T2.ДатаДоступностиПлюс,
    T2.ЦенаИсточника,
    T2.ЦенаИсточникаМинус,
    T2.ДатаДоступности
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);

With Temp_ClosestDate AS
(SELECT
T1.НоменклатураСсылка,
T1.СкладНазначения,
Cast(MIN(T1.ДатаДоступности)as datetime) AS ДатаДоступности
FROM #Temp_Sources T1 WITH(NOLOCK)
GROUP BY T1.НоменклатураСсылка,
T1.СкладНазначения
)
SELECT
            T4.НоменклатураСсылка,
            Min(T4.ДатаДоступности)AS ДатаДоступности,
            T4.СкладНазначения
		Into #Temp_T3
        FROM
            #Temp_Sources T4 WITH(NOLOCK)
            INNER JOIN Temp_ClosestDate T5 WITH(NOLOCK)
            ON (T4.НоменклатураСсылка = T5.НоменклатураСсылка)
            AND (T4.СкладНазначения = T5.СкладНазначения)
            AND (T4.ТипИсточника = 1)
			AND T4.ДатаДоступности <= DATEADD(DAY, 4, T5.ДатаДоступности)
Group by T4.НоменклатураСсылка, T4.СкладНазначения
OPTION (KEEP PLAN, KEEPFIXED PLAN);


With Temp_SourcesCorrectedDate AS
(
SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    Min(ISNULL(T2.ДатаДоступности1, T1.ДатаДоступности)) AS ДатаДоступности
FROM
    #Temp_Sources T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_BestPriceAfterClosestDate T2 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
    AND (T1.ДатаДоступности = T2.ДатаДоступности)
    AND (T1.СкладНазначения = T2.СкладНазначения)
    AND (T1.ТипИсточника = 3)
GROUP BY
	T1.НоменклатураСсылка,
	T1.СкладНазначения
)
SELECT
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
    ISNULL(T3.СкладНазначения, T2.СкладНазначения) AS СкладНазначения,
    MIN(ISNULL(T3.ДатаДоступности, T2.ДатаДоступности)) AS БлижайшаяДата,
    1 AS Количество,
    T1.Вес,
    T1.Объем,
	T1.ТНВЭДСсылка,
	T1.ТоварнаяКатегорияСсылка,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя,
    T1.Приоритет,
	0 AS PickUp
into #Temp_ClosestDatesByGoodsWithoutShifting
FROM
    #Temp_Goods T1 WITH(NOLOCK)	
    LEFT JOIN Temp_SourcesCorrectedDate T2 WITH(NOLOCK)
		LEFT JOIN  #Temp_T3 T3 ON (T2.НоменклатураСсылка = T3.НоменклатураСсылка) 
			And T2.СкладНазначения = T3.СкладНазначения
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка) 
		AND ISNULL(T3.СкладНазначения, T2.СкладНазначения) IN (Select СкладСсылка From #Temp_GeoData) 
Where 
	T1.СкладСсылка IS NULL
    And T1.ГруппаПланированияСклад = ISNULL(T3.СкладНазначения, T2.СкладНазначения)
GROUP BY
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
	ISNULL(T3.СкладНазначения, T2.СкладНазначения),
    T1.Вес,
    T1.Объем,
	T1.ТНВЭДСсылка,
	T1.ТоварнаяКатегорияСсылка,
    T1.ВремяНаОбслуживание,
    T1.Количество,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя,
	T1.Приоритет
UNION ALL
SELECT
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
    ISNULL(T3.СкладНазначения, T2.СкладНазначения) AS СкладНазначения,
    MIN(ISNULL(T3.ДатаДоступности, T2.ДатаДоступности)) AS БлижайшаяДата,
    1 AS Количество,
    T1.Вес,
    T1.Объем,
	T1.ТНВЭДСсылка,
	T1.ТоварнаяКатегорияСсылка,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя,
    T1.Приоритет,
	1 AS PickUp
FROM
    #Temp_Goods T1 WITH(NOLOCK)	
    LEFT JOIN Temp_SourcesCorrectedDate T2 WITH(NOLOCK)
		LEFT JOIN  #Temp_T3 T3 ON (T2.НоменклатураСсылка = T3.НоменклатураСсылка) 
			And T2.СкладНазначения = T3.СкладНазначения
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка) 
		AND T1.СкладСсылка = ISNULL(T3.СкладНазначения, T2.СкладНазначения)
Where 
	NOT T1.СкладСсылка IS NULL
GROUP BY
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
	ISNULL(T3.СкладНазначения, T2.СкладНазначения),
    T1.Вес,
    T1.Объем,
	T1.ТНВЭДСсылка,
	T1.ТоварнаяКатегорияСсылка,
    T1.ВремяНаОбслуживание,
    T1.Количество,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя,
	T1.Приоритет
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);

SELECT
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
    T1.СкладНазначения,
    Case when ПрослеживаемыеТоварныеКатегории._Fld28349RRef is null then T1.БлижайшаяДата else DateAdd(DAY, @P_DaysToShift, ПрослеживаемыеТоварныеКатегории._Period) end as БлижайшаяДата,
    T1.Количество,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя,
    T1.Приоритет,
	T1.PickUp
into #Temp_ClosestDatesByGoods
FROM
    #Temp_ClosestDatesByGoodsWithoutShifting T1 WITH(NOLOCK)
	left join dbo._InfoRg28348 as ПрослеживаемыеТоварныеКатегории WITH(NOLOCK)
		on 1 = @P_ApplyShifting -- это будет значение ГП ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров
			and ПрослеживаемыеТоварныеКатегории._Fld28349RRef = T1.ТоварнаяКатегорияСсылка 
			and T1.БлижайшаяДата BETWEEN ПрослеживаемыеТоварныеКатегории._Period AND DateAdd(DAY, @P_DaysToShift, ПрослеживаемыеТоварныеКатегории._Period)
OPTION (KEEP PLAN, KEEPFIXED PLAN);


SELECT
    T1.НоменклатураСсылка,
    T1.article,
    T1.code,
    T1.СкладНазначения,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
    MIN(
        CASE
            WHEN T2.Источник_RTRef = 0x00000141
            OR T2.Источник_RTRef = 0x00000153
                THEN DATEADD(SECOND, DATEDIFF(SECOND, @P_EmptyDate, T1.ГруппаПланированияДобавляемоеВремя), T1.БлижайшаяДата)
            ELSE T1.БлижайшаяДата
        END
    ) AS ДатаДоступности,
    T1.Приоритет,
	T1.PickUp
Into #Temp_ShipmentDates
FROM
    #Temp_ClosestDatesByGoods T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_Sources T2 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
    AND (T1.СкладНазначения = T2.СкладНазначения)
    AND (T1.БлижайшаяДата = T2.ДатаДоступности)
Where 
	NOT T1.БлижайшаяДата IS NULL
GROUP BY
    T1.НоменклатураСсылка,
    T1.article,
    T1.code,
    T1.СкладНазначения,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
    T1.Приоритет,
	T1.PickUp
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With MinDates AS
(
Select 
	T1.НоменклатураСсылка,
    MIN(T1.ДатаДоступности) AS ДатаСоСклада
FROM 
    #Temp_ShipmentDates T1 WITH(NOLOCK)
Where T1.PickUp = 0
Group by T1.НоменклатураСсылка
)
SELECT
    T1.НоменклатураСсылка,
    T1.article,
    T1.code,
    MinDates.ДатаСоСклада AS ДатаСоСклада,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
    T1.Приоритет,
	T1.PickUp
Into #Temp_ShipmentDatesDeliveryCourier
FROM
    #Temp_ShipmentDates T1 WITH(NOLOCK)
    Inner Join MinDates
		On T1.НоменклатураСсылка = MinDates.НоменклатураСсылка 
		And T1.ДатаДоступности = MinDates.ДатаСоСклада 
Where T1.PickUp = 0
OPTION (KEEP PLAN, KEEPFIXED PLAN);

SELECT
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
    MIN(T1.ДатаДоступности) AS ДатаСоСклада,
	T1.СкладНазначения
Into #Temp_ShipmentDatesPickUp
FROM 
    #Temp_ShipmentDates T1 WITH(NOLOCK)
Where T1.PickUp = 1
GROUP BY
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
	T1.СкладНазначения
OPTION (KEEP PLAN, KEEPFIXED PLAN);

/*Это получение списка дат интервалов ПВЗ*/
WITH
    H1(N)
    AS
    (
        SELECT 1
        FROM (VALUES
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1),
                (1))H0(N)
    )
,
    cteTALLY(N)
    AS
    (
        SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
        FROM H1 a, H1 b, H1 c, H1 d, H1 e, H1 f, H1 g, H1 h
    ),
	Temp_PickupDatesGroup AS
	(
	Select 
		CAST(CAST(DateAdd(DAY, @P_DaysToShow,Max(ДатаСоСклада))AS date) AS datetime) AS МаксимальнаяДата,
		CAST(CAST(Min(ДатаСоСклада)AS date) AS datetime) AS МинимальнаяДата
	From 
		#Temp_ShipmentDatesPickUp
    )
SELECT
	DATEADD(dd,t.N-1,f.МинимальнаяДата) AS Date
INTO #Temp_Dates
FROM Temp_PickupDatesGroup f
  CROSS APPLY (SELECT TOP (Isnull(DATEDIFF(dd,f.МинимальнаяДата,f.МаксимальнаяДата)+1,1))
        N
    FROM cteTally
    ORDER BY N) t
OPTION (KEEP PLAN, KEEPFIXED PLAN);
	;

Select 
	DATEADD(
		SECOND,
		CAST(
			DATEDIFF(SECOND, @P_EmptyDate, isNull(ПВЗИзмененияГрафикаРаботы._Fld27057, ПВЗГрафикРаботы._Fld23617)) AS NUMERIC(12)
		),
		date) AS ВремяНачала,
	DATEADD(
		SECOND,
		CAST(
			DATEDIFF(SECOND, @P_EmptyDate, isNull(ПВЗИзмененияГрафикаРаботы._Fld27058, ПВЗГрафикРаботы._Fld23618)) AS NUMERIC(12)
		),
		date) AS ВремяОкончания,
	Склады._IDRRef AS СкладНазначения--,
INTO #Temp_PickupWorkingHours
From 
	#Temp_Dates
	Inner Join dbo._Reference226 Склады 
		ON Склады._IDRRef IN (Select СкладСсылка From #Temp_PickupPoints)
	Inner Join _Reference23612 
		On Склады._Fld23620RRef = _Reference23612._IDRRef
	Left Join _Reference23612_VT23613 As ПВЗГрафикРаботы 
		On _Reference23612._IDRRef = _Reference23612_IDRRef
			AND (case when @@DATEFIRST = 1 then DATEPART ( dw , #Temp_Dates.date ) when DATEPART ( dw , #Temp_Dates.date ) = 1 then 7 else DATEPART ( dw , #Temp_Dates.date ) -1 END) = ПВЗГрафикРаботы._Fld23615
	Left Join _Reference23612_VT27054 As ПВЗИзмененияГрафикаРаботы 
		On _Reference23612._IDRRef = ПВЗИзмененияГрафикаРаботы._Reference23612_IDRRef
			AND #Temp_Dates.date = ПВЗИзмененияГрафикаРаботы._Fld27056
Where
	case 
		when ПВЗИзмененияГрафикаРаботы._Reference23612_IDRRef is not null
			then ПВЗИзмененияГрафикаРаботы._Fld27059
		when ПВЗГрафикРаботы._Reference23612_IDRRef is not Null 
			then ПВЗГрафикРаботы._Fld25265 
		else 0 --не найдено ни графика ни изменения графика  
	end = 0x00  -- не выходной
;

SELECT
	#Temp_ShipmentDatesPickUp.НоменклатураСсылка,
	#Temp_ShipmentDatesPickUp.article,
	#Temp_ShipmentDatesPickUp.code,
	Min(CASE 
	WHEN 
		#Temp_PickupWorkingHours.ВремяНачала < #Temp_ShipmentDatesPickUp.ДатаСоСклада 
		then #Temp_ShipmentDatesPickUp.ДатаСоСклада
	Else
		#Temp_PickupWorkingHours.ВремяНачала
	End) As ВремяНачала
Into #Temp_AvailablePickUp
FROM
    #Temp_ShipmentDatesPickUp
		Inner JOIN #Temp_PickupWorkingHours
		On #Temp_PickupWorkingHours.ВремяОкончания > #Temp_ShipmentDatesPickUp.ДатаСоСклада
		And #Temp_PickupWorkingHours.СкладНазначения = #Temp_ShipmentDatesPickUp.СкладНазначения
Group by
	#Temp_ShipmentDatesPickUp.НоменклатураСсылка,
	#Temp_ShipmentDatesPickUp.article,
	#Temp_ShipmentDatesPickUp.code
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);

/*если реплика alwayson или основная база то заменить на исходный вариант с суммированием*/
/*
With PlanningGroups AS(
Select Distinct 
	#Temp_ShipmentDatesDeliveryCourier.ГруппаПланирования,
	#Temp_ShipmentDatesDeliveryCourier.Приоритет
From #Temp_ShipmentDatesDeliveryCourier
)
SELECT
    T5._Period AS Период,
    T5._Fld25112RRef As ГруппаПланирования, 
	T5._Fld25111RRef As Геозона,
	T5._Fld25202 As ВремяНачалаНачальное,
	T5._Fld25203 As ВремяОкончанияНачальное,
    DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T5._Fld25202) AS NUMERIC(12)
        ),
        T5._Period
    ) As ВремяНачала,
	PlanningGroups.Приоритет
into #Temp_IntervalsAll
FROM
    dbo._AccumRg25110 T5 With (READCOMMITTED)
	Inner Join PlanningGroups ON PlanningGroups.ГруппаПланирования = T5._Fld25112RRef
WHERE
    T5._Period BETWEEN @P_DateTimePeriodBegin AND @P_DateTimePeriodEnd --begin +2
    AND T5._Fld25111RRef in (Select Геозона From #Temp_GeoData) 
GROUP BY
    T5._Period,
    T5._Fld25112RRef,
    T5._Fld25111RRef,
    T5._Fld25202,
	T5._Fld25203,
	PlanningGroups.Приоритет
HAVING
    (
        CAST(
            SUM(
                CASE
                    WHEN (T5._RecordKind = 0.0) THEN T5._Fld25113
                    ELSE -(T5._Fld25113)
                END
            ) AS NUMERIC(16, 0)
        ) > 0.0
    )
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='4022-01-04T00:00:00',@P_DateTimePeriodEnd='4022-01-15T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);
*/
With PlanningGroups AS(
Select Distinct 
	#Temp_ShipmentDatesDeliveryCourier.ГруппаПланирования,
	#Temp_ShipmentDatesDeliveryCourier.Приоритет
From #Temp_ShipmentDatesDeliveryCourier
)
SELECT
	T5.Период AS Период,
	T5.ГруппаПланирования As ГруппаПланирования, 
	T5.Геозона As Геозона,
	T5.ВремяНачала As ВремяНачалаНачальное,
	T5.ВремяОкончания As ВремяОкончанияНачальное,
	DATEADD(
		SECOND,
		CAST(
			DATEDIFF(SECOND, @P_EmptyDate, T5.ВремяНачала) AS NUMERIC(12)
		),
		T5.Период
	) As ВремяНачала,
	PlanningGroups.Приоритет
into #Temp_IntervalsAll
FROM
	[dbo].[IntervalsAggregate] T5 With (READCOMMITTED)
	Inner Join PlanningGroups ON PlanningGroups.ГруппаПланирования = T5.ГруппаПланирования
WHERE
	T5.Период BETWEEN @P_DateTimePeriodBegin AND @P_DateTimePeriodEnd --begin +2
	AND T5.Геозона in (Select Геозона From #Temp_GeoData) 
	AND T5.КоличествоЗаказовЗаИнтервалВремени > 0
GROUP BY
	T5.Период,
	T5.ГруппаПланирования,
	T5.Геозона,
	T5.ВремяНачала,
	T5.ВремяОкончания,
	PlanningGroups.Приоритет
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-09-21T00:00:00',@P_DateTimePeriodEnd='4021-09-27T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);


select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) As ВремяНачала,
#Temp_IntervalsAll.Период,
#Temp_IntervalsAll.ГруппаПланирования,
#Temp_IntervalsAll.Геозона,
#Temp_IntervalsAll.Приоритет
into #Temp_Intervals
from #Temp_IntervalsAll
	Inner Join _Reference114_VT25126 ГеоЗонаВременныеИнтервалы With (NOLOCK)
		On #Temp_IntervalsAll.Геозона = ГеоЗонаВременныеИнтервалы._Reference114_IDRRef
		And #Temp_IntervalsAll.ВремяНачалаНачальное >= ГеоЗонаВременныеИнтервалы._Fld25128
		And #Temp_IntervalsAll.ВремяНачалаНачальное < ГеоЗонаВременныеИнтервалы._Fld25129
   INNER JOIN dbo._Reference23294 T2 With (NOLOCK) 
		ON (#Temp_IntervalsAll.ГруппаПланирования = T2._IDRRef)
		AND (ГеоЗонаВременныеИнтервалы._Fld25128 >= T2._Fld25137)
		AND (NOT (((@P_TimeNow >= T2._Fld25138))))
WHERE
    #Temp_IntervalsAll.Период = @P_DateTimePeriodBegin
Group By 
	ГеоЗонаВременныеИнтервалы._Fld25128,
	ГеоЗонаВременныеИнтервалы._Fld25129,
	#Temp_IntervalsAll.Период,
	#Temp_IntervalsAll.ГруппаПланирования,
	#Temp_IntervalsAll.Геозона,
	T2._Fld25137,
	#Temp_IntervalsAll.Приоритет
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4022-01-04T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);

INsert into #Temp_Intervals
select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) As ВремяНачала,
#Temp_IntervalsAll.Период,
#Temp_IntervalsAll.ГруппаПланирования,
#Temp_IntervalsAll.Геозона,
#Temp_IntervalsAll.Приоритет
from #Temp_IntervalsAll
	Inner Join _Reference114_VT25126 ГеоЗонаВременныеИнтервалы With (NOLOCK)
		On #Temp_IntervalsAll.Геозона = ГеоЗонаВременныеИнтервалы._Reference114_IDRRef
		And #Temp_IntervalsAll.ВремяНачалаНачальное >= ГеоЗонаВременныеИнтервалы._Fld25128
		And #Temp_IntervalsAll.ВремяНачалаНачальное < ГеоЗонаВременныеИнтервалы._Fld25129
  INNER JOIN dbo._Reference23294 T4 With (NOLOCK) ON (#Temp_IntervalsAll.ГруппаПланирования = T4._IDRRef)
    AND (
        (@P_TimeNow < T4._Fld25140)
        OR (ГеоЗонаВременныеИнтервалы._Fld25128 >= T4._Fld25139)
    )
WHERE
    #Temp_IntervalsAll.Период = DATEADD(DAY, 1, @P_DateTimePeriodBegin)
Group By 
	ГеоЗонаВременныеИнтервалы._Fld25128,
	ГеоЗонаВременныеИнтервалы._Fld25129,
	#Temp_IntervalsAll.Период,
	#Temp_IntervalsAll.ГруппаПланирования,
	#Temp_IntervalsAll.Геозона,
    #Temp_IntervalsAll.Приоритет
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4022-01-04T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);

INsert into #Temp_Intervals
select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) As ВремяНачала,
#Temp_IntervalsAll.Период,
#Temp_IntervalsAll.ГруппаПланирования,
#Temp_IntervalsAll.Геозона,
#Temp_IntervalsAll.Приоритет
from #Temp_IntervalsAll
	Inner Join _Reference114_VT25126 ГеоЗонаВременныеИнтервалы With (NOLOCK)
		On #Temp_IntervalsAll.Геозона = ГеоЗонаВременныеИнтервалы._Reference114_IDRRef
		And #Temp_IntervalsAll.ВремяНачалаНачальное >= ГеоЗонаВременныеИнтервалы._Fld25128
		And #Temp_IntervalsAll.ВремяНачалаНачальное < ГеоЗонаВременныеИнтервалы._Fld25129
WHERE
	#Temp_IntervalsAll.Период BETWEEN DATEADD(DAY, 2, @P_DateTimePeriodBegin) AND @P_DateTimePeriodEnd --begin +2
Group By 
	ГеоЗонаВременныеИнтервалы._Fld25128,
	ГеоЗонаВременныеИнтервалы._Fld25129,
	#Temp_IntervalsAll.Период,
	#Temp_IntervalsAll.ГруппаПланирования,
	#Temp_IntervalsAll.Геозона,
    #Temp_IntervalsAll.Приоритет
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4022-01-04T00:00:00',@P_DateTimePeriodEnd='4022-01-15T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);

/*если основная база или реплика, то заменить таблицу ниже на исходный вариант*/
/*
With Temp_DeliveryPower AS
(
SELECT   
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
    ) AS ВремяНаОбслуживаниеОборот,
    CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME) AS Дата
FROM
    dbo._AccumRg25104 МощностиДоставки With (READCOMMITTED)
WHERE
    МощностиДоставки._Period BETWEEN @P_DateTimePeriodBegin AND @P_DateTimePeriodEnd
	AND МощностиДоставки._Fld25105RRef IN (Select ЗонаДоставкиРодительСсылка From  #Temp_GeoData)
GROUP BY
    CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME)
), */


With Temp_DeliveryPower AS
(
SELECT   
        МощностиДоставки.МассаОборот AS МассаОборот,    
        МощностиДоставки.ОбъемОборот AS ОбъемОборот,    
   МощностиДоставки.ВремяНаОбслуживаниеОборот AS ВремяНаОбслуживаниеОборот,
   МощностиДоставки.Период AS Дата
FROM
    [dbo].[DeliveryPowerAggregate] МощностиДоставки With (READCOMMITTED)
WHERE
    МощностиДоставки.Период BETWEEN @P_DateTimePeriodBegin AND @P_DateTimePeriodEnd
	AND МощностиДоставки.ЗонаДоставки IN (Select ЗонаДоставкиРодительСсылка From  #Temp_GeoData)
),
Temp_PlanningGroupPriority AS
(
select Период, Max(Приоритет) AS Приоритет from #Temp_Intervals Group by Период
)
SELECT
    T1.НоменклатураСсылка,
    T1.article,
    T1.code,
    MIN(
        ISNULL(
            T3.ВремяНачала,
CASE
                WHEN (T1.ДатаСоСклада > DATEADD(SECOND,-1,@P_DateTimePeriodEnd)) THEN DATEADD(
                    DAY,
                    1.0,
                    CAST(CAST(T1.ДатаСоСклада AS DATE) AS DATETIME)
                )
                ELSE DATEADD(DAY,1,@P_DateTimePeriodEnd)
            END
        )
    ) AS ДатаКурьерскойДоставки
Into #Temp_AvailableCourier
FROM
    #Temp_ShipmentDatesDeliveryCourier T1 WITH(NOLOCK)
    Left JOIN Temp_DeliveryPower T2 --WITH(NOLOCK)
    Inner JOIN #Temp_Intervals T3 WITH(NOLOCK)
        Inner Join Temp_PlanningGroupPriority With (NOLOCK) ON T3.Период = Temp_PlanningGroupPriority.Период AND T3.Приоритет = Temp_PlanningGroupPriority.Приоритет
		ON T3.Период = T2.Дата
	ON T2.МассаОборот >= T1.Вес
    AND T2.ОбъемОборот >= T1.Объем
    AND T2.ВремяНаОбслуживаниеОборот >= T1.ВремяНаОбслуживание
    AND T2.Дата >= 
		CAST(CAST(T1.ДатаСоСклада AS DATE) AS DATETIME)    
    AND T3.ГруппаПланирования = T1.ГруппаПланирования
    AND T3.ВремяНачала >= T1.ДатаСоСклада
	AND T1.PickUp = 0
GROUP BY
	T1.НоменклатураСсылка,
    T1.article,
	T1.code
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='4022-01-04T00:00:00',@P_DateTimePeriodEnd='4022-01-15T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);

Select 
	IsNull(#Temp_AvailableCourier.article,#Temp_AvailablePickUp.article) AS article,
	IsNull(#Temp_AvailableCourier.code,#Temp_AvailablePickUp.code) AS code,
	IsNull(#Temp_AvailableCourier.ДатаКурьерскойДоставки,@P_MaxDate) AS available_date_courier,
	IsNull(#Temp_AvailablePickUp.ВремяНачала,@P_MaxDate) AS available_date_self
From
	#Temp_AvailableCourier 
	FULL Join #Temp_AvailablePickUp 
		On #Temp_AvailableCourier.НоменклатураСсылка = #Temp_AvailablePickUp.НоменклатураСсылка 

DROP TABLE #Temp_GoodsRaw
DROP TABLE #Temp_GeoData
DROP TABLE #Temp_WarehouseDates
DROP TABLE #Temp_MinimumWarehouseDates
DROP TABLE #Temp_GoodsBegin
DROP TABLE #Temp_Goods
DROP TABLE #Temp_Remains
DROP TABLE #Temp_Sources
DROP TABLE #Temp_SourcesWithPrices
DROP TABLE #Temp_BestPriceAfterClosestDate
--DROP TABLE #Temp_SourcesCorrectedDate
DROP TABLE #Temp_ClosestDatesByGoods
DROP TABLE #Temp_ShipmentDates
DROP TABLE #Temp_ShipmentDatesDeliveryCourier
DROP TABLE #Temp_Intervals
DROP TABLE #Temp_IntervalsAll
Drop Table #Temp_T3
DROP TABLE #Temp_ShipmentDatesPickUp
DROP TABLE #Temp_AvailableCourier
DROP TABLE #Temp_AvailablePickUp
--DROP TABLE #Temp_PickupDatesGroup
DROP TAble #Temp_PickupWorkingHours
DROP TAble #Temp_Dates
DROP Table #Temp_PickupPoints
--DRop Table #Temp_PlanningGroupPriority
DROP Table #Temp_ClosestDatesByGoodsWithoutShifting