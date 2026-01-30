using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
