namespace BoosterBot.Models
{
    internal class RepairPrompt(string description, List<string> files, List<Func<IdentificationResult>> identifyFuncs)
    {
        public string Description { get; set; } = description;
        public List<string> Files { get; set; } = files.Select(x => x.Split('\\')[^1]).ToList();
        public List<Func<IdentificationResult>> IdentifyFuncs { get; set; } = identifyFuncs;
    }
}
