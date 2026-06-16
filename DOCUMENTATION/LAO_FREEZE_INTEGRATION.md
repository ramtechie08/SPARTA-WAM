# LAO Freeze/Block PA & Product Freeze Integration Guide

## Overview

This guide describes the integration of LAO Freeze/Block PA and LAO Product Freeze processes into the SPARTA WAM solution.

## Processes Supported

### 1. LAO Freeze/Block PA (Contracts)

**Purpose**: Freeze or block specific contracts for a defined period.

**Input**: Excel file with Contract Numbers, Block Dates, and Release Dates

**Output**: SQL INSERT scripts that freeze contracts in `tb_SPARTA_PriceIncrease` table

**Endpoint**: 
- Generate: `POST /api/laofreeze/freeze-pa/generate-script`
- Execute: `POST /api/laofreeze/freeze-pa/upload-and-execute`

### 2. LAO Product Freeze (Products)

**Purpose**: Freeze products for a specific period.

**Input**: Excel file with Sales Org, Block Dates, Release Dates, and Short Codes

**Output**: SQL INSERT scripts that freeze products in `tb_SPARTA_PriceIncrease` table

**Endpoint**:
- Generate: `POST /api/laofreeze/freeze-product/generate-script`
- Execute: `POST /api/laofreeze/freeze-product/upload-and-execute`

## Excel File Format

### LAO Freeze/Block PA

Required columns:
Contract Number	Block Date	Release Date
27198	1/1/2024	12/31/2024
54321	2/1/2024	6/30/2024

### LAO Product Freeze

Required columns:
Sales Org	Block Date	Release Date	Short Code
1000	1/1/2024	12/31/2024	ABC123
2000	2/1/2024	6/30/2024	XYZ789

## Data Processing Steps

### Step 1: Input Validation
- Check mandatory fields
- Validate date formats
- Remove duplicates (Contract Numbers for PA)
- Validate date logic (BlockDate < ReleaseDate)

### Step 2: Date Format Conversion
- Dates are parsed from Excel
- Converted to ISO format for SQL statements
- Handled as datetime values in database

### Step 3: Template Population
- Data copied from validated Excel rows
- Organized into structured format
- Ready for SQL script generation

### Step 4: SQL Script Generation
- Contract Number prefixed with "00" → "00" + ContractNumber
- Short Code prefixed with "0000000000" → "0000000000" + ShortCode
- INSERT statements created using formula template
- Dates formatted as M/d/yyyy for SQL CAST

### Step 5: Script Execution
- Scripts executed in SQL Server
- Records inserted into `tb_SPARTA_PriceIncrease` table
- Automatic block/freeze applied for duration

## API Endpoints

POST /api/laofreeze/freeze-product/generate-script Content-Type: multipart/form-data
	Response (200): { "success": true, "freezeType": "Product", "scriptCount": 1, "sqlScript": "insert into tb_SPARTA_PriceIncrease...", "details": [ { "salesOrganization": "7042", "shortCode": "30214696", "blockDate": "2024-02-24T00:00:00Z", "releaseDate": "2024-12-31T00:00:00Z" } ] }

### LAO Freeze/Block PA Endpoints

#### Generate Script


#### Execute (Upload and Execute)

POST /api/laofreeze/freeze-product/upload-and-execute Content-Type: multipart/form-data
Body: [file upload]

# Response
json
{
  "success": true,
  "freezeType": "Product",
  "message": "LAO Product Freeze completed successfully",
  "scriptCount": 1,
  "rowsAffected": 1,
  "details": [...]

}

### Query Endpoints

#### Get All Freeze Records

GET /api/laofreeze/records
Response: { "success": true, "recordCount": 5, "records": [...] }

#### Get by Contract Number

GET /api/laofreeze/records?contractNumber=27198

#### Get by Sales Organization

GET /api/laofreeze/records?salesOrganization=7042

#### Get Active Frozen Items

GET /api/laofreeze/active

# Response
    json
{
  "success": true,
  "frozenItems": [
    {
      "contractNumber": "27198",
      "salesOrganization": "1000",
      "shortCode": "ABC123",
      "blockDate": "2024-01-01",
      "releaseDate": "2024-12-31"
    }
  ]
}

## Database Schema

### tb_SPARTA_PriceIncrease Table


| Column Name       | Data Type    | Description                          |
|-------------------|--------------|--------------------------------------|
| id                | INT          | Primary key, auto-increment          |
| contractNumber    | VARCHAR(20)  | Contract number (for PA freeze)      |
| salesOrganization | VARCHAR(20)  | Sales organization (for product freeze) |
| shortCode         | VARCHAR(20)  | Short code (for product freeze)      |
| blockDate         | DATETIME     | Date when freeze/block starts        |
| releaseDate       | DATETIME     | Date when freeze/block ends          |

## Validation Rules

### LAO Freeze/Block PA

- **Contract Number**: Required, 1-50 characters, digits only
- **Block Date**: Required, must be before Release Date
- **Release Date**: Required, valid datetime

### LAO Product Freeze

- **Sales Organization**: Required, 1-50 characters
- **Block Date**: Required, must be before Release Date
- **Release Date**: Required, valid datetime
- **Short Code**: Required, 1-50 characters

## Error Handling

### Common Errors

**Missing mandatory fields**:

- Response: { "success": false, "message": "Validation failed: Contract Number is required" }

