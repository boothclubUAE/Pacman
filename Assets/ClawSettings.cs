using UnityEngine;

[HasTabField]
public class ClawSettings: MonoBehaviour
{
    [TabField]
    public string machineName = "Test";

    [TabField]
    public int coins = 10;

    [TabField]
    public float speed = 1;

    [TabField]
    public bool enabled = true;


    [TabButton]
    public void Restart()
    {
        Debug.Log("Restart");
    }
}