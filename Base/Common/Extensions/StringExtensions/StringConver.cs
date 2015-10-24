using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sinan.Extensions
{
    public static partial class StringConver
    {
        /// <summary>
        /// Replaces a given character with another character in a string. 
        /// The replacement is case insensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="charToReplace">The character to replace</param>
        /// <param name="replacement">The character by which to be replaced</param>
        /// <returns>Copy of string with the characters replaced</returns>
        public static string CaseInsenstiveReplace(this string val, char charToReplace, char replacement)
        {
            Regex regEx = new Regex(charToReplace.ToString(), RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regEx.Replace(val, replacement.ToString());
        }
        /// <summary>
        /// Replaces a given string with another string in a given string. 
        /// The replacement is case insensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="stringToReplace">The string to replace</param>
        /// <param name="replacement">The string by which to be replaced</param>
        /// <returns>Copy of string with the string replaced</returns>
        public static string CaseInsenstiveReplace(this string val, string stringToReplace, string replacement)
        {
            Regex regEx = new Regex(stringToReplace, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regEx.Replace(val, replacement);
        }
        /// <summary>
        /// Replaces the first occurrence of a string with another string in a given string
        /// The replacement is case insensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="stringToReplace">The string to replace</param>
        /// <param name="replacement">The string by which to be replaced</param>
        /// <returns>Copy of string with the string replaced</returns>
        public static string ReplaceFirst(this string val, string stringToReplace, string replacement)
        {
            Regex regEx = new Regex(stringToReplace, RegexOptions.Multiline);
            return regEx.Replace(val, replacement, 1);
        }
        /// <summary>
        /// Replaces the first occurrence of a character with another character in a given string
        /// The replacement is case insensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="charToReplace">The character to replace</param>
        /// <param name="replacement">The character by which to replace</param>
        /// <returns>Copy of string with the character replaced</returns>
        public static string ReplaceFirst(this string val, char charToReplace, char replacement)
        {
            Regex regEx = new Regex(charToReplace.ToString(), RegexOptions.Multiline);
            return regEx.Replace(val, replacement.ToString(), 1);
        }
        /// <summary>
        /// Replaces the last occurrence of a character with another character in a given string
        /// The replacement is case insensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="charToReplace">The character to replace</param>
        /// <param name="replacement">The character by which to replace</param>
        /// <returns>Copy of string with the character replaced</returns>
        public static string ReplaceLast(this string val, char charToReplace, char replacement)
        {
            int index = val.LastIndexOf(charToReplace);
            if (index < 0)
            {
                return val;
            }
            else
            {
                StringBuilder sb = new StringBuilder(val.Length - 2);
                sb.Append(val.Substring(0, index));
                sb.Append(replacement);
                sb.Append(val.Substring(index + 1,
                   val.Length - index - 1));
                return sb.ToString();
            }
        }
        /// <summary>
        /// Replaces the last occurrence of a string with another string in a given string
        /// The replacement is case insensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="stringToReplace">The string to replace</param>
        /// <param name="replacement">The string by which to be replaced</param>
        /// <returns>Copy of string with the string replaced</returns>
        public static string ReplaceLast(this string val, string stringToReplace, string replacement)
        {
            int index = val.LastIndexOf(stringToReplace);
            if (index < 0)
            {
                return val;
            }
            else
            {
                StringBuilder sb = new StringBuilder(val.Length - stringToReplace.Length + replacement.Length);
                sb.Append(val.Substring(0, index));
                sb.Append(replacement);
                sb.Append(val.Substring(index + stringToReplace.Length,
                   val.Length - index - stringToReplace.Length));

                return sb.ToString();
            }
        }
        /// <summary>
        /// Removes occurrences of words in a string
        /// The match is case sensitive
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="filterWords">Array of words to be removed from the string</param>
        /// <returns>Copy of the string with the words removed</returns>
        public static string RemoveWords(this string val, params string[] filterWords)
        {
            return MaskWords(val, char.MinValue, filterWords);
        }
        /// <summary>
        /// Masks the occurence of words in a string with a given character
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="mask">The character mask to apply</param>
        /// <param name="filterWords">The words to be replaced</param>
        /// <returns>The copy of string with the mask applied</returns>
        public static string MaskWords(this string val, char mask, params string[] filterWords)
        {
            string stringMask = mask == char.MinValue ?
               string.Empty : mask.ToString();
            string totalMask = stringMask;

            foreach (string s in filterWords)
            {
                Regex regEx = new Regex(s, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (stringMask.Length > 0)
                {
                    for (int i = 1; i < s.Length; i++)
                        totalMask += stringMask;
                }

                val = regEx.Replace(val, totalMask);
                totalMask = stringMask;
            }
            return val;
        }
        /// <summary>
        /// Left pads the passed string using the passed pad string for the total number of spaces. 
        /// It will not cut-off the pad even if it causes the string to exceed the total width.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="pad">The pad string</param>
        /// <param name="totalWidth">The total width of the resulting string</param>
        /// <returns>Copy of string with the padding applied</returns>
        public static string PadLeft(this string val, string pad, int totalWidth)
        {
            return PadLeft(val, pad, totalWidth, false);
        }
        /// <summary>
        /// Left pads the passed string using the passed pad string for the total number of spaces. 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="pad">The pad string</param>
        /// <param name="totalWidth">The total width of the resulting string</param>
        /// <param name="cutOff">True to cut off the characters if exceeds the specified width</param>
        /// <returns>Copy of string with the padding applied</returns>
        public static string PadLeft(this string val, string pad, int totalWidth, bool cutOff)
        {
            if (val.Length >= totalWidth)
                return val;

            int padCount = pad.Length;
            string paddedString = val;

            while (paddedString.Length < totalWidth)
            {
                paddedString += pad;
            }

            if (cutOff)
                paddedString = paddedString.Substring(0, totalWidth);
            return paddedString;
        }
        /// <summary>
        /// Right pads the passed string using the passed pad string for the total number of spaces. 
        /// It will not cut-off the pad even if it causes the string to exceed the total width.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="pad">The pad string</param>
        /// <param name="totalWidth">The total width of the resulting string</param>
        /// <returns>Copy of string with the padding applied</returns>
        public static string PadRight(this string val, string pad, int totalWidth)
        {
            return PadRight(val, pad, totalWidth, false);
        }
        /// <summary>
        /// Right pads the passed string using the passed pad string for the total number of spaces. 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="pad">The pad string</param>
        /// <param name="totalWidth">The total width of the resulting string</param>
        /// <param name="cutOff">True to cut off the characters if exceeds the specified width</param>
        /// <returns>Copy of string with the padding applied</returns>
        public static string PadRight(this string val, string pad, int totalWidth, bool cutOff)
        {
            if (val.Length >= totalWidth)
                return val;

            string paddedString = string.Empty;

            while (paddedString.Length < totalWidth - val.Length)
            {
                paddedString += pad;
            }

            if (cutOff)
                paddedString = paddedString.Substring(0, totalWidth - val.Length);
            paddedString += val;
            return paddedString;
        }
        /// <summary>
        /// Removes new line characters from a string
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Returns copy of string with the new line characters removed</returns>
        public static string RemoveNewLines(this string val)
        {
            return RemoveNewLines(val, false);
        }
        /// <summary>
        /// Removes new line characters from a string
        /// Test Coverage: Included
        /// </summary>
        /// <param name="input"></param>
        /// <param name="addSpace">True to add a space after removing a new line character</param>
        /// <returns>Returns a copy of the string after removing the new line character</returns>
        public static string RemoveNewLines(this string input, bool addSpace)
        {
            string replace = addSpace ? " " : string.Empty;
            const string pattern = @"[\r|\n]";
            Regex regEx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regEx.Replace(input, replace);
        }
        /// <summary>
        /// Removes a non numeric character from a string
        /// Test Coverage: Included
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Copy of the string after removing non numeric characters</returns>
        public static string RemoveNonNumeric(this string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
                if (Char.IsNumber(s[i]))
                    sb.Append(s[i]);
            return sb.ToString();
        }
        /// <summary>
        /// Removes numeric characters from a given string
        /// Test Coverage: Included
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Copy of the string after removing the numeric characters</returns>
        public static string RemoveNumeric(this string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
                if (!Char.IsNumber(s[i]))
                    sb.Append(s[i]);
            return sb.ToString();
        }
        /// <summary>
        /// Reverses a string
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Copy of the reversed string</returns>
        public static string Reverse(this string val)
        {
            char[] reverse = new char[val.Length];
            for (int i = 0, k = val.Length - 1; i < val.Length; i++, k--)
            {
                if (char.IsSurrogate(val[k]))
                {
                    reverse[i + 1] = val[k--];
                    reverse[i++] = val[k];
                }
                else
                {
                    reverse[i] = val[k];
                }
            }
            return new string(reverse);
        }
        /// <summary>
        /// Changes the string as sentence case.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Copy of string with the sentence case applied</returns>
        public static string SentenceCase(this string val)
        {
            if (val.Length < 1)
                return val;

            string sentence = val.ToLower();
            return sentence[0].ToString().ToUpper() +
               sentence.Substring(1);
        }
        /// <summary>
        /// Changes the string as title case.
        /// Ignores short words in the string.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Copy of string with the title case applied</returns>
        public static string TitleCase(this string val)
        {
            if (val.Length == 0) return string.Empty;
            return TitleCase(val, true);
        }
        /// <summary>
        /// Changes the string as title case.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="ignoreShortWords">true to ignore short words</param>
        /// <returns>Copy of string with the title case applied</returns>
        public static string TitleCase(this string val, bool ignoreShortWords)
        {
            if (val.Length == 0) return string.Empty;

            IList<string> ignoreWords = null;
            if (ignoreShortWords)
            {
                //TODO: Add more ignore words?
                ignoreWords = new List<string>();
                ignoreWords.Add("a");
                ignoreWords.Add("is");
                ignoreWords.Add("was");
                ignoreWords.Add("the");
            }

            string[] tokens = val.Split(' ');
            StringBuilder sb = new StringBuilder(val.Length);
            foreach (string s in tokens)
            {
                if (ignoreShortWords == true
                    && s != tokens[0]
                    && ignoreWords.Contains(s.ToLower()))
                {
                    sb.Append(s + " ");
                }
                else
                {
                    sb.Append(s[0].ToString().ToUpper());
                    sb.Append(s.Substring(1).ToLower());
                    sb.Append(" ");
                }
            }

            return sb.ToString().Trim();
        }
        /// <summary>
        /// Removes multiple spaces between words
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Returns a copy of the string after removing the extra spaces</returns>
        public static string TrimIntraWords(this string val)
        {
            Regex regEx = new Regex(@"[\s]+");
            return regEx.Replace(val, " ");
        }
        /// <summary>
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="charCount">The number of characters after which it should wrap the text</param>
        /// <returns>The copy of the string after applying the Wrap</returns>
        public static string WordWrap(this string val, int charCount)
        {
            return WordWrap(val, charCount, false, Environment.NewLine);
        }
        /// <summary>
        /// Wraps the passed string at the passed total number of characters (if cuttOff is true)
        /// or at the next whitespace (if cutOff is false). 
        /// Uses the environment new line symbol for the break text
        /// </summary>
        /// <param name="val"></param>
        /// <param name="charCount">The number of characters after which to break</param>
        /// <param name="cutOff">true to break at specific</param>
        /// <returns></returns>
        public static string WordWrap(this string val, int charCount, bool cutOff)
        {
            return WordWrap(val, charCount, cutOff, Environment.NewLine);
        }
        private static string WordWrap(this string val, int charCount, bool cutOff, string breakText)
        {
            StringBuilder sb = new StringBuilder(val.Length + 100);
            int counter = 0;

            if (cutOff)
            {
                while (counter < val.Length)
                {
                    if (val.Length > counter + charCount)
                    {
                        sb.Append(val.Substring(counter, charCount));
                        sb.Append(breakText);
                    }
                    else
                    {
                        sb.Append(val.Substring(counter));
                    }
                    counter += charCount;
                }
            }
            else
            {
                string[] strings = val.Split(' ');
                for (int i = 0; i < strings.Length; i++)
                {
                    // added one to represent the space.
                    counter += strings[i].Length + 1;
                    if (i != 0 && counter > charCount)
                    {
                        sb.Append(breakText);
                        counter = 0;
                    }

                    sb.Append(strings[i] + ' ');
                }
            }
            // to get rid of the extra space at the end.
            return sb.ToString().TrimEnd();
        }
        /// <summary>
        /// Converts an list of string to CSV string representation.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="insertSpaces">True to add spaces after each comma</param>
        /// <returns>CSV representation of the data</returns>
        public static string ToCSV(this IEnumerable<string> val, bool insertSpaces)
        {
            if (insertSpaces)
                return String.Join(", ", val.ToArray());
            else
                return String.Join(",", val.ToArray());
        }
        /// <summary>
        /// Converts an list of characters to CSV string representation.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="insertSpaces">True to add spaces after each comma</param>
        /// <returns>CSV representation of the data</returns>
        public static string ToCSV(this IEnumerable<char> val, bool insertSpaces)
        {
            List<string> casted = new List<string>();
            foreach (var item in val)
                casted.Add(item.ToString());

            if (insertSpaces)
                return String.Join(", ", casted.ToArray());
            else
                return String.Join(",", casted.ToArray());
        }
        /// <summary>
        /// Converts CSV to list of string.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>IEnumerable collection of string</returns>
        public static IEnumerable<string> ListFromCSV(this string val)
        {
            string[] split = val.Split(',');
            foreach (string item in split)
            {
                item.Trim();
            }
            return split;
        }

        /// <summary>
        /// Binary Serialization to a file
        /// </summary>
        /// <param name="val"></param>
        /// <param name="filePath">The file where serialized data has to be stored</param>
        public static void Serialize(this string val, string filePath)
        {
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, val);
            }
        }



    }
}
