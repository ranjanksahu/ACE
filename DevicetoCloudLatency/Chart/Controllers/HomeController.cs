using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chart.Models;
using DBOperations;
using Dataobjects;

namespace Chart.Controllers
{
    public class HomeController : Controller
    {
        MongoOperations mongodbsi = new MongoOperations("South India");
        MongoOperations mongodbwestus = new MongoOperations("West US");
        public IActionResult Index()
        {
            return View("SIChart");
        }

        public IActionResult WestUS()
        {
            return View("WestUsChart");
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetChartDataSI()
        {
            ChartData chartdata = mongodbsi.GetChartData();
            return Json(new { chartData = chartdata });
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetOtherChartDataSI()
        {
            var avgTime = mongodbsi.GetAverageSentReceivedTimedifference();
            var maxTime = mongodbsi.GetMaxSentReceivedTimedifference();
            var minTime = mongodbsi.GetMinSentReceivedTimedifference();

            return Json(new { AverageTime = avgTime, MaxTime = maxTime, MinTime = minTime });
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetTimeRangewithCountSI()
        {
            Dictionary<string, int> range = mongodbsi.GetTimeRangewithCount();
            return Json(new { RangeData = range });
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetChartDataWestUS()
        {
            ChartData chartdata = mongodbwestus.GetChartData();
            return Json(new { chartData = chartdata });
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetOtherChartDataWestUS()
        {
            var avgTime = mongodbwestus.GetAverageSentReceivedTimedifference();
            var maxTime = mongodbwestus.GetMaxSentReceivedTimedifference();
            var minTime = mongodbwestus.GetMinSentReceivedTimedifference();

            return Json(new { AverageTime = avgTime, MaxTime = maxTime, MinTime = minTime });
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetTimeRangewithCountWestUS()
        {
            Dictionary<string, int> range = mongodbwestus.GetTimeRangewithCount();
            return Json(new { RangeData = range });
        }




        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
