using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracktor.model.DAL
{
    public class TUser
    {
        public int TUserID { get; set; }
        public string Name { get; set; }
        public string AspNetId { get; set; }

        public TState CurrentState { get; set; }
        public int LastTaskID { get; set; }

        public virtual ICollection<TProject> TProjects { get; set; }
    }

    public class TProject
    {
        public int TProjectID { get; set; }

        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsObsolete { get; set; }

        public int TUserID { get; set; }
        public virtual ICollection<TTask> TTasks { get; set; }
    }

    public class TTask
    {
        public int TTaskID { get; set; }
        
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsObsolete { get; set; }

        public int TProjectID { get; set; }
        public virtual TProject TProject { get; set; }
        public virtual ICollection<TEntry> TEntries { get; set; }
    }

    public class TEntry
    {
        public int TEntryID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int TTaskID { get; set; }
        public virtual TTask TTask { get; set; }
    }

    public class TracktorContext : DbContext
    {
        public TracktorContext() : base("TracktorContext")
        {

        }

        public DbSet<TUser> TUsers { get; set; }
        public DbSet<TProject> TProjects { get; set; }
        public DbSet<TTask> TTasks { get; set; }
        public DbSet<TEntry> TEntries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
