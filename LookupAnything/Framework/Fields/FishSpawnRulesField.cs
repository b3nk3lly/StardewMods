using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows the spawn rules for a fish.</summary>
internal class FishSpawnRulesField : CheckboxListField
{
    /*********
    ** Fields
    *********/
    /// <summary>The valid seasons.</summary>
    private static readonly string[] Seasons = ["spring", "summer", "fall", "winter"];
    /// <summary>Whether to show spawn conditions of uncaught fish.</summary>
    private readonly bool ShowUncaughtFishSpawnRules;
    /// <summary>Collection relating fish IDs to the fish's spawning conditions.</summary>
    private readonly Dictionary<string, CheckboxList> SpawnConditions = [];

    /*********
    ** Public methods
    *********/
    /// <param name="fishID">The fish ID.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions of uncaught fish.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, ParsedItemData fishID, bool showUncaughtFishSpawnRules)
        : this(label, showUncaughtFishSpawnRules, GetConditions(gameHelper, fishID))
    {
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions of uncaught fish.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, GameLocation location, Vector2 tile, string fishAreaId, bool showUncaughtFishSpawnRules)
        : this(label, showUncaughtFishSpawnRules, GetConditions(gameHelper, location, tile, fishAreaId))
    {
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions of uncaught fish.</param>
    public FishSpawnRulesField(string label, bool showUncaughtFishSpawnRules, Dictionary<string, CheckboxList> spawnConditions)
    : base(label)
    {
        this.SpawnConditions = spawnConditions;
        this.CheckboxLists = spawnConditions.Select(condition => condition.Value).ToArray();
        this.HasValue = this.CheckboxLists.Any();
        this.ShowUncaughtFishSpawnRules = showUncaughtFishSpawnRules;
    }

    /// <inheritdoc/>
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        float topOffset = 0;
        int hiddenSpawnRulesCount = 0;

        foreach ((string fishID, CheckboxList checkboxes) in this.SpawnConditions)
        {
            ParsedItemData fishItemData = ItemRegistry.GetDataOrErrorItem(fishID);

            if (!this.ShowUncaughtFishSpawnRules && !HasPlayerCaughtFish(fishItemData))
            {
                hiddenSpawnRulesCount++;
            }
            else
            {
                topOffset += this.LineHeight + this.DrawCheckboxList(checkboxes, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth).Y;
            }
        }

        if (hiddenSpawnRulesCount > 0)
        {
            // draw number of hidden spawn rules
            topOffset += this.LineHeight + this.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, I18n.Item_FishUnknownSpawnRules(hiddenSpawnRulesCount), Color.Gray).Y;
        }

