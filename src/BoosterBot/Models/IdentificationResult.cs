using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal struct IdentificationResult
    {
        public bool IsMatch { get; set; }
        public List<string> Logs { get; set; }

        public IdentificationResult(bool isMatch, List<string> logs)
        {
            IsMatch = isMatch;
            Logs = logs ?? new List<string>();
        }
    }
}
