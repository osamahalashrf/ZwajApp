using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZwajApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ZwajApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // we user sqlLite for development and sql server for production
            services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // add cors to allow cross origin requests from our angular app to our api
            services.AddCors();

            // add the auth repository to the dependency injection container, we can use it in our controllers by injecting it in the constructor
            services.AddScoped<IAuthRepository, AuthRepository>();

            // add the JwtService to the dependency injection container, we can use it in our controllers by injecting it in the constructor
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // التحقق من صحة المفتاح السري المستخدم لتوقيع التوكن
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                    .GetBytes(Configuration.GetSection("AppSettings:Token").Value)), // المفتاح السري نفسه الذي استخدمناه في JwtService لإنشاء التوكن
                ValidateIssuer = false, // لا نستخدم الـ Issuer في هذا المشروع، لذا نضعه false، وهذا أصلا عمله هو التحقق من أن الجهة التي أصدرت التوكن هي الجهة الموثوقة، وفي حالتنا نحن نسمح لأي جهة تصدر التوكن، لذا نضعه false
                ValidateAudience = false // لا نستخدم الـ Audience في هذا المشروع، لذا نضعه false، وهذا أصلا عمله هو التحقق من أن المستهلك للتوكن هو الجهة المقصودة، وفي حالتنا نحن نسمح لأي جهة تستهلك التوكن، لذا نضعه false
            };
        });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseHsts();
            }

            // here we allow any origin to access our api, we can specify the allowed origins instead of allowing any origin
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // here we use authentication middleware to authenticate the user before accessing any endpoint, we will use the JwtService to create a token for the user and then we will use that token to authenticate the user
            app.UseAuthentication();
            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
