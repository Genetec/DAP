// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.Media.Export;

namespace Genetec.Dap.CodeSamples
{
    public static class G64ConverterBaseExtensions
    {
        public static async Task<List<string>> ConvertAsync(this G64ConverterBase converter, IProgress<(int Percent, string Message)> progress, CancellationToken token)
        {
            using CancellationTokenRegistration registration = token.Register(Cancel);
            ConversionFinishedEventArgs args = null;

            converter.ProgressChanged += OnProgressChanged;
            converter.ConversionFinished += OnConversionFinished;
            try
            {
                await converter.ConvertAsync().ConfigureAwait(false);

                if (args.Result == ConversionResult.Cancelled)
                    throw new OperationCanceledException(token);

                if (args.ExceptionDetails != null)
                    throw args.ExceptionDetails;

                if (!string.IsNullOrEmpty(args.ErrorMessage))
                {
                    if (args.Result == ConversionResult.PartiallySuccessful)
                    {
                        progress.Report((100, args.ErrorMessage));
                    }
                    else
                    {
                        throw new Exception(args.ErrorMessage);
                    }
                }

                return args.Filenames;
            }
            finally
            {
                converter.ProgressChanged -= OnProgressChanged;
                converter.ConversionFinished -= OnConversionFinished;
            }

            void OnProgressChanged(object sender, ProgressChangedEventArgs e) => progress?.Report((e.ProgressPercentage, null));

            void OnConversionFinished(object sender, ConversionFinishedEventArgs e) => args = e;

            void Cancel()
            {
                Console.Write("\rCancelling conversion...");
                converter.CancelConversion(true);
            }
        }
    }
}
