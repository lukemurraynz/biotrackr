# Data Model: Food API

**Feature**: Food API with Full Test Coverage and Infrastructure  
**Date**: 2025-11-11  
**Status**: Complete

## Overview

This document defines the data models for the Food API, including database entities, API request/response models, and validation rules. All models follow the established patterns from Weight, Activity, and Sleep APIs for consistency.

---

## Database Entities

### FoodLog (Cosmos DB Document)

Represents a daily food and nutrition log stored in Cosmos DB.

**Container**: `HealthData`  
**Partition Key**: `/documentType` (value: "FoodLog")  
**Document Type**: Persisted entity

```csharp
public class FoodLog
{
    [JsonProperty("id")]
    public string Id { get; set; }  // GUID string

    [JsonProperty("documentType")]
    public string DocumentType { get; set; }  // Always "FoodLog"

    [JsonProperty("userId")]
    public string UserId { get; set; }  // User identifier

    [JsonProperty("date")]
    public string Date { get; set; }  // YYYY-MM-DD format

    [JsonProperty("summary")]
    public FoodLogSummary Summary { get; set; }  // Nutrition totals

    [JsonProperty("goals")]
    public FoodLogGoals Goals { get; set; }  // Nutrition targets

    [JsonProperty("ttl")]
    public int? Ttl { get; set; }  // Time-to-live in seconds (optional)
}
```

**Field Descriptions**:
- `id`: Unique identifier (GUID), primary key
- `documentType`: Discriminator for document type ("FoodLog"), partition key
- `userId`: Identifies the user who owns this food log
- `date`: Date of the food log in YYYY-MM-DD format (e.g., "2025-11-11")
- `summary`: Nested object containing nutrition totals for the day
- `goals`: Nested object containing nutrition targets for the day
- `ttl`: Optional time-to-live for automatic document expiration

**Validation Rules**:
- `Id` must be a valid GUID string
- `DocumentType` must always be "FoodLog"
- `Date` must be valid YYYY-MM-DD format
- `Summary` and `Goals` must not be null
- `Ttl` is optional, if present must be > 0

**Example Document**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "documentType": "FoodLog",
  "userId": "user123",
  "date": "2025-11-11",
  "summary": {
    "calories": 2500,
    "caloriesIn": 2200,
    "water": 2000
  },
  "goals": {
    "calories": 2400
  },
  "ttl": 31536000
}
```

---

### FoodLogSummary (Nested Object)

Represents daily nutrition totals within a FoodLog.

```csharp
public class FoodLogSummary
{
    [JsonProperty("calories")]
    public int Calories { get; set; }  // Total calories burned

    [JsonProperty("caloriesIn")]
    public int CaloriesIn { get; set; }  // Calories consumed

    [JsonProperty("water")]
    public int Water { get; set; }  // Water intake in milliliters
}
```

**Field Descriptions**:
- `calories`: Total calories burned for the day (from activity + BMR)
- `caloriesIn`: Total calories consumed from food and beverages
- `water`: Total water intake in milliliters

**Validation Rules**:
- All fields must be non-negative integers
- `calories` typically ranges from 1500-4000
- `caloriesIn` typically ranges from 0-5000
- `water` typically ranges from 0-5000 (in ml)

---

### FoodLogGoals (Nested Object)

Represents daily nutrition targets within a FoodLog.

```csharp
public class FoodLogGoals
{
    [JsonProperty("calories")]
    public int Calories { get; set; }  // Calorie goal
}
```

**Field Descriptions**:
- `calories`: Daily calorie intake goal set by user or calculated from profile

**Validation Rules**:
- `calories` must be a non-negative integer
- Typically ranges from 1200-4000

**Future Extensions** (not in initial scope):
- `protein`, `carbs`, `fat`: Macronutrient goals (grams)
- `water`: Hydration goal (milliliters)

---

## API Response Models

### FoodLogResponse (Single Food Log)

API response model for a single food log, returned by `GET /{date}` endpoint.

```csharp
public class FoodLogResponse
{
    public string Id { get; set; }
    public string Date { get; set; }  // YYYY-MM-DD
    public FoodLogSummary Summary { get; set; }
    public FoodLogGoals Goals { get; set; }
}
```

**Field Descriptions**:
- Same as `FoodLog` but excludes internal fields (`documentType`, `userId`, `ttl`)
- Simplified for client consumption

**Example Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "date": "2025-11-11",
  "summary": {
    "calories": 2500,
    "caloriesIn": 2200,
    "water": 2000
  },
  "goals": {
    "calories": 2400
  }
}
```

---

### PaginatedFoodLogsResponse (Collection)

