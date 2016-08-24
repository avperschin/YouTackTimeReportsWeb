using System.Xml.Serialization;

namespace YouTackTimeReportsWeb.Models
{
    [XmlRoot("project")]
    public class Project
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("lead")]
        public string Lead { get; set; }
        [XmlAttribute("assigneesUrl")]
        public string AssigneesUrl { get; set; }
        [XmlAttribute("subsystemsUrl")]
        public string SubsystemsUrl { get; set; }
        [XmlAttribute("buildsUrl")]
        public string BuildsUrl { get; set; }
        [XmlAttribute("versionsUrl")]
        public string VersionsUrl { get; set; }
        [XmlAttribute("startingNumber")]
        public string StartingNumber { get; set; }
    }
}
