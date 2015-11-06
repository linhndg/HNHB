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
using System.Text;
using System.Net;
using System.IO;


namespace HNHB.Controllers
{
    
    
    public class AskingController : Controller
    {
        // Get database
        Entities db = new Entities();
        AskingDAO AskingDAO = new AskingDAO();
        // GET: Asking
        //const string APIKey = "411DA00A2AF749AD511C405870AE43";
        //const string SecretKey = "FB0E24FEA02BFAF407EBDB51CD12AD";
        public ActionResult Index(int? tagId, string key)
        {
            var question = AskingDAO.ListQuestion();
            ViewBag.lstByView = AskingDAO.LstByView().Take(7);
            ViewBag.lstByDate = AskingDAO.LstByDate().Take(7);
            ViewBag.lstByAnswer = (from v in db.Answers where v.Question.isActive == true && v.isActive == true select v)
                            .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            List<int> allTgs = AskingDAO.LstAllTag();
            if (tagId != null)
            {
                List<int> listId = db.AppliedTagQuestions.Where(a => a.TagId == tagId).Select(a => a.QuestionId).Distinct().ToList();
                question = question.Where(q => listId.Contains(q.Id)).ToList();
            }
            if (key != null)
            {
                question = question.Where(q => q.Title.ToLower().ConvertToUnSign().Contains(key.ToLower().ConvertToUnSign())).ToList();
                ViewBag.Key = key;
            }
            ViewBag.allTags = db.Tags.Where(t => allTgs.Contains(t.Id));
            return View(question);
        }

