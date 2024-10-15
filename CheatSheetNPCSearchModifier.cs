using hyjiacan.py4n;
using InnoVault;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static CheaterPinYin.CheaterPinYin;

namespace CheaterPinYin
{
    internal class CheatSheetNPCSearchModifier
    {
        internal static CheatSheetNPCSearchModifier Instance;

        #region NPCBrowser
        internal static Type npcBrowserType;
        internal static FieldInfo npcView_FieldInfo;
        internal static FieldInfo npc_textbox_FieldInfo;

        internal static object textbox_Instance { get; private set; }
        internal static Type textbox_Type { get; private set; }
        internal static PropertyInfo textbox_Text_Property { get; private set; }

        internal static object npcView_Instance { get; private set; }
        internal static Type npcView_Type { get; private set; }
        internal static FieldInfo activeSlots_FieldInfo { get; private set; }
        internal static FieldInfo allNPCSlot_FieldInfo { get; private set; }
        internal static Array allNPCSlot_ArrayValue { get; private set; }
        internal static PropertyInfo selectedCategory_Property { get; private set; }
        internal static MethodInfo reorderSlots_Method { get; private set; }

        internal static Type slot_Type { get; private set; }
        //internal static FieldInfo slot_NPC_FieldInfo { get; private set; }
        internal static FieldInfo slot_DisplayName_FieldInfo { get; private set; }
        #endregion

        public static void Unload() {
            npcBrowserType = null;
            npcView_FieldInfo = null;
            npc_textbox_FieldInfo = null;
            textbox_Instance = null;
            textbox_Type = null;
            textbox_Text_Property = null;
            npcView_Instance = null;
            npcView_Type = null;
            activeSlots_FieldInfo = null;
            allNPCSlot_FieldInfo = null;
            allNPCSlot_ArrayValue = null;
            selectedCategory_Property = null;
            reorderSlots_Method = null;
            slot_Type = null;
            slot_DisplayName_FieldInfo = null;
            CheatSheetNPCSearchModifier.Instance = null;
        }

        internal void LoadNPCBrowserInfo() {
            npcBrowserType = null;

            Type[] types = VaultUtils.GetAnyModCodeType();
            foreach (Type type in types) {
                if (type.FullName == "CheatSheet.Menus.NPCBrowser") {
                    npcBrowserType = type;
                }
            }

            if (npcBrowserType != null) {
                //NPCBrowser.npcView是公开的
                npcView_FieldInfo = npcBrowserType.GetField("npcView", BindingFlags.Public | BindingFlags.Instance);
                npc_textbox_FieldInfo = npcBrowserType.GetField("textbox", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        internal void LoadHook_NPCBrowser_textbox_KeyPressed() {
            MethodInfo method = npcBrowserType.GetMethod("textbox_KeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) {
                MonoModHooks.Add(method, on_textbox_KeyPressed_Hook);
            }
            else {
                CheaterPinYin.Instance.Logger.Info("LoadHook_NPCBrowser_textbox_KeyPressed:npcBrowserType.GetMethod Value is Null, textbox_KeyPressed is error?");
            }
        }

        private void LoadTextboxInfo(object obj) {
            if (textbox_Instance == null) {
                textbox_Instance = npc_textbox_FieldInfo.GetValue(obj);
            }
            if (textbox_Type == null) {
                textbox_Type = textbox_Instance.GetType();
            }
            if (textbox_Text_Property == null) {
                textbox_Text_Property = textbox_Type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal string GetTextbox_Text_Property(object obj) => (string)textbox_Text_Property.GetValue(textbox_Instance);

        private void LoadNPCViewInfo(object obj) {
            if (npcView_Instance == null) {
                npcView_Instance = npcView_FieldInfo.GetValue(obj);
            }
            if (npcView_Type == null) {
                npcView_Type = npcView_Instance.GetType();
            }
            if (activeSlots_FieldInfo == null) {
                activeSlots_FieldInfo = npcView_Type.GetField("activeSlots", BindingFlags.Public | BindingFlags.Instance);
            }
            if (allNPCSlot_FieldInfo == null) {
                allNPCSlot_FieldInfo = npcView_Type.GetField("allNPCSlot", BindingFlags.Public | BindingFlags.Instance);
            }
            if (allNPCSlot_ArrayValue == null) {
                allNPCSlot_ArrayValue = (Array)allNPCSlot_FieldInfo.GetValue(npcView_Instance);
            }
            if (selectedCategory_Property == null) {
                selectedCategory_Property = npcView_Type.GetProperty("selectedCategory", BindingFlags.Public | BindingFlags.Instance);
            }
            if (reorderSlots_Method == null) {
                reorderSlots_Method = npcView_Type.GetMethod("ReorderSlots", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal int[] Get_activeSlots_Field(object obj) => (int[])activeSlots_FieldInfo.GetValue(npcView_Instance);

        internal void Set_activeSlots_Field(object obj, int[] newList) => activeSlots_FieldInfo.SetValue(npcView_Instance, newList);

        internal int[] Get_selectedCategory_Property(object obj) => (int[])selectedCategory_Property.GetValue(npcView_Instance);

        internal void Run_NPCView_ReorderSlots_MethodValue(object obj) => reorderSlots_Method.Invoke(npcView_Instance, null);

        private void LoadSlotInfo(object slot_Instance) {
            if (slot_Type == null) {
                slot_Type = slot_Instance.GetType();
            }
            //if (slot_NPC_FieldInfo == null) {
            //    slot_NPC_FieldInfo = slot_Instance.GetType().GetField("item", BindingFlags.Public | BindingFlags.Instance);
            //}
            if (slot_DisplayName_FieldInfo == null) {
                slot_DisplayName_FieldInfo = slot_Type.GetField("displayName", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        //性能开销主要来自于这个函数中的反射操作，如何降低这个函数的调用复杂度是重中之重
        internal string Get_Slot_ItemName(object obj, int index) {
            object slot_Instance = allNPCSlot_ArrayValue.GetValue(index);
            LoadSlotInfo(slot_Instance);
            return ((LocalizedText)slot_DisplayName_FieldInfo.GetValue(slot_Instance)).Value.ToLower();
        }

        internal void HanderStrLengs(object obj, string textbox_TextValue) {
            if (textbox_TextValue.Length >= 15) {
                textbox_Text_Property.SetValue(textbox_Instance, textbox_TextValue.Substring(0, textbox_TextValue.Length - 1));
            }
        }

        private void on_textbox_KeyPressed_Hook(textbox_KeyPressed_Delegate orig, object obj, object sender, char key) {
            LoadTextboxInfo(obj);

            string textbox_TextValue = GetTextbox_Text_Property(obj);

            LoadNPCViewInfo(obj);

            int[] npcView_activeSlotsValue = Get_activeSlots_Field(obj);
            int[] npcView_selectedCategoryValue = Get_selectedCategory_Property(obj);

            if (textbox_TextValue.Length <= 0) {
                npcView_activeSlotsValue = Get_selectedCategory_Property(obj);
                Run_NPCView_ReorderSlots_MethodValue(obj);
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
                Run_NPCView_ReorderSlots_MethodValue(obj);
                return;
            }

            HanderStrLengs(obj, textbox_TextValue);
        }
    }
}
