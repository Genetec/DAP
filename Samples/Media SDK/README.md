# Media SDK developer guide

The Media SDK extends the Platform SDK with real-time video streaming, playback control, audio transmission, PTZ camera operations, and video export for Security Center integrations. The samples illustrate the classes and patterns used for these operations.

## Prerequisites

- **.NET Framework 4.8.1 targeting pack**: these samples target `net481`. Media SDK playback, streaming, frame access, audio, conversion, and export require .NET Framework and do not support .NET 8 or .NET 10.
- **Security Center SDK**: installed with the `GSC_SDK` environment variable configured.
- **Build tools**: Visual Studio 2022 version 17.8 or later, or the .NET 8 SDK or a later compatible SDK for command-line builds. The samples compile with C# 12.
- **Security Center**: the Security Desk and Config Tool client applications are installed.
- **Valid Security Center license**: all samples include the development SDK certificate.

## Building a sample

The Media SDK projects use `Debug` and `Release` configurations and target .NET Framework 4.8.1. Selecting `Debug_NET8` or `Release_NET8` in the solution still builds these projects for `net481`.

1. In PowerShell, set `GSC_SDK` to the installed SDK directory containing the .NET Framework assemblies. Replace the example path with your installation:

   ```powershell
   $env:GSC_SDK = 'C:\Program Files (x86)\Genetec Security Center 5.14 SDK'
   ```

2. From the repository root, build the MediaPlayer sample:

   ```powershell
   dotnet build "Samples/Media SDK/MediaPlayerSample/MediaPlayerSample.csproj" -c Debug -f net481
   ```

3. Use `-c Release` for a release build, or replace the project path with another Media sample. In Visual Studio, select **Debug** or **Release** and build the project.

