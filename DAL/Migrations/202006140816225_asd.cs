namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class asd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KeyTables", "VideoAdress", c => c.String());
            AddColumn("dbo.KeyTables", "UserEmail", c => c.String());
            DropColumn("dbo.KeyTables", "VideoId");
            DropColumn("dbo.KeyTables", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KeyTables", "UserId", c => c.Int(nullable: false));
            AddColumn("dbo.KeyTables", "VideoId", c => c.Int(nullable: false));
            DropColumn("dbo.KeyTables", "UserEmail");
            DropColumn("dbo.KeyTables", "VideoAdress");
        }
    }
}
