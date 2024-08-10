using UnityEngine;

public class sphereBehavior : MonoBehaviour
{
    public static int counter = 0;


    private string ID;
    private Vector3 loc;
    private float scale;
    public sphereBehavior createTag()
    {
        counter++;
        this.ID = "S:"+counter.ToString();
        return this;
        
        
    }
    public sphereBehavior setLoc(Vector3 vec)
    {
        this.loc=vec;
        return this;

    }

    public sphereBehavior setScale(float scale)
    {
        this.scale = scale;
        transform.localScale =new Vector3(scale, scale, scale);
        return this;
    }

    public void getstr() 
    { 
        string sts= "( ID="+this.ID+", Loc=<"+this.loc.x.ToString()+","+this.loc.y.ToString()+","+this.loc.z.ToString()+">, Scale="+this.scale.ToString()+" )";
        Debug.Log(sts);
    }

}
