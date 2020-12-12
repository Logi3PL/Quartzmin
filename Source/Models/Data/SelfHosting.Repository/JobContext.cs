using Microsoft.EntityFrameworkCore;

namespace SelfHosting.Repository
{
    public class JobContext:DbContext
    {

        public JobContext(DbContextOptions<JobContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("JMS_JOB");

                entity.HasKey(e => e.Id);

                entity.HasMany(x => x.CustomerJobs).WithOne(y => y.Job);

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).HasColumnName("NAME");

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(1000);

                entity.Property(e => e.Active)
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedBy).HasColumnName("CREATEDBY");

                entity.Property(e => e.CreatedTime)
                    .HasColumnName("CREATEDTIME")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ModifiedBy).HasColumnName("MODIFIEDBY").IsRequired(false);

                entity.Property(e => e.ModifiedTime).IsRequired(false)
                    .HasColumnName("MODIFIEDTIME")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("JMS_CUSTOMER");

                entity.HasMany(x => x.CustomerJobs).WithOne(y => y.Customer);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CustomerName).HasColumnName("CUSTOMERNAME");
                entity.Property(e => e.CustomerCode).HasColumnName("CUSTOMERCODE");

                entity.Property(e => e.Active)
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedBy).HasColumnName("CREATEDBY");

                entity.Property(e => e.CreatedTime)
                    .HasColumnName("CREATEDTIME")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(1000);

                entity.Property(e => e.ModifiedBy).HasColumnName("MODIFIEDBY").IsRequired(false);

                entity.Property(e => e.ModifiedTime).IsRequired(false)
                    .HasColumnName("MODIFIEDTIME")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

            });

            modelBuilder.Entity<CustomerJob>(entity =>
            {
                entity.ToTable("JMS_CUSTOMERJOB");

                entity.HasKey(cj => cj.Id);

                entity.HasOne(c => c.Customer)
                .WithMany(b => b.CustomerJobs)
                .HasForeignKey(c => c.CustomerId); 

                entity.HasOne(bc => bc.Job)
                .WithMany(c => c.CustomerJobs)
                .HasForeignKey(j => j.JobId);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMERID");
                entity.Property(e => e.JobId).HasColumnName("JOBID");
                entity.Property(e => e.Cron).HasColumnName("CRON").HasMaxLength(1000);

                entity.Property(e => e.Active)
                    .HasColumnName("ACTIVE")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedBy).HasColumnName("CREATEDBY");

                entity.Property(e => e.CreatedTime)
                    .HasColumnName("CREATEDTIME")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ModifiedBy).HasColumnName("MODIFIEDBY").IsRequired(false);

                entity.Property(e => e.ModifiedTime).IsRequired(false)
                    .HasColumnName("MODIFIEDTIME")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

            });

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            //optionsBuilder.UseSqlServer("Data Source=nctestdb01.e-cozum.com;Initial Catalog=scheduler_tayfun;Integrated Security=False;Persist Security Info=False;User ID=scheduler_test;Password=3T0r1m1t5wR;MultipleActiveResultSets=True;");

        }

        public  DbSet<Customer>  Customers { get; set; }
        public  DbSet<Job> Jobs { get; set; }
        public  DbSet<CustomerJob> CustomerJob { get; set; }
    }
}
