// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Workflows.EntityCertificatesManagers;

class Program
{
    static Program() => SdkResolver.Initialize();

    static async Task Main()
    {
        const string server = "localhost";
        const string username = "admin";
        const string password = "";

        using var engine = new Engine();

        ConnectionStateCode state = await engine.LogOnAsync(server, username, password);

        if (state == ConnectionStateCode.Success)
        {
            CertificateSummary summary = await GetOrCreateCertificate(engine, "Sample");

            Console.WriteLine($"Certificate ID: {summary.Id}, Thumbprint: {summary.Thumbprint}");

            X509Certificate2 certificate = await engine.EntityCertificatesManager.GetCertificateAsync(summary.Id);

            Console.WriteLine($"Certificate fetched: {certificate.Subject}");

            await engine.EntityCertificatesManager.DeleteCertificateAsync(summary.Id);
            Console.WriteLine($"Certificate deleted: {summary.Id}");
        }
        else
        {
            Console.WriteLine($"Logon failed: {state}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task<CertificateSummary> GetOrCreateCertificate(Engine engine, string tag)
    {
        List<CertificateSummary> summaries = await engine.EntityCertificatesManager.GetAllCertificatesOfEntityAsync(SystemConfiguration.SystemConfigurationGuid, tag);
        if (!summaries.Any())
        {
            Console.WriteLine($"Creating certificate with tag: {tag}");

            X509Certificate2 certificate = CreateSelfSignedCertificate();

            Guid certificateId = await engine.EntityCertificatesManager.CreateCertificateAsync(
                relatedEntity: SystemConfiguration.SystemConfigurationGuid,
                certificate: certificate,
                tag: tag,
                eraseExistingEntries: false);

            return await engine.EntityCertificatesManager.GetCertificateSummaryAsync(certificateId);
        }

        Console.WriteLine("A certificate already exists.");
           
        return summaries.First();
    }

    public static X509Certificate2 CreateSelfSignedCertificate()
    {
        var distinguishedName = new X500DistinguishedName("CN=SelfSignedEncryptionCert");
        using var rsa = new RSACryptoServiceProvider(2048);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment, false));
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        byte[] certData = certificate.Export(X509ContentType.Pfx);

        return new X509Certificate2(certData, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }
}