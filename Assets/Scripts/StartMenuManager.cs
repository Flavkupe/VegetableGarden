using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour 
{

    static StartMenuManager instance;
    public static StartMenuManager Instance 
    { 
        get { return instance; } 
    }

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () 
    {        
        SoundManager.Instance.PlayMusic(MusicChoice.Menu);
	}
	
	// Update is called once per frame
	void Update () 
    {	
	}

    public void StartGame()
    {        
        //Scene scene = SceneManager.GetSceneByName("Main");        
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}
