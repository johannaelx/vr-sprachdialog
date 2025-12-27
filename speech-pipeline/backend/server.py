from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import JSONResponse

from backend.asr_core import transcribe_wav_bytes

app = FastAPI(title="VR Speech Backend")

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
        text =  transcribe_wav_bytes(wav_bytes)
    except Exception as e:  
        raise HTTPException(
            status_code=500, 
            detail=f"ASR failed: {str(e)}"
        )
    
    return JSONResponse(
        content={"transcription": text}
    )