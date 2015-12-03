/*
 * This is an example script that demonstrates how to do simple distance calculations
 * based on a starting Hex and a target Hex.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FX_Player : MonoBehaviour {
	public Camera PlayerCameraC;

	public Transform CurrentHex;
	public Transform TargetHex;

    public GameObject entity;
    public int entityMaxDistance = 3;
    public int entityNumber = 1;
    private GameObject[] playerEntities;

	public int MoveDistance;
	public Text DistanceText;

	// Use this for initialization
	void Start () {
		DistanceText = GameObject.Find ("Distance Text").GetComponent<Text>();

        playerEntities = new GameObject[20];
        for (int i = 0; i < entityNumber; i++)
        {
            playerEntities[i] = Instantiate(entity);
            playerEntities[i].transform.position = new Vector3(0,0);
        }
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = PlayerCameraC.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.collider.gameObject.tag != "Player")
            {
                if(TargetHex && TargetHex != CurrentHex){
                    if(hit.transform != TargetHex){
                        TargetHex.GetComponent<Renderer>().material.color = Color.white;
                    }
                    TargetHex = hit.transform;
                    TargetHex.GetComponent<Renderer>().material.color = Color.red;
                }

                if(hit.transform != CurrentHex){
                    TargetHex = hit.transform;
                }

                if(Input.GetMouseButtonDown(0)){
                    if(CurrentHex){
                        CurrentHex.GetComponent<Renderer>().material.color = Color.white;
                    }
                    CurrentHex = hit.transform;
                    CurrentHex.GetComponent<Renderer>().material.color = Color.green;
                }
            }

            if (CurrentHex && TargetHex)
            {
                CalculateDistance();
                printInformation();
            }
        }
	}

	void CalculateDistance() {
		Vector3 CurrentHexInfo = CurrentHex.GetComponent<FX_HexInfo>().HexPosition;
		Vector3 TargetHexInfo = TargetHex.GetComponent<FX_HexInfo>().HexPosition;

        MoveDistance = CalculateDistance(CurrentHexInfo, TargetHexInfo);
	}

    int CalculateDistance(Vector3 start, Vector3 end) {
        int dx = (int)Mathf.Abs(end.x - start.x);
        int dy = (int)Mathf.Abs(end.y - start.y);
        int dz = (int)Mathf.Abs(end.z - start.z);

        return (int)Mathf.Max(dx, dy, dz);
    }

    void printInformation(){
        Vector3 CurrentHexInfo = CurrentHex.GetComponent<FX_HexInfo>().HexPosition;
        Vector3 TargetHexInfo = TargetHex.GetComponent<FX_HexInfo>().HexPosition;

        DistanceText.text = "Current Hex : " + CurrentHexInfo.ToString() + "   Target Hex : " + TargetHexInfo.ToString() + "   Distance : " + MoveDistance.ToString();
    }
}
