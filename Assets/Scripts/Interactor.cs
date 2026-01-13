using UnityEngine;

public class doorScript : MonoBehaviour
{
    [SerializeField] GameObject doorHinge;
    [SerializeField] bool doorIsOpen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        doorIsOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        void OpenDoor()
        {
            doorHinge.GetComponent<Animator>().Play("openDoor");
        }
    }
}
