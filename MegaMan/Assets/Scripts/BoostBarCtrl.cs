using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Image=UnityEngine.UI.Image;

public class BoostBarCtrl : MonoBehaviour {

	Image Boostbar;
	CharController CC;
	int lastBoostCount;
	// Use this for initialization
	void Start () {
		Boostbar = GameObject.Find ("Main Camera").transform.FindChild ("Canvas").FindChild ("Boostbar").GetComponent <Image> ();
		CC = GameObject.Find ("Player").GetComponent <CharController> ();
		lastBoostCount = CC.boostCountMax;
	}
	
	// Update is called once per frame
	void Update () {
		if (lastBoostCount != CC.boostCount) {
			Boostbar.fillAmount = (float)CC.boostCount / (float)CC.boostCountMax;
			lastBoostCount = CC.boostCount;
		}
	}
}
