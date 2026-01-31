import os
import json
from typing import Dict
from openai import OpenAI

# OpenAI model used for conversational responses
API_MODEL_NAME = "gpt-4o-mini"

# API key is read from the environment
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

if not OPENAI_API_KEY:
    raise RuntimeError("OPENAI_API_KEY ist nicht gesetzt")

# OpenAI client instance
client = OpenAI(api_key=OPENAI_API_KEY)


def language_tutor_api(
    user_text: str, target_language: str = "English", learner_level: str = "A2"
) -> str:
    """
    Sends the user's utterance to the LLM and returns a JSON-formatted response as text.

    The model acts as a language tutor: it checks correctness, optionally corrects
    mistakes, explains them briefly, and provides a natural conversational reply.
    """
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

    response = client.chat.completions.create(
        model=API_MODEL_NAME,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        temperature=0.7,
    )

    # the API always returns text; JSON parsing is handled by the wrapper function
    return response.choices[0].message.content


def language_tutor(
    user_text: str, target_language: str = "English", learner_level: str = "A2"
) -> Dict:
    """
    High-level wrapper for the language tutor used in the speech pipeline.

    This function calls the LLM API and parses the returned JSON string into
    a Python dictionary. If parsing fails, a fallback response is returned.
    """
    raw_response = language_tutor_api(user_text, target_language, learner_level)

    try:
        return json.loads(raw_response)
    except json.JSONDecodeError:
        return {
            "is_correct": False,
            "correction": "",
            "explanation": "The response could not be parsed correctly.",
            "reply": raw_response,
        }


# Test
# if __name__ == "__main__":
#   test_sentence = "Ein Belibiger Satz den man verarbeiten lassen möchte"
#   result = language_tutor(test_sentence)

#   print(result)
#   print(type(result))  # sollte <class 'dict'> sein
