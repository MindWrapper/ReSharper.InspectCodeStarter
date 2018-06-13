namespace RCLTStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            // command line example: --base-dir=c:\RCLTTest c:\temp\classlib\classlib.sln --output=c:\temp\res.xml
            new InspectCodeRunner(args).Run();
        }
    }
}
