namespace DateTimeService.Models
{
    public class RequestDataCodeItemDTO
    {
        public string code { get; set; }
        public string sale_code { get; set; }
        public int quantity { get; set; }
    }
    public class RequestDataCodeItem
    {
        public string article { get; set; }
        public string code { get; set; }
        public string sale_code { get; set; }  // код с сайта без префиксов и нулей
        public int quantity { get; set; }
    }
}