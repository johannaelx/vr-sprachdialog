using UnityEngine;

public class doorInteract : MonoBehaviour, IInteractable
{
    public void Interact ()
    {
        Debug.Log("pressed E");
        if (isOpen)
        {
            doorHinge.GetComponent<Animator>().Play("closeDoor");
            isOpen = false;
        }
        else
        {
            doorHinge.GetComponent<Animator>().Play("openDoor");
            isOpen = true;
        }
    }
    [SerializeField] bool isOpen;
    [SerializeField] GameObject doorHinge;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
