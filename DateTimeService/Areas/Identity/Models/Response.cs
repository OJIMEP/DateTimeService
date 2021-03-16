using System.Collections;

namespace DateTimeService.Areas.Identity.Models
{
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable Description { get; set; }
    }
}
