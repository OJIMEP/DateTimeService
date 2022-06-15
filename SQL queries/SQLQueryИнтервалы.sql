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

DECLARE @P_AdressCode nvarchar(20);

DECLARE @P_DateTimeNow datetime;
DECLARE @P_DateTimePeriodBegin datetime;
DECLARE @P_DateTimePeriodEnd datetime;
DECLARE @P_TimeNow datetime;
DECLARE @P_EmptyDate datetime;
DECLARE @P_MaxDate datetime;

 SET @P_Article1 = '936160'; --артикулы
 SET @P_Article2 = '424941';
 SET @P_Article3 = '69516';
 SET @P_Article4 = '5962720';
 SET @P_Article5 = '538584';
 SET @P_Article6 = '6045211';
 SET @P_Article7 = '5944337';
 SET @P_Article8 = '5657497';

 SET @P_Code1 = '00-005990263'; --коды для уценки
 SET @P_Code2 = '00-00527933';
 SET @P_Code3 = NULL;
 SET @P_Code4 = NULL;
 SET @P_Code5 = NULL;
 SET @P_Code6 = NULL;
 SET @P_Code7 = NULL;
 SET @P_Code8 = NULL;

 SET @PickupPoint1 = '';

 Set @P_AdressCode = '47175';--'4948900';--'47175'--'47175000000'--'3298156' --код адреса
 
  Set @P_DateTimeNow = '4021-11-10T16:46:00' 
 Set @P_DateTimePeriodBegin = '4021-11-10T00:00:00'
 Set @P_DateTimePeriodEnd = '4021-11-13T00:00:00'
 Set @P_TimeNow = '2001-01-01T16:46:00'
 Set @P_EmptyDate = '2001-01-01T00:00:00'
 Set @P_MaxDate = '5999-11-11T00:00:00'


 DECLARE @P_Credit numeric(2);
 Set @P_Credit = 0;

 DECLARE @P_Floor numeric(2);
 Set @P_Floor = 4;

 DECLARE @P_DaysToShow numeric(2);
 Set @P_DaysToShow = 7;

  DECLARE @P_GeoCode nvarchar(4);
 Set @P_GeoCode = '';

   DECLARE @P_OrderNumber nvarchar(11);
 Set @P_OrderNumber = '';--'102.567.095';--'231.727.030';

   DECLARE @P_OrderDate datetime;
 Set @P_OrderDate = ''; --'4021-07-20T16:42:52'--'4021-07-20T14:23:51';

DECLARE @P_ApplyShifting numeric(2);
set @P_ApplyShifting = 1;

DECLARE @P_DaysToShift numeric(2);
set @P_DaysToShift = 3;

Create Table #Temp_GoodsRaw   
(	
	Article nvarchar(20), 
	code nvarchar(20), 
    PickupPoint nvarchar(10),
    quantity int
)

INSERT INTO 
	#Temp_GoodsRaw ( 
		Article, code, PickupPoint, quantity 
	)
VALUES
	--(@P_Article1,@P_Code1,NULL,0),
	--(@P_Article2,@P_Code2,NULL,0),
	--(@P_Article1,@P_Code1,NULL,0),
	--(@P_Article3,@P_Code3,NULL,1)--,
	--('843414',NULL,NULL,1)
	(@P_Article1,NULL,NULL,1)--,
	--(@P_Article6,NULL,NULL,1),
	--(@P_Article7,NULL,NULL,1),
	--(@P_Article8,NULL,NULL,1)--,
	--(@P8,1)
	;


Select 
	_IDRRef AS ЗаказСсылка,
	_Fld8243RRef AS ЗонаДоставки,
	_Fld8244 AS ВремяДоставкиС,
	_Fld8245 AS ВремяДоставкиПо,
	_Fld8205RRef AS ПВЗСсылка,
	_Fld8241RRef As СпособДоставки,
	_Fld8260RRef As АдресДоставки,
	_Fld21917RRef AS Габариты,
	Case When _Fld21650 = ''
		then @P_Floor
		Else
		Convert(numeric(2),_Fld21650)
	End As Этаж,
	_Fld25158 As Вес,
	_Fld25159 As Объем,
	_Date_Time,
	_Number
Into #Temp_OrderInfo
from dbo._Document317 OrderDocument
where 
	OrderDocument._Date_Time = @P_OrderDate 
	And OrderDocument._Number = @P_OrderNumber
	And _Fld8244 = '2001-01-01T01:00:00' 
	And _Fld8245 = '2001-01-01T23:00:00'

Select
	Товары._Fld8276RRef AS НоменклатураСсылка,
	_Fld8280 AS Количество,
	#Temp_OrderInfo.ЗаказСсылка AS ЗаказСсылка
Into #Temp_GoodsOrder
From 
	dbo._Document317_VT8273 Товары
	Inner Join #Temp_OrderInfo
		On Товары._Document317_IDRRef = #Temp_OrderInfo.ЗаказСсылка

Select
	IsNull(_Reference114_VT23370._Fld23372RRef,Геозона._Fld23104RRef) As СкладСсылка,
	ЗоныДоставки._ParentIDRRef As ЗонаДоставкиРодительСсылка,
	ЗоныДоставкиРодитель._Description AS ЗонаДоставкиРодительНаименование,
	Геозона._IDRRef As Геозона
Into #Temp_GeoData
From dbo._Reference114 Геозона With (NOLOCK)
	Inner Join _Reference114_VT23370 With (NOLOCK)
	on _Reference114_VT23370._Reference114_IDRRef = Геозона._IDRRef
	Inner Join _Reference99 ЗоныДоставки With (NOLOCK)
	on Геозона._Fld2847RRef = ЗоныДоставки._IDRRef
	Inner Join _Reference99 ЗоныДоставкиРодитель With (NOLOCK)
	on ЗоныДоставки._ParentIDRRef = ЗоныДоставкиРодитель._IDRRef
where		
	(@P_GeoCode = '' AND 
	@P_AdressCode <> '' And
Геозона._IDRRef IN (
	Select Top 1 --по адресу находим геозону
	ГеоАдрес._Fld2785RRef 
	From dbo._Reference112 ГеоАдрес With (NOLOCK)
	Where ГеоАдрес._Fld25155 = @P_AdressCode))
