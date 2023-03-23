using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountWhite : MonoBehaviour {

    GameController gameController;
    Text text;

    void Start() {
      gameController = GameObject.Find("GameManager").GetComponent<GameController>();
      text = this.GetComponent<Text>();
    }

    void Update() {
      (int black, int white) count = gameController.CountStone();
      text.text = count.white + " 枚";
    }
}
