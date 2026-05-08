using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal class FindReplaceValue
    {
        public string Token { get; set; }

        public string Value { get; set; }

        public FindReplaceValue(string token, string value)
        {
            Token = token;
            Value = value;
        }
    }
}
