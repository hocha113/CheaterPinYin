using hyjiacan.py4n;
using InnoVault;
using static CheaterPinYin.CheaterPinYin;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;

namespace CheaterPinYin
{
    internal class CheatSheetItemSearchModifier
    {
        internal static CheatSheetItemSearchModifier Instance;

        #region ItemBrowser
        internal static Type itemBrowserType;
        internal static FieldInfo itemView_FieldInfo;
        internal static FieldInfo textbox_FieldInfo;

        internal static object textbox_Instance { get; private set; }
        internal static Type textbox_Type { get; private set; }
        internal static PropertyInfo textbox_Text_Property { get; private set; }

        internal static object itemView_Instance { get; private set; }
        internal static Type itemView_Type { get; private set; }
        internal static FieldInfo activeSlots_FieldInfo { get; private set; }
        internal static FieldInfo allItemsSlots_FieldInfo { get; private set; }
        internal static Array allItemsSlots_ArrayValue { get; private set; }
        internal static PropertyInfo selectedCategory_Property { get; private set; }
        internal static MethodInfo reorderSlots_Method { get; private set; }

        internal static Type slot_Type { get; private set; }
        internal static FieldInfo slot_Item_FieldInfo { get; private set; }
        #endregion

        public static void Unload() {
            itemBrowserType = null;
            itemView_FieldInfo = null;
            textbox_FieldInfo = null;
            textbox_Instance = null;
            textbox_Type = null;
            textbox_Text_Property = null;
            itemView_Instance = null;
            itemView_Type = null;
            activeSlots_FieldInfo = null;
            allItemsSlots_FieldInfo = null;
            allItemsSlots_ArrayValue = null;
            selectedCategory_Property = null;
            reorderSlots_Method = null;
            slot_Type = null;
            slot_Item_FieldInfo = null;
            CheatSheetItemSearchModifier.Instance = null;
        }

        internal void LoadItemBrowserInfo() {
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

        internal void LoadHook_ItemBrowser_textbox_KeyPressed() {
            MethodInfo method = itemBrowserType.GetMethod("textbox_KeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) {
                MonoModHooks.Add(method, on_textbox_KeyPressed_Hook);
            }
            else {
                CheaterPinYin.Instance.Logger.Info("LoadHook_ItemBrowser_textbox_KeyPressed:itemBrowserType.GetMethod Value is Null, textbox_KeyPressed is error?");
            }
        }

        private void LoadTextboxInfo(object obj) {
            if (textbox_Instance == null) {
                textbox_Instance = textbox_FieldInfo.GetValue(obj);
            }
            if (textbox_Type == null) {
                textbox_Type = textbox_Instance.GetType();
            }
            if (textbox_Text_Property == null) {
                textbox_Text_Property = textbox_Type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal string GetTextbox_Text_Property(object obj) {
            LoadTextboxInfo(obj);
            return (string)textbox_Text_Property.GetValue(textbox_Instance);
        }

        private void LoadItemViewInfo(object obj) {
            if (itemView_Instance == null) {
                itemView_Instance = itemView_FieldInfo.GetValue(obj);
            }
            if (itemView_Type == null) {
                itemView_Type = itemView_Instance.GetType();
            }
            if (activeSlots_FieldInfo == null) {
                activeSlots_FieldInfo = itemView_Type.GetField("activeSlots", BindingFlags.Public | BindingFlags.Instance);
            }
            if (allItemsSlots_FieldInfo == null) {
                allItemsSlots_FieldInfo = itemView_Type.GetField("allItemsSlots", BindingFlags.Public | BindingFlags.Instance);
            }
            if (allItemsSlots_ArrayValue == null) {
                allItemsSlots_ArrayValue = (Array)allItemsSlots_FieldInfo.GetValue(itemView_Instance);
            }
            if (selectedCategory_Property == null) {
                selectedCategory_Property = itemView_Type.GetProperty("selectedCategory", BindingFlags.Public | BindingFlags.Instance);
            }
            if (reorderSlots_Method == null) {
                reorderSlots_Method = itemView_Type.GetMethod("ReorderSlots", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal int[] Get_activeSlots_Field(object obj) {
            LoadItemViewInfo(obj);
            return (int[])activeSlots_FieldInfo.GetValue(itemView_Instance);
        }

        internal void Set_activeSlots_Field(object obj, int[] newList) {
            LoadItemViewInfo(obj);
            activeSlots_FieldInfo.SetValue(itemView_Instance, newList);
        }

        internal int[] Get_selectedCategory_Property(object obj) {
            LoadItemViewInfo(obj);
            return (int[])selectedCategory_Property.GetValue(itemView_Instance);
        }

        internal void Run_itemView_ReorderSlots_MethodValue(object obj) {
            LoadItemViewInfo(obj);
            reorderSlots_Method.Invoke(itemView_Instance, null);
        }

        private void LoadSlotInfo(object slot_Instance) {
            if (slot_Type == null) {
                slot_Type = slot_Instance.GetType();
            }
            if (slot_Item_FieldInfo == null) {
                slot_Item_FieldInfo = slot_Instance.GetType().GetField("item", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        //性能开销主要来自于这个函数中的反射操作，如何降低这个函数的调用复杂度是重中之重
        internal string Get_Slot_ItemName(object obj, int index) {
            LoadItemViewInfo(obj);
            object slot_Instance = allItemsSlots_ArrayValue.GetValue(index);
            LoadSlotInfo(slot_Instance);
            return ((Item)slot_Item_FieldInfo.GetValue(slot_Instance)).Name.ToLower();
        }

        internal void HanderStrLengs(object obj, string textbox_TextValue) {
            if (textbox_TextValue.Length >= 15) {
                LoadTextboxInfo(obj);
                textbox_Text_Property.SetValue(textbox_Instance, textbox_TextValue.Substring(0, textbox_TextValue.Length - 1));
            }
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

            string noAirInput = userInput.Replace(" ", "");

            PinyinFormat pinyinFormat = PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE;

            // 获取用户输入的拼音
            string userInputPinyin = Pinyin4Net.GetPinyin(userInput, pinyinFormat, false, false, false).ToLower().Replace(" ", "");

            if (noAirInput.Length > 0) {//如果输入的全是空格那么就别搜索来浪费性能了
                for (int i = 0; i < category.Length; i++) {
                    int num = category[i];
                    string itemName = Get_Slot_ItemName(obj, num);

                    // 比较物品名称和输入内容以及它们的拼音
                    if (SearchUtility.ItemMatching(itemName, userInput, userInputPinyin)) {
                        list.Add(num);
                    }
                }
            }

            if (list.Count > 0) {
                Set_activeSlots_Field(obj, list.ToArray());
                Run_itemView_ReorderSlots_MethodValue(obj);
                return;
            }

            HanderStrLengs(obj, textbox_TextValue);
        }
    }
}
