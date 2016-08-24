using System.Collections.Generic;
using System.Xml.Serialization;

namespace YouTackTimeReportsWeb.Models
{
    [XmlRoot("timesettings")]
    public class TimeSettings
    {
        [XmlElement(ElementName = "hoursADay")]
        public int HoursADay { get; set; }
        [XmlElement(ElementName = "daysAWeek")]
        public int DaysAWeek { get; set; }
        [XmlArray("workWeek")]
        [XmlArrayItem("workDay")]
        public List<int> WorkWeek { get; set; }
    }
    public enum WorkDay
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }
}
