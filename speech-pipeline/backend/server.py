from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import JSONResponse

from backend.asr_core import transcribe_wav_bytes
from backend.llm import language_tutor
from backend.piper import speaker

app = FastAPI(title="VR Speech Backend")

# =====================================================
# Audio Pipeline mit Verbindung zum HTTP Endpoint
# =====================================================
@app.post("/conversation")
async def conversation(audio: UploadFile = File(...)):
    if audio.content_type not in ("audio/wav", "audio/x-wav"):
        raise HTTPException (
            status_code=400, 
            detail="Invalid audio format. Only WAV files are supported."
        )
    
    wav_bytes = await audio.read()

    if len(wav_bytes) == 0:
        raise HTTPException(
            status_code=400, 
            detail="Empty audio file."
        )
    
    try:
        #ASR
        transcription: str = transcribe_wav_bytes(wav_bytes)

        #LLM
        llm_response: dict = language_tutor(transcription)
        # Erwartete Struktur
        # {
        #   "is_correct": bool,
        #   "correction": str,
        #   "explanation": str,
        #   "reply": str
        # }
        # Wird so vom LLM zurück gegeben 

        #TTS
        audio_path: str = speaker(llm_response["reply"])

    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Conversation pipeline failed: {str(e)}"
        )
    # =====================================================
    #JSON-mit sämtlichen Informationen aus der Pipeline 
    # =====================================================
    return JSONResponse(
        content={
            "transcription": transcription,
            "feedback": {
                "is_correct": llm_response.get("is_correct"),
                "correction": llm_response.get("correction"),
                "explanation": llm_response.get("explanation"),
                "reply": llm_response.get("reply"),
            },
            "audio_path": audio_path
        }
    )