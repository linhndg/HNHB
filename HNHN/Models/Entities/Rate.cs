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
    
    public partial class Rate
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public int RateCategoryId { get; set; }
        public int Value { get; set; }
    
        public virtual RateCategory RateCategory { get; set; }
        public virtual Review Review { get; set; }
    }
}
