import os
import json
from typing import Dict
from openai import OpenAI

# =====================================================
# Konfiguration des LLM
# =====================================================
USE_API = True  # False = lokales LLM, in unserem Fall durch die Hardware Beschränkungen keine Option
API_MODEL_NAME = (
    "gpt-4o-mini"  # Alternativ gpt-4 langsamer und teurer aber verlässlicher
)
LOCAL_MODEL_NAME = "mistralai/Mistral-7B-Instruct-v0.2"

OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")  # Key muss ergänzt werden

if USE_API and not OPENAI_API_KEY:
    raise RuntimeError("OPENAI_API_KEY ist nicht gesetzt")

client = OpenAI(api_key=OPENAI_API_KEY)


# =====================================================
# Die eigentliche LLM Funktion (liefert Text bzw einen Sring)
# =====================================================
def language_tutor_api(
    user_text: str, target_language: str = "English", learner_level: str = "A2"
) -> str:
    # Konfiguriert den Chat-Bot und gibt ihm eine "Identität"
    system_prompt = (
        f"You are a friendly language tutor for {target_language}. "
        f"The learner has language level {learner_level}. "
        "Your tasks:\n"
        "1. Check whether the sentence is grammatically correct.\n"
        "2. If there are mistakes, correct them gently.\n"
        "3. Explain mistakes briefly and clearly.\n"
        "4. Then respond naturally to the content.\n"
        "5. Stay motivating.\n"
        "6. Respond strictly in JSON format."
    )

    # Der Prompt des Nutzers, der die transkribierte Eingabe enthält
    user_prompt = f"""
        Spoken sentence:
        "{user_text}"

        Respond exactly in this JSON schema:
        {{
         "is_correct": true | false,
         "correction": "corrected sentence or empty",
         "explanation": "short explanation or empty",
         "reply": "natural conversational reply"
        }}
        """

    # Erschafft den Chat mit dem Chatbot
    response = client.chat.completions.create(
        model=API_MODEL_NAME,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        temperature=0.7,
    )

    # Rückgabe ist IMMER Text (JSON als String)
    return response.choices[0].message.content


# =====================================================
# Interface für die Nutzung in der Pipeline
# Parst JSON und kapselt API / lokales Modell
# =====================================================
def language_tutor(
    user_text: str, target_language: str = "English", learner_level: str = "A2"
) -> Dict:
    raw_response = language_tutor_api(user_text, target_language, learner_level)

    try:
        return json.loads(raw_response)
    except json.JSONDecodeError:
        # Für den Fall das kein JSON erzeugt werden kann
        return {
            "is_correct": False,
            "correction": "",
            "explanation": "Antwort konnte nicht korrekt verarbeitet werden.",
            "reply": raw_response,
        }


# =====================================================
# Test
# =====================================================
if __name__ == "__main__":
    test_sentence = "Ein Belibiger Satz den man verarbeiten lassen möchte"
    result = language_tutor(test_sentence)

    print(result)
    print(type(result))  # sollte <class 'dict'> sein
