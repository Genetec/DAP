// Copyright (C) 2023 by Genetec, Inc. All rights reserved.
// May be used only in accordance with a valid Source Code License Agreement.

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
        public static async Task<List<string>> ConvertAsync(this G64ConverterBase converter, IProgress<int> progress, CancellationToken token)
        {
            token.Register(() => converter.CancelConversion(true));

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
                    throw new Exception(args.ErrorMessage);

                return args.Filenames;
            }
            finally
            {
                converter.ProgressChanged -= OnProgressChanged;
                converter.ConversionFinished -= OnConversionFinished;
            }

            void OnProgressChanged(object sender, ProgressChangedEventArgs e) => progress?.Report(e.ProgressPercentage);

            void OnConversionFinished(object sender, ConversionFinishedEventArgs e) => args = e;
        }
    }
}