﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/************************************************************************************************************\
 * HexGrid.cs by Daniel Carrier																				*
 * 																											*
 * This program gives coordinates to positions on a hex grid, checks which cell your cursor is on, and      *
 * outlines that cell. It uses basic mathematical operations on the position of the cursor to find the cell *
 * rather than checking each hexagon.                                                                       *
 *                                                                                                          *
 * I have also included a function to calculate the distance between two hexes.								*
 *																											*
\************************************************************************************************************/

public class Heex : MonoBehaviour {
	
	public GameObject marker;
	public GameObject unitPrefab1;
	public GameObject unitPrefab2;
	public GameObject obstacles;
	
	private List<Unit> units = new List<Unit>();		//I should make this thread safe.
	//public GameObject[] players;
	
	private bool waiting = false;
	//private int timeout = 0;		//waiting is still false as this counts down.
	//private const int MAX_TIME = 1000;
	
	private enum Turn {SELECT, MOVE, ATTACK};
	private Turn turn = Turn.SELECT;
	public int PLAYERS = 2;
	private int player = 0;
	private int updating = 0;
	private HexPosition mouse = null;
	private HexPosition selection = null;
	private HexPosition[] path = null;
	private AI ai;
	
	bool gameOver = false;
	bool modeSelected = true;
	bool computerPlayer = true;
	
	public void wait() {
		waiting = true;
	}
	
	//SendMessage must call this on the completion of an action.
	public void actionComplete() {
		waiting = false;
	}
	
	//Is this thread safe? I don't know how thread safety works.
	void AddUnit (Unit unit) {
		while(updating > 0) {
			//do nothing.
		}
		++updating;
		units.Add(unit);
		--updating;
		unit.Coordinates = new HexPosition (unit.transform.position);
	}
	
	public void remove (Unit unit) {
		units.Remove (unit);
	}
	
	//Returns true if there are any selectable units.
	private bool selectSelectable () {
		bool nonempty = false;
		foreach (Unit otherUnit in units) {
			if (otherUnit.PLAYER == player && otherUnit.Status != Unit.State.WAIT) {
				otherUnit.Coordinates.select ("Selectable");
				nonempty = true;
			}
		}
		return nonempty;
	}
	
	//TODO: Move to Unit.cs
	private bool isAttackable (Unit attacker, Unit attacked, HexPosition coordinates) {
		return attacked.PLAYER != player && coordinates.dist (attacked.Coordinates) <= attacker.RANGE;
	}
	private bool isAttackable (Unit attacker, Unit attacked) {
		return isAttackable (attacker, attacked, attacker.Coordinates);
	}
	
	//Returns true if there's at least one attackable unit.
	private bool selectAttackable (Unit attacker, HexPosition coordinates) {
		bool nonempty = false;
		foreach (Unit otherUnit in units) {
			if (isAttackable (attacker, otherUnit, coordinates)) {
				otherUnit.Coordinates.select ("Attack");
				nonempty = true;
			}
		}
		return nonempty;
	}
	
	//Returns true if there's at least one attackable unit.
	private bool selectAttackable (Unit attacker) {
		return selectAttackable (attacker, attacker.Coordinates);
	}
	
	private int _getNumUnits(int player) {
		int numUnits = 0;
		foreach (Unit otherUnit in units) {
			if (otherUnit.PLAYER == player) {
				numUnits++;
			}
		}
		return numUnits;
	}
	
	private void _spawnOwnUnits(GameObject parent) {
		for(int i = 0; i < PlayerControl.ownUnits; i++) {
			Vector3 spawnPoint = new Vector3((i-(PlayerControl.ownUnits/2))*1.5f, 0, -1 * Random.Range(3, 6));
			GameObject tmp = (GameObject)Instantiate(unitPrefab2, spawnPoint, Quaternion.identity);
			tmp.transform.parent = parent.transform;
			tmp.name = "ownUnit"+i;
		}
	}
	
	private void _spawnOtherUnits(GameObject parent) {
		int spawnUnits = PlayerControl.selectedEnemy.Units;
		for (int i = 0; i < spawnUnits; i++) {
			Vector3 spawnPoint = new Vector3((-i+(spawnUnits/2))*1.5f, 0, Random.Range(3, 6));
			GameObject tmp = (GameObject)Instantiate(unitPrefab1, spawnPoint, Quaternion.identity);
			tmp.transform.parent = parent.transform;
			tmp.name = "otherUnit" + i;
		}
	}
	
