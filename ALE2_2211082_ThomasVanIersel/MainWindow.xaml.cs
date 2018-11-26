using System;
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
            // Reset the test words variable and the test words list box.
            testWords = new Dictionary<string, bool>();
            SetupListBox();

            // Do nothing if the path isn't set.
            if (String.IsNullOrWhiteSpace(selectedFilePath))
                return;

            automaton = ReadAutomatonFile();

            if (automaton.Transitions.All(t => t.Label == "_"))
            {
                // If the automaton only has epsilon transitions, it is considered invalid.
                string errorMessage = "The Automaton's transitions cannot all be epsilon (empty) transitions!";
                System.Windows.MessageBox.Show(errorMessage, "Error!");
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
                System.Windows.MessageBox.Show(errorMessage, "Error!");
            }

            Dictionary<string, bool> testedWords = CheckTestWords(testWords.Keys.ToList());
            foreach (var pair in testedWords)
            {
                lbWordTesting.Items.Add(String.Format("{0}\t\t{1}\t\t{2}", 
                    pair.Key, 
                    pair.Value ? "Y" : "N", 
                    testWords[pair.Key] ? "Y" : "N"));
            }
        }

        private void BtnSetPath_Click(object sender, RoutedEventArgs e)
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

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
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
        /// Run all the test words found in the automaton text file through the word checker method.
        /// </summary>
        /// <param name="words"></param>
        /// <returns>Returns a Dictionary of string/bool. The string is the word that was checked and the bool indicates 
        /// if it was accepted by the automaton or not.</returns>
        private Dictionary<string, bool> CheckTestWords(List<string> words)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            foreach (string word in words)
            {
                result.Add(word, automaton.IsAcceptedWord(word, automaton.States.First()));
            }

            return result;
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

        #endregion
        #region ---------------------------------- Enumerations -----------------------------------

        private enum MultiLineType
        {
            None,
            Transitions,
            Words
        }

        #endregion
    }
}
