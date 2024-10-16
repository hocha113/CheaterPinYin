﻿using hyjiacan.py4n;
using static CheaterPinYin.CheaterPinYin;
using System.Collections.Generic;
using System.Reflection;
using System;
using InnoVault;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using System.Text.RegularExpressions;

namespace CheaterPinYin
{
    internal class CheatSheetRecipeSearchModifier
    {
        internal static CheatSheetRecipeSearchModifier Instance;

        #region RecipeBrowser
        internal static Type recipeBrowserWindowType;
        internal static FieldInfo recipeView_FieldInfo;
        internal static FieldInfo recipe_textbox_FieldInfo;

        internal static object textbox_Instance { get; private set; }
        internal static Type textbox_Type { get; private set; }
        internal static PropertyInfo textbox_Text_Property { get; private set; }

        internal static object recipeView_Instance { get; private set; }
        internal static Type recipeView_Type { get; private set; }
        internal static FieldInfo activeSlots_FieldInfo { get; private set; }
        internal static FieldInfo allRecipeSlot_FieldInfo { get; private set; }
        internal static Array allRecipeSlot_ArrayValue { get; private set; }
        internal static PropertyInfo selectedCategory_Property { get; private set; }
        internal static MethodInfo reorderSlots_Method { get; private set; }

        internal static Type slot_Type { get; private set; }
        internal static FieldInfo slot_Recipe_FieldInfo { get; private set; }
        #endregion

        public static void Unload() {
            recipeBrowserWindowType = null;
            recipeView_FieldInfo = null;
            recipe_textbox_FieldInfo = null;
            textbox_Instance = null;
            textbox_Type = null;
            textbox_Text_Property = null;
            recipeView_Instance = null;
            recipeView_Type = null;
            activeSlots_FieldInfo = null;
            allRecipeSlot_FieldInfo = null;
            allRecipeSlot_ArrayValue = null;
            selectedCategory_Property = null;
            reorderSlots_Method = null;
            slot_Type = null;
            slot_Recipe_FieldInfo = null;
            CheatSheetRecipeSearchModifier.Instance = null;
        }

        internal void LoadHook() {
            LoadRecipeBrowserInfo();
            LoadHook_RecipeBrowser_textbox_KeyPressed();
        }

