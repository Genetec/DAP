using System;
using Genetec.Sdk;

namespace Genetec.Dap.CodeSamples;

record AuditTrail
{
    public int Id { get; set; }
    public Guid EntityGuid { get; set; }
    public DateTime ModificationTimestamp { get; set; }
    public Guid ModifiedBy { get; set; }
    public Guid SourceApplicationGuid { get; set; }
    public string Name { get; set; }
    public string ModifiedByAsString { get; set; }
    public string SourceApplicationAsString { get; set; }
    public string Machine { get; set; }
    public ApplicationType SourceApplicationType { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public Guid CustomFieldId { get; set; }
    public string CustomFieldName { get; set; }
    public CustomFieldValueType CustomFieldValueType { get; set; }
    public AuditTrailModificationType ModificationType { get; set; }
    public EntityType EntityType { get; set; }
    public string Description { get; set; }
    public string Value { get; set; }
}