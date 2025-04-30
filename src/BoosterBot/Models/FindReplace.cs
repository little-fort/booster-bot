using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal class FindReplaceValue(string token, string value)
    {
        public string Token { get; set; } = token;
        public string Value { get; set; } = value;
        public string Find { get; set; } = string.Empty;
        public string Replace { get; set; } = string.Empty;
    }
}
