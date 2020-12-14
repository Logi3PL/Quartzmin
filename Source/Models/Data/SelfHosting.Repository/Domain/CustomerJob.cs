using System;
using System.Collections.Generic;

public class CustomerJob:BaseEntity
{
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; }
    public int JobId { get; set; }
    public virtual Job Job { get; set; }
    public string Cron { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public virtual List<CustomerJobParameter> CustomerJobParameters { get; set; }
}
