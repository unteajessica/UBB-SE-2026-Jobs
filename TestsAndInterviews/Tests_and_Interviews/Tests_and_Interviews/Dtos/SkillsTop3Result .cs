using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Dtos
{
    public class SkillsTop3Result
    {
        public List<string> SkillNames { get; set; } = new();
        public List<int> Percents { get; set; } = new();
    }
}
