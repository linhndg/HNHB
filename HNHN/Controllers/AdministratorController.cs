using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using HNHB.Models.Entities;
using System.Text;
using System.Data;
using FeedFormulation.HelperExtension;
using WebMatrix.Data;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;


namespace HNHB.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : Controller
    {
        // Get Database
        private Entities dbContext = new Entities();
        //
        // GET: /Administrator/
        //Get data for index page
        public ActionResult Index()
        {
            ViewBag.NumOfPlace = dbContext.Places.Where(p => p.IsActive == true).Count();
            ViewBag.NumOfUser = dbContext.UserProfiles.Where(u => u.isActive == true).Count();
            ViewBag.NumOfArticle = dbContext.Articles.Where(a => a.isActive == true).Count();
            ViewBag.NumOfQuestion = dbContext.Questions.Where(q => q.isActive == true).Count();
            ViewBag.NumOfComment = dbContext.Answers.Where(a => a.UserProfile.isActive == true).Count() + dbContext.Comments.Where(c => c.UserProfile.isActive == true).Count() + dbContext.Reviews.Where(r => r.UserProfile.isActive == true).Count();
            ViewBag.Report = dbContext.Reports.Where(r => r.isChecked == false).ToList();
            return View();
        }
        //Get data for manage category
        public ActionResult ManageCategory()
        {
            var category = (from ct in dbContext.Categories where ct.isActive == true select ct).ToList();
            return View(category);
        }
        //Partial to run Ajax update
        public PartialViewResult CategoryUpdate()
        {
            return PartialView();
        }
        // Update Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryUpdate(Category model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    model.isActive = true;
                    dbContext.Categories.Add(model);
                    dbContext.SaveChanges();
                }
                else
                {
                    Category cate = dbContext.Categories.Find(model.Id);
                    cate.Name = model.Name;
                    dbContext.Entry(cate).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            return RedirectToAction("ManageCategory", "Administrator");
        }
        // Change data Sub Category
        public ActionResult ManageSubCategory(int? id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            var subcategory = (from sct in dbContext.SubCategories
                               where sct.CategoryId == id && sct.isActive == true
                               select sct).ToList();
            ViewBag.CtId = id;
            ViewBag.CtName = dbContext.Categories.Find(id).Name;
            return View(subcategory);
        }
        // Run partial to use Ajax in SubCategory Update
        public PartialViewResult SubCategoryUpdate()
        {
            return PartialView();
        }
        // Change data sub category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubCategoryUpdate(SubCategory model, HttpPostedFileBase uploadIcon)
        {
            int ctId = model.CategoryId;
            if (ModelState.IsValid)
            {
                if (uploadIcon != null && uploadIcon.ContentLength > 0)
                {
                    model.Icon = ImageUploadExtension.renameUploadIcon(uploadIcon);
                }
                if (model.Id == 0)
                {
                    model.isActive = true;
                    dbContext.SubCategories.Add(model);
                    dbContext.SaveChanges();
                }
                else
                {
                    SubCategory sc = dbContext.SubCategories.Find(model.Id);
                    sc.Name = model.Name;
                    if (model.Icon != null)
                    {
                        sc.Icon = model.Icon;
                    }
                    dbContext.Entry(sc).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            return RedirectToAction("ManageSubCategory", "Administrator", new { id = ctId });
        }
        // Get list User
        #region[ManageUser]
        public ActionResult ManageUser()
        {
            int CurrentID = User.Identity.GetUserId<int>();
            var users = (from user in dbContext.UserProfiles where user.Id != CurrentID select user).ToList();
            return View(users);
        }
        // Active or Deactive User
        public ActionResult ChangeUsersActive(int id)
        {
            UserProfile user = dbContext.UserProfiles.Find(id);
            if (user.isActive == true)
            {
                user.isActive = false;
            }
            else
            {
                user.isActive = true;
            }
            dbContext.Entry(user).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManageUser", "Administrator");
        }

        public ActionResult ChangeUsersRole(int id)
        {
            UserProfile user = dbContext.UserProfiles.Find(id);
            if (!Roles.IsUserInRole(user.UserName, "Administrator"))
            {
                Roles.AddUserToRole(user.UserName, "Administrator");
            }
            else
            {
                Roles.RemoveUserFromRole(user.UserName, "Administrator");
            }
            return RedirectToAction("ManageUser", "Administrator");
        }
        #endregion
        // Manage Coordinate
        #region[ManageCoordinate]
        public ActionResult ManageCoordinate()
        {
            var coordinate = (from coor in dbContext.Coordinates select coor).ToList();
            var place = dbContext.Places.ToList();
            ViewBag.place = place;
            return View(coordinate);
        }
        public PartialViewResult CoordinateUpdate()
        {
            return PartialView();
        }
        // Update Coordinate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CoordinateUpdate(Coordinate model)
        {
            if (ModelState.IsValid)
            {
                if (model.PlaceId == 0)
                {
                    dbContext.Coordinates.Add(model);
                    dbContext.SaveChanges();
                }
                else
                {
                    dbContext.Entry(model).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            return RedirectToAction("ManageCoordinate", "Administrator");
        }
        #endregion
        // Manage Rate Category
        #region[ManageRateCategory]
        public ActionResult ManageRateCategory()
        {
            var ratecate = (from rate in dbContext.RateCategories select rate).ToList();

            return View(ratecate);
        }
        public PartialViewResult RateCategoryUpdate()
        {
            return PartialView();
        }
        // Update Rate Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RateCategoryUpdate(RateCategory model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    dbContext.RateCategories.Add(model);
                    dbContext.SaveChanges();
                }
                else
                {
                    dbContext.Entry(model).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            return RedirectToAction("ManageRateCategory", "Administrator");
        }
        #endregion
        // Get list all tag
        #region[ManageTag]
        public ActionResult ManageTag()
        {
            var tags = (from tag in dbContext.Tags select tag).ToList();

            return View(tags);
        }
        public PartialViewResult TagUpdate()
        {
            return PartialView();
        }
        // Save tag to database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TagUpdate(Tag model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    dbContext.Tags.Add(model);
                    dbContext.SaveChanges();
                }
                else
                {
                    dbContext.Entry(model).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            return RedirectToAction("ManageTag", "Administrator");
        }
        #endregion
        //Get all question
        #region[ManageQuestion]
        public ActionResult ManageQuestion()
        {
            var question = (from ques in dbContext.Questions select ques).ToList();
            return View(question);
        }
        // Active or deactive question
        [HttpPost]
        public ActionResult ChangeQuestionsActive(int id)
        {
            Question question = dbContext.Questions.Find(id);
            if (question.isActive)
            {
                question.isActive = false;
            }
            else
            {
                question.isActive = true;
            }
            dbContext.Entry(question).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManageQuestion", "Administrator");
        }
        #endregion
        //Get all Artical
        public ActionResult ManageArticle()
        {
            return View(dbContext.Articles.ToList());
        }
        //Active or deactive artical
        public ActionResult ChangeArticlesActive(int id)
        {
            Article article = dbContext.Articles.Find(id);
            if (article.isActive)
            {
                article.isActive = false;
            }
            else
            {
                article.isActive = true;
            }
            dbContext.Entry(article).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManageArticle", "Administrator");
        }
        // Chaneg state of report
        public void ReportCheck(int id)
        {
            Report report = dbContext.Reports.Find(id);
            report.isChecked = true;
            dbContext.Entry(report).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
        // Get all place
        public ActionResult ManagePlace()
        {
            return View(dbContext.Places.ToList());
        }
        // Active or deactive Place
        public ActionResult ChangePlaceActive(int id)
        {
            Place place = dbContext.Places.Find(id);
            if (place.IsActive)
            {
                place.IsActive = false;
                SubCategory sub = dbContext.SubCategories.Find(place.SubCategoryId);
                sub.NumberOfPlace -= 1;
                dbContext.Entry(sub).State = EntityState.Modified;
            }
            else
            {
                place.IsActive = true;
                SubCategory sub = dbContext.SubCategories.Find(place.SubCategoryId);
                sub.NumberOfPlace += 1;
                dbContext.Entry(sub).State = EntityState.Modified;
            }
            dbContext.Entry(place).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManagePlace", "Administrator");
        }
        // Approve or not approve Place
        public ActionResult ChangePlaceApprove(int id)
        {
            Place place = dbContext.Places.Find(id);
            if (place.IsApproved)
            {
                place.IsApproved = false;
            }
            else
            {
                place.IsApproved = true;
            }
            dbContext.Entry(place).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManagePlace", "Administrator");
        }
        // Manage Applied Rate
        public ActionResult ManageAppiledRate(int id)
        {
            ViewBag.SCtId = id;
            ViewBag.SCtName = dbContext.SubCategories.Find(id).Name;
            return View(dbContext.AppliedRateCategories.Where(a => a.SubCategoryId == id));
        }
        // Delete Applied Rate
        public ActionResult DeleteAppliedRate(int subcategory, int ratecategory)
        {
            AppliedRateCategory model = dbContext.AppliedRateCategories.Where(a => a.RateCategoryId == ratecategory && a.SubCategoryId == subcategory).FirstOrDefault();
            dbContext.AppliedRateCategories.Remove(model);
            dbContext.SaveChanges();
            return RedirectToAction("ManageAppiledRate", "Administrator", new { id = subcategory });
        }
        // Change Applied Rate
        public ActionResult AppliedRateUpdate(int id)
        {
            var listAppliedRate = dbContext.AppliedRateCategories.Where(a => a.SubCategoryId == id).Select(a => a.RateCategoryId).ToList();
            ViewBag.RateCategoryId = new SelectList(dbContext.RateCategories.Where(r => !listAppliedRate.Contains(r.Id)), "Id", "Name");
            return PartialView();
        }

        [HttpPost]
        public ActionResult AppliedRateUpdate(AppliedRateCategory model)
        {
            model.AppliedDate = DateTime.Now;
            dbContext.AppliedRateCategories.Add(model);
            dbContext.SaveChanges();
            return RedirectToAction("ManageAppiledRate", "Administrator", new { id = model.SubCategoryId });
        }
        // Approve Virtual Tourist
        public ActionResult ManageVirtualTourist()
        {
            List<Place> model = dbContext.Places.Where(p => p.Virtualtourist != null).ToList();
            ViewBag.PlaceId = new SelectList(dbContext.Places.Where(p => p.IsActive == true && p.IsApproved == true), "Id", "Name");
            return View(model);
        }
        // Insert Virtual Tour
        public ActionResult AddVirtualTourist(int PlaceId, string virtualTouristLink)
        {
            Place model = dbContext.Places.Find(PlaceId);
            model.Virtualtourist = virtualTouristLink;
            dbContext.Entry(model).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManageVirtualTourist", "Administrator");
        }

        public ActionResult DeleteVirtualTourist(int id)
        {
            Place model = dbContext.Places.Find(id);
            model.Virtualtourist = null;
            dbContext.Entry(model).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("ManageVirtualTourist", "Administrator");
        }

    

    

       
    }
}
