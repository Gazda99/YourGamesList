using AutoFixture;
using YourGamesList.Api.Model.Dto;
using YourGamesList.Api.Services.ModelMappers;
using YourGamesList.Database.Entities;

namespace YourGamesList.Api.UnitTests.Services.ModelMappers;

public class YglDatabaseAndDtoMapperTests
{
    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void Map_GamesList_To_GamesListDto()
    {
        //ARRANGE
        var entity = _fixture.Build<GamesList>()
            .With(x => x.User, (User) null!)
            .WithAutoProperties()
            .Create();
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var dto = mapper.Map(entity);

        //ASSERT
        Assert.That(dto.Id, Is.EqualTo(entity.Id));
        Assert.That(dto.Desc, Is.EquivalentTo(entity.Desc));
        Assert.That(dto.Name, Is.EquivalentTo(entity.Name));
        Assert.That(dto.IsPublic, Is.EqualTo(entity.IsPublic));
        Assert.That(dto.CanBeDeleted, Is.EqualTo(entity.CanBeDeleted));
        //GameListEntries
    }

    [Test]
    public void Map_GameListEntry_To_GameListEntryDto()
    {
        //ARRANGE
        var entity = _fixture.Build<GameListEntry>()
            .With(x => x.Game, (Game) null!)
            .With(x => x.GamesList, (GamesList) null!)
            .WithAutoProperties()
            .Create();
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var dto = mapper.Map(entity);

        //ASSERT
        Assert.That(dto.Id, Is.EqualTo(entity.Id));
        Assert.That(dto.Desc, Is.EquivalentTo(entity.Desc));
        //Platforms
        //GameDistributions
        Assert.That(dto.IsStarred, Is.EqualTo(entity.IsStarred));
        Assert.That(dto.Rating, Is.EqualTo(entity.Rating));
        //CompletionStatus
    }

    [Test]
    public void Map_Game_To_GameDto()
    {
        //ARRANGE
        var entity = _fixture.Build<Game>()
            .With(x => x.GameListEntries, [])
            .WithAutoProperties()
            .Create();

        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var dto = mapper.Map(entity);

        //ASSERT
        Assert.That(dto.Id, Is.EqualTo(entity.Id));
        Assert.That(dto.FirstReleaseDate, Is.EqualTo(entity.FirstReleaseDate));
        Assert.That(dto.GameType, Is.EquivalentTo(entity.GameType));
        Assert.That(dto.Genres, Is.EquivalentTo(entity.Genres));
        Assert.That(dto.IgdbGameId, Is.EqualTo(entity.IgdbGameId));
        Assert.That(dto.ImageId, Is.EquivalentTo(entity.ImageId));
        Assert.That(dto.Name, Is.EquivalentTo(entity.Name));
        Assert.That(dto.StoryLine, Is.EquivalentTo(entity.StoryLine));
        Assert.That(dto.Summary, Is.EquivalentTo(entity.Summary));
        Assert.That(dto.Themes, Is.EquivalentTo(entity.Themes));
        Assert.That(dto.RatingCount, Is.EqualTo(entity.RatingCount));
    }

    [Test]
    [TestCaseSource(nameof(CompletionStatus_To_CompletionStatusDto_TestCases))]
    public void Map_CompletionStatus_To_CompletionStatusDto(CompletionStatus completionStatus, CompletionStatusDto expectedDto)
    {
        //ARRANGE
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var dto = mapper.Map(completionStatus);

        //ASSERT
        Assert.That(dto, Is.EqualTo(expectedDto));
    }

    [Test]
    [TestCaseSource(nameof(GameDistribution_To_GameDistributionDto_TestCases))]
    public void Map_GameDistribution_To_GameDistributionDto(GameDistribution completionStatus, GameDistributionDto expectedDto)
    {
        //ARRANGE
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var dto = mapper.Map(completionStatus);

        //ASSERT
        Assert.That(dto, Is.EqualTo(expectedDto));
    }

    [Test]
    [TestCaseSource(nameof(Platform_To_PlatformDto_TestCases))]
    public void Map_Platform_To_PlatformDto(Platform completionStatus, PlatformDto expectedDto)
    {
        //ARRANGE
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var dto = mapper.Map(completionStatus);

        //ASSERT
        Assert.That(dto, Is.EqualTo(expectedDto));
    }

