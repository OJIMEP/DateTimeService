using System;

namespace DateTimeService.Exceptions
{
    class DbConnectionNotFoundException : SystemException
    {
        public DbConnectionNotFoundException(string message) : base(message)
        {
        }
    }
}
