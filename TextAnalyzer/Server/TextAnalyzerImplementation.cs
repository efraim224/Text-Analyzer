using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common;


namespace Server
{
    /// <summary>
    /// The server implementation of method calls
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
    class TextAnalyzerImplementation : ITextAnalyzerContract
    {
        /// <summary>
        /// we assuming uppercase isn't relevant. meaning Hello and hello are the same word.
        /// get a string as input, return key pair where:
        /// key is word with most occurrences
        /// value is the number of occurrences
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Key value pair</returns>
        public KeyValuePair<string, int> AnalyzeText(string text)
        {
            string[] words = this.getWordsArray(text);
            words = words.Select(str => str.ToLower()).ToArray();
            Dictionary<string, int> wordInstancesDictionary = this.getWordDictionary(words);
            
            string mostFrequentWord = getMostFrequentWord(wordInstancesDictionary);
            KeyValuePair<string, int> answer = new KeyValuePair<string, int>(mostFrequentWord, wordInstancesDictionary[mostFrequentWord]);
            return answer;
        }

        /// <summary>
        /// creates a dictionary out of the word of the input, where:
        /// key is the word
        /// value is the number of occurrences
        /// </summary>
        /// <param name="i_Words"></param>
        /// <returns></returns>
        private Dictionary<string, int> getWordDictionary(string[] i_Words)
        {
            Dictionary<string, int> wordsDictionary = new Dictionary<string, int>();
            foreach (string word in i_Words)
            {
                if (wordsDictionary.ContainsKey(word))
                {
                    wordsDictionary[word] = wordsDictionary[word] + 1;
                }
                else
                {
                    wordsDictionary[word] = 1;
                }

            }

            return wordsDictionary;
        }

        /// <summary>
        /// creates a dictionary out of the input string, where:
        /// key is the word
        /// value is hashtable, where the values of the hashtable are word s.t. the Levenshtein distance from the key is 1.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Dictionary<string, HashSet<string>> FindTyposForWordsInText(string text)
        {
            Dictionary<string, HashSet<string>> answer = new Dictionary<string, HashSet<string>>();
            string[] words = this.getWordsArray(text);
            words = words.Select(str => str.ToLower()).ToArray();
            words = this.getWordDictionary(words).Keys.ToArray();
            foreach (string word in words)
            {
                HashSet<string> similarWords = new HashSet<string>();
                foreach(string wordToCompare in words)
                {
                    if(this.computeDistanceAlgorithm(word, wordToCompare) == 1)
                    {
                        similarWords.Add(wordToCompare);
                    }
                }

                if(similarWords.Count > 0)
                {
                    answer.Add(word, similarWords);
                }
            }

            return answer;
        }

        /// <summary>
        /// counts the number of letters in the string.
        /// </summary>
        /// <param name="i_Text"></param>
        /// <returns></returns>
        public int LetterCount(string i_Text)
        {
            return i_Text.Count(char.IsLetter);
        }

        /// <summary>
        /// split the input string to valid array of words.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string[] getWordsArray(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select this.trimSuffix(m.Value);

            return words.ToArray();
        }

        /// <summary>
        /// trim suffix of word
        /// </summary>
        /// <param name="i_Word"></param>
        /// <returns></returns>
        private string trimSuffix(string i_Word)
        {
            int apostropheLocation = i_Word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                i_Word = i_Word.Substring(0, apostropheLocation);
            }

            return i_Word;
        }

        /// <summary>
        /// calculate the most frequent word in the dictionary
        /// </summary>
        /// <param name="i_WordsKeyValuePair"></param>
        /// <returns></returns>
        private string getMostFrequentWord(Dictionary<string, int> i_WordsKeyValuePair)
        {
            int max = -1;
            string mostFrequentWord = null;
            foreach (KeyValuePair<string, int> entry in i_WordsKeyValuePair)
            {
                if (entry.Value > max)
                {
                    max = entry.Value;
                    mostFrequentWord = entry.Key;
                }
            }

            return mostFrequentWord;
        }

        /// <summary>
        /// compute the distance of the two strings with my version to this using Levenshtein distance algorithm
        /// return the distance between the strings.
        /// the distance will be 1 if one of the following rules happen:
        /// 1.	Replacement (one letter was changed from the original)
        /// 2.  Duplication(one letter was duplicated)
        /// 3.  Removal(one letter was removed)
        /// </summary>
        /// <param name="i_First"></param>
        /// <param name="i_Second"></param>
        /// <returns></returns>
        private int computeDistanceAlgorithm(string i_First, string i_Second)
        {
            if (i_First.Length == 0 && i_Second.Length > 0)
            {
                return int.MaxValue;
            }

            if (Math.Abs(i_First.Length - i_Second.Length) > 1)
            {
                return int.MaxValue;
            }

            // special case :
            if(i_First.Length + 1 == i_Second.Length)
            {
                foreach(char letter in i_Second)
                {
                    //not the case we want
                    if(!i_First.Contains(letter))
                    {
                        return 2;
                    }
                }

                return 1;
            }

            var current = 1;
            var previous = 0;
            var r = new int[2, i_Second.Length + 1];
            for (var i = 0; i <= i_Second.Length; i++)
            {
                r[previous, i] = i;
            }

            for (var i = 0; i < i_First.Length; i++)
            {
                r[current, 0] = i + 1;

                for (var j = 1; j <= i_Second.Length; j++)
                {
                    var cost = (i_Second[j - 1] == i_First[i]) ? 0 : 1;
                    r[current, j] = minThreeValues(
                        r[previous, j] + 1,
                        r[current, j - 1] + 1,
                        r[previous, j - 1] + cost);
                }
                previous = (previous + 1) % 2;
                current = (current + 1) % 2;
            }
            return r[previous, i_Second.Length];
        }

        /// <summary>
        /// returns the min of 3 inputs.
        /// </summary>
        /// <param name="i_Num1"></param>
        /// <param name="i_Num2"></param>
        /// <param name="i_Num3"></param>
        /// <returns></returns>
        ///
        private int minThreeValues(int i_Num1, int i_Num2, int i_Num3)
        {
            return Math.Min(Math.Min(i_Num1, i_Num2), i_Num3);
        }
    }
}
