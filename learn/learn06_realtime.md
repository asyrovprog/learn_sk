# Learning Materials: Real-Time Multimodal Streaming with OpenAI & Semantic Kernel

**Iteration**: 06.5  
**Topic**: Real-Time Audio/Video Streaming with OpenAI APIs  
**Estimated Time**: ~30 minutes  
**Date**: December 14, 2025

---

## 📚 Overview

**Real-time multimodal streaming** enables bidirectional, low-latency communication with AI models using audio and video. Unlike batch processing (send audio → wait → receive response), streaming allows:

- **Continuous audio conversations** with natural interruptions
- **WebSocket-based bidirectional streaming** for real-time interaction
- **Function calling during voice conversations**
- **Streaming video frame analysis** for live video processing
- **Sub-second latency** for natural conversational experiences

> **📖 Related Reading**: For batch audio/image processing, see [learn06.md](learn06.md). This guide focuses on **real-time streaming** patterns.

---

## 🎯 What You'll Learn

1. **OpenAI Realtime API** - WebSocket-based audio streaming
2. **Audio Streaming in C#** - Building voice assistants
3. **Streaming Video Processing** - Frame-by-frame analysis
4. **Semantic Kernel Integration** - Custom connectors and patterns
5. **Production Considerations** - Connection management, error handling, optimization

---

## 🎙️ OpenAI Realtime API (Beta)

### What Is the Realtime API?

OpenAI's Realtime API provides **persistent WebSocket connections** for low-latency, multi-turn audio conversations. Unlike REST-based APIs:

| Feature | REST API (Traditional) | Realtime API (WebSocket) |
|---------|----------------------|--------------------------|
| **Connection** | One request/response | Persistent bidirectional |
| **Latency** | ~500ms - 2s | ~250ms - 500ms |
| **Audio Flow** | Batch (send all → wait → receive all) | Streaming (continuous) |
| **Interruptions** | Not supported | Natural interruptions |
| **Function Calling** | After full response | During conversation |
| **Model** | `gpt-4o-audio-preview` | `gpt-4o-realtime-preview-2024-12-17` |

### Key Capabilities

1. **Native Speech-to-Speech** - No separate Whisper + TTS needed
2. **Natural Conversations** - Interrupt, pause, resume naturally
3. **Multiple Voices** - Choose from `alloy`, `echo`, `shimmer`, etc.
4. **Function Calling** - Execute functions during voice conversations
5. **Flexible Audio Formats** - PCM16 24kHz (primary), G.711 support

