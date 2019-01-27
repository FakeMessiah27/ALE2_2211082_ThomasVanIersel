using System;
using System.Collections.Generic;
using System.IO;
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
            Alphabet = string.Concat(alphabet.OrderBy(c => c)) ?? throw new ArgumentNullException(nameof(alphabet));
            States = states ?? throw new ArgumentNullException(nameof(states));
            Transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
        }

        #endregion
        #region ------------------------------------ METHODS -------------------------------------

        /// <summary>
        /// Checks if an automaton is finite.
        /// </summary>
        /// <returns></returns>
        public bool IsFinite()
        {
            // DFA's are never finite.
            if (this.IsDFA())
                return false;
            
            List<State> visitedStates = new List<State>();
            bool hasNoLoops = true;

            HasNoLoops(States.First(), visitedStates, ref hasNoLoops);
            
            return hasNoLoops;
        }

        /// <summary>
        /// Checks if an automaton has any loops.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="visitedStates"></param>
        /// <param name="hasNoLoops">Boolean value that contains the outcome. Will be set to false if any loops were detected.
        /// Is left unchanged otherwise. Must always be provided as a true value for first call.</param>
        private void HasNoLoops(State currentState, List<State> visitedStates, ref bool hasNoLoops)
        {
            List<State> visitedStatesCopy = visitedStates.ToList();
            visitedStatesCopy.Add(currentState);

            foreach (Transition t in Transitions.Where(t => t.FirstState == currentState))
            {
                if (visitedStatesCopy.Contains(t.SecondState))
                {
                    if (t.SecondState.IsFinal == false)
                        hasNoLoops = false;
                    else if(t.FirstState == t.SecondState)
                        hasNoLoops = false;

                    return;
                }
                
                HasNoLoops(t.SecondState, visitedStatesCopy, ref hasNoLoops);
            }
        }

        /// <summary>
        /// Gets all the possible words that can be formed with this automaton.
        /// This method assumes the automaton is finite and therefore has no loops!
        /// </summary>
        /// <returns>A list of unique words that can be formed with this automaton.</returns>
        public List<string> GetFiniteWords()
        {
            List<string> finiteWords = new List<string>();
            GetAllFiniteWords(States.First(), ref finiteWords);

            return finiteWords.Distinct().ToList();
        }

        /// <summary>
        /// Recursively runs through the automaton, recording all the possible words that can be formed.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="words"></param>
        /// <param name="currentWord"></param>
        private void GetAllFiniteWords(State currentState, ref List<string> words, string currentWord = "")
        {
            foreach (Transition t in Transitions.Where(t => t.FirstState == currentState))
            {
                string newWord = currentWord + t.Label;

                if (t.SecondState.IsFinal)
                {
                    words.Add(newWord);
                }

                GetAllFiniteWords(t.SecondState, ref words, newWord);
            }
        }

        /// <summary>
        /// Attemps to find the next transition that is not an epsilon. This is used to find an alternative to an epsilon transition.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="visitedStates"></param>
        /// <returns></returns>
        private List<Transition> FindNextNonEpsilonTransitions(State s, ref List<State> visitedStates)
        {
            List<Transition> nextNonEpsilonTransitions = new List<Transition>();
            visitedStates.Add(s);

            foreach (Transition t in Transitions.Where(t => t.FirstState == s))
            {
                if (visitedStates.Contains(t.SecondState))
                    continue;
                if (t.Label != "_")
                {
                    nextNonEpsilonTransitions.Add(t);
                    continue;
                }

                nextNonEpsilonTransitions.AddRange(FindNextNonEpsilonTransitions(t.SecondState, ref visitedStates));
            }

            return nextNonEpsilonTransitions.Distinct().ToList();
        }

        /// <summary>
        /// Removes all epsilon transitions from the automaton, while keeping the functional outcomes the same.
        /// It does so by finding alternatives for each epsilon transition.
        /// </summary>
        public void RemoveEpsilonTransitions()
        {
            var epsilonTransitions = Transitions.Where(t => t.Label == "_").ToList();
            List<Transition> newTransitions = new List<Transition>();

            foreach (var t in epsilonTransitions)
            {
                var startingState = t.FirstState;
                var transitionsToAndFromStartingState = Transitions.Where(tr => tr.FirstState == startingState || tr.SecondState == startingState).ToList();
                var visitedStates = new List<State>();
                var nextNonEpsilonTransitions = FindNextNonEpsilonTransitions(t.SecondState, ref visitedStates);

                foreach (var nonEpsilonTransition in nextNonEpsilonTransitions)
                {
                    if (transitionsToAndFromStartingState.Contains(nonEpsilonTransition) == false)
                    {
                        var newTransition = new Transition(nonEpsilonTransition.FirstState, nonEpsilonTransition.SecondState, nonEpsilonTransition.Label);
                        newTransition.FirstState = startingState;

                        if (TransitionIsNotDuplicate(newTransitions, newTransition))
                            newTransitions.Add(newTransition);
                    }
                }
            }

            foreach (var t in epsilonTransitions)
                Transitions.Remove(t);

            Transitions.AddRange(newTransitions);

            RemoveUnreachableStates();
            RemoveTransitionsWithoutStart();
        }

        /// <summary>
        /// Checks if a newly created transition is a duplicate of an existing one that a State already has.
        /// </summary>
        /// <param name="transitions"></param>
        /// <param name="newTransition"></param>
        /// <returns></returns>
        private bool TransitionIsNotDuplicate(List<Transition> transitions, Transition newTransition)
        {
            foreach (Transition t in transitions)
            {
                if (t.FirstState == newTransition.FirstState && t.SecondState == newTransition.SecondState && t.Label == newTransition.Label)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes states that can no longer be reached after epsilon removal has occured.
        /// </summary>
        private void RemoveUnreachableStates()
        {
            for (int i = States.Count - 1; i >= 1; i--)
            {
                var transitionsToState = Transitions.Where(t => t.SecondState == States[i]).ToList();

                if (transitionsToState.Count == 0)
                    States.RemoveAt(i);
            }
        }

        /// <summary>
        /// Removes transitions who's starting states are no longer a part of the automaton, after epsilon removal has occured.
        /// </summary>
        private void RemoveTransitionsWithoutStart()
        {
            for (int i = Transitions.Count - 1; i >= 0; i--)
            {
                if (States.Contains(Transitions[i].FirstState) == false)
                    Transitions.RemoveAt(i);
            }
        }

        /// <summary>
        /// Tests if the automaton is a DFA. A DFA is an automaton that, for each state, has one and only one outgoing
        /// transition. One for each letter of the alphabet.
        /// </summary>
        /// <returns></returns>
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
                    if (t.Label == "_")
                    {
                        if (IsAcceptedWord(word, t.SecondState))
                            return true;
                    }
                    else
                    {
                        if (IsAcceptedWord(word.Substring(1), t.SecondState))
                            return true;
                    }
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

        /// <summary>
        /// Writes the Automaton to a text file in the directory where the application is running.
        /// </summary>
        public string WriteToFile()
        {
            // Create a new unique datetimeSeed to act as a filename each time a new helper is created.
            string datetimeSeed = Utilities.CreateDatetimeSeed();

            // Get the path to application's directory.
            string outputFilesPath = Directory.GetCurrentDirectory() + "\\";

            // Create a string for the path to the new .txt file that will be created.
            string txtFilePath = outputFilesPath + "automaton" + datetimeSeed + ".txt";

            // Create the new file.
            var file = File.Create(txtFilePath);
            file.Close();

            // Write contents of the .txt file.
            using (TextWriter tw = new StreamWriter(txtFilePath))
            {
                // Alphabet
                tw.WriteLine("alphabet: " + Alphabet);

                // States.
                string statesLine = "states: ";
                foreach (var s in States)
                {
                    statesLine += s.StateName + ",";
                }
                tw.WriteLine(statesLine.TrimEnd(','));

                // Final states.
                string finalLine = "final: ";
                foreach (var s in States)
                {
                    if (s.IsFinal)
                    {
                        finalLine += s.StateName + ",";
                    }
                }
                tw.WriteLine(finalLine.TrimEnd(','));

                // Transitions header.
                tw.WriteLine("transitions:");
                // Lines for the Transitions.
                foreach (var t in Transitions)
                {
                    tw.WriteLine(string.Format("{0},{1} --> {2}", t.FirstState, t.Label, t.SecondState));
                }
                // Transitions ending.
                tw.WriteLine("end.");

                // White line
                tw.WriteLine();

                // Words header.
                tw.WriteLine("words:");
                // Words are unknown for a generated automaton file.
                // Leave an empty line to allow user to manually add them.
                tw.WriteLine();
                // Words ending.
                tw.WriteLine("end.");

                // White line
                tw.WriteLine();

                // DFA test vector.
                if (this.IsDFA())
                    tw.WriteLine("dfa:y");
                else
                    tw.WriteLine("dfa:n");
            }

            return txtFilePath;
        }

        #endregion
    }
}
