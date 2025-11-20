using System.Collections.Generic;
using Rampastring.Tools;

namespace TSMapEditor.Models
{
    public class AITargetType
    {
        public AITargetType()
        {
        }

        public AITargetType(List<string> technoNames)
        {
            TechnoNames = technoNames ?? new List<string>();
        }

        public List<string> TechnoNames { get; set; } = new List<string>();

        public void ReadFromIniString(string iniString)
        {
            if (string.IsNullOrWhiteSpace(iniString))
                return;

            var tokens = iniString.Split(new[] { ',' });
            TechnoNames.Clear();
            foreach (var token in tokens)
            {
                var trimmed = token.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    TechnoNames.Add(trimmed);
            }
        }

        public string WriteToIniString()
        {
            return string.Join(",", TechnoNames);
        }
    }
}