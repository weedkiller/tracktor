using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracktor.model.DAL
{
    public class TracktorInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<TracktorContext>
    {
        protected override void Seed(TracktorContext context)
        {
            var user = new TUser { Name = "guest", AspNetId = null };
            context.TUsers.Add(user);
            context.SaveChanges();

            var projects = new List<TProject>
            {
                new TProject { Name = "Project 1", DisplayOrder = 1, IsObsolete = false, TUserID = user.TUserID },
                new TProject { Name = "Project 2", DisplayOrder = 2, IsObsolete = false, TUserID = user.TUserID },
                new TProject { Name = "Project 3", DisplayOrder = 3, IsObsolete = false, TUserID = user.TUserID },
                new TProject { Name = "Project 4", DisplayOrder = 4, IsObsolete = true, TUserID = user.TUserID },
            };
            projects.ForEach(p => context.TProjects.Add(p));
            context.SaveChanges();

            var tasks = new List<TTask>
            {
                new TTask { Name = "Task 1.1", DisplayOrder = 1, IsObsolete = false, TProjectID = projects[0].TProjectID },
                new TTask { Name = "Task 1.2", DisplayOrder = 2, IsObsolete = false, TProjectID = projects[0].TProjectID },
                new TTask { Name = "Task 2.1", DisplayOrder = 3, IsObsolete = false, TProjectID = projects[1].TProjectID },
                new TTask { Name = "Task 2.2", DisplayOrder = 4, IsObsolete = true, TProjectID = projects[1].TProjectID },
                new TTask { Name = "Task 3.1", DisplayOrder = 5, IsObsolete = false, TProjectID = projects[2].TProjectID },
                new TTask { Name = "Task 3.2", DisplayOrder = 6, IsObsolete = false, TProjectID = projects[2].TProjectID },
                new TTask { Name = "Task 3.3", DisplayOrder = 7, IsObsolete = true, TProjectID = projects[2].TProjectID },
                new TTask { Name = "Task 4.1", DisplayOrder = 8, IsObsolete = false, TProjectID = projects[3].TProjectID }
            };
            tasks.ForEach(t => context.TTasks.Add(t));
            context.SaveChanges();

            var now = DateTime.UtcNow;
            var entries = new List<TEntry>
            {
                new TEntry { StartDate = now.AddHours(-24), EndDate = now.AddHours(-22), TTaskID = tasks[0].TTaskID },
                new TEntry { StartDate = now.AddHours(-21), EndDate = now.AddHours(-18).AddMinutes(-22), TTaskID = tasks[2].TTaskID },
                new TEntry { StartDate = now.AddDays(-2).AddHours(-15).AddMinutes(-1), EndDate = now.AddDays(-2).AddHours(-14), TTaskID = tasks[3].TTaskID },
                new TEntry { StartDate = now.AddHours(-10).AddMinutes(33), EndDate = now.AddHours(-8), TTaskID = tasks[4].TTaskID },
                new TEntry { StartDate = now.AddHours(-5).AddMinutes(-17), EndDate = now.AddHours(-5).AddHours(-14), TTaskID = tasks[5].TTaskID },
                new TEntry { StartDate = now.AddDays(-40).AddHours(-15).AddMinutes(-1), EndDate = now.AddDays(-39).AddHours(-1), TTaskID = tasks[7].TTaskID },
            };
            entries.ForEach(e => context.TEntries.Add(e));
            context.SaveChanges();
        }
    }
}
