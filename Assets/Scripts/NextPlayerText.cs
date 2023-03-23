using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextPlayerText : MonoBehaviour {

    GameController gameController;
    Text text;

    void Start() {
      gameController = GameObject.Find("GameManager").GetComponent<GameController>();
      text = this.GetComponent<Text>();
    }

    void Update() {
      string colorText = "";
      switch (gameController.player) {
        case GameController.Color.Black:
            colorText = "先手(黒)";
            break;
        case GameController.Color.White:
            colorText = "後手(白)";
            break;
        default:
            break;
      }
      text.text = colorText + "の番です";

      if (gameController.gameOver) {
        (int black, int white) finalCount = gameController.CountStone();
        if (finalCount.black > finalCount.white) {
          text.text = "先手(黒)が勝ちました";
        }
        else if (finalCount.black < finalCount.white) {
          text.text = "後手(白)が勝ちました";
        }
        else {
          text.text = "引き分けです";
        }
      }
    }

}
