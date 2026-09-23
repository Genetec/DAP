# Plugin reports sample

This sample shows how a Security Center plugin can receive report records over a local HTTP endpoint and return them through native Security Center reports and a custom Security Desk report.

The sample demonstrates these Plugin SDK features:

- Plugin database creation and cleanup through `DatabaseManager`
- Loopback-only HTTP ingestion hosted by the plugin role
- Parameterized stored-procedure calls
- Native report handlers, filtering, batching, and cancellation
- A custom report with event, entity, time-range, and message filters
- A Config Tool page for role-specific settings

Every record comes from a local HTTP request; the plugin does not generate sample data.

## Supported reports

| Ingestion endpoint | Report |
| --- | --- |
| `POST /access-control-events` | Cardholder, credential, door, area, elevator, and unit activity |
| `POST /zone-activities` | Zone activity |
| `POST /intrusion-events` | Intrusion area and intrusion unit activity |
| `POST /video-events` | Camera events and video motion |
| `POST /health-events` | Health history |
| `POST /health-statistics` | Health statistics |
| `POST /activity-trails` | Activity trails |
| `POST /audit-trails` | Audit trails |
| `POST /custom-events` | Custom event activities (custom report) |

## Custom event activities

The **Custom event activities** task appears in Security Desk. Its event selector reads the custom-event definitions in Security Center and displays each event's configured source entity type. The entity picker allows the union of those types. Select one custom event or **All custom events**, select at least one source entity, set the native time-range filter, and optionally enter **Message contains**.

Message matching treats SQL wildcard characters such as `%` and `_` literally. Case sensitivity follows the plugin database's collation. An empty message filter matches any message. All filters apply together. Reopen the task after adding or changing custom-event definitions so its event list and available entity types refresh. A saved selection whose event was deleted is marked unavailable instead of silently querying all events.

Send a record to `POST /custom-events` with this JSON shape. Replace the event ID and source GUID with values from your system; the source must exist and have the definition's configured source entity type.

```json
{
  "customEventId": 4,
  "source": "11111111-1111-1111-1111-111111111111",
  "timestamp": "2026-09-21T02:00:00Z",
  "message": "External system reported a condition"
}
```

All four fields are required. `customEventId` must identify an existing definition, `source` must be a nonempty entity GUID, and `timestamp` must be a valid timestamp. `message` can be an empty string. Invalid records return HTTP 400; successful writes return HTTP 204. Ingestion stores a report occurrence; it does not raise a live Security Center event.

The database stores the positive definition ID. The custom report returns its negative value in the Event column so Security Desk resolves the custom-event name. Results include the source entity, UTC timestamp, event, and message. Definitions deleted from Security Center are excluded from this report, while their stored history remains subject to database retention.

For SDK clients, use a `Custom` report query with `CustomReportId` set to `975ab1e5-0f1c-43e7-b446-e4f005944e33`. Populate `QueryEntities` with at least one source entity, set its time range, and serialize [CustomEventFilterData](CustomEventReport.cs) into `FilterData` for the custom-event ID and message. The source validates the custom report ID so it does not answer another plugin's custom report.

Use `FilterData` for the event selector rather than Web SDK's `CustomEvents@ID` syntax: in the tested SDK build, that collection expression does not persist the selection. Web SDK returns the custom report's timestamp text without a timezone suffix; interpret this report's `EventTimestamp` column as UTC.

## Audit report formatting

Audit report output is formatted by Security Center. The selected `auditFormat` controls the generated description and modification category; `modificationType` and `description` are not independent values that always round-trip. For example, `EntityPropertyFormatter` produces a properties-modified description, even when the payload specifies an entity-rename modification. The tested SDK also changes the supplied audit application type to `AccessDatastore`. The plugin database retains the original submitted values. Do not rely on the native audit report to preserve all payload metadata in this SDK build.

## How ingestion works

`EventIngestor` validates each request and selects the domain-specific insert method on `SampleDatabaseManager`. The database manager calls the matching `InsertXxx` stored procedure defined in `Resources/CreationScript.sql`. The native report handler reads the same table and applies the filters from the Security Center report query.

This keeps the learning path visible in one project:

1. JSON request
2. Payload validation
3. Typed stored-procedure parameters
4. Plugin database table
5. Native report query and result rows

## Configure the sample

Use a Windows development system with Security Center, its SDK, SQL Server, the .NET Framework 4.8.1 targeting pack, and a compiler supporting C# 12. Set `GSC_SDK` to the installed SDK directory containing the .NET Framework assemblies. The project targets `net481`; its build has been checked against SDK 5.14. The bundled Plugin SDK certificate and application ID are for a development system.

