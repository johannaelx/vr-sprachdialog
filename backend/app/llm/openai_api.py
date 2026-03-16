import os
import json
from typing import Dict
from pathlib import Path
from openai import OpenAI
from collections import deque

from dotenv import load_dotenv
load_dotenv()

# OpenAI chat model used for generating NPC responses
API_MODEL_NAME = "gpt-4o-mini"

# API key is read from the environment
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

if not OPENAI_API_KEY:
    raise RuntimeError("OPENAI_API_KEY ist nicht gesetzt")

# OpenAI client instance
client = OpenAI(api_key=OPENAI_API_KEY)

# short-term dialogue memory
NPC_MEMORY = deque(maxlen=6)

# directory containing prompt files
PROMPT_DIR = Path(__file__).parent / "prompts"

# prompt cache to avoid disk reads every request
PROMPT_CACHE = {}


def load_prompt(npc_type: str) -> str:
    """
    Loads the system prompt for the given NPC type.

    Prompts are cached after the first load to avoid repeated disk access.
    If the requested NPC type does not exist, the default prompt is used.
    """

    prompt_file = PROMPT_DIR / f"{npc_type}.txt"

    if not prompt_file.exists():
        npc_type = "default"
        prompt_file = PROMPT_DIR / "default.txt"

    if npc_type in PROMPT_CACHE:
        return PROMPT_CACHE[npc_type]

    with open(prompt_file, "r", encoding="utf-8") as f:
        prompt = f.read()

    PROMPT_CACHE[npc_type] = prompt
    return prompt


def npc_api(user_text: str, npc_type: str) -> str:
    """
    Sends the user's utterance to the LLM and returns a JSON-formatted response.
    The NPC personality is determined by npc_type.
    """

    system_prompt = load_prompt(npc_type)

    user_prompt = f"""
    Player said:
    "{user_text}"

    Respond in JSON only.
    """

    messages = [
        {"role": "system", "content": system_prompt},
    ]

    # add recent dialogue turns
    messages.extend(NPC_MEMORY)

    messages.append(
        {"role": "user", "content": user_prompt}
    )

    response = client.chat.completions.create(
        model=API_MODEL_NAME,
        messages=messages,
        temperature=0.6,
    )

    return response.choices[0].message.content


def npc_chat(user_text: str, npc_type: str) -> Dict:
    """
    High-level wrapper used by the backend conversation pipeline.
    Parses the JSON reply from the LLM.
    """

    raw_response = npc_api(user_text, npc_type)

    try:
        parsed = json.loads(raw_response)
        reply_text = parsed.get("reply", "")
    except json.JSONDecodeError:
        reply_text = raw_response
        parsed = {"reply": reply_text}

    # update memory
    NPC_MEMORY.append({"role": "user", "content": user_text})
    NPC_MEMORY.append({"role": "assistant", "content": reply_text})

    return parsed


def reset_npc_memory():
    """
    Clears conversation memory.
    Should be called when a new scene starts.
    """
    NPC_MEMORY.clear()
