from dotenv import load_dotenv

load_dotenv()  # must run before importing modules that access env vars

from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import Response

from app.asr.whisper import transcribe_wav_bytes
from app.llm.openai_api import baker_npc
from app.tts.piper import speaker

import traceback

app = FastAPI(title="VR Speech Backend")


@app.get("/health")
def health():
    return {"status": "ok"}


# Audio pipeline endpoint (ASR -> LLM -> TTS)
@app.post("/conversation")
async def conversation(audio: UploadFile = File(...)):
    if audio.content_type not in ("audio/wav", "audio/x-wav"):
        raise HTTPException(
            status_code=400,
            detail="Invalid audio format. Only WAV files are supported.",
        )

    wav_bytes = await audio.read()

    if len(wav_bytes) == 0:
        raise HTTPException(status_code=400, detail="Empty audio file.")

    try:
        # ASR
        transcription: str = transcribe_wav_bytes(wav_bytes)
        print("TRANSCRIPTION:", repr(transcription))

        # LLM
        llm_response: dict = baker_npc(transcription)
        print("LLM_RESPONSE:", repr(llm_response))

        # TTS
        tts_audio: bytes = speaker(llm_response["reply"])

    except Exception as e:
        traceback.print_exc()
        raise HTTPException(
            status_code=500, detail=f"Conversation pipeline failed: {str(e)}"
        )

    # Audio response (WAV) from TTS
    return Response(content=tts_audio, media_type="audio/wav")
