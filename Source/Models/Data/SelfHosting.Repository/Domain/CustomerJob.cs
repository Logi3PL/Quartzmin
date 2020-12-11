using System;

public class CustomerJob:BaseEntity
{
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; }
    public int JobId { get; set; }
    public virtual Job Job { get; set; }
    public string Cron { get; set; }
}
