using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using myChatRoomZ_WebAPI.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Data
{
    public class ChatRoomZSeeder
    {
        private readonly ChatRoomZContext _ctx;
        private readonly IWebHostEnvironment _hosting;
        private readonly UserManager<ChatUser> _userManager;

        public ChatRoomZSeeder(ChatRoomZContext ctx, IWebHostEnvironment hosting, UserManager<ChatUser> userManager)
        {
            _ctx = ctx;
            _hosting = hosting;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            _ctx.Database.EnsureCreated();
            ChatUser user = await _userManager.FindByEmailAsync("romanm@chatroomz.com");

            if (user == null)
            {
                user = new ChatUser()
                {
                    FirstName = "Roman",
                    LastName = "Mayerson",
                    Email = "romanm@chatroomz.com",
                    UserName = "admin"
                };

                var result = await _userManager.CreateAsync(user, "1q2w3e4R!");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create new user in seeder");
                }
            }


            if (!_ctx.Channels.Any())
            {
                //If there is no Data in "Channels" We will Need to create a Sample Data
                var filepath = Path.Combine(_hosting.ContentRootPath, "Data/channels.json");
                var json = File.ReadAllText(filepath);
                var channels = JsonConvert.DeserializeObject<IEnumerable<Channel>>(json);
                _ctx.Channels.AddRange(channels);

                _ctx.SaveChanges();


            }

        }


    }
}
