namespace Igdb.Model.Helpers;

public static class IgdbEndpoints
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

    public static string GetEndpointBasedOnType<T>()
    {
        var type = typeof(T);
        if (type == typeof(AgeRating))
            return AgeRatings;
        if (type == typeof(AgeRatingContentDescription))
            return AgeRatingContentDescriptions;
        if (type == typeof(AlternativeName))
            return AlternativeNames;
        if (type == typeof(Artwork))
            return Artworks;
        if (type == typeof(Character))
            return Characters;
        if (type == typeof(CharacterMugShot))
            return CharacterMugShots;
        if (type == typeof(Collection))
            return Collections;
        if (type == typeof(CollectionMembership))
            return CollectionMembership;
        if (type == typeof(CollectionMembershipType))
            return CollectionMembershipType;
        if (type == typeof(CollectionRelation))
            return CollectionRelations;
        if (type == typeof(CollectionRelationType))
            return CollectionRelationTypes;
        if (type == typeof(CollectionType))
            return CollectionType;
        if (type == typeof(Company))
            return Companies;
        if (type == typeof(CompanyLogo))
            return CompanyLogos;
        if (type == typeof(CompanyWebsite))
            return CompanyWebsites;
        if (type == typeof(Cover))
            return Covers;
        if (type == typeof(Event))
            return Event;
        if (type == typeof(EventLogo))
            return EventLogos;
        if (type == typeof(EventNetwork))
            return EventNetworks;
        if (type == typeof(ExternalGame))
            return ExternalGames;
        if (type == typeof(Franchise))
            return Franchises;
        if (type == typeof(Game))
            return Games;
        if (type == typeof(GameEngine))
            return GameEngines;
        if (type == typeof(GameEngineLogo))
            return GameEngineLogos;
        if (type == typeof(GameLocalization))
            return GameLocalizations;
        if (type == typeof(GameMode))
            return GameModes;
        if (type == typeof(GameVersion))
            return GameVersions;
        if (type == typeof(GameVersionFeature))
            return GameVersionFeatures;
        if (type == typeof(GameVersionFeatureValue))
            return GameVersionFeatureValues;
        if (type == typeof(GameVideo))
            return GameVideos;
        if (type == typeof(Genre))
            return Genres;
        if (type == typeof(InvolvedCompany))
            return InvolvedCompanies;
        if (type == typeof(Keyword))
            return Keywords;
        if (type == typeof(Language))
            return Languages;
        if (type == typeof(LanguageSupport))
            return LanguageSupports;
        if (type == typeof(LanguageSupportType))
            return LanguageSupportTypes;
        if (type == typeof(MultiplayerMode))
            return MultiplayerModes;
        if (type == typeof(NetworkType))
            return NetworkTypes;
        if (type == typeof(Platform))
            return Platforms;
        if (type == typeof(PlatformFamily))
            return PlatformFamilies;
        if (type == typeof(PlatformLogo))
            return PlatformLogos;
        if (type == typeof(PlatformVersion))
            return PlatformVersions;
        if (type == typeof(PlatformVersionCompany))
            return PlatformVersionCompanies;
        if (type == typeof(PlatformVersionReleaseDate))
            return PlatformVersionReleaseDates;
        if (type == typeof(PlatformWebsite))
            return PlatformWebsites;
        if (type == typeof(PlayerPerspective))
            return PlayerPerspectives;
        if (type == typeof(Region))
            return Regions;
        if (type == typeof(ReleaseDate))
            return ReleaseDates;
        if (type == typeof(ReleaseDateStatus))
            return ReleaseDateStatus;
        if (type == typeof(Screenshot))
            return Screenshots;
        if (type == typeof(Search))
            return Search;
        if (type == typeof(Theme))
            return Themes;
        if (type == typeof(Website))
            return Websites;

        throw new ArgumentException($"Couldn't resolve endpoint for type: {type}", nameof(type));
    }
}