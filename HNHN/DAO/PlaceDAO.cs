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
using System.Xml.Linq;

namespace HNHB.DAO
{
    public class PlaceDAO
    {
        Entities db = null;
        public PlaceDAO()
        {
            db = new Entities();
        }

        public HNHB.Models.PlaceModels.SearchModel Search(HNHB.Models.PlaceModels.SearchModel search)
        {
            string tempKeyword = null;
            if (search.Keyword != null) tempKeyword = search.Keyword.ToUnsign();
            List<String> stringKeyword = new List<string>();
            int subIndex = -1;
            for (int i = 0; i < tempKeyword.Length; i++)
            {
                if (tempKeyword[i].Equals('\"'))
                {
                    if (subIndex < 0)
                    {
                        subIndex = i;
                    }
                    else
                    {
                        stringKeyword.Add(tempKeyword.Substring(subIndex + 1, i - subIndex - 1).Trim().ToUnsign().ToLower());
                        tempKeyword = tempKeyword.Remove(subIndex, i - subIndex + 1).Trim();
                        i = subIndex;
                        subIndex = -1;
                    }
                }
            }

            subIndex = -1;
            for (int i = 0; i < tempKeyword.Length; i++)
            {
                if ((tempKeyword[i].Equals(' ') && !tempKeyword[i + 1].Equals(' ')) || i == tempKeyword.Length - 1)
                {
                    stringKeyword.Add(tempKeyword.Substring(subIndex + 1, i - subIndex).Trim().ToUnsign().ToLower());
                    subIndex = i;
                }
            }
            try
            {
                List<Place> place;
                try
                {
                    //Tested with 1000 record
                    //Get from database: 765 miliseconds
                    //Get from XML data: 61 miliseconds
                    place = GetXMLData();
                    if (place.Count == 0)
                    {
                       
                        place = db.Places.ToList();
                    }
                }
                catch (Exception e)
                {
                   
                    place = db.Places.ToList();
                }
                List<Place> rslt;
                foreach (var s in stringKeyword)
                {
                    rslt = place.Where(p => (p.Name.ToUnsign().ToLower().SearchByWord(s) || p.Address.ToUnsign().ToLower().SearchByWord(s))
                        && (search.DistrictId <= 0 || p.DistrictId == search.DistrictId)
                        && (search.SubCategoryId <= 0 || p.SubCategoryId == search.SubCategoryId)).OrderBy(p => p.Id).ToList();
                    if (rslt != null && rslt.Count() > 0)
                    {
                        bool isExisted;
                        foreach (var p in rslt)
                        {
                            isExisted = false;
                            if (search.Result != null && search.Result.Count > 0)
                            {
                                foreach (var item in search.Result)
                                {
                                    if (p.Id == item.Place.Id)
                                    {
                                        item.Count++;
                                        isExisted = true;
                                        break;
                                    }
                                }
                                if (!isExisted)
                                {
                                    search.Result.Add(new HNHB.Models.PlaceModels.PlaceResult { Place = p, Count = 1 });
                                }
                            }
                            else
                            {
                                search.Result = new List<HNHB.Models.PlaceModels.PlaceResult>();
                                search.Result.Add(new HNHB.Models.PlaceModels.PlaceResult { Place = p, Count = 1 });
                            }
                        }
                    }
                }
                if (search.Result != null || search.Result.Count > 0)
                {
                    search.Result = search.Result.OrderByDescending(ps => ps.Count).ToList();
                }
            }
            catch (Exception e)
            {
                search.Result = null;
            }
            return search;
        }
        public List<Place> GetFromDB()
        {
           
            return db.Places.OrderByDescending(p => p.CreateDate).ToList();
        }

        public List<Place> GetXMLData()
        {
            string xmlData = HttpContext.Current.Server.MapPath("~/Content/placeList.xml");
            XDocument doc = XDocument.Load(xmlData);
            List<Place> place = doc.Descendants("Place")
                .Select(p => new Place
                {
                    Id = Convert.ToInt32(p.Attribute("Id").Value),
                    Name = p.Element("Name").Value,
                    Address = p.Element("Address").Value,
                    Images = new Image[]
                        {
                            new Image
                            {
                                ImagePath = p.Element("Image").Value
                            }
                        },
                    Description = p.Element("Description").Value,
                    DistrictId = Convert.ToInt32(p.Element("DistrictId").Value),
                    SubCategoryId = Convert.ToInt32(p.Element("SubCategoryId").Value),
                    CreateDate = Convert.ToDateTime(p.Element("CreateDate").Value),
                    AppliedTagPlaces = p.Descendants("Tags")
                        .Select(t => new AppliedTagPlace
                        {
                            TagId = Convert.ToInt32(t.Element("TagId").Value)
                        }).ToList()
                }).ToList();
            return place;
        }
        public void UpdateXMLData(List<Place> place)
        {
            XDocument doc = new XDocument(
                new XElement("Places",
                    place.Select(p => new XElement("Place",
                        new XAttribute("Id", p.Id),
                        new XElement("Name", p.Name),
                        new XElement("Address", p.Address),
                        new XElement("Image", p.Images != null && p.Images.Count > 0 ? p.Images.FirstOrDefault().ImagePath : ""),
                        new XElement("Description", p.Description),
                        new XElement("DistrictId", p.DistrictId),
                        new XElement("SubCategoryId", p.SubCategoryId),
                        new XElement("CreateDate", p.CreateDate),
                        new XElement("Tags", p.AppliedTagPlaces.Select(t => new XElement("TagId", t.TagId))))
                    )
                )
            );
            string xmlData = HttpContext.Current.Server.MapPath("~/Content/placeList.xml");
            doc.Save(xmlData);
        }


    }
}