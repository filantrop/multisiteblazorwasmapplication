using Microsoft.CodeAnalysis;
using SourceGenerators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.
namespace Generators
{
    [Generator]
    public class GenerateDomainUris : ISourceGenerator
    {
        private AppConfig? AppConfig = null;

        private string? SolutionDirectory;

        public void Execute(GeneratorExecutionContext context)
        {
            //Execute();
        }
        public void Execute(bool createExtraDockerFiles, bool createEnvFiles, bool createAppEnvironmentsFiles,bool setupLaunchSettings, bool addToHostsFile, bool createCertificates)
        {
            if (AppConfig == null)
            {
                //return;
                throw new NullReferenceException("AppConfig can not be null");

            }
            var publicClientProjectPath = Path.Combine(SolutionDirectory, AppConfig?.ProjectPaths?.PublicClientProjectPath ?? "");
            var publicServerProjectPath = Path.Combine(SolutionDirectory, AppConfig?.ProjectPaths?.PublicServerProjectPath ?? "");
            var adminClientProjectPath = Path.Combine(SolutionDirectory, AppConfig?.ProjectPaths?.AdminClientProjectPath ?? "");
            var adminServerProjectPath = Path.Combine(SolutionDirectory, AppConfig?.ProjectPaths?.AdminServerProjectPath ?? "");

            if (createExtraDockerFiles)
                CreateExtraDockerFiles();

            if(createEnvFiles)
                CreateEnvFiles();

            if(createAppEnvironmentsFiles)
                CreateAppEnvironmentsFiles(publicClientProjectPath, publicServerProjectPath, adminClientProjectPath, adminServerProjectPath);

            if(setupLaunchSettings)
                SetupLaunchsettingsFiles(publicClientProjectPath, publicServerProjectPath, adminClientProjectPath, adminServerProjectPath);

            if(addToHostsFile)
                AddToHostsFile();

            if(createCertificates)
                CreateCertificates();

        }

