using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot.Models
{
    internal class RepairPrompt
    {
        public string Description { get; set; }
        public List<string> Files { get; set; }
        public List<Func<IdentificationResult>> IdentifyFuncs { get; set; }

        public RepairPrompt(string description, List<string> files, List<Func<IdentificationResult>> identifyFuncs)
        {
            Description = description;
            Files = files.Select(x => x.Split('\\')[^1]).ToList();
            IdentifyFuncs = identifyFuncs;
        }
    }
}
