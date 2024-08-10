using Unity.VisualScripting;
using UnityEngine;

public class cubeBehavior : MonoBehaviour
{
    public static int counter = 0;
    private string ID;
    private Vector3 loc;
    private string Name;
    private bool status; // Add status field

    public cubeBehavior createTag()
    {
        counter++;
        this.ID = "s:" + counter.ToString();
        return this;
    }

    public cubeBehavior setLoc(Vector3 vec)
    {
        this.loc = vec;
        return this;
    }

    public cubeBehavior setStatus(bool newStatus)
    {
        this.status = newStatus;
        Debug.Log($"Status of {gameObject.name} set to {status}");
        return this;
    }

    public bool getStatus()
    {
        return this.status;
    }

    public bool nameNotSet()
    {
        return this.Name is null;
    }

    public cubeBehavior setName(string name)
    {
        this.Name = name;
        return this;
    }

    public string getName()
    {
        return this.Name;
    }

    public void getstr()
    {
        string sts = "( ID=" + this.ID + ", Loc=<" + this.loc.x.ToString() + "," + this.loc.y.ToString() + "," + this.loc.z.ToString() + ">, status=" + this.status.ToString() + " )";
        Debug.Log(sts);
    }
}
