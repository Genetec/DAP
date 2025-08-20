using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genetec.Sdk;
using Genetec.Sdk.Entities;

namespace Genetec.Dap.CodeSamples;

class AccessVerifierSample : SampleBase
{
    protected override async Task RunAsync(Engine engine, CancellationToken token)
    {
        // Load the specified entity types into the entity cache
        await LoadEntities(engine, token, EntityType.Credential, EntityType.AccessPoint, EntityType.Schedule, EntityType.AccessRule);

        List<Guid> doorGuids = engine.GetEntities(EntityType.Door).Select(door => door.Guid).ToList();

        DateTime currentTime = DateTime.UtcNow;

        List<(AccessPoint, Credential, AccessResult Result)> accessResults = engine.GetEntities(EntityType.Credential)
            .SelectMany(credential => engine.AccessVerifier.GetAccessResults(doorGuids, currentTime, credential.Guid, Guid.Empty, Guid.Empty)
                .Select(result => (engine.GetEntity<AccessPoint>(result.AccessPoint), engine.GetEntity<Credential>(credential.Guid), result.Result)))
            .ToList();

        using var fileStream = new FileStream($"AccessMatrix_{currentTime.ToLocalTime():yyyyMMdd_HHmmss}.csv", FileMode.Create, FileAccess.Write);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

        GenerateAccessMatrixCsv(engine, accessResults, streamWriter);

        Console.WriteLine($"File created: {fileStream.Name}");
    }


    static void GenerateAccessMatrixCsv(IEngine engine, ICollection<(AccessPoint AccessPoint, Credential Credential, AccessResult Result)> accessResults, TextWriter textWriter)
    {
        var accessPoints = accessResults.Select(tuple => tuple.AccessPoint).Distinct().ToList();

        var resultLookup = accessResults.ToDictionary(tuple => (tuple.AccessPoint, tuple.Credential), tuple => tuple.Result);

        textWriter.Write("Credential");
        foreach (var accessPoint in accessPoints)
        {
            textWriter.Write($",{engine.GetEntity(accessPoint.AccessPointGroup).Name} ({accessPoint.Name})");
        }
        textWriter.WriteLine();

        foreach (var credential in accessResults.Select(tuple => tuple.Credential).Distinct())
        {
            textWriter.Write(credential.Name);

            foreach (var accessPoint in accessPoints)
            {
                textWriter.Write(resultLookup.TryGetValue((accessPoint, credential), out var result) ? $",{result}" : ",");
            }

            textWriter.WriteLine();
        }
    }
}