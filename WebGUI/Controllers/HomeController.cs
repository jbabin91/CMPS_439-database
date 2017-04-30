using Microsoft.AspNetCore.Mvc;
using WebGUI.Models;

namespace WebGUI.Controllers {
    public class HomeController : Controller {
        [HttpGet]
        public IActionResult Index() {
            return View();
        }

        [HttpPost]
        public PartialViewResult Command(string query) {
            var relation = WebDatabase.Db.Parse(query);
            var table = new Tables {MyTables = relation};
            return PartialView("~/Views/Home/TablePartial.cshtml", table);
        }

        [HttpGet]
        public IActionResult ListTables() {
            var table = new Tables {MyTables = WebDatabase.Db.Tables};
            return View(table);
        }

        [HttpGet]
        public IActionResult Tutorial() {
            return View();
        }

        [HttpPost]
        public IActionResult CreateGameTut(string query) {
            WebDatabase.Db.Parse(query);
            var ret = WebDatabase.Db.Parse("Print Game");
            var table = new Tables {MyTables = ret};
            return PartialView("~/Views/Home/TablePartial.cshtml", table);
        }

        [HttpPost]
        public IActionResult CreateMovieTut(string query) {
            WebDatabase.Db.Parse(query);
            var ret = WebDatabase.Db.Parse("Print Movie");
            var table = new Tables {MyTables = ret};
            return PartialView("~/Views/Home/TablePartial.cshtml", table);
        }

        [HttpPost]
        public IActionResult ReturTutInput(string query) {
            var ret = WebDatabase.Db.Parse(query);
            var table = new Tables {MyTables = ret};
            return PartialView("~/Views/Home/TablePartial.cshtml", table);
        }
    }
}