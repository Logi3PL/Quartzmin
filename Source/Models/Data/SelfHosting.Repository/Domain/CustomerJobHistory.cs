using System;
using System.Collections.Generic;

public class CustomerJobHistory : IEntity
{
    public int Id { get; set; }
    public int CustomerJobId { get; set; }
    public byte ProcessStatus { get; set; }
    public DateTimeOffset ProcessTime { get; set; }
    public virtual CustomerJob CustomerJob { get; set; }

}