OR 
(@P_GeoCode <> '' AND Геозона._Fld21249 = @P_GeoCode) 
OR 
Геозона._Fld2847RRef In (select ЗонаДоставки from #Temp_OrderInfo Where #Temp_OrderInfo.СпособДоставки = 0x9B7EC3D470857E364E10EF7D3C09E30D) 
OPTION (KEEP PLAN, KEEPFIXED PLAN);

Select _IDRRef As СкладСсылка
Into #Temp_PickupPoints
From dbo._Reference226 Склады 
Where Склады._Fld19544 = @PickupPoint1
Union All
Select #Temp_OrderInfo.ПВЗСсылка from #Temp_OrderInfo
Where #Temp_OrderInfo.СпособДоставки = 0x9B5E4A5ABB206D854BE9B32BF442A653
OPTION (KEEP PLAN, KEEPFIXED PLAN);


/*Создание таблицы товаров и ее наполнение данными из БД*/
Select 
	Номенклатура._IDRRef AS НоменклатураСсылка,
	Упаковки._IDRRef AS УпаковкаСсылка,
	Номенклатура._Fld21822RRef as ТНВЭДСсылка,
	Номенклатура._Fld3515RRef as ТоварнаяКатегорияСсылка,
	Sum(T1.quantity) As Количество	
INTO #Temp_Goods
From 
	#Temp_GoodsRaw T1
	Inner Join 	dbo._Reference149 Номенклатура With (NOLOCK) 
		ON T1.code is NULL and T1.Article = Номенклатура._Fld3480
	Inner Join dbo._Reference256 Упаковки With (NOLOCK)
		On 
		Упаковки._OwnerID_TYPE = 0x08  
		AND Упаковки.[_OwnerID_RTRef] = 0x00000095
		AND Номенклатура._IDRRef = Упаковки._OwnerID_RRRef		
		And Упаковки._Fld6003RRef = Номенклатура._Fld3489RRef
		AND Упаковки._Marked = 0x00
Group By 
	Номенклатура._IDRRef,
	Упаковки._IDRRef,
	Номенклатура._Fld21822RRef,
	Номенклатура._Fld3515RRef
union all
Select 
	Номенклатура._IDRRef,
	Упаковки._IDRRef,
	Номенклатура._Fld21822RRef,
	Номенклатура._Fld3515RRef,
	Sum(T1.quantity)	
From 
	#Temp_GoodsRaw T1
	Inner Join 	dbo._Reference149 Номенклатура With (NOLOCK) 
		ON T1.code is not NULL and T1.code = Номенклатура._Code
	Inner Join dbo._Reference256 Упаковки With (NOLOCK)
		On 
		Упаковки._OwnerID_TYPE = 0x08  
		AND Упаковки.[_OwnerID_RTRef] = 0x00000095
		AND Номенклатура._IDRRef = Упаковки._OwnerID_RRRef		
		And Упаковки._Fld6003RRef = Номенклатура._Fld3489RRef
		AND Упаковки._Marked = 0x00
Group By 
	Номенклатура._IDRRef,
	Упаковки._IDRRef,
	Номенклатура._Fld21822RRef,
	Номенклатура._Fld3515RRef
union all
Select 
	Номенклатура._IDRRef,
	Упаковки._IDRRef,
	Номенклатура._Fld21822RRef,
	Номенклатура._Fld3515RRef,
	Sum(T1.Количество)	
From 
	#Temp_GoodsOrder T1
	Inner Join 	dbo._Reference149 Номенклатура With (NOLOCK) 
		ON T1.НоменклатураСсылка = Номенклатура._IDRRef
	Inner Join dbo._Reference256 Упаковки With (NOLOCK)
		On 
		Упаковки._OwnerID_TYPE = 0x08  
		AND Упаковки.[_OwnerID_RTRef] = 0x00000095
		AND Номенклатура._IDRRef = Упаковки._OwnerID_RRRef		
		And Упаковки._Fld6003RRef = Номенклатура._Fld3489RRef
		AND Упаковки._Marked = 0x00
Where 
	Номенклатура._Fld3514RRef = 0x84A6131B6DC5555A4627E85757507687 -- тип номенклатуры товар
Group By 
	Номенклатура._IDRRef,
	Упаковки._IDRRef,
	Номенклатура._Fld21822RRef,
	Номенклатура._Fld3515RRef
OPTION (KEEP PLAN, KEEPFIXED PLAN);
/*Конец товаров*/

/*Размеры корзины в целом для расчета габаритов*/
SELECT
CAST(SUM((T2._Fld6000 * T1.Количество)) AS NUMERIC(36, 6)) AS Вес,
CAST(SUM((T2._Fld6006 * T1.Количество)) AS NUMERIC(38, 8)) AS Объем,
MAX(T2._Fld6001) AS Высота,
MAX(T2._Fld6002) AS Глубина,
MAX(T2._Fld6009) AS Ширина,
0x00000000000000000000000000000000  AS Габарит
Into #Temp_Size
FROM #Temp_Goods T1 WITH(NOLOCK)
INNER JOIN dbo._Reference256 T2 With (NOLOCK) 
ON (T2._IDRRef = T1.УпаковкаСсылка) AND (T1.УпаковкаСсылка <> 0x00000000000000000000000000000000)
OPTION (KEEP PLAN, KEEPFIXED PLAN);
/*Габарит корзины общий*/
SELECT
    TOP 1 CASE
        WHEN (
            ISNULL(
                T1.Габарит,
                0x00000000000000000000000000000000
            ) <> 0x00000000000000000000000000000000
        ) THEN T1.Габарит
        WHEN (T4._Fld21339 > 0)
        AND (T1.Вес >= T4._Fld21339)
        AND (T5._Fld21337 > 0)
        AND (T1.Объем >= T5._Fld21337) THEN 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D --хбт в кбт
        WHEN (T2._Fld21168 > 0)
        AND (T1.Вес >= T2._Fld21168) THEN 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D --кбт
        WHEN (T3._Fld21166 > 0)
        AND (T1.Объем >= T3._Fld21166) THEN 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D --кбт
        WHEN (T6._Fld21580 > 0)
        AND (T1.Высота > 0)
        AND (T1.Глубина > 0)
        AND (T1.Ширина >0) THEN CASE
            WHEN (T1.Высота >= T6._Fld21580) OR (T1.Глубина >= T6._Fld21580) OR (T1.Ширина >= T6._Fld21580) THEN 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D --кбт
            ELSE 0x8AB421D483ABE88A4C4C9928262FFB0D --мбт
        END
        ELSE 0x8AB421D483ABE88A4C4C9928262FFB0D --мбт
    END AS Габарит
Into #Temp_Dimensions
FROM
    #Temp_Size T1 WITH(NOLOCK)
    INNER JOIN dbo._Const21167 T2 ON 1 = 1
    INNER JOIN dbo._Const21165 T3 ON 1 = 1
    INNER JOIN dbo._Const21338 T4 ON 1 = 1
    INNER JOIN dbo._Const21336 T5 ON 1 = 1
    INNER JOIN dbo._Const21579 T6 ON 1 = 1
OPTION (KEEP PLAN, KEEPFIXED PLAN);

SELECT
    COUNT_BIG(T1.НоменклатураСсылка) AS КоличествоСтрок,
    T1.НоменклатураСсылка AS НоменклатураСсылка,
    T2._Fld6000 * T1.Количество AS Вес,
    T2._Fld6006 * T1.Количество AS Объем,
    CASE
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 < 120.0)
        AND (T2._Fld6000 >= 50.0) THEN (T3.Fld24101_ * @P_Floor)
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 >= 120.0) THEN (T3.Fld24102_ * @P_Floor)
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 < 5.0) THEN (T3.Fld26615_ * @P_Floor)
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 >= 5.0)
        AND (T2._Fld6000 < 50.0) THEN (T3.Fld26616_ * @P_Floor)
        ELSE 0.0
    END  AS УсловиеЭтажМассаПоТоварам
