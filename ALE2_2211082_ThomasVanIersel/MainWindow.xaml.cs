﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace ALE2_2211082_ThomasVanIersel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GraphvizHelper gh;
        Automaton automaton;

        Dictionary<string, bool> testWords;
        string selectedFilePath;
        ExecutionType executionType;

        public MainWindow()
        {
            InitializeComponent();

            gh = new GraphvizHelper();
            
            // Currently hard-coding file location for ease of testing.
            selectedFilePath = Directory.GetCurrentDirectory() + @"\Automaton Files\a3.txt";
            SetSelectedFileLabel();
            SetupListBox();
        }

        #region ------------------------------- UI Event Handlers ---------------------------------

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            switch (executionType)
            {
                case ExecutionType.SelectFile:
                    {
                        // Reset the test words variable and the test words list box.
                        testWords = new Dictionary<string, bool>();
                        SetupListBox();

                        // Do nothing if the path isn't set.
                        if (String.IsNullOrWhiteSpace(selectedFilePath))
                            return;

                        try
                        {
                            automaton = ReadAutomatonFile();
                        }
                        catch (IOException)
                        {
                            string errorMessage = "The selected file could not be opened or was not found.";
                            DisplayMessageBox(errorMessage);

                            return;
                        }

                        Dictionary<string, bool> testedWords = AutomatonUtilities.CheckTestWords(testWords.Keys.ToList(), automaton);
                        foreach (var pair in testedWords)
                        {
                            lbWordTesting.Items.Add(string.Format("{0}\t\t{1}\t\t{2}",
                                pair.Key,
                                pair.Value ? "Y" : "N",
                                testWords[pair.Key] ? "Y" : "N"));
                        }

                        break;
                    }
                case ExecutionType.RegularExpression:
                    {
                        string regularExpression = tbRegularExpression.Text;
                        
                        // Do nothing if the text box was empty.
                        if (string.IsNullOrWhiteSpace(regularExpression))
                            return;

                        // Remove spaces.
                        regularExpression = regularExpression.Replace(" ", "");

                        automaton = ReadRegularExpression(regularExpression);

                        if (automaton == null)
                        {
                            string errorMessage = "The regular expression is in an invalid format.";
                            DisplayMessageBox(errorMessage);
                            return;
                        }

                        break;
                    }
            }
            
            if (automaton.Transitions.All(t => t.Label == "_"))
            {
                // If the automaton only has epsilon transitions, it is considered invalid.
                string errorMessage = "The Automaton's transitions cannot all be epsilon (empty) transitions!";
                DisplayMessageBox(errorMessage);

                return;
            }

            lblIsDFA.Content = automaton.IsDFA() ? "Yes" : "No";
            lblIsDFA.Visibility = Visibility.Visible;

            // Use the GraphvizHelper object to generate the graph and display it in the ui.
            bool graphCreated = gh.CreateGraph(automaton);

            if (graphCreated == true)
            {
                // Apply the created .png file to the image element as a bitmap.
                graph.Source = gh.GetBitmapFromPng();
            }
            else
            {
                // If the graph was not created successfully, the system could not find GraphViz' "dot.exe".
                string errorMessage = "Couldn't find GraphViz' \"dot.exe\"! Please ensure you have it installed on your computer and have your PATH variables set up correctly.\n\n" +
                    "Alternatively, use the button on the Graph tab to set the path to your GraphViz  \"dot.exe\".";
                DisplayMessageBox(errorMessage);
            }

            if (automaton != null)
                btnWriteToFile.IsEnabled = true;
        }
        
        private void BtnSetDotPath_Click(object sender, RoutedEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select your GraphViz dot.exe";
                ofd.Filter = "exe files (*.exe)|*.exe";
                DialogResult result = ofd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    gh.DotProcessFileName = ofd.FileName;
                }
            }
        }

        private void BtnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select the automaton file";
                ofd.Filter = "txt files (*.txt)|*.txt";
                DialogResult result = ofd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    selectedFilePath = ofd.FileName;
                }
            }

            SetSelectedFileLabel();
        }

        private void BtnTestWord_Click(object sender, RoutedEventArgs e)
        {
            if (automaton == null)
                return;

            string word = tbTestWord.Text;
            bool isAccepted = automaton.IsAcceptedWord(word, automaton.States.First());

            lbWordTesting.Items.Insert(1, String.Format("{0}\t\t{1}\t\tN/A",
                    word,
                    isAccepted ? "Y" : "N"));
        }

        private void BtnWriteToFile_Click(object sender, RoutedEventArgs e)
        {
            if (automaton == null)
            {
                // If the automaton was not created, show an error message.
                string errorMessage = "No automaton found, please click execute again.";
                DisplayMessageBox(errorMessage);
            }

            try
            {
                string fileLocation = automaton.WriteToFile();
                DisplayMessageBox("Your automaton was saved at: \n\n" + fileLocation);
            }
            catch (IOException)
            {
                string errorMessage = "Something went wrong with writing the automaton to a file!";
                DisplayMessageBox(errorMessage);
            }
        }

        private void RbSelectFile_Checked(object sender, RoutedEventArgs e)
        {
            recSelectFile.Visibility = Visibility.Visible;
            recRegularExpression.Visibility = Visibility.Hidden;
            executionType = ExecutionType.SelectFile;
        }

        private void RbRegularExpression_Checked(object sender, RoutedEventArgs e)
        {
            recSelectFile.Visibility = Visibility.Hidden;
            recRegularExpression.Visibility = Visibility.Visible;
            executionType = ExecutionType.RegularExpression;
        }


        #endregion
        #region ------------------------------------ METHODS --------------------------------------

        /// <summary>
        /// Reads a text file containing an automaton. The text in the file needs to be in a specific format.
        /// </summary>
        /// <returns>A new Automaton object.</returns>
        private Automaton ReadAutomatonFile()
        {
            // Read all lines.
            string[] lines = System.IO.File.ReadAllLines(selectedFilePath);
            
            // Prepare variables to hold data from the file.
            string alphabet = "";
            List<State> states = new List<State>();
            List<string> finals = new List<string>();
            List<Transition> transitions = new List<Transition>();

            MultiLineType currentMultiLineType = MultiLineType.None;

            foreach (string line in lines)
            {
                // Ignore empty lines.
                if (line == "")
                    continue;

                // If the lines contains a colon, it indicates a special case such as the alphabet line or the finals line.
                if (line.Contains(':'))
                {
                    // Split the line into two parts. The left side of the colon is the identifier, the right side the value.
                    string[] splitLine = line.Split(':');

                    // Make sure the identifier is in lower case.
                    switch (splitLine[0].ToLower())
                    {
                        case "alphabet":
                            alphabet = splitLine[1].Trim();
                            break;
                        case "states":
                            splitLine[1].Split(',').Select(x => x.Trim()).ToList().ForEach(x => 
                            {
                                states.Add(new State(x));
                            });
                            break;
                        case "final":
                            finals = splitLine[1].Split(',').Select(x => x.Trim()).ToList();
                            foreach (State s in states)
                            {
                                if (finals.Contains(s.StateName))
                                    s.IsFinal = true;
                            }
                            break;
                        case "transitions":
                            // If the transitions line is found, all subsequent lines until the line containing "end." describe transitions.
                            currentMultiLineType = MultiLineType.Transitions;
                            break;
                        case "words":
                            // If the transitions line is found, all subsequent lines until the line containing "end." describe transitions.
                            currentMultiLineType = MultiLineType.Words;
                            break;
                    }
                }
                else if (line == "end.")
                {
                    currentMultiLineType = MultiLineType.None;
                }
                else if (currentMultiLineType == MultiLineType.Transitions)
                {
                    string[] commaSplitLine = line.Split(',');
                    State firstState = states.Find(s => s.StateName == commaSplitLine[0]);
                    State secondState = states.Find(s => s.StateName == line.Split('>')[1].Trim());
                    string label = commaSplitLine[1].First().ToString();

                    transitions.Add(new Transition(firstState, secondState, label));
                }
                else if (currentMultiLineType == MultiLineType.Words)
                {
                    string[] splitLine = line.Split(',');
                    testWords.Add(splitLine[0], splitLine[1].Trim() == "y");
                }
            }

            // Create a new Automaton and return it.
            return new Automaton(alphabet, states, transitions);
        }

        /// <summary>
        /// Creates an Automaton object from a regular expression.
        /// </summary>
        /// <param name="input">Regular expression in prefix notation.</param>
        /// <returns></returns>
        private Automaton ReadRegularExpression(string input)
        {
            // Check if the regular expression is in a valid format.
            if (Utilities.RegularExpressionIsValid(input) == false)
                return null;

            // Get the first character of the regular expression.
            string op = input[0].ToString();
            // Create a counter to use for unique State names.
            int stateCounter = 1;
            // Set up the initial and final states.
            State firstState = new State("S" + stateCounter++);
            State lastState = new State("S" + stateCounter++, true);

            try
            {
                automaton = AutomatonUtilities.CreateAutomaton(op, input, ref stateCounter, firstState, lastState);
            }
            catch (Exception)
            {
                return null;
            }
            
            return automaton;
        }

        /// <summary>
        ///  Changes the selected file label to the currently selected file's name.
        /// </summary>
        private void SetSelectedFileLabel()
        {
            int fileNameStart = selectedFilePath.LastIndexOf('\\') + 1;
            string fileName = Utilities.Slice(selectedFilePath, fileNameStart, selectedFilePath.Count());

            lblSelectedFile.Content = fileName;
        }

        /// <summary>
        /// Sets the header of the listbox.
        /// </summary>
        private void SetupListBox()
        {
            lbWordTesting.Items.Clear();
            lbWordTesting.Items.Add("Word:\t\tAccepted:\tFrom file:");
        }

        private void DisplayMessageBox(string message)
        {
            System.Windows.MessageBox.Show(message, "Error!");
        }

        #endregion
        #region ---------------------------------- Enumerations -----------------------------------

        private enum MultiLineType
        {
            None,
            Transitions,
            Words
        }

        private enum ExecutionType
        {
            SelectFile,
            RegularExpression
        }

        #endregion
        
    }
}
