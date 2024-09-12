using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles
{
    /// <summary>Describes the <see cref="IslandShrine"/> puzzle.</summary>
    internal class IslandShrinePuzzleSubject : TileSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether to show puzzle solutions.</summary>
        private readonly bool ShowPuzzleSolutions;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
        /// <param name="showPuzzleSolutions">Whether to show puzzle solutions.</param>
        public IslandShrinePuzzleSubject(GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo, bool showPuzzleSolutions)
            : base(gameHelper, location, position, showRawTileInfo)
        {
            this.Name = I18n.Puzzle_IslandShrine_Title();
            this.Description = null;
            this.Type = null;
            this.ShowPuzzleSolutions = showPuzzleSolutions;
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // island shrine puzzle
            {
                IslandShrine shrine = (IslandShrine)this.Location;
                bool complete = shrine.puzzleFinished.Value;

                if (!this.ShowPuzzleSolutions && !complete)
                    yield return new GenericField(I18n.Puzzle_Solution(), new FormattedText(I18n.Puzzle_Solution_Hidden(), Color.Gray));
                else
                {
                    CheckboxList checkboxList = new CheckboxList(
                        new Checkbox(
                            text: I18n.Puzzle_IslandShrine_Solution_North(shrine.northPedestal.requiredItem.Value.DisplayName),
                            value: complete || shrine.northPedestal.match.Value
                        ),
                        new Checkbox(
                            text: I18n.Puzzle_IslandShrine_Solution_East(shrine.eastPedestal.requiredItem.Value.DisplayName),
                            value: complete || shrine.eastPedestal.match.Value
                        ),
                        new Checkbox(
                            text: I18n.Puzzle_IslandShrine_Solution_South(shrine.southPedestal.requiredItem.Value.DisplayName),
                            value: complete || shrine.southPedestal.match.Value
                        ),
                        new Checkbox(
                            text: I18n.Puzzle_IslandShrine_Solution_West(shrine.westPedestal.requiredItem.Value.DisplayName),
                            value: complete || shrine.westPedestal.match.Value
                        )
                    );
                    checkboxList.AddIntro(complete
                        ? I18n.Puzzle_Solution_Solved()
                        : I18n.Puzzle_IslandShrine_Solution()
                    );
                    var field = new CheckboxListField(I18n.Puzzle_Solution(), checkboxList);

                    

                    yield return field;
                }
            }

            // raw map data
            foreach (ICustomField field in base.GetData())
                yield return field;
        }
    }
}
