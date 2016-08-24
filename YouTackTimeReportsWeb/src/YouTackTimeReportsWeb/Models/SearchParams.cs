namespace YouTackTimeReportsWeb.Models
{
    public class SearchParams
    {
        public string host { get; set; }
        public int port { get; set; }
        public string path { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public bool isssl { get; set; }
        public string datestart { get; set; }
        public string dateend { get; set; }
        public string projectid { get; set; }
    }
}
