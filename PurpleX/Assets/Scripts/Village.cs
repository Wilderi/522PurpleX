using UnityEngine;
using System;

public class Village : MonoBehaviour {
    public string title;
    private float units = 1;
    public bool conquered = false;
    public int workers = 20;

    void Start() {
        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        //GameMaker.Instance.enemysUnits = new int[3] { 1, 1, 1 };
        int counter = 0;
        //print(counter);
        foreach (GameObject enemyObj in enemyObjs) {
            if (enemyObj == gameObject) {
                Units = GameMaker.Instance.enemysUnits[counter];
                conquered = GameMaker.Instance.enemysConquered[counter];
            }
            counter++;
        }
        //print(counter);
    }

    public int Units {
        get {
            return (int)units;
        }
        set {
            units = units - (int)units + value; 
        }
    }

    void Update() {
        if (!conquered) {
            units += Time.deltaTime * UnityEngine.Random.Range(0, workers) * 0.001f * Mathf.Sqrt(GameMaker.Instance.workers);
        } else {
            PlayerControl.money += Time.deltaTime * UnityEngine.Random.Range(0, workers) * 0.1f * Mathf.Sqrt(GameMaker.Instance.workers);
        }
    }
}