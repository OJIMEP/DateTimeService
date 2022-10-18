namespace DateTimeService.Data
{
    public class Queries
    {

        public const string CreateTableGoodsRawCreate = @"Create Table #Temp_GoodsRaw   
(	
	Article nvarchar(20), 
	code nvarchar(20), 
    PickupPoint nvarchar(100),
    quantity int 
)
;";

        public const string CreateTableGoodsRawInsert = @"
INSERT INTO 
	#Temp_GoodsRaw ( 
		Article, code, PickupPoint, quantity 
	)
VALUES
	{0}
	OPTION (KEEP PLAN, KEEPFIXED PLAN)
;";

        public const string IntervalList = @"
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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
    #Temp_OrderInfo.ПВЗСсылка AS Склад,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
{0}
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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	0x00000000000000000000000000000000 AS Склад,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	0x00000000000000000000000000000000,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	T1.Склад,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	T1.Склад,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='{1}'),KEEP PLAN, KEEPFIXED PLAN);

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
    dbo._InfoRg23830 T1 With (READCOMMITTED{6}) 
    Inner Join SourceWarehouses On T1._Fld23831RRef = SourceWarehouses.СкладИсточника	
WHERE
	T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData UNION ALL Select СкладСсылка From #Temp_PickupPoints)
		AND	T1._Fld23832 BETWEEN @P_DateTimeNow AND DateAdd(DAY,6,@P_DateTimeNow)
GROUP BY T1._Fld23831RRef,
T1._Fld23833RRef
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='{1}'), KEEP PLAN, KEEPFIXED PLAN);


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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
    1 AS ЭтоСклад,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    --T5.ДатаПрибытия,
    2,
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
    0,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    --T7.ДатаПрибытия,
    3,
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
    0,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	CASE 
		WHEN Товары.Склад = векРезервированиеТоваров._Fld21410_RRRef
			THEN 1
		ELSE 0
	END,
    -- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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

-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
With TempSourcesGrouped AS
(
Select
	T1.НоменклатураСсылка AS НоменклатураСсылка,
	Sum(T1.Количество) AS Количество
From
	#Temp_Sources T1
Where T1.ЭтоСклад = 1
Group by
	T1.НоменклатураСсылка
)
Select
	T1.НоменклатураСсылка,
	min(Case when T1.Количество <= isNull(T2.Количество, 0) Then 1 Else 0 End) As ОстаткаДостаточно
Into #Temp_StockSourcesAvailable
From #Temp_Goods T1
	left join TempSourcesGrouped T2
	on T1.НоменклатураСсылка = T2.НоменклатураСсылка
Where @P_StockPriority = 1
Group by
    T1.НоменклатураСсылка
Having 
    min(Case when T1.Количество <= isNull(T2.Количество, 0) Then 1 Else 0 End) = 1;
-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918

