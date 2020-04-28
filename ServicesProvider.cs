using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComputerVision_Tutorial
{
    public static class ServicesProvider
    {
        public static IConfigurationRoot GetServiceProvider(string path) => 
            new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile(path)
                .AddUserSecrets(typeof(Program).Assembly)
                .Build();
    }
}
