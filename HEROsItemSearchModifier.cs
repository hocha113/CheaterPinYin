using hyjiacan.py4n;
using InnoVault;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using static CheaterPinYin.CheaterPinYin;

namespace CheaterPinYin
{
    internal class HEROsItemSearchModifier
    {
        internal static HEROsItemSearchModifier Instance;

        internal static Type itemBrowserType;
        internal static MethodInfo getItems_Method;
        internal static MethodInfo textbox_KeyPressed_Method;

        internal static Type UIViewType;
        internal static FieldInfo availableSorts_Field;
        internal static Array availableSorts_ArrayValue;

        internal static Type slot_Type;
        internal static FieldInfo slot_button_Field;
        internal static PropertyInfo foregroundColor_Property;

        internal static PropertyInfo defaultSorts_Property;
        internal static Array defaultSorts_ArrayValue;
        internal static FieldInfo selectedSort_Field;
        internal static object selectedSort_Instance;
        internal static FieldInfo selectedSort_Sort_Field;

        internal static PropertyInfo selectedCategory_Property;
        internal static object selectedCategory_Instance;

        internal static PropertyInfo categories_Property;
        internal static Array categories_ArrayValue;

        internal static Type categories_Type;
        internal static MethodInfo categories_GetItems_Method;

        internal static FieldInfo searchBox_Field;
        internal static object searchBox_Instance;
        internal static PropertyInfo searchBox_Text_Property;

        internal static MethodInfo passFilters_Method;
        internal static Type myComparerType;

        internal static PropertyInfo currentItems_Property;

        public static void UnLoad() {
            itemBrowserType = null;
            getItems_Method = null;
            UIViewType = null;
            availableSorts_Field = null;
            availableSorts_ArrayValue = null;
            slot_Type = null;
            slot_button_Field = null;
            foregroundColor_Property = null;
            defaultSorts_Property = null;
            defaultSorts_ArrayValue = null;
            selectedSort_Field = null;
            selectedSort_Instance = null;
            selectedCategory_Property = null;
            selectedCategory_Instance = null;
            categories_Property = null;
            categories_ArrayValue = null;
            categories_Type = null;
            categories_GetItems_Method = null;
            searchBox_Field = null;
            searchBox_Instance = null;
            searchBox_Text_Property = null;
            passFilters_Method = null;
            HEROsItemSearchModifier.Instance = null;
        }

