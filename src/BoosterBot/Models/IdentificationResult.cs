using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal struct IdentificationResult(bool isMatch, List<string> logs)
    {
        public bool IsMatch { get; set; } = isMatch;
        public List<string> Logs { get; set; } = logs ?? new List<string>();
    }
}
