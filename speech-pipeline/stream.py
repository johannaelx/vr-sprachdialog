import whisper              #Sprachmodell zum Transkribieren 
import sounddevice as sd    #Ermöglicht die Nutzung des Mikrofons in Echtzeit
import numpy as np          #Zum erstellen von Arrays und des Audiobuffers
import queue                #Ebenfalls für den Audiobuffer genutzt
import webrtcvad            #Voice Activity Detection, soll Störgeräusche rausfiltern
import json                 #wird benötigt um die Daten als JSON zu exportieren
import os                   #Ermöglicht Zugriffe auf das Dateisystem (lesen/schreiben/löschen)
import threading            #Wird genutzt um den laufenden Stream mit Stopthread zu stoppen
import sys                  #Liest die Tastatur eingaben, geht Hand in Hand mit Threading
import time                 #Wird für Zeitmessungen genutzt, in unserem Fall zum Sortieren der Chunks

# -----------------------
# Einstellungen für den Stream (Konstanten und Variablen)
# -----------------------
CHUNK_DURATION = 4       # Sekunden pro Chunk
SAMPLERATE = 16000       # Audio-Sample-Rate
LANGUAGE = "de"          # Sprache festlegen, kann später über Menü angepasst werden
VAD_STAERKE = 1          # 0-3
FRAME_DURATION = 30      # ms, muss 10, 20 oder 30 sein (webrtcvad akzeptiert nichts anderes)
EXPORT_DIR = "exports"   # Ordner für JSON-Export
last_timestamp = 0       #Timestamp Variable
counter = 0              #Counter startend bei Null
export_files = []        #Array für die Exports

# -----------------------
# Setup, laden von Whisper und Vad und erstellen von Ordnern falls nicht vorhanden
# -----------------------
os.makedirs(EXPORT_DIR, exist_ok=True) 
audio_queue = queue.Queue()
vad = webrtcvad.Vad(VAD_STAERKE)
model = whisper.load_model("small")

# Event zum Stoppen des Streams
stop_event = threading.Event()

# -----------------------
# Mikrofon Callback, Rohdaten werden in die Queue kopiert um der Logik den Zugriff zu ermöglichen
# -----------------------
def audio_callback(indata, status):
    if status:
        print("Audio-Status:", status)
    # indata ist shape (frames, channels)
    audio_queue.put(indata.copy())

# -----------------------
# Hilfsfunktion: VAD auf Chunk anwenden um Störgeräusche zu entfernen
# -----------------------
def is_speech(audio_chunk):
    # In PCM16 umwandeln, notwendig da VAD nicht mit Float arbeiten kann
    audio_int16 = (audio_chunk * 32767).astype(np.int16)
    frame_size = int(SAMPLERATE * FRAME_DURATION / 1000)
    if frame_size == 0:
        return False
    num_frames = len(audio_int16) // frame_size
    if num_frames == 0:
        return False

    for i in range(num_frames):
        start = i * frame_size
        end = start + frame_size
        frame_bytes = audio_int16[start:end].tobytes()
        if vad.is_speech(frame_bytes, SAMPLERATE):
            return True
    return False

# -----------------------
# Erstellt die ID, bestehend aus Timestamp und Increment
# zweiteres ist eigentlich nicht nötig aber zur Sicherheit dabei
# -----------------------

def generate_sorted_id():
    global last_timestamp, counter

    ts = int(time.time() * 1000)  # Millisekunden
    
    if ts == last_timestamp:
        counter += 1
    else:
        last_timestamp = ts
        counter = 0

    return f"{ts}_{counter:04d}"

# -----------------------
# Thread-Funktion: Wartet auf Enter oder 'q' + Enter zum Stoppen
# -----------------------
def wait_for_stop():
# Läuft in einem separaten Thread. Warte auf Benutzereingabe.
    try:
        s = sys.stdin.readline().strip()
        if s == "" or s.lower() == "q":
            stop_event.set()
    except Exception as e:
        print("Stop-Thread Fehler:", e)
        stop_event.set()

