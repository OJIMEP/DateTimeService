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
 SET @P_Article5 = '6167903';
 SET @P_Article6 = '6167903';
 SET @P_Article7 = '380386';
 SET @P_Article8 = '358619';

 SET @P_Code1 = '00-00444697'; --коды дл€ уценки
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

 Set @P_CityCode = '17600'--'17030' --код адреса

 Set @P_DateTimeNow = '4021-05-13T12:28:00' 
 Set @P_DateTimePeriodBegin = '4021-05-13T00:00:00'
 Set @P_DateTimePeriodEnd = '4021-05-17T00:00:00'
 Set @P_TimeNow = '2001-01-01T12:28:00'
 Set @P_EmptyDate = '2001-01-01T00:00:00'
 Set @P_MaxDate = '5999-11-11T00:00:00'

Select
	IsNull(_Reference114_VT23370._Fld23372RRef,√еозона._Fld23104RRef) As —клад—сылка,
	«оныƒоставки._ParentIDRRef As «онаƒоставки–одитель—сылка,
	√еозона._IDRRef As √еозона
Into #Temp_GeoData
From dbo._Reference114 √еозона With (NOLOCK)
	Inner Join _Reference114_VT23370 With (NOLOCK)
	on _Reference114_VT23370._Reference114_IDRRef = √еозона._IDRRef
	Inner Join _Reference99 «оныƒоставки With (NOLOCK)
	on √еозона._Fld2847RRef = «оныƒоставки._IDRRef
where √еозона._IDRRef IN (
	Select Top 1 --по городу в геоадресе находим геозону
	√еојдрес._Fld2785RRef 
	From dbo._Reference112 √еојдрес With (NOLOCK)
	Where √еојдрес._Fld25552 = @P_CityCode)
OPTION (KEEP PLAN, KEEPFIXED PLAN)

DECLARE @Temp_GoodsRaw Table  
(	
	Article nvarchar(20), 
	code nvarchar(20), 
    PickupPoint nvarchar(10)
)

INSERT INTO 
	@Temp_GoodsRaw ( 
		Article, code, PickupPoint
	)
VALUES
	(@P_Article1,@P_Code1,@PickupPoint3),
	(@P_Article2,@P_Code2,@PickupPoint2),
	(@P_Article1,@P_Code1,NULL),
	(@P_Article3,@P_Code3,@PickupPoint3),
	('843414',NULL,NULL)
	--(@P3,3),
	--(@P5,4),
	--(@P6,3),
	--(@P7,2),
	--(@P8,1)
	;

Select 
	Ќоменклатура._IDRRef AS Ќоменклатура—сылка,
	—клады._IDRRef AS —кладѕ¬«—сылка
INTO #Temp_GoodsBegin
From
	@Temp_GoodsRaw T1
	Inner Join 	dbo._Reference149 Ќоменклатура With (NOLOCK) 
		ON T1.code is NULL and T1.Article = Ќоменклатура._Fld3480
	Left Join dbo._Reference226 —клады 
		ON T1.PickupPoint = —клады._Fld19544
union
Select 
	Ќоменклатура._IDRRef,
	—клады._IDRRef
From 
	@Temp_GoodsRaw T1
	Inner Join 	dbo._Reference149 Ќоменклатура With (NOLOCK) 
		ON T1.code is not NULL and T1.code = Ќоменклатура._Code
	Left Join dbo._Reference226 —клады 
		ON T1.PickupPoint = —клады._Fld19544
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

Select 
	Ќоменклатура._IDRRef AS Ќоменклатура—сылка,
	Ќоменклатура._Fld3480 AS article,
	Ќоменклатура._Code AS code,
	#Temp_GoodsBegin.—кладѕ¬«—сылка AS —клад—сылка,
	”паковки._IDRRef AS ”паковка—сылка,
	1 As  оличество,
	”паковки._Fld6000 AS ¬ес,
	”паковки._Fld6006 AS ќбъем,
	10 AS ¬рем€Ќаќбслуживание,
	IsNull(√руппыѕланировани€._IDRRef, 0x00000000000000000000000000000000) AS √руппаѕланировани€,
	IsNull(√руппыѕланировани€._Description, '') AS √руппаѕланировани€Ќаименование,
	IsNull(√руппыѕланировани€._Fld25519, @P_EmptyDate) AS √руппаѕланировани€ƒобавл€емое¬рем€
