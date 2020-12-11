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

            //Fluent Api ile Entitylerimiz arasındaki ORM ilişkisini kod içerisinde gerçekleştiriyoruz.

            modelBuilder.Entity<Customer>().ToTable("JMS_CUSTOMER")
                .HasMany(x => x.CustomerJobs)
                .WithOne(y => y.Customer);

            modelBuilder.Entity<Job>().ToTable("JMS_JOB")
                .HasMany(x => x.CustomerJobs)
                .WithOne(y => y.Job);


            modelBuilder.Entity<CustomerJob>().ToTable("JMS_CUSTOMERJOB")
        .HasKey(cj => new { cj.CustomerId, cj.JobId });
            modelBuilder.Entity<CustomerJob>()
                .HasOne(c => c.Customer)
                .WithMany(b => b.CustomerJobs)
                .HasForeignKey(c => c.CustomerId);

            modelBuilder.Entity<CustomerJob>().ToTable("JMS_CUSTOMERJOB")
                .HasOne(bc => bc.Job)
                .WithMany(c => c.CustomerJobs)
                .HasForeignKey(j => j.JobId);


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
