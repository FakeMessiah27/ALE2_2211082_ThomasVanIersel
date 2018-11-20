using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ALE2_2211082_ThomasVanIersel
{
    class GraphvizHelper
    {
        string datetimeSeed;

        public string DotProcessFileName { get; set; }

        public GraphvizHelper()
        { }

        public bool CreateGraph(Automaton automaton)
        {
            // Create a new unique datetimeSeed to act as a filename each time a new helper is created.
            datetimeSeed = CreateDatetimeSeed();

            // Get the path to application's directory.
            string outputFilesPath = Directory.GetCurrentDirectory() + "\\Outputs\\";

            // Create a string for the path to the new .dot file that will be created.
            string dotfilePath = outputFilesPath + datetimeSeed + ".dot";

            // Create the new file.
            var file = File.Create(dotfilePath);
            file.Close();

            // Write contents of .dot file for the graph.
            using (TextWriter tw = new StreamWriter(dotfilePath))
            {
                // Write standard header for .dot graph file.
                tw.WriteLine("digraph myAutomaton {");
                tw.WriteLine("rankdir=LR;");
                tw.WriteLine("\"\" [shape=none]");

                // Use recursion to write correct lines to .dot graph file.
                WriteAutomaton(tw, automaton);

                // Write standard footer line for .dot graph file.
                tw.WriteLine("}");
            }

            // Create a png file from the above created .dot graph file.
            return CreatePng(outputFilesPath);
        }

        /// <summary>
        /// Creates a PNG image from a .dot file using the Graphviz application.
        /// </summary>
        /// <param name="outputFilesPath">Path to the folder containing the .dot file. This will also be the destination for the newly created image.</param>
        private bool CreatePng(string outputFilesPath)
        {
            Process dot = new Process();

            if (String.IsNullOrWhiteSpace(DotProcessFileName) == false)
            {
                if (DotProcessFileName.Contains("dot.exe") == true)
                    dot.StartInfo.FileName = DotProcessFileName;
                else
                    return false;
            }
            else
            {
                dot.StartInfo.FileName = @"dot.exe";
            }

            dot.StartInfo.Arguments = String.Format("-Tpng -o{0}.png {0}.dot", outputFilesPath + "\\" + datetimeSeed);

            try
            {
                dot.Start();
                dot.WaitForExit();
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a string of numbers from the current system date and time.
        /// </summary>
        /// <returns></returns>
        private string CreateDatetimeSeed()
        {
            string datetime = DateTime.Now.ToString();
            string datetimeSeed = "";

            // Remove any special characters.
            foreach (char c in datetime)
            {
                if (Char.IsLetterOrDigit(c) == true)
                    datetimeSeed += c;
            }

            return datetimeSeed;
        }

        /// <summary>
        /// Takes an Automaton object and writes all the lines required for GraphViz to draw it as a PNG into a file.
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="automaton"></param>
        private void WriteAutomaton(TextWriter tw, Automaton automaton)
        {
            // Write the lines for all of the states.
            foreach (State s in automaton.States)
            {
                // Final states get a double circle, non-final states get a normal circle.
                // Ex: "A" [shape=doublecircle]
                if (s.IsFinal)
                    tw.WriteLine(String.Format("\"{0}\" [shape=doublecircle]", s));
                else
                    tw.WriteLine(String.Format("\"{0}\" [shape=circle]", s));
            }

            // Write the line for the entry transition (which is always the first state of the Automaton.
            // Ex: "" -> "C"
            tw.WriteLine(String.Format("\"\" -> \"{0}\"", automaton.States.First()));

            // Write the lines for all of the transitions.
            foreach (Transition t in automaton.Transitions)
            {
                // An empty transition is recorded with an underscore ("_"); it will be written in the graph with an epsilon ("ε").
                // Ex: "C" -> "A" [label="ε"] (This would be a transition from state C to state A with label ε).
                if (t.Label == "_")
                    tw.WriteLine(String.Format("\"{0}\" -> \"{1}\" [label=\"ε\"]", t.FirstState, t.SecondState));
                else
                    tw.WriteLine(String.Format("\"{0}\" -> \"{1}\" [label=\"{2}\"]", t.FirstState, t.SecondState, t.Label));
            }
        }

        /// <summary>
        /// Gets the bitmap from the previously created png file.
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetBitmapFromPng()
        {
            var imageUri = new Uri(Directory.GetCurrentDirectory() + "\\Outputs\\" + datetimeSeed + ".png");
            var bitmap = new BitmapImage();

            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.EndInit();
            }
            catch (Exception)
            {
                return null;
            }

            return bitmap;
        }
    }
}