INTO #Temp_Goods
From 
	dbo._Reference149 Ќоменклатура With (NOLOCK)
	inner join #Temp_GoodsBegin on Ќоменклатура._IDRRef = #Temp_GoodsBegin.Ќоменклатура—сылка
	Inner Join dbo._Reference256 ”паковки With (NOLOCK)
		On 
		”паковки._OwnerID_TYPE = 0x08  
		AND ”паковки.[_OwnerID_RTRef] = 0x00000095
		AND Ќоменклатура._IDRRef = ”паковки._OwnerID_RRRef		
		And ”паковки._Fld6003RRef = Ќоменклатура._Fld3489RRef
		AND ”паковки._Marked = 0x00
	Left Join dbo._Reference23294 √руппыѕланировани€ With (NOLOCK)
		Inner Join dbo._Reference23294_VT23309 With (NOLOCK)
			on √руппыѕланировани€._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
			and _Reference23294_VT23309._Fld23311RRef in (Select «онаƒоставки–одитель—сылка From #Temp_GeoData)
		On 
		√руппыѕланировани€._Fld23302RRef IN (Select —клад—сылка From #Temp_GeoData) --склад
		AND √руппыѕланировани€._Fld25141 = 0x01--участвует в расчете мощности
		AND (√руппыѕланировани€._Fld23301RRef = Ќоменклатура._Fld3526RRef OR (Ќоменклатура._Fld3526RRef = 0xAC2CBF86E693F63444670FFEB70264EE AND √руппыѕланировани€._Fld23301RRef= 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) ) --габариты
		AND √руппыѕланировани€._Marked = 0x00
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

With Temp_ExchangeRates AS (
SELECT
	T1._Period AS ѕериод,
	T1._Fld14558RRef AS ¬алюта,
	T1._Fld14559 AS  урс,
	T1._Fld14560 AS  ратность
FROM _InfoRgSL26678 T1 With (NOLOCK)
	)
SELECT
    T2._Fld21408RRef AS Ќоменклатура—сылка,
    T2._Fld21410_TYPE AS »сточник_TYPE,
	T2._Fld21410_RTRef AS »сточник_RTRef,
	T2._Fld21410_RRRef AS »сточник_RRRef,
	÷ены._Fld21410_TYPE AS –егистратор_TYPE,
    ÷ены._Fld21410_RTRef AS –егистратор_RTRef,
    ÷ены._Fld21410_RRRef AS –егистратор_RRRef,
    T2._Fld23568RRef AS —клад»сточника,
    T2._Fld21424 AS ƒата—обыти€,
    SUM(T2._Fld21411) - SUM(T2._Fld21412) AS  оличество
Into #Temp_Remains
FROM
    dbo._AccumRgT21444 T2 With (NOLOCK)
	Left Join _AccumRg21407 ÷ены With (NOLOCK)
		Inner Join Temp_ExchangeRates With (NOLOCK)
			On ÷ены._Fld21443RRef = Temp_ExchangeRates.¬алюта 
		On T2._Fld21408RRef = ÷ены._Fld21408RRef
		AND T2._Fld21410_RTRef = 0x00000153
		AND ÷ены._Fld21410_RTRef = 0x00000153 --÷ены.–егистратор ——џЋ ј ƒокумент.мегапрайс–егистраци€ѕрайса
		And ÷ены._Fld21442<>0 AND (÷ены._Fld21442 * Temp_ExchangeRates. урс / Temp_ExchangeRates. ратность >= ÷ены._Fld21982 OR ÷ены._Fld21411 >= ÷ены._Fld21616)
		And ÷ены._Fld21408RRef IN(SELECT
                Ќоменклатура—сылка
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
                TNomen.Ќоменклатура—сылка
            FROM
                #Temp_Goods TNomen WITH(NOLOCK))) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0)
GROUP BY
    T2._Fld21408RRef,
    T2._Fld21410_TYPE,
    T2._Fld21410_RTRef,
    T2._Fld21410_RRRef,
	÷ены._Fld21410_TYPE,
	÷ены._Fld21410_RTRef,
	÷ены._Fld21410_RRRef,
    T2._Fld23568RRef,
    T2._Fld21424
HAVING
    (SUM(T2._Fld21412) <> 0.0
    OR SUM(T2._Fld21411) <> 0.0)
	AND SUM(T2._Fld21412) - SUM(T2._Fld21411) <> 0.0
OPTION (OPTIMIZE FOR (@P_DateTimeNow='4021-05-13T00:00:00'),KEEP PLAN, KEEPFIXED PLAN)
;

SELECT Distinct
    T1._Fld23831RRef AS —клад»сточника,
    T1._Fld23832 AS ƒата—обыти€,
    T1._Fld23834 AS ƒатаѕрибыти€,
    T1._Fld23833RRef AS —кладЌазначени€
Into #Temp_WarehouseDates
FROM
    dbo._InfoRg23830 T1 With (NOLOCK)
	Inner Join #Temp_Remains With (NOLOCK)
	ON T1._Fld23831RRef = #Temp_Remains.—клад»сточника
	AND T1._Fld23832 = #Temp_Remains.ƒата—обыти€
	AND T1._Fld23833RRef IN (Select —клад—сылка From #Temp_GeoData UNION ALL Select —клад—сылка From #Temp_Goods) 
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

SELECT
	T1._Fld23831RRef AS —клад»сточника,
	T1._Fld23833RRef AS —кладЌазначени€,
	MIN(T1._Fld23834) AS ƒатаѕрибыти€ 
Into #Temp_MinimumWarehouseDates
FROM
    dbo._InfoRg23830 T1 With (NOLOCK)
WHERE
    T1._Fld23831RRef IN (
        SELECT
            T2.—клад»сточника AS —клад»сточника
        FROM
            #Temp_Remains T2 WITH(NOLOCK)) 
		AND T1._Fld23832 >= @P_DateTimeNow
		AND T1._Fld23832 <= DateAdd(DAY,6,@P_DateTimeNow)
		AND T1._Fld23833RRef IN (Select —клад—сылка From #Temp_GeoData UNION ALL Select —клад—сылка From #Temp_Goods)
GROUP BY T1._Fld23831RRef,
T1._Fld23833RRef
OPTION (OPTIMIZE FOR (@P_DateTimeNow='4021-05-13T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);

;

SELECT
    T1.Ќоменклатура—сылка,
    T1. оличество,
    T1.»сточник_TYPE,
    T1.»сточник_RTRef,
    T1.»сточник_RRRef,
    T1.—клад»сточника,
    T1.ƒата—обыти€,
    ISNULL(T3.ƒатаѕрибыти€, T2.ƒатаѕрибыти€) AS ƒатаƒоступности,
    1 AS “ип»сточника,
    ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€) AS —кладЌазначени€
INTO #Temp_Sources
FROM
    #Temp_Remains T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_WarehouseDates T2 WITH(NOLOCK)
    ON (T1.—клад»сточника = T2.—клад»сточника)
    AND (T1.ƒата—обыти€ = T2.ƒата—обыти€)
    LEFT OUTER JOIN #Temp_MinimumWarehouseDates T3 WITH(NOLOCK)
    ON (T1.—клад»сточника = T3.—клад»сточника)
    AND (T1.ƒата—обыти€ = '2001-01-01 00:00:00')
WHERE
    T1.»сточник_RTRef = 0x000000E2 OR T1.»сточник_RTRef = 0x00000150

UNION
ALL
SELECT
    T4.Ќоменклатура—сылка,
    T4. оличество,
    T4.»сточник_TYPE,
    T4.»сточник_RTRef,
    T4.»сточник_RRRef,
    T4.—клад»сточника,
    T4.ƒата—обыти€,
    T5.ƒатаѕрибыти€,
    2,
    T5.—кладЌазначени€
FROM
    #Temp_Remains T4 WITH(NOLOCK)
    INNER JOIN #Temp_WarehouseDates T5 WITH(NOLOCK)
    ON (T4.—клад»сточника = T5.—клад»сточника)
    AND (T4.ƒата—обыти€ = T5.ƒата—обыти€)
WHERE
    T4.»сточник_RTRef = 0x00000141

UNION
ALL
SELECT
    T6.Ќоменклатура—сылка,
    T6. оличество,
    T6.»сточник_TYPE,
    T6.»сточник_RTRef,
    T6.»сточник_RRRef,
    T6.—клад»сточника,
    T6.ƒата—обыти€,
    T7.ƒатаѕрибыти€,
    3,
    T7.—кладЌазначени€
FROM
    #Temp_Remains T6 WITH(NOLOCK)
    INNER JOIN #Temp_WarehouseDates T7 WITH(NOLOCK)
    ON (T6.—клад»сточника = T7.—клад»сточника)
    AND (T6.ƒата—обыти€ = T7.ƒата—обыти€)
WHERE
    NOT T6.–егистратор_RRRef IS NULL
	And T6.»сточник_RTRef = 0x00000153
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

With Temp_ExchangeRates AS (
SELECT
	T1._Period AS ѕериод,
	T1._Fld14558RRef AS ¬алюта,
	T1._Fld14559 AS  урс,
	T1._Fld14560 AS  ратность
FROM _InfoRgSL26678 T1 With (NOLOCK)
	)
SELECT
    T1.Ќоменклатура—сылка,
    T1.»сточник_TYPE,
    T1.»сточник_RTRef,
    T1.»сточник_RRRef,
    T1.—клад»сточника,
    T1.—кладЌазначени€,
    T1.ƒата—обыти€,
    T1.ƒатаƒоступности,
    CAST(
        (
            CAST(
                (–езервирование._Fld21442 * T3. урс) AS NUMERIC(27, 8)
            ) / T3. ратность
        ) AS NUMERIC(15, 2)
    )  AS ÷ена
Into #Temp_SourcesWithPrices
FROM
    #Temp_Sources T1 WITH(NOLOCK)
    INNER JOIN dbo._AccumRg21407 –езервирование WITH(NOLOCK)
    LEFT OUTER JOIN Temp_ExchangeRates T3 WITH(NOLOCK)
		ON (–езервирование._Fld21443RRef = T3.¬алюта) 
	ON (T1.Ќоменклатура—сылка = –езервирование._Fld21408RRef)
    AND (
        T1.»сточник_TYPE = 0x08
        AND T1.»сточник_RTRef = –езервирование._RecorderTRef
        AND T1.»сточник_RRRef = –езервирование._RecorderRRef
    )
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;
With Temp_SupplyDocs AS
(
SELECT
    T1.Ќоменклатура—сылка,
    T1.—кладЌазначени€,
    T1.ƒатаƒоступности,
    DATEADD(DAY, 4.0, T1.ƒатаƒоступности) AS ƒатаƒоступностиѕлюс, --это параметр  оличествоƒнейјнализа
    MIN(T1.÷ена) AS ÷ена»сточника,
    MIN(T1.÷ена / 100.0 * (100 - 3.0)) AS ÷ена»сточникаћинус --это параметр ѕроцентƒнейјнализа
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
WHERE
    T1.÷ена <> 0
    AND T1.»сточник_RTRef = 0x00000153    
GROUP BY
    T1.Ќоменклатура—сылка,
    T1.ƒатаƒоступности,
    T1.—кладЌазначени€,
    DATEADD(DAY, 4.0, T1.ƒатаƒоступности)--это параметр  оличествоƒнейјнализа
)
SELECT
    T2.Ќоменклатура—сылка,
    T2.ƒатаƒоступности,
    T2.—кладЌазначени€,
    T2.ƒатаƒоступностиѕлюс,
    T2.÷ена»сточника,
    T2.÷ена»сточникаћинус,
    MIN(T1.ƒатаƒоступности) AS ƒатаƒоступности1,
    MIN(T1.÷ена) AS ÷ена1
Into #Temp_BestPriceAfterClosestDate
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
    INNER JOIN Temp_SupplyDocs T2 WITH(NOLOCK)
    ON (T1.Ќоменклатура—сылка = T2.Ќоменклатура—сылка)
    AND (T1.ƒатаƒоступности >= T2.ƒатаƒоступности)
    AND (T1.ƒатаƒоступности <= T2.ƒатаƒоступностиѕлюс)
    AND (T1.÷ена <= T2.÷ена»сточникаћинус)
    AND (T1.÷ена <> 0)
GROUP BY
    T2.Ќоменклатура—сылка,
    T2.—кладЌазначени€,
    T2.ƒатаƒоступностиѕлюс,
    T2.÷ена»сточника,
    T2.÷ена»сточникаћинус,
    T2.ƒатаƒоступности
OPTION (KEEP PLAN, KEEPFIXED PLAN)

SELECT
    T1.Ќоменклатура—сылка,
    T1.—кладЌазначени€,
    Min(ISNULL(T2.ƒатаƒоступности1, T1.ƒатаƒоступности)) AS ƒатаƒоступности
Into #Temp_SourcesCorrectedDate
FROM
    #Temp_Sources T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_BestPriceAfterClosestDate T2 WITH(NOLOCK)
    ON (T1.Ќоменклатура—сылка = T2.Ќоменклатура—сылка)
    AND (T1.ƒатаƒоступности = T2.ƒатаƒоступности)
    AND (T1.—кладЌазначени€ = T2.—кладЌазначени€)
    AND (T1.“ип»сточника = 3)
GROUP BY
	T1.Ќоменклатура—сылка,
	T1.—кладЌазначени€
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

With Temp_ClosestDate AS
(SELECT
T1.Ќоменклатура—сылка,
T1.—кладЌазначени€,
Cast(MIN(T1.ƒатаƒоступности)as datetime) AS ƒатаƒоступности
FROM #Temp_Sources T1 WITH(NOLOCK)
GROUP BY T1.Ќоменклатура—сылка,
T1.—кладЌазначени€
)
SELECT
            T4.Ќоменклатура—сылка,
            Min(T4.ƒатаƒоступности)AS ƒатаƒоступности,
            T4.—кладЌазначени€
		Into #Temp_T3
        FROM
            #Temp_Sources T4 WITH(NOLOCK)
            LEFT OUTER JOIN Temp_ClosestDate T5 WITH(NOLOCK)
            ON (T4.Ќоменклатура—сылка = T5.Ќоменклатура—сылка)
            AND (T4.—кладЌазначени€ = T5.—кладЌазначени€)
            AND (T4.“ип»сточника = 1)
			AND T4.ƒатаƒоступности <= DATEADD(DAY, 4, T5.ƒатаƒоступности)
Group by T4.Ќоменклатура—сылка, T4.—кладЌазначени€
OPTION (KEEP PLAN, KEEPFIXED PLAN)

SELECT
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€) AS —кладЌазначени€,
    MIN(ISNULL(T3.ƒатаƒоступности, T2.ƒатаƒоступности)) AS Ѕлижайша€ƒата,
    1 AS  оличество,
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1.√руппаѕланировани€,
	T1.√руппаѕланировани€ƒобавл€емое¬рем€,
	0 AS PickUp
into #Temp_ClosestDatesByGoods
FROM
    #Temp_Goods T1 WITH(NOLOCK)	
    LEFT JOIN #Temp_SourcesCorrectedDate T2 WITH(NOLOCK)
		LEFT JOIN  #Temp_T3 T3 ON (T2.Ќоменклатура—сылка = T3.Ќоменклатура—сылка) 
			And T2.—кладЌазначени€ = T3.—кладЌазначени€
    ON (T1.Ќоменклатура—сылка = T2.Ќоменклатура—сылка) 
		AND ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€) IN (Select —клад—сылка From #Temp_GeoData) 
Where 
	T1.—клад—сылка IS NULL
GROUP BY
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
	ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€),
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1. оличество,
    T1.√руппаѕланировани€,
	T1.√руппаѕланировани€ƒобавл€емое¬рем€
UNION ALL
SELECT
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€) AS —кладЌазначени€,
    MIN(ISNULL(T3.ƒатаƒоступности, T2.ƒатаƒоступности)) AS Ѕлижайша€ƒата,
    1 AS  оличество,
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1.√руппаѕланировани€,
	T1.√руппаѕланировани€ƒобавл€емое¬рем€,
	1 AS PickUp
FROM
    #Temp_Goods T1 WITH(NOLOCK)	
    LEFT JOIN #Temp_SourcesCorrectedDate T2 WITH(NOLOCK)
		LEFT JOIN  #Temp_T3 T3 ON (T2.Ќоменклатура—сылка = T3.Ќоменклатура—сылка) 
			And T2.—кладЌазначени€ = T3.—кладЌазначени€
    ON (T1.Ќоменклатура—сылка = T2.Ќоменклатура—сылка) 
		AND T1.—клад—сылка = ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€)
Where 
	NOT T1.—клад—сылка IS NULL
GROUP BY
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
	ISNULL(T3.—кладЌазначени€, T2.—кладЌазначени€),
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1. оличество,
    T1.√руппаѕланировани€,
	T1.√руппаѕланировани€ƒобавл€емое¬рем€
OPTION (KEEP PLAN, KEEPFIXED PLAN)

SELECT
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    T1.—кладЌазначени€,
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1.√руппаѕланировани€,
    MIN(
        CASE
            WHEN T2.»сточник_RTRef = 0x00000141
            OR T2.»сточник_RTRef = 0x00000153
                THEN DATEADD(SECOND, DATEDIFF(SECOND, @P_EmptyDate, T1.√руппаѕланировани€ƒобавл€емое¬рем€), T1.Ѕлижайша€ƒата)
            ELSE T1.Ѕлижайша€ƒата
        END        
    ) AS ƒатаƒоступности,
	T1.PickUp
Into #Temp_ShipmentDates
FROM
    #Temp_ClosestDatesByGoods T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_Sources T2 WITH(NOLOCK)
    ON (T1.Ќоменклатура—сылка = T2.Ќоменклатура—сылка)
    AND (T1.—кладЌазначени€ = T2.—кладЌазначени€)
    AND (T1.Ѕлижайша€ƒата = T2.ƒатаƒоступности)
Where 
	NOT T1.Ѕлижайша€ƒата IS NULL
GROUP BY
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    T1.—кладЌазначени€,
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1.√руппаѕланировани€,
	T1.PickUp
OPTION (KEEP PLAN, KEEPFIXED PLAN)



SELECT
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    MIN(T1.ƒатаƒоступности) AS ƒата—о—клада,
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1.√руппаѕланировани€,
	T1.PickUp
Into #Temp_ShipmentDatesDeliveryCourier
FROM 
    #Temp_ShipmentDates T1 WITH(NOLOCK)
Where T1.PickUp = 0
GROUP BY
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    T1.¬ес,
    T1.ќбъем,
    T1.¬рем€Ќаќбслуживание,
    T1.√руппаѕланировани€,
	T1.PickUp
OPTION (KEEP PLAN, KEEPFIXED PLAN)

SELECT
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
    MIN(T1.ƒатаƒоступности) AS ƒата—о—клада,
	T1.—кладЌазначени€
Into #Temp_ShipmentDatesPickUp
FROM 
    #Temp_ShipmentDates T1 WITH(NOLOCK)
Where T1.PickUp = 1
GROUP BY
    T1.Ќоменклатура—сылка,
	T1.article,
	T1.code,
	T1.—кладЌазначени€
OPTION (KEEP PLAN, KEEPFIXED PLAN);

WITH Tdate(date, Ќоменклатура—сылка, —кладЌазначени€) AS (
    /*Ёто получение списка дат интервалов после даты окончани€ расчета*/
    SELECT         
		CAST(CAST(#Temp_ShipmentDatesPickUp.ƒата—о—клада  AS DATE) AS DATETIME), 		
		#Temp_ShipmentDatesPickUp.Ќоменклатура—сылка,
		#Temp_ShipmentDatesPickUp.—кладЌазначени€
	From #Temp_ShipmentDatesPickUp
    UNION
    ALL
    SELECT 
        DateAdd(day, 1, Tdate.date),
		#Temp_ShipmentDatesPickUp.Ќоменклатура—сылка,
		#Temp_ShipmentDatesPickUp.—кладЌазначени€
    FROM
        Tdate
		Inner Join #Temp_ShipmentDatesPickUp 
		ON Tdate.date < DateAdd(DAY, 8, CAST(CAST(#Temp_ShipmentDatesPickUp.ƒата—о—клада  AS DATE) AS DATETIME))
		AND Tdate.Ќоменклатура—сылка = #Temp_ShipmentDatesPickUp.Ќоменклатура—сылка
		AND Tdate.—кладЌазначени€ = #Temp_ShipmentDatesPickUp.—кладЌазначени€
)
SELECT
	#Temp_ShipmentDatesPickUp.Ќоменклатура—сылка,
	#Temp_ShipmentDatesPickUp.article,
	#Temp_ShipmentDatesPickUp.code,
	--#Temp_ShipmentDatesPickUp.ƒата—о—клада,
	--#Temp_ShipmentDatesPickUp.—кладЌазначени€,
	Min(CASE 
	WHEN 
		DATEADD(
			SECOND,
			CAST(
				DATEDIFF(SECOND, @P_EmptyDate, ѕ¬«√рафик–аботы._Fld23617) AS NUMERIC(12)
			),
			date
		) < #Temp_ShipmentDatesPickUp.ƒата—о—клада 
		then #Temp_ShipmentDatesPickUp.ƒата—о—клада
	Else
		DATEADD(
			SECOND,
			CAST(
				DATEDIFF(SECOND, @P_EmptyDate, ѕ¬«√рафик–аботы._Fld23617) AS NUMERIC(12)
			),
			date
		)
	End) As ¬рем€Ќачала--,
	--DATEADD(
 --       SECOND,
 --       CAST(
 --           DATEDIFF(SECOND, @P_EmptyDate, ѕ¬«√рафик–аботы._Fld23618) AS NUMERIC(12)
 --       ),
 --       date
 --   ) As ¬рем€ќкончани€
Into #Temp_AvailablePickUp
FROM
    #Temp_ShipmentDatesPickUp
		Inner Join Tdate On 
			#Temp_ShipmentDatesPickUp.Ќоменклатура—сылка = Tdate.Ќоменклатура—сылка
			And #Temp_ShipmentDatesPickUp.—кладЌазначени€ = Tdate.—кладЌазначени€
		Inner Join dbo._Reference226 —клады ON —клады._IDRRef = #Temp_ShipmentDatesPickUp.—кладЌазначени€
			Inner Join _Reference23612 On —клады._Fld23620RRef = _Reference23612._IDRRef
				Inner Join _Reference23612_VT23613 As ѕ¬«√рафик–аботы 
				On _Reference23612._IDRRef = _Reference23612_IDRRef
				AND (case when DATEPART ( dw , Tdate.date ) = 1 then 7 else DATEPART ( dw , Tdate.date ) -1 END) = ѕ¬«√рафик–аботы._Fld23615
					AND ѕ¬«√рафик–аботы._Fld25265 = 0x00 --не выходной				
		WHERE DATEADD(
			SECOND,
			CAST(
            DATEDIFF(SECOND, @P_EmptyDate, ѕ¬«√рафик–аботы._Fld23618) AS NUMERIC(12)
			),
			Tdate.date) > #Temp_ShipmentDatesPickUp.ƒата—о—клада 	 
Group by
	#Temp_ShipmentDatesPickUp.Ќоменклатура—сылка,
	#Temp_ShipmentDatesPickUp.article,
	#Temp_ShipmentDatesPickUp.code
--Order by 		
--	CASE 
--	WHEN 
--		DATEADD(
--			SECOND,
--			CAST(
--				DATEDIFF(SECOND, @P_EmptyDate, ѕ¬«√рафик–аботы._Fld23617) AS NUMERIC(12)
--			),
--			date
--		) < #Temp_ShipmentDatesPickUp.ƒата—о—клада 
--		then #Temp_ShipmentDatesPickUp.ƒата—о—клада
--	Else
--		DATEADD(
--			SECOND,
--			CAST(
--				DATEDIFF(SECOND, @P_EmptyDate, ѕ¬«√рафик–аботы._Fld23617) AS NUMERIC(12)
--			),
--			date
--		)
--	End
;


SELECT
    T5._Period AS ѕериод,
    T5._Fld25112RRef As √руппаѕланировани€, 
	T5._Fld25111RRef As √еозона,
	T5._Fld25202 As ¬рем€ЌачалаЌачальное,
	T5._Fld25203 As ¬рем€ќкончани€Ќачальное,
    DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T5._Fld25202) AS NUMERIC(12)
        ),
        T5._Period
    ) As ¬рем€Ќачала
into #Temp_IntervalsAll
FROM
    dbo._AccumRg25110 T5 With (NOLOCK)
WHERE
    T5._Period >= @P_DateTimePeriodBegin --begin +2
    AND T5._Period <= @P_DateTimePeriodEnd --end
    AND T5._Fld25111RRef in (Select √еозона From #Temp_GeoData) 
GROUP BY
    T5._Period,
    T5._Fld25112RRef,
    T5._Fld25111RRef,
    T5._Fld25202,
	T5._Fld25203
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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-05-12T00:00:00',@P_DateTimePeriodEnd='4021-05-16T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);
;

select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, √ео«она¬ременные»нтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.ѕериод
    ) As ¬рем€Ќачала,
#Temp_IntervalsAll.ѕериод,
#Temp_IntervalsAll.√руппаѕланировани€,
#Temp_IntervalsAll.√еозона
into #Temp_Intervals
from #Temp_IntervalsAll
	Inner Join _Reference114_VT25126 √ео«она¬ременные»нтервалы With (NOLOCK)
		On #Temp_IntervalsAll.√еозона = √ео«она¬ременные»нтервалы._Reference114_IDRRef
		And #Temp_IntervalsAll.¬рем€ЌачалаЌачальное >= √ео«она¬ременные»нтервалы._Fld25128
		And #Temp_IntervalsAll.¬рем€ЌачалаЌачальное < √ео«она¬ременные»нтервалы._Fld25129
   INNER JOIN dbo._Reference23294 T2 With (NOLOCK) 
		ON (#Temp_IntervalsAll.√руппаѕланировани€ = T2._IDRRef)
		AND (√ео«она¬ременные»нтервалы._Fld25128 >= T2._Fld25137)
		AND (NOT (((@P_TimeNow >= T2._Fld25138))))
WHERE
    #Temp_IntervalsAll.ѕериод = @P_DateTimePeriodBegin
Group By 
	√ео«она¬ременные»нтервалы._Fld25128,
	√ео«она¬ременные»нтервалы._Fld25129,
	#Temp_IntervalsAll.ѕериод,
	#Temp_IntervalsAll.√руппаѕланировани€,
	#Temp_IntervalsAll.√еозона,
	T2._Fld25137
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

INsert into #Temp_Intervals
select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, √ео«она¬ременные»нтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.ѕериод
    ) As ¬рем€Ќачала,
#Temp_IntervalsAll.ѕериод,
#Temp_IntervalsAll.√руппаѕланировани€,
#Temp_IntervalsAll.√еозона

from #Temp_IntervalsAll
	Inner Join _Reference114_VT25126 √ео«она¬ременные»нтервалы With (NOLOCK)
		On #Temp_IntervalsAll.√еозона = √ео«она¬ременные»нтервалы._Reference114_IDRRef
		And #Temp_IntervalsAll.¬рем€ЌачалаЌачальное >= √ео«она¬ременные»нтервалы._Fld25128
		And #Temp_IntervalsAll.¬рем€ЌачалаЌачальное < √ео«она¬ременные»нтервалы._Fld25129
  INNER JOIN dbo._Reference23294 T4 With (NOLOCK) ON (#Temp_IntervalsAll.√руппаѕланировани€ = T4._IDRRef)
    AND (
        (@P_TimeNow < T4._Fld25140)
        OR (√ео«она¬ременные»нтервалы._Fld25128 >= T4._Fld25139)
    )
WHERE
    #Temp_IntervalsAll.ѕериод = DATEADD(DAY, 1, @P_DateTimePeriodBegin)
Group By 
	√ео«она¬ременные»нтервалы._Fld25128,
	√ео«она¬ременные»нтервалы._Fld25129,
	#Temp_IntervalsAll.ѕериод,
	#Temp_IntervalsAll.√руппаѕланировани€,
	#Temp_IntervalsAll.√еозона
OPTION (KEEP PLAN, KEEPFIXED PLAN) 
;

INsert into #Temp_Intervals
select
DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, √ео«она¬ременные»нтервалы._Fld25128) AS NUMERIC(12)
        ),
        #Temp_IntervalsAll.ѕериод
    ) As ¬рем€Ќачала,
#Temp_IntervalsAll.ѕериод,
#Temp_IntervalsAll.√руппаѕланировани€,
#Temp_IntervalsAll.√еозона

from #Temp_IntervalsAll
	Inner Join _Reference114_VT25126 √ео«она¬ременные»нтервалы With (NOLOCK)
		On #Temp_IntervalsAll.√еозона = √ео«она¬ременные»нтервалы._Reference114_IDRRef
		And #Temp_IntervalsAll.¬рем€ЌачалаЌачальное >= √ео«она¬ременные»нтервалы._Fld25128
		And #Temp_IntervalsAll.¬рем€ЌачалаЌачальное < √ео«она¬ременные»нтервалы._Fld25129
WHERE
	#Temp_IntervalsAll.ѕериод >= DATEADD(DAY, 2, @P_DateTimePeriodBegin) --begin +2
    AND #Temp_IntervalsAll.ѕериод <= @P_DateTimePeriodEnd --end
Group By 
	√ео«она¬ременные»нтервалы._Fld25128,
	√ео«она¬ременные»нтервалы._Fld25129,
	#Temp_IntervalsAll.ѕериод,
	#Temp_IntervalsAll.√руппаѕланировани€,
	#Temp_IntervalsAll.√еозона 
OPTION (KEEP PLAN, KEEPFIXED PLAN)
;

With Temp_DeliveryPower AS
(
SELECT
    CAST(
        SUM(
            CASE
                WHEN (ћощностиƒоставки._RecordKind = 0.0) THEN ћощностиƒоставки._Fld25107
                ELSE -(ћощностиƒоставки._Fld25107)
            END
        ) AS NUMERIC(16, 3)
    ) AS ћассаќборот,
    CAST(
        SUM(
            CASE
                WHEN (ћощностиƒоставки._RecordKind = 0.0) THEN ћощностиƒоставки._Fld25108
                ELSE -(ћощностиƒоставки._Fld25108)
            END
        ) AS NUMERIC(16, 3)
    ) AS ќбъемќборот,
    CAST(
        SUM(
            CASE
                WHEN (ћощностиƒоставки._RecordKind = 0.0) THEN ћощностиƒоставки._Fld25201
                ELSE -(ћощностиƒоставки._Fld25201)
            END
        ) AS NUMERIC(16, 2)
    ) AS ¬рем€Ќаќбслуживаниеќборот,
    CAST(CAST(ћощностиƒоставки._Period  AS DATE) AS DATETIME) AS ƒата
FROM
    dbo._AccumRg25104 ћощностиƒоставки With (NOLOCK)
	--Inner Join #Temp_GeoData ON ћощностиƒоставки._Fld25105RRef = #Temp_GeoData.«онаƒоставки–одитель—сылка
WHERE
    ћощностиƒоставки._Period >= @P_DateTimePeriodBegin
    AND ћощностиƒоставки._Period <= @P_DateTimePeriodEnd
	AND ћощностиƒоставки._Fld25105RRef IN (Select «онаƒоставки–одитель—сылка From  #Temp_GeoData)
GROUP BY
    CAST(CAST(ћощностиƒоставки._Period  AS DATE) AS DATETIME)
)
SELECT
	T1.Ќоменклатура—сылка,
    T1.article,
	T1.code,
    MIN(
        ISNULL(
            T3.¬рем€Ќачала,
CASE
                WHEN (T1.ƒата—о—клада > DATEADD(SECOND,-1,@P_DateTimePeriodEnd)) THEN DATEADD(
                    DAY,
                    1.0,
                    CAST(CAST(T1.ƒата—о—клада AS DATE) AS DATETIME)
                )
                ELSE DATEADD(DAY,1,@P_DateTimePeriodEnd)
            END
        )
    ) AS ƒата урьерскойƒоставки
Into #Temp_AvailableCourier
FROM
    #Temp_ShipmentDatesDeliveryCourier T1 WITH(NOLOCK)
    Left JOIN Temp_DeliveryPower T2 WITH(NOLOCK)
    Inner JOIN #Temp_Intervals T3 WITH(NOLOCK)
		ON (T3.ѕериод = T2.ƒата) 
	ON (T2.ћассаќборот >= T1.¬ес)
    AND (T2.ќбъемќборот >= T1.ќбъем)
    AND (T2.¬рем€Ќаќбслуживаниеќборот >= T1.¬рем€Ќаќбслуживание)
    AND (
        T2.ƒата >= 
		CAST(CAST(T1.ƒата—о—клада AS DATE) AS DATETIME)
    )
    AND (T3.√руппаѕланировани€ = T1.√руппаѕланировани€)
    AND (T3.¬рем€Ќачала >= T1.ƒата—о—клада)
	AND T1.PickUp = 0
GROUP BY
	T1.Ќоменклатура—сылка,
    T1.article,
	T1.code
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-05-12T00:00:00',@P_DateTimePeriodEnd='4021-05-16T00:00:00'),KEEP PLAN, KEEPFIXED PLAN);


Select 
	IsNull(#Temp_AvailableCourier.article,#Temp_AvailablePickUp.article) AS article,
	IsNull(#Temp_AvailableCourier.code,#Temp_AvailablePickUp.code) AS code,
	IsNull(#Temp_AvailableCourier.ƒата урьерскойƒоставки,@P_MaxDate) AS available_date_courier,
	IsNull(#Temp_AvailablePickUp.¬рем€Ќачала,@P_MaxDate) AS available_date_self
From
	#Temp_AvailableCourier 
	FULL Join #Temp_AvailablePickUp 
		On #Temp_AvailableCourier.Ќоменклатура—сылка = #Temp_AvailablePickUp.Ќоменклатура—сылка 


DROP TABLE #Temp_GeoData
DROP TABLE #Temp_WarehouseDates
DROP TABLE #Temp_MinimumWarehouseDates
DROP TABLE #Temp_GoodsBegin
DROP TABLE #Temp_Goods
DROP TABLE #Temp_Remains
DROP TABLE #Temp_Sources
DROP TABLE #Temp_SourcesWithPrices
DROP TABLE #Temp_BestPriceAfterClosestDate
DROP TABLE #Temp_SourcesCorrectedDate
DROP TABLE #Temp_ClosestDatesByGoods
DROP TABLE #Temp_ShipmentDates
DROP TABLE #Temp_ShipmentDatesDeliveryCourier
DROP TABLE #Temp_Intervals
DROP TABLE #Temp_IntervalsAll
Drop Table #Temp_T3
DROP TABLE #Temp_ShipmentDatesPickUp
DROP TABLE #Temp_AvailableCourier
DROP TABLE #Temp_AvailablePickUp
