using System.Linq;
using System.Web;
using System.Web.Mvc;
using HNHB.Models;
using System.Data.Entity;
using Newtonsoft.Json;
using HNHB.HelperExtension;
using System.Data;
using HNHB.Models.Entities;
using System.Collections.Generic;


namespace HNHB.DAO
{
    public class AskingDAO
    {
        
        Entities  db = null;
        public  AskingDAO()
        {
            db = new Entities();
        }
        public List<Question> ListQuestion()
        {
            return (from q in db.Questions where q.isActive == true select q).ToList().OrderByDescending(x => x.CreateDate).ToList();
        }
        public List<Question> LstByView()
        {
            return (from v in db.Questions where v.isActive == true select v).OrderByDescending(v => v.View).Take(7).ToList();
        }
        public  List<Question> LstByDate()
        {
            return (from v in db.Questions where v.isActive == true select v)
               .OrderByDescending(v => v.CreateDate).Take(7).ToList();
        }
        public List<int> LstAllTag()
        {
            return db.AppliedTagQuestions.GroupBy(a => a.TagId).OrderBy(g => g.Count()).Select(g => g.Key).ToList();
        }
        public List<int> LstId(int? tagId)
        {
            return db.AppliedTagQuestions.Where(a => a.TagId == tagId).Select(a => a.QuestionId).Distinct().ToList();
        }
     
    }
}