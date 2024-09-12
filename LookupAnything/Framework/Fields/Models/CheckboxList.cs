using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    using Checkbox = KeyValuePair<IFormattedText[], bool>;

    internal class CheckboxList
    {
        /// <summary>The checkbox values to display.</summary>
        public Checkbox[] Checkboxes;

        /// <summary>The intro text to show before the checkboxes.</summary>
        public IFormattedText[]? Intro;

        public CheckboxList(IEnumerable<Checkbox> checkboxes)
        {
            this.Checkboxes = checkboxes.ToArray();
        }

        public CheckboxList(params Checkbox[] checkboxes)
        {
            this.Checkboxes = checkboxes;
        }

        /// <summary>Add intro text before the checkboxes.</summary>
        /// <param name="text">The text to show before the checkboxes.</param>
        public CheckboxList AddIntro(params IFormattedText[] text)
        {
            this.Intro = text;
            return this;
        }

        /// <summary>Add intro text before the checkboxes.</summary>
        /// <param name="text">The text to show before the checkboxes.</param>
        public CheckboxList AddIntro(params string[] text)
        {
            return this.AddIntro(
                text.Select(p => (IFormattedText)new FormattedText(p)).ToArray()
            );
        }

        /// <summary>Build a checkbox entry.</summary>
        /// <param name="value">Whether the value is enabled.</param>
        /// <param name="text">The checkbox text to display.</param>
        public static Checkbox Checkbox(bool value, params IFormattedText[] text)
        {
            return new Checkbox(text, value);
        }

        /// <summary>Build a checkbox entry.</summary>
        /// <param name="value">Whether the value is enabled.</param>
        /// <param name="text">The checkbox text to display.</param>
        public static Checkbox Checkbox(bool value, string text)
        {
            return CheckboxList.Checkbox(value, new FormattedText(text));
        }
    }
}
