﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2_2211082_ThomasVanIersel
{
    class AutomatonUtilities
    {
        /// <summary>
        /// Creates an Automaton object based on a regular expression.
        /// </summary>
        /// <param name="op">Operator of the regular expression.</param>
        /// <param name="input">Regular Expression.</param>
        /// <param name="stateCounter">A counter to create unique state names.</param>
        /// <param name="firstState">First state of the Automaton.</param>
        /// <param name="lastState">Last state of the Automaton.</param>
        /// <returns></returns>
        public static Automaton CreateAutomaton(string op, string input, ref int stateCounter, State firstState, State lastState)
        {
            switch (op)
            {
                case ".":
                    return CreateConcatenationAutomaton(input, ref stateCounter, firstState, lastState);
                case "|":
                    return CreateUnionAutomaton(input, ref stateCounter, firstState, lastState);
                case "*":
                    return CreateKleeneStarAutomaton(input, ref stateCounter, firstState, lastState);
                case "_":
                default:
                    return CreateSingleSymbolAutomaton(op, ref stateCounter, firstState, lastState);
            }
        }

        /// <summary>
        /// Creates an Automaton for a concatenation regular expression.
        /// </summary>
        /// <param name="input">Regular Expression.</param>
        /// <param name="stateCounter">A counter to create unique state names.</param>
        /// <param name="firstState">First state of the Automaton.</param>
        /// <param name="lastState">Last state of the Automaton.</param>
        /// <returns></returns>
        private static Automaton CreateConcatenationAutomaton(string input, ref int stateCounter, State firstState, State lastState)
        {
            Automaton leftAutomaton = null;
            Automaton rightAutomaton = null;

            // Create the middle state that will sit inbetween the two parts of the concatenation
            State middleState = new State("S" + stateCounter++);

            // Find the position of the comma that seperates the two sides of the regular expression.
            int middleCommaPosition = FindMiddleCommaPosition(input);
            // Store the two sides of the regular expression.
            string leftOperand = Utilities.Slice(input, 2, middleCommaPosition);
            string rightOperand = Utilities.Slice(input, middleCommaPosition + 1, input.LastIndexOf(')'));

            Transition leftTransition = null;
            if (leftOperand.Contains("("))
            {
                // If the left part of the regular expression contains more opening brackets, it means it is a sub-regular expression.
                // In this case, we will call the main CreateAutomaton method to create a sub-automaton out of the left part of the RE.
                string op = leftOperand[0].ToString();
                leftAutomaton = CreateAutomaton(op, leftOperand, ref stateCounter, firstState, middleState);
            }
            else
            {
                // If no brackets were found, we have reached a symbol, for which we will create a transition.
                leftTransition = CreateTransition(firstState, middleState, leftOperand);
            }

            Transition rightTransition = null;
            if (rightOperand.Contains("("))
            {
                // If the right part of the regular expression contains more opening brackets, it means it is a sub-regular expression.
                // In this case, we will call the main CreateAutomaton method to create a sub-automaton out of the right part of the RE.
                string op = rightOperand[0].ToString();
                rightAutomaton = CreateAutomaton(op, rightOperand, ref stateCounter, middleState, lastState);
            }
            else
            {
                // If no brackets were found, we have reached a symbol, for which we will create a transition.
                rightTransition = CreateTransition(middleState, lastState, rightOperand);
            }

            // Take all the unique letters from the right and left parts of the RE and add them together to form the Automaton's alphabet.
            leftOperand = Utilities.GetUniqueChars(leftOperand);
            rightOperand = Utilities.GetUniqueChars(rightOperand);
            string alphabet = Utilities.AddUniqueCharsToString(leftOperand, rightOperand);
            // Set up the lists of states and transitions.
            List<State> states = new List<State>()
            {
                firstState,
                middleState,
                lastState
            };
            List<Transition> transitions = new List<Transition>();

            // The left and right transitions can be null if their respective part of the RE was a sub-RE.
            // We only want to add the transition if it was actually assigned.
            if (leftTransition != null)
                transitions.Add(leftTransition);
            if (rightTransition != null)
                transitions.Add(rightTransition);

            // Create the concatenation Automaton.
            Automaton returnAutomaton = new Automaton(alphabet, states, transitions);

            // If the left or right side of the RE was a sub-RE it will have a sub-Automaton. 
            // We need to add that sub-Automaton to our returnAutomaton.
            if (leftAutomaton != null)
            {
                AddAutomatonToAutomaton(returnAutomaton, leftAutomaton);
            }
            if (rightAutomaton != null)
            {
                AddAutomatonToAutomaton(returnAutomaton, rightAutomaton);
            }

            return returnAutomaton;
        }

        /// <summary>
        /// Creates an Automaton for a union regular expression.
        /// </summary>
        /// <param name="input">Regular Expression.</param>
        /// <param name="stateCounter">A counter to create unique state names.</param>
        /// <param name="firstState">First state of the Automaton.</param>
        /// <param name="lastState">Last state of the Automaton.</param>
        /// <returns></returns>
        private static Automaton CreateUnionAutomaton(string input, ref int stateCounter, State firstState, State lastState)
        {
            Automaton leftAutomaton = null;
            Automaton rightAutomaton = null;

            // Set up the necessary standard states for a union.
            State topLeft = new State("S" + stateCounter++);
            State topRight = new State("S" + stateCounter++);
            State bottomLeft = new State("S" + stateCounter++);
            State bottomRight = new State("S" + stateCounter++);

            // Set up the necessary standard transitions for a union.
            Transition topLeftTrans = new Transition(firstState, topLeft, "_");
            Transition topRightTrans = new Transition(topRight, lastState, "_");
            Transition bottomLeftTrans = new Transition(firstState, bottomLeft, "_");
            Transition bottomRightTrans = new Transition(bottomRight, lastState, "_");

            // Find the position of the comma that seperates the two sides of the regular expression.
            int middleCommaPosition = FindMiddleCommaPosition(input);
            // Store the two sides of the regular expression.
            string leftOperand = Utilities.Slice(input, 2, middleCommaPosition);
            string rightOperand = Utilities.Slice(input, middleCommaPosition + 1, input.LastIndexOf(')'));

            Transition topTransition = null;
            if (leftOperand.Contains("("))
            {
                // If the left part of the regular expression contains more opening brackets, it means it is a sub-regular expression.
                // In this case, we will call the main CreateAutomaton method to create a sub-automaton out of the left part of the RE.
                string op = leftOperand[0].ToString();
                leftAutomaton = CreateAutomaton(op, leftOperand, ref stateCounter, topLeft, topRight);
            }
            else
            {
                // If no brackets were found, we have reached a symbol, for which we will create a transition.
                topTransition = CreateTransition(topLeft, topRight, leftOperand);
            }

            Transition bottomTransition = null;
            if (rightOperand.Contains("("))
            {
                // If the right part of the regular expression contains more opening brackets, it means it is a sub-regular expression.
                // In this case, we will call the main CreateAutomaton method to create a sub-automaton out of the right part of the RE.
                string op = rightOperand[0].ToString();
                rightAutomaton = CreateAutomaton(op, rightOperand, ref stateCounter, bottomLeft, bottomRight);
            }
            else
            {
                // If no brackets were found, we have reached a symbol, for which we will create a transition.
                bottomTransition = CreateTransition(bottomLeft, bottomRight, rightOperand);
            }

            // Take all the unique letters from the right and left parts of the RE and add them together to form the Automaton's alphabet.
            leftOperand = Utilities.GetUniqueChars(leftOperand);
            rightOperand = Utilities.GetUniqueChars(rightOperand);
            string alphabet = Utilities.AddUniqueCharsToString(leftOperand, rightOperand);
            // Set up the lists of states and transitions.
            List<State> states = new List<State>()
            {
                firstState,
                topLeft,
                topRight,
                bottomLeft,
                bottomRight,
                lastState
            };
            List<Transition> transitions = new List<Transition>()
            {
                topLeftTrans,
                topRightTrans,
                bottomLeftTrans,
                bottomRightTrans
            };

            // The left and right transitions can be null if their respective part of the RE was a sub-RE.
            // We only want to add the transition if it was actually assigned.
            if (topTransition != null)
                transitions.Add(topTransition);
            if (bottomTransition != null)
                transitions.Add(bottomTransition);

            // Create the union Automaton.
            Automaton returnAutomaton = new Automaton(alphabet, states, transitions);

            // If the left or right side of the RE was a sub-RE it will have a sub-Automaton. 
            // We need to add that sub-Automaton to our returnAutomaton.
            if (leftAutomaton != null)
            {
                AddAutomatonToAutomaton(returnAutomaton, leftAutomaton);
            }

            if (rightAutomaton != null)
            {
                AddAutomatonToAutomaton(returnAutomaton, rightAutomaton);
            }

            return returnAutomaton;
        }

        /// <summary>
        /// Create an Automaton for a Kleene star regular expression.
        /// </summary>
        /// <param name="input">Regular Expression.</param>
        /// <param name="stateCounter">A counter to create unique state names.</param>
        /// <param name="firstState">First state of the Automaton.</param>
        /// <param name="lastState">Last state of the Automaton.</param>
        /// <returns></returns>
        private static Automaton CreateKleeneStarAutomaton(string input, ref int stateCounter, State firstState, State lastState)
        {
            Automaton subAutomaton = null;

            // Set up the necessary standard states for a Kleene star.
            State leftState = new State("S" + stateCounter++);
            State rightState = new State("S" + stateCounter++);

            // Set up the necessary standard transitions for a Kleene star.
            Transition firstToLast = new Transition(firstState, lastState, "_");
            Transition rightToLeft = new Transition(rightState, leftState, "_");
            Transition firstToLeft = new Transition(firstState, leftState, "_");
            Transition rightToLast = new Transition(rightState, lastState, "_");

            // Extract the operand from between the brackets of the regular expression.
            string operand = Utilities.Slice(input, 2, input.LastIndexOf(')'));

            Transition leftToRight = null;
            if (operand.Contains("("))
            {
                // If the regular expression contains more opening brackets, it means it has a sub-regular expression.
                // In this case, we will call the main CreateAutomaton method to create a sub-automaton out of the sub-RE.
                string op = operand[0].ToString();
                subAutomaton = CreateAutomaton(op, operand, ref stateCounter, leftState, rightState);
            }
            else
            {
                // If no brackets were found, we have reached a symbol, for which we will create a transition.
                leftToRight = CreateTransition(leftState, rightState, operand);
            }

            // Take all the unique letters from the RE and store it as the alphabet for the Automaton.
            operand = Utilities.GetUniqueChars(operand);
            string alphabet = Utilities.AddUniqueCharsToString("", operand);
            // Set up the lists of states and transitions.
            List<State> states = new List<State>()
            {
                firstState,
                leftState,
                rightState,
                lastState
            };
            List<Transition> transitions = new List<Transition>()
            {
                firstToLast,
                rightToLeft,
                firstToLeft,
                rightToLast
            };

            // The left to right transition can be null if the RE contains a sub-RE.
            // We only want to add the transition if it was actually assigned.
            if (leftToRight != null)
                transitions.Add(leftToRight);

            // Create the Kleene star automaton.
            Automaton returnAutomaton = new Automaton(alphabet, states, transitions);

            // If the RE contained a sub-RE there will be a sub-Automaton.
            // We need to add that sub-Automaton to our returnAutomaton.
            if (subAutomaton != null)
            {
                AddAutomatonToAutomaton(returnAutomaton, subAutomaton);
            }

            return returnAutomaton;
        }

        /// <summary>
        /// Create an Automaton for a single symbol regular expression.
        /// </summary>
        /// <param name="input">Regular Expression.</param>
        /// <param name="stateCounter">A counter to create unique state names.</param>
        /// <param name="firstState">First state of the Automaton.</param>
        /// <param name="lastState">Last state of the Automaton.</param>
        /// <returns></returns>
        private static Automaton CreateSingleSymbolAutomaton(string input, ref int stateCounter, State firstState, State lastState)
        {
            // Take the symbol and store it as the Automaton's alphabet.
            input = Utilities.GetUniqueChars(input);
            string alphabet = Utilities.AddUniqueCharsToString("", input);
            // Set up the lists of states and transitions.
            List<State> states = new List<State>()
            {
                firstState,
                lastState
            };
            List<Transition> transitions = new List<Transition>()
            {
                new Transition(firstState, lastState, input)
            };

            return new Automaton(alphabet, states, transitions);
        }

        /// <summary>
        /// Adds one Automaton object to another by adding all the States and Transitions of the automatonToAdd to the
        /// lists of States and Transitions of the first automaton.
        /// </summary>
        /// <param name="automaton">Automaton object that will receive the contents of the second.</param>
        /// <param name="automatonToAdd">Automaton object who's contents will be added onto the first.</param>
        private static void AddAutomatonToAutomaton(Automaton automaton, Automaton automatonToAdd)
        {
            automatonToAdd.States.ForEach(s => automaton.States.Add(s));
            automatonToAdd.Transitions.ForEach(t => automaton.Transitions.Add(t));
            automaton.Alphabet = Utilities.AddUniqueCharsToString(automaton.Alphabet, automatonToAdd.Alphabet);
        }

        /// <summary>
        /// Finds the index position of the middle comma, separating the left and right operands of the formula.
        /// </summary>
        /// <param name="input">Formula in infix notation.</param>
        /// <returns></returns>
        private static int FindMiddleCommaPosition(string input)
        {
            int nrOfOpeningBrackets = 0;
            int nrOfClosingBrackets = 0;
            int middleCommaPosition = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '(')
                    nrOfOpeningBrackets++;
                else if (input[i] == ')')
                    nrOfClosingBrackets++;
                else if (input[i] == ',')
                {
                    if (nrOfOpeningBrackets == nrOfClosingBrackets + 1)
                        middleCommaPosition = i;
                }
            }

            return middleCommaPosition;
        }

        /// <summary>
        /// Creates a new transition between the first and second state.
        /// Throws an Exception if the label is empty.
        /// </summary>
        /// <param name="firstState"></param>
        /// <param name="secondState"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private static Transition CreateTransition(State firstState, State secondState, string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new Exception();
            else
                return new Transition(firstState, secondState, label);
        }

        /// <summary>
        /// Run all the test words found in the automaton text file through the word checker method.
        /// </summary>
        /// <param name="words"></param>
        /// <returns>Returns a Dictionary of string/bool. The string is the word that was checked and the bool indicates 
        /// if it was accepted by the automaton or not.</returns>
        public static Dictionary<string, bool> CheckTestWords(List<string> words, Automaton automaton)
        {
            var result = new Dictionary<string, bool>();

            foreach (string word in words)
            {
                result.Add(word, automaton.IsAcceptedWord(word, automaton.States.First()));
            }

            return result;
        }
    }
}
