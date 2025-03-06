using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DICOMAnonymizer.Application;
using DICOMAnonymizer.Infrastructure;
using Microsoft.Extensions.Hosting;
using DICOMAnonymizer.Core;

namespace DICOMAnonymizer.CommandLine;

class Program
{
    static void Main(string[] _)
    {
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        string? inputFolder = null;
        string? outputFolder = null;

        // Skip the first element (executable name)
        for (int i = 1; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--InputFolder")
            {
                if (i + 1 < commandLineArgs.Length)
                {
                    inputFolder = commandLineArgs[i + 1];
                    i++; // Skip the next argument (the value)
                }
                else
                {
                    Console.WriteLine("Error: --InputFolder requires a value.");
                    return;
                }
            }
            else if (commandLineArgs[i] == "--OutputFolder")
            {
                if (i + 1 < commandLineArgs.Length)
                {
                    outputFolder = commandLineArgs[i + 1];
                    i++; // Skip the next argument (the value)
                }
                else
                {
                    Console.WriteLine("Error: --OutputFolder requires a value.");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Error: Unknown argument: {commandLineArgs[i]}");
                return;
            }
        }

        if (string.IsNullOrEmpty(inputFolder) || string.IsNullOrEmpty(outputFolder))
        {
            Console.WriteLine("InputFolder and OutputFolder are required.");
            return;
        }

        if (!Directory.Exists(inputFolder))
        {
            Console.WriteLine($"Input folder does not exist: {inputFolder}");
            return;
        }

        try
        {
            using var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                    });

                    services.AddSingleton<IRandomNumberGenerator, DefaultRandomNumberGenerator>();

                    services.AddSingleton(provider =>
                    {
                        var randomNumberGenerator = provider.GetRequiredService<IRandomNumberGenerator>();
                        return new AnonymizationConfiguration(randomNumberGenerator);
                    });

                    services.AddSingleton<DICOMFileService>();

                    services.AddSingleton<IAnonymizationService, DICOMAnonymizationService>();
                })
                .Build();

            // Resolve dependencies
            var anonymizationService = host.Services.GetRequiredService<IAnonymizationService>();

            // Anonymize the folder
            anonymizationService.AnonymizeFolder(inputFolder, outputFolder);

            Console.WriteLine("Anonymization complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}