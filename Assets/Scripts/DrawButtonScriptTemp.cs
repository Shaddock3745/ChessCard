using UnityEngine;

public class DrawButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
        for(int i=0;i<5;i++)
        MatchDataManager.drawCard(true);
    }
}
