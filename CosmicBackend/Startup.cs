using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CosmicBackend
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CosmicBackend.Core;
    using CosmicBackend.Models;

    using Microsoft.AspNetCore.Http;

    public class Startup
    {
        

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddJsonOptions(option => option.JsonSerializerOptions.WriteIndented = true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /*app.Use(
                async (context, next) =>
                {
                    string path = context.Request.Path;
                    if (Program.NoAuthHeaderEndpoints.Contains(path))
                    {
                        await next.Invoke().ConfigureAwait(false);
                        return;
                    }

                    string auth = context.Request.Headers["authorization"];
                    JWT token = JWT.FromJWTToken(auth);
                    if (!token.IsValid)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(
                            Utilities.SetError("Supplied token data is invalid", "cosmic.jwt.invalid_token_data", 400)
                                     .Value).ConfigureAwait(false);
                        return;
                    }

                    if (token.TokenType == TokenType.client_credentials)
                    {

                    }

                    ulong seed = token.Seed;
                    string userId = token.Id;
                    ConcurrentDictionary<string, Session> sessions = Singleton.Instance.RegisteredSessions;
                    if (!sessions.TryGetValue(userId, out Session session))
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsJsonAsync(
                            Utilities.SetError(
                                "Supplied token is not registered",
                                "cosmic.jwt.unregistered_token",
                                404)).ConfigureAwait(false);
                        return;
                    }

                    if (session.Id != seed)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsJsonAsync(
                            Utilities.SetError(
                                "The token doesn't belong to this account",
                                "cosmic.jwt.unauthorized_operation",
                                403).Value).ConfigureAwait(false);
                        return;
                    }

                    await next.Invoke().ConfigureAwait(false);
                });*/


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}