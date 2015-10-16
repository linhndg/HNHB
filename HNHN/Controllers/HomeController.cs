using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HNHB.Models.Entities;// gì mà HNHB nữa? Moi dau ten project sai =)) copy pasr thì cũng phải để ý đầy đủ chứ
//làm lại đi, bây giờ namespace tùm lum hết rồi, nếu biết chỗ sửa thì sửa, dùng find replace all thử, gio doi lai ten het ah uh. 
//bây giờ mày đang dùng 2 cái HNHB do mày copy qua, còn cái HNHB là do code máy tự sinh.
//đáng lẽ nó phải là 1, giớ lam sao, đổi HNHB thành HNHB
// replace all
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
namespace HNHB.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        Entities db = new Entities();
        public ActionResult Index()
        {
            //List<int> CateList = new List<int> { 1, 2, 8, 10 };
            //List<Place> model = db.Places.Where(p => p.IsActive == true && p.IsApproved == true && CateList.Contains(p.SubCategory.CategoryId)).OrderBy(p => p.RateValue).ThenByDescending(p => p.View).Take(8).ToList();
            //ViewBag.Sub = model.GroupBy(p => new { p.SubCategoryId, p.SubCategory.Name }).Select(g => new SubCategory { Id = g.Key.SubCategoryId, Name = g.Key.Name });
            //ViewBag.Ask = db.Questions.Where(q => q.isActive == true).OrderBy(q => q.CreateDate).Take(5).ToList();
            //ViewBag.Article = db.Articles.Where(q => q.isActive == true).OrderBy(q => q.View).Take(8).ToList();
            //return View(model);
           
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Error()
        {
            return View();
        }
    }
}