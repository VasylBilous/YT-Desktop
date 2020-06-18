namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _0609 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KeyTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VideoId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.KeyTables");
        }
    }
}
