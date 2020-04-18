using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Data.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset SentAt { get; set; }

        public string Attachment { get; set; }

        public int ChannelId { get; set; }
    }
}