With TempSourcesGrouped AS
(
Select
	T1.НоменклатураСсылка AS НоменклатураСсылка,
	-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	T1.ЭтоСклад,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
	Sum(T1.Количество) AS Количество,
	T1.ДатаДоступности AS ДатаДоступности,
	T1.СкладНазначения AS СкладНазначения
From
	#Temp_Sources T1	
Group by
	T1.НоменклатураСсылка,
	-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	T1.ЭтоСклад,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
        -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
		Inner Join #Temp_StockSourcesAvailable
		On @P_StockPriority = 1
			AND Источники1.НоменклатураСсылка = #Temp_StockSourcesAvailable.НоменклатураСсылка
			AND Источники1.ЭтоСклад = 1
		-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
Group by
	Источники1.НоменклатураСсылка,
	Источники1.ДатаДоступности,
	Источники1.СкладНазначения
-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
Union all

Select
	Источники1.НоменклатураСсылка AS Номенклатура,
	Источники1.СкладНазначения AS СкладНазначения,
	Источники1.ДатаДоступности AS ДатаДоступности,
	Sum(Источник2.Количество) AS Количество
From
	TempSourcesGrouped AS Источники1
		Left Join TempSourcesGrouped AS Источник2
		On Источники1.НоменклатураСсылка = Источник2.НоменклатураСсылка
		AND Источники1.СкладНазначения = Источник2.СкладНазначения
			AND Источники1.ДатаДоступности >= Источник2.ДатаДоступности	
		Left Join #Temp_StockSourcesAvailable
		On Источники1.НоменклатураСсылка = #Temp_StockSourcesAvailable.НоменклатураСсылка
Where @P_StockPriority = 0
    Or #Temp_StockSourcesAvailable.ОстаткаДостаточно is null
Group by
	Источники1.НоменклатураСсылка,
	Источники1.ДатаДоступности,
	Источники1.СкладНазначения
-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
OPTION (KEEP PLAN, KEEPFIXED PLAN, maxdop 4);

With Temp_SupplyDocs AS
(
SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.ДатаДоступности,
    DATEADD(DAY, {4}, T1.ДатаДоступности) AS ДатаДоступностиПлюс, --это параметр КоличествоДнейАнализа
    MIN(T1.Цена) AS ЦенаИсточника,
    MIN(T1.Цена / 100.0 * (100 - {5})) AS ЦенаИсточникаМинус --это параметр ПроцентДнейАнализа
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
WHERE
    T1.Цена <> 0
    AND T1.Источник_RTRef = 0x00000153    
GROUP BY
    T1.НоменклатураСсылка,
    T1.ДатаДоступности,
    T1.СкладНазначения,
    DATEADD(DAY, {4}, T1.ДатаДоступности)--это параметр КоличествоДнейАнализа
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
    INNER HASH JOIN Temp_SupplyDocs T2 WITH(NOLOCK)
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
        T3.ДатаДоступности <= DATEADD(DAY, {4}, T3.БлижайшаяДата) --это параметр КоличествоДнейАнализа
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
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'),KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}',@P_DateTimeNow='{1}'),KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'), KEEP PLAN, KEEPFIXED PLAN);
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
    Case when ГеоЗонаВременныеИнтервалы._Fld27342 = 0x01 then 1 else 0 End AS Стимулировать,
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
	#Temp_IntervalsAll.Приоритет,
    ГеоЗонаВременныеИнтервалы._Fld27342
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}'), KEEP PLAN, KEEPFIXED PLAN);

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
Case when ГеоЗонаВременныеИнтервалы._Fld27342 = 0x01 then 1 else 0 End AS Стимулировать,
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
	#Temp_IntervalsAll.Приоритет,
    ГеоЗонаВременныеИнтервалы._Fld27342
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}'), KEEP PLAN, KEEPFIXED PLAN); 

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
Case when ГеоЗонаВременныеИнтервалы._Fld27342 = 0x01 then 1 else 0 End AS Стимулировать,
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
	#Temp_IntervalsAll.Приоритет,
    ГеоЗонаВременныеИнтервалы._Fld27342
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'), KEEP PLAN, KEEPFIXED PLAN);

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
    Case when ГеоЗонаВременныеИнтервалы._Fld27342 = 0x01 then 1 else 0 End AS Стимулировать
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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'), KEEP PLAN, KEEPFIXED PLAN);

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
";

        public const string AvailableDate1 = @"
Select 
	Склады._IDRRef AS СкладСсылка,
	Склады._Fld19544 AS ERPКодСклада
Into #Temp_PickupPoints
From 
	dbo._Reference226 Склады 
Where Склады._Fld19544 in({0})
 

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
	ЦеныТолькоПрайсы._Fld21410_TYPE AS Регистратор_TYPE,
    ЦеныТолькоПрайсы._Fld21410_RTRef AS Регистратор_RTRef,
    ЦеныТолькоПрайсы._Fld21410_RRRef AS Регистратор_RRRef,
    T2._Fld23568RRef AS СкладИсточника,
    T2._Fld21424 AS ДатаСобытия,
	Цены._Fld21442 * ExchangeRates.Курс / ExchangeRates.Кратность AS Цена,
    SUM(T2._Fld21411) - SUM(T2._Fld21412) AS Количество
