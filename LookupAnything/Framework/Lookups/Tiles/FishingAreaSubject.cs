using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using StardewValley.GameData.Locations;
using System.Linq;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

/// <summary>Describes a fishable area.</summary>
internal class FishingAreaSubject : TileSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The fish area ID which applies, if any.</summary>
    private readonly string FishAreaId;
    ///<summary>The game location to which this area belongs.</summary>
    private readonly LocationData LocationData;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="location">The game location.</param>
    /// <param name="position">The tile position.</param>
    /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
    /// <param name="fishAreaId">The internal name of the fishing area to which this tile belongs.</param>
    public FishingAreaSubject(GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo, string fishAreaId) : base(gameHelper, location, position, showRawTileInfo)
    {
        this.Name = this.GameHelper.GetLocationDisplayName(location, fishAreaId);
        this.Description = null;
        this.Type = "Fishing area";
        this.FishAreaId = fishAreaId;
        this.LocationData = location.GetData();
    }

    /// <inheritdoc/>
    public override IEnumerable<ICustomField> GetData()
    {
        IEnumerable<ParsedItemData> fishes = from spawn in this.LocationData.Fish
                                             where spawn.ItemId != null && this.IsSpawnPossible(spawn)
                                             orderby spawn.Precedence
                                             select ItemRegistry.GetData(spawn.ItemId);

        yield return new FishSpawnRulesField(this.GameHelper, I18n.Item_FishSpawnRules(), fishes.ToArray());

        // raw map data
        foreach (ICustomField field in base.GetData())
            yield return field;
    }

    /*********
    ** Private methods
    *********/
    /// <summary>Gets whether a fish can spawn in this area.</summary>
    /// <param name="spawn">The fish spawn data that describes the fish to test.</param>
    /// <remarks>Derived from <see cref="GameLocation.GetFishFromLocationData"/>.</remarks>
    private bool IsSpawnPossible(SpawnFishData spawn)
    {
        // check if fish can spawn in this body of water
        if (spawn.FishAreaId != null && spawn.FishAreaId != this.FishAreaId)
            return false;

        // check if bobber is in proper position
        if (spawn.BobberPosition.HasValue && !spawn.BobberPosition.GetValueOrDefault().Contains((int)this.Position.X, (int)this.Position.Y))
            return false;

        // check if player is in proper position
        if (spawn.PlayerPosition.HasValue && !spawn.PlayerPosition.GetValueOrDefault().Contains(Game1.player.TilePoint.X, Game1.player.TilePoint.Y))
            return false;

        return true;
    }
}
