namespace HNHB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.Roles",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Name = c.String(nullable: false, maxLength: 256),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            //CreateTable(
            //    "dbo.UserRoles",
            //    c => new
            //        {
            //            UserId = c.Int(nullable: false),
            //            RoleId = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => new { t.UserId, t.RoleId })
            //    .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
            //    .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.UserId)
            //    .Index(t => t.RoleId);
            
            //CreateTable(
            //    "dbo.UserProfile",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Fullname = c.String(),
            //            Avatar = c.String(),
            //            Point = c.Int(),
            //            CreateDate = c.DateTime(),
            //            isActive = c.Boolean(),
            //            Email = c.String(maxLength: 256),
            //            EmailConfirmed = c.Boolean(nullable: false),
            //            PasswordHash = c.String(),
            //            SecurityStamp = c.String(),
            //            PhoneNumber = c.String(),
            //            PhoneNumberConfirmed = c.Boolean(nullable: false),
            //            TwoFactorEnabled = c.Boolean(nullable: false),
            //            LockoutEndDateUtc = c.DateTime(),
            //            LockoutEnabled = c.Boolean(nullable: false),
            //            AccessFailedCount = c.Int(nullable: false),
            //            UserName = c.String(nullable: false, maxLength: 256),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            //CreateTable(
            //    "dbo.UserClaims",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            UserId = c.Int(nullable: false),
            //            ClaimType = c.String(),
            //            ClaimValue = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.UserId);
            
            //CreateTable(
            //    "dbo.UserLogins",
            //    c => new
            //        {
            //            LoginProvider = c.String(nullable: false, maxLength: 128),
            //            ProviderKey = c.String(nullable: false, maxLength: 128),
            //            UserId = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
            //    .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            //DropForeignKey("dbo.UserRoles", "UserId", "dbo.UserProfile");
            //DropForeignKey("dbo.UserLogins", "UserId", "dbo.UserProfile");
            //DropForeignKey("dbo.UserClaims", "UserId", "dbo.UserProfile");
            //DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            //DropIndex("dbo.UserLogins", new[] { "UserId" });
            //DropIndex("dbo.UserClaims", new[] { "UserId" });
            //DropIndex("dbo.UserProfile", "UserNameIndex");
            //DropIndex("dbo.UserRoles", new[] { "RoleId" });
            //DropIndex("dbo.UserRoles", new[] { "UserId" });
            //DropIndex("dbo.Roles", "RoleNameIndex");
            //DropTable("dbo.UserLogins");
            //DropTable("dbo.UserClaims");
            //DropTable("dbo.UserProfile");
            //DropTable("dbo.UserRoles");
            //DropTable("dbo.Roles");
        }
    }
}
