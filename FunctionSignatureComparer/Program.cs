

using System;
using System.IO;

namespace FunctionSignatureComparer
{
    class Program
    {
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You Shoud Determine a Git Repository Path as Parameter(Contain .git directory)");
                return;
            }
            if (CheckFolderPathValidation(args[0]) == CheckResult.Valid)
            {
                Console.WriteLine($"\n\nYou Select {args[0]} Path ...\n");
                Console.WriteLine($"Starting process ...");
                Console.WriteLine($".");
                Console.WriteLine($".");
                var signatureComparer = new FunctionSignatureComparer(args[0], string.Empty);
                signatureComparer.CheckParameterIsAddedToSignature();
                Console.WriteLine($"Process Completed ... \n");
                return;
            }

        }


        private static CheckResult CheckFolderPathValidation(string path)
        {
            Console.WriteLine(path + "/.git");
            if (Directory.Exists(path) && Directory.Exists(path + "/.git"))
                return CheckResult.Valid;
            else if(!Directory.Exists(path))
                Console.WriteLine($"{path} path is not valid.");
            else
                Console.WriteLine(".git Directory Does Not Exist in This Path.");

            return CheckResult.Invalid;
        }


    }
}
