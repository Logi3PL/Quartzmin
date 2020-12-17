using System;

public class CustomerJobSubscriber : BaseEntity
{
    public int CustomerJobId { get; set; }
    public virtual CustomerJob CustomerJob { get; set; }
    public byte SubscriberType { get; set; }
    public string Subscriber { get; set; }
    public string Description { get; set; }

}
