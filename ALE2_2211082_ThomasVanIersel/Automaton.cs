using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2_2211082_ThomasVanIersel
{
    class Automaton
    {
        #region ---------------------------------- PROPERTIES ------------------------------------

        public string Alphabet { get; set; }

        public List<State> States { get; set; }

        public List<Transition> Transitions { get; set; }

        #endregion
        #region --------------------------------- CONSTRUCTORS -----------------------------------

        public Automaton() { }

        public Automaton(string alphabet, List<State> states, List<Transition> transitions)
        {
            Alphabet = alphabet ?? throw new ArgumentNullException(nameof(alphabet));
            States = states ?? throw new ArgumentNullException(nameof(states));
            Transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
        }

        #endregion
        #region ------------------------------------ METHODS -------------------------------------

        public bool IsDFA()
        {
            // DFA's can't have an empty transition, so that's an easy first check.
            if (Transitions.Any(t => t.Label == "_"))
                return false;
            
            foreach (State s in States)
            {
                // Check for each State if it satisfies the rules for this automaton to be a DFA.
                // If even one State doesn't satisfy the rules, the automaton is not a DFA.
                if (IsDFAState(s) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests if one State satisfies the rules for an automaton to be a DFA.
        /// </summary>
        /// <param name="s">The State to check.</param>
        /// <returns></returns>
        private bool IsDFAState(State s)
        {
            List<Transition> transitionsFromS = Transitions.Where(t => t.FirstState == s).ToList();
            int alphabetLength = Alphabet.Length;
            char[] alphabetLetters = Alphabet.ToArray();
            char[] transitionLabels = transitionsFromS.Select(t => Convert.ToChar(t.Label)).ToArray();
            Array.Sort(alphabetLetters);
            Array.Sort(transitionLabels);

            // In order for an automaton to be a DFA, all the states need to have an outbound number of transitions equal to the number
            // of letters in the automaton's alphabet. Furthermore, each state needs to have one (and only one) transition for each letter.
            if (transitionsFromS.Count() != alphabetLength)
                return false;
            else if (alphabetLetters.SequenceEqual(transitionLabels) == false)
                return false;

            return true;
        }

        #endregion
    }
}
