using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace fixer
{
	public class WordTypo
	{
		public WordTypo(string originalWord, string wordWithTypo)
		{
			OriginalWord = originalWord;
			WordWithTypo = wordWithTypo;
		}

		public string OriginalWord;
		public string WordWithTypo;
	}
    /// <summary>
    /// Метрика Дамерау-Левенштейна.
    /// </summary>
    public class DamerauLevensteinMetric
    {
        private const int DEFAULT_LENGTH = 255;
        private int[] _currentRow;
        private int[] _previousRow;
        private int[] _transpositionRow;

        /// <summary>
        /// 
        /// </summary>
        public DamerauLevensteinMetric()
            : this(DEFAULT_LENGTH)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxLength"></param>
        public DamerauLevensteinMetric(int maxLength)
        {
            _currentRow = new int[maxLength + 1];
            _previousRow = new int[maxLength + 1];
            _transpositionRow = new int[maxLength + 1];
        }

        /// <summary>
        /// Расстояние Дамерау-Левенштейна вычисляется за асимптотическое время O((max + 1) * min(first.length(), second.length()))
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int GetDistance(string first, string second, int max)
        {
            int firstLength = first.Length;
            int secondLength = second.Length;

            if (firstLength == 0)
                return secondLength;

            if (secondLength == 0) return firstLength;

            if (firstLength > secondLength)
            {
                string tmp = first;
                first = second;
                second = tmp;
                firstLength = secondLength;
                secondLength = second.Length;
            }

            if (max < 0) max = secondLength;
            if (secondLength - firstLength > max) return max + 1;

            if (firstLength > _currentRow.Length)
            {
                _currentRow = new int[firstLength + 1];
                _previousRow = new int[firstLength + 1];
                _transpositionRow = new int[firstLength + 1];
            }

            for (int i = 0; i <= firstLength; i++)
                _previousRow[i] = i;

            char lastSecondCh = (char)0;
            for (int i = 1; i <= secondLength; i++)
            {
                char secondCh = second[i - 1];
                _currentRow[0] = i;

                // Вычисляем только диагональную полосу шириной 2 * (max + 1)
                int from = Math.Max(i - max - 1, 1);
                int to = Math.Min(i + max + 1, firstLength);

                char lastFirstCh = (char)0;
                for (int j = from; j <= to; j++)
                {
                    char firstCh = first[j - 1];

                    // Вычисляем минимальную цену перехода в текущее состояние из предыдущих среди удаления, вставки и
                    // замены соответственно.
                    int cost = firstCh == secondCh ? 0 : 1;
                    int value = Math.Min(Math.Min(_currentRow[j - 1] + 1, _previousRow[j] + 1), _previousRow[j - 1] + cost);

                    // Если вдруг была транспозиция, надо также учесть и её стоимость.
                    if (firstCh == lastSecondCh && secondCh == lastFirstCh)
                        value = Math.Min(value, _transpositionRow[j - 2] + cost);

                    _currentRow[j] = value;
                    lastFirstCh = firstCh;
                }
                lastSecondCh = secondCh;

                int[] tempRow = _transpositionRow;
                _transpositionRow = _previousRow;
                _previousRow = _currentRow;
                _currentRow = tempRow;
            }

            return _previousRow[firstLength];
        }
    }
	public class TyposFixer
	{
		/// <summary>
		/// тут вы можете собрать некоторую статистику про виды опечаток, которую потом использовать в методе FixWord
		/// </summary>
		/// <param name="typos">список пар "оригинальное слово" — "слово, которое набрал наборщик"</param>
		public void Learn(WordTypo[] typos)
		{
			//В первой задачи тут нужно составить словарь из переданных данных
			//Для второй задачи тут можно считать любую статистику про виды опечаток, которую потом использовать в методе FixWord

            AddCorrectWordsFromTestTypo();
            FillTrainingFreqDict(trainingDict);
            FillTrainingFreqDict(trainingTypoDict);
            //FillTrainingFreqDict(testTypoDict);
            InitTestFreqDict();
            //knownWordsDict = MergeDicts(testTypoDict, GetSpecialWordsDict(testTypoDict, "correct"));
            //knownWordsDict = trainingDict;
            //errorsDict = MergeDicts(testTypoDict, GetSpecialWordsDict(testTypoDict, "error"));
		}
		public string[] GetSimilarWords(string s)
		{
			// Первая задача
			return new string[0];
		}
		public string FixWord(string word)
		{
			// Вторая задача

            checkingWordIndex++;
            var currentFix = word;
            if (CheckIfThereAreKnownWords(word, knownWordsDict))
                return word;
            if (CheckIfThereAreKnownWords(word, trainingDict))
                return word;
            currentFix = GetRightWordForKnownError(word, trainingTypo);
            if (currentFix != word)
                return currentFix;
            currentFix = LinearAlgorithm(word, knownWordsDict);
            if (currentFix != word)
                return currentFix;
            return LinearAlgorithm(word, trainingDict);
		}
        public void AddCorrectWordsFromTestTypo()
        {
            var pathToFile = "training.txt";
            var wordsWithTypos = File.ReadAllLines(pathToFile + ".typos");
            var originalWords = File.ReadAllLines(pathToFile);

            trainingTypo = new WordTypo[originalWords.GetLength(0)];
            for (int index = 0; index < originalWords.GetLength(0); index++)
            {
                trainingTypo[index] = new WordTypo(originalWords[index], wordsWithTypos[index]);
            }
        }
        public void FillTrainingFreqDict(Dictionary<string, int> dict)
        {
            foreach (var word in trainingTypo)
            {
                int currentFreq = 0;
                dict.TryGetValue(word.OriginalWord, out currentFreq);
                currentFreq++;
                dict[word.OriginalWord] = currentFreq;
            }
        }
        public void FillTestFreqDict()
        {
            string[] wordsWithTypos = File.ReadAllLines("test.txt.typos");
            foreach (var word in wordsWithTypos)
            {
                int currentFreq = 0;
                testTypoDict.TryGetValue(word, out currentFreq);
                currentFreq++;
                testTypoDict[word] = currentFreq;
            }
        }
        public void AddErrorVariation(char goodLetter, char badLetter)
        {
            var currentFreq = 0;
            if (!errorVariations.ContainsKey(goodLetter))
                errorVariations.Add(goodLetter, new Dictionary<char, int>());
            if (!errorVariations[goodLetter].ContainsKey(badLetter))
                errorVariations[goodLetter].Add(badLetter, 0);
            else
            {
                errorVariations[goodLetter].TryGetValue(badLetter, out currentFreq);
                currentFreq++;
                errorVariations[goodLetter][badLetter] = currentFreq;
            }
        }
        public static void ShowBadLetters(char goodLetter)
        {
            Console.WriteLine("Bad letters for {0} are:", goodLetter);
            foreach(var letter in errorVariations[goodLetter])
            {
                Console.WriteLine("Char {0, 1}, freq {1}", letter.Key, letter.Value);
            }
            Console.WriteLine();
        }
        public bool CheckIfThereAreKnownWords(string word, Dictionary<string, int> dict)
        {
            foreach (var keyword in dict)
            {
                bool isEqual = word.Equals(keyword.Key);
                bool isPlural = word.Equals(keyword.Key + "s") || keyword.Key.Equals(word + "s");
                bool isAdjective = word.Equals(keyword.Key + "y") || keyword.Key.Equals(word + "y");
                bool isPastTense = (word.Substring(0, word.Length - 2) == keyword.Key) && word.EndsWith("ed") ||
                                   (keyword.Key.Substring(0, keyword.Key.Length - 2) == word) && keyword.Key.EndsWith("ed"); 
                if (isEqual || isPlural || isPastTense || isAdjective)
                {
                    //Console.WriteLine("{0, -50} is correct", word);
                    return true;
                }
            }
            return false;
        }
        public string GetRightWordForKnownError(string word, WordTypo[] typos)
        {
            foreach (var typo in typos)
            {
                if (word.Equals(typo.WordWithTypo))
                    return typo.OriginalWord;
            }
            return word;
        }
        public Dictionary<string, int> GetSpecialWordsDict(Dictionary<string, int> dict, string type)
        {
            var specialDict = new Dictionary<string, int>();
            if (type == "error")
                foreach(var keyword in dict)
                {
                    if (keyword.Value == 1)
                        specialDict[keyword.Key] = keyword.Value;
                }
            else
                foreach (var keyword in dict)
                {
                    if (keyword.Value != 1)
                        specialDict[keyword.Key] = keyword.Value;
                }
            return specialDict;
        }
        public Dictionary<string, int> MergeDicts(Dictionary<string, int> dict1, Dictionary<string, int> dict2)
        {
            var mergedDict = new Dictionary<string, int>();
            foreach (var keyword in dict1)
                mergedDict[keyword.Key] = keyword.Value;
            foreach (var keyword in dict2)
                mergedDict[keyword.Key] = keyword.Value;
            return mergedDict;
        }
        public string LinearAlgorithm(string word, Dictionary<string, int> dict)
        {
            var currentFix = word;
            var metric = new DamerauLevensteinMetric();
            for (var threshold = 1; threshold < 2; threshold++)
            {
                int maxFreq = 0;
                foreach (var keyword in dict)
                {
                    if (metric.GetDistance(word, keyword.Key, threshold) == threshold)
                    {  
                        if (keyword.Value > maxFreq)
                        {
                            currentFix = keyword.Key;
                            maxFreq = keyword.Value;
                        }
                    }
                }
                if (currentFix != word)
                {
                    Console.WriteLine("{0, -50} correct word is {1}", word, currentFix);
                    return currentFix;
                }
            }
            return word;
        }
        public void FillTwoGrammsDict()
        {
            string[] wordsWithTyposTest = File.ReadAllLines("test.txt.typos");
            string[] originalWordsTraining = File.ReadAllLines("training.txt");
            for (int index = 0; index < wordsWithTyposTest.Length - 1; index++ )
            {
                var currentWord = wordsWithTyposTest[index];
                var nextWord = wordsWithTyposTest[index + 1];
                int currentFreq = 0;
                StringBuilder builder = new StringBuilder();
                var twoGramm = builder.AppendLine(currentWord).Append(" ").AppendLine(nextWord).ToString();
                twoGrammsDict.TryGetValue(twoGramm, out currentFreq);
                currentFreq++;
                twoGrammsDict[twoGramm] = currentFreq;
            }
            for (int index = 0; index < originalWordsTraining.Length - 1; index++)
            {
                var currentWord = originalWordsTraining[index];
                var nextWord = originalWordsTraining[index + 1];
                int currentFreq = 0;
                StringBuilder builder = new StringBuilder();
                var twoGramm = builder.AppendLine(currentWord).Append(" ").AppendLine(nextWord).ToString();
                twoGrammsDict.TryGetValue(twoGramm, out currentFreq);
                currentFreq++;
                twoGrammsDict[twoGramm] = currentFreq;
            }
        }
        public void InitTestFreqDict()
        {
            testTypoDict = new Dictionary<string, int>();
            var words = File.ReadAllLines("test.txt.typos");
            foreach (var word in words)
            {
                int currentFreq = 0;
                testTypoDict.TryGetValue(word, out currentFreq);
                if (currentFreq < 2)
                {
                    testTypoDict[word] = currentFreq + 1;
                }
                else
                {
                    knownWordsDict.TryGetValue(word, out currentFreq);
                    knownWordsDict[word] = currentFreq + 1;
                }
            }
        }
        #region Variables
        public static WordTypo[] trainingTypo;
        public static Dictionary<string, int> trainingDict = new Dictionary<string,int>();
        public static Dictionary<string, int> trainingTypoDict = new Dictionary<string,int>();
        public static Dictionary<string, int> testTypoDict = new Dictionary<string,int>();
        public static Dictionary<string, int> knownWordsDict = new Dictionary<string, int>();
        public static Dictionary<string, int> errorsDict = new Dictionary<string, int>();
        public static Dictionary<string, int> twoGrammsDict = new Dictionary<string, int>();
        public static Dictionary<char, Dictionary<char,int>> errorVariations = new Dictionary<char, Dictionary<char, int>>();
        public static int checkingWordIndex = -1;
        #endregion
        public static void Main(string[] args)
		{
			//Если вы решаете первую задачу, напишите тут код, демонстрирующий работоспособность метода GetSimilarWords.

			//Если вы решаете вторую задачу, то раскомментируйте эту строку:

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
			AccuracyTester.Test(args);
            stopwatch.Stop();
            Console.WriteLine("Времени потрачено: {0}", stopwatch.Elapsed);
            ShowBadLetters('a');
            Console.ReadKey();
		}

	}
}