# SPARTA WAM Database Setup Guide

## Overview

This document provides comprehensive instructions for setting up the database connector for the SPARTA Walk Away Margin (WAM) application.

## Architecture

The database layer follows a multi-layered architecture:

## Components

### 1. Connection Factory
- **Purpose**: Creates and manages SQL connections
- **Location**: `Data/Connections/SqlConnectionFactory.cs`
- **Features**:
  - Connection pooling
  - Async connection creation
  - Connection validation

### 2. Repository Pattern
- **Purpose**: Encapsulates data access logic
- **Location**: `Data/Repositories/WalkAwayMarginRepository.cs`
- **Methods**:
  - `GetByKeyAsync()` - Retrieve single record
  - `GetAllAsync()` - Retrieve all records
  - `ExistsAsync()` - Check record existence
  - `CreateAsync()` - Insert new record
  - `UpdateAsync()` - Update existing record
  - `DeleteAsync()` - Delete record

### 3. Resilience Policies
- **Purpose**: Provides retry logic and circuit breaker pattern
- **Location**: `Data/Resilience/ResiliencePolicyProvider.cs`
- **Features**:
  - Exponential backoff retry (3 attempts)
  - Circuit breaker (opens after 3 failures)
  - Transient error detection

### 4. SQL Execution Service
- **Purpose**: Direct SQL execution for complex queries
- **Location**: `Data/Services/SqlExecutionService.cs`
- **Methods**:
  - `ExecuteSqlAsync()` - Execute update/insert/delete
  - `ExecuteQueryAsync()` - Execute SELECT queries

### 5. WAM Database Service
- **Purpose**: Business logic for WAM operations
- **Location**: `Data/Services/WamDatabaseService.cs`
- **Methods**:
  - `ExecuteWamScriptsAsync()` - Execute batch WAM updates
  - `GetWamRecordsAsync()` - Retrieve filtered WAM records

## Configuration

### Connection String

Update `appsettings.json`:

### Database Setup

Run Entity Framework migrations:

### Generated Table Schema

## API Endpoints

### 1. Generate SQL Script
### 2. Execute Scripts
### 3. Retrieve WAM Records
### 4. Upload and Execute
## Error Handling

### Transient Errors (Auto-Retry)
- Connection timeouts
- Temporary connection failures
- SQL errors: 2, 20, 64, 233

### Non-Transient Errors (No Retry)
- SQL injection attempts
- Invalid credentials
- Schema mismatch

## Logging

All database operations are logged:

## Security Considerations

### Connection Security
- Use encrypted connections (Encrypt=true)
- Implement certificate validation
- Store credentials in secure vaults

### SQL Injection Prevention
- Use parameterized queries (built into EF Core)
- Validate all input data
- Sanitize external inputs

### Access Control
- Use SQL authentication roles
- Implement database-level permissions
- Audit all modifications

## Performance Optimization

### Connection Pooling
- Default max pool size: 100
- Min pool size: 5
- Connection timeout: 30 seconds

### Query Optimization
- Indexes on frequently queried columns
- Connection string optimizations
- Batch operations for bulk updates

### Caching
- Consider implementing distributed cache
- Cache frequently accessed records
- Invalidate cache on updates

## Troubleshooting

### Connection Issues
1. Verify connection string
2. Check SQL Server service status
3. Verify firewall rules
4. Check credentials and permissions

### Performance Issues
1. Check query execution plans
2. Monitor connection pool usage
3. Review application logs
4. Check SQL Server metrics

### Timeout Issues
1. Increase command timeout (currently 300s)
2. Optimize slow queries
3. Check SQL Server load
4. Review network latency