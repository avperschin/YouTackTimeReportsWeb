namespace YouTackTimeReportsWeb.Models
{
    public class SearchParams
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Path { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsSsl { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public string ProjectId { get; set; }
    }
}
