import wave
from piper import PiperVoice
from piper import SynthesisConfig

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
voice = PiperVoice.load("/path/to/en_US-lessac-medium.onnx") #Pfad muss ggf angepasst werden

def speaker(text_input: str, output_path: str = "output.wav") -> str:
    with wave.open(output_path, "wb") as wav_file:
        voice.synthesize_wav(text_input, wav_file, syn_config=syn_config)

    return output_path