Into #Temp_Remains
FROM
    dbo._AccumRgT21444 T2 With (READCOMMITTED)
	Left Join _AccumRg21407 ЦеныТолькоПрайсы With (READCOMMITTED)
		Inner Join Temp_ExchangeRates With (NOLOCK)
			On ЦеныТолькоПрайсы._Fld21443RRef = Temp_ExchangeRates.Валюта 
		On T2._Fld21408RRef = ЦеныТолькоПрайсы._Fld21408RRef
		AND T2._Fld21410_RTRef = 0x00000153
		AND ЦеныТолькоПрайсы._Fld21410_RTRef = 0x00000153  --Цены.Регистратор ССЫЛКА Документ.мегапрайсРегистрацияПрайса
		AND T2._Fld21410_RRRef = ЦеныТолькоПрайсы._Fld21410_RRRef
        And (ЦеныТолькоПрайсы._Fld21982<>0 
		AND ЦеныТолькоПрайсы._Fld21442 * Temp_ExchangeRates.Курс / Temp_ExchangeRates.Кратность >= ЦеныТолькоПрайсы._Fld21982 OR ЦеныТолькоПрайсы._Fld21411 >= ЦеныТолькоПрайсы._Fld21616)
		And ЦеныТолькоПрайсы._Fld21408RRef IN(SELECT
                НоменклатураСсылка
            FROM
                #Temp_Goods)
	Left Join _AccumRg21407 Цены With (READCOMMITTED)
		Inner Join Temp_ExchangeRates ExchangeRates With (NOLOCK)
			On Цены._Fld21443RRef = ExchangeRates.Валюта 
		On T2._Fld21408RRef = Цены._Fld21408RRef
		AND T2._Fld21410_RTRef IN(0x00000141,0x00000153)
		AND Цены._Fld21410_RTRef IN(0x00000141,0x00000153)  --Цены.Регистратор ССЫЛКА Документ.мегапрайсРегистрацияПрайса, ЗаказПоставщику
		AND T2._Fld21410_RRRef = Цены._Fld21410_RRRef
        And (Цены._Fld21982<>0 
		AND Цены._Fld21410_RTRef = 0x00000141 OR (Цены._Fld21442 * ExchangeRates.Курс / ExchangeRates.Кратность >= Цены._Fld21982 OR Цены._Fld21411 >= Цены._Fld21616))
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
	ЦеныТолькоПрайсы._Fld21410_TYPE,
	ЦеныТолькоПрайсы._Fld21410_RTRef,
	ЦеныТолькоПрайсы._Fld21410_RRRef,
    T2._Fld23568RRef,
    T2._Fld21424,
	Цены._Fld21442 * ExchangeRates.Курс / ExchangeRates.Кратность
HAVING
    (SUM(T2._Fld21412) <> 0.0
    OR SUM(T2._Fld21411) <> 0.0)
	AND SUM(T2._Fld21411) - SUM(T2._Fld21412) > 0.0
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='{1}'),KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate2MinimumWarehousesBasic = @"With SourceWarehouses AS
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
    dbo._InfoRg23830 T1 With (READCOMMITTED{7})
    Inner Join SourceWarehouses On T1._Fld23831RRef = SourceWarehouses.СкладИсточника
WHERE
    T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData UNION ALL Select СкладСсылка From #Temp_PickupPoints)
		AND	T1._Fld23832 BETWEEN @P_DateTimeNow AND DateAdd(DAY,6,@P_DateTimeNow)
GROUP BY T1._Fld23831RRef,
T1._Fld23833RRef
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='{1}'), KEEP PLAN, KEEPFIXED PLAN);";
        public const string AvailableDate2MinimumWarehousesCustom = @"With SourceWarehouses AS
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
    [dbo].[WarehouseDatesAggregate] T1 With (READCOMMITTED{7})
    Inner Join SourceWarehouses On T1.СкладИсточника = SourceWarehouses.СкладИсточника
