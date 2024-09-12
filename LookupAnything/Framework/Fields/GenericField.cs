using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

/// <summary>A generic metadata field shown as an extended property in the lookup UI.</summary>
internal class GenericField : ICustomField
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Label { get; protected set; }

    /// <inheritdoc />
    public LinkField? ExpandLink { get; protected set; }

    /// <inheritdoc />
    public IFormattedText[]? Value { get; protected set; }

    /// <inheritdoc />
    public bool HasValue { get; protected set; }


    /*********
    ** Fields
    *********/
    /// <summary>The number of pixels between an item's icon and text.</summary>
    protected readonly int IconMargin = 5;

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericField(string label, string? value, bool? hasValue = null)
    {
        this.Label = label;
        this.Value = this.FormatValue(value);
        this.HasValue = hasValue ?? this.Value?.Any() == true;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericField(string label, IFormattedText value, bool? hasValue = null)
        : this(label, new[] { value }, hasValue) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="hasValue">Whether the field should be displayed (or <c>null</c> to check the <paramref name="value"/>).</param>
    public GenericField(string label, IEnumerable<IFormattedText> value, bool? hasValue = null)
    {
        this.Label = label;
        this.Value = value.ToArray();
        this.HasValue = hasValue ?? this.Value?.Any() == true;
    }

    /// <summary>Draw the value (or return <c>null</c> to render the <see cref="Value"/> using the default format).</summary>
    /// <param name="spriteBatch">The sprite batch being drawn.</param>
    /// <param name="font">The recommended font.</param>
    /// <param name="position">The position at which to draw.</param>
    /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
    /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="Value"/> using the default format.</returns>
    public virtual Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
    {
        return null;
    }

    /// <summary>Collapse the field content into an expandable link if it contains at least the given number of results.</summary>
    /// <param name="minResultsForCollapse">The minimum results needed before the field is collapsed.</param>
    /// <param name="countForLabel">The total number of results represented by the content (including grouped entries like "11 unrevealed items").</param>
    public virtual void CollapseIfLengthExceeds(int minResultsForCollapse, int countForLabel)
    {
        if (this.Value?.Length >= minResultsForCollapse)
        {
            this.CollapseByDefault(I18n.Generic_ShowXResults(count: countForLabel));
        }
    }


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="label">A short field label.</param>
    /// <param name="hasValue">Whether the field should be displayed.</param>
    protected GenericField(string label, bool hasValue = false)
        : this(label, null as string, hasValue) { }

    /// <summary>Draw text with an icon.</summary>
    /// <param name="batch">The sprite batch.</param>
    /// <param name="font">The sprite font.</param>
    /// <param name="position">The position at which to draw the text.</param>
    /// <param name="absoluteWrapWidth">The width at which to wrap the text.</param>
    /// <param name="text">The block of text to write.</param>
    /// <param name="textColor">The text color.</param>
    /// <param name="icon">The sprite to draw.</param>
    /// <param name="iconSize">The size to draw.</param>
    /// <param name="iconColor">The color to tint the sprite.</param>
    /// <param name="qualityIcon">The quality for which to draw an icon over the sprite.</param>
    /// <param name="probe">Whether to calculate the positions without actually drawing anything to the screen.</param>
    /// <returns>Returns the drawn size.</returns>
    protected Vector2 DrawIconText(SpriteBatch batch, SpriteFont font, Vector2 position, float absoluteWrapWidth, string text, Color textColor, SpriteInfo? icon = null, Vector2? iconSize = null, Color? iconColor = null, int? qualityIcon = null, bool probe = false)
    {
        // draw icon
        int textOffset = 0;
        if (icon != null && iconSize.HasValue)
        {
            if (!probe)
                batch.DrawSpriteWithin(icon, position.X, position.Y, iconSize.Value, iconColor);
            textOffset = this.IconMargin;
        }
        else
            iconSize = Vector2.Zero;

        // draw quality icon overlay
        if (qualityIcon > 0 && iconSize is { X: > 0, Y: > 0 })
        {
            Rectangle qualityRect = qualityIcon < SObject.bestQuality ? new(338 + (qualityIcon.Value - 1) * 8, 400, 8, 8) : new(346, 392, 8, 8); // from Item.DrawMenuIcons
            Texture2D qualitySprite = Game1.mouseCursors;

            Vector2 qualitySize = iconSize.Value / 2;
            Vector2 qualityPos = new Vector2(
                position.X + iconSize.Value.X - qualitySize.X,
                position.Y + iconSize.Value.Y - qualitySize.Y
            );

            batch.DrawSpriteWithin(qualitySprite, qualityRect, qualityPos.X, qualityPos.Y, qualitySize, iconColor);
        }


        // draw text
        Vector2 textSize = probe
            ? font.MeasureString(text)
            : batch.DrawTextBlock(font, text, position + new Vector2(iconSize.Value.X + textOffset, 0), absoluteWrapWidth - position.X, textColor);

        // get drawn size
        return new Vector2(
            x: iconSize.Value.X + textSize.X,
            y: Math.Max(iconSize.Value.Y, textSize.Y)
        );
    }

    /// <summary>Wrap text into a list of formatted snippets.</summary>
    /// <param name="value">The text to wrap.</param>
    protected IFormattedText[] FormatValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? [new FormattedText(value)]
            : [];
    }

    /// <summary>Collapse the field by default, so the user needs to click a link to expand it.</summary>
    /// <param name="linkText">The link text to show.</param>
    protected void CollapseByDefault(string linkText)
    {
        this.ExpandLink = new LinkField(this.Label, linkText, () =>
        {
            this.ExpandLink = null;
            return null;
        });
    }

    /// <summary>Get the display value for sale price data.</summary>
    /// <param name="saleValue">The flat sale price.</param>
    /// <param name="stackSize">The number of items in the stack.</param>
    public static string? GetSaleValueString(int saleValue, int stackSize)
    {
        return GenericField.GetSaleValueString(new Dictionary<ItemQuality, int> { [ItemQuality.Normal] = saleValue }, stackSize);
    }

    /// <summary>Get the display value for sale price data.</summary>
    /// <param name="saleValues">The sale price data.</param>
    /// <param name="stackSize">The number of items in the stack.</param>
    public static string? GetSaleValueString(IDictionary<ItemQuality, int>? saleValues, int stackSize)
    {
        // can't be sold
        if (saleValues == null || !saleValues.Any() || saleValues.Values.All(p => p == 0))
            return null;

        // one quality
        if (saleValues.Count == 1)
        {
            string result = I18n.Generic_Price(price: saleValues.First().Value);
            if (stackSize > 1 && stackSize <= Constant.MaxStackSizeForPricing)
                result += $" ({I18n.Generic_PriceForStack(price: saleValues.First().Value * stackSize, count: stackSize)})";
            return result;
        }

        // prices by quality
        List<string> priceStrings = [];
        for (ItemQuality quality = ItemQuality.Normal; ; quality = quality.GetNext())
        {
            if (saleValues.ContainsKey(quality))
            {
                priceStrings.Add(quality == ItemQuality.Normal
                    ? I18n.Generic_Price(price: saleValues[quality])
                    : I18n.Generic_PriceForQuality(price: saleValues[quality], quality: I18n.For(quality))
                );
            }

            if (quality.GetNext() == quality)
                break;
        }
        return I18n.List(priceStrings);
    }
}
