using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour {
    public Text hud;

    private static float money = 100;
    int oldAmountMoney = 100;    // To handle smooth HUD updates

    public static int ownUnits = 1; // static to use in combat script
    public static int workers = 1;

    public static int p1Units = 1;
    private static float p1UnitsFloat = 1;

    int priceWorker = 70;
    int priceUnit = 50; 

    float speed = 0.5f;

    bool showVillage = false;
    string villageName;

    bool showOwn = false;

    Transform mousePos;

    // Use this for initialization
    void Start () {
        updateHUD();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.collider.gameObject.tag == "Village1")
                {
                    villageName = "Warlord";
                    showVillage = true;
                    showOwn = false;
                }
                if (hit.collider.gameObject.tag == "Player") {
                    showOwn = true;
                    showVillage = false;
                }
            }

        }

        money += speed * Time.deltaTime * Mathf.Sqrt(workers);
        if (oldAmountMoney != (int)money) {
            oldAmountMoney = (int)money;
            updateHUD();
        }

        p1UnitsFloat += Time.deltaTime * Random.Range(0, 20) * 0.001f * Mathf.Sqrt(workers);
        if(p1Units != (int) p1UnitsFloat) {
            p1Units = (int)p1UnitsFloat;
        }
    }

    void OnGUI() {
        if (showVillage) {
            GUI.Box(new Rect(5, 5, 110, 90), villageName + "\n" + "Units: "+p1Units);
            if (GUI.Button(new Rect(10, 45, 100, 20), "Attack")) {
                showVillage = false;
                Application.LoadLevel("HexGrid");
            }
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
}
