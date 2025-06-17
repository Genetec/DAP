// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;

public static class MediaTypes
{
    /// <summary>Legacy media type</summary>
    public static readonly Guid Legacy = new("E5FDCEF3-0A63-4615-A2A2-BB34C892FB97");

    /// <summary>Video media type</summary>
    public static readonly Guid Video = new("4A0425E9-5F15-4675-83EB-2777CE199CB3");

    /// <summary>Audio input (from microphone of the unit to be played in Security Desk speakers) media type</summary>
    public static readonly Guid AudioIn = new("5FFBA43D-649A-473b-955A-E1EE5581A2E3");

    /// <summary>Audio output (from the Security Desk microphone to be played in unit speakers) media type</summary>
    public static readonly Guid AudioOut = new("58ED2FFD-E3DC-437e-B25F-BEACF9BBCEC7");

    /// <summary>Generic metadata media type</summary>
    public static readonly Guid Metadata = new("7D06EB54-F1DE-4b29-BCAE-0E5A102CDBEE");

    /// <summary>PTZ media type</summary>
    public static readonly Guid Ptz = new("00000000-0000-0000-0000-111111111111");

    /// <summary>Single connection per agent PTZ media type</summary>
    public static readonly Guid AgentPtz = new("00000000-0000-0000-0000-222222222222");

    /// <summary>Overlay update (from overlay source) media type</summary>
    public static readonly Guid OverlayUpdate = new("90E7162D-7D5E-4D3E-9780-110A9570C821");

    /// <summary>Overlay stream (to overlay consumer) media type</summary>
    public static readonly Guid OverlayStream = new("D6882AC2-C02D-4EFF-9217-9397EBAFB3E4");

    /// <summary>Encryption key stream, contains the encrypted symmetric keys using the user private key</summary>
    public static readonly Guid EncryptionKey = new("EC0C1A55-CAFE-96BD-1EE7-ACC01ADE5FEE");

    /// <summary>No g64 files or sequences, contains only the events common to all tracks of a collection</summary>
    public static readonly Guid CollectionEvents = new("CEDF3497-0A63-4615-A2A2-BB34C892FB97");

    /// <summary>No g64 files or sequences, contains only the events of a given archiver</summary>
    public static readonly Guid ArchiverEvents = new("BADDECAF-FACE-DEAF-ACED-7294DEED9863");

    /// <summary>Onvif analytics metadata Stream</summary>
    public static readonly Guid OnvifAnalyticsStream = new("B23F13AA-1F1D-4D87-865A-D1D706584504");

    /// <summary>Bosch Video content analysis metadata stream</summary>
    public static readonly Guid BoschVcaStream = new("CF640C0F-7B5B-4207-BD47-D219A634619C");

    /// <summary>Fusion Stream</summary>
    public static readonly Guid FusionStream = new("F0510457-FAB1-A05C-865A-08B57ADF9348");

    /// <summary>Fusion Stream Events Media type</summary>
    public static readonly Guid FusionStreamEvents = new("F5EBE375-CA6D-4A0E-968D-413D6C7BA3D7");

    /// <summary>Original video media type (AKA Confidential, Non transformed, not privacy protected)</summary>
    public static readonly Guid OriginalVideo = new("DA7ABA5E-0DD5-FEED-868F-A110CA7AB1E5");

    /// <summary>Block media type</summary>
    public static readonly Guid Block = new("91B05F52-B83A-4CCC-A3F1-0BAA3E0B665D");
}
