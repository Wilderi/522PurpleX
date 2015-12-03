using UnityEngine;
using System.Collections;

public class Village : MonoBehaviour {
    public string title;
    private static float units = 1;
    private static bool conquered = false;
    public int workers = 20;

    public int Units {
        get {
            return (int)units;
        }
        set {
            units = units - (int)units + value; 
        }
    }

    public bool Conquered {
        get {
            return conquered;
        }
        set {
            conquered = value;
        }
    }

    void Update() {
        if (!conquered) {
            units += Time.deltaTime * Random.Range(0, workers) * 0.001f * Mathf.Sqrt(PlayerControl.workers);
        } else {
            PlayerControl.money += Time.deltaTime * Random.Range(0, workers) * 0.1f * Mathf.Sqrt(PlayerControl.workers);
        }
    }
}