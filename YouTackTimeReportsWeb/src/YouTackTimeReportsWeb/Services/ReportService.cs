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
            var project = await GetProject(parameters.projectid);
            if(project == null)
            {
                throw new Exception("Произошла ошибка! Проект с таким ID не найден!");
            }
            FinalReport finalReports = new FinalReport();
            DateTime outputStartDate;
            DateTime outputEndDate;
            DateTime.TryParseExact(parameters.datestart + " 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputStartDate);
            DateTime.TryParseExact(parameters.dateend + " 23:59:59", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputEndDate);

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
                var createReport = new CreateReport() { type = "time", parameters = new CreateReportParameters() { range = new Range() { type = "fixed", from = ds, to = de }, projects = new List<Projects>() { new Projects() { name = project.Name, shortName = project.Id } }, groupById = "WORK_AUTHOR" }, own = true, name = reportName };
                //Отправляем создаваемый отчет на сервер, в ответ получаем ID отчета
                string reportId = await CreateReport(createReport);
                //Получаем результаты отчета с сервера
                var report = await GetReport(reportId);
                //Добавляем дополнительные значения в отчет
                report.reportDate = date;
                if (report.reportData.groups.Count > 0) report.reportFill = true;
                //Добавляем отчет в список отчетов
                reports.Add(report);
                //Удаляем отчет с сервера
                var del = await DeleteReport(reportId);
                DateList _dateList = new DateList();
                _dateList.Date = date;
                _dateList.WeekNumber = GetWeekNumber(date);
                finalReports.DateList.Add(_dateList);
                i++;
            }
            //Проверяем пустые отчеты или нет
            if (reports.Where(r => r.reportFill == true).ToList().Count > 0)
            {
                //Получаем список пользователей с сервера
                var users = await GetUsers(null, null, null, parameters.projectid, null, null, null);
                //Получаем настройки тайм трекинга с сервера
                var timesettings = await GetTimeSettings();
                finalReports.UserReports = new List<UserReport>();
                finalReports.ProjectName = project.Name;
                //Подготовительный цикл формирования окончательного отчета, в нем заполняем поля на основе списка пользователей и настроек тайм трекинга
                foreach (var user in users)
                {
                    UserReport userReport = new UserReport();
                    userReport.UserName = user.FullName;
                    userReport.UserLogin = user.Username;
                    userReport.WorkingDays = new List<WorkingDay>();
                    finalReports.DateList.ForEach(date =>
                    {
                        WorkingDay workingDay = new WorkingDay();
                        workingDay.Date = date.Date;
                        workingDay.Duration = 0;
                        workingDay.Estimation = 0;
                        workingDay.Norm = 0;
                        timesettings.WorkWeek.ForEach(workDay =>
                        {
                            WorkDay wd = (WorkDay)workDay;
                            if (wd.ToString().ToLower() == date.Date.DayOfWeek.ToString().ToLower())
                            {
                                workingDay.Norm += timesettings.HoursADay;
                                workingDay.IsWorkingDay = true;
                            }
                        });
                        workingDay.WeekNumber = date.WeekNumber;
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
                        var report = reports.Where(r => r.reportDate == day.Date).FirstOrDefault();
                        if (report != null)
                        {
                            var group = report.reportData.groups.Where(g => g.name == finalReport.GroupName).FirstOrDefault();
                            if (group != null)
                            {
                                day.Duration = group.durationNumber;
                                if (group.durationNumber == 0)
                                {
                                    day.Duration = group.lines.Sum(l => l.durationNumber);
                                }
                                day.Estimation = group.estimationNumber;
                                if (group.estimationNumber == 0)
                                {
                                    day.Estimation = group.lines.Sum(l => l.estimationNumber);
                                }
                            }
                        }
                    });
                    double sum = finalReport.WorkingDays.Sum(d => d.Duration);
                    int sumNorm = finalReport.WorkingDays.Sum(d => d.Norm);
                    double Norm = (sum * 100) / sumNorm;
                    double sumPlan = finalReport.WorkingDays.Sum(d => d.Estimation);
                    finalReport.Estimation = sumPlan;
                    finalReport.Norm = Norm;
                    finalReport.Duration = sum;
                });
                var workingDays = finalReports.UserReports.SelectMany(r => r.WorkingDays);
                finalReports.DateList.ForEach(_dateList =>
                {
                    var maxDate = finalReports.DateList.Where(d => d.WeekNumber == _dateList.WeekNumber).Max(d => d.Date);
                    var Days = workingDays.Where(d => d.WeekNumber == _dateList.WeekNumber);
                    if (maxDate == _dateList.Date)
                    {
                        _dateList.DurationWeek = Days.Sum(l => l.Duration);
                        _dateList.EstimationWeek = Days.Sum(l => l.Estimation);
                        _dateList.IsMaxDateOfWeek = true;
                    }
                    _dateList.Duration = workingDays.Where(d => d.Date == _dateList.Date).Sum(s => s.Duration);
                    _dateList.Estimation = workingDays.Where(d => d.Date == _dateList.Date).Sum(s => s.Estimation);
                });

                DateTime now = DateTime.Now;
                finalReports.DurationToday = workingDays.Where(d => d.Date == now).Sum(l => l.Duration);
                finalReports.EstimationToday = workingDays.Where(d => d.Date == now).Sum(l => l.Estimation);
                finalReports.NormToday = timesettings.HoursADay * finalReports.UserReports.Count;

                finalReports.Duration = workingDays.Sum(d => d.Duration);
                finalReports.Estimation = workingDays.Sum(d => d.Estimation);
                int Weeks = workingDays.GroupBy(w => w.WeekNumber).Count();
                int WorkingDays = workingDays.GroupBy(w => w.Date).Count();
                int weekDays = Weeks * timesettings.DaysAWeek;
                if (Weeks == 1)
                {
                    weekDays = WorkingDays;
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
            Report _report = JsonConvert.DeserializeObject<Report>(response);
            return _report.id;
        }
        private async Task<Report> GetReport(string Id)
        {
            string command = String.Format("current/reports/{0}", Id);
            var queryString = new Dictionary<string, object>();
            queryString["fields"] = "reportData,oldData";
            var response = await _connection.GetJson<Report>(command, queryString);
            return response;
        }
        private async Task<string> DeleteReport(string Id)
        {
            string command = String.Format("current/reports/{0}", Id);
            var response = await _connection.Delete(command);
            return response;
        }
        private async Task<Project> GetProject(string Id)
        {
            string command = String.Format("admin/project/{0}", Id);
            var response = await _connection.Get(command, null);
            XDocument doc = XDocument.Parse(response);
            Project project = SerializationUtil.Deserialize<Project>(doc);
            return project;
        }
        private async Task<TimeSettings> GetTimeSettings()
        {
            string command = "admin/timetracking";
            var response = await _connection.Get(command, null);
            XDocument doc = XDocument.Parse(response);
            TimeSettings timeSettings = SerializationUtil.Deserialize<TimeSettings>(doc);
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
            AllUsers allUsers = SerializationUtil.Deserialize<AllUsers>(doc);
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
                user = SerializationUtil.Deserialize<User>(doc);
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
