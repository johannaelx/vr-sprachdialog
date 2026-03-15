using UnityEngine;

public class VocabularyObject : MonoBehaviour
{
    public string germanWord;
    public string englishWord;

    public string GetDisplayText()
    {
        return germanWord + " = " + englishWord;
    }
}