API response model for paginated collections of food logs, returned by `GET /` and `GET /range/{startDate}/{endDate}` endpoints.

```csharp
public class PaginatedFoodLogsResponse
{
    public List<FoodLogResponse> Data { get; set; }  // Food log items
    public int TotalCount { get; set; }              // Total matching items
    public int PageNumber { get; set; }              // Current page (1-indexed)
    public int PageSize { get; set; }                // Items per page
    public int TotalPages { get; set; }              // Calculated total pages
}
```

**Field Descriptions**:
- `Data`: Array of food log items for current page
- `TotalCount`: Total number of matching food logs (across all pages)
- `PageNumber`: Current page number (starting at 1)
- `PageSize`: Number of items per page (max 100)
- `TotalPages`: Calculated as `Math.Ceiling(TotalCount / (double)PageSize)`

**Calculation**:
```csharp
TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
```

**Example Response**:
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "date": "2025-11-11",
      "summary": { "calories": 2500, "caloriesIn": 2200, "water": 2000 },
      "goals": { "calories": 2400 }
    },
    {
      "id": "650e8400-e29b-41d4-a716-446655440001",
      "date": "2025-11-10",
      "summary": { "calories": 2300, "caloriesIn": 2100, "water": 1800 },
      "goals": { "calories": 2400 }
    }
  ],
  "totalCount": 365,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 19
}
```

---

## API Request Models

### Pagination Parameters

Query string parameters for paginated endpoints (`GET /`, `GET /range/{startDate}/{endDate}`).

```csharp
public class PaginationParameters
{
    public int PageNumber { get; set; } = 1;   // Default: 1, Min: 1
    public int PageSize { get; set; } = 20;    // Default: 20, Min: 1, Max: 100
}
```

**Validation Rules**:
- `PageNumber` must be >= 1
- `PageSize` must be between 1 and 100 (inclusive)
- If `PageSize` > 100, cap at 100
- If `PageNumber` < 1, return 400 Bad Request

**Example Usage**:
```
GET /?pageNumber=2&pageSize=50
GET /range/2025-11-01/2025-11-30?pageNumber=1&pageSize=25
```

---

### Date Parameters

Path parameters for date-based endpoints.

**Single Date** (`GET /{date}`):
```
{date}: YYYY-MM-DD format (e.g., "2025-11-11")
```

**Date Range** (`GET /range/{startDate}/{endDate}`):
```
{startDate}: YYYY-MM-DD format (e.g., "2025-11-01")
{endDate}: YYYY-MM-DD format (e.g., "2025-11-30")
```

**Validation Rules**:
- Dates must match `YYYY-MM-DD` format (ISO 8601)
- Month must be 01-12
- Day must be valid for the month (01-31, accounting for month length)
- Year must be 1900-2100 (reasonable bounds)
- For date ranges: `startDate` must be <= `endDate`

**Invalid Examples**:
- `2025-13-01` - Invalid month (13)
- `2025-11-32` - Invalid day (32)
- `2025/11/11` - Wrong separator (should be hyphen)
- `11-11-2025` - Wrong order (should be YYYY-MM-DD)

---

## Error Response Models

### ErrorResponse

Standard error response for 400/500 errors.

```csharp
public class ErrorResponse
{
    public string Error { get; set; }       // Human-readable error message
    public int StatusCode { get; set; }     // HTTP status code
}
```

**Example Responses**:

**400 Bad Request** (Invalid Date):
```json
{
  "error": "Invalid date format. Expected YYYY-MM-DD.",
  "statusCode": 400
}
```

**400 Bad Request** (Invalid Date Range):
```json
{
  "error": "Invalid date range. Start date must be before or equal to end date.",
  "statusCode": 400
}
```

**400 Bad Request** (Invalid Pagination):
```json
{
  "error": "Invalid pageSize. Must be between 1 and 100.",
  "statusCode": 400
}
```

**500 Internal Server Error**:
```json
{
  "error": "An unexpected error occurred. Please try again later.",
  "statusCode": 500
}
```

**503 Service Unavailable** (Cosmos DB down):
```json
{
  "error": "Database is temporarily unavailable. Please try again later.",
  "statusCode": 503
}
```

---

## Configuration Models

### CosmosDbSettings

Configuration model for Cosmos DB connection settings.

```csharp
public class CosmosDbSettings
{
    public string Endpoint { get; set; }          // Cosmos DB endpoint URL
    public string AccountKey { get; set; }        // Account key (optional, for local)
    public string DatabaseName { get; set; }      // Database name ("Biotrackr")
    public string ContainerName { get; set; }     // Container name ("HealthData")
    public string PartitionKeyPath { get; set; }  // Partition key ("/documentType")
}
```

**Configuration Source**:
- Production: Azure App Configuration (`Biotrackr:CosmosDb:*`)
- Local: `appsettings.Development.json`

**Example (appsettings.Development.json)**:
```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "AccountKey": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "Biotrackr",
    "ContainerName": "HealthData",
    "PartitionKeyPath": "/documentType"
  }
}
```

---

## Relationships

### Entity Relationships

```
FoodLog (1) --contains--> (1) FoodLogSummary
FoodLog (1) --contains--> (1) FoodLogGoals
FoodLog (many) --belongs to--> (1) User (via userId)
```

**Notes**:
- `FoodLogSummary` and `FoodLogGoals` are always embedded within `FoodLog`
- No separate collections or tables
- One-to-one relationship between FoodLog and nested objects
- Many-to-one relationship between FoodLog and User (not enforced at DB level)

---

## State Transitions

Food logs are **immutable** after creation by the Food Service. The API provides **read-only** access.

**Lifecycle**:
1. **Created**: Food Service fetches data from Fitbit and creates FoodLog document
2. **Stored**: Document persisted in Cosmos DB with TTL
3. **Queried**: Food API reads and returns data to clients
4. **Expired**: Document automatically deleted by Cosmos DB after TTL expires

**No State Transitions**: Food logs do not have different states (no draft/published/archived). They exist or they don't.

---

## Validation Summary

### Database Write Validation (Food Service - out of scope)
- Performed by Food Service when creating documents
- Not relevant for Food API (read-only)

### API Read Validation (Food API - in scope)

**Date Validation**:
- Format: YYYY-MM-DD (regex: `^\d{4}-\d{2}-\d{2}$`)
- Valid date: Month 01-12, day valid for month
- Date range: startDate <= endDate

**Pagination Validation**:
- pageNumber >= 1
- pageSize between 1 and 100

**Response Validation**:
- All returned documents must have valid structure
- Missing fields should be handled gracefully (e.g., nulls)

---

## Performance Considerations

### Indexing
- **Default Indexing**: Cosmos DB indexes all fields by default
- **Composite Index** (recommended for range queries):
  ```json
  {
    "compositeIndexes": [
      [
        { "path": "/documentType", "order": "ascending" },
        { "path": "/date", "order": "descending" }
      ]
    ]
  }
  ```
- Improves performance for `ORDER BY c.date DESC WHERE c.documentType = 'FoodLog'`

### Query Patterns
- **Always include partition key**: `WHERE c.documentType = 'FoodLog'`
- **Use date range queries**: `WHERE c.date >= @startDate AND c.date <= @endDate`
- **Leverage ORDER BY**: `ORDER BY c.date DESC` for chronological sorting

### Pagination Performance
- **OFFSET LIMIT**: Uses continuation token internally
- **Avoid large offsets**: Performance degrades with very high page numbers
- **Acceptable for expected volume**: ~365 documents/year, pagination efficient

---

## Future Enhancements (Out of Scope)

### Potential Data Model Extensions
1. **Meal Details**: Expand to include individual meal entries (breakfast, lunch, dinner)
2. **Macronutrients**: Add protein, carbs, fat tracking
3. **Micronutrients**: Vitamins, minerals
4. **Food Items**: Detailed list of foods consumed
5. **Hydration Goals**: Separate water goal tracking

### Potential API Extensions
1. **POST /foodlogs**: Create/update food logs (currently read-only)
2. **DELETE /foodlogs/{id}**: Delete food logs
3. **PATCH /foodlogs/{id}**: Partial updates
4. **Filtering**: Query by calorie range, date range, etc.
5. **Aggregations**: Weekly/monthly summaries

**Decision**: Start with read-only API to match other APIs. Write operations can be added in future iterations if needed.

---

## References

### Existing Data Models
- `src/Biotrackr.Weight.Api/Biotrackr.Weight.Api/Models/` - Reference for model structure
- `src/Biotrackr.Activity.Api/Biotrackr.Activity.Api/Models/` - Reference for pagination
- `src/Biotrackr.Food.Svc/` - Source of FoodLog documents (write side)

### Cosmos DB Documentation
- [Cosmos DB Partitioning](https://learn.microsoft.com/en-us/azure/cosmos-db/partitioning-overview)
- [Cosmos DB Indexing](https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy)
- [Cosmos DB TTL](https://learn.microsoft.com/en-us/azure/cosmos-db/time-to-live)

---

**Data Model Complete**: All entities, validation rules, and relationships defined. Ready for contract generation (Phase 1 continued).
