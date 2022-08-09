using Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Meziantou.Framework.Win32;
using Fclp;
//using System.Security.Principal;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace RunGenerators
{
    public class Program
    {
        static bool Debug;

        static bool Pause;
        static string StdOutFilepath = string.Empty;
        static string ConfigFilePath = string.Empty;

        static Action<string> Log;

        static Action<string> Error;
        static TextWriter Out;
        static TextWriter ConsoleWriter;
        static void Main(string[] args)
        {
            if (args.Length != 0 && args.ToList().Any(t => t == "-d"))
            {
                Debugger.Launch();
            }
            ConsoleWriter = Console.Out;


            Log = LogMessage;
            Error = ErrorMessage;
            var p = new FluentCommandLineParser();

            p.Setup<bool>('d')
                .Callback(v => Debug = v)
                .SetDefault(false)
                .WithDescription("Launch debug in code");

            p.Setup<bool>('p')
                .Callback(v => Pause = v)
                .SetDefault(false)
                .WithDescription("Pause at end of consoleprogram")
                ;

            p.Setup<string>("stdoutpath")
                .Callback(v =>
                {
                    if (!String.IsNullOrEmpty(v))
                    { StdOutFilepath = Path.GetFullPath(v); }
                    else StdOutFilepath = String.Empty;
                })

                .SetDefault(string.Empty)
                .WithDescription("Stdoutput is also logged to this file.")
                ;

            p.Setup<string>("configpath")
                .Callback(v => ConfigFilePath = Path.GetFullPath(v))
                .Required()
                .WithDescription("The configurationpath to use for the generator.");

            // Arguments to the generator
            bool createExtraDockerFiles = false;
            bool createEnvFiles = false;
            bool createAppEnvironmentsFiles = false;
            bool setupLaunchSettings = false;
            bool addToHostsFile = false;
            bool createCertificates = false;

            p.Setup<bool>('o', "createExtraDockerFiles")
                .Callback(v => createExtraDockerFiles = v)
                .SetDefault(false)
                .WithDescription("Create extra docker files for use in public client.");

            p.Setup<bool>('e', "createEnvFiles")
                .Callback(v => createEnvFiles = v)
                .SetDefault(false)
                .WithDescription("Create environment files used by docker-compose and docker files.");

            p.Setup<bool>('a', "createAppEnvironmentsFiles")
                .Callback(v => createAppEnvironmentsFiles = v)
                .SetDefault(false)
                .WithDescription("Create AppEnvironment files used to replace AppEnvironment.cs on build for blazor client.");

            p.Setup<bool>('l', "setupLaunchSettings")
                .Callback(v => setupLaunchSettings = v)
                .SetDefault(false)
                .WithDescription("Replace properties in launchSettings.json files.");

            p.Setup<bool>('h', "addToHostsFile")
                .Callback(v => addToHostsFile = v)
                .SetDefault(false)
                .WithDescription("Adds all domains to hosts file.");

            p.Setup<bool>('c', "createCertificates")
                .Callback(v => createCertificates = v)
                .SetDefault(false)
                .WithDescription("Create certificates and add them to trusted root provider in system.");

            p.SetupHelp("?", "help")
            .Callback(text => Console.WriteLine(text));


            var result = p.Parse(args);


            if (result.HasErrors == true)
            {
                Console.WriteLine(result.ErrorText);
                // use the instantiated ApplicationArguments object from the Object property on the parser.
                ValidatePause();

                return;
            }


            var errors = new List<string>();

            if (!File.Exists(ConfigFilePath))
            {
                errors.Add($"Can't find the ConfigFilePath: {ConfigFilePath}");
            }
            if (!string.IsNullOrEmpty(StdOutFilepath) && !Directory.Exists(Path.GetDirectoryName(StdOutFilepath)))
            {
                errors.Add($"Can't find the directory of StdOutFilepath: {StdOutFilepath}");
            }

            if (errors.Any())
            {
                var error = string.Join(Environment.NewLine, errors);
                Console.WriteLine(error);
                ValidatePause();
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(StdOutFilepath))
                {
                    if (!File.Exists(StdOutFilepath))
                    {
                        File.WriteAllText(StdOutFilepath, string.Empty);
                    }
                    var fileAppend = File.AppendText(StdOutFilepath);
                    Out = new CustomTextwriter(fileAppend, Console.Out);
                    Console.SetOut(Out);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ValidatePause();
                return;

            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Starting program at Time: {DateTime.Now}");

            Console.WriteLine("Is Admin: " + IsUserAdministrator());
            if (args.Length == 0)
            {
                Error("You must give a path to the config.json file");
                ValidatePause();
                return;
            }

            var path = ConfigFilePath;



            var fullPath = Path.GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                // Try relative path
                //var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                //fullPath = Path.GetFullPath(Path.Combine(currentDirectory, path));
                //if (!File.Exists(fullPath))
                //{
                Error($"Can't find filepath {fullPath}");
                ValidatePause();
                return;
                //}
            }
            var generateDomainUris = new GenerateDomainUris();
            generateDomainUris.Initialize(fullPath);


            generateDomainUris.Execute(createExtraDockerFiles,createEnvFiles, createAppEnvironmentsFiles, setupLaunchSettings, addToHostsFile, createCertificates);

            Console.WriteLine(string.Empty);
            Console.WriteLine($"{DateTime.Now}: Code Generattion has finnished.");

            if (HasErrors)
            {
                Console.WriteLine($"{DateTime.Now}: Some errors occured.");

            }
            

            ValidatePause();

            if (Out != null)
                Out.Close();


        }
        static void ValidatePause()
        {
            if (Pause)
            {
                Console.WriteLine("press enter to continue.");
                Console.ReadLine();
            }
        }
        static void LogMessage(string m)
        {

            Console.WriteLine(m);

        }
        //public static bool IsAdministrator2()
        //{
        //    var identity = WindowsIdentity.GetCurrent();
        //    var principal = new WindowsPrincipal(identity);
        //    return principal.IsInRole(WindowsBuiltInRole.Administrator);
        //}
        public static bool IsUserAdministrator()
        {
            using var token = AccessToken.OpenCurrentProcessToken(TokenAccessLevels.Query);
            if (!IsAdministrator(token) && token.GetElevationType() == TokenElevationType.Limited)
            {
                using var linkedToken = token.GetLinkedToken();
                return IsAdministrator(linkedToken);
            }

            return false;

            static bool IsAdministrator(AccessToken accessToken)
            {
                var adminSid = SecurityIdentifier.FromWellKnown(WellKnownSidType.WinBuiltinAdministratorsSid);
                foreach (var group in accessToken.EnumerateGroups())
                {
                    if (group.Attributes.HasFlag(GroupSidAttributes.SE_GROUP_ENABLED) && group.Sid == adminSid)
                        return true;
                }
                return false;
            }
        }
        static bool HasErrors;
        static void ErrorMessage(string error)
        {
            HasErrors = true;
            LogMessage($"An error occured:{Environment.NewLine}{error}{Environment.NewLine}");

        }
    }
}
