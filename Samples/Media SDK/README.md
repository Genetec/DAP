# Media SDK Developer Guide

The Media SDK extends the Platform SDK with comprehensive video capabilities for Security Center. It provides real-time video streaming, playback control, audio transmission, PTZ camera operations, and video export functionality. This guide explains the core concepts, classes, and architectural patterns you need to understand when working with media in Security Center integrations.

## Prerequisites

- **.NET Framework 4.8.1**: The Media SDK only supports the .NET Framework; it does not support .NET 8 yet.  
- **Security Center SDK**: Installed with `GSC_SDK` environment variable configured
- **Visual Studio 2022**: Version 17.6 or later for development
- **Security Center**: Client applications (Security Desk and Config Tool) installed
- **Valid Security Center License**: All samples include the development SDK certificate

## Media SDK Architecture

### Foundation on Platform SDK

The Media SDK builds directly on Platform SDK foundations. Many classes require an Engine class connected to Security Center.

### Media-Specific Extensions

The Media SDK adds specialized capabilities:
- **Real-time Streaming**: Live video feeds from cameras
- **Playback Control**: Archived video playback with seeking and speed control
- **Media Processing**: Video overlays and frame-level processing
- **PTZ Camera Control**: PTZ controls for pan, tilt, and zoom operations
- **Export Operations**: Video export and format conversion

## Core Media SDK Classes

### Primary Video and Media Classes

#### MediaPlayer - Primary Video Display Control

**Purpose**: MediaPlayer is the main WPF UserControl for displaying video in Security Center applications. It handles live streaming, archive and file playback.

**What it does**:
- Displays live video streams from Security Center cameras
- Plays back archived video
- Handles video decoding and rendering within WPF applications
- Manages connection states and automatically handles reconnections
- Provides frame-by-frame stepping for detailed video analysis
- Supports video overlays, and privacy protection
- Takes snapshots and handles video file playback
- Integrates with Security Center's user permissions and camera access controls

#### VideoSourceFilter - Frame-Level Video Processing

**Purpose**: VideoSourceFilter provides direct access to decoded video frames without displaying them in a UI control. It's designed for applications that need to process video data programmatically.

**What it does**:
- Receives decoded video frames as they arrive from the stream
- Provides access to raw pixel data for image processing algorithms
- Supports both live streams and archived video playback
- Handles video decoding
- Enables custom video analysis, motion detection, and computer vision workflows

#### AudioVideoSourceFilter - Combined Audio and Video Processing

**Purpose**: AudioVideoSourceFilter extends VideoSourceFilter to provide simultaneous access to both audio and video streams from the same source.

**What it does**:
- Combines video frame processing with audio frame processing
- Provides access to both audio and video data streams
- Enables applications that need to process both media types simultaneously

#### MediaPlayerSynchronizer - Multi-Camera Coordination

**Purpose**: MediaPlayerSynchronizer coordinates playback across multiple MediaPlayer instances to ensure synchronized viewing of multiple camera feeds.

**What it does**:
- Synchronizes timeline navigation across multiple video players
- Maintains consistent playback speed and position across all registered players
- Handles frame-by-frame stepping for all players simultaneously
- Manages synchronized seeking and time range selection
- Coordinates live-to-playback transitions across multiple cameras
- Provides unified playback controls for multi-camera scenarios

### PTZ Control System

#### AggregatePtzCoordinatesManager - Enterprise PTZ Management

**Purpose**: AggregatePtzCoordinatesManager provides centralized PTZ control across multiple cameras.

**What it does**:
- Controls PTZ operations across multiple cameras from a single interface
- Tracks PTZ coordinates and provides real-time position feedback
- Supports advanced PTZ features like tours, patterns, and presets

#### PtzCoordinatesManager - Simplified PTZ Control

**Purpose**: PtzCoordinatesManager provides PTZ control for single camera scenarios with a simpler interface than the aggregate manager.

**What it does**:
- Controls PTZ operations for a single camera
- Provides basic coordinate tracking and position feedback
- Handles common PTZ commands like pan, tilt, zoom, and preset operations
- Manages screen-to-camera coordinate conversion

### Stream Reading and Processing

#### PlaybackStreamReader - Low-Level Stream Access

**Purpose**: PlaybackStreamReader provides direct access to raw media stream data for applications that need complete control over media processing.

**What it does**:
- Reads raw video and audio data directly from Security Center archives
- Provides frame-level access to encoded media streams
- Supports seeking to specific timestamps with high precision
- Handles multiple stream types (video, audio, metadata)

#### PlaybackSequenceQuerier - Archive Timeline Discovery

**Purpose**: PlaybackSequenceQuerier discovers available video sequences in Security Center archives to build timeline interfaces and determine data availability.

**What it does**:
- Queries Security Center archives to find available video sequences
- Provides time range information for recorded video
- Returns sequence information organized by archive source

### Export and Media Processing

#### MediaExporter - Video Export

**Purpose**: MediaExporter handles the export of video data from Security Center.

**What it does**:
- Exports video from multiple cameras and time ranges simultaneously
- Exports video in Security Center's native formats (G64/G64X)

#### FileCryptingManager - File Security and Encryption

**Purpose**: FileCryptingManager provides encryption and decryption capabilities for media files to protect sensitive video content.

**What it does**:
- Encrypts media files with password-based protection
- Decrypts previously encrypted media files for authorized access
- Provides progress tracking for long encryption/decryption operations
- Handles various media file formats including G64, G64X, and MP4

#### Video Conversion System

##### G64ToMp4Converter - Standard Format Conversion

**Purpose**: G64ToMp4Converter converts Security Center's native G64 video format to standard MP4 format for broader compatibility and sharing.

**What it does**:
- Converts G64 video files to MP4 format
- Supports audio track conversion when present

##### G64ToAsfConverter - Windows Media Format Conversion

**Purpose**: G64ToAsfConverter converts Security Center's G64 format to ASF (Advanced Systems Format) for Windows-based media applications.

**What it does**:
- Converts G64 video files to ASF format

##### G64xPasswordProtectionConverter - Secure Archive Conversion

**Purpose**: G64xPasswordProtectionConverter adds password protection to G64X archive files for secure distribution and storage.

**What it does**:
- Adds password protection to existing G64X files
- Creates secure archives that require authentication for access
- Preserves original video quality and metadata

### File Analysis and Metadata

#### MediaFile - Media File Information and Analysis

**Purpose**: MediaFile provides comprehensive analysis and metadata extraction from media files without requiring full video playback.

**What it does**:
- Analyzes media file structure and metadata
- Identifies multiple streams within container files
- Reports file format, codec information, and technical specifications

### Audio System

#### AudioRecorder - Audio Capture

**Purpose**: AudioRecorder captures audio from Security Center camera sources for recording, processing, or real-time monitoring.

**What it does**:
- Records audio streams from cameras that support audio capture
- Provides access to decoded audio data for processing

#### AudioTransmitter - Audio Communication

**Purpose**: AudioTransmitter sends audio data to Security Center cameras and devices.

**What it does**:
- Transmits audio to cameras and intercoms
- Manages audio encoding and streaming to Security Center devices

### Overlay

#### Overlay - Dynamic Video Overlays

**Purpose**: Overlay provides the ability to draw dynamic graphics, text, and visual elements on top of live streams.

**What it does**:
- Creates real-time graphical overlays on live video streams
- Supports text, shapes, and images
- Controls duration visibility of overlay elements
