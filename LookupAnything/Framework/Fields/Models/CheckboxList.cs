using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    internal class CheckboxList
    {
        internal class Intro
        {
            /*********
            ** Accessors
            *********/
            public string Text { get; }
            public SpriteInfo? Sprite { get; }

            public Intro(string text, SpriteInfo? sprite = null)
            {
                this.Text = text;
                this.Sprite = sprite;
            }
        }

        internal class Checkbox
        {
            /*********
            ** Accessors
            *********/
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

        /*********
        ** Accessors
        *********/
        /// <summary>The checkbox values to display.</summary>
        public Checkbox[] Checkboxes { get; }

        /// <summary>The intro text to show before the checkboxes.</summary>
        public Intro? IntroData { get; set; }

        public CheckboxList(IEnumerable<Checkbox> checkboxes) : this(checkboxes.ToArray())
        {
        }

        public CheckboxList(params Checkbox[] checkboxes)
        {
            this.Checkboxes = checkboxes;
        }

        /// <summary>Add intro text before the checkboxes.</summary>
        /// <param name="text">The text to show before the checkboxes.</param>
        public CheckboxList AddIntro(string text, SpriteInfo? sprite = null)
        {
            this.IntroData = new Intro(text, sprite);
            return this;
        }
    }
}
