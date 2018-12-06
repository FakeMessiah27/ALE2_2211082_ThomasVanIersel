using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ALE2_2211082_ThomasVanIersel
{
    class Utilities
    {
        /// <summary>
        /// Get the string slice between the two indexes.
        /// Inclusive for start index, exclusive for end index.
        /// Source: https://www.dotnetperls.com/string-slice
        /// </summary>
        public static string Slice(string source, int start, int end)
        {
            if (end < 0) // Keep this for negative end support
            {
                end = source.Length + end;
            }
            int len = end - start;               // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }

        /// <summary>
        /// Adds characters from second string onto first string as long as first string does not yet contain said character.
        /// Only adds letters and underscores (which represent epsilons).
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static string AddUniqueCharsToString(string first, string second)
        {
            foreach (char c in second)
            {
                if (first.Contains(c) == false)
                    first += c;
            }

            return first;
        }

        /// <summary>
        /// Returns the unique alphabetical characters of a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetUniqueChars(string input)
        {
            Regex reg = new Regex("[^a-zA-Z'_]");
            string result = string.Empty;
            input = reg.Replace(input, string.Empty);
            
            return string.Concat(input.OrderBy(c => c).Distinct());
        }

        /// <summary>
        /// Checks if the formula is in a syntactically correct, prefix notation. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool RegularExpressionIsValid(string input)
        {
            // Store the number of brackets, commas, and negations.
            MatchCollection openingBrackets = Regex.Matches(input, Regex.Escape("("));
            int nrOfClosingBrackets = Regex.Matches(input, Regex.Escape(")")).Count;
            int nrOfStars = Regex.Matches(input, Regex.Escape("*")).Count;
            int nrOfCommas = Regex.Matches(input, Regex.Escape(",")).Count;
            string[] operators = { ".", "|", "*" };

            // Check if there are any brackets.
            if (openingBrackets.Count > 0 || nrOfClosingBrackets > 0)
            {
                // Check if there are  equal number of opening and closing brackets. 
                if (openingBrackets.Count == nrOfClosingBrackets)
                {
                    // For every set of brackets, there needs to be a comma, EXCEPT if it's a negation.
                    if (openingBrackets.Count - nrOfStars == nrOfCommas)
                    {
                        // Assume every set of brackets has an operator.
                        bool hasOperators = true;

                        // Loop through the opening brackets.
                        foreach (Capture openingBracket in openingBrackets)
                        {
                            // If an opening bracket is missing an operator, set the hasOperators variable to false.
                            if (operators.Contains(input[openingBracket.Index - 1].ToString()) == false)
                                hasOperators = false;
                        }

                        return hasOperators;
                    }
                }
            }
            else
            {
                // If there are no brackets, the expression can only be a single alphabetical symbol.
                if (input.Length == 1 && char.IsLetter(input[0]))
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Creates a string of numbers from the current system date and time.
        /// </summary>
        /// <returns></returns>
        public static string CreateDatetimeSeed()
        {
            string datetime = DateTime.Now.ToString();
            string datetimeSeed = "";

            // Remove any special characters.
            foreach (char c in datetime)
            {
                if (char.IsLetterOrDigit(c) == true)
                    datetimeSeed += c;
            }

            return datetimeSeed;
        }
    }
}
