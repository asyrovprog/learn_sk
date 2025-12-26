# Quiz Answers: Multi-Modal AI Applications with Semantic Kernel

**Date:** December 25, 2025  
**Topic:** Multi-Modal AI Applications  
**User Score:** 90% (9/10 correct)

---

## Answer Key

**Question 1:** What is the primary model used in Semantic Kernel for image understanding?  
**Correct Answer:** **B** - GPT-4 Vision (gpt-4o)  
**User Answer:** B ✅

**Explanation:** GPT-4 Vision (gpt-4o) is OpenAI's multimodal model that can understand and analyze images. It's integrated into Semantic Kernel via the ChatCompletion service with ImageContent support, enabling tasks like image description, OCR, object detection, and visual question answering.

---

**Question 2:** Which OpenAI API is used for converting speech to text?  
**Correct Answer:** **C** - Whisper API  
**User Answer:** D ❌

**Explanation:** The **Whisper API** is OpenAI's dedicated automatic speech recognition (ASR) system for speech-to-text transcription. It supports 99+ languages and provides features like timestamps, word-level transcription, and language detection. While GPT-4 Audio exists in some contexts (like the Realtime API), the standard STT service is Whisper.

---

**Question 3:** What audio format does the OpenAI Realtime API primarily use?  
**Correct Answer:** **B** - PCM16 at 24kHz  
**User Answer:** B ✅

**Explanation:** The Realtime API uses PCM16 (16-bit Linear PCM) at 24,000 Hz sample rate, mono channel, encoded in Base64 for WebSocket transmission. This format provides good quality with efficient streaming performance.

---

**Question 4:** (Y/N) The Realtime API supports bidirectional streaming, allowing natural interruptions during conversations.  
**Correct Answer:** **Y** - Yes  
**User Answer:** Y ✅

**Explanation:** The Realtime API's WebSocket-based architecture enables true bidirectional streaming where both user and AI can speak simultaneously, and users can naturally interrupt the AI mid-response, just like in human conversations.

---

**Question 5:** Which VAD (Voice Activity Detection) method provides the highest accuracy?  
**Correct Answer:** **D** - Server VAD (OpenAI) / **C** - Model-based VAD (Silero)  
**User Answer:** D ✅

**Explanation:** Both Server VAD (OpenAI Realtime API) and Model-based VAD (Silero) achieve ~95%+ accuracy. Server VAD benefits from cloud processing power, while Silero VAD is a state-of-the-art ML model using ONNX Runtime. Both outperform WebRTC VAD (85-90%) and Energy-based VAD (70-80%).

---

**Question 6:** (Multi-select) Which are valid OpenAI TTS voices?  
**Correct Answer:** **A, C, E, F** - alloy, shimmer, nova, echo  
**User Answer:** A, C, E, F ✅

**Explanation:** OpenAI TTS API provides 6 voices: **alloy** (neutral), **echo** (male, clear), **fable** (expressive), **onyx** (deep), **nova** (female, energetic), and **shimmer** (soft). Options B (harmony) and D (whisper) are not valid voice names.

---

**Question 7:** What is the typical latency of the OpenAI Realtime API?  
**Correct Answer:** **B** - ~250-500ms  
**User Answer:** B ✅

**Explanation:** The Realtime API achieves sub-500ms latency (typically 250-500ms), which is significantly lower than traditional REST-based approaches (500ms-2s). This low latency enables natural, real-time conversations.

---

**Question 8:** Which WebRTC VAD operating mode is best for noisy environments?  
**Correct Answer:** **D** - VeryAggressive  
**User Answer:** D ✅

**Explanation:** VeryAggressive mode has the highest aggressiveness level, filtering out most non-speech sounds and minimizing false positives. This makes it ideal for very noisy environments where you want to be certain that detected segments are actual speech, though it may miss some quiet speech.

---

**Question 9:** What does RAG (Retrieval Augmented Generation) enable in multimodal applications?  
**Correct Answer:** **B** - AI responses based on retrieved contextual information from a knowledge base  
**User Answer:** B ✅

**Explanation:** RAG (Retrieval Augmented Generation) combines information retrieval with AI generation. It retrieves relevant context from a knowledge base (using semantic search/vector databases) and uses that context to generate more accurate, informed responses. This pattern is crucial for grounding AI responses in specific knowledge.

---

**Question 10:** (Multi-select) Which are production best practices for real-time audio streaming?  
**Correct Answer:** **A, B, D, E** - PCM16 24kHz, buffer 100-200ms, Server VAD, connection pooling  
**User Answer:** A, B, D, E ✅

**Explanation:** Best practices include: using PCM16 24kHz directly (no conversion overhead), setting appropriate buffer sizes (100-200ms), enabling Server VAD for automatic turn detection, and implementing connection pooling/reconnection logic. Option C is incorrect - you should use LOW temperature (0.6-0.7) for faster, more predictable responses, not maximum temperature.

---

## Summary

**Score:** 9/10 (90%)  
**Status:** PASSED ✅

**Strengths:**
- Strong understanding of GPT-4 Vision and multimodal capabilities
- Excellent knowledge of real-time audio streaming architecture
- Good grasp of VAD technologies and their trade-offs
- Solid understanding of production best practices

**Area for Improvement:**
- Speech-to-Text API nomenclature (Whisper vs GPT-4 Audio)

**Overall:** Excellent performance! You've demonstrated comprehensive understanding of multi-modal AI applications, including image processing, audio streaming, real-time communication patterns, and production considerations.
