/*
 *
 * 
 * 
 *  https://www.ashleyhollis.com/how-to-configure-azure-active-directory-with-identityserver4
 *
 *
 */

using IdentityServer4;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddTestUsers(TestUsers.Users);

            // in-memory, code config
            builder.AddInMemoryIdentityResources(Config.Ids);
            builder.AddInMemoryApiResources(Config.Apis);
            builder.AddInMemoryClients(Config.Clients);

            // or in-memory, json config
            //builder.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"));
            //builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            //builder.AddInMemoryClients(Configuration.GetSection("clients"));

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services
                .AddAuthentication()
                .AddOpenIdConnect("aad", "Azure AD", options =>
                 {
                     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                     options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                     // Directory (tenant) ID - displayed on Azure Application registration appended at the end of the link
                     options.Authority = "https://login.windows.net/29a052c3-b98f-4647-af4d-1532b4d335aa";
                     // Application ID  - the identifier in AAD of the application
                     options.ClientId = "8acd7c89-e695-4352-98d3-9aa02874de19";
                     options.ResponseType = OpenIdConnectResponseType.IdToken;
                     options.CallbackPath = "/signin-aad";
                     options.SignedOutCallbackPath = "/signout-callback-aad";
                     options.RemoteSignOutPath = "/signout-aad";
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         NameClaimType = "name",
                         RoleClaimType = "role"
                     };
                 });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}