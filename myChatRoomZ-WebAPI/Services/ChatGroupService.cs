using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Services
{
    public class ChatGroupService : IChatGroupService
    {
        private static readonly ConcurrentDictionary<string, string> Map = new ConcurrentDictionary<string, string>();
        public void AddConnectiontoGroup(string ConnectionID, string GroupID)
        {
            Map.TryAdd(ConnectionID, GroupID);
        }

        public string RemoveConnectionfromGroups(string ConnectionID)
        {
            string groupid;
            Map.TryRemove(ConnectionID, out groupid);

            return groupid;
        }
    }
}
