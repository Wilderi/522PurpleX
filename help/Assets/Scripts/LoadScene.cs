using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour {

    public string SceneName;

	public ArrayList Map;

    public void LoadStage()
    {
        Application.LoadLevel(SceneName);
    }
}
