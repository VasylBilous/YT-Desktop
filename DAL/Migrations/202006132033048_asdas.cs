namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class asdas : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Token", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Token");
        }
    }
}
