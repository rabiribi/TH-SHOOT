using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPInput : MonoBehaviour {
	void Start () {
	}
	void Update () {
	}
	public void IPUpdate() {
		NetWorkScript.Serverip = this.GetComponent<InputField>().text;
		Debug.Log(NetWorkScript.Serverip);
	}
}
