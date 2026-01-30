import io
import wave
from pathlib import Path
from piper import PiperVoice, SynthesisConfig

# =====================================================
# Pfade
# =====================================================
BASE_DIR = Path(__file__).resolve().parent
MODEL_PATH = BASE_DIR / "models" / "en_US-lessac-medium.onnx"

# =====================================================
# Lazy Loading der Stimme (sehr wichtig!)
# =====================================================
_voice = None

def get_voice() -> PiperVoice:
    global _voice
    if _voice is None:
        if not MODEL_PATH.exists():
            raise FileNotFoundError(
                f"Piper model not found at: {MODEL_PATH}"
            )
        _voice = PiperVoice.load(str(MODEL_PATH))
    return _voice

# =====================================================
#Konfigurationen für die Audioerzeugung
# ===================================================== 
syn_config = SynthesisConfig(
    volume=0.5,  # relative Lautstärke (Standard = 1.0) 
    length_scale=1.0,  # Sprechgeschwindigkeit (größer = langsamer)
    noise_scale=1.0,  # Stimmvariation 
    noise_w_scale=1.0,  # Variation in Timing und Betonung 
    normalize_audio=False, # Lautstärke Normalisierung (False = Rohes PCM)
)

# =====================================================
#Lädt das TTS Modell 
# =====================================================
def speaker(text_input: str) -> bytes:
    voice = get_voice()

    buffer = io.BytesIO()
    with wave.open(buffer, "wb") as wav_file:
        voice.synthesize_wav(
            text_input,
            wav_file,
            syn_config=syn_config
        )

    buffer.seek(0)
    return buffer.read()