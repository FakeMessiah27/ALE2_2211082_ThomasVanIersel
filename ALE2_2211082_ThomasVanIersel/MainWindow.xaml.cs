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
        string selectedFilePath;

        public MainWindow()
        {
            InitializeComponent();

            gh = new GraphvizHelper();
            selectedFilePath = Directory.GetCurrentDirectory() + @"\Automaton Files\a2.txt";
            SetSelectedFileLabel();
        }

        #region ------------------------------- UI Event Handlers ---------------------------------

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing if the path isn't set.
            if (String.IsNullOrWhiteSpace(selectedFilePath))
                return;

            Automaton automaton = ReadAutomatonFile();

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
                    // Do word stuff.
                }
            }

            // Create a new Automaton and return it.
            return new Automaton(alphabet, states, transitions);
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
