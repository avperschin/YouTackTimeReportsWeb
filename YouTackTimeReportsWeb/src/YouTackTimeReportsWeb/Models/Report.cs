using System;
using System.Collections.Generic;

namespace YouTackTimeReportsWeb.Models
{
    public class Report
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OwnerLogin { get; set; }
        public string Type { get; set; }
        public bool Own { get; set; }
        public VisibleTo VisibleTo { get; set; }
        public int InvalidationInterval { get; set; }
        public string State { get; set; }
        public string LastCalculated { get; set; }
        public int Progress { get; set; }
        public ReportParameters Parameters { get; set; }
        public ReportData ReportData { get; set; }
        public string OldData { get; set; }
        public DateTime? ReportDate { get; set; }
        public bool ReportFill { get; set; }
    }
    public class ReportParameters
    {
        public List<Projects> Projects { get; set; }
        public string Query { get; set; }
        public string QueryUrl { get; set; }
        public Range Range { get; set; }
        public string GroupBy { get; set; }
        public string GroupById { get; set; }
        public bool PerUserAvailable { get; set; }
        public bool ShowTypesAvailable { get; set; }
        public string IssuesQuery { get; set; }
    }
    public class ReportData
    {
        public bool PerUser { get; set; }
        public string GroupBy { get; set; }
        public string Estimation { get; set; }
        public double EstimationNumber
        {
            get
            {
                double estimation = 0;
                if (!string.IsNullOrEmpty(Estimation) && !string.IsNullOrWhiteSpace(Estimation))
                {
                    var se = Estimation.Split(' ');
                    double t = 0;
                    foreach (string t1 in se)
                    {
                        if (t1.Contains("h"))
                        {
                            t += Convert.ToInt32(t1.Replace("h", "")) * 60;
                        }
                        if (t1.Contains("m"))
                        {
                            t += Convert.ToInt32(t1.Replace("m", ""));
                        }
                    }
                    estimation = t / 60;
                }
                return estimation;
            }
        }
        public string Duration { get; set; }
        public double DurationNumber
        {
            get
            {
                double duration = 0;
                if (!string.IsNullOrEmpty(Duration) && !string.IsNullOrWhiteSpace(Duration))
                {
                    var sd = Duration.Split(' ');
                    double t = 0;
                    foreach (string t1 in sd)
                    {
                        if (t1.Contains("h"))
                        {
                            t += Convert.ToInt32(t1.Replace("h", "")) * 60;
                        }
                        if (t1.Contains("m"))
                        {
                            t += Convert.ToInt32(t1.Replace("m", ""));
                        }
                    }
                    duration = t / 60;
                }
                return duration;
            }
        }
        public List<Groups> Groups { get; set; }
    }
    public class Groups
    {
        public string Name { get; set; }
        public string Duration { get; set; }
        public double DurationNumber
        {
            get
            {
                double duration = 0;
                if (!string.IsNullOrEmpty(Duration) && !string.IsNullOrWhiteSpace(Duration))
                {
                    var sd = Duration.Split(' ');
                    double t = 0;
                    foreach (string t1 in sd)
                    {
                        if (t1.Contains("h"))
                        {
                            t += Convert.ToInt32(t1.Replace("h", "")) * 60;
                        }
                        if (t1.Contains("m"))
                        {
                            t += Convert.ToInt32(t1.Replace("m", ""));
                        }
                    }
                    duration = t / 60;
                }
                return duration;
            }
        }
        public string Estimation { get; set; }
        public double EstimationNumber
        {
            get
            {
                double estimation = 0;
                if (!string.IsNullOrEmpty(Estimation) && !string.IsNullOrWhiteSpace(Estimation))
                {
                    var se = Estimation.Split(' ');
                    double t = 0;
                    foreach (string t1 in se)
                    {
                        if (t1.Contains("h"))
                        {
                            t += Convert.ToInt32(t1.Replace("h", "")) * 60;
                        }
                        if (t1.Contains("m"))
                        {
                            t += Convert.ToInt32(t1.Replace("m", ""));
                        }
                    }
                    estimation = t / 60;
                }
                return estimation;
            }
        }
        public List<Lines> Lines { get; set; }
    }
    public class Lines
    {
        public string UserName { get; set; }
        public string IssueId { get; set; }
        public string IssueUrl { get; set; }
        public string IssueSummary { get; set; }
        public string Duration { get; set; }
        public double DurationNumber
        {
            get
            {
                double duration = 0;
                if (!string.IsNullOrEmpty(Duration) && !string.IsNullOrWhiteSpace(Duration))
                {
                    var sd = Duration.Split(' ');
                    double t = 0;
                    foreach (string t1 in sd)
                    {
                        if (t1.Contains("h"))
                        {
                            t += Convert.ToInt32(t1.Replace("h", "")) * 60;
                        }
                        if (t1.Contains("m"))
                        {
                            t += Convert.ToInt32(t1.Replace("m", ""));
                        }
                    }
                    duration = t / 60;
                }
                return duration;
            }
        }
        public string Estimation { get; set; }
        public double EstimationNumber
        {
            get
            {
                double estimation = 0;
                if (!string.IsNullOrEmpty(Estimation) && !string.IsNullOrWhiteSpace(Estimation))
                {
                    var se = Estimation.Split(' ');
                    double t = 0;
                    foreach (string t1 in se)
                    {
                        if (t1.Contains("h"))
                        {
                            t += Convert.ToInt32(t1.Replace("h", "")) * 60;
                        }
                        if (t1.Contains("m"))
                        {
                            t += Convert.ToInt32(t1.Replace("m", ""));
                        }
                    }
                    estimation = t / 60;
                }
                return estimation;
            }
        }
        public string GroupName { get; set; }
        public List<TypeDurations> TypeDurations { get; set; }
    }
    public class TypeDurations
    {
        public string WorkType { get; set; }
        public string TimeSpent { get; set; }
    }
    public class CreateReport
    {
        public string Type { get; set; }
        public CreateReportParameters Parameters { get; set; }
        public bool Own { get; set; }
        public string Name { get; set; }
    }
    public class CreateReportParameters
    {
        public Range Range { get; set; }
        public List<Projects> Projects { get; set; }
        public string GroupById { get; set; }
    }
    public class Range
    {
        public string Type { get; set; }
        public long From { get; set; }
        public long To { get; set; }
    }
    public class Projects
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
    public class VisibleTo
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public class FinalReport
    {
        public List<UserReport> UserReports { get; set; }
        public List<DateList> DateList { get; set; }
        public double Duration { get; set; }
        public double Estimation { get; set; }
        public double Norm { get; set; }
        public double DurationToday { get; set; }
        public double EstimationToday { get; set; }
        public double NormToday { get; set; }
        public string ProjectName { get; set; }
    }
    public class UserReport
    {
        public string UserName { get; set; }
        public string UserLogin { get; set; }
        public string GroupName
        {
            get
            {
                string groupName = UserName;
                if (!string.IsNullOrEmpty(UserLogin)) groupName += " (" + UserLogin + ")";
                return groupName;
            }
        }
        public List<WorkingDay> WorkingDays { get; set; }
        public double Duration { get; set; }
        public double Estimation { get; set; }
        public double Norm { get; set; }
    }
    public class WorkingDay
    {
        public DateTime Date { get; set; }
        public double Duration { get; set; }
        public double Estimation { get; set; }
        public int Norm { get; set; }
        public int WeekNumber { get; set; }
        public bool IsWorkingDay { get; set; }
    }
    public class DateList
    {
        public DateTime Date { get; set; }
        public double Duration { get; set; }
        public double Estimation { get; set; }
        public int WeekNumber { get; set; }
        public bool IsMaxDateOfWeek { get; set; }
        public double DurationWeek { get; set; }
        public double EstimationWeek { get; set; }
    }
}
