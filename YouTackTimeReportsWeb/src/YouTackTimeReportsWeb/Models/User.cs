using System.Collections.Generic;
using System.Xml.Serialization;

namespace YouTackTimeReportsWeb.Models
{
    [XmlRoot("user")]
    public class User
    {
        [XmlIgnore]
        public string Username { get; set; }
        [XmlAttribute("fullName")]
        public string FullName { get; set; }
        [XmlAttribute("email")]
        public string Email { get; set; }
        [XmlAttribute("lastCreatedProject")]
        public string LastCreatedProject { get; set; }
    }
    [XmlRoot("userRefs")]
    public class AllUsers
    {
        [XmlElement("user")]
        public AllUsersItem[] Users { get; set; }

        [XmlIgnore]
        public List<AllUsersItem> UserList => new List<AllUsersItem>(Users);
    }
    public class AllUsersItem
    {
        [XmlAttribute("login")]
        public string Login { get; set; }
        [XmlAttribute("url")]
        public string Url { get; set; }
    }
}
