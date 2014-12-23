using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Quote
    {
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public decimal Last { get; set; }

        public override string ToString()
        {
            return "[ Ask = " + Ask + " Bid = " + Bid + " Last = " + Last + " ]";
        }
    }
}
