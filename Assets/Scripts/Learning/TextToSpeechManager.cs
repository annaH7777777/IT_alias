using UnityEngine;

/// <summary>
/// Thin wrapper over Android's native android.speech.tts.TextToSpeech.
/// Singleton; created on first access and persists across scenes.
/// In the Editor it logs instead of speaking.
/// </summary>
public class TextToSpeechManager : MonoBehaviour
{
    private static TextToSpeechManager _instance;

    public static TextToSpeechManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[TextToSpeechManager]");
                _instance = go.AddComponent<TextToSpeechManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
#if UNITY_ANDROID && !UNITY_EDITOR
        InitTts();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _tts;
    private bool _ready;
    private int _utteranceCounter;

    // OnInit callback runs on a JNI thread — drain on the Unity main thread via Update.
    private volatile int _pendingInitStatus = int.MinValue;

    private void InitTts()
    {
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                var listener = new InitListener(this);
                _tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, listener);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("TTS init failed: " + e.Message);
        }
    }

    private void Update()
    {
        if (_pendingInitStatus != int.MinValue)
        {
            int status = _pendingInitStatus;
            _pendingInitStatus = int.MinValue;
            ApplyInitStatus(status);
        }
    }

    private void ApplyInitStatus(int status)
    {
        // TextToSpeech.SUCCESS = 0
        if (status != 0)
        {
            Debug.LogError("TTS init returned status " + status);
            return;
        }
        try
        {
            using (var locale = new AndroidJavaObject("java.util.Locale", "en", "US"))
            {
                _tts.Call<int>("setLanguage", locale);
            }
            _ready = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("TTS setLanguage failed: " + e.Message);
        }
    }

    public void Speak(string text)
    {
        if (!_ready || _tts == null || string.IsNullOrEmpty(text)) return;
        try
        {
            _utteranceCounter++;
            // speak(CharSequence, int queueMode=QUEUE_FLUSH(0), Bundle, String utteranceId) — API 21+
            AndroidJavaObject bundle = null;
            _tts.Call<int>("speak", text, 0, bundle, "utt_" + _utteranceCounter);
        }
        catch (System.Exception e)
        {
            Debug.LogError("TTS speak failed: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        if (_tts == null) return;
        try { _tts.Call("stop"); } catch { }
        try { _tts.Call("shutdown"); } catch { }
        _tts.Dispose();
        _tts = null;
    }

    private class InitListener : AndroidJavaProxy
    {
        private readonly TextToSpeechManager _owner;
        public InitListener(TextToSpeechManager owner)
            : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            _owner = owner;
        }

        // Matches Java signature: void onInit(int status)
        public void onInit(int status)
        {
            _owner._pendingInitStatus = status;
        }
    }
#elif UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void WebGLSpeak(string text);

    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        try { WebGLSpeak(text); }
        catch (System.Exception e) { Debug.LogError("WebGL TTS failed: " + e.Message); }
    }
#else
    public void Speak(string text)
    {
        Debug.Log($"[TTS stub] would speak: {text}");
    }
#endif
}
