using JhaChallengeTweetStreamer.Core;
using JhaChallengeTweetStreamer.DAL;
using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using JhaChallengeTweetStreamer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace JhaChallengeTweetStreamer
{

    internal class Program
    {
        public static AppSettings AppSettings = new AppSettings();

        /// <summary>
        ///   Application Entry Point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                var configuration = builder.Build();
                ConfigurationBinder.Bind(configuration.GetSection("AppSettings"), AppSettings);

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error -- Unable to start:  {ex}");
            }            
        }

        /// <summary>
        /// Dependency Injection of Concrete Types for Interfaces
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MessageStreamRetrieverService>();
                    services.AddHostedService<MessageCacheProcessorService>();
                    services.AddHostedService<MessagePersistorService>();
                    services.AddHostedService<MessageStreamAnalyzerService>();
                    services.AddSingleton<IApiMessageStreamRetriever<ReceivedMessage>, TwitterV2ApiSampleStreamRetriever>();
                    services.AddSingleton<IQueueProvider<ReceivedMessage>,ReceivedMessageInMemoryQueueProvider>();
                    services.AddSingleton<IQueueProvider<BusinessMessage>, BusinessMessageInMemoryQueueProvider>();
                    services.AddSingleton<IQueueProvider<BusinessMessageMetadata>, BusinessMessageMetadataInMemoryQueueProvider>();
                    services.AddSingleton<IMessageValidator, TwitterV2ApiSampleStreamMessageValidator>();
                    services.AddSingleton<IBusinessMessageGenerator, TwitterV2ApiBusinessMessageGenerator>();
                });

    }
}
