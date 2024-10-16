using Terraria;
using Terraria.ModLoader;

namespace CheaterPinYin
{
    public class CheaterPinYin : Mod
	{
        internal delegate void textbox_KeyPressed_Delegate(object obj, object sender, char key);
        internal delegate Item[] getItem_Delegate(object obj);

        internal static CheaterPinYin Instance => (CheaterPinYin)ModLoader.GetMod("CheaterPinYin");

        internal static Mod cheatSheet {  get; private set; }
        internal static Mod heros { get; private set; }

        public override void Load() {
            FindMod();
            CheatSheetItemSearchModifier.Instance = new CheatSheetItemSearchModifier();
            CheatSheetNPCSearchModifier.Instance = new CheatSheetNPCSearchModifier();
            CheatSheetRecipeSearchModifier.Instance = new CheatSheetRecipeSearchModifier();
            HEROsItemSearchModifier.Instance = new HEROsItemSearchModifier();
            HEROsNPCSpawnerSearchModifier.Instance = new HEROsNPCSpawnerSearchModifier();
            if (cheatSheet != null) {
                CheatSheetItemSearchModifier.Instance.LoadHook();
                CheatSheetNPCSearchModifier.Instance.LoadHook();
                CheatSheetRecipeSearchModifier.Instance.LoadHook();
            }
            if (heros != null) {
                HEROsItemSearchModifier.Instance.LoadHook();
                //这个存在一个暂时没能解决的问题，所以在当前版本不进行挂载
                //HEROsNPCSpawnerSearchModifier.Instance.LoadHook();
            }
        }

        public override void Unload() {
            cheatSheet = null;
            heros = null;
            CheatSheetItemSearchModifier.Unload();
            CheatSheetNPCSearchModifier.Unload();
            CheatSheetRecipeSearchModifier.Unload();
            HEROsItemSearchModifier.UnLoad();
            HEROsNPCSpawnerSearchModifier.UnLoad();
        }

        internal void FindMod() {
            cheatSheet = null;
            if (ModLoader.TryGetMod("CheatSheet", out Mod _cheatSheet)) {
                cheatSheet = _cheatSheet;
            }

            heros = null;
            if (ModLoader.TryGetMod("HEROsMod", out Mod _heros)) {
                heros = _heros;
            }
        }
    }
}
