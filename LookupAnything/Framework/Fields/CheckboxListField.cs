using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A metadata field which shows a list of checkbox values.</summary>
internal class CheckboxListField : GenericField
{
    /*********
    ** Fields
    *********/
    /// <summary>The checkbox values to display.</summary>
    protected CheckboxList[] CheckboxLists;

    /// <summary>The size of each checkbox to draw.</summary>
    protected float CheckboxSize;

    /// <summary>The height of one line of the checkbox list.</summary>
    protected float LineHeight;


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

        foreach (CheckboxList checkboxList in this.CheckboxLists)
        {
            topOffset += this.DrawCheckboxList(checkboxList, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth).Y;
        }

        return new Vector2(wrapWidth, topOffset - this.LineHeight);
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
        this.CheckboxSize = CommonSprites.Icons.FilledCheckbox.Width * (Game1.pixelZoom / 2);
        this.LineHeight = Math.Max(this.CheckboxSize, Game1.smallFont.MeasureString("ABC").Y);
    }

    /// <summary>Draw a list of checkboxes.</summary>
    /// <param name="checkboxList">The list of checkboxes to draw.</param>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="font">The recommended font.</param>
    /// <param name="position">The position at which to draw.</param>
    /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
    protected Vector2 DrawCheckboxList(CheckboxList checkboxList, SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        float topOffset = 0;
        float checkboxOffset = (this.LineHeight - this.CheckboxSize) / 2;

        if (checkboxList.IntroData != null)
            topOffset += this.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, checkboxList.IntroData.Text, Color.Black, checkboxList.IntroData.Icon, new Vector2(this.LineHeight)).Y;

        foreach (CheckboxList.Checkbox checkbox in checkboxList.Checkboxes)
        {
            // draw icon
            spriteBatch.Draw(
                texture: CommonSprites.Icons.Sheet,
                position: new Vector2(position.X, position.Y + topOffset + checkboxOffset),
                sourceRectangle: checkbox.IsChecked ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: this.CheckboxSize / CommonSprites.Icons.FilledCheckbox.Width,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );

            // draw text
            Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, checkbox.Text, new Vector2(position.X + this.CheckboxSize + 7, position.Y + topOffset), wrapWidth - this.CheckboxSize - 7);

            // update offset for next checkbox
            topOffset += Math.Max(this.CheckboxSize, textSize.Y);
        }

        return new Vector2(position.X, topOffset);
    }
}
