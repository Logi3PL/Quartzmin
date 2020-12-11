using System;

public class CustomerJob:BaseEntity
{
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; }
    public Guid JobId { get; set; }
    public virtual Job Job { get; set; }
    public string Cron { get; set; }
    public bool IsActive { get; set; }


}
