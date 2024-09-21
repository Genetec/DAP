// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sdk;
using Sdk.Entities;
using Sdk.Media.Export;
using File = System.IO.File;

class VideoExport(IEngine engine)
{
    public Task Export(IEnumerable<Camera> cameras, Sdk.Media.DateTimeRange range, string fileName, ExportOption option, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<CameraExportConfig> configs = cameras.Select(camera => new CameraExportConfig(camera.Guid, Enumerable.Repeat(range, 1)));

        return option.Format != VideoExportFormat.G64x
            ? Task.WhenAll(configs.Select(config => Export(Enumerable.Repeat(config, 1), fileName, option, progress, cancellationToken)))
            : Export(configs, fileName, option, progress, cancellationToken);
    }

    public async Task Export(IEnumerable<CameraExportConfig> configs, string fileName, ExportOption option, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        var exporter = new MediaExporter();
        exporter.StatisticsReceived += OnStatisticsReceived;
        try
        {
            exporter.Initialize(engine, Path.GetDirectoryName(fileName));
            exporter.SetExportFileFormat(option.Format == VideoExportFormat.G64 ? MediaExportFileFormat.G64 : MediaExportFileFormat.G64X);

            ExportEndedResult result = await exporter.ExportAsync(configs, option.PlaybackMode, Path.GetFileNameWithoutExtension(fileName), option.IncludeWatermark);

            if (result.ExceptionDetails != null)
                throw result.ExceptionDetails;

            await Task.WhenAll(result.ExportFileList.Select(filePath => Convert(filePath, option.Format, new Progress<int>(value => progress?.Report(value)))));
        }
        finally
        {
            exporter.StatisticsReceived -= OnStatisticsReceived;
            exporter.Dispose();
        }

        void OnStatisticsReceived(object sender, ExportStatisticsEventArgs args) => progress?.Report(args.ExportPercentComplete);

        async Task Convert(string filePath, VideoExportFormat format, IProgress<int> convertProgress)
        {
            switch (format)
            {
                case VideoExportFormat.Asf:
                    using (var converter = new G64ToAsfConverter())
                    {
                        converter.Initialize(
                            engine,
                            filePath,
                            false,
                            false,
                            option.ExportAudio,
                            GetOutputFilePath(), G64ToAsfConverter.GetAsfProfiles().First().Profile);

                        await ConvertAsync(converter);
                    }

                    break;

                case VideoExportFormat.MP4:
                    using (var converter = new G64ToMp4Converter())
                    {
                        converter.Initialize(
                            engine,
                            filePath,
                            option.ExportAudio,
                            GetOutputFilePath());

                        await ConvertAsync(converter);
                    }

                    break;
            }

            string GetOutputFilePath()
            {
                string path = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}.{format.ToString().ToLower()}");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return path;
            }

            Task ConvertAsync(G64ConverterBase converter) => converter.ConvertAsync(convertProgress, cancellationToken).ContinueWith(task => File.Delete(filePath), cancellationToken);
        }
    }
}