using System;
namespace Wavelength.Services
{
    public interface IPinnedCertificateService 
    {
        byte[] GetCertificateFromAssets();
    }
}
