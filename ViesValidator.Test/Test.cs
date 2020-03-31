using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;
using System.Threading;

namespace ViesValidator.Test
{
    class Test
    {
        static void Main(string[] args)
        {
            ViesValidator.Application viesValidator = new Application();
            viesValidator.Run("LT", "5410003271", "PL", "5410003271",  100);
            Console.WriteLine(viesValidator.State);
            Console.WriteLine(viesValidator.Comment);
            Console.ReadLine();
        }

    }
}
