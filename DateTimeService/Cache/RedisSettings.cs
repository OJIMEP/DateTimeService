namespace DateTimeService.Cache
{
    public class RedisSettings
    {
        public bool Enabled { get; set; }

        public string ConnectionString { get; set; }

        public int LifeTime { get; set; }
    }
}
