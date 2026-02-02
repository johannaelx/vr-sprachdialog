# VR/AR Speech Dialogue Prototype

A VR-based learning prototype that integrates open-source AI speech models to enable natural spoken dialogue inside immersive environments. The project explores how real-time voice interaction can support language learning in everyday scenarios such as a bakery, doctor’s visit, or ticket counter.

### Features

- Real-time speech pipeline: **ASR (Whisper)** → **LLM (LLaMA/Mistral)** → **TTS (Piper)**
- Interactive VR scenes built with **Unity**
- Natural spoken conversations with NPCs
- Optional subtitles and multiple difficulty levels (A1–B2)

### Project Goal

Create an immersive, playful VR training experience where users practice authentic conversations and gain confidence speaking a foreign language.

## 🚀 How to Start the Application
1. Open a terminal and change into the `backend` directory.
```bash
cd backend
```

2. Install all required Python packages using `pip`.
```bash
pip install -r requirements.txt
```

3. Additionally, download the Piper text-to-speech voice model used by the backend. This step only needs to be done once.
```bash
cd app/tts/models
```
```bash
python -m piper.download_voices en_US-ryan-medium
```

4. Change into the `backend`directory. Start the FastAPI server using Uvicorn. After starting, the backend will be available at `http://localhost:8000`. You can verify that the server ist running by opening `http://localhost:8000/health`.
```bash
python -m uvicorn app.main:app --host 0.0.0.0 --port 8000
```

5. Open the Unity project using the Unity Editor. Start the scene in Unity and press and hold the space key to record speech input.

