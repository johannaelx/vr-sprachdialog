import whisper
import json

model = whisper.load_model("small")
result = model.transcribe("output2.mp3")
export = {
    "Task" : "",
    "Content" : result["text"]
}

with open('export.json','w') as file:
    json.dump(export, file, ensure_ascii=False, indent=2)
print(result["text"])
print("JSON data has been stored in 'export.json'")
