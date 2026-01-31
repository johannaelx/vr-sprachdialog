import io
import wave
from pathlib import Path
from piper import PiperVoice, SynthesisConfig

# base directory of this module
BASE_DIR = Path(__file__).resolve().parent

# path to the Piper TTS model
MODEL_PATH = BASE_DIR / "models" / "en_US-lessac-medium.onnx"

_voice = None


def get_voice() -> PiperVoice:
    """
    Lazily loads and returns the Piper voice model.

    The model is loaded only once and reused across requests to avoid expensive reinitialization.
    """
    global _voice
    if _voice is None:
        if not MODEL_PATH.exists():
            raise FileNotFoundError(f"Piper model not found at: {MODEL_PATH}")
        _voice = PiperVoice.load(str(MODEL_PATH))
    return _voice


# configuration for speech synthesis
syn_config = SynthesisConfig(
    volume=0.5,  # output volume (default = 1.0)
    length_scale=1.0,  # speaking rate (higher -> slower)
    noise_scale=1.0,  # voice variation
    noise_w_scale=1.0,  # timing and prosody variation
    normalize_audio=False,  # disable normalization (raw PCM output)
)


def speaker(text_input: str) -> bytes:
    """
    Synthesizes speech from text and returns WAV audio as bytes.

    Args:
        text_input: The text to be spoken.

    Returns:
        WAV audio data as bytes.
    """
    voice = get_voice()

    buffer = io.BytesIO()
    with wave.open(buffer, "wb") as wav_file:
        voice.synthesize_wav(text_input, wav_file, syn_config=syn_config)

    buffer.seek(0)
    return buffer.read()
