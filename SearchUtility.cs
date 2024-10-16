using hyjiacan.py4n;
using System.Text;
using System;
using System.Text.RegularExpressions;
using Terraria;

namespace CheaterPinYin
{
    internal static class SearchUtility
    {
        internal static bool ItemMatching(string itemName, string userInput, string userInputPinyin, Regex regex = null) {
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
            
            // 如果启用了正则表达式匹配
            if (CheaterPinYinConfig.Instance.RegularExpression && regex != null && regex.IsMatch(itemName)) {
                return true;
            }

            return false;
        }

        internal static Regex SafeGetRegex(string input) {
            if (!CheaterPinYinConfig.Instance.RegularExpression) {
                return null;
            }
            return new Regex(input, RegexOptions.IgnoreCase);
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

        // 定义一个方法用于验证用户输入是否合法
        private static bool IsValidInput(string input) {
            // 可以根据需要定义合法字符的正则表达式
            // 这里假设只允许字母、数字、汉字和下划线
            Regex validInputRegex = new Regex(@"^[a-zA-Z0-9_\u4e00-\u9fa5]*$", RegexOptions.IgnoreCase);
            return validInputRegex.IsMatch(input);
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
