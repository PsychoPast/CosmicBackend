using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using CosmicBackend.Controllers;
using CosmicBackend.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;

namespace CosmicBackend
{
    public class Program
    {
        internal static ConcurrentList<string> NoAuthHeaderEndpoints { get; private set; }

        public static void Main(string[] args)
        {
            NoAuthHeaderEndpoints = new();
            IEnumerable<Type> classesE = from t in Assembly.GetExecutingAssembly().GetTypes()
                                         where t.IsClass && t.Namespace == typeof(OAuthController).Namespace
                                         select t;
            Type[] classes = classesE.ToArray();
            for (int i = 0; i < classes.Length; i++)
            {
                MethodInfo[] methods = classes[i].GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo methodInfo = methods[j];
                    IEnumerable<Attribute> attrs = methodInfo.GetCustomAttributes();
                    if (!attrs.Any(x => x.GetType() == typeof(NoAuthorizationRequiredAttribute)))
                    {
                        continue;
                    }

                    StringBuilder endpoint = new();
                    object[] classAttr = methodInfo.DeclaringType.GetCustomAttributes(typeof(RouteAttribute), false);
                    if (classAttr.Length > 0)
                    {
                        endpoint.Append(((RouteAttribute)classAttr[0]).Template);
                        endpoint.Append('/');
                    }

                    Attribute httpAttr = attrs.FirstOrDefault(x => x.GetType().BaseType == typeof(HttpMethodAttribute));
                    endpoint.Append(((HttpMethodAttribute)httpAttr).Template);
                    NoAuthHeaderEndpoints.Add(endpoint.ToString());
                }
            }
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                     {
                         webBuilder.UseStartup<Startup>();
                     });

    }
}