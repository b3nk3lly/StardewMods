using System;
using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

/// <summary>A recipe model for a recipe list.</summary>
internal class RecipeEntry
{
    /*********
    ** Fields
    *********/
    /// <summary>The backing field for <see cref="UniqueKey"/>.</summary>
    private readonly Lazy<string> UniqueKeyImpl;


    /*********
    ** Accessors
    *********/
    /// <summary>The recipe name or key.</summary>
    public string? Name { get; }

    /// <summary>The recipe type.</summary>
    public string Type { get; }

    /// <summary>Whether the player knows the recipe.</summary>
    public bool IsKnown { get; }

    /// <summary>The input items.</summary>
    public RecipeItemEntry[] Inputs { get; }

    /// <summary>The output item.</summary>
    public RecipeItemEntry Output { get; }

    /// <summary>The game state queries which indicate when this recipe is available, if any.</summary>
    public string? Conditions { get; }

    /// <summary>A key which uniquely identifies the recipe by its combination of name, inputs, and outputs.</summary>
    public string UniqueKey => this.UniqueKeyImpl.Value;

    /// <summary>Whether all items involved in this recipe are valid.</summary>
    public bool IsValid { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="name">The recipe name or key.</param>
    /// <param name="type">The recipe type.</param>
    /// <param name="isKnown">Whether the player knows the recipe.</param>
    /// <param name="inputs">The input items.</param>
    /// <param name="output">The output item.</param>
    /// <param name="conditions">The game state queries which indicate when this recipe is available, if any.</param>
    public RecipeEntry(string? name, string type, bool isKnown, RecipeItemEntry[] inputs, RecipeItemEntry output, string? conditions)
    {
        this.Name = name;
        this.Type = type;
        this.IsKnown = isKnown;
        this.Inputs = inputs;
        this.Output = output;
        this.Conditions = conditions;
        this.UniqueKeyImpl = new Lazy<string>(() => RecipeEntry.GetUniqueKey(name, inputs, output));

        this.IsValid = output.IsValid && inputs.All(input => input.IsValid);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a key which uniquely identifies the recipe by its combination of name, inputs, and outputs.</summary>
    /// <param name="name">The recipe name or key.</param>
    /// <param name="inputs">The input items.</param>
    /// <param name="output">The output item.</param>
    private static string GetUniqueKey(string? name, RecipeItemEntry[] inputs, RecipeItemEntry output)
    {
        var inputNames = inputs
            .Select(item => item.DisplayText)
            .OrderBy(item => item);

        return string.Join(", ", inputNames.Concat([output.DisplayText, name]));
    }
}
