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

    public interface ITracktorContext : IDisposable
    {
        DbSet<TUser> TUsers { get; set; }
        DbSet<TProject> TProjects { get; set; }
        DbSet<TTask> TTasks { get; set; }
        DbSet<TEntry> TEntries { get; set; }

        int SaveChanges();
    }

    public class TracktorContext : DbContext, ITracktorContext
    {
        public TracktorContext() : base("DefaultConnection")
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