        return new Vector2(wrapWidth, topOffset - this.LineHeight);
    }

    /*********
    ** Private methods
    *********/
    /// <summary>Get the formatted checkbox conditions for all fish in a location.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    private static Dictionary<string, CheckboxList> GetConditions(GameHelper gameHelper, GameLocation location, Vector2 tile, string fishAreaId)
    {
        Dictionary<string, CheckboxList> conditions = [];

        foreach (FishSpawnData spawnRules in gameHelper.GetFishSpawnRules(location, tile, fishAreaId))
        {
            ParsedItemData fishItemData = ItemRegistry.GetDataOrErrorItem(spawnRules.FishItem.QualifiedItemId);

            conditions.Add(fishItemData.QualifiedItemId, new(GetConditions(gameHelper, spawnRules))
            {
                IntroData = new CheckboxList.Intro(fishItemData.DisplayName, new SpriteInfo(fishItemData.GetTexture(), fishItemData.GetSourceRect()))
            });
        }

        return conditions;
    }

    /// <summary>Get the formatted checkbox conditions for a single fish.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="fish">The fish item data.</param>
    private static Dictionary<string, CheckboxList> GetConditions(GameHelper gameHelper, ParsedItemData fish)
    {
        Dictionary<string, CheckboxList> conditions = [];

        // get spawn data
        FishSpawnData? spawnRules = gameHelper.GetFishSpawnRules(fish);
        if (spawnRules?.Locations?.Any() == true)
            conditions.Add(fish.QualifiedItemId, new(GetConditions(gameHelper, spawnRules)));

        return conditions;
    }

    /// <summary>Get the formatted checkbox conditions to display.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="spawnRules">The fish spawn rules to format.</param>
    private static IEnumerable<CheckboxList.Checkbox> GetConditions(GameHelper gameHelper, FishSpawnData spawnRules)
    {
        // not caught uet
        if (spawnRules.IsUnique)
            yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_NotCaughtYet(), isChecked: !HasPlayerCaughtFish(spawnRules.FishItem));

        // fishing level
        if (spawnRules.MinFishingLevel > 0)
            yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_MinFishingLevel(level: spawnRules.MinFishingLevel), isChecked: Game1.player.FishingLevel >= spawnRules.MinFishingLevel);

        // extended family quest
        if (spawnRules.IsLegendaryFamily)
            yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_ExtendedFamilyQuestActive(), isChecked: Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"));

        // weather
        if (spawnRules.Weather == FishSpawnWeather.Sunny)
            yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_WeatherSunny(), isChecked: !Game1.isRaining);
        else if (spawnRules.Weather == FishSpawnWeather.Rainy)
            yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_WeatherRainy(), isChecked: Game1.isRaining);

        // time of day
        if (spawnRules.TimesOfDay?.Any() == true)
        {
            yield return new CheckboxList.Checkbox(
                text: I18n.Item_FishSpawnRules_Time(
                    times: I18n.List(
                        spawnRules.TimesOfDay.Select(p => I18n.Generic_Range(Game1.getTimeOfDayString(p.MinTime), Game1.getTimeOfDayString(p.MaxTime)).ToString())
                    )
                ),
                isChecked: spawnRules.TimesOfDay.Any(p => Game1.timeOfDay >= p.MinTime && Game1.timeOfDay <= p.MaxTime)
            );
        }

        // locations & seasons
        if (spawnRules.Locations != null && spawnRules.Locations.Any() && HaveSameSeasons(spawnRules.Locations))
        {
            var firstLocation = spawnRules.Locations[0];

            // seasons
            if (firstLocation.Seasons.Count == 4)
                yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_SeasonAny(), isChecked: true);
            else
            {
                yield return new CheckboxList.Checkbox(
                    text: I18n.Item_FishSpawnRules_SeasonList(
                        seasons: I18n.List(
                            firstLocation.Seasons.Select(gameHelper.TranslateSeason)
                        )
                    ),
                    isChecked: firstLocation.Seasons.Contains(Game1.currentSeason)
                );
            }

            // locations
            yield return new CheckboxList.Checkbox(
                text: I18n.Item_FishSpawnRules_Locations(
                    locations: I18n.List(
                        spawnRules.Locations.Select(gameHelper.GetLocationDisplayName).OrderBy(p => p)
                    )
                ),
                isChecked: spawnRules.MatchesLocation(Game1.currentLocation.Name)
            );
        }
        else
        {
            IDictionary<string, string[]> locationsBySeason =
                (
                    from location in spawnRules.Locations
                    from season in location.Seasons
                    select new { Season = season, LocationName = gameHelper.GetLocationDisplayName(location) }
                )
                .GroupBy(p => p.Season, p => p.LocationName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(p => p.Key, p => p.ToArray(), StringComparer.OrdinalIgnoreCase);

            var summary = new List<IFormattedText> { new FormattedText(I18n.Item_FishSpawnRules_LocationsBySeason_Label()) };
            foreach (string season in Seasons)
            {
                if (locationsBySeason.TryGetValue(season, out string[]? locationNames))
                {
                    summary.Add(new FormattedText(
                        text: Environment.NewLine + I18n.Item_FishSpawnRules_LocationsBySeason_SeasonLocations(season: gameHelper.TranslateSeason(season), locations: I18n.List(locationNames)),
                        color: season == Game1.currentSeason ? Color.Black : Color.Gray
                    ));
                }
            }

            bool hasMatch = spawnRules.Locations.Any(p => p.LocationId == Game1.currentLocation.Name && p.Seasons.Contains(Game1.currentSeason));
            yield return new CheckboxList.Checkbox(text: summary.ToArray(), isChecked: hasMatch);
        }
    }

    /// <summary>Get whether all locations specify the same seasons.</summary>
    /// <param name="locations">The locations to check.</param>
    private static bool HaveSameSeasons(IEnumerable<FishSpawnLocationData> locations)
    {
        ISet<string>? seasons = null;
        foreach (FishSpawnLocationData location in locations)
        {
            if (seasons == null)
                seasons = location.Seasons;
            else if (seasons.Count != location.Seasons.Count || !location.Seasons.All(seasons.Contains))
                return false;
        }

        return true;
    }

    /// <summary>Get whether the player has caught a fish.</summary>
    /// <param name="fish">The fish data.</param>
    private static bool HasPlayerCaughtFish(ParsedItemData fish)
    {
        return Game1.player.fishCaught.ContainsKey(fish.ItemId);
    }
}
