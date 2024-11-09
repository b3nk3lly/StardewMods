using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>
    /// A list of checkboxes with labels. The list may optionally contain intro text with
    /// an icon.
    /// </summary>
    /// <param name="checkboxes">The checkbox values to display.</param>
    internal class CheckboxList(CheckboxList.Checkbox[] checkboxes, bool isHidden = false)
    {
        /// <summary>The text and icon to display above a list of checkboxes.</summary>
        /// <param name="text">The text to display above the checkboxes.</param>
        /// <param name="icon">The icon to display above the checkboxes.</param>
        internal class Intro(string text, SpriteInfo? icon = null)
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The text to display above the checkboxes.</summary>
            public string Text { get; } = text;

            /// <summary>The icon to display above the checkboxes.</summary>
            public SpriteInfo? Icon { get; } = icon;
        }

        /// <summary>A checkbox with a label.</summary>
        /// <param name="isChecked">Whether the checkbox is checked.</param>
        /// <param name="text">The text to display next to the checkbox.</param>
        internal class Checkbox(bool isChecked, params IFormattedText[] text)
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The text to display next to the checkbox.</summary>
            public IFormattedText[] Text = text;

            /// <summary>Whether the checkbox is checked.</summary>
            public bool IsChecked = isChecked;

            /// <summary>Construct an instance.</summary>
            /// <param name="isChecked">Whether the checkbox is checked.</param>
            /// <param name="text">The text to display next to the checkbox.</param>
            public Checkbox(bool isChecked, string text) : this(isChecked, new FormattedText(text))
            {
            }
        }

        /*********
        ** Accessors
        *********/
        /// <summary>The checkbox values to display.</summary>
        public Checkbox[] Checkboxes { get; } = checkboxes;

        /// <summary>Whether to hide the list when drawing (e.g., when using progression mode)</summary>
        public bool IsHidden { get; } = isHidden;

        /// <summary>The intro text to show before the checkboxes.</summary>
        public Intro? IntroData { get; set; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="checkboxes">The checkbox values to display.</param>
        public CheckboxList(IEnumerable<Checkbox> checkboxes) : this(checkboxes.ToArray())
        {
        }

        /// <summary>Add text and an icon before the checkboxes.</summary>
        /// <param name="text">The text to show before the checkboxes.</param>
        public CheckboxList AddIntro(string text, SpriteInfo? icon = null)
        {
            this.IntroData = new Intro(text, icon);
            return this;
        }
    }
}
