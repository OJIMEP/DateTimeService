using DateTimeService.Areas.Identity.Data;
using DateTimeService.Data;
using DateTimeService.Logging;
using DateTimeService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateTimeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class DateTimeController : ControllerBase
    {
        private readonly ILogger<DateTimeController> _logger;
        private readonly IConfiguration _configuration;

        public DateTimeController(ILogger<DateTimeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        } 

        [Authorize(Roles = UserRoles.MaxAvailableCount + "," + UserRoles.Admin)]
        [Route("MaxAvailableCount")]
        [HttpPost]
        public ObjectResult MaxAvailableCount(IEnumerable<RequestDataMaxAvailableCount> nomenclatures)
        {

            string connString = _configuration.GetConnectionString("1CDataSqlConnection");

            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            var result = new List<ResponseMaxAvailableCount>();
            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(nomenclatures),
                Id = Guid.NewGuid().ToString()
            };


            long sqlCommandExecutionTime = 0;

            try
            {
                //sql connection object
                using SqlConnection conn = new(connString);

                conn.StatisticsEnabled = true;

                string query = @"SELECT
                    T4._Fld3480 AS nomenclature_id,
                    CAST(SUM((T1.Fld21411Balance_ - T1.Fld21412Balance_)) AS NUMERIC(34, 3)) AS max_available_count 
                    FROM (SELECT
                    T2._Fld21408RRef AS Fld21408RRef,
                    CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3)) AS Fld21412Balance_,
                    CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3)) AS Fld21411Balance_
                    FROM dbo._AccumRgT21444 T2 WITH(NOLOCK)
                    LEFT OUTER JOIN dbo._Reference149 T3 WITH(NOLOCK)
                    ON T2._Fld21408RRef = T3._IDRRef
                    WHERE T2._Period = '5999-11-01 00:00:00' AND (((T2._Fld21424 = '2001-01-01 00:00:00') OR (T2._Fld21424 >= @P1)) AND (T3._Fld3480 IN ({0})) AND (T3._Fld3514RRef = 0x84A6131B6DC5555A4627E85757507687)) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0) AND (T2._Fld21412 <> 0 OR T2._Fld21411 <> 0)
                    GROUP BY T2._Fld21408RRef
                    HAVING (CAST(SUM(T2._Fld21412) AS NUMERIC(27, 3))) <> 0.0 OR (CAST(SUM(T2._Fld21411) AS NUMERIC(27, 3))) <> 0.0) T1
                    LEFT OUTER JOIN dbo._Reference149 T4 WITH(NOLOCK)
                    ON T1.Fld21408RRef = T4._IDRRef
                    WHERE ((T1.Fld21411Balance_ - T1.Fld21412Balance_) > 0)
                    GROUP BY T1.Fld21408RRef,
                    T4._Fld3480";


                var DateMove = DateTime.Now.AddMonths(24000);

                //define the SqlCommand object
                SqlCommand cmd = new(query, conn);

                cmd.Parameters.Add("@P1", SqlDbType.DateTime);
                cmd.Parameters["@P1"].Value = DateMove;


                var parameters = new string[nomenclatures.Count()];
                for (int i = 0; i < nomenclatures.Count(); i++)
                {
                    parameters[i] = string.Format("@Article{0}", i);
                    cmd.Parameters.AddWithValue(parameters[i], nomenclatures.ToList()[i].Nomenclature_id);
                }

                cmd.CommandText = string.Format(query, string.Join(", ", parameters));

                //open connection
                conn.Open();

                //execute the SQLCommand
                SqlDataReader dr = cmd.ExecuteReader();

                //check if there are records
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var resultItem = new ResponseMaxAvailableCount
                        {
                            Nomenclature_id = dr.GetString(0),
                            Max_available_count = dr.GetDecimal(1)
                        };

                        result.Add(resultItem);
                    }
                }

                var stats = conn.RetrieveStatistics();
                sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                //close data reader
                dr.Close();

                //close connection
                conn.Close();

                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ResponseContent = JsonSerializer.Serialize(result);
                logElement.Status = "Ok";
            }
            catch (Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(result.ToArray());
        }

        /// <summary>
        /// Возвращает ближайшие даты возможной доставки или самовывоза для переданного списка артикулов
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/DateTime/AvailableDate
        ///     {
        ///       "city_id": "17030",
        ///       "codes": [
        ///         "358649","424941","1020938"
        ///       ]
        ///     }
        ///
        /// </remarks>
        /// <returns>Список артикулов с датами доставки и самовывоза</returns>
        /// <response code="200">Успешное получение</response>
        /// <response code="500">Ошибка соединения с БД</response>
        [Authorize(Roles = UserRoles.AvailableDate + "," + UserRoles.Admin)]
        [Route("AvailableDate")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AvailableDate(RequestDataAvailableDate data)
        {

            string connString = _configuration.GetConnectionString("1CDataSqlConnection");

            //string connString = @"Server=localhost;Database=DevBase_cut_v3;Uid=sa;Pwd=; Trusted_Connection = False;";

            ResponseAvailableDate result = new();
            
            var logElement = new ElasticLogElement
            {
                Path = HttpContext.Request.Path,
                Host = HttpContext.Request.Host.ToString(),
                RequestContent = JsonSerializer.Serialize(data),
                Id = Guid.NewGuid().ToString()
            };


            long sqlCommandExecutionTime = 0;

            try
            {
                //sql connection object
                using SqlConnection conn = new(connString);

                conn.StatisticsEnabled = true;

                string query = @"Select
	_Reference114_VT23370._Fld23372RRef As СкладСсылка,
	ЗоныДоставки._ParentIDRRef As ЗонаДоставкиРодительСсылка,
	Геозона._IDRRef As Геозона
Into #Temp_GeoData
From dbo._Reference114 Геозона
	Inner Join _Reference114_VT23370
	on _Reference114_VT23370._Reference114_IDRRef = Геозона._IDRRef
	Inner Join _Reference99 ЗоныДоставки
	on Геозона._Fld2847RRef = ЗоныДоставки._IDRRef
where Геозона._IDRRef IN (
	Select Top 1 --по городу в геоадресе находим геозону
	ГеоАдрес._Fld2785RRef 
	From dbo._Reference112 ГеоАдрес
	Where ГеоАдрес._Fld25552 = @P4)

CREATE CLUSTERED INDEX ix_tempCIndexAft ON #Temp_GeoData(СкладСсылка,ЗонаДоставкиРодительСсылка,Геозона asc);

Select 
	Номенклатура._IDRRef AS НоменклатураСсылка,
	Упаковки._IDRRef AS УпаковкаСсылка,
	1 As Количество,
	Упаковки._Fld6000 AS Вес,
	Упаковки._Fld6006 AS Объем,
	10 AS ВремяНаОбслуживание,
	IsNull(ГруппыПланирования._IDRRef, 0x00000000000000000000000000000000) AS ГруппаПланирования,
	IsNull(ГруппыПланирования._Description, '') AS ГруппаПланированияНаименование,
	IsNull(ГруппыПланирования._Fld25519, @P_EmptyDate) AS ГруппаПланированияДобавляемоеВремя
INTO #Temp_Goods
From 
	dbo._Reference149 Номенклатура
	Inner Join dbo._Reference256 Упаковки
		On 
		Упаковки._OwnerID_TYPE = 0x08  
		AND Упаковки.[_OwnerID_RTRef] = 0x00000095
		AND Номенклатура._IDRRef = Упаковки._OwnerID_RRRef		
		And Упаковки._Fld6003RRef = Номенклатура._Fld3489RRef
		AND Упаковки._Marked = 0x00
	Left Join dbo._Reference23294 ГруппыПланирования
		Inner Join dbo._Reference23294_VT23309 
			on ГруппыПланирования._IDRRef = _Reference23294_VT23309._Reference23294_IDRRef
			and _Reference23294_VT23309._Fld23311RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
		On 
		ГруппыПланирования._Fld23302RRef IN (Select СкладСсылка From #Temp_GeoData) --склад
		AND ГруппыПланирования._Fld25141 = 0x01--участвует в расчете мощности
		AND (ГруппыПланирования._Fld23301RRef = Номенклатура._Fld3526RRef OR (Номенклатура._Fld3526RRef = 0xAC2CBF86E693F63444670FFEB70264EE AND ГруппыПланирования._Fld23301RRef= 0xAD3F7F5FC4F15DAD4F693CAF8365EC0D) ) --габариты
		AND ГруппыПланирования._Marked = 0x00
Where
	Номенклатура._Fld3480 IN ({0})

CREATE CLUSTERED INDEX ix_tempCIndexAft1 ON #Temp_Goods (НоменклатураСсылка,УпаковкаСсылка,ГруппаПланирования);

--SELECT
--	T4._Period AS Период,
--	T4._Fld14558RRef AS Валюта,
--	T4._Fld14559 AS Курс,
--	T4._Fld14560 AS Кратность
--Into #Temp_ExchangeRates
--FROM (SELECT
--		T3._Fld14558RRef AS Fld14558RRef,
--		MAX(T3._Period) AS MAXPERIOD_
--	FROM dbo._InfoRg14557 T3
--	WHERE
--	(T3._Fld14558RRef IN (0x80C2005056A128DA11E6339ED4C110DF,0x8265002522BD9FAE11E4C0CE6721009A,0x8265002522BD9FAE11E4C0CE67210099,0x8265002522BD9FAE11E4C0CE6721009B)) --валюты
--	GROUP BY 
--		T3._Fld14558RRef) T2
--INNER JOIN dbo._InfoRg14557 T4 
--	ON T2.Fld14558RRef = T4._Fld14558RRef AND T2.MAXPERIOD_ = T4._Period


SELECT
    T2._Fld21408RRef AS НоменклатураСсылка,
    T2._Fld21410_TYPE AS Источник_TYPE,
	T2._Fld21410_RTRef AS Источник_RTRef,
	T2._Fld21410_RRRef AS Источник_RRRef,
	CASE
        WHEN (
            T2._Fld21410_TYPE = 0x08
            AND T2._Fld21410_RTRef = 0x00000153
        ) THEN T2._Fld21410_TYPE
        ELSE CAST(NULL AS BINARY(1))
    END AS Регистратор_TYPE,
    CASE
        WHEN (
            T2._Fld21410_TYPE = 0x08
            AND T2._Fld21410_RTRef = 0x00000153
        ) THEN T2._Fld21410_RTRef
        ELSE CAST(NULL AS BINARY(4))
    END AS Регистратор_RTRef,
    CASE
        WHEN (
            T2._Fld21410_TYPE = 0x08
            AND T2._Fld21410_RTRef = 0x00000153
        ) THEN T2._Fld21410_RRRef
        ELSE CAST(NULL AS BINARY(16))
    END AS Регистратор_RRRef,
    T2._Fld23568RRef AS СкладИсточника,
    T2._Fld21424 AS ДатаСобытия,
    SUM(T2._Fld21411) - SUM(T2._Fld21412) AS Количество
Into #Temp_Remains
FROM
    dbo._AccumRgT21444 T2
WHERE
    T2._Period = '5999-11-01 00:00:00'
    AND (
        (
            (T2._Fld21424 = '2001-01-01 00:00:00')
            OR (T2._Fld21424 >= @P_DateTimeNow)
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
                T2._Fld23568RRef,
                T2._Fld21424
            HAVING
                (SUM(T2._Fld21412) <> 0.0
                OR SUM(T2._Fld21411) <> 0.0)
				AND SUM(T2._Fld21412) - SUM(T2._Fld21411) <> 0.0
;

SELECT
    T1._Fld23831RRef AS СкладИсточника,
    T1._Fld23832 AS ДатаСобытия,
    T1._Fld23834 AS ДатаПрибытия,
    T1._Fld23833RRef AS СкладНазначения
Into #Temp_WarehouseDates
FROM
    dbo._InfoRg23830 T1
	Inner Join #Temp_Remains
	ON T1._Fld23831RRef = #Temp_Remains.СкладИсточника
	AND T1._Fld23832 = #Temp_Remains.ДатаСобытия
	AND T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData)
--WHERE
--    T1._Fld23831RRef IN (
--        SELECT
--            T2.СкладИсточника AS СкладИсточника
--        FROM
--            #Temp_Remains T2 WITH(NOLOCK)) 
--		--AND T1._Fld23832  @P_DateTimeNow
--		AND T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData)
    
SELECT
	T1._Fld23831RRef AS СкладИсточника,
	T1._Fld23833RRef AS СкладНазначения,
	MIN(T1._Fld23834) AS ДатаПрибытия 
Into #Temp_MinimumWarehouseDates
FROM
    dbo._InfoRg23830 T1
	WHERE
    T1._Fld23831RRef IN (
        SELECT
            T2.СкладИсточника AS СкладИсточника
        FROM
            #Temp_Remains T2 WITH(NOLOCK)) 
		AND T1._Fld23832 > @P_DateTimeNow
		AND T1._Fld23832 < @P_DateTimePeriodEnd
		AND T1._Fld23833RRef IN (Select СкладСсылка From #Temp_GeoData)
GROUP BY T1._Fld23831RRef,
T1._Fld23833RRef


;

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
    NOT T6.Источник_RRRef IS NULL
                



--SELECT
--T1.НоменклатураСсылка,
--T1.СкладНазначения,
--MIN(T1.ДатаДоступности) AS ДатаДоступности
--INto #Temp_ClosestDate
--FROM #Temp_Sources T1 WITH(NOLOCK)
--GROUP BY T1.НоменклатураСсылка,
--T1.СкладНазначения

;

With Temp_ExchangeRates AS (
SELECT
	T4._Period AS Период,
	T4._Fld14558RRef AS Валюта,
	T4._Fld14559 AS Курс,
	T4._Fld14560 AS Кратность
--Into #Temp_ExchangeRates
FROM (SELECT
		T3._Fld14558RRef AS Fld14558RRef,
		MAX(T3._Period) AS MAXPERIOD_
	FROM dbo._InfoRg14557 T3
	WHERE
	(T3._Fld14558RRef IN (0x80C2005056A128DA11E6339ED4C110DF,0x8265002522BD9FAE11E4C0CE6721009A,0x8265002522BD9FAE11E4C0CE67210099,0x8265002522BD9FAE11E4C0CE6721009B)) --валюты
	GROUP BY 
		T3._Fld14558RRef) T2
INNER JOIN dbo._InfoRg14557 T4 
	ON T2.Fld14558RRef = T4._Fld14558RRef AND T2.MAXPERIOD_ = T4._Period
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
    INNER JOIN dbo._AccumRg21407 Резервирование
    LEFT OUTER JOIN Temp_ExchangeRates T3 WITH(NOLOCK)
    ON (Резервирование._Fld21443RRef = T3.Валюта) ON (T1.НоменклатураСсылка = Резервирование._Fld21408RRef)
    AND (
        T1.Источник_TYPE = 0x08
        AND T1.Источник_RTRef = Резервирование._RecorderTRef
        AND T1.Источник_RRRef = Резервирование._RecorderRRef
    )

--SELECT
--    T1.НоменклатураСсылка,
--    T1.СкладНазначения,
--    T1.ДатаДоступности,
--    DATEADD(DAY, 4.0, T1.ДатаДоступности) AS ДатаДоступностиПлюс, --это параметр КоличествоДнейАнализа
--    MIN(T1.Цена) AS ЦенаИсточника,
--    MIN(T1.Цена / 100.0 * (100 - 3.0)) AS ЦенаИсточникаМинус --это параметр ПроцентДнейАнализа
--Into #Temp_SupplyDocs
--FROM
--    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
--WHERE
--    T1.Цена <> 0
--    AND T1.Источник_RTRef = 0x00000153
    
--GROUP BY
--    T1.НоменклатураСсылка,
--    T1.ДатаДоступности,
--    T1.СкладНазначения,
--    DATEADD(DAY, 4.0, T1.ДатаДоступности)
;
With Temp_SupplyDocs AS
(
SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.ДатаДоступности,
    DATEADD(DAY, 4.0, T1.ДатаДоступности) AS ДатаДоступностиПлюс, --это параметр КоличествоДнейАнализа
    MIN(T1.Цена) AS ЦенаИсточника,
    MIN(T1.Цена / 100.0 * (100 - 3.0)) AS ЦенаИсточникаМинус --это параметр ПроцентДнейАнализа
--Into #Temp_SupplyDocs
FROM
    #Temp_SourcesWithPrices T1 WITH(NOLOCK)
WHERE
    T1.Цена <> 0
    AND T1.Источник_RTRef = 0x00000153
    
GROUP BY
    T1.НоменклатураСсылка,
    T1.ДатаДоступности,
    T1.СкладНазначения,
    DATEADD(DAY, 4.0, T1.ДатаДоступности)
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

SELECT
    T1.НоменклатураСсылка,
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
;

With Temp_ClosestDate AS
(SELECT
T1.НоменклатураСсылка,
T1.СкладНазначения,
MIN(T1.ДатаДоступности) AS ДатаДоступности
--INto #Temp_ClosestDate
FROM #Temp_Sources T1 WITH(NOLOCK)
GROUP BY T1.НоменклатураСсылка,
T1.СкладНазначения
)
SELECT
    T1.НоменклатураСсылка,
    ISNULL(T3.СкладНазначения, T2.СкладНазначения) AS СкладНазначения,
    MIN(ISNULL(T3.ДатаДоступности, T2.ДатаДоступности)) AS БлижайшаяДата,
    1 AS Количество,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя
into #Temp_ClosestDatesByGoods
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
            AND (T4.ТипИсточника = 1)
    ) T3 ON (T1.НоменклатураСсылка = T3.НоменклатураСсылка)
    AND (
        T3.ДатаДоступности <= DATEADD(DAY, 4, T3.БлижайшаяДата)
    )
GROUP BY
    T1.НоменклатураСсылка,
    ISNULL(T3.СкладНазначения, T2.СкладНазначения),
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.Количество,
    T1.ГруппаПланирования,
	T1.ГруппаПланированияДобавляемоеВремя

SELECT
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования,
    MIN(
        ISNULL(
            CASE
                WHEN T2.Источник_RTRef = 0x00000141
                OR T2.Источник_RTRef = 0x00000153
                 THEN DATEADD(SECOND, DATEDIFF(SECOND, @P_EmptyDate, T1.ГруппаПланированияДобавляемоеВремя), T1.БлижайшаяДата)
                ELSE T1.БлижайшаяДата
            END,
            @P_MaxDate
        )
    ) AS ДатаДоступности
Into #Temp_ShipmentDates
FROM
    #Temp_ClosestDatesByGoods T1 WITH(NOLOCK)
    LEFT OUTER JOIN #Temp_Sources T2 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T2.НоменклатураСсылка)
    AND (T1.СкладНазначения = T2.СкладНазначения)
    AND (T1.БлижайшаяДата = T2.ДатаДоступности)
GROUP BY
    T1.НоменклатураСсылка,
    T1.СкладНазначения,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования


--SELECT
--    CAST(
--        SUM(
--            CASE
--                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25107
--                ELSE -(МощностиДоставки._Fld25107)
--            END
--        ) AS NUMERIC(16, 3)
--    ) AS МассаОборот,
--    CAST(
--        SUM(
--            CASE
--                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25108
--                ELSE -(МощностиДоставки._Fld25108)
--            END
--        ) AS NUMERIC(16, 3)
--    ) AS ОбъемОборот,
--    CAST(
--        SUM(
--            CASE
--                WHEN (МощностиДоставки._RecordKind = 0.0) THEN МощностиДоставки._Fld25201
--                ELSE -(МощностиДоставки._Fld25201)
--            END
--        ) AS NUMERIC(16, 2)
--    ) AS ВремяНаОбслуживаниеОборот,
--    DATETIME2FROMPARTS(
--        DATEPART(YEAR, МощностиДоставки._Period),
--        DATEPART(MONTH, МощностиДоставки._Period),
--        DATEPART(DAY, МощностиДоставки._Period),
--        0,
--        0,
--        0,
--        0,
--        0
--    ) AS Дата
--Into #Temp_DeliveryPower
--FROM
--    dbo._AccumRg25104 МощностиДоставки
--WHERE
--    МощностиДоставки._Period >= @P_DateTimePeriodBegin
--    AND МощностиДоставки._Period <= @P_DateTimePeriodEnd
--    AND МощностиДоставки._Fld25105RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
--GROUP BY
--    DATETIME2FROMPARTS(
--        DATEPART(YEAR, МощностиДоставки._Period),
--        DATEPART(MONTH, МощностиДоставки._Period),
--        DATEPART(DAY, МощностиДоставки._Period),
--        0,
--        0,
--        0,
--        0,
--        0
--    )

SELECT
    T1.НоменклатураСсылка,
    MIN(T1.ДатаДоступности) AS ДатаСоСклада,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования
Into #Temp_ShipmentDatesDeliveryCourier
FROM
    #Temp_ShipmentDates T1 WITH(NOLOCK)
--WHERE
--    T1.СкладНазначения IN (@P1, @P2) --пока не имеет значения ибо в запросе не используются склады ПВЗ
GROUP BY
    T1.НоменклатураСсылка,
    T1.Вес,
    T1.Объем,
    T1.ВремяНаОбслуживание,
    T1.ГруппаПланирования

;
SELECT 
    T1._Period AS Период,
    T1._Fld25112RRef AS ГруппаПланирования,
	DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T1._Fld25202) AS NUMERIC(12)
        ),
        T1._Period
    ) AS ВремяНачала
Into #Temp_Intervals
FROM
    dbo._AccumRg25110 T1
    INNER JOIN dbo._Reference23294 T2 ON (T1._Fld25112RRef = T2._IDRRef)
    AND (T1._Fld25202 >= T2._Fld25137)
    AND (NOT (((@P_TimeNow >= T2._Fld25138))))
	Inner Join #Temp_GeoData ON T1._Fld25111RRef = #Temp_GeoData.Геозона
WHERE
    T1._Period = @P_DateTimePeriodBegin
    --AND T1._Fld25111RRef in (Select Геозона From #Temp_GeoData) 
GROUP BY
    T1._Period,
    T1._Fld25112RRef,
    T1._Fld25202
HAVING
    (
        CAST(
            SUM(
                CASE
                    WHEN (T1._RecordKind = 0.0) THEN T1._Fld25113
                    ELSE -(T1._Fld25113)
                END
            ) AS NUMERIC(16, 0)
        ) > 0.0
    )
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-03-27 00:00:00'));
--option (recompile)
--UNION
--ALL
INsert into #Temp_Intervals
SELECT
    T3._Period,
    T3._Fld25112RRef,    
    DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T3._Fld25202) AS NUMERIC(12)
        ),
        T3._Period
    )
FROM
    dbo._AccumRg25110 T3
    INNER JOIN dbo._Reference23294 T4 ON (T3._Fld25112RRef = T4._IDRRef)
    AND (
        (@P_TimeNow < T4._Fld25140)
        OR (T3._Fld25202 >= T4._Fld25139)
    )
WHERE
    T3._Period = DATEADD(DAY, 1, @P_DateTimePeriodBegin) --bigin +1
    AND T3._Fld25111RRef in (Select Геозона From #Temp_GeoData)
GROUP BY
    T3._Period,
    T3._Fld25112RRef,
    T3._Fld25202
HAVING
    (
        CAST(
            SUM(
                CASE
                    WHEN (T3._RecordKind = 0.0) THEN T3._Fld25113
                    ELSE -(T3._Fld25113)
                END
            ) AS NUMERIC(16, 0)
        ) > 0.0
    )
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-03-27 00:00:00'));
--option (recompile)
--UNION
--ALL
INsert into #Temp_Intervals
SELECT
    T5._Period,
    T5._Fld25112RRef,    
    DATEADD(
        SECOND,
        CAST(
            DATEDIFF(SECOND, @P_EmptyDate, T5._Fld25202) AS NUMERIC(12)
        ),
        T5._Period
    )
FROM
    dbo._AccumRg25110 T5
WHERE
    T5._Period >= DATEADD(DAY, 2, @P_DateTimePeriodBegin) --begin +2
    AND T5._Period <= @P_DateTimePeriodEnd --end
    AND T5._Fld25111RRef in (Select Геозона From #Temp_GeoData) 
GROUP BY
    T5._Period,
    T5._Fld25112RRef,
    T5._Fld25202
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
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-03-27 00:00:00'));
--option (recompile)
;

CREATE CLUSTERED INDEX ix_tempCIndexIntervals ON #Temp_Intervals(Период,ГруппаПланирования,ВремяНачала asc);

With Temp_DeliveryPower AS
(
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
    DATETIME2FROMPARTS(
        DATEPART(YEAR, МощностиДоставки._Period),
        DATEPART(MONTH, МощностиДоставки._Period),
        DATEPART(DAY, МощностиДоставки._Period),
        0,
        0,
        0,
        0,
        0
    ) AS Дата
--Into #Temp_DeliveryPower
FROM
    dbo._AccumRg25104 МощностиДоставки
	Inner Join #Temp_GeoData ON МощностиДоставки._Fld25105RRef = #Temp_GeoData.ЗонаДоставкиРодительСсылка

WHERE
    МощностиДоставки._Period >= @P_DateTimePeriodBegin
    AND МощностиДоставки._Period <= @P_DateTimePeriodEnd
   -- AND МощностиДоставки._Fld25105RRef in (Select ЗонаДоставкиРодительСсылка From #Temp_GeoData)
GROUP BY
    DATETIME2FROMPARTS(
        DATEPART(YEAR, МощностиДоставки._Period),
        DATEPART(MONTH, МощностиДоставки._Period),
        DATEPART(DAY, МощностиДоставки._Period),
        0,
        0,
        0,
        0,
        0
    )
)
SELECT
    T5._Fld3480 AS nomenclature_id,
    MIN(
        ISNULL(
            T3.ВремяНачала,
CASE
                WHEN (T1.ДатаСоСклада > DATEADD(SECOND,-1,@P_DateTimePeriodEnd)) THEN DATEADD(
                    DAY,
                    1.0,
                    DATETIME2FROMPARTS(
                        DATEPART(YEAR, T1.ДатаСоСклада),
                        DATEPART(MONTH, T1.ДатаСоСклада),
                        DATEPART(DAY, T1.ДатаСоСклада),
                        0,
                        0,
                        0,
                        0,
                        0
                    )
                )
                ELSE DATEADD(DAY,1,@P_DateTimePeriodEnd)
            END
        )
    ) AS available_date_courier,
    MIN(ISNULL(T4.ДатаДоступности,@P_MaxDate)) AS available_date_self
FROM
    #Temp_ShipmentDatesDeliveryCourier T1 WITH(NOLOCK)
    INNER JOIN Temp_DeliveryPower T2 WITH(NOLOCK)
    INNER JOIN #Temp_Intervals T3 WITH(NOLOCK)
    ON (T3.Период = T2.Дата) 
	ON (T2.МассаОборот >= T1.Вес)
    AND (T2.ОбъемОборот >= T1.Объем)
    AND (T2.ВремяНаОбслуживаниеОборот >= T1.ВремяНаОбслуживание)
    AND (
        T2.Дата >= DATETIME2FROMPARTS(
            DATEPART(YEAR, T1.ДатаСоСклада),
            DATEPART(MONTH, T1.ДатаСоСклада),
            DATEPART(DAY, T1.ДатаСоСклада),
            0,
            0,
            0,
            0,
            0
        )
    )
    AND (T3.ГруппаПланирования = T1.ГруппаПланирования)
    AND (T3.ВремяНачала >= T1.ДатаСоСклада)
    Left JOIN #Temp_ShipmentDates T4 WITH(NOLOCK)
    ON (T1.НоменклатураСсылка = T4.НоменклатураСсылка)
    AND (T4.СкладНазначения IN (NULL)) --склады ПВЗ
    LEFT OUTER JOIN dbo._Reference149 T5 ON T1.НоменклатураСсылка = T5._IDRRef
GROUP BY
    T5._Fld3480
OPTION (OPTIMIZE FOR (@P_DateTimePeriodBegin='4021-03-27 00:00:00',@P_DateTimePeriodEnd='4021-03-31 00:00:00'));
--option (recompile)

DROP TABLE #Temp_GeoData
DROP TABLE #Temp_WarehouseDates
DROP TABLE #Temp_MinimumWarehouseDates
--DROP TABLE #Temp_ExchangeRates
DROP TABLE #Temp_Goods
DROP TABLE #Temp_Remains
DROP TABLE #Temp_Sources
--DROP TABLE #Temp_ClosestDate
DROP TABLE #Temp_SourcesWithPrices
--DROP TABLE #Temp_SupplyDocs
DROP TABLE #Temp_BestPriceAfterClosestDate
DROP TABLE #Temp_SourcesCorrectedDate
DROP TABLE #Temp_ClosestDatesByGoods
DROP TABLE #Temp_ShipmentDates
--DROP TABLE #Temp_DeliveryPower
DROP TABLE #Temp_ShipmentDatesDeliveryCourier
DROP TABLE #Temp_Intervals
";


                var DateMove = DateTime.Now.AddMonths(24000);
                var TimeNow = new DateTime(2001, 1, 1, DateMove.Hour, DateMove.Minute, DateMove.Second);
                var EmptyDate = new DateTime(2001, 1, 1, 0, 0, 0);
                var MaxDate = new DateTime(5999, 11, 11, 0, 0, 0);

                //define the SqlCommand object
                SqlCommand cmd = new(query, conn);

                cmd.Parameters.Add("@P4", SqlDbType.NVarChar);
                cmd.Parameters["@P4"].Value = data.city_id;

                cmd.Parameters.Add("@P_DateTimeNow", SqlDbType.DateTime);
                cmd.Parameters["@P_DateTimeNow"].Value = DateMove;

                cmd.Parameters.Add("@P_DateTimePeriodBegin", SqlDbType.DateTime);
                cmd.Parameters["@P_DateTimePeriodBegin"].Value = DateMove.Date;

                cmd.Parameters.Add("@P_DateTimePeriodEnd", SqlDbType.DateTime);
                cmd.Parameters["@P_DateTimePeriodEnd"].Value = DateMove.Date.AddDays(4);

                cmd.Parameters.Add("@P_TimeNow", SqlDbType.DateTime);
                cmd.Parameters["@P_TimeNow"].Value = TimeNow;

                cmd.Parameters.Add("@P_EmptyDate", SqlDbType.DateTime);
                cmd.Parameters["@P_EmptyDate"].Value = EmptyDate;

                cmd.Parameters.Add("@P_MaxDate", SqlDbType.DateTime);
                cmd.Parameters["@P_MaxDate"].Value = MaxDate;


                var parameters = new string[data.codes.Length];
                for (int i = 0; i < data.codes.Length; i++)
                {
                    parameters[i] = string.Format("@Article{0}", i);
                    cmd.Parameters.AddWithValue(parameters[i], data.codes[i]);
                }

                cmd.CommandText = string.Format(query, string.Join(", ", parameters));

                //open connection
                conn.Open();

                //execute the SQLCommand
                SqlDataReader dr = cmd.ExecuteReader();

                //check if there are records
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        var article = dr.GetString(0);
                        var availableDateCourier = dr.GetDateTime(1).AddMonths(-24000);
                        var availableDateSelf = dr.GetDateTime(2).AddMonths(-24000);

                        result.code.Add(article);
                        result.courier.Add(availableDateCourier);
                        result.self.Add(availableDateSelf);
                    }
                }

                var stats = conn.RetrieveStatistics();
                sqlCommandExecutionTime = (long)stats["ExecutionTime"];

                //close data reader
                dr.Close();

                //close connection
                conn.Close();

                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ResponseContent = JsonSerializer.Serialize(result);
                logElement.Status = "Ok";
            }
            catch (Exception ex)
            {
                logElement.TimeSQLExecution = sqlCommandExecutionTime;
                logElement.ErrorDescription = ex.Message;
                logElement.Status = "Error";
            }

            var resultDict = new ResponseAvailableDateDict();
            for (int i=0; i<result.code.Count; i++)
            {
                var resEl = new ResponseAvailableDateDictElement
                {
                    code = result.code[i],
                    courier = result.courier[i].ToString("yyyy-MM-dd"),
                    self = result.self[i].ToString("yyyy-MM-dd")
                };

                resultDict.data.Add(result.code[i],resEl);
            }
            

            var logstringElement = JsonSerializer.Serialize(logElement);

            _logger.LogInformation(logstringElement);

            return Ok(resultDict);
        }

    }
}
