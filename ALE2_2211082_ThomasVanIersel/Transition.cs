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
        public string StackPopSymbol { get; set; }
        public string StackPushSymbol { get; set; }

        public Transition(State firstState, State secondState, string label, string stackPopSymbol = null, string stackPushSymbol = null)
        {
            FirstState = firstState;
            SecondState = secondState;
            Label = label;
            StackPopSymbol = stackPopSymbol;
            StackPushSymbol = stackPushSymbol;
        }
    }
}
