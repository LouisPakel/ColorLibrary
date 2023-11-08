using System;
using System.Collections.Generic;
using UnityEngine;

namespace Library
{
    /// <summary>
    /// ColorLibrary stores common colors, for Theming or else.
    /// </summary>
    [CreateAssetMenu(menuName = "Catalyst/Libraries/ColorLibrary", fileName = nameof(ColorLibrary) + ".asset")]
    public class ColorLibrary : Library<Color>
    {
        protected override Color DefaultValue() => Color.yellow;
        public override ISet<Type> AuthorizedEnums()
        {
            return new HashSet<Type>
            {
                typeof(Theme)
            };
        }
    }
}
