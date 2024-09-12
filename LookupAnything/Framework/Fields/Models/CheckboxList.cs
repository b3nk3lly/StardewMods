using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{

    internal class Intro
    {
        public IFormattedText[] Text;
        public SpriteInfo? Sprite;

        public Intro(IFormattedText[] text, SpriteInfo? sprite = null)
        {
            this.Text = text;
            this.Sprite = sprite;
        }
    }

    internal class Checkbox
    {
        public IFormattedText[] Text;
        public bool Value;

        public Checkbox(bool value, params IFormattedText[] text)
        {
            this.Text = text;
            this.Value = value;
        }

        public Checkbox(bool value, string text) : this(value, new FormattedText(text))
        {
        }
    }

    internal class CheckboxList
    {
        /// <summary>The checkbox values to display.</summary>
        public Checkbox[] Checkboxes;

        /// <summary>The intro text to show before the checkboxes.</summary>
        public Intro? Intro;

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
            this.Intro = new Intro(text);
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
    }
}
