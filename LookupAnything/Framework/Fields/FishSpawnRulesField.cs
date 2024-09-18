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

using SpawnRuleCondition = (ParsedItemData fish, CheckboxList checkboxList);

/// <summary>A metadata field which shows the spawn rules for a fish.</summary>
internal class FishSpawnRulesField : CheckboxListField
{
    /*********
    ** Fields
    *********/
    /// <summary>The valid seasons.</summary>
    private readonly string[] Seasons = ["spring", "summer", "fall", "winter"];
    /// <summary>Whether to show spawn conditions of uncaught fish.</summary>
    private readonly bool ShowUncaughtFishSpawnRules;
    /// <summary>Collection relating fish IDs to the fish's spawning conditions.</summary>
    private readonly IEnumerable<SpawnRuleCondition> SpawnConditions;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="fish">The fish item data.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions of uncaught fish.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, ParsedItemData fish, bool showUncaughtFishSpawnRules)
        : base(label)
    {
        this.SpawnConditions = this.GetConditions(gameHelper, fish);
        this.CheckboxLists = this.SpawnConditions.Select(condition => condition.checkboxList).ToArray();
        this.HasValue = this.CheckboxLists.Any();
        this.ShowUncaughtFishSpawnRules = showUncaughtFishSpawnRules;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="label">A short field label.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    /// <param name="showUncaughtFishSpawnRules">Whether to show spawn conditions of uncaught fish.</param>
    public FishSpawnRulesField(GameHelper gameHelper, string label, GameLocation location, Vector2 tile, string fishAreaId, bool showUncaughtFishSpawnRules)
        : base(label)
    {
        this.SpawnConditions = this.GetConditions(gameHelper, location, tile, fishAreaId);
        this.CheckboxLists = this.SpawnConditions.Select(condition => condition.checkboxList).ToArray();
        this.HasValue = this.CheckboxLists.Any();
        this.ShowUncaughtFishSpawnRules = showUncaughtFishSpawnRules;
    }

    /// <inheritdoc/>
    public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        float topOffset = 0;
        int hiddenSpawnRulesCount = 0;

        foreach ((ParsedItemData fish, CheckboxList checkboxList) in this.SpawnConditions)
        {
            if (!this.ShowUncaughtFishSpawnRules && !this.HasPlayerCaughtFish(fish))
            {
                hiddenSpawnRulesCount++;
            }
            else
            {
                topOffset += this.LineHeight + this.DrawCheckboxList(checkboxList, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth).Y;
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
    /// <summary>Get the formatted checkbox conditions to display.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="location">The location whose fish spawn conditions to get.</param>
    /// <param name="tile">The tile for which to get the spawn rules.</param>
    /// <param name="fishAreaId">The internal ID of the fishing area for which to get the spawn rules.</param>
    private IEnumerable<SpawnRuleCondition> GetConditions(GameHelper gameHelper, GameLocation location, Vector2 tile, string fishAreaId)
    {
        foreach (FishSpawnData spawnRules in gameHelper.GetFishSpawnRules(location, tile, fishAreaId))
        {
            ParsedItemData fishItemData = ItemRegistry.GetDataOrErrorItem(spawnRules.FishItem.QualifiedItemId);

            yield return (fishItemData, new(this.GetConditions(gameHelper, spawnRules))
            {
                IntroData = new CheckboxList.Intro(fishItemData.DisplayName, new SpriteInfo(fishItemData.GetTexture(), fishItemData.GetSourceRect()))
            });
        }
    }

    /// <summary>Get the formatted checkbox conditions to display.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="fish">The fish item data.</param>
    private IEnumerable<SpawnRuleCondition> GetConditions(GameHelper gameHelper, ParsedItemData fish)
    {
        // get spawn data
        FishSpawnData? spawnRules = gameHelper.GetFishSpawnRules(fish);
        if (spawnRules?.Locations?.Any() != true)
            yield break;

        yield return (fish, new(this.GetConditions(gameHelper, spawnRules)));
    }

    /// <summary>Get the formatted checkbox conditions to display.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="spawnRules">The fish spawn rules to format.</param>
    private IEnumerable<CheckboxList.Checkbox> GetConditions(GameHelper gameHelper, FishSpawnData spawnRules)
    {
        // not caught uet
        if (spawnRules.IsUnique)
            yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_NotCaughtYet(), isChecked: !this.HasPlayerCaughtFish(spawnRules.FishItem));

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
        if (spawnRules.Locations != null && spawnRules.Locations.Any() && this.HaveSameSeasons(spawnRules.Locations))
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
            foreach (string season in this.Seasons)
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
    private bool HaveSameSeasons(IEnumerable<FishSpawnLocationData> locations)
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
    private bool HasPlayerCaughtFish(ParsedItemData fish)
    {
        return Game1.player.fishCaught.ContainsKey(fish.ItemId);
    }
}
