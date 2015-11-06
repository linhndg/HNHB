using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HNHB.HelperExtension;
using System.Diagnostics;
using System.Xml.Linq;
using HNHB.Models.Entities;
using HNHB.Models;
namespace HNHB.Models
{

    public class PlaceModels
    {
        /// <summary>
        /// Get all place
        /// </summary>
        /// <returns></returns>

        HNHB.Models.Entities.Entities db = new HNHB.Models.Entities.Entities();
        public List<Place> GetAllPlaces()
        {
            List<Place> place;
            try
            {
                place = GetXMLData();
                if (place.Count == 0)
                {
                    place = GetFromDB();
                }
            }
            catch (Exception e)
            {
                place = GetFromDB();
            }
            return place;
        }

        /// <summary>
        /// Get place with a number of places
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public List<Place> PagingPlace(List<Place> place, int pageNum, int numOfPlace)
        {
            place = place.OrderByDescending(p => p.CreateDate).ToList();
            List<Place> rs = new List<Place>();
            for (int i = (pageNum - 1) * numOfPlace; i < pageNum * numOfPlace; i++)
            {
                rs.Add(place[i]);
            }
            return rs;
        }

        /// <summary>
        /// Searching with condition from search model
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public SearchModel Search(SearchModel search)
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
                                    search.Result.Add(new PlaceResult { Place = p, Count = 1 });
                                }
                            }
                            else
                            {
                                search.Result = new List<PlaceResult>();
                                search.Result.Add(new PlaceResult { Place = p, Count = 1 });
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

        /// <summary>
        /// Write and update XML data
        /// </summary>
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
        public class SearchModel
        {
            public string Keyword { get; set; }
            public int DistrictId { get; set; }
            public int SubCategoryId { get; set; }
            public List<PlaceResult> Result { get; set; }
        }

        public class PlaceResult
        {
            public int Count { get; set; }
            public Place Place { get; set; }
        }
    }
}