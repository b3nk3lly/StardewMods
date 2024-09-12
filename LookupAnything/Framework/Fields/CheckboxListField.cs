using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of checkbox values.</summary>
    internal class CheckboxListField : GenericField
    {
        /*********
        ** Fields
        *********/

        protected CheckboxList[] CheckboxLists;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="checkboxLists">A list of checkbox labels and values to display.</param>
        public CheckboxListField(string label, params CheckboxList[] checkboxLists)
            : this(label)
        {
            this.CheckboxLists = checkboxLists;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            float topOffset = 0;
            float checkboxSize = CommonSprites.Icons.FilledCheckbox.Width * (Game1.pixelZoom / 2);
            float lineHeight = Math.Max(checkboxSize, Game1.smallFont.MeasureString("ABC").Y);
            float checkboxOffset = (lineHeight - checkboxSize) / 2;

            foreach (CheckboxList checkboxList in this.CheckboxLists)
            {
                if (checkboxList.IntroData != null)
                    topOffset += this.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, checkboxList.IntroData.Text, Color.Black, checkboxList.IntroData.Sprite, new Vector2(lineHeight)).Y;

                foreach (CheckboxList.Checkbox checkbox in checkboxList.Checkboxes)
                {
                    // draw icon
                    spriteBatch.Draw(
                        texture: CommonSprites.Icons.Sheet,
                        position: new Vector2(position.X, position.Y + topOffset + checkboxOffset),
                        sourceRectangle: checkbox.Value ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox,
                        color: Color.White,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: checkboxSize / CommonSprites.Icons.FilledCheckbox.Width,
                        effects: SpriteEffects.None,
                        layerDepth: 1f
                    );

                    // draw text
                    Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, checkbox.Text, new Vector2(position.X + checkboxSize + 7, position.Y + topOffset), wrapWidth - checkboxSize - 7);

                    // update offset for next checkbox
                    topOffset += Math.Max(checkboxSize, textSize.Y);
                }

                // update offset for next list
                topOffset += lineHeight;
            }

            return new Vector2(wrapWidth, topOffset);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        protected CheckboxListField(string label)
            : base(label, hasValue: true)
        {
            this.CheckboxLists = [];
        }
    }
}
