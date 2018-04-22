using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using System.Diagnostics;

namespace JinRi.LogCenter.Test
{

    public class HelloWorldTest
    {
        [Fact]
        public void Show()
        {
            string hw = "HelloWorld";
            Debug.WriteLine(hw);
            hw.ShouldBe("HelloWorld");
        }
    }
}
