using System;

namespace YourGamesList.IgdbScraper.Tests.TestHelpers;

public static class EndpointsHelper
{
    public const string AgeRatings = "age_ratings";
    public const string AgeRatingContentDescriptions = "age_rating_content_descriptions";
    public const string AlternativeNames = "alternative_names";
    public const string Artworks = "artworks";
    public const string Characters = "characters";
    public const string CharacterMugShots = "character_mug_shots";
    public const string Collections = "collections";
    public const string CollectionMembership = "collection_memberships";
    public const string CollectionMembershipType = "collection_membership_types";
    public const string CollectionRelations = "collection_relations";
    public const string CollectionRelationTypes = "collection_relation_types";
    public const string CollectionType = "collection_types";
    public const string Companies = "companies";
    public const string CompanyLogos = "company_logos";
    public const string CompanyWebsites = "company_websites";
    public const string Covers = "covers";
    public const string Event = "event";
    public const string EventLogos = "event_logos";
    public const string EventNetworks = "event_networks";
    public const string ExternalGames = "external_games";
    public const string Franchises = "franchises";
    public const string Games = "games";
    public const string GameEngines = "game_engines";
    public const string GameEngineLogos = "game_engine_logos";
    public const string GameLocalizations = "game_localizations";
    public const string GameModes = "game_modes";
    public const string GameVersions = "game_versions";
    public const string GameVersionFeatures = "game_version_features";
    public const string GameVersionFeatureValues = "game_version_feature_values";
    public const string GameVideos = "game_videos";
    public const string Genres = "genres";
    public const string InvolvedCompanies = "involved_companies";
    public const string Keywords = "keywords";
    public const string Languages = "languages";
    public const string LanguageSupports = "language_supports";
    public const string LanguageSupportTypes = "language_support_types";
    public const string MultiplayerModes = "multiplayer_modes";
    public const string NetworkTypes = "network_types";
    public const string Platforms = "platforms";
    public const string PlatformFamilies = "platform_families";
    public const string PlatformLogos = "platform_logos";
    public const string PlatformVersions = "platform_versions";
    public const string PlatformVersionCompanies = "platform_version_companies";
    public const string PlatformVersionReleaseDates = "platform_version_release_dates";
    public const string PlatformWebsites = "platform_websites";
    public const string PlayerPerspectives = "player_perspectives";
    public const string Regions = "regions";
    public const string ReleaseDates = "release_dates";
    public const string ReleaseDateStatus = "release_date_statuses";
    public const string Screenshots = "screenshots";
    public const string Search = "search";
    public const string Themes = "themes";
    public const string Websites = "websites";

    public const string MultiQuery = "multiquery";

    public static string RandomEndpoint()
    {
        var rnd = new Random();
        var i = rnd.Next(AllDataEndpoints.Length);
        return AllDataEndpoints[i];
    }


    public static readonly string[] AllDataEndpoints = new[]
    {
        AgeRatings,
        AgeRatingContentDescriptions,
        AlternativeNames,
        Artworks,
        Characters,
        CharacterMugShots,
        Collections,
        CollectionMembership,
        CollectionMembershipType,
        CollectionRelations,
        CollectionRelationTypes,
        CollectionType,
        Companies,
        CompanyLogos,
        CompanyWebsites,
        Covers,
        Event,
        EventLogos,
        EventNetworks,
        ExternalGames,
        Franchises,
        Games,
        GameEngines,
        GameEngineLogos,
        GameLocalizations,
        GameModes,
        GameVersions,
        GameVersionFeatures,
        GameVersionFeatureValues,
        GameVideos,
        Genres,
        InvolvedCompanies,
        Keywords,
        Languages,
        LanguageSupports,
        LanguageSupportTypes,
        MultiplayerModes,
        NetworkTypes,
        Platforms,
        PlatformFamilies,
        PlatformLogos,
        PlatformVersions,
        PlatformVersionCompanies,
        PlatformVersionReleaseDates,
        PlatformWebsites,
        PlayerPerspectives,
        Regions,
        ReleaseDates,
        ReleaseDateStatus,
        Screenshots,
        Search,
        Themes,
        Websites,
        // MultiQuery
    };
}