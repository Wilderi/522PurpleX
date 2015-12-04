using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour {
    public Text hud;

    public static float money;
    int oldAmountMoney;    // To handle smooth HUD update
    int ownUnits;
    int workers;

    private int selectedEnemy = 0;
    private static Village selectedVillage = null;

    int priceWorker = 70;
    int priceUnit = 50; 

    public int speed = 50;

    // display overview windows
    bool showVillage = false;
    bool showOwn = false;

    Transform mousePos;

    // Use this for initialization
    void Start () {
        loadGameValues();
        updateHUD();
    }

    private void loadGameValues() {
        money = GameMaker.Instance.money;
        ownUnits = GameMaker.Instance.units;
        workers = GameMaker.Instance.workers;
        selectedEnemy = GameMaker.Instance.selectedEnemy;
    }

    private void saveGameValues() {
        GameMaker.Instance.money = money;
        GameMaker.Instance.units = ownUnits;
        GameMaker.Instance.workers = workers;
        GameMaker.Instance.selectedEnemy = selectedEnemy;

        GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        int counter = 0;
        foreach (GameObject enemyObj in enemyObjs) {
            Village village = enemyObj.GetComponent<Village>();
            GameMaker.Instance.enemysUnits[counter] = village.Units;
            GameMaker.Instance.enemysConquered[counter] = village.conquered;
            counter++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100)) {
            if (Input.GetMouseButtonDown(0)) {
                GameObject hitObj = hit.collider.gameObject;
                if(hitObj.tag == "Player") {
                    showOwn = true;
                    showVillage = false;
                    return;
                } else if (hitObj.tag == "Enemy") {
                    GameObject[] enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
                    int counter = 0;
                    foreach (GameObject enemyObj in enemyObjs) {
                        if (enemyObj == hitObj) break;
                        counter++;
                    }
                    selectedEnemy = counter;
                    selectedVillage = hitObj.GetComponent<Village>();
                    showOwn = false;
                    showVillage = true;
                }
            }
        }

        money += speed * Time.deltaTime * Mathf.Sqrt(workers) * 0.01f;
        if (oldAmountMoney != (int)money) {
            oldAmountMoney = (int)money;
            updateHUD();
        }
    }

    void OnGUI() {
        if (showVillage) {
            GUI.Box(new Rect(5, 5, 110, 90), selectedVillage.title + "\n" + "Units: "+ selectedVillage.Units);
            if (selectedVillage.conquered) GUI.enabled = false;
            if (GUI.Button(new Rect(10, 45, 100, 20), "Attack")) {
                saveGameValues();
				string[] zones = new string[4] { "HexGrid", "HexGrid2", "Hexy", "Hixe" };
				int random=UnityEngine.Random.Range(0,4);
				Application.LoadLevel(zones[random]);
            }
            GUI.enabled = true;
            if (GUI.Button(new Rect(10, 70, 100, 20), "Close")) {
                showVillage = false;
            }
        }
        if (showOwn) {              
            int left = Screen.width - 145;

            GUI.Box(new Rect(left, 5, 140, 100), "Own Village");
            
            // Worker button
            if (money < priceWorker) GUI.enabled = false;
            if (GUI.Button(new Rect(left + 5, 30, 130, 20), "Build Worker ("+priceWorker+")") && money >= priceWorker) {
                money -= priceWorker;
                oldAmountMoney -= priceWorker;
                workers++;
                if (workers % 5 == 0) priceWorker += workers;
                updateHUD();
            }
            // Unit button
            if (money < priceUnit) {
                GUI.enabled = false;
            } else {
                GUI.enabled = true;
            }
            if (GUI.Button(new Rect(left + 5, 55, 130, 20), "Build Unit ("+priceUnit+")") && money >= priceUnit) {
                money -= priceUnit;
                oldAmountMoney -= priceUnit;
                ownUnits++;
                updateHUD();
            }
            // Close button
            GUI.enabled = true;
            if (GUI.Button(new Rect(left + 5, 80, 130, 20), "Close")) {
                showOwn = false;
            }
        }
    }

    void updateHUD() {
        hud.text = "Money: " + oldAmountMoney + "  Units: " + ownUnits + "  Worker: " + workers;
    }

    internal static void updateEnemyUnits(int newAmount) {
        if (newAmount > 0) {
            selectedVillage.Units = newAmount;
        } else {
            selectedVillage.conquered = true;
        }
    }
}