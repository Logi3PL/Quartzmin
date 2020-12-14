using System.Collections.Generic;

public class Customer:BaseEntity
{
    public string CustomerName { get; set; }
    public string CustomerCode { get; set; }
    public string Description { get; set; }
    public virtual List<CustomerJob> CustomerJobs { get; set; }

}
