using hyjiacan.py4n;
using InnoVault;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using static CheaterPinYin.CheaterPinYin;
using static System.Net.Mime.MediaTypeNames;

namespace CheaterPinYin
{
    internal class HEROsNPCSpawnerSearchModifier
    {
        internal static HEROsNPCSpawnerSearchModifier Instance;

        internal static Type mobSpawnWindowType;
        internal static Type npcStatsType;
        internal static Type npcStatsListType;

        internal static MethodInfo npcStatsList_Add_Method;
        internal static MethodInfo npcStatsList_Count_Method;

        internal static MethodInfo searchBox_KeyPressed_Method;

        internal static FieldInfo npcList_Field;
        internal static FieldInfo category_Field;
        internal static FieldInfo searchResults_Field;

        internal static object npcList_Value;
        internal static object category_Value;

        internal static PropertyInfo sender_Text_Property;

        internal static PropertyInfo npcStats_Name_Property;

        internal static MethodInfo buildList_Method;

        public static void UnLoad() {
            mobSpawnWindowType = null;
            npcStatsType = null;
            npcStatsListType = null;
            npcStatsList_Add_Method = null;
            npcStatsList_Count_Method = null;
            searchBox_KeyPressed_Method = null;
            npcList_Field = null;
            category_Field = null;
            searchResults_Field = null;
            npcList_Value = null;
            category_Value = null;
            sender_Text_Property = null;
            npcStats_Name_Property = null;
            buildList_Method = null;
            HEROsNPCSpawnerSearchModifier.Instance = null;
        }

        public void LoadHook() {
            Type[] types = VaultUtils.GetAnyModCodeType();
            foreach (Type type in types) {
                if (type.FullName == "HEROsMod.HEROsModServices.MobSpawnWindow") {
                    mobSpawnWindowType = type;
                }
                if (type.FullName == "HEROsMod.HEROsModServices.NPCStats") {
                    npcStatsType = type;
                }
            }

            npcStatsListType = typeof(List<>).MakeGenericType(npcStatsType);
            npcStatsList_Add_Method = npcStatsListType.GetMethod("Add");

            searchBox_KeyPressed_Method = mobSpawnWindowType.GetMethod("searchBox_KeyPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            buildList_Method = mobSpawnWindowType.GetMethod("BuildList", BindingFlags.NonPublic | BindingFlags.Instance);

            if (npcStats_Name_Property == null) {
                npcStats_Name_Property = npcStatsType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            }

            MonoModHooks.Add(searchBox_KeyPressed_Method, on_searchBox_KeyPressed_Hook);
        }

        internal void Load_FieldInfo(object obj) {
            if (npcList_Field == null) {
                npcList_Field = mobSpawnWindowType.GetField("npcList", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (category_Field == null) {
                category_Field = mobSpawnWindowType.GetField("category", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (searchResults_Field == null) {
                searchResults_Field = mobSpawnWindowType.GetField("searchResults", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            npcList_Value = npcList_Field.GetValue(obj);
            category_Value = category_Field.GetValue(obj);
        }

        internal string Get_textbox_TextValue(object sender) {
            if (sender_Text_Property == null) {
                sender_Text_Property = sender.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            }

            return (string)sender_Text_Property.GetValue(sender);
        }

        private void on_searchBox_KeyPressed_Hook(textbox_KeyPressed_Delegate orig, object obj, object sender, char key) {
            Load_FieldInfo(obj);

            if (category_Value != npcList_Value) {
                category_Field.SetValue(obj, npcList_Value);
            }

            string _textbox_Text = Get_textbox_TextValue(sender);

            if (_textbox_Text.Length > 0) {
                //List<object> matches = new List<object>();
                object matches = Activator.CreateInstance(npcStatsListType);
                PinyinFormat pinyinFormat = PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE;
                // 获取用户输入的拼音
                string userInputPinyin = Pinyin4Net.GetPinyin(_textbox_Text, pinyinFormat, false, false, false).ToLower().Replace(" ", "");

                foreach (var npc in (Array)category_Value) {
                    string npcName = ((string)npcStats_Name_Property.GetValue(npc)).ToLower();
                    if (SearchUtility.ItemMatching(npcName, _textbox_Text, userInputPinyin)) {
                        npcStatsList_Add_Method.Invoke(matches, [npc]);
                    }
                }
                if ((int)npcStatsList_Count_Method.Invoke(matches, null) > 0) {
                    searchResults_Field.SetValue(obj, matches);
                    buildList_Method.Invoke(obj, null);
                }
                else {
                    sender_Text_Property.SetValue(obj, _textbox_Text.Substring(0, _textbox_Text.Length - 1));
                }
            }
            else {
                searchResults_Field.SetValue(obj, category_Value);
                buildList_Method.Invoke(obj, null);
            }
        }
    }
}
