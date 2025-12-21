using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fairmark.Intelligence.Models
{
    public class LLMModelInfo : IEquatable<LLMModelInfo>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public bool Equals(LLMModelInfo other)
        {
            if (other == null)
                return false;
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LLMModelInfo);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}