> **📚 Official Docs**: [OpenAI Realtime API Guide](https://platform.openai.com/docs/guides/realtime)  
> **📚 API Reference**: [Realtime API Reference](https://platform.openai.com/docs/api-reference/realtime)

---

## 🔧 Audio Format Requirements

The Realtime API uses **PCM16 audio** at **24kHz** sample rate:

```
Format: PCM16 (16-bit Linear PCM)
Sample Rate: 24,000 Hz
Channels: Mono (1 channel)
Encoding: Base64 (for transmission)
```

### Converting Audio in C#

```csharp
using NAudio.Wave;
using System;

public class AudioConverter
{
    // Convert any audio file to PCM16 24kHz format
    public static byte[] ConvertToPCM16(string inputPath)
    {
        using var reader = new AudioFileReader(inputPath);
        
        // Resample to 24kHz mono
        var resampler = new MediaFoundationResampler(reader, 
            new WaveFormat(24000, 16, 1));
        
        using var memoryStream = new MemoryStream();
        
        // Convert to PCM16
        var buffer = new byte[resampler.WaveFormat.AverageBytesPerSecond];
        int bytesRead;
        
        while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
        {
            memoryStream.Write(buffer, 0, bytesRead);
        }
        
        return memoryStream.ToArray();
    }
    
    // Encode PCM16 bytes to Base64 for WebSocket transmission
    public static string EncodeToBase64(byte[] audioData)
    {
        return Convert.ToBase64String(audioData);
    }
    
    // Decode Base64 to PCM16 bytes from WebSocket
    public static byte[] DecodeFromBase64(string base64Audio)
    {
        return Convert.FromBase64String(base64Audio);
    }
}
```

> **📦 NuGet Package**: `NAudio` - Install with `dotnet add package NAudio`  
> **📚 NAudio Docs**: [NAudio Documentation](https://github.com/naudio/NAudio)

---

## � Speech-to-Text (STT) Technologies

### OpenAI Whisper API

**Whisper** is OpenAI's automatic speech recognition (ASR) system that converts audio to text with high accuracy across multiple languages.

#### Basic Whisper Usage in C#

```csharp
using Microsoft.SemanticKernel;
using System.Net.Http;
using System.Text.Json;

public class WhisperSTTService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public WhisperSTTService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
    
    public async Task<string> TranscribeAudioAsync(
        string audioFilePath,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        
        // Add audio file
        var audioBytes = await File.ReadAllBytesAsync(audioFilePath, cancellationToken);
        var audioContent = new ByteArrayContent(audioBytes);
        audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            Path.GetExtension(audioFilePath) switch
            {
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".m4a" => "audio/m4a",
                ".webm" => "audio/webm",
                _ => "application/octet-stream"
            }
        );
        form.Add(audioContent, "file", Path.GetFileName(audioFilePath));
        
        // Add model
        form.Add(new StringContent("whisper-1"), "model");
        
        // Add language (optional)
        if (!string.IsNullOrEmpty(language))
        {
            form.Add(new StringContent(language), "language");
        }
        
        // Add response format
        form.Add(new StringContent("verbose_json"), "response_format");
        
        // Send request
        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/audio/transcriptions",
            form,
            cancellationToken
        );
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<WhisperResponse>(json);
        
        return result?.Text ?? string.Empty;
    }
    
    public async Task<WhisperDetailedResponse> TranscribeWithTimestampsAsync(
        string audioFilePath,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        
        var audioBytes = await File.ReadAllBytesAsync(audioFilePath, cancellationToken);
        var audioContent = new ByteArrayContent(audioBytes);
        form.Add(audioContent, "file", Path.GetFileName(audioFilePath));
        form.Add(new StringContent("whisper-1"), "model");
        form.Add(new StringContent("verbose_json"), "response_format");
        form.Add(new StringContent("word"), "timestamp_granularities[]");
        
        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/audio/transcriptions",
            form,
            cancellationToken
        );
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WhisperDetailedResponse>(json)!;
    }
}

public record WhisperResponse(string Text);

public record WhisperDetailedResponse(
    string Text,
    string Language,
    double Duration,
    WhisperWord[] Words,
    WhisperSegment[] Segments
);

public record WhisperWord(string Word, double Start, double End);

public record WhisperSegment(
    int Id,
    int Seek,
    double Start,
    double End,
    string Text,
    int[] Tokens,
    double Temperature,
    double AvgLogprob,
    double CompressionRatio,
    double NoSpeechProb
);
```

#### Streaming STT with Whisper

For real-time streaming, you need to chunk audio and send continuously:

```csharp
public class StreamingWhisperService
{
    private readonly WhisperSTTService _whisper;
    private readonly WaveInEvent _waveIn;
    private readonly MemoryStream _audioBuffer;
    private readonly Timer _chunkTimer;
    
    public event EventHandler<string>? TranscriptionReceived;
    
    public StreamingWhisperService(string apiKey, int chunkDurationSeconds = 5)
    {
        _whisper = new WhisperSTTService(apiKey);
        _audioBuffer = new MemoryStream();
        
        // Setup microphone (16kHz, 16-bit, mono - optimal for Whisper)
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 100
        };
        _waveIn.DataAvailable += OnAudioData;
        
        // Timer to send chunks periodically
        _chunkTimer = new Timer(
            async _ => await ProcessChunkAsync(),
            null,
            TimeSpan.FromSeconds(chunkDurationSeconds),
            TimeSpan.FromSeconds(chunkDurationSeconds)
        );
    }
    
    public void StartListening()
    {
        _waveIn.StartRecording();
        Console.WriteLine("🎤 Listening... (chunks every 5s)");
    }
    
    public void StopListening()
    {
        _waveIn.StopRecording();
        _chunkTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }
    
    private void OnAudioData(object? sender, WaveInEventArgs e)
    {
        lock (_audioBuffer)
        {
            _audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }
    
    private async Task ProcessChunkAsync()
    {
        byte[] audioData;
        
        lock (_audioBuffer)
        {
            if (_audioBuffer.Length == 0)
                return;
            
            audioData = _audioBuffer.ToArray();
            _audioBuffer.SetLength(0);
            _audioBuffer.Position = 0;
        }
        
        // Save to temp WAV file
        var tempFile = Path.GetTempFileName() + ".wav";
        
        try
        {
            using (var writer = new WaveFileWriter(tempFile, _waveIn.WaveFormat))
            {
                writer.Write(audioData, 0, audioData.Length);
            }
            
            // Transcribe
            var text = await _whisper.TranscribeAudioAsync(tempFile);
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                TranscriptionReceived?.Invoke(this, text);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
```

#### Usage Example

```csharp
var stt = new StreamingWhisperService("your-api-key", chunkDurationSeconds: 3);

stt.TranscriptionReceived += (sender, text) =>
{
    Console.WriteLine($"📝 Transcription: {text}");
};

stt.StartListening();

Console.WriteLine("Press Enter to stop...");
Console.ReadLine();

stt.StopListening();
```

> **📚 Whisper API Docs**: [OpenAI Whisper API](https://platform.openai.com/docs/guides/speech-to-text)  
> **💡 Tip**: Whisper supports 99+ languages. Set `language="auto"` for automatic detection.

### Azure Speech Services (Alternative)

For Microsoft Azure integration:

```csharp
// Install: dotnet add package Microsoft.CognitiveServices.Speech

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class AzureSTTService
{
    private readonly SpeechConfig _speechConfig;
    
    public AzureSTTService(string subscriptionKey, string region)
    {
        _speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        _speechConfig.SpeechRecognitionLanguage = "en-US";
    }
    
    public async Task<string> RecognizeContinuousAsync(CancellationToken cancellationToken)
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
        
        var result = new StringBuilder();
        
        recognizer.Recognizing += (s, e) =>
        {
            Console.WriteLine($"Recognizing: {e.Result.Text}");
        };
        
        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                result.AppendLine(e.Result.Text);
                Console.WriteLine($"✅ Recognized: {e.Result.Text}");
            }
        };
        
        await recognizer.StartContinuousRecognitionAsync();
        
        // Wait for cancellation
        var tcs = new TaskCompletionSource<bool>();
        cancellationToken.Register(() => tcs.SetResult(true));
        await tcs.Task;
        
        await recognizer.StopContinuousRecognitionAsync();
        
        return result.ToString();
    }
}
```

> **📚 Azure Speech**: [Azure Cognitive Services Speech](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/)

---

## 🔊 Text-to-Speech (TTS) Technologies

### OpenAI TTS API

OpenAI provides high-quality neural text-to-speech with multiple voices.

#### Basic TTS Usage in C#

```csharp
public class OpenAITTSService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public OpenAITTSService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
    
    public async Task<byte[]> SynthesizeSpeechAsync(
        string text,
        string voice = "alloy",
        string model = "tts-1",
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = model, // "tts-1" or "tts-1-hd"
            input = text,
            voice = voice, // alloy, echo, fable, onyx, nova, shimmer
            response_format = "mp3" // mp3, opus, aac, flac, wav, pcm
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/audio/speech",
            content,
            cancellationToken
        );
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
    
    public async Task SynthesizeToFileAsync(
        string text,
        string outputPath,
        string voice = "alloy",
        CancellationToken cancellationToken = default)
    {
        var audioData = await SynthesizeSpeechAsync(text, voice, "tts-1", cancellationToken);
        await File.WriteAllBytesAsync(outputPath, audioData, cancellationToken);
        Console.WriteLine($"💾 Saved audio to: {outputPath}");
    }
    
    public async Task SynthesizeAndPlayAsync(
        string text,
        string voice = "alloy",
        CancellationToken cancellationToken = default)
    {
        var audioData = await SynthesizeSpeechAsync(text, voice, "tts-1", cancellationToken);
        
        // Save to temp file and play
        var tempFile = Path.GetTempFileName() + ".mp3";
        await File.WriteAllBytesAsync(tempFile, audioData, cancellationToken);
        
        try
        {
            using var audioFile = new AudioFileReader(tempFile);
            using var outputDevice = new WaveOutEvent();
            
            outputDevice.Init(audioFile);
            outputDevice.Play();
            
            // Wait for playback to complete
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
```

#### Available Voices

| Voice | Description | Best For |
|-------|-------------|----------|
| **alloy** | Neutral, balanced | General purpose |
| **echo** | Male, clear | Professional narration |
| **fable** | Expressive, warm | Storytelling |
| **onyx** | Deep, authoritative | News, announcements |
| **nova** | Female, energetic | Conversational |
| **shimmer** | Soft, gentle | Calming content |

#### Streaming TTS

```csharp
public async Task SynthesizeStreamingAsync(
    string text,
    string voice = "alloy",
    CancellationToken cancellationToken = default)
{
    var request = new
    {
        model = "tts-1",
        input = text,
        voice = voice,
        response_format = "pcm" // PCM for streaming
    };
    
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync(
        "https://api.openai.com/v1/audio/speech",
        content,
        HttpCompletionOption.ResponseHeadersRead, // Stream response
        cancellationToken
    );
    
    response.EnsureSuccessStatusCode();
    
    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    using var waveOut = new WaveOutEvent();
    using var waveProvider = new RawSourceWaveStream(
        stream,
        new WaveFormat(24000, 16, 1) // PCM 24kHz
    );
    
    waveOut.Init(waveProvider);
    waveOut.Play();
    
    while (waveOut.PlaybackState == PlaybackState.Playing)
    {
        await Task.Delay(100, cancellationToken);
    }
}
```

#### Usage Example

```csharp
var tts = new OpenAITTSService("your-api-key");

// Generate and play
await tts.SynthesizeAndPlayAsync(
    "Hello! I'm using OpenAI's text-to-speech API.",
    voice: "nova"
);

// Save to file
await tts.SynthesizeToFileAsync(
    "This is a test of the text-to-speech system.",
    "output.mp3",
    voice: "alloy"
);
```

> **📚 TTS API Docs**: [OpenAI Text-to-Speech](https://platform.openai.com/docs/guides/text-to-speech)  
> **💰 Pricing**: $15 per 1M characters (tts-1), $30 per 1M characters (tts-1-hd)

### Azure Neural TTS (Alternative)

```csharp
using Microsoft.CognitiveServices.Speech;

public class AzureTTSService
{
    private readonly SpeechConfig _speechConfig;
    
    public AzureTTSService(string subscriptionKey, string region)
    {
        _speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
    }
    
    public async Task SpeakAsync(string text, string voiceName = "en-US-JennyNeural")
    {
        _speechConfig.SpeechSynthesisVoiceName = voiceName;
        
        using var synthesizer = new SpeechSynthesizer(_speechConfig);
        
        var result = await synthesizer.SpeakTextAsync(text);
        
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            Console.WriteLine("✅ Speech synthesized successfully");
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            Console.WriteLine($"❌ TTS canceled: {cancellation.Reason}");
        }
    }
    
    public async Task<byte[]> SynthesizeToDataAsync(string text, string voiceName = "en-US-JennyNeural")
    {
        _speechConfig.SpeechSynthesisVoiceName = voiceName;
        _speechConfig.SetSpeechSynthesisOutputFormat(
            SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3
        );
        
        using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
        var result = await synthesizer.SpeakTextAsync(text);
        
        return result.AudioData;
    }
}
```

> **📚 Azure Neural TTS**: [Azure TTS Documentation](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/text-to-speech)

---

## 🎙️ Voice Activity Detection (VAD)

**Voice Activity Detection** determines when speech is present in audio, crucial for:
- Knowing when to start/stop recording
- Reducing unnecessary API calls
- Creating natural conversation flow
- Improving transcription accuracy

### Types of VAD

#### 1. Energy-Based VAD (Simple)

```csharp
public class EnergyBasedVAD
{
    private readonly double _threshold;
    private readonly int _sampleRate;
    
    public EnergyBasedVAD(double threshold = 0.02, int sampleRate = 16000)
    {
        _threshold = threshold;
        _sampleRate = sampleRate;
    }
    
    public bool IsSpeech(byte[] audioData)
    {
        // Convert bytes to 16-bit samples
        var samples = new short[audioData.Length / 2];
        Buffer.BlockCopy(audioData, 0, samples, 0, audioData.Length);
        
        // Calculate RMS (Root Mean Square) energy
        double sum = 0;
        foreach (var sample in samples)
        {
            double normalized = sample / 32768.0;
            sum += normalized * normalized;
        }
        
        double rms = Math.Sqrt(sum / samples.Length);
        
        return rms > _threshold;
    }
    
    public SpeechSegment[] DetectSpeechSegments(byte[] audioData, int minSpeechDurationMs = 300)
    {
        var segments = new List<SpeechSegment>();
        var frameSize = _sampleRate * 2 / 100; // 10ms frames (16-bit = 2 bytes/sample)
        
        bool inSpeech = false;
        int speechStartFrame = 0;
        
        for (int i = 0; i < audioData.Length; i += frameSize)
        {
            var frameLength = Math.Min(frameSize, audioData.Length - i);
            var frame = new byte[frameLength];
            Array.Copy(audioData, i, frame, 0, frameLength);
            
            bool isSpeechFrame = IsSpeech(frame);
            int currentFrame = i / frameSize;
            
            if (isSpeechFrame && !inSpeech)
            {
                // Speech started
                speechStartFrame = currentFrame;
                inSpeech = true;
            }
            else if (!isSpeechFrame && inSpeech)
            {
                // Speech ended
                int durationMs = (currentFrame - speechStartFrame) * 10;
                
                if (durationMs >= minSpeechDurationMs)
                {
                    segments.Add(new SpeechSegment
                    {
                        StartMs = speechStartFrame * 10,
                        EndMs = currentFrame * 10,
                        DurationMs = durationMs
                    });
                }
                
                inSpeech = false;
            }
        }
        
        return segments.ToArray();
    }
}

public record SpeechSegment
{
    public int StartMs { get; init; }
    public int EndMs { get; init; }
    public int DurationMs { get; init; }
}
```

#### 2. WebRTC VAD (Production-Quality)

WebRTC VAD is a robust, low-latency VAD algorithm used in Google Chrome.

```csharp
// Install: dotnet add package WebRtcVadSharp

using WebRtcVadSharp;

public class WebRtcVADService
{
    private readonly WebRtcVad _vad;
    private readonly int _sampleRate;
    
    public WebRtcVADService(int sampleRate = 16000, OperatingMode mode = OperatingMode.Quality)
    {
        if (sampleRate != 8000 && sampleRate != 16000 && 
            sampleRate != 32000 && sampleRate != 48000)
        {
            throw new ArgumentException("Sample rate must be 8000, 16000, 32000, or 48000 Hz");
        }
        
        _sampleRate = sampleRate;
        _vad = new WebRtcVad
        {
            OperatingMode = mode // VeryAggressive, Aggressive, Normal, Quality
        };
    }
    
    public bool IsSpeech(byte[] audioFrame)
    {
        // Frame must be 10, 20, or 30ms
        // For 16kHz: 10ms = 320 bytes, 20ms = 640 bytes, 30ms = 960 bytes
        return _vad.HasSpeech(audioFrame, _sampleRate, audioFrame.Length);
    }
    
    public async Task<List<SpeechSegment>> ProcessStreamAsync(
        WaveInEvent waveIn,
        CancellationToken cancellationToken)
    {
        var segments = new List<SpeechSegment>();
        var frameSize = _sampleRate * 2 * 30 / 1000; // 30ms frames (16-bit PCM)
        var buffer = new List<byte>();
        
        bool inSpeech = false;
        int speechStartTime = 0;
        int currentTime = 0;
        
        var tcs = new TaskCompletionSource<bool>();
        cancellationToken.Register(() => tcs.SetResult(true));
        
        waveIn.DataAvailable += (sender, e) =>
        {
            buffer.AddRange(e.Buffer.Take(e.BytesRecorded));
            
            while (buffer.Count >= frameSize)
            {
                var frame = buffer.Take(frameSize).ToArray();
                buffer.RemoveRange(0, frameSize);
                
                bool hasSpeech = IsSpeech(frame);
                
                if (hasSpeech && !inSpeech)
                {
                    speechStartTime = currentTime;
                    inSpeech = true;
                    Console.WriteLine($"🎤 Speech started at {currentTime}ms");
                }
                else if (!hasSpeech && inSpeech)
                {
                    var segment = new SpeechSegment
                    {
                        StartMs = speechStartTime,
                        EndMs = currentTime,
                        DurationMs = currentTime - speechStartTime
                    };
                    segments.Add(segment);
                    inSpeech = false;
                    Console.WriteLine($"🔇 Speech ended at {currentTime}ms (duration: {segment.DurationMs}ms)");
                }
                
                currentTime += 30; // 30ms frame
            }
        };
        
        waveIn.StartRecording();
        await tcs.Task;
        waveIn.StopRecording();
        
        return segments;
    }
}
```

#### WebRTC VAD Operating Modes

| Mode | Aggressiveness | False Positives | Best For |
|------|---------------|-----------------|----------|
| **Quality** | Low | More | Continuous recording, max accuracy |
| **Normal** | Medium | Balanced | General purpose |
| **Aggressive** | High | Fewer | Noisy environments |
| **VeryAggressive** | Very High | Minimal | Very noisy, bandwidth-critical |

#### Usage Example

```csharp
var vad = new WebRtcVADService(
    sampleRate: 16000,
    mode: OperatingMode.Normal
);

using var waveIn = new WaveInEvent
{
    WaveFormat = new WaveFormat(16000, 16, 1),
    BufferMilliseconds = 30
};

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

Console.WriteLine("🎤 Listening for speech...");
var segments = await vad.ProcessStreamAsync(waveIn, cts.Token);

Console.WriteLine($"\n📊 Detected {segments.Count} speech segments:");
foreach (var segment in segments)
{
    Console.WriteLine($"  • {segment.StartMs}ms - {segment.EndMs}ms ({segment.DurationMs}ms)");
}
```

> **📦 NuGet Package**: `WebRtcVadSharp` - C# wrapper for WebRTC VAD  
> **📚 WebRTC VAD**: [WebRTC Voice Detection](https://webrtc.googlesource.com/src/+/refs/heads/main/common_audio/vad/)

#### 3. Model-Based VAD (ML)

Modern ML-based VAD using Silero VAD model:

```csharp
// Install: dotnet add package Microsoft.ML.OnnxRuntime

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public class SileroVAD : IDisposable
{
    private readonly InferenceSession _session;
    private readonly int _sampleRate;
    private float[] _state;
    private float[] _context;
    
    public SileroVAD(string modelPath, int sampleRate = 16000)
    {
        _sampleRate = sampleRate;
        _session = new InferenceSession(modelPath);
        
        // Initialize state (2, 1, 128) and context (1, 64)
        _state = new float[2 * 128];
        _context = new float[64];
    }
    
    public float GetSpeechProbability(float[] audioChunk)
    {
        // Silero VAD expects chunks of specific sizes:
        // 16kHz: 512 samples (32ms), 256, 128, 96, 64, or 32
        
        if (audioChunk.Length != 512 && audioChunk.Length != 256 && 
            audioChunk.Length != 128 && audioChunk.Length != 96 && 
            audioChunk.Length != 64 && audioChunk.Length != 32)
        {
            throw new ArgumentException($"Invalid chunk size: {audioChunk.Length}");
        }
        
        // Prepare inputs
        var inputTensor = new DenseTensor<float>(
            audioChunk,
            new[] { 1, audioChunk.Length }
        );
        
        var stateTensor = new DenseTensor<float>(
            _state,
            new[] { 2, 1, 128 }
        );
        
        var contextTensor = new DenseTensor<float>(
            _context,
            new[] { 1, 64 }
        );
        
        var sampleRateTensor = new DenseTensor<long>(
            new[] { (long)_sampleRate },
            new[] { 1 }
        );
        
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor),
            NamedOnnxValue.CreateFromTensor("state", stateTensor),
            NamedOnnxValue.CreateFromTensor("sr", sampleRateTensor)
        };
        
        // Run inference
        using var results = _session.Run(inputs);
        
        // Get outputs
        var outputTensor = results.First(v => v.Name == "output").AsEnumerable<float>().ToArray();
        var newState = results.First(v => v.Name == "stateN").AsEnumerable<float>().ToArray();
        
        // Update state for next prediction
        _state = newState;
        
        // Return speech probability [0.0 - 1.0]
        return outputTensor[0];
    }
    
    public void ResetState()
    {
        _state = new float[2 * 128];
        _context = new float[64];
    }
    
    public void Dispose()
    {
        _session?.Dispose();
    }
}

public class SileroVADProcessor
{
    private readonly SileroVAD _vad;
    private readonly float _threshold;
    private readonly int _minSpeechDurationMs;
    private readonly int _minSilenceDurationMs;
    
    public SileroVADProcessor(
        string modelPath,
        float threshold = 0.5f,
        int minSpeechDurationMs = 250,
        int minSilenceDurationMs = 100)
    {
        _vad = new SileroVAD(modelPath);
        _threshold = threshold;
        _minSpeechDurationMs = minSpeechDurationMs;
        _minSilenceDurationMs = minSilenceDurationMs;
    }
    
    public List<SpeechSegment> DetectSpeech(float[] audio, int sampleRate = 16000)
    {
        var segments = new List<SpeechSegment>();
        var chunkSize = 512; // 32ms at 16kHz
        var msPerChunk = (chunkSize * 1000) / sampleRate;
        
        bool inSpeech = false;
        int speechStartMs = 0;
        int silenceDurationMs = 0;
        
        _vad.ResetState();
        
        for (int i = 0; i < audio.Length; i += chunkSize)
        {
            var chunk = audio.Skip(i).Take(chunkSize).ToArray();
            
            if (chunk.Length < chunkSize)
                break; // Skip incomplete chunk
            
            var probability = _vad.GetSpeechProbability(chunk);
            var currentTimeMs = (i * 1000) / sampleRate;
            
            bool isSpeech = probability >= _threshold;
            
            if (isSpeech && !inSpeech)
            {
                // Potential speech start
                if (silenceDurationMs < _minSilenceDurationMs && segments.Count > 0)
                {
                    // Too short silence, extend previous segment
                    inSpeech = true;
                }
                else
                {
                    speechStartMs = currentTimeMs;
                    inSpeech = true;
                }
                silenceDurationMs = 0;
            }
            else if (!isSpeech && inSpeech)
            {
                silenceDurationMs += msPerChunk;
                
                if (silenceDurationMs >= _minSilenceDurationMs)
                {
                    var duration = currentTimeMs - speechStartMs;
                    
                    if (duration >= _minSpeechDurationMs)
                    {
                        segments.Add(new SpeechSegment
                        {
                            StartMs = speechStartMs,
                            EndMs = currentTimeMs - silenceDurationMs,
                            DurationMs = duration - silenceDurationMs
                        });
                    }
                    
                    inSpeech = false;
                    silenceDurationMs = 0;
                }
            }
            else if (isSpeech)
            {
                silenceDurationMs = 0; // Reset silence counter
            }
            else
            {
                silenceDurationMs += msPerChunk;
            }
            
            // Real-time feedback
            Console.Write($"\r🎤 Time: {currentTimeMs}ms | Probability: {probability:F3} | " +
                         $"Speech: {(isSpeech ? "YES" : "NO ")} ");
        }
        
        return segments;
    }
}
```

#### Download Silero VAD Model

```bash
# Download the ONNX model
curl -L https://github.com/snakers4/silero-vad/raw/master/files/silero_vad.onnx \
     -o silero_vad.onnx
```

#### Usage Example

```csharp
var vadProcessor = new SileroVADProcessor(
    modelPath: "silero_vad.onnx",
    threshold: 0.5f,
    minSpeechDurationMs: 250,
    minSilenceDurationMs: 100
);

// Load audio file
using var audioReader = new AudioFileReader("recording.wav");
var samples = new List<float>();
var buffer = new float[audioReader.WaveFormat.SampleRate];

int read;
while ((read = audioReader.Read(buffer, 0, buffer.Length)) > 0)
{
    samples.AddRange(buffer.Take(read));
}

// Detect speech
var segments = vadProcessor.DetectSpeech(samples.ToArray());

Console.WriteLine($"\n\n📊 Detected {segments.Count} speech segments:");
foreach (var segment in segments)
{
    Console.WriteLine($"  • {segment.StartMs / 1000.0:F2}s - {segment.EndMs / 1000.0:F2}s " +
                     $"({segment.DurationMs / 1000.0:F2}s)");
}
```

> **📦 ONNX Runtime**: `Microsoft.ML.OnnxRuntime` for running ML models  
> **📚 Silero VAD**: [Silero VAD on GitHub](https://github.com/snakers4/silero-vad)  
> **🎯 Accuracy**: ~95%+ on clean speech, robust to noise

### VAD Comparison

| Method | Latency | Accuracy | CPU Usage | Best For |
|--------|---------|----------|-----------|----------|
| **Energy-Based** | ~1ms | 70-80% | Very Low | Quick prototypes, quiet environments |
| **WebRTC VAD** | ~10ms | 85-90% | Low | Production apps, real-time |
| **Silero VAD** | ~30ms | 95%+ | Medium | High accuracy needs, recorded audio |
| **Server VAD** (OpenAI) | ~100-200ms | 95%+ | None (cloud) | OpenAI Realtime API integration |

### Practical VAD Integration

```csharp
public class SmartAudioRecorder
{
    private readonly WebRtcVADService _vad;
    private readonly WhisperSTTService _stt;
    private readonly WaveInEvent _waveIn;
    private readonly MemoryStream _audioBuffer;
    private bool _isRecording = false;
    private DateTime _lastSpeechTime;
    private readonly TimeSpan _autoStopDelay = TimeSpan.FromSeconds(2);
    
    public event EventHandler<string>? TranscriptionComplete;
    
    public SmartAudioRecorder(string apiKey)
    {
        _vad = new WebRtcVADService(16000, OperatingMode.Normal);
        _stt = new WhisperSTTService(apiKey);
        _audioBuffer = new MemoryStream();
        
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 30
        };
        _waveIn.DataAvailable += OnAudioData;
    }
    
    public void StartListening()
    {
        _waveIn.StartRecording();
        Console.WriteLine("🎤 Listening... (speak to start recording)");
    }
    
    private async void OnAudioData(object? sender, WaveInEventArgs e)
    {
        var frame = e.Buffer.Take(e.BytesRecorded).ToArray();
        bool hasSpeech = _vad.IsSpeech(frame);
        
        if (hasSpeech)
        {
            if (!_isRecording)
            {
                Console.WriteLine("\n🔴 Recording started...");
                _isRecording = true;
                _audioBuffer.SetLength(0);
            }
            
            _audioBuffer.Write(frame, 0, frame.Length);
            _lastSpeechTime = DateTime.Now;
        }
        else if (_isRecording)
        {
            // Continue recording for a bit after speech ends
            _audioBuffer.Write(frame, 0, frame.Length);
            
            if (DateTime.Now - _lastSpeechTime > _autoStopDelay)
            {
                Console.WriteLine("⏸️  Recording stopped (silence detected)");
                _isRecording = false;
                
                // Transcribe
                await TranscribeBufferAsync();
            }
        }
    }
    
    private async Task TranscribeBufferAsync()
    {
        var audioData = _audioBuffer.ToArray();
        
        if (audioData.Length == 0)
            return;
        
        // Save to temp WAV
        var tempFile = Path.GetTempFileName() + ".wav";
        
        try
        {
            using (var writer = new WaveFileWriter(tempFile, _waveIn.WaveFormat))
            {
                writer.Write(audioData, 0, audioData.Length);
            }
            
            Console.WriteLine("📝 Transcribing...");
            var text = await _stt.TranscribeAudioAsync(tempFile);
            
            Console.WriteLine($"✅ Transcription: {text}\n");
            TranscriptionComplete?.Invoke(this, text);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
```

#### Usage

```csharp
var recorder = new SmartAudioRecorder("your-api-key");

recorder.TranscriptionComplete += (sender, text) =>
{
    Console.WriteLine($"💬 You said: {text}");
    
    // Process transcription (e.g., send to chatbot)
};

recorder.StartListening();

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
```

> **💡 Key Benefit**: VAD automatically starts/stops recording based on speech, eliminating need for push-to-talk buttons!

---

## �🌐 WebSocket Connection in C#

### Basic WebSocket Client Setup

```csharp
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class OpenAIRealtimeClient : IDisposable
{
    private readonly ClientWebSocket _webSocket;
    private readonly string _apiKey;
    private readonly string _model = "gpt-4o-realtime-preview-2024-12-17";
    
    public OpenAIRealtimeClient(string apiKey)
    {
        _apiKey = apiKey;
        _webSocket = new ClientWebSocket();
    }
    
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"wss://api.openai.com/v1/realtime?model={_model}");
        
        // Add authentication header
        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        _webSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
        
        await _webSocket.ConnectAsync(uri, cancellationToken);
        Console.WriteLine("✅ Connected to OpenAI Realtime API");
    }
    
    public async Task SendMessageAsync(object message, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken
        );
    }
    
    public async Task<string?> ReceiveMessageAsync(
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[1024 * 16]; // 16KB buffer
        var result = await _webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            cancellationToken
        );
        
        if (result.MessageType == WebSocketMessageType.Close)
        {
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closing",
                cancellationToken
            );
            return null;
        }
        
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }
    
    public void Dispose()
    {
        _webSocket?.Dispose();
    }
}
```

### Usage Example

```csharp
var client = new OpenAIRealtimeClient("your-api-key");

try
{
    await client.ConnectAsync();
    
    // Send session configuration
    await client.SendMessageAsync(new
    {
        type = "session.update",
        session = new
        {
            modalities = new[] { "text", "audio" },
            instructions = "You are a helpful assistant.",
            voice = "alloy",
            temperature = 0.8
        }
    });
    
    // Start receiving messages
    while (true)
    {
        var message = await client.ReceiveMessageAsync();
        if (message == null) break;
        
        Console.WriteLine($"Received: {message}");
    }
}
finally
{
    client.Dispose();
}
```

> **📚 Learn More**: [.NET WebSocket Client](https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket)

---

## 🎤 Real-Time Voice Conversation Example

### Complete Voice Assistant Implementation

```csharp
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NAudio.Wave;

public class VoiceAssistant : IDisposable
{
    private readonly ClientWebSocket _webSocket;
    private readonly string _apiKey;
    private readonly WaveInEvent _waveIn;
    private readonly WaveOutEvent _waveOut;
    private readonly BufferedWaveProvider _playbackBuffer;
    
    public VoiceAssistant(string apiKey)
    {
        _apiKey = apiKey;
        _webSocket = new ClientWebSocket();
        
        // Setup microphone input (24kHz, 16-bit, mono)
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(24000, 16, 1),
            BufferMilliseconds = 100
        };
        _waveIn.DataAvailable += OnAudioCaptured;
        
        // Setup speaker output
        var outputFormat = new WaveFormat(24000, 16, 1);
        _playbackBuffer = new BufferedWaveProvider(outputFormat)
        {
            BufferDuration = TimeSpan.FromSeconds(10)
        };
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_playbackBuffer);
    }
    
    public async Task StartConversationAsync(CancellationToken cancellationToken = default)
    {
        // Connect to WebSocket
        var uri = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-12-17");
        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        _webSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
        
        await _webSocket.ConnectAsync(uri, cancellationToken);
        Console.WriteLine("🎤 Voice assistant connected. Start speaking!");
        
        // Configure session
        await SendEventAsync(new
        {
            type = "session.update",
            session = new
            {
                modalities = new[] { "text", "audio" },
                instructions = "You are a helpful voice assistant. Be conversational and concise.",
                voice = "alloy",
                input_audio_format = "pcm16",
                output_audio_format = "pcm16",
                input_audio_transcription = new { model = "whisper-1" },
                turn_detection = new
                {
                    type = "server_vad", // Voice Activity Detection
                    threshold = 0.5,
                    prefix_padding_ms = 300,
                    silence_duration_ms = 500
                }
            }
        });
        
        // Start microphone
        _waveIn.StartRecording();
        
        // Start playback
        _waveOut.Play();
        
        // Listen for responses
        _ = Task.Run(() => ReceiveMessagesAsync(cancellationToken), cancellationToken);
    }
    
    private void OnAudioCaptured(object? sender, WaveInEventArgs e)
    {
        // Send audio to API
        var base64Audio = Convert.ToBase64String(e.Buffer, 0, e.BytesRecorded);
        
        _ = SendEventAsync(new
        {
            type = "input_audio_buffer.append",
            audio = base64Audio
        });
    }
    
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 64]; // 64KB buffer
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken
            );
            
            if (result.MessageType == WebSocketMessageType.Close)
                break;
            
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await HandleEventAsync(json);
        }
    }
    
    private async Task HandleEventAsync(string json)
    {
        var eventData = JsonNode.Parse(json);
        var eventType = eventData?["type"]?.ToString();
        
        switch (eventType)
        {
            case "response.audio.delta":
                // Streaming audio response
                var audioDelta = eventData["delta"]?.ToString();
                if (audioDelta != null)
                {
                    var audioBytes = Convert.FromBase64String(audioDelta);
                    _playbackBuffer.AddSamples(audioBytes, 0, audioBytes.Length);
                }
                break;
            
            case "response.audio_transcript.delta":
                // Real-time transcript of AI's speech
                var transcript = eventData["delta"]?.ToString();
                Console.Write(transcript);
                break;
            
            case "conversation.item.input_audio_transcription.completed":
                // User's speech transcribed
                var userText = eventData["transcript"]?.ToString();
                Console.WriteLine($"\n👤 You: {userText}");
                break;
            
            case "response.done":
                Console.WriteLine("\n🤖 Assistant finished speaking.");
                break;
            
            case "error":
                var error = eventData["error"]?.ToString();
                Console.WriteLine($"❌ Error: {error}");
                break;
        }
    }
    
    private async Task SendEventAsync(object eventData)
    {
        var json = JsonSerializer.Serialize(eventData);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None
        );
    }
    
    public void Dispose()
    {
        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _webSocket?.Dispose();
    }
}
```

### Running the Voice Assistant

```csharp
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
using var assistant = new VoiceAssistant(apiKey!);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await assistant.StartConversationAsync(cts.Token);

Console.WriteLine("\nPress Ctrl+C to stop.");
await Task.Delay(-1, cts.Token);
```

> **💡 Key Features**:
> - **Server VAD** - Automatic speech detection (no manual button press)
> - **Bidirectional Streaming** - Speak and hear responses simultaneously
> - **Transcription** - See both your speech and AI's responses as text
> - **Natural Flow** - Interrupt and have natural conversations

---

## 🛠️ Function Calling in Voice Conversations

### Defining Functions

```csharp
public class VoiceAssistantWithFunctions : VoiceAssistant
{
    public VoiceAssistantWithFunctions(string apiKey) : base(apiKey) { }
    
    protected override async Task ConfigureSessionAsync()
    {
        await SendEventAsync(new
        {
            type = "session.update",
            session = new
            {
                modalities = new[] { "text", "audio" },
                instructions = "You are a helpful assistant. Use functions when needed.",
                voice = "alloy",
                tools = new[]
                {
                    new
                    {
                        type = "function",
                        name = "get_weather",
                        description = "Get the current weather for a location",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                location = new
                                {
                                    type = "string",
                                    description = "City name, e.g. San Francisco, CA"
                                },
                                unit = new
                                {
                                    type = "string",
                                    @enum = new[] { "celsius", "fahrenheit" }
                                }
                            },
                            required = new[] { "location" }
                        }
                    }
                },
                tool_choice = "auto"
            }
        });
    }
    
    protected override async Task HandleEventAsync(string json)
    {
        var eventData = JsonNode.Parse(json);
        var eventType = eventData?["type"]?.ToString();
        
        if (eventType == "response.function_call_arguments.done")
        {
            var callId = eventData["call_id"]?.ToString();
            var functionName = eventData["name"]?.ToString();
            var arguments = eventData["arguments"]?.ToString();
            
            Console.WriteLine($"\n🔧 Calling function: {functionName}({arguments})");
            
            // Execute function
            var result = await ExecuteFunctionAsync(functionName!, arguments!);
            
            // Send result back
            await SendEventAsync(new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "function_call_output",
                    call_id = callId,
                    output = result
                }
            });
            
            // Generate response with function result
            await SendEventAsync(new { type = "response.create" });
        }
        else
        {
            await base.HandleEventAsync(json);
        }
    }
    
    private async Task<string> ExecuteFunctionAsync(string name, string argsJson)
    {
        // Parse arguments
        var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argsJson);
        
        return name switch
        {
            "get_weather" => await GetWeatherAsync(
                args!["location"].GetString()!,
                args.ContainsKey("unit") ? args["unit"].GetString()! : "fahrenheit"
            ),
            _ => JsonSerializer.Serialize(new { error = "Unknown function" })
        };
    }
    
    private async Task<string> GetWeatherAsync(string location, string unit)
    {
        // Simulate API call
        await Task.Delay(500);
        
        return JsonSerializer.Serialize(new
        {
            location,
            temperature = unit == "celsius" ? 22 : 72,
            unit,
            condition = "sunny",
            humidity = 65
        });
    }
}
```

### Example Conversation

```
👤 User: "What's the weather like in San Francisco?"

🔧 Calling function: get_weather({"location":"San Francisco, CA","unit":"fahrenheit"})

🤖 Assistant: "The weather in San Francisco is currently sunny with a 
temperature of 72 degrees Fahrenheit and 65% humidity. It's a beautiful day!"
```

> **📚 Function Calling Guide**: [OpenAI Function Calling](https://platform.openai.com/docs/guides/function-calling)

---

## 🎬 Streaming Video Processing

While OpenAI doesn't have a dedicated video streaming API, you can process video in real-time by:

1. **Extracting frames** at regular intervals
2. **Analyzing frames** with GPT-4 Vision
3. **Streaming results** back to the application

### Real-Time Video Frame Analysis

```csharp
using OpenCvSharp;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class VideoStreamAnalyzer
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    
    public VideoStreamAnalyzer(string apiKey)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o", apiKey);
        _kernel = builder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }
    
    public async Task AnalyzeVideoStreamAsync(
        string videoSource, 
        int framesPerSecond = 1,
        CancellationToken cancellationToken = default)
    {
        using var capture = new VideoCapture(videoSource);
        
        if (!capture.IsOpened())
        {
            throw new Exception($"Cannot open video source: {videoSource}");
        }
        
        Console.WriteLine($"📹 Analyzing video stream at {framesPerSecond} FPS...\n");
        
        var frameMat = new Mat();
        var frameNumber = 0;
        var frameInterval = (int)(capture.Fps / framesPerSecond);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            "You are analyzing a video stream. Describe what you see in each frame " +
            "and note any significant changes or events."
        );
        
        while (capture.Read(frameMat) && !cancellationToken.IsCancellationRequested)
        {
            frameNumber++;
            
            // Process every Nth frame
            if (frameNumber % frameInterval != 0)
                continue;
            
            // Convert frame to base64 JPEG
            var imageData = EncodeFrameToBase64(frameMat);
            
            // Analyze with GPT-4 Vision
            chatHistory.AddUserMessage(new ChatMessageContentItemCollection
            {
                new TextContent($"Frame {frameNumber} at {frameNumber / capture.Fps:F1}s:"),
                new ImageContent(new Uri($"data:image/jpeg;base64,{imageData}"))
            });
            
            Console.WriteLine($"⏱️  Frame {frameNumber} ({frameNumber / capture.Fps:F1}s)");
            
            // Stream response
            await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(
                chatHistory,
                cancellationToken: cancellationToken))
            {
                Console.Write(chunk.Content);
            }
            
            Console.WriteLine("\n");
            
            // Keep chat history manageable (last 3 frames)
            if (chatHistory.Count > 7) // System + 3 frames (user + assistant each)
            {
                chatHistory.RemoveRange(1, 2);
            }
        }
        
        Console.WriteLine("✅ Video analysis complete.");
    }
    
    private string EncodeFrameToBase64(Mat frame)
    {
        // Resize for efficiency (optional)
        using var resized = frame.Resize(new Size(640, 480));
        
        // Encode to JPEG
        var success = Cv2.ImEncode(".jpg", resized, out var buffer, 
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 85));
        
        if (!success)
            throw new Exception("Failed to encode frame");
        
        return Convert.ToBase64String(buffer);
    }
}
```

### Usage Examples

```csharp
var analyzer = new VideoStreamAnalyzer("your-api-key");

// Analyze webcam (real-time)
await analyzer.AnalyzeVideoStreamAsync("0", framesPerSecond: 1);

// Analyze video file
await analyzer.AnalyzeVideoStreamAsync("video.mp4", framesPerSecond: 2);

// Analyze RTSP stream
await analyzer.AnalyzeVideoStreamAsync("rtsp://camera-ip/stream", framesPerSecond: 0.5);
```

> **📦 NuGet Package**: `OpenCvSharp4` - Install with `dotnet add package OpenCvSharp4`  
> **📚 OpenCvSharp Docs**: [OpenCvSharp Wiki](https://github.com/shimat/opencvsharp/wiki)

---

## 🧩 Semantic Kernel Integration Patterns

### Custom Streaming Connector

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class RealtimeAudioChatCompletionService : IChatCompletionService
{
    private readonly OpenAIRealtimeClient _client;
    
    public IReadOnlyDictionary<string, object?> Attributes => 
        new Dictionary<string, object?>();
    
    public RealtimeAudioChatCompletionService(string apiKey)
    {
        _client = new OpenAIRealtimeClient(apiKey);
    }
    
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Not typically used for streaming audio
        throw new NotSupportedException("Use GetStreamingChatMessageContentsAsync");
    }
    
    public async IAsyncEnumerable<StreamingChatMessageContent> 
        GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await _client.ConnectAsync(cancellationToken);
        
        // Send conversation history
        foreach (var message in chatHistory)
        {
            if (message.Role == AuthorRole.User)
            {
                await _client.SendMessageAsync(new
                {
                    type = "conversation.item.create",
                    item = new
                    {
                        type = "message",
                        role = "user",
                        content = new[]
                        {
                            new { type = "input_text", text = message.Content }
                        }
                    }
                });
            }
        }
        
        // Request response
        await _client.SendMessageAsync(new { type = "response.create" });
        
        // Stream responses
        while (true)
        {
            var json = await _client.ReceiveMessageAsync(cancellationToken);
            if (json == null) break;
            
            var eventData = JsonNode.Parse(json);
            var eventType = eventData?["type"]?.ToString();
            
            if (eventType == "response.audio.delta")
            {
                var audioDelta = eventData["delta"]?.ToString();
                yield return new StreamingChatMessageContent(
                    AuthorRole.Assistant,
                    audioDelta,
                    metadata: new Dictionary<string, object?> 
                    { 
                        ["type"] = "audio",
                        ["format"] = "pcm16_base64"
                    }
                );
            }
            else if (eventType == "response.done")
            {
                break;
            }
        }
    }
}
```

### Registering in Semantic Kernel

```csharp
var builder = Kernel.CreateBuilder();

// Add custom realtime audio service
builder.Services.AddSingleton<IChatCompletionService>(
    _ => new RealtimeAudioChatCompletionService("your-api-key")
);

var kernel = builder.Build();

// Use like any other chat completion service
var chatHistory = new ChatHistory();
chatHistory.AddUserMessage("Tell me a short story");

await foreach (var chunk in kernel.GetRequiredService<IChatCompletionService>()
    .GetStreamingChatMessageContentsAsync(chatHistory))
{
    if (chunk.Metadata?["type"]?.ToString() == "audio")
    {
        // Handle audio chunk
        var audioBase64 = chunk.Content;
        var audioBytes = Convert.FromBase64String(audioBase64!);
        // Play audio...
    }
}
```

---

## ⚡ Production Considerations

### 1. Connection Management

```csharp
public class ResilientRealtimeClient : IDisposable
{
    private ClientWebSocket? _webSocket;
    private readonly string _apiKey;
    private readonly SemaphoreSlim _reconnectLock = new(1, 1);
    private int _reconnectAttempts = 0;
    private const int MaxReconnectAttempts = 5;
    
    public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_webSocket?.State == WebSocketState.Open)
            return;
        
        await _reconnectLock.WaitAsync(cancellationToken);
        try
        {
            while (_reconnectAttempts < MaxReconnectAttempts)
            {
                try
                {
                    _webSocket?.Dispose();
                    _webSocket = new ClientWebSocket();
                    
                    var uri = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-12-17");
                    _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
                    _webSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
                    
                    await _webSocket.ConnectAsync(uri, cancellationToken);
                    
                    Console.WriteLine("✅ Reconnected successfully");
                    _reconnectAttempts = 0;
                    return;
                }
                catch (Exception ex)
                {
                    _reconnectAttempts++;
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, _reconnectAttempts));
                    
                    Console.WriteLine(
                        $"⚠️  Connection failed (attempt {_reconnectAttempts}/{MaxReconnectAttempts}). " +
                        $"Retrying in {delay.TotalSeconds}s... Error: {ex.Message}"
                    );
                    
                    await Task.Delay(delay, cancellationToken);
                }
            }
            
            throw new Exception("Max reconnection attempts reached");
        }
        finally
        {
            _reconnectLock.Release();
        }
    }
    
    public void Dispose()
    {
        _webSocket?.Dispose();
        _reconnectLock?.Dispose();
    }
}
```

### 2. Error Handling

```csharp
public async Task HandleRealtimeEventsAsync(CancellationToken cancellationToken)
{
    var buffer = new byte[1024 * 64];
    
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            await EnsureConnectedAsync(cancellationToken);
            
            var result = await _webSocket!.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken
            );
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("🔌 Server closed connection");
                await EnsureConnectedAsync(cancellationToken);
                continue;
            }
            
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var eventData = JsonNode.Parse(json);
            
            if (eventData?["type"]?.ToString() == "error")
            {
                var errorType = eventData["error"]?["type"]?.ToString();
                var errorMessage = eventData["error"]?["message"]?.ToString();
                
                Console.WriteLine($"❌ API Error [{errorType}]: {errorMessage}");
                
                // Handle specific errors
                if (errorType == "invalid_request_error")
                {
                    // Don't retry, fix the request
                    throw new InvalidOperationException(errorMessage);
                }
                else if (errorType == "rate_limit_error")
                {
                    // Wait and retry
                    await Task.Delay(5000, cancellationToken);
                }
            }
            else
            {
                await ProcessEventAsync(eventData!);
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"⚠️  WebSocket error: {ex.Message}");
            await Task.Delay(1000, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }
}
```

### 3. Audio Buffer Management

```csharp
public class CircularAudioBuffer
{
    private readonly byte[] _buffer;
    private int _writePosition = 0;
    private int _readPosition = 0;
    private readonly object _lock = new();
    
    public CircularAudioBuffer(int sizeInBytes)
    {
        _buffer = new byte[sizeInBytes];
    }
    
    public void Write(byte[] data, int offset, int count)
    {
        lock (_lock)
        {
            for (int i = 0; i < count; i++)
            {
                _buffer[_writePosition] = data[offset + i];
                _writePosition = (_writePosition + 1) % _buffer.Length;
                
                // Overwrite detection
                if (_writePosition == _readPosition)
                {
                    _readPosition = (_readPosition + 1) % _buffer.Length;
                    Console.WriteLine("⚠️  Buffer overrun - dropped audio data");
                }
            }
        }
    }
    
    public int Read(byte[] data, int offset, int count)
    {
        lock (_lock)
        {
            int available = AvailableBytes;
            int toRead = Math.Min(count, available);
            
            for (int i = 0; i < toRead; i++)
            {
                data[offset + i] = _buffer[_readPosition];
                _readPosition = (_readPosition + 1) % _buffer.Length;
            }
            
            return toRead;
        }
    }
    
    public int AvailableBytes
    {
        get
        {
            lock (_lock)
            {
                if (_writePosition >= _readPosition)
                    return _writePosition - _readPosition;
                else
                    return _buffer.Length - _readPosition + _writePosition;
            }
        }
    }
}
```

### 4. Cost Optimization

```csharp
public class CostOptimizedVideoAnalyzer : VideoStreamAnalyzer
{
    private DateTime _lastAnalysisTime = DateTime.MinValue;
    private readonly TimeSpan _minAnalysisInterval = TimeSpan.FromSeconds(5);
    
    // Cache for similar frames
    private readonly Dictionary<int, string> _frameHashCache = new();
    private string? _lastAnalysis;
    
    protected override async Task<bool> ShouldAnalyzeFrameAsync(Mat frame)
    {
        // Rate limiting
        if (DateTime.UtcNow - _lastAnalysisTime < _minAnalysisInterval)
            return false;
        
        // Motion detection - only analyze if frame changed significantly
        var currentHash = ComputeFrameHash(frame);
        
        if (_frameHashCache.TryGetValue(currentHash, out var cachedAnalysis))
        {
            Console.WriteLine($"📋 Using cached analysis (motion threshold not met)");
            return false;
        }
        
        _lastAnalysisTime = DateTime.UtcNow;
        return true;
    }
    
    private int ComputeFrameHash(Mat frame)
    {
        // Simple perceptual hash using image histogram
        using var gray = new Mat();
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
        
        // Resize to small fixed size
        using var small = gray.Resize(new Size(8, 8));
        
        // Compute average pixel value
        var mean = Cv2.Mean(small);
        
        // Binarize
        int hash = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (small.At<byte>(i, j) > mean.Val0)
                    hash |= 1 << (i * 8 + j);
            }
        }
        
        return hash;
    }
}
```

---

## 📊 Latency Optimization Tips

### Audio Streaming
- Use **PCM16 24kHz** directly (no conversion overhead)
- Set **buffer size to 100-200ms** for input
- Enable **Server VAD** for automatic turn detection
- Use **low temperature** (0.6-0.7) for faster responses

### Video Processing
- **Resize frames** to 640x480 or lower
- **Reduce frame rate** (1-2 FPS is often sufficient)
- Use **motion detection** to skip similar frames
- Consider **edge processing** (analyze locally, send only annotations)

### Network
- Use **WebSocket keep-alive** to prevent connection drops
- Implement **connection pooling** for multiple sessions
- Monitor **RTT** and adjust buffer sizes dynamically

---

## 🔗 Additional Resources

### Official Documentation
- [OpenAI Realtime API Guide](https://platform.openai.com/docs/guides/realtime) - Complete API documentation
- [Realtime API Reference](https://platform.openai.com/docs/api-reference/realtime) - Event types and parameters
- [GPT-4 with Vision](https://platform.openai.com/docs/guides/vision) - Image and video understanding

### C# Libraries
- [NAudio](https://github.com/naudio/NAudio) - Audio capture and playback
- [OpenCvSharp](https://github.com/shimat/opencvsharp) - Video processing
- [System.Net.WebSockets](https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets) - WebSocket client

### Semantic Kernel
- [SK Chat Completion](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/) - Chat service architecture
- [SK Streaming](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/streaming) - Streaming patterns

### Community Examples
- [OpenAI Cookbook - Realtime](https://cookbook.openai.com/examples/gpt_with_realtime_api) - Python examples (adaptable to C#)
- [Azure OpenAI Realtime](https://learn.microsoft.com/en-us/azure/ai-services/openai/realtime-audio-quickstart) - Azure-specific guide

---

## 🎯 Key Takeaways

1. **Realtime API** enables natural voice conversations with sub-500ms latency
2. **WebSocket** connections provide bidirectional streaming for audio
3. **Server VAD** automatically detects when users start/stop speaking
4. **Function calling** works seamlessly during voice conversations
5. **Video streaming** requires frame extraction + GPT-4 Vision analysis
6. **Production readiness** needs connection management, error handling, and cost optimization
7. **Semantic Kernel** can integrate custom streaming connectors

---

## 🧪 Practice Exercise

**Build a Real-Time Voice Translator**:
1. Capture user's audio in real-time
2. Use Realtime API to transcribe speech
3. Translate to target language (via function calling or prompt)
4. Stream translated audio back to user
5. Display both original and translated text

**Bonus**: Add video stream from webcam showing live captions of both languages!

---

## 📝 Next Steps

- **Lab Exercise**: Build the Voice Translator (30-45 min)
- **Read**: [learn06.md](learn06.md) for batch audio/image processing
- **Explore**: Function calling patterns in voice conversations
- **Advanced**: Multi-speaker recognition and diarization

---

**Questions? Check the quiz coming soon!** 🎓

*Estimated completion time: ~30 minutes (reading + basic implementation)*
