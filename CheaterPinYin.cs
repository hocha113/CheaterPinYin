using Terraria.ModLoader;

namespace CheaterPinYin
{
    public class CheaterPinYin : Mod
	{
        internal delegate void textbox_KeyPressed_Delegate(object obj, object sender, char key);

        internal static CheaterPinYin Instance => (CheaterPinYin)ModLoader.GetMod("CheaterPinYin");

        internal static Mod cheatSheet {  get; private set; }
        internal static Mod heros { get; private set; }

        public override void Load() {
            FindMod();
            CheatSheetItemSearchModifier.Instance = new CheatSheetItemSearchModifier();
            CheatSheetNPCSearchModifier.Instance = new CheatSheetNPCSearchModifier();
            CheatSheetRecipeSearchModifier.Instance = new CheatSheetRecipeSearchModifier();
            HEROsSearchModifier.Instance = new HEROsSearchModifier();
            if (cheatSheet != null) {
                CheatSheetItemSearchModifier.Instance.LoadItemBrowserInfo();
                CheatSheetItemSearchModifier.Instance.LoadHook_ItemBrowser_textbox_KeyPressed();
                CheatSheetNPCSearchModifier.Instance.LoadNPCBrowserInfo();
                CheatSheetNPCSearchModifier.Instance.LoadHook_NPCBrowser_textbox_KeyPressed();
                CheatSheetRecipeSearchModifier.Instance.LoadRecipeBrowserInfo();
                CheatSheetRecipeSearchModifier.Instance.LoadHook_RecipeBrowser_textbox_KeyPressed();
            }
        }

        public override void Unload() {
            cheatSheet = null;
            heros = null;
            CheatSheetItemSearchModifier.Unload();
            CheatSheetNPCSearchModifier.Unload();
            CheatSheetRecipeSearchModifier.Unload();
            HEROsSearchModifier.UnLoad();
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
