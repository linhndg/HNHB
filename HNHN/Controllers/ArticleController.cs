using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HNHB.Models;
using System.Data.Entity;
using Newtonsoft.Json;
using HNHB.HelperExtension;
using System.Data;
using HNHB.Models.Entities;
using HNHB.DAO;

namespace HNHB.Controllers
{
    public class ArticleController : Controller
    {
        Entities db = new Entities();
        // GET: Article
        public ActionResult Index(int? tagId, string key)
        {
            var article = new ArticleDAO().ListArticalByCreateDate();
            ViewBag.lstByView = (from v in db.Articles where v.isActive == true select v)
                .ToList().OrderByDescending(v => v.View).Take(7);
            ViewBag.lstByDate = (from v in db.Articles where v.isActive == true select v)
                .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            ViewBag.lstByComment = (from v in db.Comments where v.Article.isActive == true && v.isActive == true select v)
                            .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            List<int> allTgs = new ArticleDAO().ListAllTag();
            if (tagId != null)
            {
                List<int> listId = new ArticleDAO().ListId(tagId) ;
                article = article.Where(a => listId.Contains(a.Id)).ToList();
            }
            if (key != null)
            {
                article = article.Where(a => a.Title.ToLower().ConvertToUnSign().Contains(key.ToLower().ConvertToUnSign())).ToList();
                ViewBag.Key = key;
            }
            ViewBag.allTags = db.Tags.Where(t => allTgs.Contains(t.Id));
            return View(article);
        }
    }
}