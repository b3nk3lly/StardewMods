using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows the spawn rules for a fish.</summary>
    internal class FishSpawnRulesField : CheckboxListField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The valid seasons.</summary>
        private readonly string[] Seasons = ["spring", "summer", "fall", "winter"];


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="fishID">The fish ID.</param>
        public FishSpawnRulesField(GameHelper gameHelper, string label, params string[] fishIDs)
            : base(label)
        {
            this.CheckboxLists = this.BuildCheckboxLists(gameHelper, fishIDs).ToArray();
            this.HasValue = this.CheckboxLists.Any();
        }


        /*********
        ** Private methods
        *********/

        private IEnumerable<CheckboxList> BuildCheckboxLists(GameHelper gameHelper, params string[] fishIDs)
        {
            foreach (string fishID in fishIDs)
            {
                Item fish = ItemRegistry.Create(fishID);

                // get spawn data
                FishSpawnData? spawnRules = gameHelper.GetFishSpawnRules(fish.ItemId);
                if (spawnRules?.Locations?.Any() != true)
                    continue;

                CheckboxList checkboxes = new CheckboxList(this.GetConditions(gameHelper, spawnRules));
                if (fishIDs.Length > 1)
                {
                    checkboxes.IntroData = new CheckboxList.Intro(fish.DisplayName, gameHelper.GetSprite(fish));
                }

                yield return checkboxes;
            }
        }

        /// <summary>Get the formatted checkbox conditions to display.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="fishID">The fish ID.</param>
        private IEnumerable<CheckboxList.Checkbox> GetConditions(GameHelper gameHelper, FishSpawnData spawnRules)
        {
            // not caught uet
            if (spawnRules.IsUnique)
                yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_NotCaughtYet(), value: !Game1.player.fishCaught.ContainsKey(spawnRules.FishID));

            // fishing level
            if (spawnRules.MinFishingLevel > 0)
                yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_MinFishingLevel(level: spawnRules.MinFishingLevel), value: Game1.player.FishingLevel >= spawnRules.MinFishingLevel);

            // weather
            if (spawnRules.Weather == FishSpawnWeather.Sunny)
                yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_WeatherSunny(), value: !Game1.isRaining);
            else if (spawnRules.Weather == FishSpawnWeather.Rainy)
                yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_WeatherRainy(), value: Game1.isRaining);

            // time of day
            if (spawnRules.TimesOfDay?.Any() == true)
            {
                yield return new CheckboxList.Checkbox(
                    text: I18n.Item_FishSpawnRules_Time(
                        times: I18n.List(
                            spawnRules.TimesOfDay.Select(p => I18n.Generic_Range(gameHelper.FormatMilitaryTime(p.MinTime), gameHelper.FormatMilitaryTime(p.MaxTime)).ToString())
                        )
                    ),
                    value: spawnRules.TimesOfDay.Any(p => Game1.timeOfDay >= p.MinTime && Game1.timeOfDay <= p.MaxTime)
                );
            }

            // locations & seasons
            if (this.HaveSameSeasons(spawnRules.Locations))
            {
                var firstLocation = spawnRules.Locations[0];

                // seasons
                if (firstLocation.Seasons.Count == 4)
                    yield return new CheckboxList.Checkbox(text: I18n.Item_FishSpawnRules_SeasonAny(), value: true);
                else
                {
                    yield return new CheckboxList.Checkbox(
                        text: I18n.Item_FishSpawnRules_SeasonList(
                            seasons: I18n.List(
                                firstLocation.Seasons.Select(gameHelper.TranslateSeason)
                            )
                        ),
                        value: firstLocation.Seasons.Contains(Game1.currentSeason)
                    );
                }

                // locations
                yield return new CheckboxList.Checkbox(
                    text: I18n.Item_FishSpawnRules_Locations(
                        locations: I18n.List(
                            spawnRules.Locations.Select(p => p.DisplayName).OrderBy(p => p)
                        )
                    ),
                    value: spawnRules.MatchesLocation(Game1.currentLocation.Name)
                );
            }
            else
            {
                IDictionary<string, string[]> locationsBySeason =
                    (
                        from location in spawnRules.Locations
                        from season in location.Seasons
                        select new { Season = season, LocationName = location.DisplayName }
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
                yield return new CheckboxList.Checkbox(text: summary.ToArray(), value: hasMatch);
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
    }
}
