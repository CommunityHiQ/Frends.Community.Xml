using Pluralsight.Crypto;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Frends.Community.Xml.Tests.TestFiles
{
    /// <summary>
    /// Utility-class for generating .pfx certificate for unit tests.
    /// </summary>
    public static class CreateSignatureFile
    {
        /// <summary>
        /// Generate signature-file
        /// </summary>
        /// <param name="path">Path where the file will be created.</param>
        /// <param name="password">Password for the signature file.</param>
        public static void GenerateSignatureFile(string path, string password)
        {
            using (CryptContext ctx = new CryptContext())
            {
                ctx.Open();

                X509Certificate2 cert = ctx.CreateSelfSignedCertificate(
                    new SelfSignedCertProperties
                    {
                        IsPrivateKeyExportable = true,
                        KeyBitLength = 4096,
                        Name = new X500DistinguishedName("cn=localhost"),
                        ValidFrom = DateTime.Today.AddDays(-1),
                        ValidTo = DateTime.Today.AddYears(1),
                    });

                byte[] certData = cert.Export(X509ContentType.Pfx, password);
                File.WriteAllBytes(path, certData);
            }
        }
    }
}
