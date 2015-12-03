using UnityEngine;
using System.Collections;

public class GameMaker : MonoBehaviour {
    public static GameMaker Instance;

    public int speed = 50;

    public int units = 2;
    public int workers = 1;
    public float money = 50;

    public int[] enemysUnits = new int[] { 1, 1, 1 };
    public bool[] enemysConquered = new bool[3];

    public int selectedEnemy;

    void Awake() {
        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }
}
