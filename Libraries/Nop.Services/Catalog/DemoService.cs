using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Catalog
{
    public class DemoService : IDemoService
    {
        public void TestMethod()
        {
            Console.WriteLine("test method");
        }
    }

    public interface IDemoService {
        void TestMethod();
    }
}
