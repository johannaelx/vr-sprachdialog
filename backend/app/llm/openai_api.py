import os
import json
from typing import Dict
from openai import OpenAI
from collections import deque

# OpenAI model used for conversational responses
API_MODEL_NAME = "gpt-4o-mini"

# API key is read from the environment
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

if not OPENAI_API_KEY:
    raise RuntimeError("OPENAI_API_KEY ist nicht gesetzt")

# OpenAI client instance
client = OpenAI(api_key=OPENAI_API_KEY)

NPC_MEMORY = deque(maxlen=6)

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
    You remember the recent conversation with the player and use it naturally.

    Language rule:
    Before responding, determine whether the player's input is written in English.

    If the input is NOT English:
    - Do NOT respond to the content.
    - Do NOT translate it.
    - Politely ask the player to repeat their sentence in English.
    - Stay in character as a baker.

    If the input IS English:
    - Respond naturally to the content.
    - If there is a small language mistake, correct it subtly inside the reply.
    - Do NOT explain grammar.
    - Do NOT sound like a teacher.

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
    # Build messages with memory
    messages = [
        {"role": "system", "content": system_prompt},
    ]

    # add previous conversation memory
    messages.extend(NPC_MEMORY)

    # add current user message
    messages.append(
        {"role": "user", "content": user_prompt}
    )

    response = client.chat.completions.create(
        model=API_MODEL_NAME,
        messages=messages,
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
        parsed = json.loads(raw_response)
        reply_text = parsed.get("reply", "")
    except json.JSONDecodeError:
        reply_text = raw_response
        parsed = {"reply": reply_text}

    # update NPC memory
    NPC_MEMORY.append({"role": "user", "content": user_text})
    NPC_MEMORY.append({"role": "assistant", "content": reply_text})

    return parsed

# TODO use this method to clear the NPC's memory after the level
def reset_npc_memory():
    NPC_MEMORY.clear()


# Test
# if __name__ == "__main__":
#   test_sentence = "Ein Belibiger Satz den man verarbeiten lassen möchte"
#   result = language_tutor(test_sentence)

#   print(result)
#   print(type(result))  # sollte <class 'dict'> sein
