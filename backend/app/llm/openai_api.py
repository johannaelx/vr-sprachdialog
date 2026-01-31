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


def baker_npc_api(user_text: str) -> str:
    """
    Sends the user's utterance to the LLM and returns a JSON-formatted response as text.

    The model acts as a language tutor: it checks correctness, optionally corrects
    mistakes, explains them briefly, and provides a natural conversational reply.
    """
    system_prompt = """
    You are an in-world NPC in a VR game.

    Role:
    You are a baker who works in a small bakery.
    You speak as a real person inside the game world, not as an AI.

    Conversation state:
    Assume the conversation is already ongoing.
    Do NOT greet the player unless the player greets you first.
    Do NOT introduce yourself.
    Do NOT say hello, hi, welcome, or similar phrases.

    Behavior:
    Respond naturally and concisely, as a baker would.
    Keep responses short (1–3 sentences).
    Stay in character at all times.

    Speech rules:
    Use simple spoken language.
    Always speak English.
    Never mention being an AI or assistant.
    Never explain grammar or rules.

    Output format:
    Respond strictly in JSON.

    If the player makes a language mistake:
    Correct it subtly inside the reply, without explaining grammar rules.
    Do not sound like a teacher.

    JSON schema:
    {
    "reply": "the baker's spoken reply"
    }
    """

    user_prompt = f"""
    Player said:
    "{user_text}"

    Respond in JSON only.
    """

    response = client.chat.completions.create(
        model=API_MODEL_NAME,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        temperature=0.6,
    )

    # the API always returns text; JSON parsing is handled by the wrapper function
    return response.choices[0].message.content


def baker_npc(user_text: str) -> Dict:
    """
    High-level wrapper for the language tutor used in the speech pipeline.

    This function calls the LLM API and parses the returned JSON string into
    a Python dictionary. If parsing fails, a fallback response is returned.
    """
    raw_response = baker_npc_api(user_text)

    try:
        return json.loads(raw_response)
    except json.JSONDecodeError:
        return {
            "reply": raw_response
        }


# Test
# if __name__ == "__main__":
#   test_sentence = "Ein Belibiger Satz den man verarbeiten lassen möchte"
#   result = language_tutor(test_sentence)

#   print(result)
#   print(type(result))  # sollte <class 'dict'> sein
