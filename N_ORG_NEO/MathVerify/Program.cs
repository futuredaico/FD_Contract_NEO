using System;
using System.Collections.Generic;

namespace MathVerify
{
    class Program
    {
        private static Dictionary<string, Func<bool>> methods = new Dictionary<string, Func<bool>>();

        static void Main(string[] args)
        {
            InitMethods();
            ShowMethods();
            while (true)
            {
                try
                {
                    var key = Console.ReadLine();
                    methods.TryGetValue(key,out var value);
                    value();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }

        static void InitMethods()
        {
            methods.Add("sqrt",Sqrt);
        }

        static void ShowMethods()
        {
            foreach (var k in methods.Keys)
            {
                Console.WriteLine("输入   " + k + "   进入对应的测试");
            }
        }

        static bool Sqrt()
        {
            Console.WriteLine("输入x");
            uint x = uint.Parse(Console.ReadLine());
            uint z = (x+1) / 2;
            uint y = x;
            while (z < y)
            {
                y = z;
                z = (x / z + z) / 2;
            }
            Console.WriteLine("结果："+y);
            return true;
        }

    }
}
