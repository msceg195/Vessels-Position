using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Vessels_Position.Models;

namespace Vessels_Position.Controllers
{
    public class HomeController : Controller
    {
        #region Members

        private static List<Vessels> allData = new List<Vessels>();

        private static string Path => ConfigurationManager.AppSettings["path"];

        public string[] Ports => new string[] { "PUBLIC TERMINAL", "ALEX OLD PORT", "AICT", "SOKHNA", "PORT SAID WEST", "DAMIETTA", "PORT SAID EAST TERMINAL" };

        private static object Port
        {
            get => System.Web.HttpContext.Current.Session["port"];
            set => System.Web.HttpContext.Current.Session["port"] = value;
        }

        #endregion Members

        #region Actions

        public ActionResult Index()
        {
            LoadData();

            return View(allData.Distinct());
        }

        public ActionResult LoadData()
        {
            allData = new List<Vessels>();

            List<FileInfo> files = new DirectoryInfo(Path).GetFiles("*.xls", SearchOption.TopDirectoryOnly).ToList();

            string port = Request.Form["Port"];
            port = string.IsNullOrEmpty(port) ?
            (Port == null ? "-1" : Port.ToString()) : port;
            Port = port.ToString();

            switch (Port)
            {
                case "1":

                    foreach (var file in files)
                    {
                        // Alexandria Sheet
                        GetSheetData("ALEXANDRIA", GetConnectionString(file.FullName));
                    }

                    break;

                case "2":
                    foreach (var file in files)
                    {
                        // Sokhna Sheet
                        GetSheetData("SOKHNA", GetConnectionString(file.FullName));
                    }

                    break;

                case "3":

                    foreach (var file in files)
                    {
                        // Port Said West Sheet
                        GetSheetData("PORT SAID WEST", GetConnectionString(file.FullName));
                    }

                    break;

                case "4":
                    foreach (var file in files)
                    {
                        // DAMIETTA Sheet
                        GetSheetData("DAMIETTA", GetConnectionString(file.FullName));
                    }

                    break;

                case "5":
                    foreach (var file in files)
                    {
                        // Port Said East Sheet
                        GetSheetData("PORT SAID EAST", GetConnectionString(file.FullName));
                    }

                    break;

                default:

                    foreach (var file in files)
                    {
                        // Alexandria Sheet
                        GetSheetData("ALEXANDRIA", GetConnectionString(file.FullName));

                        // Sokhna Sheet
                        GetSheetData("SOKHNA", GetConnectionString(file.FullName));

                        // Port Said West Sheet
                        GetSheetData("PORT SAID WEST", GetConnectionString(file.FullName));

                        // DAMIETTA Sheet
                        GetSheetData("DAMIETTA", GetConnectionString(file.FullName));

                        // Port Said East Sheet
                        GetSheetData("PORT SAID EAST", GetConnectionString(file.FullName));
                    }

                    break;
            }

            allData = allData.OrderBy(q => q?.ARRIVAL_AT_ANCHORAGE).ToList();

            return View(allData.Distinct());
        }

        public ActionResult Reload()
        {
            LoadData();

            return PartialView("_Grid", allData.Distinct());
        }

        #endregion Actions

        #region Methods

        private List<Vessels> FilterData(DataTable data)
        {
            List<Vessels> result = null;
            try
            {
                string period = Request.Form["Period"];

                period = string.IsNullOrEmpty(period) ?
                    (System.Web.HttpContext.Current.Session["period"] == null ? "7" : System.Web.HttpContext.Current.Session["period"].ToString()) : period;

                var halfPeriod = (int.Parse(period.ToString()) / 2);

                System.Web.HttpContext.Current.Session["period"] = period.ToString();

                var startDate = DateTime.Today.AddDays(-halfPeriod);
                var endDate = DateTime.Today.AddDays(halfPeriod);

                var _data = data.AsEnumerable()
                    .Where(q => !q.IsNull(0) && !string.IsNullOrEmpty(q[0].ToString()))
                    .SkipWhile(q => q.IsNull(0) || q[0].ToString() == "SERVICE")
                    .ToList();

                result = _data.Select(q => new Vessels
                {
                    SERVICE = q[0].ToString(),
                    PORT = q[1].ToString(),
                    VESSEL = q[2].ToString(),
                    VOY_NO = q[3].ToString(),
                    ARRIVAL_AT_ANCHORAGE = q.IsNull(4) ? "" : ToDate(q[4].ToString()).ToString(),
                    PILOT_ARRIVAL = q.IsNull(5) ? "" : ToDate(q[5].ToString()).ToString(),
                    BERTHING = q.IsNull(6) ? "" : ToDate(q[6].ToString()).ToString(),
                    OPERATIONS_COMMENCE = q[7].ToString(),
                    OPERATIONS_COMPLETE = q[8].ToString(),
                    SAILED = q[9].ToString(),
                    ROAD = ""
                }).OrderBy(q => q.ARRIVAL_AT_ANCHORAGE).ToList();

                result = result.Where(q =>
                         q.ARRIVAL_AT_ANCHORAGE != "" &&
                         DateTime.Parse(q.ARRIVAL_AT_ANCHORAGE) >= startDate &&
                         DateTime.Parse(q.ARRIVAL_AT_ANCHORAGE) <= endDate).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return result;
        }

        private static string GetConnectionString(string FileName) => string.Format(ConfigurationManager.AppSettings["connection"], FileName);

        private void GetSheetData(string sheetName, string connectionString)
        {
            try
            {
                var sheet = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "$]", connectionString);

                var data = new DataTable();

                sheet.Fill(data);

                var _data = FilterData(data);

                allData.AddRange(_data);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void LogError(Exception ex)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = Server.MapPath("~/Log.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        private DateTime? ToDate(string date)
        {
            DateTime _date;
            try
            {
                if (date.Contains("/") || date.Contains(":"))
                {
                    string[] d = date.Split(new char[] { '/', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);

                    _date = new DateTime(int.Parse("20" + d[2]), int.Parse(d[1]), int.Parse(d[0]), int.Parse(d[3]), int.Parse(d[4]), 0);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return _date;
        }

        #endregion Methods

    }
}