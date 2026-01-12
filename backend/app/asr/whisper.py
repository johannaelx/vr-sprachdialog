import io
import numpy as np
import whisper
import soundfile as sf

model = whisper.load_model("small")

def wav_bytes_to_pcm(wac_bytes: bytes, expected_sr: int = 16000) -> np.ndarray:
    with io.BytesIO(wac_bytes) as wav_io:
        audio, samplerate = sf.read(wav_io, dtype="float32")

    if samplerate != expected_sr:
        raise ValueError(f"Unexpected sample rate: {samplerate}, expected: {expected_sr}")
    
    # convert to mono if stereo
    if audio.ndim > 1:
        audio = audio.mean(axis=1)

    return audio

def transcribe_pcm(audio_pcm: np.ndarray, language: str = "de") -> str:
    if audio_pcm.ndim != 1:
        raise ValueError("audio_pcm must be a 1D mono signal")
    
    if audio_pcm.size == 0:
        return ""
    
    result = model.transcribe(
        audio_pcm, 
        fp16=False, 
        language=language
    )

    return result.get("text", "").strip()

def transcribe_wav_bytes(
    wav_bytes: bytes,
    language: str = "de"
) -> str:
    audio_pcm = wav_bytes_to_pcm(wav_bytes)
    return transcribe_pcm(audio_pcm, language)