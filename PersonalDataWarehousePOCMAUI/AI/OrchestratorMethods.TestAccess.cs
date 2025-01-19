﻿using OpenAI.Chat;
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
        #region public async Task<bool> TestAccess(string AIModel)
        public async Task<bool> TestAccess(string AIModel)
        {
            var chatClient = await CreateAIChatClientAsync(AIModel);
            string SystemMessage = "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";
            var response = await chatClient.CompleteAsync(SystemMessage);

            if (response.Choices.Count == 0)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
