using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE2_2211082_ThomasVanIersel
{
    class State
    {
        public string StateName { get; set; }
        public bool IsFinal { get; set; }

        public State(string stateName, bool isFinal = false)
        {
            StateName = stateName;
            IsFinal = isFinal;
        }

        public override string ToString()
        {
            return StateName;
        }
    }
}