# -----------------------
# Zusammenfügen der JSON nach Ende des Streams
# -----------------------
def combine_json():

    files = sorted([
        f for f in os.listdir(EXPORT_DIR)
        if f.endswith(".json")
    ])

    if not files:
        print("Keine JSON-Dateien zum Zusammenführen gefunden.")
        return None, None

    combined_text = ""
    chunks = []

    # JSON-Dateien einlesen
    for filename in files:
        filepath = os.path.join(EXPORT_DIR, filename)
        try:
            with open(filepath, "r", encoding="utf-8") as f:
                data = json.load(f)

            chunks.append(data)
            combined_text += data.get("Content", "").strip() + " "

        except Exception as e:
            print(f"Fehler beim Lesen von {filename}: {e}")

    # Archiv Ordner vorbereiten, Name enthält Timestamp, deswegen erst hier und nicht beim Start
    timestamp = time.strftime("%Y-%m-%d_%H-%M-%S")
    archive_dir = os.path.join("archive", timestamp)
    os.makedirs(archive_dir, exist_ok=True)

    # combined.json erzeugen
    combined_json_path = os.path.join(archive_dir, "combined.json")
    output = {
        "chunks": chunks,
        "combined_text": combined_text.strip()
    }

    with open(combined_json_path, "w", encoding="utf-8") as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    # Einzel-Chunks verschieben, so wird sicher dass keine Alten Daten in die combined JSON rutschen
    for filename in files:
        src = os.path.join(EXPORT_DIR, filename)
        dst = os.path.join(archive_dir, filename)
        os.rename(src, dst)

    print(f"JSON erfolgreich archiviert → {combined_json_path}")

    return combined_json_path, archive_dir
# -----------------------
# Streaming-Transkription, die eigentliche Hauptfunktion.
# -----------------------
def stream_transcribe(chunk_duration=CHUNK_DURATION, samplerate=SAMPLERATE):
    chunk_size = int(chunk_duration * samplerate)

    # Starte Stop-Thread
    stopper = threading.Thread(target=wait_for_stop, daemon=True)
    stopper.start()

    with sd.InputStream(channels=1, samplerate=samplerate, callback=audio_callback):
        print("Starte Streaming... Sprich jetzt!")
        buffer = np.zeros((0,), dtype=np.float32)

        try:
            while not stop_event.is_set():
                # Versuche kurz Audio aus der Queue zu holen (timeout), um Stop-Event regelmäßig zu prüfen
                try:
                    chunk = audio_queue.get(timeout=0.5)  # blockiert max 0.5s
                except queue.Empty:
                    continue

                buffer = np.concatenate((buffer, chunk.flatten()))

                # Wenn genug Audio für einen Chunk
                while len(buffer) >= chunk_size and not stop_event.is_set():
                    audio_chunk = buffer[:chunk_size]
                    buffer = buffer[chunk_size:]

                    # Lautstärke-Threshold, zusätzlicher Filter gegen Hintergrundgeräusche oder Rauschen
                    if np.abs(audio_chunk).mean() < 0.01:
                        continue

                    # VAD prüfen (prüft in 10/20/30ms-Frames), wenn keine Sprache erkannt wird 
                    # geht es hier nicht weiter, so soll unnötige Auslastung von Whisper verhindert werden
                    if not is_speech(audio_chunk):
                        continue

                    # Transkription durch Whisper
                    result = model.transcribe(audio_chunk, fp16=False, language=LANGUAGE)
                    text = result.get("text", "").strip()
                    if text == "":
                        continue

                    # JSON-Export mit eindeutiger ID
                    chunk_id = generate_sorted_id()
                    export = {"ID": chunk_id, "Content": text}
                    with open(f"{EXPORT_DIR}/{chunk_id}.json", "w", encoding="utf-8") as f:
                        json.dump(export, f, ensure_ascii=False, indent=2)

                    # Ausgabe
                    print(f"Teil-Transkript: {text}")

        except KeyboardInterrupt:
            print("Streaming beendet.")
            stop_event.set()
        finally:
            combined_json, archive_file = combine_json()
            return combined_json, archive_file
        

# -----------------------
# Main
# -----------------------
if __name__ == "__main__":
   combined_json, archive_file = stream_transcribe()