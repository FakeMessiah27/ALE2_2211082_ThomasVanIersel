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

        /// <summary>
        /// Recursivley tests if a given word is accepted by the automaton.
        /// </summary>
        /// <param name="word">Word to be tested.</param>
        /// <param name="currentState">Current state in the automaton. Should be the first state when the method is first called.</param>
        /// <returns></returns>
        public bool IsAcceptedWord(string word, State currentState)
        {
            // If the current word is still bigger than 0, we have letters left to check, therefore continue.
            if (word.Length > 0)
            {
                // Get the possible outbound transitions from the current state with the first letter of the remainder of the word as the label.
                var possibleTransitions = GetPossibleTransitions(currentState, word[0].ToString());

                foreach (var t in possibleTransitions)
                {
                    // Recursively search the found transition(s).
                    // For the word parameters, we pass the current word minus the first letter.
                    // For the state we pass the state that the current transition would lead us to.
                    if (IsAcceptedWord(word.Substring(1), t.SecondState))
                        return true;
                }

                // If none of the transitions had any more outbound transitions that would have allowed us to continue,
                // or no transitions for the current state could be found, the word is not accepted.
                return false;
            }

            // If we have no letters left to check (we're reached the end of the word), see if we ended up in a final state.
            if (currentState.IsFinal)
                return true;

            // If we're not in a final state, see if there are epsilon transitions we can take.
            List<Transition> possibleEpsilonTransitions = GetPossibleEpsilonTransitions(currentState);
            if (possibleEpsilonTransitions.Count != 0)
            {
                foreach (var t in possibleEpsilonTransitions)
                {
                    if (IsAcceptedWord(word, t.SecondState))
                        return true;
                }
            }

            // If no epsilon transitions could be taken and we are not in a final state, the word is not accepted.
            return false;
        }

        /// <summary>
        /// Returns all possible transitions from a given state with a given label.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public List<Transition> GetPossibleTransitions(State state, string label)
        {
            return Transitions.Where(t => t.FirstState == state && (t.Label == label || t.Label == "_")).ToList();
        }

        /// <summary>
        /// Returns all possible transitions from a given state with epsilon (empty string) label.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<Transition> GetPossibleEpsilonTransitions(State state)
        {
            return Transitions.Where(t => t.FirstState == state && t.Label == "_").ToList();
        }

        #endregion
    }
}