1. Build the project from an elevated Visual Studio instance. The post-build target registers the client and server modules on that computer. Restart Config Tool to load the client module.
2. Create the plugin role in Config Tool and configure a new sample database.
3. On the role's **Properties** page, keep the default port or select another unused port.
4. Activate the role and confirm that its state shows `http://127.0.0.1:<port>`.

The listener accepts requests only from the role's server. This keeps the sample easy to run without exposing an unauthenticated network endpoint. It is not a production or remote-ingestion endpoint. A production integration must provide authenticated HTTPS through a dedicated service or an authentication design appropriate to its deployment.

If the role reports an access-denied listener error, reserve the loopback URL for the account running the plugin host. Run the following command in an elevated terminal after replacing the account name:

```powershell
netsh http add urlacl url=http://127.0.0.1:8085/ `
  user='DOMAIN\SERVICE-ACCOUNT' listen=yes
```

Create a new sample database using the complete creation script. This unreleased sample does not provide database upgrades for earlier development copies.

## Send a test record

Run this example in PowerShell on the role's server. Replace `DOOR-GUID` with an existing door's GUID. Omitting `timestamp` stores the current UTC time.

```powershell
$payload = @{
  eventType = 'AccessGranted'
  source = 'DOOR-GUID'
  timeZone = 'UTC'
} | ConvertTo-Json -Compress

$response = Invoke-WebRequest `
  -Method Post `
  -Uri 'http://127.0.0.1:8085/access-control-events' `
  -ContentType 'application/json' `
  -Body $payload

$response.StatusCode
```

A successful write returns **HTTP 204 No Content**, with an empty body. In Security Desk, open **Door activity**, select the door used as `source`, include **Access granted**, select a time range containing the request time, and run the report. Posting a report record does not unlock the door or raise a live event.

To compile without changing the local plugin registry, use:

```powershell
dotnet build PluginReportsSample.csproj -c Debug /p:SkipPluginRegistration=true
```

## Payloads and responses

Send one JSON object per POST. Requests require `Content-Length` and must not exceed 4 MiB. Video thumbnails are Base64 strings limited to 1 MiB after decoding. For the complete fields, see [IngestionPayloads.cs](Ingestion/IngestionPayloads.cs).

| Endpoint | Required fields |
| --- | --- |
| `/access-control-events` | Nonempty `source`; either `eventType` or `customEventId` |
| `/zone-activities` | Nonempty `zone`; either `eventType` or `customEventId` |
| `/intrusion-events` | At least one of `intrusionUnit`, `intrusionArea`, or `source`; either `eventType` or `customEventId` |
| `/video-events` | Nonempty `camera` and built-in `eventType` |
| `/health-events` | Nonempty `source` |
| `/health-statistics` | Nonempty `source` |
| `/activity-trails` | A JSON object; populate the activity and entity fields for a meaningful report |
| `/audit-trails` | A JSON object; populate the modification and entity fields for a meaningful report |
| `/custom-events` | Defined positive `customEventId`, matching nonempty `source`, `timestamp`, and `message` |

Use existing Security Center entity GUIDs and event types appropriate to the selected report. Built-in event types use SDK enumeration names; custom event IDs must already exist in Security Center. Other type fields use numeric SDK enumeration values. Supply timestamps in ISO 8601 format with an explicit UTC offset. Event timestamps default to the current UTC time; an omitted health-statistics `lastErrorTimestamp` uses the "never" sentinel. For zone activity, `timeZoneId` must be a Windows time-zone identifier. When `localTimestamp` is present, its offset must match that time zone. When it is omitted, the plugin derives the local timestamp from the event timestamp and `timeZoneId`.

Health statistics keep one current snapshot per `source`, `eventSourceType`, and `observer`. A later successful POST replaces that snapshot. These rows are not filtered by report time range. Health events retain history, but only the last successfully ingested record for the same `healthEventId`, `source`, and `observer` can be active. Ingestion order determines this state, including for backfilled timestamps.

| HTTP status | Meaning |
| --- | --- |
| 204 | The database write completed |
| 400 | The payload failed validation |
| 404 | The endpoint is unknown |
| 405 | The method is not POST |
| 411 | The request has no declared content length |
| 413 | The request exceeds the size limit |
| 500 | An unexpected processing or database error occurred; check the plugin log |
| 503 | The database is unavailable |

Event and trail endpoints append records, so retrying a request after losing its response can create a duplicate.

## Explore the code

- `Ingestion/IngestionPayloads.cs` defines the JSON contract for every endpoint.
- `Server/EventIngestor.cs` validates requests and routes them to insert methods.
- `Server/SampleDatabaseManager.cs` calls the insert stored procedures.
- `Resources/CreationScript.sql` creates the tables and stored procedures.
- `Server/ReportHandlers` translates native report filters into parameterized SQL and maps database rows to SDK result rows.
