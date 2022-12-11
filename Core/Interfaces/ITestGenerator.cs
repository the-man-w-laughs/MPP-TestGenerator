using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    internal interface ITestGenerator
    {
        public List<string> GenerateTests(string code);
    }
}
