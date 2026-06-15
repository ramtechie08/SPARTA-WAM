## Overview

This .NET 8 application automates the SPARTA Walk Away Margin update process. It validates Excel input files, generates SQL scripts, and manages WAM updates in the SPARTA database.

## Features

- **Excel File Processing**: Parses and validates Excel files containing WAM data
- **Input Validation**: Comprehensive validation of all mandatory fields
- **SQL Script Generation**: Automatically generates UPSERT SQL scripts
- **RESTful API**: Easy integration with ServiceNow and other systems
- **Unit Tested**: Comprehensive test coverage for reliability

## Project Structure
- `Controllers/`: API controllers for handling requests
- `Services/`: Business logic for processing Excel files and generating SQL scripts
- `Models/`: Data models representing input and output structures
- `Utilities/`: Helper classes for validation and date handling
- `Tests/`: Unit tests for validating functionality


## API Endpoints

### Generate WAM Update Script

**POST** `/api/wam/generate-script`

Upload an Excel file to generate SQL update scripts.

**Request:**
- Form-data: `file` (required, .xlsx only)

**Response:**
```json
{
  "success": true,
  "scriptCount": 1,
  "sqlScript": "MERGE INTO tb_SPARTA_PriceIncrease AS target USING (VALUES ('7042', '30214696', '2024-02-24', '2024-12-31')) AS source (SalesOrganization, ShortCode, BlockDate, ReleaseDate) ON target.SalesOrganization = source.SalesOrganization AND target.ShortCode = source.ShortCode WHEN MATCHED THEN UPDATE SET BlockDate = source.BlockDate, ReleaseDate = source.ReleaseDate WHEN NOT MATCHED THEN INSERT (SalesOrganization, ShortCode, BlockDate, ReleaseDate) VALUES (source.SalesOrganization, source.ShortCode, source.BlockDate, source.ReleaseDate);",
  "details": [
	{
	  "salesOrganization": "7042",
	  "shortCode": "30214696",
	  "blockDate": "2024-02-24T00:00:00Z",
	  "releaseDate": "2024-12-31T00:00:00Z"
	}
  ]
}
```

## Excel File Format

The Excel file must contain the following columns:

| Column | Required | Format | Example |
|--------|----------|--------|---------|
| Short Code | Yes | Alphanumeric | 30246470 |
| Walk Away Margin | Yes | Numeric (with or without %) | 7 or 7% |
| Sales Org | Yes | Alphanumeric | 7090 |
| Type | Yes | D (Direct) or I (Indirect) | I |

## Validation Rules

- **Short Code**: Mandatory, 1-20 characters
- **Walk Away Margin**: Mandatory, 0-100 (percentage as decimal)
- **Sales Organization**: Mandatory, 1-50 characters
- **Type**: Mandatory, must be 'D' or 'I' (case-insensitive)

## SQL Script Logic

The generated script uses an UPSERT pattern:

### If Record Exists
Updates the existing record:
- `WalkAwayMargin` value
- `Updated_Datetime` to current UTC time

### If Record Does Not Exist
Inserts a new record into `tb_SPARTA_WalkAwayfloorMargin_Mst`

## Database Schema

### tb_SPARTA_WalkAwayfloorMargin_Mst
| Column Name | Data Type | Description |
|-------------|-----------|-------------|
| SalesOrganization | VARCHAR(50) | Sales organization code |
| ShortCode | VARCHAR(20) | Product short code |
| WalkAwayMargin | DECIMAL(5,2) | Walk Away Margin percentage |
| Type | CHAR(1) | 'D' for Direct, 'I' for Indirect |
| Updated_Datetime | DATETIME | Timestamp of last update |


## Installation

1. Clone the repository
2. Open in Visual Studio 2022
3. Restore NuGet packages
4. Build the solution
5. Run the application

## Running Tests

1. Open Test Explorer in Visual Studio

2. Run all tests to validate functionality


## Dependencies

- **ClosedXML**: Excel file parsing
- **FluentValidation**: Data validation framework
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests

## Usage Example

1. Prepare an Excel file with WAM data
2. Call the API endpoint with the file
3. Receive generated SQL script
4. Review and execute in SQL Server Management Studio

## Error Handling

The API returns error responses with detailed messages:

- **400 Bad Request**: Validation errors (e.g., missing fields, invalid formats)
- **500 Internal Server Error**: Unexpected errors during processing
- **Example Error Response**:
```json
{
  "success": false,
  "message": "Validation failed: Walk Away Margin must be a number between 0 and 100"
}
```


## Security Considerations

- SQL parameters should be used in production for SQL injection prevention
- File upload size limits should be configured
- API authentication/authorization should be implemented
- Input sanitization should be enhanced for production use

## Future Enhancements

- Database direct execution capability
- Batch processing support
- Audit logging for all updates
- ServiceNow integration layer
- Performance optimization for large datasets

