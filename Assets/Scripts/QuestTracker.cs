using UnityEngine;
using UnityEngine.UI;

public class QuestTracker : MonoBehaviour
{
    public Toggle task1;
    public Toggle task2;
    public Toggle task3;
    public Toggle task4;

    public void CompleteTask1()
    {
        task1.isOn = true;
    }

    public void CompleteTask2()
    {
        task2.isOn = true;
    }

    public void CompleteTask3()
    {
        task3.isOn = true;
    }

    public void CompleteTask4()
    {
        task4.isOn = true;
    }

}