For a modern .NET application that needs video, see [Migrating an application that uses the Media SDK](https://github.com/Genetec/DAP/wiki/platform-sdk-migrating-to-modern-dotnet#applications-that-use-the-media-sdk). The guide covers a separate .NET Framework process for media operations and Genetec™ Web Player hosting for video display.

## Dependencies and deployment

The projects reference `Genetec.Sdk.dll` and `Genetec.Sdk.Media.dll` through `GSC_SDK`; .NET Framework supplies the Windows Presentation Foundation (WPF) assemblies used by the samples. A media DLL found in a modern SDK directory does not enable .NET 8 or .NET 10 media operations.

Install the .NET Framework 4.8.1 runtime and compatible Security Center media components on the machine running the sample. Use the Security Center SDK installer or the Security Center installation to supply SDK assemblies and media dependencies. Keep **Copy Local** set to `False` for SDK references. The shared .NET Framework resolver locates the installation; deploy any additional non-SDK dependencies with the application. See [Shared sample helpers](../Shared/README.md#choosing-an-assembly-resolver) for resolver setup and dependency deployment.

## Media SDK architecture

The Media SDK uses the Platform SDK and adds media operations.

### Foundation on the Platform SDK

The Media SDK builds on the Platform SDK. Many classes require an `Engine` instance connected to Security Center.

### Media extensions

The Media SDK adds specialized capabilities:
- **Real-time Streaming**: Live video feeds from cameras
- **Playback Control**: Archived video playback with seeking and speed control
- **Media Processing**: Video overlays and frame-level processing
- **PTZ Camera Control**: PTZ controls for pan, tilt, and zoom operations
- **Export Operations**: Video export and format conversion

## Media SDK classes

These classes support video display, processing, playback, export, file analysis, audio, and overlays.

### Video and media classes

Use these classes to display video, access decoded frames, and synchronize playback.

#### MediaPlayer - primary video display control

**Purpose**: MediaPlayer is the main WPF UserControl for displaying video in Security Center applications. It handles live streaming, archive and file playback.

The class supports these operations:

- Displays live video streams from Security Center cameras
- Plays back archived video
- Handles video decoding and rendering within WPF applications
- Manages connection states and automatically handles reconnections
- Provides frame-by-frame stepping for detailed video analysis
- Supports video overlays and privacy protection
- Takes snapshots and handles video file playback
- Integrates with Security Center's user permissions and camera access controls

#### VideoSourceFilter - frame-level video processing

**Purpose**: `VideoSourceFilter` gives applications direct access to decoded video frames for programmatic processing without displaying them in a UI control.

The class supports these operations:

- Receives decoded video frames as they arrive from the stream
- Provides access to raw pixel data for image processing algorithms
- Supports both live streams and archived video playback
- Handles video decoding
- Enables custom video analysis, motion detection, and computer vision workflows

#### AudioVideoSourceFilter - combined audio and video processing

**Purpose**: AudioVideoSourceFilter extends VideoSourceFilter to provide simultaneous access to both audio and video streams from the same source.

The class supports these operations:

- Combines video frame processing with audio frame processing
- Provides access to both audio and video data streams
- Enables applications that need to process both media types simultaneously

#### MediaPlayerSynchronizer - multi-camera coordination

**Purpose**: MediaPlayerSynchronizer coordinates playback across multiple MediaPlayer instances to ensure synchronized viewing of multiple camera feeds.

The class supports these operations:

- Synchronizes timeline navigation across multiple video players
- Maintains consistent playback speed and position across all registered players
- Handles frame-by-frame stepping for all players simultaneously
- Manages synchronized seeking and time range selection
- Coordinates live-to-playback transitions across multiple cameras
- Provides unified playback controls for multi-camera scenarios

### PTZ control

These classes coordinate camera PTZ operations.

#### AggregatePtzCoordinatesManager - enterprise PTZ management

**Purpose**: AggregatePtzCoordinatesManager provides centralized PTZ control across multiple cameras.

The class supports these operations:

- Controls PTZ operations across multiple cameras from a single interface
- Tracks PTZ coordinates and provides real-time position feedback
- Supports advanced PTZ features like tours, patterns, and presets

#### PtzCoordinatesManager - simplified PTZ control

**Purpose**: PtzCoordinatesManager provides PTZ control for single camera scenarios with a simpler interface than the aggregate manager.

The class supports these operations:

- Controls PTZ operations for a single camera
- Provides basic coordinate tracking and position feedback
- Handles common PTZ commands like pan, tilt, zoom, and preset operations
- Manages screen-to-camera coordinate conversion

### Stream reading and processing

These classes provide stream data and information about archive sequences.

#### PlaybackStreamReader - low-level stream access

**Purpose**: PlaybackStreamReader provides direct access to raw media stream data for applications that need complete control over media processing.

The class supports these operations:

- Reads raw video and audio data directly from Security Center archives
- Provides frame-level access to encoded media streams
- Supports seeking to specific timestamps with high precision
- Handles multiple stream types (video, audio, metadata)

#### PlaybackSequenceQuerier - archive timeline discovery

**Purpose**: PlaybackSequenceQuerier discovers available video sequences in Security Center archives to build timeline interfaces and determine data availability.

The class supports these operations:

- Queries Security Center archives to find available video sequences
- Provides time range information for recorded video
- Returns sequence information organized by archive source

### Export and media processing

These classes export, encrypt, decrypt, and convert media files.

#### MediaExporter - video export

**Purpose**: MediaExporter handles the export of video data from Security Center.

The class supports these operations:

- Exports video from multiple cameras and time ranges simultaneously
- Exports video in Security Center's native formats (G64/G64X)

#### FileCryptingManager - file security and encryption

**Purpose**: FileCryptingManager provides encryption and decryption capabilities for media files to protect sensitive video content.

The class supports these operations:

- Encrypts media files with password-based protection
- Decrypts previously encrypted media files for authorized access
- Provides progress tracking for long encryption/decryption operations
- Handles media file formats including G64, G64X, and MP4

#### Video conversion

These converters produce MP4 or Advanced Systems Format (ASF) files or add password protection to G64X files.

##### G64ToMp4Converter - standard format conversion

**Purpose**: `G64ToMp4Converter` converts Security Center's native G64 video format to MP4 for sharing and playback compatibility.

The class supports these operations:

- Converts G64 video files to MP4 format
- Supports audio track conversion when present

##### G64ToAsfConverter - Windows Media format conversion

**Purpose**: G64ToAsfConverter converts Security Center's G64 format to ASF (Advanced Systems Format) for Windows-based media applications.

The class supports these operations:

- Converts G64 video files to ASF format

##### G64xPasswordProtectionConverter - secure archive conversion

**Purpose**: G64xPasswordProtectionConverter adds password protection to G64X archive files for secure distribution and storage.

The class supports these operations:

- Adds password protection to existing G64X files
- Creates secure archives that require authentication for access
- Preserves original video quality and metadata

### File analysis and metadata

Use `MediaFile` to read source information, filenames, start and end times, time zones, and watermark status.

#### MediaFile - media file information

**Purpose**: MediaFile provides analysis and metadata extraction from media files without requiring full video playback.

The class supports these operations:

- Analyzes media file structure and metadata
- Identifies multiple streams within container files
- Reports file format, codec information, and technical specifications

### Audio

These classes send audio to Security Center camera audio outputs. `AudioRecorder` uses the computer's microphone; `AudioTransmitter` uses PCM audio buffers supplied by the application.

#### AudioRecorder - microphone capture and transmission

**Purpose**: `AudioRecorder` captures audio from the computer's microphone and sends it to the selected camera's audio output.

The class supports these operations:

- Starts microphone capture and audio transmission
- Stops microphone capture and audio transmission

#### AudioTransmitter - audio communication

**Purpose**: AudioTransmitter sends audio data to Security Center cameras and devices.

The class supports these operations:

- Transmits audio to cameras and intercoms
- Manages audio encoding and streaming to Security Center devices

### Overlay

Use overlays to add text and shapes to camera video.

#### Overlay - dynamic video overlays

**Purpose**: Overlay provides the ability to draw dynamic graphics, text, and visual elements on top of live streams.

The class supports these operations:

- Creates real-time graphical overlays on live video streams
- Supports text, shapes, and images
- Controls duration visibility of overlay elements
