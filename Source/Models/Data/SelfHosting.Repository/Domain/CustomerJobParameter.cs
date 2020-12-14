using System;

public class CustomerJobParameter : BaseEntity
{
    public int CustomerJobId { get; set; }
    public virtual CustomerJob CustomerJob { get; set; }
    public string ParamSource { get; set; }
    public string ParamKey { get; set; }
    public string ParamValue { get; set; }

}
