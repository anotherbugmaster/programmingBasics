using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Autotype
{
    class Program
    {
        static void ShowElementsFreq(SortedDictionary<string, int> dic)
        {
            var mostFreqElements = dic.OrderByDescending(r => r.Value).Select(r => r).Take(20);
            foreach (var e in mostFreqElements)
            {
                StringBuilder grammBuilder = new StringBuilder();
                foreach (var word in e.Key)
                {
                    grammBuilder.Append(word);
                    //grammBuilder.Append(" ");
                }
                Console.WriteLine("{0,-70}{1}", grammBuilder.ToString(), e.Value);
            }
        }
        static string GetSentence()
        {
            return "";
        }
        static void FillWithGramms(SortedDictionary<string, int> dic, string text, int grammN, string currentSentence)
        {
            dic.Clear();
            char[] fSeparators = { (char)8230, '.', ',' };
            char[] sSeparators = { ' ', '\n', '\"', ':', '“', '”', ';', '!', '?', '—', '(', ')' };
            string[] freqWords = { "as", "that", "at", "on", "in", "at", "of", "the", "to", "a" };
            var sentences = text.Split(fSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var sentence in sentences)
            {

                var words = sentence.Split(sSeparators, StringSplitOptions.RemoveEmptyEntries).Except(freqWords, StringComparer.CurrentCultureIgnoreCase).ToArray();
                for (int index = 0; index < words.Length - grammN; index++)
                {
                    StringBuilder gramm = new StringBuilder();
                    for (int grammIndex = 0; grammIndex < grammN; grammIndex++)
                    {
                        gramm.Append(words[index + grammIndex].ToLower());
                        gramm.Append(" ");
                    }
                    string grammString = gramm.ToString();
                    if (grammString.Substring(0, gramm.Length - 2) == "voldemort kill me")
                       Console.WriteLine("Mysterious symbol is: {0}", (int)grammString[grammString.Length - 2]);
                    if (grammString.StartsWith(currentSentence, StringComparison.OrdinalIgnoreCase))
                    {
                        int currentCount = 0;
                        dic.TryGetValue(grammString, out currentCount);
                        currentCount++;
                        dic[grammString] = currentCount;
                    }
                }
            }
            if (dic.Count > 0)
                currentSentence = dic.OrderByDescending(r => r.Value).Select(r => r.Key).Last();
            else
            {
                Console.WriteLine("Actually, I can't speak in such long and difficult sentence. Try other word.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        static void Main(string[] args)
        {
            var wordList = new SortedDictionary<string, int>();
            var text = File.ReadAllText("Text.txt");
            string currentSentence = Console.ReadLine();
            int wordsAmount = 2;
            for (int wordsNum = 1; wordsNum <= wordsAmount; wordsNum++)
                FillWithGramms(wordList, text, wordsNum, currentSentence);
            ShowElementsFreq(wordList);
            Console.ReadKey();
        }
    }
}
