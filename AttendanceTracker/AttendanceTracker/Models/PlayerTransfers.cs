using AttendanceTracker.Attributes;

namespace AttendanceTracker.Models
{
    public class PlayerTransfers
    {
        public string Id { get; set; }
        public string User { get; set; }

        [SortOrder(1)]
        public bool Want { get; set; }

        [SortOrder(2)]
        public bool CanPay { get; set; }
        
    }
}