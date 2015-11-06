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
    public class PlaceController : Controller
    {
        // GET: Place
        Entities db = new Entities();
        public ActionResult Index(string layoutSearch, int? tag, int? subcategory)
        {
            ViewBag.DistrictId = new SelectList(db.Districts, "Id", "DistrictName");
            if (tag != null)
            {
                ViewBag.TagId = new SelectList(db.Tags, "Id", "Name", tag);
            }
            else
            {
                ViewBag.TagId = new SelectList(db.Tags, "Id", "Name");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategories = from s in db.SubCategories
                                    where s.isActive == true
                                    select s;
            if (subcategory != null)
            {
                ViewBag.SelectedSub = subcategory;
            }
            if (layoutSearch != null && layoutSearch.Trim().Length > 0)
            {
                layoutSearch = layoutSearch.Trim();
                ViewBag.LayoutSearch = layoutSearch;
            }
            return View();
        }

        public ActionResult GetPlaceList(int? page, string search, string sort, List<int> district, List<int> tag, List<int> subcategory)
        {
            //Prepare paging
            if (page == null)
            {
                page = 1;
            }
            int num = (int)page * 8;
            List<Place> place = db.Places.Where(p => p.IsActive == true).ToList();
            ViewBag.SearchKey = "";
            //Search
            if (search != null && search.Trim().Length > 0)
            {
                ViewBag.SearchKey = search;
                search = search.Trim().ToLower().ToUnsign();
                List<int> placebyitem = new List<int>();
                foreach (var item in db.Items)
                {
                    if (placebyitem.Contains(item.PlaceId))
                        continue;
                    var name = item.Name.ToLower().ToUnsign();
                    if (BreakStringsAndCheck(search, name) >= 50 || name.Contains(search))
                    {
                        placebyitem.Add(item.PlaceId);
                    }
                }
                place = place.Where(p => BreakStringsAndCheck(search, p.Name.ToLower().ToUnsign()) >= 50 || p.Name.ToLower().ToUnsign().Contains(search) || placebyitem.Contains(p.Id)).ToList();
            }
            //District Filter
            if (district != null && district.Count > 0)
            {
                place = place.Where(p => district.Contains(p.DistrictId)).ToList();
                ViewBag.DistrictId = new MultiSelectList(db.Districts, "Id", "DistrictName", district);
            }
            else
            {
                ViewBag.DistrictId = new SelectList(db.Districts, "Id", "DistrictName");
            }
            //SubCategory Filter
            if (subcategory != null && subcategory.Count > 0)
            {
                place = place.Where(p => subcategory.Contains(p.SubCategoryId)).ToList();
                ViewBag.SubCategoryId = new MultiSelectList(db.SubCategories, "Id", "Name", subcategory);
            }
            else
            {
                if (ViewBag.SearchKey == "")
                {
                    List<int> CateList = new List<int> { 1, 2, 8, 10 };
                    place = place.Where(p => CateList.Contains(p.SubCategory.CategoryId)).ToList();
                }
                ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            }
            //Tag Filter
            if (tag != null && tag.Count > 0)
            {
                List<int> listId = db.AppliedTagPlaces.Where(a => tag.Contains(a.TagId)).Select(a => a.PlaceId).Distinct().ToList();
                place = place.Where(p => listId.Contains(p.Id)).ToList();
                ViewBag.TagId = new MultiSelectList(db.Tags, "Id", "Name", tag);
            }
            else
            {
                ViewBag.TagId = new SelectList(db.Tags, "Id", "Name");
            }
            ViewBag.NumOfPlace = place.Count;
            ViewBag.Page = page;
            //Sort
            if (sort == null)
            {
                sort = "view";
            }
            switch (sort.ToString())
            {
                case "view": place = place.OrderByDescending(p => p.View).ToList();
                    break;
                case "new": place = place.OrderByDescending(p => p.CreateDate).ToList();
                    break;
                case "rate": place = place.OrderByDescending(p => p.RateValue).ToList();
                    break;
                case "review": place = place.OrderByDescending(p => p.Reviews.Count()).ToList();
                    break;
                case "lowprice": place = place.OrderBy(p => p.Items.FirstOrDefault() != null ? p.Items.FirstOrDefault().Price : double.MaxValue).ToList();
                    break;
            }
            ViewBag.Sort = sort.ToString();
            return PartialView(place.Take(num));
        }

        public JsonResult GetSubCategories(int id)
        {
            var data = from s in db.SubCategories
                       where s.CategoryId == id
                       select new { Id = s.Id, Name = s.Name };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Place/Create
        [Authorize]
        public ActionResult Create()
        {
            int first = db.Categories.Where(c => c.isActive == true && c.SubCategories.Count > 0).FirstOrDefault().Id;
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories.Where(s => s.CategoryId == first), "Id", "Name");
            ViewBag.DistrictId = new SelectList(db.Districts, "Id", "DistrictName");
            ViewBag.TagId = new SelectList(db.Tags, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(Place model)
        {
            if (ModelState.IsValid)
            {
                model.IsApproved = true;
                //model.IsApproved = false;
                //if (Roles.IsUserInRole(User.Identity.Name, "Administrator"))
                //{
                //    model.IsApproved = true;
                //}
                Place place = new Place()
                {


                    Name = model.Name,
                    Address = model.Address,
                    SubCategoryId = model.SubCategoryId,
                    DistrictId = model.DistrictId,
                    UserId = User.Identity.GetUserId<int>(),
                    PhoneNumber = model.PhoneNumber,
                    Description = model.Description,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    IsApproved = model.IsApproved,
                    RateValue = 0,
                    View = 0,
                    Virtualtourist = model.Virtualtourist

                };
                db.Places.Add(place);
                db.SaveChanges();
                Coordinate coordinate = CoordinateExtension.GetCoordinates(model.Address);
                if (coordinate != null)
                {
                    coordinate.PlaceId = place.Id;
                    db.Coordinates.Add(coordinate);
                    db.SaveChanges();
                }
                else
                {
                    coordinate = new Coordinate
                    {
                        PlaceId = place.Id,
                        Latitude = 16.0466743,
                        Longitude = 108.206706
                    };
                }
                if (model.SelectedTagIds != null && model.SelectedTagIds.Count > 0)
                {
                    foreach (var tag in model.SelectedTagIds)
                    {
                        AppliedTagPlace appliedTag = new AppliedTagPlace()
                        {
                            PlaceId = place.Id,
                            TagId = tag,
                            AppliedDate = place.CreateDate
                        };
                        db.AppliedTagPlaces.Add(appliedTag);
                    }
                    db.SaveChanges();
                }
                if (model.Items.Count > 0)
                {
                    foreach (var item in model.Items)
                    {
                        Item addedItem = new Item()
                        {
                            Name = item.Name,
                            Price = item.Price,
                            PlaceId = place.Id,
                            isActive = true
                        };
                        db.Items.Add(addedItem);
                    }
                    db.SaveChanges();
                }
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
                            ImageSmallPath = list[1],
                            PlaceId = place.Id,
                        };
                        db.Images.Add(img);
                    }
                }
                db.SaveChanges();
                SubCategory sub = db.SubCategories.Find(model.SubCategoryId);
                if (sub != null)
                {
                    sub.NumberOfPlace += 1;
                    db.Entry(sub).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(new { isSuccess = true, id = place.Id });
            }
            return Json(new { isSuccess = false });
        }
        public ActionResult Search()
        {
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            ViewBag.DistrictId = new SelectList(db.Districts, "Id", "DistrictName");
            return View();
        }


        // PlaceDAO PlaceDAO= new PlaceDAO();

        [HttpPost]
        public ActionResult Search(HNHB.Models.PlaceModels.SearchModel model)
        {
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            ViewBag.DistrictId = new SelectList(db.Districts, "Id", "DistrictName");
            var place = new PlaceModels();
            return View(place.Search(model));
        }

        public ActionResult Details(int id)
        {
            Place model = (from p in db.Places where p.Id == id && p.IsActive == true select p).FirstOrDefault();
            if (model == null)
            {
                return RedirectToAction("Error", "Home");
            }
            model.View = model.View += 1;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();
            if (model.PhoneNumber == null)
            {
                model.PhoneNumber = "";
            }
            model.PhoneNumber = model.PhoneNumber.ToPhoneNumber();
            ViewBag.Rate = model.SubCategory.AppliedRateCategories;
            if (model.UserId == User.Identity.GetUserId<int>() || User.IsInRole("Administrator"))
            {
                ViewBag.Edidable = true;
            }
            return View(model);
        }

        public ActionResult PlacesMap()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategories = from s in db.SubCategories
                                    where s.isActive == true
                                    select s;
            return View();
        }

        public JsonResult GetPlaceInRadius(int radius, double lat, double lng, string search)
        {
            var places = db.Places.Where(p => p.IsActive == true).Select(p => new { p.Id, p.Name, p.SubCategoryId, p.Address, Latitude = p.Coordinate.Latitude, Longitude = p.Coordinate.Longitude, Icon = p.SubCategory.Icon.Substring(1, (p.SubCategory.Icon.Length - 1)), LowPrice = p.Items.Count > 0 ? p.Items.FirstOrDefault().Price : 0 }).ToList();
            var locA = new GeoCoordinate(lat, lng);
            foreach (var place in places.ToList())
            {
                var locB = new GeoCoordinate(place.Latitude, place.Longitude);
                if (locA.GetDistanceTo(locB) > radius)
                {
                    places.Remove(place);
                }
            }
            //Search
            if (search != null && search.Trim().Length > 0)
            {
                search = search.Trim().ToLower().ToUnsign();
                List<int> placebyitem = new List<int>();
                foreach (var item in db.Items)
                {
                    if (placebyitem.Contains(item.PlaceId))
                        continue;
                    var name = item.Name.ToLower().ToUnsign();
                    if (BreakStringsAndCheck(search, name) >= 50 || name.Contains(search))
                    {
                        placebyitem.Add(item.PlaceId);
                    }
                }
                places = places.Where(p => BreakStringsAndCheck(search, p.Name.ToLower().ToUnsign()) >= 50 || p.Name.ToLower().ToUnsign().Contains(search) || placebyitem.Contains(p.Id)).ToList();
            }
            List<SubCategory> numOfPlaces = new List<SubCategory>();
            foreach (int sub in db.SubCategories.Select(s => s.Id))
            {
                SubCategory numOfPlace = new SubCategory
                {
                    Id = sub,
                    NumberOfPlace = places.Where(p => p.SubCategoryId == sub).Count()
                };
                numOfPlaces.Add(numOfPlace);
            }
            return Json(new { success = true, results = places, nums = numOfPlaces });
        }

        //
        // GET: /Place/EditPlaceInfo/9
        [Authorize]
        public ActionResult EditPlaceInfo(int id)
        {
            if (!(db.Places.Any(p => p.Id == id && p.IsActive == true && p.UserId == User.Identity.GetUserId<int>() || User.IsInRole("Administrator"))))
            {
                return RedirectToAction("Error", "Home");
            }
            Place place = db.Places.Find(id);
            if (place != null)
            {
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", place.SubCategory.Category.Id);
                ViewBag.SubCategoryId = new SelectList(db.SubCategories.Where(s => s.CategoryId == place.SubCategory.Category.Id), "Id", "Name", place.SubCategoryId);
                ViewBag.DistrictId = new SelectList(db.Districts, "Id", "DistrictName", place.DistrictId);
                place.SelectedTagIds = new List<int>();
                foreach (var tag in place.AppliedTagPlaces)
                {
                    place.SelectedTagIds.Add(tag.TagId);
                }
                ViewBag.TagId = new SelectList(db.Tags, "Id", "Name", place.SelectedTagIds);
                return View(place);
            }
            return RedirectToAction("Error", "Home");
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditPlaceInfo(Place place)
        {
            if (ModelState.IsValid)
            {
                Place model = db.Places.Find(place.Id);
                if (model.SubCategoryId != place.SubCategoryId)
                {
                    SubCategory sc = db.SubCategories.Find(model.SubCategoryId);
                    sc.NumberOfPlace -= 1;
                    db.Entry(sc).State = EntityState.Modified;
                    db.SaveChanges();
                    sc = db.SubCategories.Find(place.SubCategoryId);
                    sc.NumberOfPlace += 1;
                    db.Entry(sc).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (!model.Address.Equals(place.Address))
                {
                    Coordinate coordinate = CoordinateExtension.GetCoordinates(model.Address);
                    if (coordinate != null)
                    {
                        coordinate.PlaceId = place.Id;
                        db.Entry(coordinate).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        coordinate = new Coordinate
                        {
                            PlaceId = place.Id,
                            Latitude = 16.0466743,
                            Longitude = 108.206706
                        };
                        db.Entry(coordinate).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                model.Name = place.Name;
                model.Address = place.Address;
                model.SubCategoryId = place.SubCategoryId;
                model.DistrictId = place.DistrictId;
                model.PhoneNumber = place.PhoneNumber;
                model.Description = place.Description;
                model.StartTime = place.StartTime;
                model.EndTime = place.EndTime;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                foreach (var tag in db.AppliedTagPlaces.Where(t => t.PlaceId == place.Id))
                {
                    db.AppliedTagPlaces.Remove(tag);
                }
                db.SaveChanges();
                if (place.SelectedTagIds != null)
                {
                    foreach (var tagId in place.SelectedTagIds)
                    {
                        AppliedTagPlace appliedTag = new AppliedTagPlace
                        {
                            PlaceId = place.Id,
                            TagId = tagId,
                            AppliedDate = DateTime.Now
                        };
                        db.AppliedTagPlaces.Add(appliedTag);
                    }
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false, id = place.Id });
        }

        //
        // GET: /Place/EditPlaceGeo/9
        [Authorize]
        public ActionResult EditPlaceGeo(int id)
        {
            if (!(db.Places.Any(p => p.Id == id && p.IsActive == true && p.UserId == User.Identity.GetUserId<int>()) || User.IsInRole("Administrator")))
            {
                return RedirectToAction("Error", "Home");
            }
            Place place = db.Places.Find(id);
            if (place != null)
            {
                return View(place.Coordinate);
            }
            return RedirectToAction("Error", "Home");
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditPlaceGeo(Coordinate coordinate)
        {
            if (ModelState.IsValid)
            {
                Coordinate model = db.Coordinates.Find(coordinate.PlaceId);
                model.Latitude = coordinate.Latitude;
                model.Longitude = coordinate.Longitude;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, id = coordinate.PlaceId });
        }

        private double BreakStringsAndCheck(string s1, string s2)
        {
            if (s1.Length > s2.Length)
            {
                var tmp = s2;
                s2 = s1;
                s1 = tmp;
            }
            var split1 = s1.Split(' ');
            return (double)split1.Intersect(s2.Split(' ')).Count() / split1.Count() * 100.0;
        }

        public ActionResult EditPlaceItem(int id)
        {
            if (!(db.Places.Any(p => p.Id == id && p.IsActive == true && p.UserId == User.Identity.GetUserId<int>()) || User.IsInRole("Administrator")))
            {
                return RedirectToAction("Error", "Home");
            }
            Place place = db.Places.Find(id);
            if (place != null)
            {
                return View(place);
            }
            return RedirectToAction("Error", "Home");
        }

        public ActionResult EditPlaceImage(int id)
        {
            if (!(db.Places.Any(p => p.Id == id && p.IsActive == true && p.UserId == User.Identity.GetUserId<int>()) || User.IsInRole("Administrator")))
            {
                return RedirectToAction("Error", "Home");
            }
            Place place = db.Places.Find(id);
            if (place != null)
            {
                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                return View(place);
            }
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public ActionResult AddPlaceImage(int id, HttpPostedFileBase uploadImage)
        {
            if (uploadImage != null && uploadImage.ContentLength > 0)
            {
                List<string> list = ImageUploadExtension.renameUploadFile(uploadImage, "Resize");
                if (list != null)
                {
                    Image img = new Image()
                    {
                        ImagePath = list[0],
                        ImageSmallPath = list[1],
                        PlaceId = id,
                    };
                    db.Images.Add(img);
                    db.SaveChanges();
                }
                TempData["Message"] = "Đã thêm ảnh thành công";
                return RedirectToAction("EditPlaceImage", "Place", new { id = id });
            }
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public ActionResult RemovePlaceImage(int id)
        {
            Image img = db.Images.Find(id);
            if (img != null)
            {
                var placeId = img.PlaceId;
                ImageUploadExtension.removeUploadedFile(img.ImagePath);
                ImageUploadExtension.removeUploadedFile(img.ImageSmallPath);
                db.Images.Remove(img);
                db.SaveChanges();
                TempData["Message"] = "Đã xóa ảnh thành công";
                return RedirectToAction("EditPlaceImage", "Place", new { id = placeId });
            }
            return RedirectToAction("Error", "Home");
        }

        public ActionResult VirtualTourist(int id)
        {
            ViewBag.Link = db.Places.Find(id).Virtualtourist;
            if (ViewBag.Link != null)
            {
                ViewBag.Name = db.Places.Find(id).Name;
                return View();
            }
            return RedirectToAction("Error", "Home");
        }
        //public ActionResult VirtualTourist()
        //{

        //    return View();
        //}
        public ActionResult IsNameAvailble(string Name)
        {
            using (Entities db = new Entities())
            {
                try
                {
                    var name = db.Places.Single(m => m.Name == Name);
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }
          [HttpPost]
        public JsonResult LoadImageList(int imageId, int placeId, int reviewId)
        {
            List<Image> images = new List<Image>();
            Image first = (from i in db.Images where i.Id == imageId select i).FirstOrDefault();
            if (first == null)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Xin lỗi vì sự bất tiện này." });
            }
            else
            {
                images.Add(new Image
                {
                    Id = first.Id,
                    ImagePath = Url.Content(first.ImagePath),
                    ImageSmallPath = Url.Content(first.ImageSmallPath)
                });
            }
            if (reviewId != 0)
            {
                List<Image> list = (from i in db.Images where i.ReviewId == reviewId && i.Id != imageId select i).ToList();
                foreach (var image in list)
                {
                    images.Add(new Image
                    {
                        Id = image.Id,
                        ImagePath = Url.Content(image.ImagePath),
                        ImageSmallPath = Url.Content(image.ImageSmallPath)
                    });
                }
            }
            else
            {
                List<Image> list = (from i in db.Images where i.PlaceId == placeId && i.Id != imageId select i).ToList();
                foreach (var image in list)
                {
                    images.Add(new Image
                    {
                        Id = image.Id,
                        ImagePath = Url.Content(image.ImagePath),
                        ImageSmallPath = Url.Content(image.ImageSmallPath)
                    });
                }
            }
            return Json(new { success = true, images = images });
        }

        [HttpPost]
        public JsonResult LoadPlaceImageList(int placeId)
        {
            List<Image> images = new List<Image>();
            if (db.Places.Any(p => p.Id == placeId))
            {
                List<Image> list = (from i in db.Images where i.PlaceId == placeId select i).ToList();
                foreach (var image in list)
                {
                    images.Add(new Image
                    {
                        Id = image.Id,
                        ImagePath = Url.Content(image.ImagePath),
                        ImageSmallPath = Url.Content(image.ImageSmallPath)
                    });
                }
            }
            else
            {
                return Json(new { success = false });
            }
            return Json(new { success = true, images = images });
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
        [Authorize]
        public void AddReport(int Id, string reportContent)
        {
            Report rp = new Report
            {
                PlaceId = Id,
                Reason = reportContent,
                UserId = User.Identity.GetUserId<int>(),
                isChecked = false
            };
            db.Reports.Add(rp);
            db.SaveChanges();
        }
    }
    
}