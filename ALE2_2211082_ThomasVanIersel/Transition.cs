using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2_2211082_ThomasVanIersel
{
    class Transition
    {
        public State FirstState { get; set; }
        public State SecondState { get; set; }
        public string Label { get; set; }

        public Transition(State firstState, State secondState, string label)
        {
            FirstState = firstState;
            SecondState = secondState;
            Label = label;
        }
    }
}
