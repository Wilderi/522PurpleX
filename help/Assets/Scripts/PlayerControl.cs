using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour {
    public Text hud;

    public static float money = 100;
    int oldAmountMoney = 100;    // To handle smooth HUD updates

    public static int ownUnits = 1; // static to use in combat script
    public static int workers = 4;

    public static Village selectedEnemy = null;

    int priceWorker = 70;
    int priceUnit = 50; 
	
	//public AudioSource au_clash;
	//public AudioSource au_ching;
		


    public int speed = 50;

    // display overview windows
    bool showVillage = false;
    bool showOwn = false;

    Transform mousePos;

    // Use this for initialization
    void Start () {
        updateHUD();
		//au_clash = (AudioSource)gameObject.AddComponent<"AudioSource">;
		//AudioClip clash;
		//clash = (AudioClip)Resources.Load ("clash");
		//au_ching = (AudioSource)gameObject.AddComponent<"AudioSource">;
		//AudioClip ching;
		//ching = (AudioClip)Resources.Load ("ching");
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
                    selectedEnemy = hitObj.GetComponent<Village>();
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
            GUI.Box(new Rect(5, 5, 110, 90), selectedEnemy.title + "\n" + "Units: "+ selectedEnemy.Units);
            if (selectedEnemy.Conquered) GUI.enabled = false;
            if (GUI.Button(new Rect(10, 45, 100, 20), "Attack")) {
                showVillage = false;
				string[] zones = new string[4] { "HexGrid", "HexGrid2", "Hexy", "Hixe" };
				int random = Random.Range(0, 4);
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
            selectedEnemy.Units = newAmount;
        } else {
            selectedEnemy.Conquered = true;
        }
        /*
        int diff = selectedEnemyUnits - newAmount;
        if (diff + 1 >= selectedEnemyUnits) diff = selectedEnemyUnits - 1;
        print(diff + " " + selectedEnemyUnits + " " + newAmount + " " + p3UnitsFloat);
        switch (selectedEnemy) {
            case 1:
                p1UnitsFloat -= diff ;
                break;
            case 2:
                p2UnitsFloat -= diff;
                break;
            case 3:
                p3UnitsFloat -= diff;
                break;
        }
        */
    }
}
