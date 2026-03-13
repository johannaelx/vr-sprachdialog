from dotenv import load_dotenv

load_dotenv()  # must run before importing modules that access env vars

import base64

from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import JSONResponse

from app.asr.whisper import transcribe_wav_bytes
from app.llm.openai_api import baker_npc
from app.tts.piper import speaker

import traceback

app = FastAPI(title="VR Speech Backend")


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/conversation")
async def conversation(audio: UploadFile = File(...)):
    """
    Processes a spoken user input through the full speech pipeline:
    ASR (Whisper) -> LLM (NPC logic) -> TTS (Piper).

    Expects a WAV audio file and returns synthesized speech as WAV audio.
    """
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

    # encode WAV audio as base64 for JSON transport
    audio_b64 = base64.b64encode(tts_audio).decode("utf-8")

    # return NPC reply text alongside the synthesized audio
    return JSONResponse(content={
        "reply": llm_response["reply"],
        "audio": audio_b64,
    })
