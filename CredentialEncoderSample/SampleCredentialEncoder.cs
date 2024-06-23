// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Windows.Controls;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Markup;
    using Sdk.Workspace.Components.CredentialEncoder;

    public class SampleCredentialEncoder : CredentialEncoder
    {
        public override string Name => "Sample Credential Encoder";
        public override Guid UniqueId { get; } = new Guid("9E6F7361-D493-463A-9730-319434DF93A4"); // Replace with your own GUID
        public override void Encode(CredentialEncoderData data)
        {
            string rawData = data.Credential.RawData;
            FixedDocument document = data.Document;
            if (document != null)
            {
                var fixedPage = new FixedPage();
                var textBlock = new TextBlock
                {
                    Text = rawData,
                    Margin = new Thickness(20),
                    FontSize = 14
                };
                fixedPage.Children.Add(textBlock);

                
                var pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fixedPage);
                document.Pages.Add(pageContent);
            }
        }

        public override int Priority => 0;
    }
}