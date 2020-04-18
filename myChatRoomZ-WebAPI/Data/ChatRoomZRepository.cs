using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using myChatRoomZ_WebAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Data
{

        public class ChatRoomZRepository : IChatRoomZRepository
        {
            private readonly ChatRoomZContext _context;
            private readonly ILogger<ChatRoomZRepository> _logger;
            public ChatRoomZRepository(ChatRoomZContext context, ILogger<ChatRoomZRepository> logger)
            {
                _context = context;
                _logger = logger;

            }

            public IEnumerable<Channel> GetAllChannels()
            {
                try
                {
                    _logger.LogInformation("GetAllChannels");
                    //Eager Loading ,including "ChatMessage" Entities
                    return _context.Channels.OrderBy(prop => prop.Title).Include("MessageHistory").ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to get all channels: {ex}");
                    return null;
                }

            }

            public bool SaveAll()
            {
                return _context.SaveChanges() > 0;
            }

            public void AddEntity(object model)
            {
                _context.Add(model);
            }

            public void AddMessage(ChatMessage newMsg)
            {
                AddEntity(newMsg);
            }

            private void RemoveEntity(object model)
            {
                _context.Remove(model);
            }
            public void DeleteMessage(ChatMessage model)
            {
                //Channel ch = _context.Channels.Include("MessageHistory").Single(a => a.Id == model.ChannelId);
                //ChatMessage msg = ch.MessageHistory.Single(a => a.Id == model.Id);
                RemoveEntity(model);
            }


        }
    }

