using System;
using System.IO;
using Android.Content.Res;
using Wavelength.Services;

namespace Wavelength.Droid.Services
{
    public class PinnedCertificateService 
	    : IPinnedCertificateService
    {
        private readonly AssetManager _assetManager;

        public PinnedCertificateService(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public byte[] GetCertificateFromAssets()
        {
            const int maxReadSize = 256 * 1024;
            byte[] content;

            using var br = new BinaryReader(_assetManager.Open("fullchain.cer"));
            content = br.ReadBytes(maxReadSize);
            return content;
        }
    }
}
