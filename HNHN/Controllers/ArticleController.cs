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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace HNHB.Controllers
{
    public class ArticleController : Controller
    {
        Entities db = new Entities();
        ArticleDAO ArticalDAO = new ArticleDAO();
        // GET: Article
        public ActionResult Index(int? tagId, string key)
        {
            var article = ArticalDAO.ListArticalByCreateDate();
            ViewBag.lstByView = (from v in db.Articles where v.isActive == true select v)
                .ToList().OrderByDescending(v => v.View).Take(7);
            ViewBag.lstByDate = (from v in db.Articles where v.isActive == true select v)
                .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            ViewBag.lstByComment = (from v in db.Comments where v.Article.isActive == true && v.isActive == true select v)
                            .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            List<int> allTgs = ArticalDAO.ListAllTag();
            if (tagId != null)
            {
                List<int> listId = ArticalDAO.ListId(tagId);
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
        [HttpGet]
        [Authorize]
        // Create Artical
        public ActionResult CreateArticle()
        {
            var lstTags = ArticalDAO.ListTag();
            return View(lstTags);
        }
        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateArticle(string editorContent, string tagSelect, string stringTitle)
        {
            var appliedTagList = JsonConvert.DeserializeObject<string[]>(tagSelect);
            int arId = 0;
            if (ModelState.IsValid)
            {
                Article article = new Article();

                article.Title = stringTitle;
                while (editorContent.EndsWith("<br>"))
                    editorContent = editorContent.Substring(0, editorContent.Length - 4);
                article.Content = editorContent.Trim('\r', '\n', ' ');
                article.UserId = User.Identity.GetUserId<int>();
                article.CreateDate = DateTime.Now;
                article.isActive = true;
                article.View = 0;

                db.Articles.Add(article);
                db.SaveChanges();
                arId = article.Id;
                if (appliedTagList != null)
                {
                    foreach (var item in appliedTagList)
                    {
                        AppliedTagArticle appliedTag = new AppliedTagArticle();
                        appliedTag.ArticleId = article.Id;
                        appliedTag.TagId = Int32.Parse(item);
                        //appliedTag.TaggingUser = WebSecurity.CurrentUserId;
                        appliedTag.AppliedDate = DateTime.Now;
                        db.AppliedTagArticles.Add(appliedTag);
                        db.SaveChanges();
                    }
                }
            }
            return Json(arId, JsonRequestBehavior.AllowGet);
        }
        // View Artical Details
        public ActionResult ArticleDetails(int? id = 0)
        {

            if (id == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            Article ar = (from a in db.Articles where a.isActive == true && a.Id == id select a).FirstOrDefault();
            if (ar != null)
            {
                ar.View++;
                db.SaveChanges();
                ViewBag.lstByView = (from v in db.Articles where v.isActive == true select v)
                    .ToList().OrderByDescending(v => v.View).Take(7);
                ViewBag.lstByDate = (from v in db.Articles where v.isActive == true select v)
                    .ToList().OrderByDescending(v => v.CreateDate).Take(7);
                ViewBag.lstByComment = (from v in db.Comments where v.Article.isActive == true && v.isActive == true select v)
                                .ToList().OrderByDescending(v => v.CreateDate).Take(7);
                var allTgs = (from tg in db.Tags
                              from apptag in tg.AppliedTagArticles
                              where apptag.TagId == tg.Id
                              select tg).Distinct();

                ViewBag.allTags = allTgs.OrderBy(x => x.Name).ToList();

                return View(ar);
            }
            return RedirectToAction("Error", "Home");
        }

        // Article comment using Ajax
        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult ArticleComment(string arContent, int arId = 0)
        {
            if (ModelState.IsValid)
            {
                Comment cm = new Comment();
                cm.ArticleId = arId;
                cm.Content = arContent;
                cm.UserId = User.Identity.GetUserId<int>();
                cm.CreateDate = DateTime.Now;
                cm.isActive = true;

                db.Comments.Add(cm);
                db.SaveChanges();
            }


            var comment = ArticalDAO.GetListComment(arId);
            return PartialView("ArCommentPartial", comment);
        }
        // Remove Comment by admin
        [Authorize(Roles = "Administrator")]
        public ActionResult RemoveComment(int cmtId)
        {
            Comment cmt = db.Comments.Find(cmtId);
            if (cmt != null)
            {
                cmt.isActive = false;
                db.Entry(cmt).State = EntityState.Modified;
                db.SaveChanges();
            }
            var comment = (from cm in db.Comments.Include("UserProfile")
                           where cm.ArticleId == cmt.ArticleId && cm.isActive == true
                           select cm).ToList().OrderByDescending(c => c.CreateDate);
            return PartialView("ArCommentPartial", comment);
        }
         

        // Edit artical
        [Authorize]
        public ActionResult EditArticle(int? id)
        {
            //if (!(db.Articles.Any(a => a.Id == id && a.isActive == true && a.UserId == User.Identity.GetUserId<int>())))
            //{
            //    return RedirectToAction("Error", "Home");
            //}
            var arContent = db.Articles.Find(id);
            ViewBag.LstTags = (from tag in db.Tags select tag).ToList();
            return View(arContent);
        }

        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult EditArticle(string articleId, string editorContent, string tagSelect, string stringTitle)
        {
            int id = Int32.Parse(articleId);
            var appliedTagList = JsonConvert.DeserializeObject<string[]>(tagSelect);
            if (ModelState.IsValid)
            {
                Article article = (from qt in db.Articles
                                   where qt.Id == id && qt.isActive == true
                                   select qt).FirstOrDefault();

                article.Title = stringTitle;
                while (editorContent.EndsWith("<br>"))
                    editorContent = editorContent.Substring(0, editorContent.Length - 4);
                article.Content = editorContent.Trim('\r', '\n', ' ');
                article.CreateDate = DateTime.Now;
                article.isActive = true;
                //asking. = 0;

                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();

                // Delete applied tags in question
                var appliedTags = (from apTag in db.AppliedTagArticles
                                   where apTag.ArticleId == id
                                   select apTag).ToList();
                foreach (var item in appliedTags)
                {
                    var delTag = db.AppliedTagArticles.Where(a => a.ArticleId == id
                                                                        && a.TagId == item.TagId).Single();
                    db.AppliedTagArticles.Remove(delTag);
                }
                db.SaveChanges();
                //Save Applied tags
                if (appliedTagList != null)
                {
                    foreach (var item in appliedTagList)
                    {
                        AppliedTagArticle appliedTag = new AppliedTagArticle();
                        appliedTag.ArticleId = article.Id;
                        appliedTag.TagId = Int32.Parse(item);
                        //appliedTag.TaggingUser = WebSecurity.CurrentUserId;
                        appliedTag.AppliedDate = DateTime.Now;
                        db.AppliedTagArticles.Add(appliedTag);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        //Deactive artical
        [Authorize(Roles = "Administrator")]
        public ActionResult Deactive(int? id)
        {
            if (id != null)
            {
                Article article = (from ar in db.Articles where ar.Id == id select ar).FirstOrDefault();
                if (ModelState.IsValid)
                {
                    article.isActive = false;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Article");
        }

    }
}