        [Authorize]
        public ActionResult CreateQuestion()
        {
            var lstTags = (from tag in db.Tags select tag).ToList();
            return View(lstTags);
        }
        // Create Question
        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateQuestion(string editorContent, string tagSelect, string stringTitle)
        {
            var appliedTagList = JsonConvert.DeserializeObject<string[]>(tagSelect);
            int ptId = 0;
            if (ModelState.IsValid)
            {
                Question asking = new Question();

                asking.Title = stringTitle;
                while (editorContent.EndsWith("<br>"))
                    editorContent = editorContent.Substring(0, editorContent.Length - 4);
                asking.Content = editorContent.Trim('\r', '\n', ' ');
                //article.UserId =  User.Identity.GetUserId<int>();
                asking.UserId =  User.Identity.GetUserId<int>();
                asking.CreateDate = DateTime.Now;
                asking.isActive = true;
                //asking. = 0;

                db.Questions.Add(asking);
                db.SaveChanges();
                ptId = asking.Id;
                if (appliedTagList != null)
                {
                    foreach (var item in appliedTagList)
                    {
                        AppliedTagQuestion appliedTag = new AppliedTagQuestion();
                        appliedTag.QuestionId = asking.Id;
                        appliedTag.TagId = Int32.Parse(item);
                        //appliedTag.TaggingUser =  User.Identity.GetUserId<int>();
                        appliedTag.AppliedDate = DateTime.Now;
                        db.AppliedTagQuestions.Add(appliedTag);
                        db.SaveChanges();
                    }
                }
            }
          
            return Json(ptId, JsonRequestBehavior.AllowGet);
        }
     // View Detail of question
        public ActionResult QuestionDetails(int? id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var questionContent = (from qtContent in db.Questions
                                   where qtContent.Id == id && qtContent.isActive == true
                                   select qtContent).Single();
            Question qt = questionContent;
            qt.View++;
            db.SaveChanges();
            ViewBag.lstByView = (from v in db.Questions where v.isActive == true select v)
                .ToList().OrderByDescending(v => v.View).Take(7);
            ViewBag.lstByDate = (from v in db.Questions where v.isActive == true select v)
                .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            ViewBag.lstByAnswer = (from v in db.Answers where v.Question.isActive == true && v.isActive == true select v)
                            .ToList().OrderByDescending(v => v.CreateDate).Take(7);
            List<int> allTgs = db.AppliedTagQuestions.GroupBy(a => a.TagId).OrderBy(g => g.Count()).Select(g => g.Key).ToList();

            ViewBag.allTags = db.Tags.Where(t => allTgs.Contains(t.Id));

            return View(questionContent);
        }
        // Post answer 
        [Authorize]
        public ActionResult AnswerQuestion(string qtContent, int qtId = 0)
        {
            if (ModelState.IsValid)
            {
                Answer ans = new Answer();
                ans.QuestionId = qtId;
                ans.Content = qtContent;
                ans.UserId =  User.Identity.GetUserId<int>();
                ans.CreateDate = DateTime.Now;
                ans.isActive = true;

                db.Answers.Add(ans);
                db.SaveChanges();
            }


            var answer = (from ans in db.Answers.Include("UserProfile")
                          where ans.QuestionId == qtId && ans.isActive == true
                          select ans).ToList().OrderByDescending(x => x.CreateDate);
            return PartialView("QtAnswerPartial", answer);
        }
        // Delete answer by admin
        [Authorize(Roles = "Administrator")]
        public ActionResult RemoveAnswer(int ansId)
        {
            Answer ans = db.Answers.Find(ansId);
            if (ans != null)
            {
                ans.isActive = false;
                db.Entry(ans).State = EntityState.Modified;
                db.SaveChanges();
            }
            var answer = (from a in db.Answers.Include("UserProfile")
                          where a.QuestionId == ans.QuestionId && a.isActive == true
                          select a).ToList().OrderByDescending(x => x.CreateDate);
            return PartialView("QtAnswerPartial", answer);
        }
        // Vote up function
        [HttpGet]
        [Authorize]
        public ActionResult VoteUp(int? canswerId)
        {
            if (ModelState.IsValid)
            {
                Vote vt = new Vote();
                vt.AnswerId = canswerId;
                vt.Value = 1;
                vt.UserId =  User.Identity.GetUserId<int>();

                db.Votes.Add(vt);
                db.SaveChanges();
            }

            var votes = (from v in db.Votes where v.AnswerId == canswerId select v).Sum(x => x.Value);

            return Json(votes, JsonRequestBehavior.AllowGet);
        }
        // Vote down function
        [Authorize]
        [HttpGet]
        [Authorize]
        public ActionResult VoteDown(int? canswerId)
        {
            int userId =  User.Identity.GetUserId<int>();
            if (ModelState.IsValid)
            {
                Vote vt = new Vote();
                vt.AnswerId = canswerId;
                vt.Value = -1;
                vt.UserId = userId;

                db.Votes.Add(vt);
                db.SaveChanges();
            }

            var votes = (from v in db.Votes where v.AnswerId == canswerId select v).Sum(x => x.Value);

            return Json(votes, JsonRequestBehavior.AllowGet);
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
        // Get list to suggest Anwer
        [HttpPost]
        public ActionResult QuestionSuggest(string title)
        {
            List<Question> results = new List<Question>();
            foreach (var question in db.Questions.Where(q => q.isActive == true && q.Answers.Count > 0))
            {
                if (BreakStringsAndCheck(title.ToLower().ToUnsign(), question.Title.ToLower().ToUnsign()) >= 50)
                {
                    Question result = new Question
                    {
                        Id = question.Id,
                        Title = question.Title,
                        Content = question.Answers.Count + " câu trả lời, " + question.CreateDate.ToTimeAgo()
                    };
                    results.Add(result);
                }
            }
            if (results.Count > 0)
            {
                return Json(new { success = true, result = results });
            }
            return Json(new { success = false });
        }
        // Return vote for answer
        [Authorize]
        [HttpGet]
        public ActionResult CurrentUserVote(int? canswerId)
        {
            var isVote = (from v in db.Votes
                          where v.AnswerId == canswerId
                              && v.UserProfile.UserName == User.Identity.Name
                          select v).Sum(x => x.Value);
            return Json(isVote, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult EditQuestion(int? id)
        {
            //if (!(db.Questions.Any(q => q.Id == id && q.isActive == true && q.UserId ==  User.Identity.GetUserId<int>()) || User.IsInRole("Administrator")))
            //{
            //    return RedirectToAction("Error", "Home");
            //}
            var qtContent = db.Questions.Find(id);
            ViewBag.LstTags = (from tag in db.Tags select tag).ToList();
            return View(qtContent);
        }
        // Edit Question
        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult EditQuestion(string questionId, string editorContent, string tagSelect, string stringTitle)
        {
            int id = Int32.Parse(questionId);
            var appliedTagList = JsonConvert.DeserializeObject<string[]>(tagSelect);
            if (ModelState.IsValid)
            {
                Question asking = (from qt in db.Questions
                                   where qt.Id == id && qt.isActive == true
                                   select qt).FirstOrDefault();

                asking.Title = stringTitle;
                while (editorContent.EndsWith("<br>"))
                    editorContent = editorContent.Substring(0, editorContent.Length - 4);
                asking.Content = editorContent.Trim('\r', '\n', ' ');
                asking.CreateDate = DateTime.Now;
                asking.isActive = true;
                //asking. = 0;

                db.Entry(asking).State = EntityState.Modified;
                db.SaveChanges();

                // Delete applied tags in question
                var appliedTags = (from apTag in db.AppliedTagQuestions
                                   where apTag.QuestionId == id
                                   select apTag).ToList();
                foreach (var item in appliedTags)
                {
                    var delTag = db.AppliedTagQuestions.Where(a => a.QuestionId == id
                                                                        && a.TagId == item.TagId).Single();
                    db.AppliedTagQuestions.Remove(delTag);
                }
                db.SaveChanges();
                //Save Applied tags
                if (appliedTagList != null)
                {
                    foreach (var item in appliedTagList)
                    {
                        AppliedTagQuestion appliedTag = new AppliedTagQuestion();
                        appliedTag.QuestionId = asking.Id;
                        appliedTag.TagId = Int32.Parse(item);
                        //appliedTag.TaggingUser =  User.Identity.GetUserId<int>();
                        appliedTag.AppliedDate = DateTime.Now;
                        db.AppliedTagQuestions.Add(appliedTag);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        // Deactive question
        [Authorize(Roles = "Administrator")]
        public ActionResult Deactive(int? id)
        {
            if (id != null)
            {
                Question question = (from qt in db.Questions where qt.Id == id select qt).FirstOrDefault();
                if (ModelState.IsValid)
                {
                    question.isActive = false;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Asking");
        }
    }
}