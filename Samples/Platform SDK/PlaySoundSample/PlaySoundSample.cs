// Copyright 2025 Genetec Inc.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

namespace Genetec.Dap.CodeSamples;

public class PlaySoundSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load files into the entity cache
        await LoadEntities(engine, token, EntityType.File);

        // Get audio files from the entity cache
        IEnumerable<File> audioFiles = engine.GetEntities(EntityType.File).OfType<File>().Where(file => file.FileType == FileType.Audio);

        foreach (File file in audioFiles)
        {
            Console.WriteLine($"Playing sound file: {file.Name}");

            engine.ActionManager.PlaySound(UserGroup.AdministratorsUserGroupGuid, file.Guid);

            await Task.Delay(2000, token); // Wait 2 seconds between each sound
        }
    }

}