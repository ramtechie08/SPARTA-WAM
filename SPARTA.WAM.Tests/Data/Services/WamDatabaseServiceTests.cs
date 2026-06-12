using Moq;
using NUnit.Framework;
using SPARTA_WAM.Data.Repositories;
using SPARTA_WAM.Data.Services;
using SPARTA_WAM.Models;
using Xunit;

namespace SPARTA.WAM.Tests.Data.Services;

public class WamDatabaseServiceTests
{
    [Fact]
    public async Task ExecuteWamScriptsAsync_WithValidScripts_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mockSqlService = new Mock<ISqlExecutionService>();
        var mockRepository = new Mock<IWalkAwayMarginRepository>();
        var mockLogger = new Mock<ILogger<WamDatabaseService>>();

        mockSqlService
            .Setup(s => s.ExecuteSqlAsync(It.IsAny<string>()))
            .ReturnsAsync(2);

        var service = new WamDatabaseService(mockSqlService.Object, mockRepository.Object, mockLogger.Object);

        var scripts = new List<SqlScriptResult>
        {
            new()
            {
                SqlStatement = "SELECT 1;",
                ShortCode = "001",
                MarginValue = 5,
                SalesOrganization = "7090",
                PricingType = "D"
            }
        };

        // Act
        var result = await service.ExecuteWamScriptsAsync(scripts);

        // Assert
        Assert.Equals(2, result);
        mockSqlService.Verify(s => s.ExecuteSqlAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteWamScriptsAsync_WithEmptyScripts_ShouldReturnZero()
    {
        // Arrange
        var mockSqlService = new Mock<ISqlExecutionService>();
        var mockRepository = new Mock<IWalkAwayMarginRepository>();
        var mockLogger = new Mock<ILogger<WamDatabaseService>>();

        var service = new WamDatabaseService(mockSqlService.Object, mockRepository.Object, mockLogger.Object);
        var scripts = new List<SqlScriptResult>();

        // Act
        var result = await service.ExecuteWamScriptsAsync(scripts);

        // Assert
        Assert.Equals(0, result);
        mockSqlService.Verify(s => s.ExecuteSqlAsync(It.IsAny<string>()), Times.Never);
    }
}