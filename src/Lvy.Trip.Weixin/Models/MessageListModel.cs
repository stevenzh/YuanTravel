using System.Collections.Generic;
using System.Globalization;
using MvcChat.Util;
using TopWeb.DAL.Model;

namespace CCT.Travel.Model
{
    public class ChatMessageDto
    {
        public ChatMessageDto(ChatMessage msg)
        {
            UserName = msg.UserName;
            Id = msg.MessageId;
            Text = msg.Content;
            SendTime = msg.SendTime.ToUnixTime().ToString("F0", CultureInfo.InvariantCulture);
        }

        public string UserName { get; set; }
        public long Id { get; set; }
        public string Text { get; set; }
        public string SendTime { get; set; }
    }

    public class MessageListModel
    {
        public IEnumerable<ChatMessageDto> Messages { get; set; }
    }
}