WHERE
    T1.СкладНазначения IN (Select СкладСсылка From #Temp_GeoData UNION ALL Select СкладСсылка From #Temp_PickupPoints)
		AND	T1.ДатаСобытия BETWEEN @P_DateTimeNow AND DateAdd(DAY,6,@P_DateTimeNow)
GROUP BY T1.СкладИсточника,
T1.СкладНазначения
OPTION (OPTIMIZE FOR (@P_DateTimeNow='{1}'), KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate3 = @"SELECT
    T1.НоменклатураСсылка,
    T1.Количество,
    T1.Источник_TYPE,
    T1.Источник_RTRef,
    T1.Источник_RRRef,
    T1.СкладИсточника,
    T1.Цена,
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
    T4.Цена,
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
    T6.Цена,
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
OPTION (KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate4SourcesWithPrices = @"
SELECT
    T1.НоменклатураСсылка,
    T1.Источник_TYPE,
    T1.Источник_RTRef,
    T1.Источник_RRRef,
    T1.СкладИсточника,
    T1.СкладНазначения,
    T1.ДатаСобытия,
    T1.ДатаДоступности,
	T1.Цена AS Цена
Into #Temp_SourcesWithPrices
FROM
    #Temp_Sources T1 WITH(NOLOCK)
Where  T1.Цена <> 0
OPTION (KEEP PLAN, KEEPFIXED PLAN, maxdop 2);";

        
        public const string AvailableDate5 = @"

With Temp_SupplyDocs AS
(
SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.ДатаДоступности,
    DATEADD(DAY, {4}, T1.ДатаДоступности) AS ДатаДоступностиПлюс, --это параметр КоличествоДнейАнализа
    MIN(T1.Цена) AS ЦенаИсточника,
    MIN(T1.Цена / 100.0 * (100 - {5})) AS ЦенаИсточникаМинус --это параметр ПроцентДнейАнализа
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
WHERE 
    T1.Источник_RTRef = 0x00000153    
GROUP BY
    T1.НоменклатураСсылка,
    T1.ДатаДоступности,
    T1.СкладНазначения,
    DATEADD(DAY, {4}, T1.ДатаДоступности)
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
    INNER HASH JOIN Temp_SupplyDocs T2 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
    AND (T1.ДатаДоступности >= T2.ДатаДоступности)
    AND (T1.ДатаДоступности <= T2.ДатаДоступностиПлюс)
    AND (T1.Цена <= T2.ЦенаИсточникаМинус)
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
			AND T4.ДатаДоступности <= DATEADD(DAY, {4}, T5.ДатаДоступности)
Group by T4.НоменклатураСсылка, T4.СкладНазначения
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);


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
		on 1 = @P_ApplyShifting 
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
		Inner {6} JOIN #Temp_PickupWorkingHours
		On #Temp_PickupWorkingHours.СкладНазначения = #Temp_ShipmentDatesPickUp.СкладНазначения
        And #Temp_PickupWorkingHours.ВремяОкончания > #Temp_ShipmentDatesPickUp.ДатаСоСклада
Group by
	#Temp_ShipmentDatesPickUp.НоменклатураСсылка,
	#Temp_ShipmentDatesPickUp.article,
	#Temp_ShipmentDatesPickUp.code
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate6IntervalsBasic = @"With PlanningGroups AS(
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
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'),KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate6IntervalsCustom = @"With PlanningGroups AS(
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
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'),KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate7 = @"
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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}'), KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}'), KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'), KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDate8DeliveryPowerBasic = @"With Temp_DeliveryPower AS
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
), ";

        public const string AvailableDate8DeliveryPowerCustom = @"With Temp_DeliveryPower AS
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
),";

        public const string AvailableDate9 = @"Temp_PlanningGroupPriority AS
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
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimePeriodBegin='{2}',@P_DateTimePeriodEnd='{3}'),KEEP PLAN, KEEPFIXED PLAN);

