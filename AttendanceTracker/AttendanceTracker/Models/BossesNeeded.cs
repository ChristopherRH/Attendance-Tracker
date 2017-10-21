using AttendanceTracker.Attributes;

namespace AttendanceTracker.Models
{
    public class BossesNeeded
    {
        public string Id { get; set; }
        public string User { get; set; }
        public bool Highlighted { get; set; }

        [SortOrder(1)]
        public bool Goroth { get; set; }

        [SortOrder(2)]
        public bool Di { get; set; }

        [SortOrder(3)]
        public bool Harjatan { get; set; }

        [SortOrder(4)]
        public bool Sisters { get; set; }

        [SortOrder(5)]
        public bool Mistress { get; set; }

        [SortOrder(6)]
        public bool Host { get; set; }

        [SortOrder(7)]
        public bool Maiden { get; set; }

        [SortOrder(8)]
        public bool FallenAvatar { get; set; }

        [SortOrder(9)]
        public bool Kiljaeden { get; set; }
    }
}