Into #Temp_Weight
FROM
    #Temp_Goods T1 WITH(NOLOCK)
    LEFT OUTER JOIN dbo._Reference256 T2 With (NOLOCK) ON (
        0x08 = T2._OwnerID_TYPE
        AND 0x00000095 = T2._OwnerID_RTRef
        AND T1.УпаковкаСсылка = T2._IDRRef
    )
    INNER JOIN (
        SELECT
            T6._Fld24101 AS Fld24101_,
            T6._Fld24102 AS Fld24102_,
            T6._Fld26615 AS Fld26615_,
            T6._Fld26616 AS Fld26616_
        FROM
            (
                SELECT
                    MAX(T5._Period) AS MAXPERIOD_
                FROM
                    dbo._InfoRg24088 T5
            ) T4
            INNER JOIN dbo._InfoRg24088 T6 ON T4.MAXPERIOD_ = T6._Period
    ) T3 ON 1 = 1
GROUP BY
    T1.НоменклатураСсылка,
    T2._Fld6000 * T1.Количество,
    T2._Fld6006 * T1.Количество,
    CASE
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 < 120.0)
        AND (T2._Fld6000 >= 50.0) THEN (T3.Fld24101_ * @P_Floor)
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 >= 120.0) THEN (T3.Fld24102_ * @P_Floor)
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 < 5.0) THEN (T3.Fld26615_ * @P_Floor)
        WHEN (
            (T2._Fld6006 > 0.8)
            OR (T2._Fld6002 > 1.85)
            OR (T2._Fld6001 > 1.85)
            OR (T2._Fld6009 > 1.85)
        )
        AND (T2._Fld6000 >= 5.0)
        AND (T2._Fld6000 < 50.0) THEN (T3.Fld26616_ * @P_Floor)
        ELSE 0.0
    END
OPTION (KEEP PLAN, KEEPFIXED PLAN);

SELECT Distinct
    CASE
        WHEN (IsNull(T2.Габарит,0x8AB421D483ABE88A4C4C9928262FFB0D) = 0x8AB421D483ABE88A4C4C9928262FFB0D) THEN 7 --мбт
        ELSE 14
    END AS УсловиеГабариты,
    CASE
        WHEN (@P_Credit = 1) --кредит рассрочка
            THEN T3.Fld24103_
        ELSE 0
    END AS УсловиеСпособОплаты,
    CASE
        WHEN (T1.ЗонаДоставкиРодительНаименование LIKE '%Минск%') --наименование зоны доставки
        AND (IsNull(T2.Габарит,0x8AB421D483ABE88A4C4C9928262FFB0D) = 0x8AB421D483ABE88A4C4C9928262FFB0D) THEN T3.Fld24091_ --мбт
        WHEN (T1.ЗонаДоставкиРодительНаименование LIKE '%Минск%')
        AND (IsNull(T2.Габарит,0x8AB421D483ABE88A4C4C9928262FFB0D) = 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) THEN T3.Fld24092_ --кбт
        ELSE 0
    END AS УсловиеМинскЧас
Into #Temp_TimeByOrders
FROM
    #Temp_GeoData T1 WITH(NOLOCK)
	Left Join #Temp_Dimensions T2 On 1=1
    INNER JOIN (
        SELECT
            T5._Fld24103 AS Fld24103_,
            T5._Fld24091 AS Fld24091_,
            T5._Fld24092 AS Fld24092_
        FROM
            (
                SELECT
                    MAX(T4._Period) AS MAXPERIOD_
                FROM
                    dbo._InfoRg24088 T4
            ) T3
            INNER JOIN dbo._InfoRg24088 T5 ON T3.MAXPERIOD_ = T5._Period
    ) T3 ON 1 = 1
OPTION (KEEP PLAN, KEEPFIXED PLAN);

SELECT
    T2.Fld24090_ * SUM(T1.КоличествоСтрок) AS УсловиеКоличествоСтрок,
    CASE
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) < 5.0 THEN T2.Fld24094_
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) >= 5.0
			AND SUM(T1.Вес) < 20.0 THEN T2.Fld24095_
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) >= 20.0
			AND SUM(T1.Вес) < 65.0 THEN T2.Fld24096_
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) >= 65.0
			AND SUM(T1.Вес) < 120.0 THEN T2.Fld24097_
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) >= 120.0
			AND SUM(T1.Вес) < 250.0 THEN T2.Fld24098_
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) >= 250.0
			AND SUM(T1.Вес) < 400.0 THEN T2.Fld26611_
        WHEN SUM(T1.Объем) < 0.8
			AND SUM(T1.Вес) >= 400.0 THEN T2.Fld26612_
        WHEN SUM(T1.Объем) >= 0.8
			AND SUM(T1.Вес) < 120.0 THEN T2.Fld24099_
        WHEN SUM(T1.Объем) >= 0.8
			AND SUM(T1.Вес) >= 120.0
			AND SUM(T1.Вес) < 250.0 THEN T2.Fld24100_
        WHEN SUM(T1.Объем) >= 0.8
			AND SUM(T1.Вес) >= 250.0
			AND SUM(T1.Вес) < 600.0 THEN T2.Fld26613_
        WHEN SUM(T1.Объем) >= 0.8
			AND SUM(T1.Вес) >= 600.0 THEN T2.Fld26614_
    END As УсловиеВесОбъем,
    T2.Fld24089_ As МинимальноеВремя,
    SUM(T1.УсловиеЭтажМассаПоТоварам) As УсловиеЭтажМассаОбщ
INTO #Temp_Time1
FROM
    #Temp_Weight T1 WITH(NOLOCK)
    INNER JOIN (
        SELECT
            T5._Fld24090 AS Fld24090_,
            T5._Fld24094 AS Fld24094_,
            T5._Fld24095 AS Fld24095_,
            T5._Fld24096 AS Fld24096_,
            T5._Fld24097 AS Fld24097_,
            T5._Fld24098 AS Fld24098_,
            T5._Fld26611 AS Fld26611_,
            T5._Fld26612 AS Fld26612_,
            T5._Fld24099 AS Fld24099_,
            T5._Fld24100 AS Fld24100_,
            T5._Fld26613 AS Fld26613_,
            T5._Fld26614 AS Fld26614_,
            T5._Fld24089 AS Fld24089_
        FROM
            (
                SELECT
                    MAX(T4._Period) AS MAXPERIOD_
                FROM
                    dbo._InfoRg24088 T4
            ) T3
            INNER JOIN dbo._InfoRg24088 T5 ON T3.MAXPERIOD_ = T5._Period
    ) T2 ON 1 = 1