    [Test]
    [TestCaseSource(nameof(CompletionStatus_To_CompletionStatusDto_TestCases))]
    public void Map_CompletionStatusDto_To_CompletionStatus(CompletionStatus expectedEntity, CompletionStatusDto completionStatusDto)
    {
        //ARRANGE
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var entity = mapper.Map(completionStatusDto);

        //ASSERT
        Assert.That(entity, Is.EqualTo(expectedEntity));
    }

    [Test]
    [TestCaseSource(nameof(GameDistribution_To_GameDistributionDto_TestCases))]
    public void Map_GameDistributionDto_To_GameDistribution(GameDistribution expectedEntity, GameDistributionDto gameDistributionDto)
    {
        //ARRANGE
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var entity = mapper.Map(gameDistributionDto);

        //ASSERT
        Assert.That(entity, Is.EqualTo(expectedEntity));
    }

    [Test]
    [TestCaseSource(nameof(Platform_To_PlatformDto_TestCases))]
    public void Map_PlatformDto_To_Platform(Platform expectedEntity, PlatformDto platformDto)
    {
        //ARRANGE
        var mapper = new YglDatabaseAndDtoMapper();

        //ACT
        var entity = mapper.Map(platformDto);

        //ASSERT
        Assert.That(entity, Is.EqualTo(expectedEntity));
    }


    private static TestCaseData[] CompletionStatus_To_CompletionStatusDto_TestCases =
    [
        new TestCaseData(CompletionStatus.Unspecified, CompletionStatusDto.Unspecified),
        new TestCaseData(CompletionStatus.JustTried, CompletionStatusDto.JustTried),
        new TestCaseData(CompletionStatus.Played, CompletionStatusDto.Played),
        new TestCaseData(CompletionStatus.FullyCompleted, CompletionStatusDto.FullyCompleted),
    ];

    private static TestCaseData[] GameDistribution_To_GameDistributionDto_TestCases =
    [
        new TestCaseData(GameDistribution.Unspecified, GameDistributionDto.Unspecified),
        new TestCaseData(GameDistribution.Physical, GameDistributionDto.Physical),
        new TestCaseData(GameDistribution.Crack, GameDistributionDto.Crack),
        new TestCaseData(GameDistribution.Steam, GameDistributionDto.Steam),
        new TestCaseData(GameDistribution.Origin, GameDistributionDto.Origin),
        new TestCaseData(GameDistribution.Uplay, GameDistributionDto.Uplay),
        new TestCaseData(GameDistribution.Epic, GameDistributionDto.Epic),
        new TestCaseData(GameDistribution.Xbox, GameDistributionDto.Xbox),
        new TestCaseData(GameDistribution.Other, GameDistributionDto.Other),
    ];

    private static TestCaseData[] Platform_To_PlatformDto_TestCases =
    [
        new TestCaseData(Platform.Unspecified, PlatformDto.Unspecified),
        new TestCaseData(Platform.PC, PlatformDto.PC),
        new TestCaseData(Platform.PlayStation1, PlatformDto.PlayStation1),
        new TestCaseData(Platform.PlayStation2, PlatformDto.PlayStation2),
        new TestCaseData(Platform.PlayStation3, PlatformDto.PlayStation3),
        new TestCaseData(Platform.PlayStation4, PlatformDto.PlayStation4),
        new TestCaseData(Platform.PlayStation5, PlatformDto.PlayStation5),
        new TestCaseData(Platform.PSP, PlatformDto.PSP),
        new TestCaseData(Platform.PSVita, PlatformDto.PSVita),
        new TestCaseData(Platform.Xbox, PlatformDto.Xbox),
        new TestCaseData(Platform.Xbox360, PlatformDto.Xbox360),
        new TestCaseData(Platform.XboxOne, PlatformDto.XboxOne),
        new TestCaseData(Platform.XboxSeriesX, PlatformDto.XboxSeriesX),
        new TestCaseData(Platform.XboxSeriesS, PlatformDto.XboxSeriesS),
        new TestCaseData(Platform.NintendoSwitch, PlatformDto.NintendoSwitch),
        new TestCaseData(Platform.AndroidMobileDevice, PlatformDto.AndroidMobileDevice),
        new TestCaseData(Platform.IOSMobileDevice, PlatformDto.IOSMobileDevice),
    ];
}