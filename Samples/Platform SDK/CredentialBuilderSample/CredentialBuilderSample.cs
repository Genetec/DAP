// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.AccessControl.Credentials.CardCredentials;
using Genetec.Sdk.Credentials;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Entities.Builders;
using Genetec.Sdk.Queries;

namespace Genetec.Dap.CodeSamples;

public class CredentialBuilderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        DisplayCredentialFormats(engine);

        ICredentialBuilder credentialBuilder = engine.EntityManager.GetCredentialBuilder();

        await BuildAndDisplayCredential(engine, credentialBuilder, "Standard Wiegand Credential", new WiegandStandardCredentialFormat(facility: 1, cardId: 2));
        await BuildAndDisplayCredential(engine, credentialBuilder, "H10306 Wiegand Credential", new WiegandH10306CredentialFormat(facility: 1, cardId: 2));
        await BuildAndDisplayCredential(engine, credentialBuilder, "H10304 Wiegand Credential", new WiegandH10304CredentialFormat(facility: 1, cardId: 2));
        await BuildAndDisplayCredential(engine, credentialBuilder, "H10302 Wiegand Credential", new WiegandH10302CredentialFormat(cardId: 1));
        await BuildAndDisplayCredential(engine, credentialBuilder, "CSN32 Wiegand Credential", new WiegandCsn32CredentialFormat(cardId: 12345));
        await BuildAndDisplayCredential(engine, credentialBuilder, "48-Bit Corporate 1000 Wiegand Credential", new Wiegand48BitCorporate1000CredentialFormat(companyId: 1, cardId: 2));
        await BuildAndDisplayCredential(engine, credentialBuilder, "Corporate 1000 Wiegand Credential", new WiegandCorporate1000CredentialFormat(companyId: 1, cardId: 2));
        await BuildAndDisplayCredential(engine, credentialBuilder, "License Plate Credential", new LicensePlateCredentialFormat(licensePlate: "12345"));
        await BuildAndDisplayCredential(engine, credentialBuilder, "Keypad Credential", new KeypadCredentialFormat(credentialCode: 12345));
        await BuildAndDisplayCredential(engine, credentialBuilder, "Raw Card Credential", new RawCardCredentialFormat(rawData: "1234", bitLength: 32));

        var fascN75Dict = new Dictionary<string, string>
        {
            { FascN75BitCardCredentialFormat.AGENCY_CODE_FIELD_NAME, "16383" },
            { FascN75BitCardCredentialFormat.SYSTEM_CODE_FIELD_NAME, "16383" },
            { FascN75BitCardCredentialFormat.CREDENTIAL_NUMBER_FIELD_NAME, "1234" },
            { FascN75BitCardCredentialFormat.EXP_DATE_FIELD_NAME, "9999" }
        };

        await BuildAndDisplayCredential(engine, credentialBuilder, "FascN 75-Bit Card Credential", new FascN75BitCardCredentialFormat(fascN75Dict));

        var fascN200Dict = new Dictionary<string, string>
        {
            { FascN200BitCardCredentialFormat.AGENCY_CODE_FIELD_NAME, "9999" },
            { FascN200BitCardCredentialFormat.SYSTEM_CODE_FIELD_NAME, "9999" },
            { FascN200BitCardCredentialFormat.CREDENTIAL_NUMBER_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.CS_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.ICI_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.PI_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.OC_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.OI_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.POA_FIELD_NAME, "0" },
            { FascN200BitCardCredentialFormat.LRC_FIELD_NAME, "F" }
        };

        await BuildAndDisplayCredential(engine, credentialBuilder, "FascN 200-Bit Card Credential", new FascN200BitCardCredentialFormat(fascN200Dict));
    }

    private async Task BuildAndDisplayCredential(Engine engine, ICredentialBuilder credentialBuilder, string name, CredentialFormat format)
    {
        Console.WriteLine($"Creating credential with format: {format.UniqueId}.");

        Credential credential = await TryGetCredentialByFormat(engine, format);

        if (credential != null)
        {
            Console.WriteLine($"Credential with format {format.UniqueId} already exists.");
        }
        else
        {
            credential = credentialBuilder.SetName(name).SetFormat(format).Build();
        }

        DisplayCredentialDetails(credential);
    }

    private async Task<Credential> TryGetCredentialByFormat(Engine engine, CredentialFormat format)
    {
        var query = (CredentialConfigurationQuery)engine.ReportManager.CreateReportQuery(ReportType.CredentialConfiguration);
        query.UniqueIds.Add(format);
        QueryCompletedEventArgs args = await Task.Factory.FromAsync(query.BeginQuery, query.EndQuery, null);
        return args.Data.AsEnumerable().Select(row => engine.GetEntity(row.Field<Guid>(nameof(Guid)))).OfType<Credential>().FirstOrDefault();
    }

    private void DisplayCredentialFormats(Engine engine)
    {
        var config = (SystemConfiguration)engine.GetEntity(entityId: SystemConfiguration.SystemConfigurationGuid);
        foreach (CredentialFormat format in config.CredentialFormats)
        {
            Console.WriteLine("Credential Format Details:");
            Console.WriteLine("---------------------------");
            Console.WriteLine($"Type Name       : {format.GetType().Name}");
            Console.WriteLine($"Name            : {format.Name}");
            Console.WriteLine($"Type            : {format.Type}");
            Console.WriteLine($"Bit Length      : {format.BitLength}");
            Console.WriteLine($"Custom Format   : {format.IsCustomFormat}");
            Console.WriteLine();
        }
    }

    private void DisplayCredentialDetails(Credential credential)
    {
        Console.WriteLine("Credential Details:");
        Console.WriteLine("-------------------");
        Console.WriteLine($"Name            : {credential.Name}");
        Console.WriteLine($"Type            : {credential.Format.GetType().Name}");
        Console.WriteLine($"Unique Id       : {credential.Format.UniqueId}");
        Console.WriteLine($"Bit Length      : {credential.Format.BitLength}");
        Console.WriteLine($"Raw Data        : {credential.Format.RawData}");

        switch (credential.Format)
        {
            case KeypadCredentialFormat keypad:
                Console.WriteLine($"Credential Code : {keypad.Code}");
                break;
            case Wiegand48BitCorporate1000CredentialFormat wiegand48:
                Console.WriteLine($"Company ID      : {wiegand48.Facility}");
                Console.WriteLine($"Card ID         : {wiegand48.CardId}");
                break;
            case WiegandCorporate1000CredentialFormat wiegandCorp:
                Console.WriteLine($"Company ID      : {wiegandCorp.Facility}");
                Console.WriteLine($"Card ID         : {wiegandCorp.CardId}");
                break;
            case WiegandCsn32CredentialFormat csn32:
                Console.WriteLine($"Card ID         : {csn32.CardId}");
                break;
            case WiegandH10302CredentialFormat h10302:
                Console.WriteLine($"Card ID         : {h10302.CardId}");
                break;
            case WiegandH10304CredentialFormat h10304:
                Console.WriteLine($"Facility Code   : {h10304.Facility}");
                Console.WriteLine($"Card ID         : {h10304.CardId}");
                break;
            case WiegandH10306CredentialFormat h10306:
                Console.WriteLine($"Facility Code   : {h10306.Facility}");
                Console.WriteLine($"Card ID         : {h10306.CardId}");
                break;
            case WiegandStandardCredentialFormat wiegandStd:
                Console.WriteLine($"Facility Code   : {wiegandStd.Facility}");
                Console.WriteLine($"Card ID         : {wiegandStd.CardId}");
                break;
            case RawCardCredentialFormat rawCard:
                Console.WriteLine($"Raw Data        : {rawCard.RawData}");
                Console.WriteLine($"Bit Length      : {rawCard.BitLength}");
                break;
        }

        Console.WriteLine();
    }
}