using System;

namespace AttendanceTracker.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SortOrder : Attribute
    {
        public int Order { get; }

        public SortOrder(int order)
        {
            Order = order;
        }
    }
}