GROUP BY
    T2.Fld24090_,
    T2.Fld24089_,
    T2.Fld24094_,
    T2.Fld24095_,
    T2.Fld24096_,
    T2.Fld24097_,
    T2.Fld24098_,
    T2.Fld26611_,
    T2.Fld26612_,
    T2.Fld26613_,
    T2.Fld26614_,
    T2.Fld24099_,
    T2.Fld24100_
OPTION (KEEP PLAN, KEEPFIXED PLAN);

/*Время обслуживания началось выше и тут итоговая цифра*/
SELECT
    ISNULL(T2.МинимальноеВремя, 0) + ISNULL(T2.УсловиеКоличествоСтрок, 0) + ISNULL(T1.УсловиеМинскЧас, 0) + ISNULL(T2.УсловиеЭтажМассаОбщ, 0) + ISNULL(T2.УсловиеВесОбъем, 0) + ISNULL(T1.УсловиеСпособОплаты, 0) AS ВремяВыполнения
Into #Temp_TimeService
FROM
    #Temp_TimeByOrders T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_Time1 T2 WITH(NOLOCK)
    ON 1 = 1
OPTION (KEEP PLAN, KEEPFIXED PLAN);

/*Группа планирования*/
Select ГруппыПланирования._IDRRef AS ГруппаПланирования,
	ГруппыПланирования._Fld23302RRef AS Склад,
	ГруппыПланирования._Fld25137 AS ВремяДоступностиНаСегодня,
	ГруппыПланирования._Fld25138 AS ВремяСтопаСегодня,
	ГруппыПланирования._Fld25139 AS ВремяДоступностиНаЗавтра,
	ГруппыПланирования._Fld25140 AS ВремяСтопаЗавтра,
	IsNull(ГруппыПланирования._Fld25519, @P_EmptyDate)AS ГруппаПланированияДобавляемоеВремя,
	1 AS Основная,
	ГруппыПланирования._Description
