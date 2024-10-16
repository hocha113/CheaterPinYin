using hyjiacan.py4n;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace CheaterPinYin
{
    internal static class SearchUtility
    {
        internal static bool ItemMatching(string itemName, string userInput, string userInputPinyin) {
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

            // 添加首字母缩写匹配逻辑
            string itemNameInitials = GetInitials(itemName);

            // 检查首字母缩写是否匹配
            if (itemNameInitials.IndexOf(userInput, StringComparison.Ordinal) != -1) {
                return true;
            }

            // 如果启用了正则表达式匹配，尝试匹配物品名称和物品名称的拼音
            if (CheaterPinYinConfig.Instance.RegularExpression) {
                // 创建一个匹配用户输入的正则表达式
                Regex regex = new Regex(userInputPinyin, RegexOptions.IgnoreCase);
                // 使用正则表达式匹配拼音
                if (regex.IsMatch(itemNamePinyin)) {
                    return true;
                }
                // 使用正则表达式匹配物品名称
                regex = new Regex(userInput, RegexOptions.IgnoreCase);
                if (regex.IsMatch(itemName)) {
                    return true;
                }
            }

            return false;
        }

        // 提取每个汉字的首字母，并返回首字母组成的字符串
        internal static string GetInitials(string name) {
            StringBuilder initials = new StringBuilder();

            foreach (char c in name) {
                // 使用 Pinyin4Net 获取汉字的拼音
                string pinyin = Pinyin4Net.GetPinyin(c.ToString(), PinyinFormat.LOWERCASE | PinyinFormat.WITHOUT_TONE, false, false, false);
                if (!string.IsNullOrEmpty(pinyin)) {
                    initials.Append(pinyin[0]); // 获取拼音的首字母
                }
            }

            return initials.ToString();
        }

        internal static int GetActualLength(string input) {
            int actualLength = 0;

            foreach (char currentChar in input) {
                // 如果字符是汉字或全角字符，占2位；否则占1位
                if (currentChar > 0xFF) {
                    actualLength += 2;
                }
                else {
                    actualLength += 1;
                }
            }

            return actualLength;
        }

    }
}
