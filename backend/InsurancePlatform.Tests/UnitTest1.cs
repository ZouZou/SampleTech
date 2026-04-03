using InsurancePlatform.Domain.Entities;
using InsurancePlatform.Domain.Enums;

namespace InsurancePlatform.Tests;

public class DomainEntityTests
{
    [Fact]
    public void User_FullName_CombinesFirstAndLastName()
    {
        var user = new User { FirstName = "Jane", LastName = "Smith" };
        Assert.Equal("Jane Smith", user.FullName);
    }

    [Fact]
    public void BaseEntity_NewInstance_HasNonEmptyId()
    {
        var user = new User();
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void BaseEntity_IsDeleted_FalseByDefault()
    {
        var user = new User();
        Assert.False(user.IsDeleted);
    }

    [Fact]
    public void BaseEntity_IsDeleted_TrueWhenDeletedAtSet()
    {
        var user = new User { DeletedAt = DateTime.UtcNow };
        Assert.True(user.IsDeleted);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Underwriter)]
    [InlineData(UserRole.Agent)]
    [InlineData(UserRole.Broker)]
    [InlineData(UserRole.Client)]
    public void UserRole_AllValuesAreDefined(UserRole role)
    {
        Assert.True(Enum.IsDefined(role));
    }
}
