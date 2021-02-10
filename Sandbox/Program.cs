using System;
using Sandbox.FSharp;

namespace Sandbox.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var answer = FSharp.testFunctions.factorial(3);

            Console.WriteLine("C Sharp Main Called");
            Console.WriteLine("Factorial 3: " + answer.ToString());
        }
    }
}