        private void CreateCertificates()
        {
            Console.WriteLine("Create certificates");
            var certsDirectory = Path.Combine(SolutionDirectory, "GitIgnored/Certs");
            var createCertificateScript = Path.GetFullPath(Path.Combine(SolutionDirectory, @"Scripts\CreateCertificate.ps1"));


            var domainsForCertificate = GetStarDomainsForCertifcates();
            var passwordGenerator = new PasswordGenerator();
            var password = passwordGenerator.Next();
            string strCmdText = @$"{createCertificateScript} -domains {domainsForCertificate} -password '{password}' -certspath {certsDirectory}";

            

            using (var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe",
                    Arguments = strCmdText,
                }
            })
            {

                p.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                p.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();
            }

        }

        private string GetStarDomainsForCertifcates()
        {

            var uniqueStartDomains = new HashSet<string>();

            foreach (var env in AppConfig.Environments)
            {

                var domains = new List<string> { env.ClientCertDomain, env.AdminCertDomain };
                var domainsToWrite = new List<string>();

                foreach (var d in domains)
                {
                    var trimmedDomain = d.Replace("https://", "").Replace("http://", "").Replace("www.", "");

                    uniqueStartDomains.Add(trimmedDomain);
                }
            }

            var uniqueDomains = string.Join(",", uniqueStartDomains);

            return uniqueDomains;
        }
        private void AddToHostsFile()
        {
            LogHeader("Add domains to hostsfile");

            var filePath = @"C:\Windows\System32\drivers\etc\hosts";

            var fileContent = File.ReadAllText(filePath);

            var ipNumber = "127.0.0.1 ";
            foreach (var env in AppConfig.Environments)
            {

                var domains = new List<string> { env.AdminApiUrl, env.AdminUrl, env.ClientApiUrl, env.ClientUrl };
                var trimemdDomains = new List<string>();

                foreach (var d in domains)
                {
                    var trimmedDomain = d.Replace("https://", "").Replace("http://", "");

                    if (trimmedDomain.StartsWith("www."))
                    {
                        var domainWithWwwRemoved = d.Replace("www.", "");
                        trimemdDomains.Add(domainWithWwwRemoved);

                    }

                    trimemdDomains.Add(d);
                }

                var domainsToWrite = new List<string>();

                foreach (var trimmedDomain in trimemdDomains)
                {
                    try
                    {
                        var foundMatch = Regex.IsMatch(fileContent, @"(?<![\w.])" + trimmedDomain.Replace(".", @"\.") + @"(?![\w.])", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                        if (!foundMatch)
                        {
                            domainsToWrite.Add(trimmedDomain);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        throw ex;
                    }
                }

                if (!domainsToWrite.Any()) continue;

                var appendDomains = ipNumber + string.Join(" ", domainsToWrite);
                fileContent = fileContent + Environment.NewLine + appendDomains;
                try
                {
                    File.WriteAllText(filePath, fileContent);
                    LogWithTime($"Added {appendDomains} to hostsfile: {filePath}");
                }
                catch (Exception ex)
                {
                    LogWithTime($"Error: Can't add '{appendDomains}' to hosts file{Environment.NewLine}{ex}");
                }
            }



        }

        private void CreatePublicExtraDockerfile(string environment, string publicDomain)
        {
            var dockerTemplatePath = Path.Combine(SolutionDirectory, "nginx", "Dockerfile.template");

            var content = File.ReadAllText(dockerTemplatePath);

            var dockerfilePath = Path.Combine(SolutionDirectory, "nginx", $"Dockerfile.{environment}");

            var newContent = content.Replace("{YOUR_PUBLIC_DOMAIN}", publicDomain);

            File.WriteAllText(dockerfilePath, newContent);

            LogWithTime($"Created public dockerfile: {dockerfilePath}");
        }

        private void CreateAppEnvironmentsFiles(string publicClientProjectPath, string publicServerProjectPath, string adminClientProjectPath, string adminServerProjectPath)
        {
            LogHeader("Create app environment files");
            //GenerateSectionInAppSettings(Path.Combine(publicClientProjectPath, "wwwroot"), nameof(EnvironmentUrls.ClientApiUrl), nameof(EnvironmentUrls.ClientAuth0));

            GenerateAppEnvironments("Public.Client.Domain",Path.Combine(publicClientProjectPath, "AppEnvironments"), nameof(EnvironmentItem.ClientApiUrl), nameof(EnvironmentItem.ClientAuth_Authority), nameof(EnvironmentItem.ClientAuth_ClientId));


            //GenerateSectionInAppSettings(publicServerProjectPath, nameof(EnvironmentItem.ClientUrl), nameof(EnvironmentItem.ClientAuth_Authority), nameof(EnvironmentItem.ClientAuth_ClientId));

            //GenerateSectionInAppSettings(Path.Combine(adminClientProjectPath, "wwwroot"), nameof(EnvironmentUrls.AdminApiUrl), nameof(EnvironmentUrls.AdminAuth0));

            GenerateAppEnvironments("Public.Admin.Domain",Path.Combine(adminClientProjectPath, "AppEnvironments"), nameof(EnvironmentItem.AdminApiUrl), nameof(EnvironmentItem.AdminAuth_Authority), nameof(EnvironmentItem.AdminAuth_ClientId));

            //GenerateSectionInAppSettings(adminServerProjectPath, nameof(EnvironmentItem.AdminUrl), nameof(EnvironmentItem.AdminAuth_Authority), nameof(EnvironmentItem.AdminAuth_ClientId));

        }

        private void SetupLaunchsettingsFiles(string publicClientProjectPath, string publicServerProjectPath, string adminClientProjectPath, string adminServerProjectPath)
        {

            LogHeader("Setup launchsettingsfiles");
            var l = AppConfig?.LocalEnvironment;

            ReplaceAndCreateLaunchsettingsFromTemplate(publicClientProjectPath, 
                @"Properties\template.launchSettings.json",
                @"Properties\launchSettings.json",
                l);

            ReplaceAndCreateLaunchsettingsFromTemplate(publicServerProjectPath, @"Properties\template.launchSettings.json", @"Properties\launchSettings.json", l);

            ReplaceAndCreateLaunchsettingsFromTemplate(adminClientProjectPath, @"Properties\template.launchSettings.json", @"Properties\launchSettings.json", l);

            ReplaceAndCreateLaunchsettingsFromTemplate(adminServerProjectPath, @"Properties\template.launchSettings.json", @"Properties\launchSettings.json", l);




        }
        private void GenerateAppEnvironments(string namespaceName,string projectPath, params string[] envUrls)
        {
            var envUrlsList = envUrls.ToList();

            if (AppConfig?.Environments == null) return;
            var environments = AppConfig.Environments.ToList();

            environments.Add(AppConfig.LocalEnvironment);


            foreach (var env in environments)
            {

                var appsettingsName = $"AppEnvironment{env.Name}.txt";

                var appsettingsPath = Path.Combine(projectPath, appsettingsName);

                // First clone
                var envUrlsClone = (EnvironmentItem)env.Clone();
                var propertiesListToAdd = new List<string>();
                PropertyInfo[] properties = envUrlsClone.GetType().GetProperties();

                foreach (var propertyInfo in properties)
                {
                    var value = string.Empty;
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        if (envUrlsList.Contains(propertyInfo.Name))
                        {
                            value = propertyInfo.GetValue(envUrlsClone).ToString();
                        }

                        propertiesListToAdd.Add($"        public static string {propertyInfo.Name} = \"{value}\";");

                    }
                }

                string? generatedPropertiesContent = string.Join(Environment.NewLine, propertiesListToAdd);


                var content = $$"""
namespace {{namespaceName}}
{
    public static class AppEnvironment
    {        
{{generatedPropertiesContent}}
    }
}
"""; ;

                var dir = Path.GetDirectoryName(appsettingsPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(appsettingsPath, content);
                LogWithTime($"Generated the appsettingsfile: {appsettingsPath}");

            }
        }
        private void GenerateSectionInAppSettings(string projectPath, params string[] envUrls)
        {
            var envUrlsList = envUrls.ToList();

            if (AppConfig?.Environments == null) return;
            var environments = AppConfig.Environments.ToList();
            environments.Add(AppConfig.LocalEnvironment);

            foreach (var env in environments)
            {

                var appsettingsName = $"appsettings{(string.IsNullOrEmpty(env.Name) ? "" : ".")}{env.Name}.json";

                var appsettingsPath = Path.Combine(projectPath, appsettingsName);
                if (!File.Exists(appsettingsPath))
                {
                    var template = @"{                        
                        //<GENERATED_START>
                        //<GENERATED_END>
                    }";
                    File.WriteAllText(appsettingsPath, template);
                }
                var content = File.ReadAllText(appsettingsPath);

                var generatedList = new List<string>();



                // First clone
                var envUrlsClone = (EnvironmentItem)env.Clone();

                PropertyInfo[] properties = envUrlsClone.GetType().GetProperties();

                foreach (var propertyInfo in properties)
                {
                    if (envUrlsList.Contains(propertyInfo.Name)) continue;
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(envUrlsClone, null, null);
                    }
                    else if (propertyInfo.PropertyType == typeof(EnvironmentAuth0))
                    {
                        propertyInfo.SetValue(envUrlsClone, null, null);
                    }
                }

                var jsonContent = JsonSerializer.Serialize<EnvironmentItem>(envUrlsClone, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }).Trim('{', '}');
                generatedList.Add(jsonContent);


                var generatedContent = string.Join(",", generatedList);
                try
                {
                    content = Regex.Replace(content, "(//<GENERATED_START>)(.*?)(//<GENERATED_END>)", $"$1{Environment.NewLine}{generatedContent}{Environment.NewLine}$3", RegexOptions.Singleline);
                }
                catch (ArgumentException ex)
                {
                    throw ex;

                }

                File.WriteAllText(appsettingsPath, content);

            }
        }

        private void ReplaceAndCreateLaunchsettingsFromTemplate(string projectPathDir, string templateFilename, string launchSettingsFilename,EnvironmentItem env)
        {
            // First clone
            var envUrlsClone = (EnvironmentItem)env.Clone();
            var replacements = new Dictionary<string,string>();
            PropertyInfo[] properties = envUrlsClone.GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                var value = string.Empty;
                if (propertyInfo.PropertyType == typeof(string))
                {
                    
                    value = propertyInfo.GetValue(envUrlsClone)?.ToString()??string.Empty;
                    

                    replacements.Add($"Local.{propertyInfo.Name}",value);

                }
            }

            var templateFilepath = Path.Combine(projectPathDir, templateFilename);
            var launchSettingFilepath = Path.Combine(projectPathDir, launchSettingsFilename);

            var content = File.ReadAllText(templateFilepath);

            var newContent = content;
            var replacedPropertyNames = new Dictionary<string,string>();
            foreach (var replace in replacements)
            {
                try
                {
                    var searchKey = replace.Key.Replace(".", "\\.");
                    //var regex = @"^(?<First>.*:.*?"")(?<ReplaceValue>[^""]*?)(?<Last>"",//[{]{2}" + searchKey + @"[}]{2})$";
                    var regex = @"(?<First>[^^+]?:.*?"")(?<ReplaceValue>[^""]*?)(?<Last>""\s*?,?\s*?//[{]{2}" + searchKey + @"[}]{2}[^$+])";
                    var replaceValue = $"$1{replace.Value}$3";
                    var match = Regex.Match(newContent, regex);
                    if (match.Success)
                    {
                        //LogWithTime($"Found match {regex} in {filePath}");
                        replacedPropertyNames.Add(replace.Key,replace.Value);
                    }
                    newContent = Regex.Replace(newContent, regex, replaceValue, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                //newContent = newContent.Replace($"{{{replace.Key}}}", replace.Value);
            }
            File.WriteAllText(launchSettingFilepath, newContent);

            var replacementText = string.Join(Environment.NewLine, replacedPropertyNames);
            LogWithTime($"Replaced:{Environment.NewLine}{replacementText}{Environment.NewLine}in {launchSettingFilepath}");
        }

        private void ReplaceInFileOld(string projectPathDir, string fileName, Dictionary<string, string> replacements)
        {
            var filePath = Path.Combine(projectPathDir, fileName);

            var content = File.ReadAllText(filePath);
            var newContent = content;
            foreach (var replace in replacements)
            {
                try
                {
                    var searchKey = replace.Key.Replace(".", "\\.");
                    //var regex = @"^(?<First>.*:.*?"")(?<ReplaceValue>[^""]*?)(?<Last>"",//[{]{2}" + searchKey + @"[}]{2})$";
                    var regex = @"(?<First>[^^+]?:.*?"")(?<ReplaceValue>[^""]*?)(?<Last>""\s*?,?\s*?//[{]{2}" + searchKey + @"[}]{2}[^$+])";
                    var replaceValue = $"$1{replace.Value}$3";
                    var match = Regex.Match(newContent, regex);
                    newContent = Regex.Replace(newContent, regex, replaceValue, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                //newContent = newContent.Replace($"{{{replace.Key}}}", replace.Value);
            }
            File.WriteAllText(filePath, newContent);

            var replacementText = string.Join(",", replacements);
            LogWithTime($"Replaced {replacementText}: in {filePath}");
        }

        private void CreateExtraDockerFiles()
        {
            LogHeader("Create extra docker files");

            var dockerEnvironmentsPath = Path.Combine(SolutionDirectory, AppConfig?.ProjectPaths?.DockerEnvironmentsPath ?? "");

            // Create .env files
            if (AppConfig?.Environments == null || AppConfig?.ProjectPaths == null) return;
            foreach (var env in AppConfig.Environments)
            {
                var fileName = $".env.{env.Name}";
                var filePath = Path.Combine(dockerEnvironmentsPath, fileName);

                var clientUrlTrimmed = env.ClientUrl?.Replace("https://", "").Replace("http://", "").Replace("www.", "");

                // Create docker file
                CreatePublicExtraDockerfile(env.Name, clientUrlTrimmed);

                
            }
        }
        private void CreateEnvFiles()
        {
            LogHeader("Create environment files");
            var dockerEnvironmentsPath = Path.Combine(SolutionDirectory, AppConfig?.ProjectPaths?.DockerEnvironmentsPath ?? "");

            // Create .env files
            if (AppConfig?.Environments == null || AppConfig?.ProjectPaths == null) return;
            foreach (var env in AppConfig.Environments)
            {
                var fileName = $".env.{env.Name}";
                var filePath = Path.Combine(dockerEnvironmentsPath, fileName);

                var clientUrlTrimmed = env.ClientUrl?.Replace("https://", "").Replace("http://", "").Replace("www.", "");

                var clientCertName = env.ClientCertDomain.Replace("*", "star");

                var adminCertName = env.AdminCertDomain.Replace("*", "star");

                var content =
@$"DOCKER_REGISTRY=""""
{nameof(EnvironmentItem.ClientApiUrl)} = ""{env.ClientApiUrl?.Replace("https://", "").Replace("http://", "")}""
{nameof(EnvironmentItem.ClientUrl)} = ""{clientUrlTrimmed}""
{nameof(EnvironmentItem.ClientCertDomain)} = ""{clientCertName}""
{nameof(EnvironmentItem.AdminApiUrl)} = ""{env.AdminApiUrl?.Replace("https://", "").Replace("http://", "")}""
{nameof(EnvironmentItem.AdminUrl)} = ""{env.AdminUrl?.Replace("https://", "").Replace("http://", "")}""
{nameof(EnvironmentItem.AdminCertDomain)} = ""{adminCertName}""
{nameof(EnvironmentItem.ClientAuth_Authority)} = ""{env.ClientAuth_Authority}""
{nameof(EnvironmentItem.ClientAuth_ClientId)} = ""{env.ClientAuth_ClientId}""
{nameof(EnvironmentItem.AdminAuth_Authority)} = ""{env.AdminAuth_Authority}""
{nameof(EnvironmentItem.AdminAuth_ClientId)} = ""{env.AdminAuth_ClientId}""
ASPNETCORE_ENVIRONMENT=""{env.Name}""
";

                File.WriteAllText(filePath, content);

                LogWithTime($"Created environmentfile: {filePath}");
            }
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            //Initialize(string path);
        }
        public void Initialize(string configpath)
        {
            LogHeader("Initialize generator");

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            SolutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..\\..\\..\\..\\..\\"));

            var json = File.ReadAllText(configpath) ?? String.Empty;


            AppConfig = JsonSerializer.Deserialize<AppConfig>(json, options: new JsonSerializerOptions()
            {

            });
            LogWithTime($"Deserialized AppConfig from {configpath}");

            
        }

        private void LogHeader(string header)
        {
            Console.WriteLine(string.Empty);
            var message = $"{DateTime.Now}: {header}";
            Console.WriteLine(message);
            var length = message.Length;
            Console.WriteLine(new string('-', length));
        }

        private void LogWithTime(string message,bool separateRows = false)
        {            
            if(separateRows)
                Console.WriteLine($"{DateTime.Now}:{Environment.NewLine} {message}");
            else
                Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