	void Start () {
		//Spawn the units
		GameObject unitsRoot = new GameObject();
		unitsRoot.name = "UnitsCreated";
		_spawnOwnUnits(unitsRoot);
		_spawnOtherUnits(unitsRoot);
		unitsRoot.BroadcastMessage ("SetGrid", this);
		
		Debug.Log("Loaded Scene by calling start");
		//timeout = MAX_TIME;
		HexPosition.setColor("Path", Color.yellow, 1);
		HexPosition.setColor("Selection", Color.green, 2);
		HexPosition.setColor("Selectable", Color.green, 3);
		HexPosition.setColor("Attack", Color.red, 4);
		HexPosition.setColor("Cursor", Color.blue, 5);
		HexPosition.Marker = marker;
		foreach (Transform child in obstacles.transform) {
			HexPosition position = new HexPosition(child.position);
			child.position = position.getPosition();
			position.flag("Obstacle");
		}
		selectSelectable();
		ai = new AI(units, 1);
		
	}
	
	private void select () {
		if (mouse.isSelected ("Selectable")) {
			HexPosition.clearSelection ("Selectable");
			selection = mouse;
			mouse.select ("Selection");
			Unit unit = (Unit) mouse.getValue ("Unit");
			selectAttackable (unit);
			switch (unit.Status) {
			case Unit.State.MOVE:
				turn = Turn.MOVE;
				break;
			case Unit.State.ATTACK:
				turn = Turn.ATTACK;
				break;
			default:
				print ("Error: Action " + ((Unit) mouse.getValue ("Unit")).Status + " not implemented.");
				break;
			}
		}
	}
	
	public void endTurn () {
		HexPosition.clearSelection ();
		foreach (Unit unit in units) {	//I only need to do this with units on that team, but checking won't speed things up. I could also only do it when player overflows.
			unit.newTurn ();
		}
		player = (player + 1) % PLAYERS;
		if(player == 0 || !computerPlayer) {
			selectSelectable ();
		}
	}
	
	private void unselect () {
		HexPosition.clearSelection ();
		selection = null;
		mouse.select ("Cursor");
		if (!selectSelectable ()) {
			endTurn ();
		}
		turn = Turn.SELECT;
	}
	
	private void checkGameOver() {
		gameOver = true;
		foreach (Unit unit in units) {
			if (unit.PLAYER != player) {
				gameOver = false;
				break;
			}
		}
		if (gameOver) {
			return;
		}
	}
	
	private void actuallyAttack () {
		((Unit) selection.getValue("Unit")).attack ((Unit) mouse.getValue("Unit"));
		checkGameOver ();
		unselect ();
	}
	
	private void move () {
		if (mouse.Equals (selection)) {
			unselect ();
		} else if (!mouse.containsKey ("Unit")) {
			if (path.Length > 0) {
				Unit myUnit = ((Unit) selection.getValue ("Unit"));
				myUnit.move (path);
				HexPosition.clearSelection ();
				selection = mouse;
				selection.select ("Selection");
				if (selectAttackable (myUnit)) {
					turn = Turn.ATTACK;
				} else {
					myUnit.Status = Unit.State.WAIT;
					unselect ();
				}
			}
		} else {
			object enemy = null;
			if (mouse.tryGetValue ("Unit", out enemy)) {
				Unit myUnit = ((Unit) selection.getValue ("Unit"));
				if (isAttackable (myUnit, (Unit) enemy)) {
					actuallyAttack ();
				}
			}
		}
	}
	
	private void attack () {
		if (mouse.isSelected ("Attack")) {
			actuallyAttack ();
		}
	}
	
	void Update () {
		if (waiting || gameOver || !modeSelected) {
			return;
		}
		if (player == 1 && computerPlayer) {
			if (ai.go()) {
				endTurn ();
			}
			checkGameOver ();
			return;
		}
		/*if (timeout > 0) {
			--timeout;
			if(timeout == 0) {
				print ("Warning: HexGrid.cs timed out.");
			}
			return;
		}*/
		if (!Input.mousePresent) {
			mouse = null;
		} else {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hits = Physics.RaycastAll(ray);
			if (hits.Length == 0) {
				if (mouse != null && turn == Turn.MOVE) {
					HexPosition.clearSelection ("Path");
					HexPosition.clearSelection ("Attack");
					path = null;
				}
				mouse = null;
			} else {
				float minDist = float.PositiveInfinity;
				int min = 0;
				for (int i=0; i<hits.Length; ++i) {
					if(hits[i].distance < minDist) {
						minDist = hits[i].distance;
						min = i;
					}
				}
				HexPosition newMouse = new HexPosition (hits[min].point);
				if (newMouse != mouse) {
					if (mouse != null) {
						mouse.unselect ("Cursor");
					}
					if (newMouse.containsKey ("Obstacle")) {	//The Obstacle tag is being used to make the tile unselectable.
						if (mouse != null && turn == Turn.MOVE) {
							HexPosition.clearSelection ("Path");
							HexPosition.clearSelection ("Attack");
							path = null;
						}
						mouse = null;
						return;
					}
					mouse = newMouse;
					mouse.select ("Cursor");
					if(turn == Turn.MOVE) {
						Unit unit = (Unit) selection.getValue ("Unit");
						HexPosition.clearSelection ("Path");
						HexPosition.clearSelection ("Attack");
						path = AStar.search (selection, mouse, unit.SPEED);
						HexPosition.select ("Path", path);
						selectAttackable (unit, mouse);
					}
				}
				if (Input.GetButtonDown ("Fire1")) {
					switch (turn) {
					case Turn.SELECT:
						select ();
						break;
					case Turn.MOVE:
						move ();
						break;
					case Turn.ATTACK:
						attack ();
						break;
					default:
						print ("Error: Turn " + turn + " not implemented.");
						break;
					}
					return;
				}
			}
		}
	}
	
