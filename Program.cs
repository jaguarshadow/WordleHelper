using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace WordleHelper
{
    class Word
    {
        public string WordStr;
        public (char Letter, string Color)[] Letters = new (char Letter, string Color)[5];  // Letter, Color

        public Word(string guess, string result)
        {
            WordStr = guess.ToLower();
            for (int i = 0; i < Program.WORD_LENGTH; i++)
            {
                char letter = guess[i];
                string color;
                if (result[i] == 'g') color = "green";
                else if (result[i] == 'y') color = "yellow";
                else color = "grey";
                (char Letter, string Color) current = (letter, color);
                Letters[i] = current;
                
            }
        }

        public static void PrintWord(Word word)
        {
            foreach (var l in word.Letters)
            {
                if (l.Color == "grey") Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (l.Color == "green") Console.ForegroundColor = ConsoleColor.Green;
                else if (l.Color == "yellow") Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(char.ToUpper(l.Letter) + " ");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    class Program
    {
        public static List<(string Word, int Score)> WordList = new List<(string Word, int Score)>();
        public const int WORD_LENGTH = 5;
        public static bool WordFound = false;

        static void LoadWordList()
        {
            WordList.Clear();
            foreach (var line in File.ReadLines(@"word_list.txt"))
            {
                var data = line.Split(' ');
                (string Word, int Score) current = (data[0], int.Parse(data[1]));
                WordList.Add(current);
            }
        }

        static string AskForGuess()
        {
            Regex rx = new Regex(@"\w{5}", RegexOptions.IgnoreCase);
            Console.Write("\n Enter Your Guess: ");
            string guess = Console.ReadLine();
            if (guess == "exit")
            {
                return "exit";
            }
            Match match = rx.Match(guess);
            while (!match.Success || guess.Length != 5)
            {
                Console.WriteLine("Your guess wasn't in the right format. Try again.");
                Console.Write("Enter your guess: ");
                guess = Console.ReadLine();
                match = rx.Match(guess);
            }
            return guess;

        }

        static string AskForResult(string guess)
        {
            Console.Write($"Enter 5 digit result (x, y, g, or -----) for {guess.ToUpper()}: ");
            string result = Console.ReadLine().ToLower();
            if (result != "-----")
            {
                Regex rx = new Regex(@"[xyg]{5}", RegexOptions.IgnoreCase);
                Match match = rx.Match(result);
                while (!match.Success || result.Length != 5)
                {
                    Console.WriteLine("Your result wasn't in the right format. Try again.");
                    Console.WriteLine($"Enter your result for guess {guess}:");
                    result = Console.ReadLine();
                    match = rx.Match(result);
                }
                return result;
            }
            else
            {
                // Remove word from Candidate List
                return result;
            }
        }

        static void PrintPossibleMatches(Word guess)
        {
            // Deal with Grey
            var bad_letters = guess.Letters.Where(l => l.Color == "grey").Select(l => l.Letter).ToList();
            var candidates = WordList.Where(w => !w.Word.Any(c => bad_letters.Any(x => x == c))).ToList();
            if (candidates.Count > 0) WordList = new List<(string Word, int Score)>(candidates);

            // Deal with Green
            var known_letters = guess.Letters.Where(l => l.Color == "green").Select(l => l.Letter).ToList();
            string toMatch = "";
            if (known_letters.Count > 0)
            {
                candidates = new List<(string Word, int Score)>();
                foreach (var l in guess.Letters)
                {
                    if (l.Color == "green") toMatch += l.Letter;
                    else toMatch += @"\w";
                }
                Regex rx = new Regex(toMatch, RegexOptions.IgnoreCase);
                foreach (var w in WordList)
                {
                    Match match = rx.Match(w.Word);
                    if (match.Success) candidates.Add(w);
                }
                WordList = new List<(string Word, int Score)>(candidates);
            }

            // Deal with Yellow
            for (int i = 0; i < WORD_LENGTH; i++)
            {
                candidates = new List<(string Word, int Score)>();
                if (guess.Letters[i].Color == "yellow")
                {
                    char c = char.ToLower(guess.Letters[i].Letter);
                    candidates = WordList.Where(w => w.Word[i] != c && w.Word.Contains(c)).ToList();
                }
                if (candidates.Count > 0) WordList = new List<(string Word, int Score)>(candidates);
            }

            if (WordList.Count > 1)
            {
                Console.WriteLine("Top possible matches:");
                foreach (var w in WordList.OrderByDescending(w => w.Score).Take(10)) Console.Write($"{w.Word} ");
                Console.WriteLine();
            }
            else if (WordList.Count == 1) {
                Console.WriteLine("Only one possible match remains: ");
                Word.PrintWord(new Word(WordList[0].Word, "ggggg"));
                WordFound = true;
                Console.WriteLine();
            }
            else Console.Write("No Matches were found. Check your result input and try again.");
        }

        static void Main(string[] args)
        {
            LoadWordList();
            Console.WriteLine("The secret word is ?????");
            string guess = AskForGuess();
            
            while (guess != "exit")
            {
                string result = AskForResult(guess);

                Word currentWord = new Word(guess, result);
                Console.Write($"\nThe current guess is ");
                Word.PrintWord(currentWord);

                Console.WriteLine($"\n\nChecking for possible words...");
                PrintPossibleMatches(currentWord);

                if (!WordFound)
                {
                    guess = AskForGuess();
                }
 
                else guess = "exit";
            }

            Console.WriteLine("\nThanks for playing!");

        }
    }
}