        internal void LoadRecipeBrowserInfo() {
            recipeBrowserWindowType = null;

            Type[] types = VaultUtils.GetAnyModCodeType();
            foreach (Type type in types) {
                if (type.FullName == "CheatSheet.Menus.RecipeBrowserWindow") {
                    recipeBrowserWindowType = type;
                }
            }

            if (recipeBrowserWindowType != null) {
                recipeView_FieldInfo = recipeBrowserWindowType.GetField("recipeView", BindingFlags.NonPublic | BindingFlags.Static);
                recipe_textbox_FieldInfo = recipeBrowserWindowType.GetField("textbox", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        internal void LoadHook_RecipeBrowser_textbox_KeyPressed() {
            MethodInfo method = recipeBrowserWindowType.GetMethod("textbox_KeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) {
                MonoModHooks.Add(method, on_textbox_KeyPressed_Hook);
            }
            else {
                CheaterPinYin.Instance.Logger.Info("LoadHook_RecipeBrowser_textbox_KeyPressed:recipeBrowserWindowType.GetMethod Value is Null, textbox_KeyPressed is error?");
            }
        }

        private void LoadTextboxInfo(object obj) {
            if (textbox_Instance == null) {
                textbox_Instance = recipe_textbox_FieldInfo.GetValue(obj);
            }
            if (textbox_Type == null) {
                textbox_Type = textbox_Instance.GetType();
            }
            if (textbox_Text_Property == null) {
                textbox_Text_Property = textbox_Type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal string GetTextbox_Text_Property(object obj) => (string)textbox_Text_Property.GetValue(textbox_Instance);

        private void LoadRecipeViewInfo(object obj) {
            if (recipeView_Instance == null) {
                recipeView_Instance = recipeView_FieldInfo.GetValue(obj);
            }
            if (recipeView_Type == null) {
                recipeView_Type = recipeView_Instance.GetType();
            }
            if (activeSlots_FieldInfo == null) {
                activeSlots_FieldInfo = recipeView_Type.GetField("activeSlots", BindingFlags.Public | BindingFlags.Instance);
            }
            if (allRecipeSlot_FieldInfo == null) {
                allRecipeSlot_FieldInfo = recipeView_Type.GetField("allRecipeSlot", BindingFlags.Public | BindingFlags.Instance);
            }
            if (allRecipeSlot_ArrayValue == null) {
                allRecipeSlot_ArrayValue = (Array)allRecipeSlot_FieldInfo.GetValue(recipeView_Instance);
            }
            if (selectedCategory_Property == null) {
                selectedCategory_Property = recipeView_Type.GetProperty("selectedCategory", BindingFlags.Public | BindingFlags.Instance);
            }
            if (reorderSlots_Method == null) {
                reorderSlots_Method = recipeView_Type.GetMethod("ReorderSlots", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal int[] Get_activeSlots_Field(object obj) => (int[])activeSlots_FieldInfo.GetValue(recipeView_Instance);

        internal void Set_activeSlots_Field(object obj, int[] newList) => activeSlots_FieldInfo.SetValue(recipeView_Instance, newList);

        internal int[] Get_selectedCategory_Property(object obj) => (int[])selectedCategory_Property.GetValue(recipeView_Instance);

        internal void Run_recipeView_ReorderSlots_MethodValue(object obj) => reorderSlots_Method.Invoke(recipeView_Instance, null);

        private void LoadSlotInfo(object slot_Instance) {
            if (slot_Type == null) {
                slot_Type = slot_Instance.GetType();
            }
            if (slot_Recipe_FieldInfo == null) {
                slot_Recipe_FieldInfo = slot_Type.GetField("recipe", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        //性能开销主要来自于这个函数中的反射操作，如何降低这个函数的调用复杂度是重中之重
        internal string Get_Slot_ItemName(object obj, int index) {
            object slot_Instance = allRecipeSlot_ArrayValue.GetValue(index);
            LoadSlotInfo(slot_Instance);
            return ((Recipe)slot_Recipe_FieldInfo.GetValue(slot_Instance)).createItem.Name.ToLower();
        }

        internal void HanderStrLengs(object obj, string textbox_TextValue) {
            if (SearchUtility.GetActualLength(textbox_TextValue) >= CheaterPinYinConfig.Instance.CharactersMaxNum) {
                textbox_Text_Property.SetValue(textbox_Instance, textbox_TextValue.Substring(0, textbox_TextValue.Length - 1));
            }
        }

        private void on_textbox_KeyPressed_Hook(textbox_KeyPressed_Delegate orig, object obj, object sender, char key) {
            LoadTextboxInfo(obj);

            string textbox_TextValue = GetTextbox_Text_Property(obj);

            LoadRecipeViewInfo(obj);

            int[] npcView_activeSlotsValue = Get_activeSlots_Field(obj);
            int[] npcView_selectedCategoryValue = Get_selectedCategory_Property(obj);

            if (textbox_TextValue.Length <= 0) {
                npcView_activeSlotsValue = Get_selectedCategory_Property(obj);
                Run_recipeView_ReorderSlots_MethodValue(obj);
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
                Regex regex = SearchUtility.SafeGetRegex(userInput);
                for (int i = 0; i < category.Length; i++) {
                    int num = category[i];
                    string itemName = Get_Slot_ItemName(obj, num);

                    // 比较物品名称和输入内容以及它们的拼音
                    if (SearchUtility.ItemMatching(itemName, userInput, userInputPinyin, regex)) {
                        list.Add(num);
                    }
                }
            }

            if (list.Count > 0) {
                Set_activeSlots_Field(obj, list.ToArray());
                Run_recipeView_ReorderSlots_MethodValue(obj);
                return;
            }

            HanderStrLengs(obj, textbox_TextValue);
        }
    }
}
