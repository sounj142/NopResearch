namespace Research.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRunWithAnotherTaskToScheduleTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleTask", "RunWithAnotherTask", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScheduleTask", "RunWithAnotherTask");
        }
    }
}
