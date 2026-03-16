import io
import wave
from pathlib import Path
from piper import PiperVoice, SynthesisConfig

# base directory of this module
BASE_DIR = Path(__file__).resolve().parent

# directory containing TTS models
MODEL_DIR = BASE_DIR / "models"

# mapping NPC personalities → voice models
VOICE_MODELS = {
    "baker": "en_US-ryan-medium.onnx",
    "friend": "en_US-hfc_male-medium.onnx",
    "default": "en_US-ryan-medium.onnx",
}

# cache loaded voices to avoid reloading models
VOICE_CACHE = {}


def get_voice(npc_type: str) -> PiperVoice:
    """
    Returns the Piper voice for the given NPC type.
    Voices are cached to avoid repeated model loading.
    """

    model_name = VOICE_MODELS.get(npc_type, VOICE_MODELS["default"])

    if model_name in VOICE_CACHE:
        return VOICE_CACHE[model_name]

    model_path = MODEL_DIR / model_name

    if not model_path.exists():
        raise FileNotFoundError(f"Piper model not found: {model_path}")

    voice = PiperVoice.load(str(model_path))

    VOICE_CACHE[model_name] = voice

    print(f"Loaded TTS voice model: {model_name}")

    return voice


# configuration for speech synthesis
syn_config = SynthesisConfig(
    volume=0.5,
    length_scale=1.0,
    noise_scale=1.0,
    noise_w_scale=1.0,
    normalize_audio=False,
)


def speaker(text_input: str, npc_type: str) -> bytes:
    """
    Synthesizes speech from text using the voice assigned to the NPC type.
    """

    voice = get_voice(npc_type)

    buffer = io.BytesIO()

    with wave.open(buffer, "wb") as wav_file:
        voice.synthesize_wav(text_input, wav_file, syn_config=syn_config)

    buffer.seek(0)

    return buffer.read()
