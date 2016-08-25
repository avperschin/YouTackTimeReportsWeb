using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using YouTackTimeReportsWeb.Models;
using YouTackTimeReportsWeb.Utils;

namespace YouTackTimeReportsWeb.Services
{
    public class ReportService : IReportService
    {
        readonly IConnection _connection;

        public ReportService(IConnection connection)
        {
            _connection = connection;
        }
        public async Task<FinalReport> GetReports(SearchParams parameters)
        {
            var project = await GetProject(parameters.ProjectId);
            if(project == null)
            {
                throw new Exception("Произошла ошибка! Проект с таким ID не найден!");
            }
            FinalReport finalReports = new FinalReport();
            DateTime outputStartDate;
            DateTime outputEndDate;
            DateTime.TryParseExact(parameters.DateStart + " 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputStartDate);
            DateTime.TryParseExact(parameters.DateEnd + " 23:59:59", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputEndDate);

            TimeSpan oneDay = TimeSpan.FromDays(1);
            int i = 0;
            List<Report> reports = new List<Report>();
            finalReports.DateList = new List<DateList>();
            //Цикл получения результатов отчета по заданному периоду. Из-за их АПИ приходится создавать отчет по каждой дате переода.
            for (DateTime date = outputStartDate; date <= outputEndDate; date += oneDay)
            {
                long ds = UnixTimeStampUTC(date);
                long de = UnixTimeStampUTC(date.AddHours(23).AddMinutes(59).AddSeconds(59));
                string reportName = "TimeReport_" + i;
                //Формируем создаваемый отчет с параметрами: фиксированный, за определенную дату, для заданного проекта с группировкой по пользователям
                var createReport = new CreateReport() { Type = "time", Parameters = new CreateReportParameters() { Range = new Range() { Type = "fixed", From = ds, To = de }, Projects = new List<Projects>() { new Projects() { Name = project.Name, ShortName = project.Id } }, GroupById = "WORK_AUTHOR" }, Own = true, Name = reportName };
                //Отправляем создаваемый отчет на сервер, в ответ получаем ID отчета
                var reportId = await CreateReport(createReport);
                //Получаем результаты отчета с сервера
                var report = await GetReport(reportId);
                //Добавляем дополнительные значения в отчет
                report.ReportDate = date;
                if (report.ReportData.Groups.Count > 0) report.ReportFill = true;
                //Добавляем отчет в список отчетов
                reports.Add(report);
                //Удаляем отчет с сервера
                if (reportId != null) await DeleteReport(reportId);
                DateList dateList = new DateList
                {
                    Date = date,
                    WeekNumber = GetWeekNumber(date)
                };
                finalReports.DateList.Add(dateList);
                i++;
            }
            //Проверяем пустые отчеты или нет
            if (reports.Where(r => r.ReportFill).ToList().Count > 0)
            {
                //Получаем список пользователей с сервера
                var users = await GetUsers(null, null, null, parameters.ProjectId);
                //Получаем настройки тайм трекинга с сервера
                var timesettings = await GetTimeSettings();
                finalReports.UserReports = new List<UserReport>();
                finalReports.ProjectName = project.Name;
                //Подготовительный цикл формирования окончательного отчета, в нем заполняем поля на основе списка пользователей и настроек тайм трекинга
                foreach (var user in users)
                {
                    UserReport userReport = new UserReport
                    {
                        UserName = user.FullName,
                        UserLogin = user.Username,
                        WorkingDays = new List<WorkingDay>()
                    };
                    finalReports.DateList.ForEach(date =>
                    {
                        WorkingDay workingDay = new WorkingDay
                        {
                            Date = date.Date,
                            Duration = 0,
                            Estimation = 0,
                            Norm = 0,
                            WeekNumber = date.WeekNumber
                        };
                        timesettings.WorkWeek.ForEach(workDay =>
                        {
                            WorkDay wd = (WorkDay)workDay;
                            if (wd.ToString().ToLower() == date.Date.DayOfWeek.ToString().ToLower())
                            {
                                workingDay.Norm += timesettings.HoursADay;
                                workingDay.IsWorkingDay = true;
                            }
                        });
                        userReport.WorkingDays.Add(workingDay);
                    });
                    finalReports.UserReports.Add(userReport);
                }
                //Второй цикл формирования окончательного отчета
                finalReports.UserReports.ForEach(finalReport =>
                {
                    //Третий цикл формирования окончательного отчета, в нем заполняем для каждого пользователя данные о запланированных часах и отработанных часах
                    finalReport.WorkingDays.ForEach(day =>
                    {
                        var report = reports.FirstOrDefault(r => r.ReportDate == day.Date);
                        var group = report?.ReportData.Groups.FirstOrDefault(g => g.Name == finalReport.GroupName);
                        if (@group != null)
                        {
                            day.Duration = @group.DurationNumber;
                            if (@group.DurationNumber == 0)
                            {
                                day.Duration = @group.Lines.Sum(l => l.DurationNumber);
                            }
                            day.Estimation = @group.EstimationNumber;
                            if (@group.EstimationNumber == 0)
                            {
                                day.Estimation = @group.Lines.Sum(l => l.EstimationNumber);
                            }
                        }
                    });
                    double sum = finalReport.WorkingDays.Sum(d => d.Duration);
                    int sumNorm = finalReport.WorkingDays.Sum(d => d.Norm);
                    double norm = (sum * 100) / sumNorm;
                    double sumPlan = finalReport.WorkingDays.Sum(d => d.Estimation);
                    finalReport.Estimation = sumPlan;
                    finalReport.Norm = norm;
                    finalReport.Duration = sum;
                });
                var workingDays = finalReports.UserReports.SelectMany(r => r.WorkingDays);
                finalReports.DateList.ForEach(dateList =>
                {
                    var maxDate = finalReports.DateList.Where(d => d.WeekNumber == dateList.WeekNumber).Max(d => d.Date);
                    var days = workingDays.Where(d => d.WeekNumber == dateList.WeekNumber).ToList();
                    if (maxDate == dateList.Date)
                    {
                        dateList.DurationWeek = days.Sum(l => l.Duration);
                        dateList.EstimationWeek = days.Sum(l => l.Estimation);
                        dateList.IsMaxDateOfWeek = true;
                    }
                    dateList.Duration = workingDays.Where(d => d.Date == dateList.Date).Sum(s => s.Duration);
                    dateList.Estimation = workingDays.Where(d => d.Date == dateList.Date).Sum(s => s.Estimation);
                });

                DateTime now = DateTime.Now;
                finalReports.DurationToday = workingDays.Where(d => d.Date == now).Sum(l => l.Duration);
                finalReports.EstimationToday = workingDays.Where(d => d.Date == now).Sum(l => l.Estimation);
                finalReports.NormToday = timesettings.HoursADay * finalReports.UserReports.Count;

                finalReports.Duration = workingDays.Sum(d => d.Duration);
                finalReports.Estimation = workingDays.Sum(d => d.Estimation);
                int weeks = workingDays.GroupBy(w => w.WeekNumber).Count();
                int workingDaysCount = workingDays.GroupBy(w => w.Date).Count();
                int weekDays = weeks * timesettings.DaysAWeek;
                if (weeks == 1)
                {
                    weekDays = workingDaysCount;
                }
                finalReports.Norm = timesettings.HoursADay * weekDays * finalReports.UserReports.Count;
            }
            else
            {
                throw new Exception("За выбранный Вами период, по данному проекту не проводилось работ!");
            }
            return finalReports;
        }
        private async Task<string> CreateReport(CreateReport report)
        {
            string command = "current/reports";
            var response = await _connection.PostJson(command, report);
            Report rep = JsonConvert.DeserializeObject<Report>(response);
            return rep.Id;
        }
        private async Task<Report> GetReport(string id)
        {
            string command = string.Format("current/reports/{0}", id);
            var queryString = new Dictionary<string, object> {["fields"] = "reportData,oldData"};
            var response = await _connection.GetJson<Report>(command, queryString);
            return response;
        }
        private async Task DeleteReport(string id)
        {
            string command = string.Format("current/reports/{0}", id);
            await _connection.Delete(command);
        }
        private async Task<Project> GetProject(string id)
        {
            string command = string.Format("admin/project/{0}", id);
            var response = await _connection.Get(command, null);
            XDocument doc = XDocument.Parse(response);
            Project project = doc.Deserialize<Project>();
            return project;
        }
        private async Task<TimeSettings> GetTimeSettings()
        {
            string command = "admin/timetracking";
            var response = await _connection.Get(command, null);
            XDocument doc = XDocument.Parse(response);
            TimeSettings timeSettings = doc.Deserialize<TimeSettings>();
            return timeSettings;
        }
        private async Task<List<User>> GetUsers(string query = null, string group = null, string role = null,
                                          string project = null, string permission = null, bool? onlineOnly = null,
                                          int? start = null)
        {
            string command = "admin/user";
            var queryString = new Dictionary<string, object>();

            if (query != null) queryString["q"] = query;
            if (group != null) queryString["group"] = group;
            if (role != null) queryString["role"] = role;
            if (project != null) queryString["project"] = project;
            if (permission != null) queryString["permission"] = permission;
            if (onlineOnly != null) queryString["onlineOnly"] = onlineOnly == true ? "true" : "false";
            if (start != null) queryString["start"] = start.ToString();
            var response = await _connection.Get(command, queryString);
            XDocument doc = XDocument.Parse(response);
            AllUsers allUsers = doc.Deserialize<AllUsers>();
            List<User> users = new List<User>();
            foreach (var userItem in allUsers.UserList)
            {
                var user = await GetUserByUserName(userItem.Login);
                if (user != null) users.Add(user);
            }

            return users;
        }

        private async Task<User> GetUserByUserName(string username)
        {
            string command = String.Format("user/bylogin/{0}", username);
            var response = await _connection.Get(command, null);

            User user = new User();

            if (response != null)
            {
                XDocument doc = XDocument.Parse(response);
                user = doc.Deserialize<User>();
                user.Username = username;
            }
            return user;
        }
        /// <summary>
        /// Получаем номер недели по дате
        /// </summary>
        /// <param name="date">Обязательный параметр, передается для получения номера недели</param>
        /// <returns>Возвращается число, полученного номера недели</returns>
        private int GetWeekNumber(DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return weekNum;
        }
        /// <summary>
        /// Конвертация даты в милисекунды
        /// </summary>
        /// <param name="date">Обязательный параметр, передается для конвертации</param>
        /// <returns>Возвращается число, дата сконвертированная в милисекунды</returns>
        private Int64 UnixTimeStampUTC(DateTime date)
        {
            Int64 unixTimeStamp;
            DateTime zuluTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            unixTimeStamp = (Int64)(zuluTime.Subtract(unixEpoch)).TotalMilliseconds;
            return unixTimeStamp;
        }
    }
}
