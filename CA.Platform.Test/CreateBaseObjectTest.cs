using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Test.Database;
using CA.Platform.Tests;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CA.Platform.Test;

public class CreateBaseObjectTest : BaseTest<DataContext>
{
    private readonly Guid _userId = Guid.NewGuid();

    protected override void AddUserContext(ServiceCollection services)
    {
        services.AddScoped(_ =>
        {
            var moq = new Mock<IUserContext>();
            moq.Setup(t => t.GetCurrentUser()).Returns(new UserDto()
            {
                Id = _userId
            });
            
            moq.Setup(t => t.GetCurrentUserId()).Returns(_userId);
            return moq.Object;
        });
    }

    [Test]
    public async Task ShouldSetCreateDateAndCreated()
    {
        var topic = new Topic
        {
            Title = "Test"
        };
        DbContext.GetDbSet<Topic>().Add(topic);
        await DbContext.SaveChangesAsync(cancellationToken: CancellationToken.None);
        
        Assert.True(topic.Created > DateTime.Today && topic.CreatedBy == _userId);
        Assert.True(topic.LastModified == null && topic.LastModifiedBy == null);
    }
    
    [Test]
    public async Task ShouldSetModifyAndModifyDate()
    {
        var topic = new Topic
        {
            Title = "Test"
        };
        DbContext.GetDbSet<Topic>().Add(topic);
        await DbContext.SaveChangesAsync(cancellationToken: CancellationToken.None);

        topic.Description = "Test description";
        await DbContext.SaveChangesAsync(cancellationToken: CancellationToken.None);

        Assert.True(topic.LastModified > DateTime.Today && topic.LastModifiedBy == _userId);
    }
}