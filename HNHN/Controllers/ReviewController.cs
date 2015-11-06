using HNHB.HelperExtension;
using HNHB.Models.Entities;
using FeedFormulation.HelperExtension;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data.Entity;
using HNHB.Models;
using HNHB.DAO;
using System.Device.Location;

namespace HNHB.Controllers
{
    public class ReviewController : Controller
    {
        Entities db = new Entities();
        // GET: Review
        public ActionResult Index()
        {
            return View();
        }
         [Authorize]
        public ActionResult AddReview(int Id, string title, string content, bool? checkRate, List<Image> images, List<Rate> rate)
        {
            if (checkRate == null)
            {
                checkRate = false;
            }
            Review model = new Review
            {
                Title = title,
                Content = content,
                PlaceId = Id,
                isActive = true,
                CreateDate = DateTime.Now,
                UserId = User.Identity.GetUserId<int>(),
                UserProfile = db.UserProfiles.Find(User.Identity.GetUserId<int>())
            };
            db.Reviews.Add(model);
            db.SaveChanges();
            if (images != null && images.Count > 0)
            {
                foreach (var img in images)
                {
                    img.PlaceId = model.PlaceId;
                    img.ReviewId = model.Id;
                    db.Images.Add(img);
                }
                db.SaveChanges();
            }
            if ((bool)checkRate)
            {
                foreach (var rt in rate)
                {
                    rt.ReviewId = model.Id;
                    rt.RateCategory = db.RateCategories.Find(rt.RateCategoryId);
                    db.Rates.Add(rt);
                }
                db.SaveChanges();
                List<Rate> rates = db.Rates.Where(r => r.Review.PlaceId == Id && r.Review.isActive == true).ToList();
                double totalRate = Math.Round((rates.Sum(r => r.Value) / (double)rates.Count()), 1);
                Place place = db.Places.Find(Id);
                place.RateValue = totalRate;
                db.Entry(place).State = EntityState.Modified;
                db.SaveChanges();
            }

            return PartialView("PlaceReview", db.Places.Where(p => p.Id == Id).FirstOrDefault().Reviews.Where(r => r.isActive == true).ToList());
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult RemoveReview(int rvId)
        {
            Review review = db.Reviews.Find(rvId);
            if (review != null)
            {
                review.isActive = false;
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                List<Rate> rates = db.Rates.Where(r => r.Review.PlaceId == review.PlaceId && r.Review.isActive == true).ToList();
                double totalRate = Math.Round((rates.Sum(r => r.Value) / (double)rates.Count()), 1);
                Place place = db.Places.Find(review.PlaceId);
                place.RateValue = totalRate;
                db.Entry(place).State = EntityState.Modified;
                db.SaveChanges();
            }
            return PartialView("PlaceReview", db.Places.Where(p => p.Id == review.PlaceId).FirstOrDefault().Reviews.Where(r => r.isActive == true).ToList());
        }

        [Authorize]
        public ActionResult UploadReviewImage()
        {
            List<Image> images = new List<Image>();
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];
                //Save file content goes here
                List<string> list = ImageUploadExtension.renameUploadFile(file, "Resize");
                if (list != null)
                {
                    Image img = new Image()
                    {
                        ImagePath = list[0],
                        ImageSmallPath = list[1]
                    };
                    images.Add(img);
                }
            }
            return Json(new { isSuccess = true, images = images });
        }

        [HttpPost]
        public ActionResult TotalRate(int Id)
        {
            var rateList = from r in db.Rates where r.Review.PlaceId == Id && r.Review.isActive == true select new { r.RateCategory.Name, r.Value };
            if (rateList != null)
            {
                if (rateList.Count() > 0)
                {
                    var result = from r in rateList
                                 group r by new { r.Name } into g
                                 select new { Name = g.Key.Name, Value = g.Sum(x => x.Value), Count = g.Count() };
                    return Json(new { isSuccess = true, result = result });
                }
                else
                {
                    int placeSubCategory = (from p in db.Places where p.Id == Id select p.SubCategoryId).FirstOrDefault();
                    var result = from a in db.AppliedRateCategories
                                 where a.SubCategoryId == placeSubCategory
                                 select new { Name = a.RateCategory.Name, Value = "--" };
                    return Json(new { isSuccess = true, result = result });
                }
            }
            return Json(new { isSuccess = false });
        }
    }
    }
