//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HNHB.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class AppliedRateCategory
    {
        public int SubCategoryId { get; set; }
        public int RateCategoryId { get; set; }
        public System.DateTime AppliedDate { get; set; }
    
        public virtual RateCategory RateCategory { get; set; }
        public virtual SubCategory SubCategory { get; set; }
    }
}
