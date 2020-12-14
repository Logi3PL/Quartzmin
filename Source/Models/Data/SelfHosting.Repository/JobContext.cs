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

                entity.Property(e => e.Code)
                    .HasColumnName("CODE")
                    .HasMaxLength(100);

                entity.Property(e => e.Description).IsRequired(false)
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
                entity.Property(e => e.StartDate).HasColumnName("STARTDATE").IsRequired(false);
                entity.Property(e => e.EndDate).HasColumnName("ENDDATE").IsRequired(false);
                entity.Property(e => e.Cron).HasColumnName("CRON").HasMaxLength(1000).IsRequired(false);

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

            modelBuilder.Entity<CustomerJobParameter>(entity =>
            {
                entity.ToTable("JMS_CUSTOMERJOBPARAMETER");

                entity.HasKey(cj => cj.Id);

                entity.HasOne(c => c.CustomerJob)
                .WithMany(b => b.CustomerJobParameters)
                .HasForeignKey(c => c.CustomerJobId);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerJobId).HasColumnName("CUSTOMERJOBID");
                entity.Property(e => e.ParamSource).HasColumnName("PARAMSOURCE");
                entity.Property(e => e.ParamKey).HasColumnName("PARAMKEY");
                entity.Property(e => e.ParamValue).HasColumnName("PARAMVALUE");

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
            
            modelBuilder.Entity<CustomerJobHistory>(entity =>
            {
                entity.ToTable("JMS_CUSTOMERJOBHISTORY");

                entity.HasOne(x => x.CustomerJob).WithMany(y => y.CustomerJobHistories)
                .HasForeignKey(c => c.CustomerJobId);

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CustomerJobId).HasColumnName("CUSTOMERJOBID");
                entity.Property(e => e.ProcessStatus).HasColumnName("PROCESSSTATUS");
                entity.Property(e => e.ProcessTime).HasColumnName("PROCESSTIME").HasColumnType("datetimeoffset(7)");

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
        public DbSet<CustomerJobParameter> CustomerJobParameter { get; set; }
        public  DbSet<CustomerJobHistory> CustomerJobHistories { get; set; }
    }
}
