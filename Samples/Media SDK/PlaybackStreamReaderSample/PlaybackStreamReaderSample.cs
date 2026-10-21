// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Genetec.Sdk;
using Genetec.Sdk.Entities;
using Genetec.Sdk.Media.Reader;
using Genetec.Sdk.Media.Video;

namespace Genetec.Dap.CodeSamples;

public class PlaybackStreamReaderSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        const string cameraGuid = "YOUR_CAMERA_GUID_HERE"; // TODO: Replace with your camera GUID
        const string metadataStreamName = ""; // Set a stream name, or leave empty to read all camera metadata streams.
        DateTime startTimeUtc = DateTime.UtcNow.AddMinutes(-5);
        DateTime endTimeUtc = DateTime.UtcNow;

        await LoadEntities(engine, token, EntityType.Camera);

        if (engine.GetEntity(new Guid(cameraGuid)) is not Camera camera)
        {
            Console.WriteLine($"Camera {cameraGuid} not found");
            return;
        }

        await ReadMetadataStream(token);
        await ReadVideoStream(token);
        await ReadAudioStream(token);

        async Task ReadVideoStream(CancellationToken cancellationToken)
        {
            Console.WriteLine($"\nReading video stream from camera: {camera.Name}");
            await using var reader = PlaybackStreamReader.CreateVideoReader(engine, camera.Guid);
            await ReadStream(reader, cancellationToken);
        }

        async Task ReadAudioStream(CancellationToken cancellationToken)
        {
            Console.WriteLine($"\nReading audio stream from camera: {camera.Name}");
            await using var reader = PlaybackStreamReader.CreateAudioReader(engine, camera.Guid);
            await ReadStream(reader, cancellationToken);
        }

        async Task ReadMetadataStream(CancellationToken cancellationToken)
        {
            MetadataStreamInfo[] streams = camera.MetadataStreams
                .Where(stream => string.IsNullOrEmpty(metadataStreamName) || stream.Name == metadataStreamName)
                .ToArray();

            if (streams.Length == 0)
            {
                Console.WriteLine("No matching metadata stream available.");
                return;
            }

            foreach (MetadataStreamInfo stream in streams)
            {
                Console.WriteLine($"\nMetadata stream: {stream.Name} | ID: {stream.StreamId} | Type: {GetMediaTypeName(stream.MediaType)} ({stream.MediaType}) | Archived only: {stream.IsGhost}");
                await using var reader = PlaybackStreamReader.CreateReader(engine, camera.Guid, stream.StreamId);
                await ReadStream(reader, cancellationToken, printMetadata: true);
            }
        }

        async Task ReadStream(PlaybackStreamReader reader, CancellationToken cancellationToken, bool printMetadata = false)
        {
            await reader.ConnectAsync(cancellationToken);
            await reader.SeekAsync(startTimeUtc, cancellationToken);

            var (frames, bytes, start) = (0, 0L, DateTime.Now);

            while (true)
            {
                RawDataContent rawDataContent = await reader.ReadAsync(cancellationToken);
                if (rawDataContent is null)
                {
                    Console.WriteLine("\nEnd of stream reached.");
                    break;
                }

                using (rawDataContent)
                {
                    if (rawDataContent.FrameTime > endTimeUtc)
                        break;

                    frames++;
                    bytes += rawDataContent.Data.Count;
                    string statistics = $"{rawDataContent.FrameTime:O} | {GetMediaTypeName(rawDataContent.MediaType)} | {rawDataContent.Format} | Key frame: {rawDataContent.IsKeyFrame} | Size: {rawDataContent.Data.Count} B | Avg: {bytes / frames} B | FPS: {frames / (DateTime.Now - start).TotalSeconds:F1}";
                    if (printMetadata)
                    {
                        Console.WriteLine(statistics);
                        PrintMetadata(rawDataContent.Format, rawDataContent.Data);
                    }
                    else
                    {
                        Console.Write("\r" + statistics);
                    }
                }
            }
        }
    }

    private static void PrintMetadata(EncodingType format, ArraySegment<byte> data)
    {
        try
        {
            switch (format)
            {
                case EncodingType.Overlay:
                    PrintOverlay(data);
                    break;
                case EncodingType.OnvifAnalytics:
                    Console.WriteLine("  ONVIF analytics XML:");
                    Console.WriteLine(ReadXml(data).ToString());
                    break;
                default:
                    Console.WriteLine($"  No content decoder for {format}. Raw payload: {data.Count} bytes.");
                    break;
            }
        }
        catch (Exception error) when (error is InvalidDataException or EndOfStreamException or XmlException or FormatException)
        {
            Console.WriteLine($"  Metadata could not be decoded: {error.Message}");
        }
    }

    private static void PrintOverlay(ArraySegment<byte> data)
    {
        using var stream = new MemoryStream(data.Array, data.Offset, data.Count, writable: false);
        using var reader = new BinaryReader(stream);
        int headerSize = reader.ReadInt32();
        byte payloadType = reader.ReadByte();
        uint payloadSize = reader.ReadUInt32();
        byte version = reader.ReadByte();

        // ponytail: decode version 1 overlay frames; add a version branch if the recording format changes.
        if (version != 1 || payloadType != 1)
        {
            Console.WriteLine($"  Unsupported overlay version {version} or payload type {payloadType}.");
            return;
        }

        if (headerSize < 35 || headerSize > stream.Length || payloadSize != stream.Length - headerSize)
            throw new InvalidDataException("Invalid overlay header or payload size.");

        // Skip the fixed timing fields; RawDataContent already provides the frame timestamp.
        stream.Position = 31;
        int dynamicHeaderSize = reader.ReadInt32();
        if (dynamicHeaderSize != headerSize - 35)
            throw new InvalidDataException("Invalid overlay dynamic header size.");

        stream.Position = headerSize;
        int layerCount = reader.ReadInt32();
        if (layerCount < 0 || layerCount > (stream.Length - stream.Position) / 36)
            throw new InvalidDataException("Invalid overlay layer count.");

        Console.WriteLine($"  Overlay layers: {layerCount}");
        for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
        {
            var layerId = new Guid(ReadBytes(reader, 16));
            byte depth = reader.ReadByte();
            float height = reader.ReadSingle();
            float width = reader.ReadSingle();
            uint durationSeconds = reader.ReadUInt32();
            bool displayedByDefault = reader.ReadBoolean();
            Console.WriteLine(FormattableString.Invariant($"  Layer: {layerId} | Canvas: {width} x {height} | Depth: {depth} | Duration: {durationSeconds} s | Displayed by default: {displayedByDefault}"));

            int imageCount = reader.ReadByte();
            for (int imageIndex = 0; imageIndex < imageCount; imageIndex++)
            {
                int imageId = reader.ReadInt32();
                byte[] image = ReadBytes(reader, reader.ReadInt32());
                bool isPng = image.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
                using var sha256 = SHA256.Create();
                string hash = BitConverter.ToString(sha256.ComputeHash(image)).Replace("-", "").ToLowerInvariant();
                Console.WriteLine($"    Image: ID={imageId}, Format={(isPng ? "PNG" : "Unknown")}, Bytes={image.Length}, SHA256={hash}");
            }

            int locationCount = reader.ReadByte();
            for (int locationIndex = 0; locationIndex < locationCount; locationIndex++)
            {
                int imageId = reader.ReadInt32();
                double x = reader.ReadDouble();
                double y = reader.ReadDouble();
                double imageWidth = reader.ReadDouble();
                double imageHeight = reader.ReadDouble();
                Console.WriteLine(FormattableString.Invariant($"    Image placement: ID={imageId}, X={x}, Y={y}, Width={imageWidth}, Height={imageHeight}"));
            }

            byte[] drawing = ReadBytes(reader, reader.ReadInt32());
            int drawingCount = drawing.Length == 0 ? 0 : PrintDrawing(ReadXml(new ArraySegment<byte>(drawing)));
            if (drawingCount == 0 && locationCount == 0)
                Console.WriteLine("    Empty layer drawing (clear update).");
        }

        // Version 1 recordings can include one trailing padding byte per layer.
        if (stream.Length - stream.Position > layerCount)
            throw new InvalidDataException("Unexpected data after the overlay layers.");
    }

    private static int PrintDrawing(XDocument document)
    {
        XNamespace drawingNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        int count = 0;
        foreach (XElement drawing in document.Descendants(drawingNamespace + "GeometryDrawing"))
        {
            XElement pen = drawing.Element(drawingNamespace + "GeometryDrawing.Pen")?.Element(drawingNamespace + "Pen");
            string fill = (string)drawing.Attribute("Brush") ?? drawing.Element(drawingNamespace + "GeometryDrawing.Brush")?.ToString(SaveOptions.DisableFormatting);
            string stroke = (string)pen?.Attribute("Brush") ?? pen?.Element(drawingNamespace + "Pen.Brush")?.ToString(SaveOptions.DisableFormatting);
            string thickness = (string)pen?.Attribute("Thickness") ?? "1";

            if (!IsVisibleBrush(fill) && (!IsVisibleBrush(stroke) || XmlConvert.ToDouble(thickness) == 0))
                continue;

            XElement geometry = drawing.Element(drawingNamespace + "GeometryDrawing.Geometry")?.Elements().FirstOrDefault();
            if (geometry == null)
                continue;

            string details;
            switch (geometry.Name.LocalName)
            {
                case "EllipseGeometry":
                    string radiusX = (string)geometry.Attribute("RadiusX") ?? "0";
                    string radiusY = (string)geometry.Attribute("RadiusY") ?? "0";
                    string kind = XmlConvert.ToDouble(radiusX) == XmlConvert.ToDouble(radiusY) ? "Circle" : "Ellipse";
                    details = $"{kind}: Center=({(string)geometry.Attribute("Center") ?? "0,0"}), RadiusX={radiusX}, RadiusY={radiusY}";
                    break;
                case "LineGeometry":
                    details = $"Line: Start=({(string)geometry.Attribute("StartPoint") ?? "0,0"}), End=({(string)geometry.Attribute("EndPoint") ?? "0,0"})";
                    break;
                case "RectangleGeometry":
                    details = $"Rectangle: Rect=({geometry.Attribute("Rect")?.Value})";
                    break;
                default:
                    details = $"Geometry: {geometry.ToString(SaveOptions.DisableFormatting)}";
                    break;
            }

            Console.WriteLine($"    {details} | Fill={fill ?? "None"}, Stroke={stroke ?? "None"}, Thickness={thickness}");
            count++;
        }

        foreach (XElement drawing in document.Descendants(drawingNamespace + "GlyphRunDrawing"))
        {
            XElement run = drawing.Descendants(drawingNamespace + "GlyphRun").FirstOrDefault();
            if (run == null)
                continue;

            string characters = ((string)run.Attribute("Characters") ?? "").Replace("\r", "\\r").Replace("\n", "\\n");
            string font = (string)run.Descendants(drawingNamespace + "GlyphTypeface").FirstOrDefault()?.Attribute("FontUri");
            Console.WriteLine($"    Text: \"{characters}\" | Baseline=({run.Attribute("BaselineOrigin")?.Value}), FontSize={run.Attribute("FontRenderingEmSize")?.Value}, Color={drawing.Attribute("ForegroundBrush")?.Value}, Font={font}");
            count++;
        }

        return count;
    }

    private static bool IsVisibleBrush(string brush) =>
        !string.IsNullOrEmpty(brush) && brush != "{x:Null}" && brush != "Transparent" &&
        !(brush.Length == 9 && brush.StartsWith("#00", StringComparison.OrdinalIgnoreCase));

    private static byte[] ReadBytes(BinaryReader reader, int length)
    {
        if (length < 0 || length > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new InvalidDataException("Invalid overlay field length.");

        return reader.ReadBytes(length);
    }

    private static XDocument ReadXml(ArraySegment<byte> data)
    {
        using var stream = new MemoryStream(data.Array, data.Offset, data.Count, writable: false);
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = data.Count
        };
        using XmlReader reader = XmlReader.Create(stream, settings);
        return XDocument.Load(reader);
    }

    private static string GetMediaTypeName(Guid mediaTypeGuid)
    {
        if (mediaTypeGuid == MediaTypes.Video) return "Video";
        if (mediaTypeGuid == MediaTypes.AudioIn) return "Audio In";
        if (mediaTypeGuid == MediaTypes.AudioOut) return "Audio Out";
        if (mediaTypeGuid == MediaTypes.Metadata) return "Metadata";
        if (mediaTypeGuid == MediaTypes.Ptz) return "Ptz";
        if (mediaTypeGuid == MediaTypes.AgentPtz) return "Agent Ptz";
        if (mediaTypeGuid == MediaTypes.OverlayUpdate) return "Overlay Update";
        if (mediaTypeGuid == MediaTypes.OverlayStream) return "Overlay Stream";
        if (mediaTypeGuid == MediaTypes.EncryptionKey) return "Encryption Key";
        if (mediaTypeGuid == MediaTypes.CollectionEvents) return "Collection Events";
        if (mediaTypeGuid == MediaTypes.ArchiverEvents) return "Archiver Events";
        if (mediaTypeGuid == MediaTypes.OnvifAnalyticsStream) return "Onvif Analytics Stream";
        if (mediaTypeGuid == MediaTypes.BoschVcaStream) return "Bosch Vca Stream";
        if (mediaTypeGuid == MediaTypes.FusionStream) return "Fusion Stream";
        if (mediaTypeGuid == MediaTypes.FusionStreamEvents) return "Fusion Stream Events";
        if (mediaTypeGuid == MediaTypes.OriginalVideo) return "Original Video";
        if (mediaTypeGuid == MediaTypes.Block) return "Block";
        return "Unknown";
    }
}
