using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

namespace Temp
{
    internal class Program
    {
        private const int MaximumConcurrency = 4;
        private static int a;
        private const string OutputPath =
            @"C:\Users\Christopher\Desktop\Visual Studio\CosmicFN\CosmicBackend\CosmicBackend\Resources\SkinData\Characters\Parsed";

        internal unsafe static void Main()
        {
            string filesPath =
                @"C:\Users\Christopher\Desktop\Visual Studio\CosmicFN\CosmicBackend\CosmicBackend\Resources\SkinData\Characters";
            string[] files = Directory.GetFiles(filesPath);
            int currentIndex = 0;
            int interval = files.Length / MaximumConcurrency;
            int remainder = files.Length % MaximumConcurrency;
            List<Task> tasks = new(MaximumConcurrency);
            for (int i = 0; i < MaximumConcurrency - 1; i++)
            {
                int index = currentIndex;
                tasks.Add(
                    Task.Factory.StartNew(
                        () => Execute(files[index..(index + interval)]),
                        TaskCreationOptions.LongRunning));
                currentIndex += interval;
            }

            tasks.Add(
                Task.Factory.StartNew(
                    () => Execute(files[currentIndex..(currentIndex + interval + remainder)]),
                    TaskCreationOptions.LongRunning));
            Task.WhenAll(tasks).GetAwaiter().GetResult();
            Console.WriteLine("Done");
            Console.ReadLine();
    }


    private static void Execute(string[] files)
        {
            bool hasFinishedParsing;
            Parallel.For(
                0,
                files.Length,
                new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                i =>
                {
                    string file = files[i];
                    hasFinishedParsing = false;
                    byte[] data = File.ReadAllBytes(file);
                    Utf8JsonReader jsonReader = new(data, true, default);
                    Utf8JsonWriter jsonWriter = new(
                        File.OpenWrite(@$"{OutputPath}\{Path.GetFileName(file)}"),
                        new() { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                    jsonWriter.WriteStartObject();
                    while (!hasFinishedParsing)
                    {
                        jsonReader.Read();
                        if (jsonReader.TokenType != JsonTokenType.PropertyName)
                        {
                            continue;
                        }

                        switch (jsonReader.GetString())
                        {
                            case "HeroDefinition":
                                {
                                    for (int _ = 0; _ < 3; _++)
                                    {
                                        jsonReader.Read();
                                    }

                                    jsonWriter.WriteString("Codename", jsonReader.GetString().Substring(8));
                                }
                                break;

                            case "Rarity":
                                {
                                    jsonReader.Read();
                                    jsonWriter.WriteString(
                                        "Rarity",
                                        ParseRarity(jsonReader.GetString()).ToString());
                                }
                                break;

                            case "DisplayName":
                                {
                                    for (int _ = 0; _ < 7; _++)
                                    {
                                        jsonReader.Read();
                                    }

                                    jsonWriter.WriteString("DisplayName", jsonReader.GetString());
                                    jsonWriter.WriteEndObject();
                                    jsonWriter.Flush();
                                    jsonWriter.Dispose();
                                    hasFinishedParsing = true;
                                }
                                break;
                        }
                    }
                });
        }

        private static SkinRarity ParseRarity(string rarity)
        {
            return rarity switch
            {
                "EFortRarity::Common" => SkinRarity.Common,
                "EFortRarity::Uncommon" => SkinRarity.Uncommon,
                "EFortRarity::Rare" => SkinRarity.Rare,
                "EFortRarity::Epic" => SkinRarity.Epic,
                "EFortRarity::Legendary" => SkinRarity.Legendary,
                _ => throw new ArgumentException(nameof(rarity))
            };
        }

        private enum SkinRarity
        {
            Common,

            Uncommon,

            Rare,

            Epic,

            Legendary
        }
    }
}