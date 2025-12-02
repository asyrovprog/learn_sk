# Conversational Memory Manager Lab

## Overview
Build a chat system using Semantic Kernel that maintains conversation context across multiple turns. You'll implement conversation history management and demonstrate how the AI maintains context throughout a dialog.

## TODO 1 – Initialize Kernel with Chat Completion Service

**Objective:** Set up a Semantic Kernel instance configured with OpenAI chat completion.

**Instructions:**
1. Create a Kernel builder
2. Add the OpenAI chat completion service using the provided API key and model name
3. Build and return the configured kernel

**Requirements:**
- Use the AddOpenAIChatCompletion method
- Service ID should be "chat"
- Model should be "gpt-4o-mini"

---

## TODO 2 – Manage Chat History

**Objective:** Implement a function that adds user and assistant messages to chat history and maintains conversation context.

**Instructions:**
1. Add the user's message to the ChatHistory
2. Get a streaming response from the chat completion service
3. Collect the streamed response chunks into a complete message
4. Add the assistant's response to ChatHistory
5. Return the complete assistant response

**Requirements:**
- Use GetRequiredService<IChatCompletionService>() to get the chat service
- Use GetStreamingChatMessageContentsAsync() for streaming
- Properly iterate through all message chunks
- Add both user and assistant messages to history

---

## TODO 3 – Display Conversation History

**Objective:** Create a method to display the entire conversation history in a readable format.

**Instructions:**
1. Iterate through all messages in ChatHistory
2. For each message, print the role (User/Assistant) and the content
3. Add visual separators for readability

**Requirements:**
- Handle both User and Assistant roles
- Format output clearly with role labels
- Add separators between messages
