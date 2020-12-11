using System;

public class BaseEntity
{
    public int Id { get; set; }
    /// <summary>
    /// Aktif
    /// </summary>
    public byte Active { get; set; }

    /// <summary>
    /// Olusma zamanı
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Olusturan
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Degisme zamanı
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// Degistiren
    /// </summary>
    public int? ModifiedBy { get; set; }
}
