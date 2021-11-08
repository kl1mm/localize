using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kli.Localize.Generator.Internal;
using Xunit;

namespace kli.Localize.Test
{
    public class GeneratorTest
    {
        // TODO: Test LocalizeCodeGenerator
        [Fact]
        public void Name()
        {
            string name = "Key with some {0} placeholders for {1}";
            var identifier = PropertyNameChangePattern.Change(name);
        }
    }
}
