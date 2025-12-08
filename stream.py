import whisper
import sounddevice as sd
import numpy as np
import queue
import webrtcvad
import uuid
import json
import os

# -----------------------
# Einstellungen für den Stream
# -----------------------
CHUNK_DURATION = 4       # Sekunden pro Chunk, bei 4 sind die Daten. nicht ganz so abgehackt
SAMPLERATE = 16000       # Audio-Sample-Rate
LANGUAGE = "de"          # Sprache festlegen, muss dann später extern über das Menü festgelegt werden
VAD_STAERKE = 2          # 0-3, höhere Zahl = aggressivere Sprachdetektion, soll Halluzinationen durch Hintergrund rauschen reduzieren. Funktioniert bedingt.
FRAME_DURATION = 30      # ms, muss 10, 20 oder 30 sein
EXPORT_DIR = "exports"   # Ordner für JSON-Export, dort werden die JSONs gesammelt und später abgegriffen

# -----------------------
# Setup
# -----------------------
os.makedirs(EXPORT_DIR, exist_ok=True)
audio_queue = queue.Queue()
vad = webrtcvad.Vad(VAD_STAERKE)
model = whisper.load_model("small")

# -----------------------
# Mikrofon Callback
# -----------------------
def audio_callback(indata, frames, time, status):
    if status:
        print("Audio-Status:", status)
    audio_queue.put(indata.copy())

# -----------------------
# Hilfsfunktion: VAD auf Chunk prüfen
# -----------------------
def is_speech(audio_chunk):
    """
    Prüft, ob ein Audio-Chunk Sprache enthält
    audio_chunk: np.float32 zwischen -1.0 und 1.0
    """
    # In PCM16 umwandeln
    audio_int16 = (audio_chunk * 32767).astype(np.int16)
    frame_size = int(SAMPLERATE * FRAME_DURATION / 1000)
    num_frames = len(audio_int16) // frame_size

    for i in range(num_frames):
        frame_bytes = audio_int16[i*frame_size:(i+1)*frame_size].tobytes()
        if vad.is_speech(frame_bytes, SAMPLERATE):
            return True
    return False

# -----------------------
# Streaming-Transkription
# -----------------------
def stream_transcribe(chunk_duration=CHUNK_DURATION, samplerate=SAMPLERATE):
    chunk_size = int(chunk_duration * samplerate)

    with sd.InputStream(channels=1, samplerate=samplerate, callback=audio_callback):
        print("Starte Streaming... Sprich jetzt!")
        buffer = np.zeros((0,), dtype=np.float32)

        try:
            while True:
                # Audio von Queue holen
                chunk = audio_queue.get()
                buffer = np.concatenate((buffer, chunk.flatten()))

                # Wenn genug Audio für einen Chunk
                if len(buffer) >= chunk_size:
                    audio_chunk = buffer[:chunk_size]
                    buffer = buffer[chunk_size:]

                    # Lautstärke-Threshold
                    if np.abs(audio_chunk).mean() < 0.01:
                        continue

                    # VAD prüfen
                    if not is_speech(audio_chunk):
                        continue

                    # Transkription
                    result = model.transcribe(audio_chunk, fp16=False, language=LANGUAGE)
                    text = result["text"].strip()
                    if text == "":
                        continue

                    # JSON-Export mit eindeutiger ID
                    chunk_id = str(uuid.uuid4())
                    export = {"ID": chunk_id, "Content": text}
                    with open(f"{EXPORT_DIR}/{chunk_id}.json", "w", encoding="utf-8") as f:
                        json.dump(export, f, ensure_ascii=False, indent=2)

                    # Ausgabe
                    print(f"Teil-Transkript: {text}")

        except KeyboardInterrupt:
            print("Streaming beendet.")

# -----------------------
# Main
# -----------------------
if __name__ == "__main__":
    stream_transcribe()