Into #Temp_PlanningGroups
From
dbo._Reference23294 ГруппыПланирования With (NOLOCK)
	Inner Join dbo._Reference23294_VT23309 With (NOLOCK)
		on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
		and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
	AND ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
	AND ГруппыПланирования._Fld23301RRef IN (Select Габарит From #Temp_Dimensions With (NOLOCK))  --габариты
	AND ГруппыПланирования._Marked = 0x00
UNION ALL
Select 
	ПодчиненнаяГП._IDRRef AS ГруппаПланирования,
	ГруппыПланирования._Fld23302RRef AS Склад,
	ПодчиненнаяГП._Fld25137 AS ВремяДоступностиНаСегодня,
	ПодчиненнаяГП._Fld25138 AS ВремяСтопаСегодня,
	ПодчиненнаяГП._Fld25139 AS ВремяДоступностиНаЗавтра,
	ПодчиненнаяГП._Fld25140 AS ВремяСтопаЗавтра,
	IsNull(ПодчиненнаяГП._Fld25519, @P_EmptyDate)AS ГруппаПланированияДобавляемоеВремя,
	0,
	ПодчиненнаяГП._Description
From
	dbo._Reference23294 ГруппыПланирования With (NOLOCK)
	Inner Join dbo._Reference23294_VT23309	With (NOLOCK)	
		on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
		and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
	Inner Join dbo._Reference23294 ПодчиненнаяГП
			On  ГруппыПланирования._Fld26526RRef = ПодчиненнаяГП._IDRRef
Where 
	--ГруппыПланирования._Fld23302RRef IN (Select СкладНазначения From #Temp_DateAvailable) --склад
	--AND 
	ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
	AND ГруппыПланирования._Marked = 0x00
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

/*Отсюда начинается процесс получения оптимальной даты отгрузки*/
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
		And (Цены._Fld21982<>0 AND Цены._Fld21442 * Temp_ExchangeRates.Курс / Temp_ExchangeRates.Кратность >= Цены._Fld21982 OR Цены._Fld21411 >= Цены._Fld21616)
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
Union ALL
SELECT --товары по заказу
    Резервирование._Fld21408RRef AS НоменклатураСсылка,
    Резервирование._Fld21410_TYPE AS Источник_TYPE,
	Резервирование._Fld21410_RTRef AS Источник_RTRef,
	Резервирование._Fld21410_RRRef AS Источник_RRRef,
	0x08 AS Регистратор_TYPE,
    Резервирование._RecorderTRef AS Регистратор_RTRef,
    Резервирование._RecorderRRef AS Регистратор_RRRef,
    Резервирование._Fld23568RRef AS СкладИсточника,
    Резервирование._Fld21424 AS ДатаСобытия,
    Резервирование._Fld21411 - Резервирование._Fld21412 AS Количество
FROM
	_AccumRg21407 Резервирование With (READCOMMITTED)
	Inner Join #Temp_GoodsOrder On
		Резервирование._RecorderRRef = #Temp_GoodsOrder.ЗаказСсылка
		And Резервирование._Fld21408RRef = #Temp_GoodsOrder.НоменклатураСсылка
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='4021-08-17T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);

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
	T1._Fld23831RRef AS СкладИсточника,
	T1._Fld23833RRef AS СкладНазначения,
	MIN(T1._Fld23834) AS ДатаПрибытия 
Into #Temp_MinimumWarehouseDates
FROM
    dbo._InfoRg23830 T1 With (READCOMMITTED) ---, INDEX([_InfoRg23830_Custom2]))
	Inner Join SourceWarehouses On T1._Fld23831RRef = SourceWarehouses.СкладИсточника
WHERE
	T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData UNION ALL Select СкладСсылка From #Temp_PickupPoints)
		AND	T1._Fld23832 BETWEEN @P_DateTimeNow AND DateAdd(DAY,6,@P_DateTimeNow)
GROUP BY T1._Fld23831RRef,
T1._Fld23833RRef
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='4021-08-17T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);


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
    T1.Количество > 0 And
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
	DATEADD(SECOND, DATEDIFF(SECOND, @P_EmptyDate, IsNull(#Temp_PlanningGroups.ГруппаПланированияДобавляемоеВремя,@P_EmptyDate)), T5.ДатаПрибытия),
    2,
    T5.СкладНазначения
FROM
    #Temp_Remains T4 WITH(NOLOCK)
    INNER JOIN #Temp_WarehouseDates T5 WITH(NOLOCK)
    ON (T4.СкладИсточника = T5.СкладИсточника)
    AND (T4.ДатаСобытия = T5.ДатаСобытия)
	Left Join #Temp_PlanningGroups On T5.СкладНазначения = #Temp_PlanningGroups.Склад AND #Temp_PlanningGroups.Основная = 1
WHERE
	T4.Количество > 0 And
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
	DATEADD(SECOND, DATEDIFF(SECOND, @P_EmptyDate, IsNull(#Temp_PlanningGroups.ГруппаПланированияДобавляемоеВремя,@P_EmptyDate)), T7.ДатаПрибытия),
    3,
    T7.СкладНазначения
FROM
    #Temp_Remains T6 WITH(NOLOCK)
    INNER JOIN #Temp_WarehouseDates T7 WITH(NOLOCK)
    ON (T6.СкладИсточника = T7.СкладИсточника)
    AND (T6.ДатаСобытия = T7.ДатаСобытия)
	Left Join #Temp_PlanningGroups With (NOLOCK) On T7.СкладНазначения = #Temp_PlanningGroups.Склад AND #Temp_PlanningGroups.Основная = 1
WHERE
	T6.Количество > 0 And
    NOT T6.Регистратор_RRRef IS NULL
	And T6.Источник_RTRef = 0x00000153

UNION
ALL
Select
	векРезервированиеТоваров._Fld21408RRef,
	векРезервированиеТоваров._Fld21412,
	векРезервированиеТоваров._Fld21410_TYPE,
	векРезервированиеТоваров._Fld21410_RTRef,
	векРезервированиеТоваров._Fld21410_RRRef,
	векРезервированиеТоваров._Fld23568RRef,
	векРезервированиеТоваров._Fld21424,
	DATEADD(SECOND, DATEDIFF(SECOND, @P_EmptyDate, IsNull(#Temp_PlanningGroups.ГруппаПланированияДобавляемоеВремя,@P_EmptyDate)), IsNULL(#Temp_WarehouseDates.ДатаПрибытия, #Temp_MinimumWarehouseDates.ДатаПрибытия)),
	4,
	ISNULL(#Temp_WarehouseDates.СкладНазначения, #Temp_MinimumWarehouseDates.СкладНазначения) AS СкладНазначения
From
	dbo._AccumRg21407 векРезервированиеТоваров With (READCOMMITTED)
		Inner Join  #Temp_GoodsOrder Товары
		ON (векРезервированиеТоваров._RecorderRRef = Товары.ЗаказСсылка)		
			AND (векРезервированиеТоваров._RecorderTRef = 0x0000013D) --поменять на правильный тип ЗаказКлиента 
				--OR векРезервированиеТоваров._RecorderTRef = 0x00000153)
			AND векРезервированиеТоваров._Fld21408RRef = Товары.НоменклатураСсылка --номенклатура
			AND (векРезервированиеТоваров._Fld21410_RRRef <> 0x00000000000000000000000000000000) 
		Left Join #Temp_WarehouseDates
		ON векРезервированиеТоваров._Fld23568RRef = #Temp_WarehouseDates.СкладИсточника
			AND векРезервированиеТоваров._Fld21424 = #Temp_WarehouseDates.ДатаСобытия
		Left Join #Temp_MinimumWarehouseDates 
		On векРезервированиеТоваров._Fld23568RRef = #Temp_MinimumWarehouseDates.СкладИсточника
			AND векРезервированиеТоваров._Fld21424 = '2001-01-01 00:00:00'
		Left Join #Temp_PlanningGroups With (NOLOCK) 
		On ISNULL(#Temp_WarehouseDates.СкладНазначения, #Temp_MinimumWarehouseDates.СкладНазначения) = #Temp_PlanningGroups.Склад 
			AND #Temp_PlanningGroups.Основная = 1
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With TempSourcesGrouped AS
(
Select
	T1.НоменклатураСсылка AS НоменклатураСсылка,
	Sum(T1.Количество) AS Количество,
	T1.ДатаДоступности AS ДатаДоступности,
	T1.СкладНазначения AS СкладНазначения
From
	#Temp_Sources T1	
Group by
	T1.НоменклатураСсылка,
	T1.ДатаДоступности,
	T1.СкладНазначения
)
Select
	Источники1.НоменклатураСсылка AS Номенклатура,
	Источники1.СкладНазначения AS СкладНазначения,
	Источники1.ДатаДоступности AS ДатаДоступности,
	Sum(Источник2.Количество) AS Количество
Into #Temp_AvailableGoods
From
	TempSourcesGrouped AS Источники1
		Left Join TempSourcesGrouped AS Источник2
		On Источники1.НоменклатураСсылка = Источник2.НоменклатураСсылка
		AND Источники1.СкладНазначения = Источник2.СкладНазначения
			AND Источники1.ДатаДоступности >= Источник2.ДатаДоступности	
Group by
	Источники1.НоменклатураСсылка,
	Источники1.ДатаДоступности,
	Источники1.СкладНазначения
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
	T1.Количество,
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
    ON (T1.НоменклатураСсылка = Резервирование._Fld21408RRef)
    AND (
        T1.Источник_TYPE = 0x08
        AND T1.Источник_RTRef = Резервирование._RecorderTRef
        AND T1.Источник_RRRef = Резервирование._RecorderRRef
    )
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

SELECT
    T1.НоменклатураСсылка,
	T1.Количество,
    T1.Источник_TYPE,
    T1.Источник_RTRef,
    T1.Источник_RRRef,
    T1.СкладИсточника,
    T1.СкладНазначения,
    T1.ДатаСобытия,
    ISNULL(T2.ДатаДоступности1, T1.ДатаДоступности) AS ДатаДоступности,
    T1.ТипИсточника
Into #Temp_SourcesCorrectedDate
FROM
    #Temp_Sources T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_BestPriceAfterClosestDate T2 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
    AND (T1.ДатаДоступности = T2.ДатаДоступности)
    AND (T1.СкладНазначения = T2.СкладНазначения)
    AND (T1.ТипИсточника = 3)
OPTION (KEEP PLAN, KEEPFIXED PLAN);

With Temp_ClosestDate AS
(
    SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    MIN(T1.ДатаДоступности) AS ДатаДоступности
FROM 
    #Temp_Sources T1 WITH(NOLOCK)
GROUP BY 
    T1.НоменклатураСсылка,
    T1.СкладНазначения
)
Select 
	T4.НоменклатураСсылка AS НоменклатураСсылка,
	T4.СкладНазначения AS СкладНазначения,
	Min(T4.БлижайшаяДата) AS БлижайшаяДата
into #Temp_ClosestDatesByGoods
From 
	(SELECT
		T1.НоменклатураСсылка,
		ISNULL(T3.СкладНазначения, T2.СкладНазначения) AS СкладНазначения,
		MIN(ISNULL(T3.ДатаДоступности, T2.ДатаДоступности)) AS БлижайшаяДата

	FROM
		#Temp_Goods T1 WITH(NOLOCK)
		LEFT OUTER JOIN #Temp_SourcesCorrectedDate T2 WITH(NOLOCK)
		ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
		LEFT OUTER JOIN (
			SELECT
				T4.НоменклатураСсылка,
				T4.ДатаДоступности,
				T4.СкладНазначения,
				T5.ДатаДоступности AS БлижайшаяДата
			FROM
				#Temp_Sources T4 WITH(NOLOCK)
				LEFT OUTER JOIN Temp_ClosestDate T5 WITH(NOLOCK)
				ON (T4.НоменклатураСсылка = T5.НоменклатураСсылка)
				AND (T4.СкладНазначения = T5.СкладНазначения)
				AND (T4.ТипИсточника = 1 Or T4.ТипИсточника = 4)
		) T3 ON (T1.НоменклатураСсылка = T3.НоменклатураСсылка)
		AND (
			T3.ДатаДоступности <= DATEADD(DAY, 4, T3.БлижайшаяДата) --это параметр КоличествоДнейАнализа
		)
		Where T1.Количество = 1
	GROUP BY
		T1.НоменклатураСсылка,
		ISNULL(T3.СкладНазначения, T2.СкладНазначения)
	Union ALL
	Select 
		#Temp_Goods.НоменклатураСсылка,
		#Temp_AvailableGoods.СкладНазначения,
		Min(#Temp_AvailableGoods.ДатаДоступности)
	From #Temp_Goods With (NOLOCK)
		Left Join #Temp_AvailableGoods With (NOLOCK) 
			On #Temp_Goods.НоменклатураСсылка = #Temp_AvailableGoods.Номенклатура
			AND #Temp_Goods.Количество <= #Temp_AvailableGoods.Количество
	Where
		#Temp_Goods.Количество > 1
	Group By
		#Temp_Goods.НоменклатураСсылка,
		#Temp_AvailableGoods.СкладНазначения) T4
Group by 
	T4.НоменклатураСсылка,
	T4.СкладНазначения
OPTION (KEEP PLAN, KEEPFIXED PLAN);

Select
    Top 1 
	Max(#Temp_ClosestDatesByGoods.БлижайшаяДата) AS DateAvailable, 
СкладНазначения AS СкладНазначения
Into #Temp_DateAvailable
from #Temp_ClosestDatesByGoods With (NOLOCK)
Group by СкладНазначения
Order by DateAvailable ASC
OPTION (KEEP PLAN, KEEPFIXED PLAN);
/*Тут закончился процесс оптимальной даты. Склад назначения нужен чтоб потом правильную ГП выбрать*/

/*Интервалы для ПВЗ*/
WITH Tdate(date, СкладНазначения) AS (
    /*Это получение списка дат интервалов после даты окончания расчета*/
    SELECT         
		CAST(CAST(#Temp_DateAvailable.DateAvailable  AS DATE) AS DATETIME), 		
		#Temp_DateAvailable.СкладНазначения
	From #Temp_DateAvailable
	Where #Temp_DateAvailable.СкладНазначения in (select СкладСсылка From #Temp_PickupPoints)
    UNION
    ALL
    SELECT 
        DateAdd(day, 1, Tdate.date),
		#Temp_DateAvailable.СкладНазначения
    FROM
        Tdate
		Inner Join #Temp_DateAvailable 
		ON Tdate.date < DateAdd(DAY, @P_DaysToShow, CAST(CAST(#Temp_DateAvailable.DateAvailable  AS DATE) AS DATETIME))
		AND Tdate.СкладНазначения = #Temp_DateAvailable.СкладНазначения
		AND #Temp_DateAvailable.СкладНазначения in (select СкладСсылка From #Temp_PickupPoints)
)
SELECT	
	CASE 
	WHEN 
		DATEADD(
			SECOND,
			CAST(
				DATEDIFF(SECOND, @P_EmptyDate, isNull(ПВЗИзмененияГрафикаРаботы._Fld27057, ПВЗГрафикРаботы._Fld23617)) AS NUMERIC(12)
			),
			date
		) < #Temp_DateAvailable.DateAvailable 
		then #Temp_DateAvailable.DateAvailable
	Else
		DATEADD(
			SECOND,
			CAST(
				DATEDIFF(SECOND, @P_EmptyDate, isNull(ПВЗИзмененияГрафикаРаботы._Fld27057, ПВЗГрафикРаботы._Fld23617)) AS NUMERIC(12)
			),
			date
		)
	End As ВремяНачала,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, isNull(ПВЗИзмененияГрафикаРаботы._Fld27058, ПВЗГрафикРаботы._Fld23618)) AS NUMERIC(12)
        ),
        date
    ) As ВремяОкончания
Into #Temp_AvailablePickUp
FROM
    #Temp_DateAvailable
		Inner Join Tdate On 
			#Temp_DateAvailable.СкладНазначения = Tdate.СкладНазначения
		Inner Join dbo._Reference226 Склады ON Склады._IDRRef = #Temp_DateAvailable.СкладНазначения
			Inner Join _Reference23612 On Склады._Fld23620RRef = _Reference23612._IDRRef
				Left Join _Reference23612_VT23613 As ПВЗГрафикРаботы 
				On _Reference23612._IDRRef = _Reference23612_IDRRef
				AND (case when @@DATEFIRST = 1 then DATEPART ( dw , Tdate.date ) when DATEPART ( dw , Tdate.date ) = 1 then 7 else DATEPART ( dw , Tdate.date ) -1 END) = ПВЗГрафикРаботы._Fld23615
		Left Join _Reference23612_VT27054 As ПВЗИзмененияГрафикаРаботы 
				On _Reference23612._IDRRef = ПВЗИзмененияГрафикаРаботы._Reference23612_IDRRef
				AND Tdate.date = ПВЗИзмененияГрафикаРаботы._Fld27056
		WHERE 
			case 
			when ПВЗИзмененияГрафикаРаботы._Reference23612_IDRRef is not null
				then ПВЗИзмененияГрафикаРаботы._Fld27059
			when ПВЗГрафикРаботы._Reference23612_IDRRef is not Null 
				then ПВЗГрафикРаботы._Fld25265 
			else 0 --не найдено ни графика ни изменения графика  
			end = 0x00  -- не выходной
		AND DATEADD(
			SECOND,
			CAST(
            DATEDIFF(SECOND, @P_EmptyDate, isNull(ПВЗИзмененияГрафикаРаботы._Fld27058, ПВЗГрафикРаботы._Fld23618)) AS NUMERIC(12)
			),
			Tdate.date) > #Temp_DateAvailable.DateAvailable
OPTION (KEEP PLAN, KEEPFIXED PLAN);
/*Конец интервалов для ПВЗ*/

/*Мощности доставки*/
SELECT
    CAST(
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25107
                ELSE -(МощностиДоставки._Fld25107)
            END
        ) AS NUMERIC(16, 3)
    ) AS МассаОборот,
    CAST(
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25108
                ELSE -(МощностиДоставки._Fld25108)
            END
        ) AS NUMERIC(16, 3)
    ) AS ОбъемОборот,
    CAST(
        SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25201
                ELSE -(МощностиДоставки._Fld25201)
            END
        ) AS NUMERIC(16, 2)
    ) AS ВремяНаОбслуживаниеОборот,
	CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME) AS Дата
Into #Temp_DeliveryPower
FROM
    dbo._AccumRg25104 МощностиДоставки With (READCOMMITTED),
	#Temp_Size With (NOLOCK),
	#Temp_TimeService With (NOLOCK)
WHERE
    МощностиДоставки._Period >= @P_DateTimePeriodBegin
    AND МощностиДоставки._Period <= @P_DateTimePeriodEnd
	AND МощностиДоставки._Fld25105RRef IN (Select ЗонаДоставкиРодительСсылка From  #Temp_GeoData)
GROUP BY
	CAST(CAST(МощностиДоставки._Period  AS DATE) AS DATETIME),
	#Temp_Size.Вес,
	#Temp_Size.Объем,
	#Temp_TimeService.ВремяВыполнения
Having 
	SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25107
                ELSE -(МощностиДоставки._Fld25107)
            END
        ) > #Temp_Size.Вес
	AND 
	SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25108
                ELSE -(МощностиДоставки._Fld25108)
            END
        ) > #Temp_Size.Объем
	And 
	SUM(
            CASE
                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25201
                ELSE -(МощностиДоставки._Fld25201)
            END
        ) > #Temp_TimeService.ВремяВыполнения	
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-10T00:00:00',@P_DateTimePeriodEnd='4021-07-24T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);

/*Тут начинаются интервалы, которые рассчитанные*/
Select Distinct
	Case 
		When DATEPART(MINUTE,ГрафикПланирования._Fld23333) > 0 
		Then DATEADD(HOUR,1,ГрафикПланирования._Fld23333) 
		else ГрафикПланирования._Fld23333 
	End As ВремяВыезда,
	ГрафикПланирования._Fld23321 AS Дата,
	ГрафикПланирования._Fld23322RRef AS ГруппаПланирования
Into #Temp_CourierDepartureDates
From 
	dbo._InfoRg23320 AS ГрафикПланирования With (READCOMMITTED)
	INNER JOIN #Temp_PlanningGroups T2 With (NOLOCK) ON (ГрафикПланирования._Fld23322RRef = T2.ГруппаПланирования) 
Where ГрафикПланирования._Fld23321 BETWEEN @P_DateTimePeriodBegin AND @P_DateTimePeriodEnd 
	AND ГрафикПланирования._Fld23333 > @P_DateTimeNow
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-20T00:00:00',@P_DateTimePeriodEnd='4021-07-24T00:00:00',@P_DateTimeNow='4021-07-20T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);

SELECT
    T5._Period AS Период,
    T5._Fld25112RRef As ГруппаПланирования, 
	T5._Fld25111RRef As Геозона,
	T2.Основная AS Приоритет,
	T5._Fld25202 As ВремяНачалаНачальное,
	T5._Fld25203 As ВремяОкончанияНачальное,
    DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T5._Fld25202) AS NUMERIC(12)
        ),
        T5._Period
    ) As ВремяНачала,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T5._Fld25203) AS NUMERIC(12)
        ),
        T5._Period
    ) AS ВремяОкончания,
	SUM(
                CASE
                    WHEN (T5._RecordKind = 0.0) THEN T5._Fld25113
                    ELSE -(T5._Fld25113)
                END
            ) AS КоличествоЗаказовЗаИнтервалВремени
into #Temp_IntervalsAll_old
FROM
    dbo._AccumRg25110 T5 With (READCOMMITTED)
	INNER JOIN #Temp_PlanningGroups T2 With (NOLOCK) ON (T5._Fld25112RRef = T2.ГруппаПланирования)
	AND T2.Склад IN (select СкладНазначения From #Temp_DateAvailable)
WHERE
    T5._Period >= @P_DateTimePeriodBegin --begin +2
    AND T5._Period <= @P_DateTimePeriodEnd --end
    AND T5._Fld25111RRef in (Select Геозона From #Temp_GeoData) 
	AND T5._Period IN (Select Дата From #Temp_DeliveryPower)
GROUP BY
    T5._Period,
    T5._Fld25112RRef,
    T5._Fld25111RRef,
    T5._Fld25202,
	T5._Fld25203,
	T2.Основная
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
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-20T00:00:00',@P_DateTimePeriodEnd='4021-07-24T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);
;

Select Distinct
	ВременныеИнтервалы.Период AS Период,
	ВременныеИнтервалы.Геозона AS Геозона,
	ВременныеИнтервалы.ГруппаПланирования AS ГруппаПланирования,
	ВременныеИнтервалы.ВремяНачалаНачальное AS ВремяНачалаНачальное,
	ВременныеИнтервалы.ВремяОкончанияНачальное AS ВремяОкончанияНачальное,
	ВременныеИнтервалы.КоличествоЗаказовЗаИнтервалВремени AS КоличествоЗаказовЗаИнтервалВремени,
	ВременныеИнтервалы.ВремяНачала AS ВремяНачала,
	ВременныеИнтервалы.ВремяОкончания AS ВремяОкончания,
	ВременныеИнтервалы.Приоритет
Into #Temp_IntervalsAll
From
	#Temp_IntervalsAll_old AS ВременныеИнтервалы
		Inner Join #Temp_CourierDepartureDates AS ВТ_ГрафикПланирования
		ON DATEPART(HOUR, ВТ_ГрафикПланирования.ВремяВыезда) <= DATEPART(HOUR, ВременныеИнтервалы.ВремяНачалаНачальное)
		AND ВременныеИнтервалы.ГруппаПланирования = ВТ_ГрафикПланирования.ГруппаПланирования
	    AND ВременныеИнтервалы.Период = ВТ_ГрафикПланирования.Дата
OPTION (KEEP PLAN, KEEPFIXED PLAN);

select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) As ВремяНачала,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25129) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) AS ВремяОкончания,
	Sum(#Temp_IntervalsAll.КоличествоЗаказовЗаИнтервалВремени) AS КоличествоЗаказовЗаИнтервалВремени,
    Case when DATEPART(hour,ГеоЗонаВременныеИнтервалы._Fld25128) = 9 then 1 else 0 End AS Стимулировать,
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
	--T2._Fld25137,
	#Temp_IntervalsAll.Приоритет
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-20T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);

INsert into #Temp_Intervals
select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) As ВремяНачала,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25129) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) AS ВремяОкончания,
	Sum(#Temp_IntervalsAll.КоличествоЗаказовЗаИнтервалВремени) AS КоличествоЗаказовЗаИнтервалВремени,
    Case when DATEPART(hour,ГеоЗонаВременныеИнтервалы._Fld25128) = 9 then 1 else 0 End AS Стимулировать,
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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-20T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);

