using hyjiacan.py4n;
using InnoVault;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CheaterPinYin
{
    public class CheaterPinYin : Mod
	{
        internal CheaterPinYin Instance => (CheaterPinYin)ModLoader.GetMod("CheaterPinYin");
        internal Mod cheatSheet {  get; private set; }
        internal Mod heros { get; private set; }

        internal Type itemBrowserType;
        internal FieldInfo itemView_FieldInfo;
        internal FieldInfo textbox_FieldInfo;
        internal delegate void textbox_KeyPressed_Delegate(object obj, object sender, char key);

        public override void Load() {
            FindMod();
            if (cheatSheet != null) {
                LoadItemBrowserInfo_InCheatSheet();
                LoadHook_ItemBrowser_textbox_KeyPressed();
            }
        }

        private void LoadHook_ItemBrowser_textbox_KeyPressed() {
            MethodInfo method = itemBrowserType.GetMethod("textbox_KeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) {
                MonoModHooks.Add(method, on_textbox_KeyPressed_Hook);
            }
            else {
                Instance.Logger.Info("LoadHook_ItemBrowser_textbox_KeyPressed:itemBrowserType.GetMethod Value is Null, textbox_KeyPressed is error?");
            }
        }

        internal void LoadItemBrowserInfo_InCheatSheet() {
            itemBrowserType = null;

            Type[] types = VaultUtils.GetAnyModCodeType();
            foreach (Type type in types) {
                if (type.FullName == "CheatSheet.Menus.ItemBrowser") {
                    itemBrowserType = type;
                }
            }

            if (itemBrowserType != null) {
                itemView_FieldInfo = itemBrowserType.GetField("itemView", BindingFlags.NonPublic | BindingFlags.Instance);
                textbox_FieldInfo = itemBrowserType.GetField("textbox", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        internal string GetTextbox_Text_Property(object obj) {
            object textbox_Instance = textbox_FieldInfo.GetValue(obj);
            Type textbox_Type = textbox_Instance.GetType();

            PropertyInfo text_Property = textbox_Type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);

            return (string)text_Property.GetValue(textbox_Instance);
        }

        internal int[] Get_activeSlots_Field(object obj) {
            object itemView_Instance = itemView_FieldInfo.GetValue(obj);
            Type itemView_Type = itemView_Instance.GetType();

            FieldInfo activeSlots_FieldInfo = itemView_Type.GetField("activeSlots", BindingFlags.Public | BindingFlags.Instance);

            return (int[])activeSlots_FieldInfo.GetValue(itemView_Instance);
        }

        internal void Set_activeSlots_Field(object obj, int[] newList) {
            object itemView_Instance = itemView_FieldInfo.GetValue(obj);
            Type itemView_Type = itemView_Instance.GetType();

            FieldInfo activeSlots_FieldInfo = itemView_Type.GetField("activeSlots", BindingFlags.Public | BindingFlags.Instance);

            activeSlots_FieldInfo.SetValue(itemView_Instance, newList);
        }

        internal int[] Get_selectedCategory_Property(object obj) {
            object itemView_Instance = itemView_FieldInfo.GetValue(obj);
            Type itemView_Type = itemView_Instance.GetType();

            PropertyInfo selectedCategory_Property = itemView_Type.GetProperty("selectedCategory", BindingFlags.Public | BindingFlags.Instance);

            return (int[])selectedCategory_Property.GetValue(itemView_Instance);
        }

        internal void Run_itemView_ReorderSlots_MethodValue(object obj) {
            object itemView_Instance = itemView_FieldInfo.GetValue(obj);
            Type itemView_Type = itemView_Instance.GetType();

            MethodInfo method = itemView_Type.GetMethod("ReorderSlots", BindingFlags.Public | BindingFlags.Instance);
            method.Invoke(itemView_Instance, null);
        }

        internal string Get_Slot_ItemName(object obj, int index) {
            object itemView_Instance = itemView_FieldInfo.GetValue(obj);
            Type itemView_Type = itemView_Instance.GetType();

            FieldInfo allItemsSlots_FieldInfo = itemView_Type.GetField("allItemsSlots", BindingFlags.Public | BindingFlags.Instance);

            object allItemsSlots_Value = allItemsSlots_FieldInfo.GetValue(itemView_Instance);

            string targetItemName = "";

            if (allItemsSlots_Value is Array array) {
                object slot_Instance = array.GetValue(index);
                FieldInfo slot_Item_FieldInfo = slot_Instance.GetType().GetField("item", BindingFlags.Public | BindingFlags.Instance);
                Item slot_Item_Instance = (Item)slot_Item_FieldInfo.GetValue(slot_Instance);
                Type itemType = slot_Item_Instance.GetType();

                targetItemName = slot_Item_Instance.Name.ToLower();
            }

            return targetItemName;
        }

        private void on_textbox_KeyPressed_Hook(textbox_KeyPressed_Delegate orig, object obj, object sender, char key) {
            string textbox_TextValue = GetTextbox_Text_Property(obj);

            int[] itemView_activeSlotsValue = Get_activeSlots_Field(obj);
            int[] itemView_selectedCategoryValue = Get_selectedCategory_Property(obj);

            if (textbox_TextValue.Length <= 0) {
                itemView_activeSlotsValue = Get_selectedCategory_Property(obj);
                Run_itemView_ReorderSlots_MethodValue(obj);
                return;
            }

            List<int> list = new List<int>();
            int[] category = Get_selectedCategory_Property(obj);
            string userInput = textbox_TextValue.ToLower();

            PinyinFormat pinyinFormat = PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE;

            // 获取用户输入的拼音
            string userInputPinyin = Pinyin4Net.GetPinyin(userInput, pinyinFormat, false, false, false).ToLower().Replace(" ", "");

            for (int i = 0; i < category.Length; i++) {
                int num = category[i];
                string itemName = Get_Slot_ItemName(obj, num);

                // 比较物品名称和输入内容以及它们的拼音
                if (ItemMatching(itemName, userInput, userInputPinyin)) {
                    list.Add(num);
                }
            }

            if (list.Count > 0) {
                Set_activeSlots_Field(obj, list.ToArray());
                Run_itemView_ReorderSlots_MethodValue(obj);
                return;
            }

            if (textbox_TextValue.Length > 15) {
                object textbox_Instance = textbox_FieldInfo.GetValue(obj);
                Type textbox_Type = textbox_Instance.GetType();

                PropertyInfo text_Property = textbox_Type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);

                text_Property.SetValue(textbox_Instance, textbox_TextValue.Substring(0, textbox_TextValue.Length - 1));
            }
        }

        private static bool ItemMatching(string itemName, string userInput, string userInputPinyin) {
            // 先检查物品名称是否直接匹配用户输入
            if (itemName.IndexOf(userInput, StringComparison.Ordinal) != -1) {
                return true;
            }

            // 获取物品名称的拼音
            string itemNamePinyin = Pinyin4Net.GetPinyin(itemName, PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE, false, false, false).ToLower().Replace(" ", "");

            // 逐字匹配：检查物品名称的拼音是否与用户输入的拼音逐字匹配
            if (itemNamePinyin.Length < userInputPinyin.Length) {
                // 如果物品名称拼音比用户输入短，直接返回false
                return false;
            }

            // 逐字匹配的核心逻辑
            for (int i = 0; i <= itemNamePinyin.Length - userInputPinyin.Length; i++) {
                if (itemNamePinyin.Substring(i, userInputPinyin.Length) == userInputPinyin) {
                    return true;
                }
            }

            return false;
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
