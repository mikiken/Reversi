using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountBlack : MonoBehaviour {

    GameController gameController;
    Text text;

    void Start() {
      gameController = GameObject.Find("GameManager").GetComponent<GameController>();
      text = this.GetComponent<Text>();
    }

    void Update() {
      (int black, int white) count = gameController.CountStone();
      text.text = count.black + " 枚";
    }
}
