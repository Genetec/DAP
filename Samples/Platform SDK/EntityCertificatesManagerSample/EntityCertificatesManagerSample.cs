// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Workflows.EntityCertificatesManagers;

namespace Genetec.Dap.CodeSamples;

public class EntityCertificatesManagerSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        CertificateSummary summary = await GetOrCreateCertificate(engine, "Sample");

        Console.WriteLine($"Certificate ID: {summary.Id}, Thumbprint: {summary.Thumbprint}");

        X509Certificate2 certificate = await engine.EntityCertificatesManager.GetCertificateAsync(summary.Id);

        Console.WriteLine($"Certificate fetched: {certificate.Subject}");

        await engine.EntityCertificatesManager.DeleteCertificateAsync(summary.Id);
        Console.WriteLine($"Certificate deleted: {summary.Id}");
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