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

namespace HNHB.DAO
{
    public class ArticleDAO
    {
        Entities db = null;
        public ArticleDAO()
        {
            db = new Entities();
        }
        public List<Article> ListArticalByCreateDate()
        {
            return (from ar in db.Articles where ar.isActive == true select ar).ToList().OrderByDescending(c => c.CreateDate).ToList();
        }
        public List<int> ListAllTag()
        {
            return db.AppliedTagArticles.GroupBy(a => a.TagId).OrderBy(g => g.Count()).Select(g => g.Key).ToList();
        }
        public List<int> ListId(int? tagId)
        {
            return db.AppliedTagArticles.Where(a => a.TagId == tagId).Select(a => a.ArticleId).Distinct().ToList();
           
        }
        public List<Tag> ListTag()
        {
            return (from tag in db.Tags select tag).ToList();

        }
        public Article GetArtical(int? id = 0)
        {
            return (from a in db.Articles where a.isActive == true && a.Id == id select a).FirstOrDefault();
        }

       
        
    }
 }
