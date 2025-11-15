using AutoFixture;
using YourGamesList.Api.Model.Requests.Lists;
using YourGamesList.Api.Model.Requests.Users;
using YourGamesList.Api.Services.ModelMappers;

namespace YourGamesList.Api.UnitTests.Services.ModelMappers;

public class RequestToParametersMapperTests
{
    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Map_From_UpdateListRequest_To_UpdateListParameters()
    {
        //ARRANGE
        var request = _fixture.Create<UpdateListRequest>();
        var mapper = new RequestToParametersMapper();

        //ACT
        var parameter = mapper.Map(request);

        //ASSERT
        Assert.That(parameter.UserInformation.UserId, Is.EqualTo(request.UserInformation.UserId));
        Assert.That(parameter.UserInformation.Username, Is.EqualTo(request.UserInformation.Username));
        Assert.That(parameter.ListId, Is.EqualTo(request.Body.ListId));
        Assert.That(parameter.Desc, Is.EqualTo(request.Body.Desc));
        Assert.That(parameter.IsPublic, Is.EqualTo(request.Body.IsPublic));
        Assert.That(parameter.Name, Is.EqualTo(request.Body.Name));
    }

    [Test]
    public void Map_From_SearchListsRequest_To_SearchListsParameters()
    {
        //ARRANGE
        var request = _fixture.Create<SearchListsRequest>();
        var mapper = new RequestToParametersMapper();

        //ACT
        var parameter = mapper.Map(request);

        //ASSERT
        Assert.That(parameter.ListName, Is.EqualTo(request.Body.ListName));
        Assert.That(parameter.UserName, Is.EqualTo(request.Body.UserName));
        Assert.That(parameter.IncludeGames, Is.EqualTo(request.Body.IncludeGames));
        Assert.That(parameter.Take, Is.EqualTo(request.Body.Take));
        Assert.That(parameter.Skip, Is.EqualTo(request.Body.Skip));
    }


    [Test]
    public void Map_From_AddEntriesToListRequest_To_AddEntriesToListParameter()
    {
        //ARRANGE
        var request = _fixture.Create<AddEntriesToListRequest>();
        var mapper = new RequestToParametersMapper();

        //ACT
        var parameter = mapper.Map(request);

        //ASSERT
        Assert.That(parameter.UserInformation.UserId, Is.EqualTo(request.UserInformation.UserId));
        Assert.That(parameter.UserInformation.Username, Is.EqualTo(request.UserInformation.Username));
        Assert.That(parameter.ListId, Is.EqualTo(request.Body.ListId));
        var i = 0;
        foreach (var entryToAddRequestPart in request.Body.EntriesToAdd)
        {
            Assert.That(parameter.EntriesToAdd[i].GameId, Is.EqualTo(entryToAddRequestPart.GameId));
            Assert.That(parameter.EntriesToAdd[i].Desc, Is.EqualTo(entryToAddRequestPart.Desc));
            Assert.That(parameter.EntriesToAdd[i].Platforms, Is.EquivalentTo(entryToAddRequestPart.Platforms));
            Assert.That(parameter.EntriesToAdd[i].GameDistributions, Is.EquivalentTo(entryToAddRequestPart.GameDistributions));
            Assert.That(parameter.EntriesToAdd[i].IsStarred, Is.EqualTo(entryToAddRequestPart.IsStarred));
            Assert.That(parameter.EntriesToAdd[i].Rating, Is.EqualTo(entryToAddRequestPart.Rating));
            Assert.That(parameter.EntriesToAdd[i].CompletionStatus, Is.EqualTo(entryToAddRequestPart.CompletionStatus));
            i++;
        }
    }

    [Test]
    public void Map_From_DeleteListEntriesRequest_To_DeleteListEntriesParameter()
    {
        //ARRANGE
        var request = _fixture.Create<DeleteListEntriesRequest>();
        var mapper = new RequestToParametersMapper();

        //ACT
        var parameter = mapper.Map(request);

        //ASSERT
        Assert.That(parameter.UserInformation.UserId, Is.EqualTo(request.UserInformation.UserId));
        Assert.That(parameter.UserInformation.Username, Is.EqualTo(request.UserInformation.Username));
        Assert.That(parameter.ListId, Is.EqualTo(request.Body.ListId));
        Assert.That(parameter.EntriesToRemove, Is.EquivalentTo(request.Body.EntriesToRemove));
    }  
    
    [Test]
    public void Map_From_UpdateListEntriesRequest_To_UpdateListEntriesParameter()
    {
        //ARRANGE
        var request = _fixture.Create<UpdateListEntriesRequest>();
        var mapper = new RequestToParametersMapper();

        //ACT
        var parameter = mapper.Map(request);

        //ASSERT
        Assert.That(parameter.UserInformation.UserId, Is.EqualTo(request.UserInformation.UserId));
        Assert.That(parameter.UserInformation.Username, Is.EqualTo(request.UserInformation.Username));
        Assert.That(parameter.ListId, Is.EqualTo(request.Body.ListId));
        var i = 0;
        foreach (var entryToAddRequestPart in request.Body.EntriesToUpdate)
        {
            Assert.That(parameter.EntriesToUpdate[i].EntryId, Is.EqualTo(entryToAddRequestPart.EntryId));
            Assert.That(parameter.EntriesToUpdate[i].Desc, Is.EqualTo(entryToAddRequestPart.Desc));
            Assert.That(parameter.EntriesToUpdate[i].Platforms, Is.EquivalentTo(entryToAddRequestPart.Platforms));
            Assert.That(parameter.EntriesToUpdate[i].GameDistributions, Is.EquivalentTo(entryToAddRequestPart.GameDistributions));
            Assert.That(parameter.EntriesToUpdate[i].IsStarred, Is.EqualTo(entryToAddRequestPart.IsStarred));
            Assert.That(parameter.EntriesToUpdate[i].Rating, Is.EqualTo(entryToAddRequestPart.Rating));
            Assert.That(parameter.EntriesToUpdate[i].CompletionStatus, Is.EqualTo(entryToAddRequestPart.CompletionStatus));
            i++;
        }
    }
    
    [Test]
    public void Map_From_UserUpdateRequest_To_UserUpdateParameters()
    {
        //ARRANGE
        var request = _fixture.Create<UserUpdateRequest>();
        var mapper = new RequestToParametersMapper();

        //ACT
        var parameter = mapper.Map(request);

        //ASSERT
        Assert.That(parameter.UserInformation, Is.EqualTo(request.UserInformation));
        Assert.That(parameter.Country, Is.EqualTo(request.Body.Country));
        Assert.That(parameter.Description, Is.EqualTo(request.Body.Description));
        Assert.That(parameter.DateOfBirth, Is.EqualTo(request.Body.DateOfBirth));
    }  

}