**Invalid date formats**:
- Response: { "success": false, "message": "Validation failed: Block Date must be a valid date" }
**Date logic errors**:
- Response: { "success": false, "message": "Validation failed: Block Date must be before Release Date" }
**Duplicate Contract Numbers (for PA)**:
- Response: { "success": false, "message": "Validation failed: Duplicate Contract Number 27198 found" }
**Database errors during execution**:
- Response: { "success": false, "message": "Database error: Unable to execute SQL script" }
**File processing errors**:
- Response: { "success": false, "message": "File processing error: Unable to read Excel file" }
**General exceptions**:
- Response: { "success": false, "message": "An unexpected error occurred: [error details]" }

- Note: All error responses should include appropriate HTTP status codes (e.g., 400 for validation errors, 500 for server errors) and detailed messages to assist with troubleshooting.

## Processing Examples

### Example 1: LAO Freeze/Block PA

**Input Excel**:

| Contract Number | Block Date  | Release Date |
|-----------------|-------------|--------------|
| 27198           | 1/1/2024    | 12/31/2024   |
| 54321           | 2/1/2024    | 6/30/2024    |
  
LAO Freeze/Block PA Template
ContractNo	BlockDate	ReleaseDate	Region
27198	1/1/2024	12/31/2024	APAC
54321	2/1/2024	6/30/2024	EMEA
11111	3/1/2024	9/30/2024	NA

LAO Product Freeze Template
SalesOrg	BlockDate	ReleaseDate	Short_Code	Region
1000	1/1/2024	12/31/2024	ABC123	APAC
2000	2/1/2024	6/30/2024	XYZ789	EMEA
3000	3/1/2024	9/30/2024	DEF456	NA

# Response
json
{
  "success": true,
  "freezeType": "PA",
  "scriptCount": 2,
  "sqlScript": "insert into tb_SPARTA_PriceIncrease (contractNumber, blockDate, releaseDate) values ('0027198', '2024-01-01', '2024-12-31'); insert into tb_SPARTA_PriceIncrease (contractNumber, blockDate, releaseDate) values ('0054321', '2024-02-01', '2024-06-30');",
  "details": [
    {
      "contractNumber": "27198",
      "blockDate": "2024-01-01T00:00:00Z",
      "releaseDate": "2024-12-31T00:00:00Z"
    },
    {
      "contractNumber": "54321",
      "blockDate": "2024-02-01T00:00:00Z",
      "releaseDate": "2024-06-30T00:00:00Z"
    }
  ]
}

### Example 2: LAO Product Freeze
**Input Excel**:
| Sales Org | Block Date  | Release Date | Short Code |
|-----------|-------------|--------------|------------|
| 1000      | 1/1/2024    | 12/31/2024   | ABC123     |
# Response
json
{
  "success": true,
  "freezeType": "Product",
  "scriptCount": 1,
  "sqlScript": "insert into tb_SPARTA_PriceIncrease (salesOrganization, shortCode, blockDate, releaseDate) values ('1000', '0000000000ABC123', '2024-01-01', '2024-12-31');",
  "details": [
    {
      "salesOrganization": "1000",
      "shortCode": "ABC123",
      "blockDate": "2024-01-01T00:00:00Z",
      "releaseDate": "2024-12-31T00:00:00Z"
    }
  ]
}

## Testing

### Manual Testing Steps

1. **Prepare Excel file** with required columns
2. **Upload to endpoint**: POST /api/laofreeze/freeze-pa/generate-script
3. **Verify SQL scripts** generated correctly
4. **Review details** for correctness
5. **Execute scripts**: POST /api/laofreeze/freeze-pa/upload-and-execute
6. **Verify in database**: SELECT * FROM tb_SPARTA_PriceIncrease

### Automated Tests

---

**LAO Freeze Integration v1.0** | June 2026 | ✅ Production Ready


Excel File Upload
    ↓
Parse & Extract Rows
    ↓
Validate Data
    ↓
Generate SQL Scripts
    ↓
Execute in Database
    ↓
Return Results with Row Count


Endpoint: POST /api/laofreeze/freeze-pa/upload-and-execute

# Response

{
  "success": true,
  "freezeType": "PA",
  "message": "LAO Freeze/Block PA completed successfully",
  "scriptCount": 6,
  "rowsAffectedByRegion": {
    "APAC": 2,
    "EMEA": 2,
    "LAO": 1,
    "NA": 1
  },
  "details": [
    {
      "contractNumber": "27198",
      "blockDate": "2024-01-01T00:00:00",
      "releaseDate": "2024-12-31T00:00:00",
      "region": "APAC",
      "freezeType": "PA"
    }
  ]
}
--Endpoint: GET /api/laofreeze/regions
{
  "success": true,
  "regionCount": 4,
  "regions": [
    {
      "regionCode": "APAC",
      "regionName": "Asia Pacific",
      "isActive": true
    },
    {
      "regionCode": "EMEA",
      "regionName": "Europe, Middle East, Africa",
      "isActive": true
    }
  ]
}


-- Note: The above responses are examples and may vary based on actual implementation and data.
-- End of LAO Freeze/Block PA & Product Freeze Integration Guide
-- Note: This documentation is intended for developers and technical teams responsible for implementing and maintaining the LAO Freeze/Block PA and Product Freeze processes within the SPARTA WAM solution. It provides detailed information on the expected input formats, processing steps, API endpoints, database schema, validation rules, error handling, and testing procedures to ensure a successful integration.
-- For any questions or issues during implementation, please refer to the SPARTA WAM technical support team or consult the internal documentation for further guidance.
-- LAO Freeze/Block PA & Product Freeze Integration Guide v1.0 | June 2026 | ✅ Production Ready
-- End of Document




























































































































