	IEnumerator _returnToMainScene() {
		yield return new WaitForSeconds(3);
		PlayerControl.updateEnemyUnits(_getNumUnits(1));
		Application.LoadLevel("Terrain");
	}
	
	
	void OnGUI() {
		if (gameOver) {
			GUIStyle style = new GUIStyle();
			style.fontSize = 72;
			style.alignment = TextAnchor.MiddleCenter;
			if (_getNumUnits(1) == 0) {
				GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height - 20), "You Win!", style);
			} else {
				GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height - 20), "You Lose! :(", style);
			}
			PlayerControl.ownUnits = _getNumUnits(0);
			if (PlayerControl.ownUnits < 1) PlayerControl.ownUnits = 1;
			
			StartCoroutine(_returnToMainScene());
			return;
		}
		if (waiting || (player == 1 && computerPlayer)) {
			return;
		}
		int oldFontSize = GUI.skin.box.fontSize;
		GUI.skin.box.fontSize = 20;
		switch (turn) {
		case Turn.SELECT:
			GUI.Box(new Rect(5, 5, 100, 70), "Select");
			if (GUI.Button(new Rect(10, 40, 90, 20), "End Turn")) {
				endTurn();
			}
			break;
		case Turn.MOVE:
			GUI.Box(new Rect(5, 5, 100, 70), "Move");
			if (GUI.Button(new Rect(10, 40, 90, 20), "Cancel Move")) {
				unselect();
			}
			break;
		case Turn.ATTACK:
			GUI.Box(new Rect(5, 5, 100, 70), "Attack");
			if (GUI.Button(new Rect(10, 40, 90, 20), "Skip Attack")) {
				HexPosition.clearSelection();
				selection = null;
				if (mouse != null) {
					mouse.select("Cursor");
				}
				selectSelectable();
				turn = Turn.SELECT;
			}
			break;
		}
		GUI.skin.box.fontSize = oldFontSize;
	}
	/*
    void OnGUI () {
        if (!modeSelected) {
            if (GUI.Button (new Rect (10, 10, 90, 20), "1 Player")) {
                selectSelectable ();
                computerPlayer = true;
                modeSelected = true;
                ai = new AI(units, 1);
                return;
            }
            if (GUI.Button (new Rect (10, 40, 90, 20), "2 Player")) {
                selectSelectable ();
                computerPlayer = false;
                modeSelected = true;
                return;
            }
            return;
        }
        if (gameOver) {
            GUIStyle style = new GUIStyle ();
            style.fontSize = 72;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Box (new Rect (10, 10, Screen.width-20, Screen.height-20), "Player " + (player + 1) + " Wins!", style);
            StartCoroutine(_returnToMainScene());
            return;
        }
        if (waiting || (player == 1 && computerPlayer)) {
            return;
        }
        GUI.Box (new Rect (10, 10, 90, 20), "Player " + (player + 1));
        switch (turn) {
        case Turn.SELECT:
            GUI.Box (new Rect (10, 40, 90, 20), "Select");
            if (GUI.Button (new Rect (10, 70, 90, 20), "End Turn")) {
                endTurn ();
            }
            break;
        case Turn.MOVE:
            GUI.Box (new Rect (10, 40, 90, 20), "Move");
            if (GUI.Button (new Rect (10, 70, 90, 20), "Cancel Move")) {
                unselect ();
            }
            break;
        case Turn.ATTACK:
            GUI.Box (new Rect (10, 40, 90, 20), "Attack");
            if (GUI.Button (new Rect (10, 70, 90, 20), "Skip Attack")) {
                HexPosition.clearSelection ();
                selection = null;
                if (mouse != null) {
                    mouse.select ("Cursor");
                }
                selectSelectable ();
                turn = Turn.SELECT;
            }
            break;
        }
    }
    */
}