﻿//------------------------------------------------------------------------------
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
    using System.ComponentModel.DataAnnotations;
    using FluentValidation;
    using System.Web.Mvc;

   
    public partial class Place
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
       
        public Place()
        {
            this.AppliedTagPlaces = new HashSet<AppliedTagPlace>();
            this.Dictionaries = new HashSet<Dictionary>();
            this.Images = new HashSet<Image>();
            this.Items = new HashSet<Item>();
            this.Reports = new HashSet<Report>();
            this.Reviews = new HashSet<Review>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên địa điểm")]
        [Remote("IsNameAvailble", "Place", ErrorMessage = "Name Already Exist.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        public string Address { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        [RegularExpression("^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không đúng")]
        public string PhoneNumber { get; set; }
        public System.DateTime CreateDate { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin mô tả")]
        public string Description { get; set; }
        public int View { get; set; }
        public double RateValue { get; set; }
        public int UserId { get; set; }
        public int SubCategoryId { get; set; }
        public int DistrictId { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public string Virtualtourist { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AppliedTagPlace> AppliedTagPlaces { get; set; }
        public virtual Coordinate Coordinate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Dictionary> Dictionaries { get; set; }
        public virtual District District { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Image> Images { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Item> Items { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public virtual UserProfile UserProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Report> Reports { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<int> SelectedTagIds { get; set; }
    }

}
