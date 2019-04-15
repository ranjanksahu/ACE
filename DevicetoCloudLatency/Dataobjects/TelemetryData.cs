using System;

namespace Dataobjects
{
    /// <summary>
    /// 
    /// </summary>
    public class TelemetryData
    {
        public int UniqueId { get; set; }
        public string Data { get; set; }
        public DateTime SentDTTM { get; set; }
    }
}