        internal void LoadHook() {
            itemBrowserType = null;

            Type[] types = VaultUtils.GetAnyModCodeType();
            foreach (Type type in types) {
                if (type.FullName == "HEROsMod.UIKit.UIComponents.ItemBrowser") {
                    itemBrowserType = type;
                }
                if (type.FullName == "HEROsMod.UIKit.UIView") {
                    UIViewType = type;
                }
                if (type.FullName == "HEROsMod.UIKit.UIComponents.ItemBrowser+MyComparer") {
                    myComparerType = type;
                }
            }

            if (itemBrowserType != null) {
                getItems_Method = itemBrowserType.GetMethod("GetItems", BindingFlags.Public | BindingFlags.Instance);
                MonoModHooks.Add(getItems_Method, on_GetItems_Hook);
                textbox_KeyPressed_Method = itemBrowserType.GetMethod("textbox_KeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                MonoModHooks.Add(textbox_KeyPressed_Method, on_textbox_KeyPressed_Hook);
            }
        }

        internal void Get_AvailableSorts(object obj) {
            if (availableSorts_Field == null) {
                availableSorts_Field = itemBrowserType.GetField("AvailableSorts", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            //不要懒加载
            availableSorts_ArrayValue = (Array)availableSorts_Field.GetValue(obj);
        }

        internal void Get_SlotInfo() {
            if (slot_Type == null) {
                slot_Type = selectedSort_Instance.GetType();
            }
            if (slot_button_Field == null) {
                slot_button_Field = slot_Type.GetField("button", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (foregroundColor_Property == null) {
                foregroundColor_Property = UIViewType.GetProperty("ForegroundColor", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal void SetForegroundColorInItem(object item, Color newColor) {
            if (item == null) {
                return;
            }
            object buttonInstance = slot_button_Field.GetValue(item);
            foregroundColor_Property.SetValue(buttonInstance, newColor);
        }

        internal void Get_SelectedSort(object obj) {
            if (selectedSort_Field == null) {
                selectedSort_Field = itemBrowserType.GetField("SelectedSort", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            selectedSort_Instance = selectedSort_Field.GetValue(obj);
            if (defaultSorts_Property == null) {
                defaultSorts_Property = itemBrowserType.GetProperty("DefaultSorts", BindingFlags.Public | BindingFlags.Static);
            }
            //这个最好也不要懒加载
            defaultSorts_ArrayValue = (Array)defaultSorts_Property.GetValue(obj);
            if (selectedSort_Instance == null) {
                SetSelectedSort_Instance_Give_DefaultSorts(obj);
            }
        }

        internal void Get_selectedCategory_Property(object obj) {
            if (selectedCategory_Property == null) {
                selectedCategory_Property = itemBrowserType.GetProperty("SelectedCategory", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            //这个最好也不要懒加载
            selectedCategory_Instance = selectedCategory_Property.GetValue(obj);
        }

        internal void Get_Categories(object obj) {
            if (categories_Property == null) {
                categories_Property = itemBrowserType.GetProperty("Categories", BindingFlags.Public | BindingFlags.Static);
            }
            categories_ArrayValue = (Array)categories_Property.GetValue(obj);
        }

        internal Item[] Run_Categories_InGetItem(object category) {
            if (categories_Type == null) {
                categories_Type = category.GetType();
            }
            if (categories_GetItems_Method == null) {
                categories_GetItems_Method = categories_Type.GetMethod("GetItems", BindingFlags.Public | BindingFlags.Instance);
            }
            return (Item[])categories_GetItems_Method.Invoke(category, null);
        }

        internal void Get_searchBox(object obj) {
            if (searchBox_Field == null) {
                searchBox_Field = itemBrowserType.GetField("SearchBox", BindingFlags.Public | BindingFlags.Instance);
            }
            searchBox_Instance = searchBox_Field.GetValue(obj);
            if (searchBox_Text_Property == null) {
                searchBox_Text_Property = searchBox_Instance.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        internal Item[] Get_currentItems(object obj) {
            if (currentItems_Property == null) {
                currentItems_Property = itemBrowserType.GetProperty("CurrentItems", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return (Item[])currentItems_Property.GetValue(obj);
        }

        internal bool Run_PassFilters(object obj, Item item) {
            if (passFilters_Method == null) {
                passFilters_Method = itemBrowserType.GetMethod("PassFilters", BindingFlags.Public | BindingFlags.Instance);
            }
            return (bool)passFilters_Method.Invoke(obj, [item]);
        }

        internal void Run_ResetSlot(List<Item> result) {
            selectedSort_Sort_Field = selectedSort_Instance.GetType().GetField("sort", BindingFlags.Public | BindingFlags.Instance);
            Func<Item, Item, int> func = (Func<Item, Item, int>)selectedSort_Sort_Field.GetValue(selectedSort_Instance);
            result.Sort((Item a, Item b) => func(a, b));
        }

        internal void SetSelectedSort_Instance_Give_DefaultSorts(object obj) {
            object defaultSorts = defaultSorts_ArrayValue.GetValue(0);
            selectedSort_Instance = defaultSorts;
            selectedSort_Field.SetValue(obj, defaultSorts);
        }

        private void on_textbox_KeyPressed_Hook(textbox_KeyPressed_Delegate orig, object obj, object sender, char key) {
            Get_searchBox(obj);

            string _searchBoxText = (string)searchBox_Text_Property.GetValue(searchBox_Instance);
            string _searchBoxText_NoAir = _searchBoxText.Replace(" ", "");
            PinyinFormat pinyinFormat = PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE;
            // 获取用户输入的拼音
            string userInputPinyin = Pinyin4Net.GetPinyin(_searchBoxText, pinyinFormat, false, false, false).ToLower().Replace(" ", "");

            if (_searchBoxText.Length > 0) {
                bool match = false;
                Item[] currentItems = Get_currentItems(obj);
                foreach (Item item in currentItems) {
                    if (SearchUtility.ItemMatching(item.Name, _searchBoxText, userInputPinyin)) {
                        match = true;
                        break;
                    }
                }
                if (!match && SearchUtility.GetActualLength(_searchBoxText_NoAir) >= CheaterPinYinConfig.Instance.CharactersMaxNum) {
                    _searchBoxText = _searchBoxText.Substring(0, _searchBoxText.Length - 1);
                    searchBox_Text_Property.SetValue(searchBox_Instance, _searchBoxText);
                }
            }

            Get_selectedCategory_Property(obj);
            //???这是在干什么，老实说让属性自己等于自己才能让程序正常，这似乎类似于一种迭代手法
            selectedCategory_Property.SetValue(obj, selectedCategory_Property.GetValue(obj));
        }

        private Item[] on_GetItems_Hook(getItem_Delegate orig, object obj) {
            Get_selectedCategory_Property(obj);
            Get_AvailableSorts(obj);
            Get_SelectedSort(obj);
            Get_searchBox(obj);
            Get_SlotInfo();

            string _searchBoxText = (string)searchBox_Text_Property.GetValue(searchBox_Instance);
            string _searchBoxText_NoAir = _searchBoxText.Replace(" ", "");

            foreach (var item in availableSorts_ArrayValue) {
                SetForegroundColorInItem(item, Color.Gray);
            }

            if (selectedSort_Instance == null || Array.IndexOf(availableSorts_ArrayValue, selectedSort_Instance) == -1) {
                SetSelectedSort_Instance_Give_DefaultSorts(obj);
            }

            SetForegroundColorInItem(selectedSort_Instance, Color.White);

            List<Item> result = new List<Item>();

            if (selectedCategory_Instance == null) {
                Get_Categories(obj);
                foreach (var category in categories_ArrayValue) {
                    Item[] items = Run_Categories_InGetItem(category);
                    foreach (Item item in items) {
                        result.Add(item);
                    }
                }
            }
            else {
                result = Run_Categories_InGetItem(selectedCategory_Instance).ToList();
            }

            PinyinFormat pinyinFormat = PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE;
            // 获取用户输入的拼音
            string userInputPinyin = Pinyin4Net.GetPinyin(_searchBoxText, pinyinFormat, false, false, false).ToLower().Replace(" ", "");

            //在这里先一步去重，减少性能开销
            result = result.Distinct().ToList();
            List<Item> newResult = [];
            foreach (var item in result) {
                if (!SearchUtility.ItemMatching(item.Name, _searchBoxText, userInputPinyin)) {
                    continue;
                }
                if (!Run_PassFilters(obj, item)) {
                    continue;
                }
                newResult.Add(item);
            }

            Run_ResetSlot(newResult);

            return newResult.ToArray();
        }
    }
}
