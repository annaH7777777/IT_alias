mergeInto(LibraryManager.library, {
  WebGLSpeak: function(textPtr) {
    var text = UTF8ToString(textPtr);
    if (!text) return;
    if (typeof window === 'undefined' || typeof window.speechSynthesis === 'undefined') {
      console.warn('Web Speech API not available in this browser.');
      return;
    }
    try {
      window.speechSynthesis.cancel();
      var u = new SpeechSynthesisUtterance(text);
      u.lang = 'en-US';
      u.rate = 0.9;
      window.speechSynthesis.speak(u);
    } catch (e) {
      console.error('Web Speech error: ' + e);
    }
  }
});
