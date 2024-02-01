using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDemo.Model
{
    public static class MessageStorage
    {
        public static ObservableCollection<Message> Messages { get; set; }
    }
    public class Message
    {
        public string MessageContent { get; set; }
        public string MessageSender { get; set; }
        public DateTime PublishDate { get; set; }
    }
}
