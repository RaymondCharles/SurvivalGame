using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading
{

    public static string sceneLoad;

    public static void Load(string targetScene)
    {
        sceneLoad = targetScene;
        SceneManager.LoadScene("LoadingScene");

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
