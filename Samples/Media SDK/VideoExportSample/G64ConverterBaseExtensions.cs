// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk.Media.Export;

namespace Genetec.Dap.CodeSamples;

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