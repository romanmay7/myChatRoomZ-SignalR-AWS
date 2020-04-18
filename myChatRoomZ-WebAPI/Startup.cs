using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using myChatRoomZ_WebAPI.Data;
using myChatRoomZ_WebAPI.Data.Models;
using myChatRoomZ_WebAPI.Services;
using myChatRoomZ_WebAPI.SignalRHub;

namespace myChatRoomZ_WebAPI
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IConfiguration Configuration { get; }

        //***********************************************************************************************************************
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //------------------------------------------------------------------------------------------------------------------
            services.AddDbContext<ChatRoomZContext>(cfg =>
            {
                cfg.UseSqlServer(_config.GetConnectionString("ChatRoomZConnectionString"));
            });

            //-----------------------------------------------------------------------------------------------------------------
            services.AddTransient<ChatRoomZSeeder>();
            services.AddScoped<IChatRoomZRepository, ChatRoomZRepository>();
            //------------------------------------------------------------------------------------------------------------------
            services.AddIdentity<ChatUser, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<ChatRoomZContext>()
              .AddDefaultTokenProviders();
            //-----------------------------------------------------------------------------------------------------------------
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("http://my-chat-roomz-client.s3-website-eu-west-1.amazonaws.com","http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            //------------------------------------------------------------------------------------------------------------------
            services.AddSignalR();
            services.AddSingleton<IChatGroupService, ChatGroupService>();
            services.AddSingleton<IS3Service, S3Service>();
            services.AddAWSService<IAmazonS3>();
            //------------------------------------------------------------------------------------------------------------------
            services.AddAuthentication()
           .AddCookie()
           .AddJwtBearer(cfg =>
           {
             cfg.TokenValidationParameters = new TokenValidationParameters()
             {
               ValidIssuer = _config["Tokens:Issuer"],
               ValidAudience = _config["Tokens:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Tokens:Key"]))
             };
           });
            //--------------------------------------------------------------------------------------------------------
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            //--------------------------------------------------------------------------------------------------------

            services.AddControllers();
        }
        //***********************************************************************************************************************
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                RequestPath = new PathString("/Resources")
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                //Route Configuration for SignalR Hub
                endpoints.MapHub<ChatHub>("/chatHub");

            });
        }
    }
}
