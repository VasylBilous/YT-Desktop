namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Second : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Videos", "User_Id", "dbo.Users");
            DropIndex("dbo.Videos", new[] { "User_Id" });
            CreateTable(
                "dbo.VideoUsers",
                c => new
                    {
                        Video_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Video_Id, t.User_Id })
                .ForeignKey("dbo.Videos", t => t.Video_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Video_Id)
                .Index(t => t.User_Id);
            
            DropColumn("dbo.Videos", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Videos", "User_Id", c => c.Int());
            DropForeignKey("dbo.VideoUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.VideoUsers", "Video_Id", "dbo.Videos");
            DropIndex("dbo.VideoUsers", new[] { "User_Id" });
            DropIndex("dbo.VideoUsers", new[] { "Video_Id" });
            DropTable("dbo.VideoUsers");
            CreateIndex("dbo.Videos", "User_Id");
            AddForeignKey("dbo.Videos", "User_Id", "dbo.Users", "Id");
        }
    }
}
