using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour {

    public string SceneName;

    public void LoadStage()
    {
        Application.LoadLevel(SceneName);
    }
}
