using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace MyDemo.Model
{
    public class Conversation
    {
        public string Member1 { get; set; }
        public string Member2 { get; set; }
        public ObservableCollection<Message> Messages { get; set; }
        public DateTime Time { get; set; }

        public string DynamicPreviewInfo { get; set; }

        public string GetPreviewInfo(string currentUserName)
        {
            string otherMember = (Member1 == currentUserName) ? Member2 : Member1;
            return $"{otherMember} - {Time}";
        }
    }

    public class ConversationManager
    {
        private readonly string filePath;

        public ConversationManager(string filePath)
        {
            this.filePath = filePath;
        }

        public ObservableCollection<Conversation> LoadConversations(string name, string search)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var allConversations = JsonConvert.DeserializeObject<ObservableCollection<Conversation>>(json) ?? new ObservableCollection<Conversation>();

                var filteredConversations = allConversations
                    .Where(conversation => string.IsNullOrEmpty(name) || conversation.Member1 == name || conversation.Member2 == name)
                    .Select(conversation => new
                    {
                        Conversation = conversation,
                        PreviewName = (conversation.Member1 == name) ? conversation.Member2 : conversation.Member1
                    })
                    .Where(item => string.IsNullOrEmpty(search) || item.PreviewName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .Select(item => item.Conversation)
                    .ToList();

                return new ObservableCollection<Conversation>(filteredConversations);

            }
            return new ObservableCollection<Conversation>();
        }

        public void SaveConversation(Conversation newConversation)
        {
            if (newConversation.Messages == null || newConversation.Messages.Count == 0)
            {
                // Don't save the conversation if Messages is empty
                return;
            }

            ObservableCollection<Conversation> existingConversations = LoadConversations("", "");

            existingConversations.Add(newConversation);

            string updatedJson = JsonConvert.SerializeObject(existingConversations);

            File.WriteAllText(filePath, updatedJson);
        }
    }
}