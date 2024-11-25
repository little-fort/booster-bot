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
        public Func<IdentificationResult> Identify { get; set; }

        public RepairPrompt(string description, List<string> files, Func<IdentificationResult> identify)
        {
            Description = description;
            Files = files.Select(x => x.Split('\\')[^1]).ToList();
            Identify = identify;
        }
    }
}
