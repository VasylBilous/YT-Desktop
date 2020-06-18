namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Videos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Adress = c.String(),
                        ImageUrl = c.String(),
                        VideoTitle = c.String(),
                        ChannelTitle = c.String(),
                        PostedDate = c.DateTime(nullable: false),
                        IsVideo = c.Boolean(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Videos", "User_Id", "dbo.Users");
            DropIndex("dbo.Videos", new[] { "User_Id" });
            DropTable("dbo.Videos");
            DropTable("dbo.Users");
        }
    }
}
