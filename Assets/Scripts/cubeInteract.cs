using UnityEngine;

public class cubeInteract : MonoBehaviour, IInteractable
{
    public void Interact ()
    {
        Debug.Log(Random.Range(0,100));
    }
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
