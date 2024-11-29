using System;
using System.Text;


static class Foo
{
    public static int Bar(int i)
    {
        switch (i)
        {
            case 0:
                return 40;
            case 1:
                return 41;
            case 2:
                return 42;
            default:
                return 43;
        }
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine(Foo.Bar(1));
    }
}