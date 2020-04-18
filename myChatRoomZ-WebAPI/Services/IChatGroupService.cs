using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Services
{
    public interface IChatGroupService
    {
        public void AddConnectiontoGroup(string ConnectionID, string GroupID);
        public string RemoveConnectionfromGroups(string ConnectionID);
    }
}
