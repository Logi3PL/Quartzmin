using System.Collections.Generic;

public class Job:BaseEntity
{
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string BaseUrl { get; set; }
    public string EndPoint { get; set; }
    public virtual List<CustomerJob> CustomerJobs { get; set; }
}
