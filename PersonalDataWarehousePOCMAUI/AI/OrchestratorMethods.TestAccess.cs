using OpenAI.Chat;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace PersonalDataWarehouse.AI
{
    public partial class OrchestratorMethods
    {
        #region public async Task<bool> TestAccess(string AIType, string AIModel, string ApiKey, string Endpoint, string AIEmbeddingModel)
        public async Task<bool> TestAccess(string AIType, string AIModel, string ApiKey, string Endpoint, string AIEmbeddingModel)
        {
            var chatClient = CreateAIChatClient(AIType, AIModel, ApiKey, Endpoint, AIEmbeddingModel);
            string SystemMessage = "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";
            var response = await chatClient.CompleteAsync(SystemMessage);

            if (response.Choices.Count == 0)
            {
                return false;
            }

            // Pass though ExtractJsonFromResponse
            var json = ExtractJsonFromResponse(response.Choices.FirstOrDefault().Text);

            return true;
        }
        #endregion
    }
}
