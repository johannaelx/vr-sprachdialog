import os
import json
from typing import Dict


# =====================================================
# Konfiguration des LLM
# =====================================================
USE_API = True  # False = lokales LLM, in unserem Fall durch die Hardware Beschränkungen keine Option
API_MODEL_NAME = "gpt-3.5-turbo"  # Alternativ gpt-4 langsamer und teurer aber verlässlicher
LOCAL_MODEL_NAME = "mistralai/Mistral-7B-Instruct-v0.2"

OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")  # Key muss ergänzt werden

if USE_API and not OPENAI_API_KEY:
    raise RuntimeError("OPENAI_API_KEY ist nicht gesetzt")


# =====================================================
# Die eigentliche LLM Funktion (liefert Text bzw einen Sring)
# =====================================================
def language_tutor_api(
    user_text: str,
    target_language: str = "Deutsch",
    learner_level: str = "A2"
) -> str:
    import openai
    openai.api_key = OPENAI_API_KEY

    # Konfiguriert den Chat-Bot und gibt ihm eine "Identität"
    system_prompt = (
        f"Du bist ein freundlicher Sprachtrainer für {target_language}. "
        f"Der Lernende hat das Sprachniveau {learner_level}. "
        "Deine Aufgaben:\n"
        "1. Prüfe, ob der Satz sprachlich korrekt ist.\n"
        "2. Wenn es Fehler gibt, korrigiere sie behutsam.\n"
        "3. Erkläre Fehler kurz und verständlich.\n"
        "4. Antworte anschließend natürlich auf den Inhalt.\n"
        "5. Bleibe motivierend.\n"
        "6. Antworte ausschließlich im JSON-Format."
    )

    # Der Prompt des Nutzers, der die transkribierte Eingabe enthält
    user_prompt = f"""
Gesprochener Satz:
"{user_text}"

Antworte exakt in diesem JSON-Schema:
{{
  "is_correct": true | false,
  "correction": "korrigierter Satz oder leer",
  "explanation": "kurze Erklärung oder leer",
  "reply": "natürliche Antwort im Dialog"
}}
"""

    # Erschafft den Chat mit dem Chatbot
    response = openai.ChatCompletion.create(
        model=API_MODEL_NAME,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt}
        ],
        temperature=0.7  # Kreativität der Antwort, geht von 0.0 bis 1.0
    )

    # Rückgabe ist IMMER Text (JSON als String)
    return response.choices[0].message["content"]


# =====================================================
# Interface für die Nutzung in der Pipeline
# Parst JSON und kapselt API / lokales Modell
# =====================================================
def language_tutor(
    user_text: str,
    target_language: str = "Deutsch",
    learner_level: str = "A2"
) -> Dict:
   
    raw_response = language_tutor_api(
        user_text,
        target_language,
        learner_level
    )

    try:
        return json.loads(raw_response)
    except json.JSONDecodeError:
        # Für den Fall das kein JSON erzeugt werden kann
        return {
            "is_correct": False,
            "correction": "",
            "explanation": "Antwort konnte nicht korrekt verarbeitet werden.",
            "reply": raw_response
        }


# =====================================================
# Test
# =====================================================
if __name__ == "__main__":
    test_sentence = "Ein Belibiger Satz den man verarbeiten lassen möchte"
    result = language_tutor(test_sentence)

    print(result)
    print(type(result))  # sollte <class 'dict'> sein