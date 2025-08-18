namespace Genetec.Dap.CodeSamples;

using System;
using Sdk;

record ActivityTrail
{
    public Guid InitiatorEntityId { get; set; }
    public string InitiatorEntityName { get; set; }
    public EntityType InitiatorEntityType { get; set; }
    public string InitiatorEntityTypeName { get; set; }
    public string InitiatorEntityVersion { get; set; }
    public string Description { get; set; }
    public ActivityType ActivityType { get; set; }
    public string ActivityTypeName { get; set; }
    public Guid ImpactedEntityId { get; set; }
    public string ImpactedEntityName { get; set; }
    public EntityType ImpactedEntityType { get; set; }
    public string ImpactedEntityTypeName { get; set; }
    public string InitiatorMachineName { get; set; }
    public ApplicationType InitiatorApplicationType { get; set; }
    public string InitiatorApplicationName { get; set; }
    public string InitiatorApplicationVersion { get; set; }
    public DateTime EventTimestamp { get; set; }
}