namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mmmki : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Videos", "Desctiption", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Videos", "Desctiption");
        }
    }
}
