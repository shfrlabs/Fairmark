using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fairmark.Intelligence.Models
{
    public enum LLMChatRole
    {
        User,
        Assistant
    }

    public class LLMChatMessage
    {
        public LLMChatRole Role { get; set; }
        public string Content { get; set; }
    }
}