INsert into #Temp_Intervals
select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) As ВремяНачала,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25129) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.Период
    ) AS ВремяОкончания,
	Sum(#Temp_IntervalsAll.КоличествоЗаказовЗаИнтервалВремени) AS КоличествоЗаказовЗаИнтервалВремени,
    Case when DATEPART(hour,ГеоЗонаВременныеИнтервалы._Fld25128) = 9 then 1 else 0 End AS Стимулировать,
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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-20T00:00:00',@P_DateTimePeriodEnd='4021-07-24T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);

select Период, Max(Приоритет) AS Приоритет into #Temp_PlanningGroupPriority from #Temp_Intervals Group by Период;
/*Выше закончились рассчитанные интервалы*/

WITH T(date) AS (
    /*Это получение списка дат интервалов после даты окончания расчета*/
    SELECT
        Case When @P_DateTimePeriodEnd >= CAST(CAST(#Temp_DateAvailable.DateAvailable  AS DATE) AS DATETIME) Then
		DateAdd(day, 1,
		@P_DateTimePeriodEnd
		)
		else 
		CAST(CAST(#Temp_DateAvailable.DateAvailable  AS DATE) AS DATETIME) 
		End
	From #Temp_DateAvailable
    UNION
    ALL
    SELECT
        DateAdd(day, 1, T.date)
    FROM
        T
		Inner Join #Temp_DateAvailable 
		ON T.date < DateAdd(DAY, @P_DaysToShow, CAST(CAST(#Temp_DateAvailable.DateAvailable  AS DATE) AS DATETIME)) 
)
/*Тут мы выбираем даты из регистра*/
select 
	#Temp_Intervals.ВремяНачала As ВремяНачала,
	#Temp_Intervals.ВремяОкончания As ВремяОкончания,
	SUM(
	#Temp_Intervals.КоличествоЗаказовЗаИнтервалВремени
	) 
	AS КоличествоЗаказовЗаИнтервалВремени,
    #Temp_Intervals.Стимулировать 
Into #Temp_IntervalsWithOutShifting
From
#Temp_Intervals With (NOLOCK)
Inner Join #Temp_DateAvailable With (NOLOCK) 
    On #Temp_Intervals.ВремяНачала >= #Temp_DateAvailable.DateAvailable
Inner Join #Temp_TimeService With (NOLOCK) On 1=1
Inner Join #Temp_PlanningGroupPriority With (NOLOCK) ON #Temp_Intervals.Период = #Temp_PlanningGroupPriority.Период AND #Temp_Intervals.Приоритет = #Temp_PlanningGroupPriority.Приоритет
Where #Temp_Intervals.Период >= DATEADD(DAY, @P_Credit, @P_DateTimePeriodBegin) -- для кредита возвращаем даты начиная со следующего дня 
Group By 
	#Temp_Intervals.ВремяНачала,
	#Temp_Intervals.ВремяОкончания,
	#Temp_Intervals.Период,
	#Temp_TimeService.ВремяВыполнения,
    #Temp_Intervals.Стимулировать
Having SUM(#Temp_Intervals.КоличествоЗаказовЗаИнтервалВремени) > #Temp_TimeService.ВремяВыполнения

Union
All
/*А тут мы выбираем даты где логисты еще не рассчитали*/
SELECT
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        date
    ) As ВремяНачала,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25129) AS NUMERIC(12)
        ),
        date
    ) As ВремяОкончания,
	0 AS КоличествоЗаказовЗаИнтервалВремени,
    Case when DATEPART(hour,ГеоЗонаВременныеИнтервалы._Fld25128) = 9 then 1 else 0 End AS Стимулировать
FROM
    T 
	Inner Join _Reference114_VT25126 AS ГеоЗонаВременныеИнтервалы  With (NOLOCK) On ГеоЗонаВременныеИнтервалы._Reference114_IDRRef In (Select Геозона From #Temp_GeoData)
	Inner Join #Temp_DateAvailable On DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ГеоЗонаВременныеИнтервалы._Fld25128) AS NUMERIC(12)
        ),
        date
    ) >= #Temp_DateAvailable.DateAvailable 
UNION ALL
Select 
	#Temp_AvailablePickUp.ВремяНачала,
	#Temp_AvailablePickUp.ВремяОкончания,
    0,
	0
From #Temp_AvailablePickUp
--Order by ВремяНачала
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-07-20T00:00:00',@P_DateTimePeriodEnd='4021-07-24T00:00:00'), KEEP PLAN, KEEPFIXED PLAN);
;

Select 
	IntervalsWithOutShifting.ВремяНачала
INTO #Temp_UnavailableDates
From #Temp_Goods as TempGoods
inner join dbo._InfoRg28348 as ПрослеживаемыеТоварныеКатегории WITH(NOLOCK)
		on 1 = @P_ApplyShifting -- это будет значение ГП ПрименятьСмещениеДоступностиПрослеживаемыхМаркируемыхТоваров
			and ПрослеживаемыеТоварныеКатегории._Fld28349RRef = TempGoods.ТоварнаяКатегорияСсылка
			and @P_DateTimeNow <= DateAdd(DAY, @P_DaysToShift, ПрослеживаемыеТоварныеКатегории._Period)  -- количество дней будет из ГП
inner join #Temp_IntervalsWithOutShifting as IntervalsWithOutShifting
		on IntervalsWithOutShifting.ВремяНачала between ПрослеживаемыеТоварныеКатегории._period AND DateAdd(DAY, @P_DaysToShift, ПрослеживаемыеТоварныеКатегории._Period)
OPTION (KEEP PLAN, KEEPFIXED PLAN);

select IntervalsWithOutShifting.* 
from #Temp_IntervalsWithOutShifting as IntervalsWithOutShifting  
left join #Temp_UnavailableDates as UnavailableDates 
	on IntervalsWithOutShifting.ВремяНачала = UnavailableDates.ВремяНачала
where 
	UnavailableDates.ВремяНачала is NULL
Order by ВремяНачала
OPTION (KEEP PLAN, KEEPFIXED PLAN);

Drop table #Temp_GeoData
Drop table #Temp_Goods
Drop table #Temp_Dimensions
Drop table #Temp_Size
Drop Table #Temp_Weight
Drop Table #Temp_TimeByOrders
Drop Table #Temp_Time1
Drop Table #Temp_TimeService
Drop table #Temp_Remains
Drop table #Temp_WarehouseDates
Drop table #Temp_MinimumWarehouseDates
Drop table #Temp_Sources
Drop table #Temp_AvailableGoods
DROP TABLE #Temp_SourcesWithPrices
DROP TABLE #Temp_BestPriceAfterClosestDate
DROP TABLE #Temp_SourcesCorrectedDate
DROP TABLE #Temp_ClosestDatesByGoods
DROP TABLE #Temp_DateAvailable
DROP TABLE #Temp_DeliveryPower
Drop TABLE #Temp_PlanningGroups
Drop TABLE #Temp_Intervals
Drop Table #Temp_AvailablePickUp
Drop Table #Temp_IntervalsAll
Drop Table #Temp_PickupPoints
Drop Table #Temp_PlanningGroupPriority
DROP Table #Temp_CourierDepartureDates
Drop Table #Temp_IntervalsAll_old
Drop Table #Temp_OrderInfo
Drop Table #Temp_GoodsRaw
Drop Table #Temp_GoodsOrder
Drop Table #Temp_IntervalsWithOutShifting
DROP Table #Temp_UnavailableDates