Select 
	IsNull(#Temp_AvailableCourier.article,#Temp_AvailablePickUp.article) AS article,
	IsNull(#Temp_AvailableCourier.code,#Temp_AvailablePickUp.code) AS code,
	IsNull(#Temp_AvailableCourier.ДатаКурьерскойДоставки,@P_MaxDate) AS available_date_courier,
	IsNull(#Temp_AvailablePickUp.ВремяНачала,@P_MaxDate) AS available_date_self
From
	#Temp_AvailableCourier 
	FULL Join #Temp_AvailablePickUp 
		On #Temp_AvailableCourier.НоменклатураСсылка = #Temp_AvailablePickUp.НоменклатураСсылка";

        public const string AvailableDateWithCount1 = @"
Select 
	Склады._IDRRef AS СкладСсылка,
	Склады._Fld19544 AS ERPКодСклада
Into #Temp_PickupPoints
From 
	dbo._Reference226 Склады 
Where Склады._Fld19544 in({0})
 

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
    t1.quantity,
	value AS PickupPoint 
from #Temp_GoodsRaw t1
	cross apply 
		string_split(IsNull(t1.PickupPoint,'-'), ',')
Where t1.quantity > 0
)
Select 
	Номенклатура._IDRRef AS НоменклатураСсылка,
	Номенклатура._Code AS code,
	Номенклатура._Fld3480 AS article,
	Номенклатура._Fld3489RRef AS ЕдиницаИзмерения,
	Номенклатура._Fld3526RRef AS Габариты,
    T1.quantity AS Количество,
	#Temp_PickupPoints.СкладСсылка AS СкладПВЗСсылка,
	Упаковки._IDRRef AS УпаковкаСсылка,
	Упаковки._Fld6000 AS Вес,
	Упаковки._Fld6006 AS Объем,
	Упаковки._Fld6001 AS Высота,
	Упаковки._Fld6002 AS Глубина,
	Упаковки._Fld6009 AS Ширина,
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
    T1.quantity AS Количество,
	#Temp_PickupPoints.СкладСсылка,
	Упаковки._IDRRef AS УпаковкаСсылка,
	Упаковки._Fld6000 AS Вес,
	Упаковки._Fld6006 AS Объем,
	Упаковки._Fld6001 AS Высота,
	Упаковки._Fld6002 AS Глубина,
	Упаковки._Fld6009 AS Ширина,
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


/*Размеры для расчета габаритов*/
SELECT
T1.НоменклатураСсылка,
CAST(SUM((T1.Вес * T1.Количество)) AS NUMERIC(36, 6)) AS Вес,
CAST(SUM((T1.Объем * T1.Количество)) AS NUMERIC(38, 8)) AS Объем,
MAX(T1.Высота) AS Высота,
MAX(T1.Глубина) AS Глубина,
MAX(T1.Ширина) AS Ширина,
0x00000000000000000000000000000000  AS Габарит
Into #Temp_Size
FROM #Temp_GoodsBegin T1 WITH(NOLOCK)
Group By 
	T1.НоменклатураСсылка
OPTION (KEEP PLAN, KEEPFIXED PLAN);

/*Габарит общий*/
SELECT
    --TOP 1 
	T1.НоменклатураСсылка,
	CASE
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

Select 
	Номенклатура.НоменклатураСсылка AS НоменклатураСсылка,
	Номенклатура.article AS article,
	Номенклатура.code AS code,
	Номенклатура.СкладПВЗСсылка AS СкладСсылка,
	Номенклатура.УпаковкаСсылка AS УпаковкаСсылка,--Упаковки._IDRRef AS УпаковкаСсылка,
	Номенклатура.Количество As Количество,
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
    Inner Join #Temp_Dimensions With (NOLOCK) On Номенклатура.НоменклатураСсылка = #Temp_Dimensions.НоменклатураСсылка
	Left Join dbo._Reference23294 ГруппыПланирования With (NOLOCK)
		Inner Join dbo._Reference23294_VT23309 With (NOLOCK)
			on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
			and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
		On 
		ГруппыПланирования._Fld23302RRef IN (Select СкладСсылка From #Temp_GeoData) --склад
		AND ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
		AND ГруппыПланирования._Fld23301RRef = #Temp_Dimensions.Габарит --Номенклатура.Габариты OR (Номенклатура.Габариты = 0xAC2CBF86E693F63444670FFEB70264EE AND ГруппыПланирования._Fld23301RRef= 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) ) --габариты
		AND ГруппыПланирования._Marked = 0x00
        AND Номенклатура.СкладПВЗСсылка Is Null
UNION ALL
Select 
	Номенклатура.НоменклатураСсылка AS НоменклатураСсылка,
	Номенклатура.article AS article,
	Номенклатура.code AS code,
	Номенклатура.СкладПВЗСсылка AS СкладСсылка,
	Номенклатура.УпаковкаСсылка AS УпаковкаСсылка,--Упаковки._IDRRef AS УпаковкаСсылка,
	Номенклатура.Количество As Количество,
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
	Inner Join #Temp_Dimensions With (NOLOCK) On Номенклатура.НоменклатураСсылка = #Temp_Dimensions.НоменклатураСсылка
	Left Join dbo._Reference23294 ГруппыПланирования With (NOLOCK)
		Inner Join dbo._Reference23294_VT23309 With (NOLOCK)
			on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
			and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
		On 
		ГруппыПланирования._Fld23302RRef IN (Select СкладСсылка From #Temp_GeoData) --склад
		AND ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
		AND ГруппыПланирования._Fld23301RRef = #Temp_Dimensions.Габарит--(ГруппыПланирования._Fld23301RRef = Номенклатура.Габариты OR (Номенклатура.Габариты = 0xAC2CBF86E693F63444670FFEB70264EE AND ГруппыПланирования._Fld23301RRef= 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) ) --габариты
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
	ЦеныТолькоПрайсы._Fld21410_TYPE AS Регистратор_TYPE,
    ЦеныТолькоПрайсы._Fld21410_RTRef AS Регистратор_RTRef,
    ЦеныТолькоПрайсы._Fld21410_RRRef AS Регистратор_RRRef,
    T2._Fld23568RRef AS СкладИсточника,
    T2._Fld21424 AS ДатаСобытия,
	Цены._Fld21442 * ExchangeRates.Курс / ExchangeRates.Кратность AS Цена,
    SUM(T2._Fld21411) - SUM(T2._Fld21412) AS Количество
Into #Temp_Remains
FROM
    dbo._AccumRgT21444 T2 With (READCOMMITTED)
	Left Join _AccumRg21407 ЦеныТолькоПрайсы With (READCOMMITTED)
		Inner Join Temp_ExchangeRates With (NOLOCK)
			On ЦеныТолькоПрайсы._Fld21443RRef = Temp_ExchangeRates.Валюта 
		On T2._Fld21408RRef = ЦеныТолькоПрайсы._Fld21408RRef
		AND T2._Fld21410_RTRef = 0x00000153
		AND ЦеныТолькоПрайсы._Fld21410_RTRef = 0x00000153  --Цены.Регистратор ССЫЛКА Документ.мегапрайсРегистрацияПрайса
		AND T2._Fld21410_RRRef = ЦеныТолькоПрайсы._Fld21410_RRRef
        And (ЦеныТолькоПрайсы._Fld21982<>0 
		AND ЦеныТолькоПрайсы._Fld21442 * Temp_ExchangeRates.Курс / Temp_ExchangeRates.Кратность >= ЦеныТолькоПрайсы._Fld21982 OR ЦеныТолькоПрайсы._Fld21411 >= ЦеныТолькоПрайсы._Fld21616)
		And ЦеныТолькоПрайсы._Fld21408RRef IN(SELECT
                НоменклатураСсылка
            FROM
                #Temp_Goods)
	Left Join _AccumRg21407 Цены With (READCOMMITTED)
		Inner Join Temp_ExchangeRates ExchangeRates With (NOLOCK)
			On Цены._Fld21443RRef = ExchangeRates.Валюта 
		On T2._Fld21408RRef = Цены._Fld21408RRef
		AND T2._Fld21410_RTRef IN(0x00000141,0x00000153)
		AND Цены._Fld21410_RTRef IN(0x00000141,0x00000153)  --Цены.Регистратор ССЫЛКА Документ.мегапрайсРегистрацияПрайса, ЗаказПоставщику
		AND T2._Fld21410_RRRef = Цены._Fld21410_RRRef
        And (Цены._Fld21982<>0 
		AND Цены._Fld21410_RTRef = 0x00000141 OR (Цены._Fld21442 * ExchangeRates.Курс / ExchangeRates.Кратность >= Цены._Fld21982 OR Цены._Fld21411 >= Цены._Fld21616))
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
	ЦеныТолькоПрайсы._Fld21410_TYPE,
	ЦеныТолькоПрайсы._Fld21410_RTRef,
	ЦеныТолькоПрайсы._Fld21410_RRRef,
    T2._Fld23568RRef,
    T2._Fld21424,
	Цены._Fld21442 * ExchangeRates.Курс / ExchangeRates.Кратность
HAVING
    (SUM(T2._Fld21412) <> 0.0
    OR SUM(T2._Fld21411) <> 0.0)
	AND SUM(T2._Fld21411) - SUM(T2._Fld21412) > 0.0
OPTION (HASH GROUP, OPTIMIZE FOR (@P_DateTimeNow='{1}'),KEEP PLAN, KEEPFIXED PLAN);

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
OPTION (KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDateWithCount3 = @"SELECT
    T1.НоменклатураСсылка,
    T1.Количество,
    T1.Источник_TYPE,
    T1.Источник_RTRef,
    T1.Источник_RRRef,
    T1.СкладИсточника,
    T1.Цена,
    T1.ДатаСобытия,
    ISNULL(T3.ДатаПрибытия, T2.ДатаПрибытия) AS ДатаДоступности,
    1 AS ТипИсточника,
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	1 AS ЭтоСклад,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    T4.Цена,
    T4.ДатаСобытия,
    T5.ДатаПрибытия,
    2,
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	0,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
    T6.Цена,
    T6.ДатаСобытия,
    T7.ДатаПрибытия,
    3,
    -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	0,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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

-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
With TempSourcesGrouped AS
(
Select
	T1.НоменклатураСсылка AS НоменклатураСсылка,
	Sum(T1.Количество) AS Количество
From
	#Temp_Sources T1
Where T1.ЭтоСклад = 1
Group By
	T1.НоменклатураСсылка
)
Select
	T1.НоменклатураСсылка,
	min(Case when T1.Количество <= isNull(T2.Количество, 0) Then 1 Else 0 End) As ОстаткаДостаточно
Into #Temp_StockSourcesAvailable
From #Temp_Goods T1
	Left Join TempSourcesGrouped T2
	On T1.НоменклатураСсылка = T2.НоменклатураСсылка
Where @P_StockPriority = 1
Group By
    T1.НоменклатураСсылка
Having 
    min(Case when T1.Количество <= isNull(T2.Количество, 0) Then 1 Else 0 End) = 1;
-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918

With TempSourcesGrouped AS
(
Select
	T1.НоменклатураСсылка AS НоменклатураСсылка,
	Sum(T1.Количество) AS Количество,
	-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	T1.ЭтоСклад,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
	T1.ДатаДоступности AS ДатаДоступности,
	T1.СкладНазначения AS СкладНазначения
From
	#Temp_Sources T1	
Group by
	T1.НоменклатураСсылка,
	-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
	T1.ЭтоСклад,
	-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
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
        -- 21век.Левковский 17.10.2022 Старт DEV1C-67918
		Inner Join #Temp_StockSourcesAvailable
		On @P_StockPriority = 1
			AND Источники1.НоменклатураСсылка = #Temp_StockSourcesAvailable.НоменклатураСсылка
			AND Источники1.ЭтоСклад = 1
		-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
Group by
	Источники1.НоменклатураСсылка,
	Источники1.ДатаДоступности,
	Источники1.СкладНазначения

-- 21век.Левковский 17.10.2022 Старт DEV1C-67918
Union All

Select
	Источники1.НоменклатураСсылка AS Номенклатура,
	Источники1.СкладНазначения AS СкладНазначения,
	Источники1.ДатаДоступности AS ДатаДоступности,
	Sum(Источник2.Количество) AS Количество
From
	TempSourcesGrouped AS Источники1
		Left Join TempSourcesGrouped AS Источник2
		On Источники1.НоменклатураСсылка = Источник2.НоменклатураСсылка
		AND Источники1.СкладНазначения = Источник2.СкладНазначения
			AND Источники1.ДатаДоступности >= Источник2.ДатаДоступности	
		Left Join #Temp_StockSourcesAvailable
		On Источники1.НоменклатураСсылка = #Temp_StockSourcesAvailable.НоменклатураСсылка
Where @P_StockPriority = 0
    Or #Temp_StockSourcesAvailable.ОстаткаДостаточно is null
Group By
	Источники1.НоменклатураСсылка,
	Источники1.ДатаДоступности,
	Источники1.СкладНазначения
-- 21век.Левковский 17.10.2022 Финиш DEV1C-67918
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);";

        public const string AvailableDateWithCount5 = @"
With Temp_SupplyDocs AS
(
SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.ДатаДоступности,
    DATEADD(DAY, {4}, T1.ДатаДоступности) AS ДатаДоступностиПлюс, --это параметр КоличествоДнейАнализа
    MIN(T1.Цена) AS ЦенаИсточника,
    MIN(T1.Цена / 100.0 * (100 - {5})) AS ЦенаИсточникаМинус --это параметр ПроцентДнейАнализа
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
WHERE
    T1.Цена <> 0
    AND T1.Источник_RTRef = 0x00000153    
GROUP BY
    T1.НоменклатураСсылка,
    T1.ДатаДоступности,
    T1.СкладНазначения,
    DATEADD(DAY, {4}, T1.ДатаДоступности)
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
    INNER HASH JOIN Temp_SupplyDocs T2 WITH(NOLOCK)
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
			AND T4.ДатаДоступности <= DATEADD(DAY, {4}, T5.ДатаДоступности)
Group by T4.НоменклатураСсылка, T4.СкладНазначения
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);


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
    T1.Количество AS Количество,
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
    AND T1.Количество = 1
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
    T1.Количество AS Количество,
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
    AND T1.Количество = 1
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
    #Temp_AvailableGoods.СкладНазначения AS СкладНазначения,
    Min(#Temp_AvailableGoods.ДатаДоступности) AS БлижайшаяДата,
    T1.Количество AS Количество,
    T1.Вес,
    T1.Объем,
    T1.ТНВЭДСсылка,
    T1.ТоварнаяКатегорияСсылка,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя,
	T1.Приоритет,
	0 AS PickUp
FROM
    #Temp_Goods T1 WITH(NOLOCK)	
   Left Join #Temp_AvailableGoods With (NOLOCK) 
			On T1.НоменклатураСсылка = #Temp_AvailableGoods.Номенклатура
			AND T1.Количество <= #Temp_AvailableGoods.Количество
			AND #Temp_AvailableGoods.СкладНазначения IN (Select СкладСсылка From #Temp_GeoData)
Where 
	T1.СкладСсылка IS NULL
	And T1.ГруппаПланированияСклад = #Temp_AvailableGoods.СкладНазначения 
	AND T1.Количество > 1
GROUP BY
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
	#Temp_AvailableGoods.СкладНазначения,
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
    #Temp_AvailableGoods.СкладНазначения AS СкладНазначения,
    Min(#Temp_AvailableGoods.ДатаДоступности) AS БлижайшаяДата,
    T1.Количество AS Количество,
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
	 Left Join #Temp_AvailableGoods With (NOLOCK) 
		On T1.НоменклатураСсылка = #Temp_AvailableGoods.Номенклатура
		AND T1.Количество <= #Temp_AvailableGoods.Количество
		AND	T1.СкладСсылка = #Temp_AvailableGoods.СкладНазначения
Where 
	NOT T1.СкладСсылка IS NULL
	AND T1.Количество > 1
GROUP BY
    T1.НоменклатураСсылка,
	T1.article,
	T1.code,
	#Temp_AvailableGoods.СкладНазначения,
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
		on 1 = @P_ApplyShifting 
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
		Inner {6} JOIN #Temp_PickupWorkingHours
		On #Temp_PickupWorkingHours.ВремяОкончания > #Temp_ShipmentDatesPickUp.ДатаСоСклада
		And #Temp_PickupWorkingHours.СкладНазначения = #Temp_ShipmentDatesPickUp.СкладНазначения
Group by
	#Temp_ShipmentDatesPickUp.НоменклатураСсылка,
	#Temp_ShipmentDatesPickUp.article,
	#Temp_ShipmentDatesPickUp.code
OPTION (HASH GROUP, KEEP PLAN, KEEPFIXED PLAN);";

        public const string DatebaseBalancingReplicaFull = @"select datediff(ms, last_commit_time, getdate())
from [master].[sys].[dm_hadr_database_replica_states]";

        public const string DatebaseBalancingMain = @"select top (1) _IDRRef from dbo._Reference112";

        public const string DatebaseBalancingReplicaTables = @"Select TOP(1) _IDRRef FROM dbo._Reference99";

        public const string CheckAggregations = @"EXEC	[dbo].[spCheckAggregates]";

        public const string ClearCacheScriptDefault = @"dbcc freeproccache";

    }
}
