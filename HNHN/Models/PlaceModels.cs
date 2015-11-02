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