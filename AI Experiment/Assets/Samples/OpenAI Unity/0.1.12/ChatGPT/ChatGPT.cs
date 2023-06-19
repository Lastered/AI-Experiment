using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {

        public string promptPrefix = "Create a short The Simpsons episode script between Homer, and Marge Simpson. Format the script so that when a character speacks, it starts with their name, followed by a colon, and ended in a %. Base the scene off of ";
        public string promptWord = "there is no more beer in the fridge.";

        private TextFileHandler textFileHandler;

        //event when generation is complete
        [SerializeField] private UnityEvent onGenerationComplete; 

        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        [SerializeField] private VoiceGeneration voiceGeneration;


        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();

        [SerializeField] string globalPrefix = "Respond to the previous prompt between 1 and 10 words:\n";

        private string lastResponse = "";

        private void Start()
        {
            button.onClick.AddListener(SendReply);
            SendReply();

            textFileHandler = GetComponent<TextFileHandler>();

            // if there is no prompt word, generate one
            if (promptWord == "")
            {
                promptWord = textFileHandler.GetRandomLine();
            }
        }

        private void AppendMessage(ChatMessage message)
        {
            
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        public void GenerateNewScript()
        {
            promptWord = textFileHandler.GetRandomLine();
            SendReply();
        }

        public async void SendReply()
        {

            Debug.Log("Sending reply");

            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };
            
            // only append message if its not contentOverride
            AppendMessage(newMessage);
            if (messages.Count == 0 && inputField != null) newMessage.Content = promptPrefix + promptWord + "\n" + inputField.text; 
            

            messages.Add(newMessage);
            
            button.enabled = false;
            if (inputField != null)
            {
                inputField.text = "";
                inputField.enabled = false;
            }


            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0301",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);
                lastResponse = message.Content;
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }


            button.enabled = true;
            if (inputField != null) 
                inputField.enabled = true;



            if (voiceGeneration != null)
                voiceGeneration.GenerateMultipleVoices(lastResponse);

            //debug the charactrer count
            Debug.Log("Character count: " + lastResponse.Length);

            onGenerationComplete.Invoke();

            promptWord = textFileHandler.GetRandomLine();

        }
    }
}
