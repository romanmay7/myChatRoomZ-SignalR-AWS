using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myChatRoomZ_WebAPI.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace myChatRoomZ_WebAPI.Data
{
    public class ChatRoomZContext : IdentityDbContext
    {
        public ChatRoomZContext(DbContextOptions<ChatRoomZContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<IdentityUserLogin<int>>(i =>
            {
                i.ToTable("UserLogins");
                i.HasKey(x => new { x.LoginProvider, x.ProviderKey });
            });
            builder.Entity<IdentityUserClaim<int>>(i =>
            {
                i.ToTable("UserClaims");
                i.HasKey(x => x.Id);
            });
            builder.Entity<IdentityUserToken<int>>(i =>
            {
                i.ToTable("UserTokens");
                i.HasKey(x => x.UserId);
            });
        }
    }
}


