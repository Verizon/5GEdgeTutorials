using System;
namespace Wavelength.Core.DomainObjects
{
    public class DomainBase
    {
        public virtual string? DocumentType { get; set; }

        public Guid? DocumentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
