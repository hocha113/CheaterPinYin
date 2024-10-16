using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CheaterPinYin
{
    internal class CheaterPinYinConfig : ModConfig
    {
        public static CheaterPinYinConfig Instance { get; private set; }

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(35, 85, 158, 255)]
        [DefaultValue(false)]
        public bool RegularExpression { get; set; }

        [BackgroundColor(35, 85, 158, 255)]
        [SliderColor(224, 165, 56, 255)]
        [Range(5, 20)]
        [DefaultValue(16)]
        public int CharactersMaxNum;

        public override void OnLoaded() => Instance = this;
    }
}
