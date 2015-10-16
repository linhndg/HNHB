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
        //#region[SendSMS]
        ////Ham gui tin nhan
        //public void SendSMSforPlace(String QuestionId)
        //{
        //    int QuesId = int.Parse(QuestionId);
        //    List<AppliedTagPlace> listTagplace = new List<AppliedTagPlace>();
        //    List<String> listPlace = new List<String>(); // tao list chua tat ca idplace co tagId giong tagid ben Tagquestion
        //    var tagIdQuestion = (from quesTag in db.AppliedTagQuestions where quesTag.QuestionId == QuesId select quesTag).ToList();//lay tat ca cac tag cua questionid truyen vao
        //    var AppTagPlace = (from apptag in db.AppliedTagPlaces select apptag).ToList();//lay tat ca cac dia diem chua tag
        //    for (int j = 0; j < tagIdQuestion.Count(); j++)//Cac tagId co trong TagQuestion cua question
        //    {
        //        int tagIDquestionDetail = tagIdQuestion[j].TagId;
        //        for (int i = 0; i < AppTagPlace.Count(); i++)
        //        {

        //            var tp2 = (from apptag2 in db.AppliedTagPlaces select apptag2).ToList();
        //            if (tp2[i].TagId == tagIDquestionDetail)
        //            {
        //                String placeid = tp2[i].PlaceId.ToString();
        //                listPlace.Add(placeid);
        //            }

        //        }
        //    }
        //    //Sap xep theo tu tu giam gian so lan xuat hien , gop cac phan tu giong nhau
        //    listPlace = listPlace.GroupBy(i => i).OrderByDescending(g => g.Count()).Select(g => g.Key).ToList();
        //    //Tu cac placeId , lay phonenumber trong bang Place
        //    //Thu hien vong lap gui Sms cac phonenumber of Place
        //    for (int i = 0; i < 1; i++) //1 phone
        //    {
        //        var placeId = int.Parse(listPlace[i]);
        //        //Lay dia diem
        //        var phoneplace = (from phonepla in db.Places where phonepla.Id == placeId select phonepla).SingleOrDefault();
        //        //Noi Dung tin nhan can gui.
        //        string contentString = "Ban co 1 cau hoi lien quan den cua hang";
        //        contentString += "\n Truy cap vao: DNIYH.com/Asking/QuestionDetails/" + QuestionId.ToString();
        //        contentString += " de xem thong tin chi tiet";
        //        //Goi API SMS truyen vao so dien thoai va noi dung tin nhan
        //        SendSMS(phoneplace.PhoneNumber, contentString);
        //    }

        //}
        ////API gui sms
        //public void SendSMS(string phone, string message)
        //{
        //    string url = "http://api.esms.vn/MainService.svc/xml/SendMultipleMessage_V2/";
        //    // declare ascii encoding
        //    ASCIIEncoding encoding = new ASCIIEncoding();
        //    string strResult = string.Empty;
        //    string customers = "";

        //    customers = customers + @"<CUSTOMER>"
        //                    + "<PHONE>" + phone + "</PHONE>"
        //                    + "</CUSTOMER>";

        //    string SampleXml = @"<RQST>"
        //                        + "<APIKEY>" + APIKey + "</APIKEY>"
        //                        + "<SECRETKEY>" + SecretKey + "</SECRETKEY>"
        //                        + "<ISFLASH>0</ISFLASH>"
        //                        + "<SMSTYPE>7</SMSTYPE>"
        //                        + "<CONTENT>" + message + "</CONTENT>"
        //                        + "<CONTACTS>" + customers + "</CONTACTS>"


        //    + "</RQST>";
        //    string postData = SampleXml.Trim().ToString();
        //    // convert xmlstring to byte using ascii encoding
        //    byte[] data = encoding.GetBytes(postData);
        //    // declare httpwebrequet wrt url defined above
        //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
        //    // set method as post
        //    webrequest.Method = "POST";
        //    webrequest.Timeout = 500000;
        //    // set content type
        //    webrequest.ContentType = "application/x-www-form-urlencoded";
        //    // set content length
        //    webrequest.ContentLength = data.Length;
        //    // get stream data out of webrequest object
        //    Stream newStream = webrequest.GetRequestStream();
        //    newStream.Write(data, 0, data.Length);
        //    newStream.Close();
        //    // declare & read response from service
        //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

        //    // set utf8 encoding
        //    Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
        //    // read response stream from response object
        //    StreamReader loResponseStream =
        //        new StreamReader(webresponse.GetResponseStream(), enc);
        //    // read string from stream data
        //    strResult = loResponseStream.ReadToEnd();
        //    // close the stream object
        //    loResponseStream.Close();
        //    // close the response object
        //    webresponse.Close();
        //    // below steps remove unwanted data from response string
        //    strResult = strResult.Replace("</string>", "");
        //}

        //#endregion


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