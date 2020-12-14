using System.Collections.Generic;

public class Job:BaseEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public virtual List<CustomerJob> CustomerJobs { get; set; }
}
