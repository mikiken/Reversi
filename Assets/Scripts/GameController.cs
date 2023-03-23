using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [SerializeField]
    GameObject blackObject = null;
    [SerializeField]
    GameObject emptyObject = null;
    [SerializeField]
    GameObject whiteObject = null;
    [SerializeField]
    GameObject boardDisplay = null;

    NextPlayerText nextPlayerText;

    AiPlayer aiPlayer;

    //盤面の大きさを定義(定数)
    const int HEIGHT = 8;
    const int WIDTH = 8;

    //盤面の色を定義
    public enum Color {Black = -1, Empty, White}

    //盤面を管理するため8by8の二次元配列を用意(C#では配列の初期値は0)
    public Color[,] board = new Color[HEIGHT, WIDTH];

    //手番を持っておく変数(公式ルールでは先手は黒なので、黒で初期化)
    public Color player = Color.Black;

    //終局判定用フラグ
    public bool gameOver = false;

    //盤面の初期化関数
    public void Initialize() {
      //盤面初期化
      board = new Color[HEIGHT, WIDTH];
      board[3, 3] = Color.White;
      board[3, 4] = Color.Black;
      board[4 ,3] = Color.Black;
      board[4, 4] = Color.White;
      player = Color.Black; //プレイヤー初期化
      //NextPlayerTextのインスタンスを生成
      nextPlayerText = GameObject.Find("nextPlayer").GetComponent<NextPlayerText>();
      //終局判定初期化
      gameOver = false;
      //AiPlayerクラスのインスタンスを生成
      aiPlayer = GameObject.Find("GameManager").GetComponent<AiPlayer>();
      ShowBoard();
    }

    //二次元配列の値ごとに対応するprefabを返す関数
    GameObject GetPrefab(Color color) {
      GameObject prefab;
      switch(color) {
        case Color.Black:
          prefab = Instantiate(blackObject);
          break;
        case Color.Empty:
          prefab = Instantiate(emptyObject);
          break;
        case Color.White:
          prefab = Instantiate(whiteObject);
          break;
        default:
          prefab = null;
          break;
      }
      return prefab;
    }

    //盤面を表示する関数
    void ShowBoard() {
      //boardDisplayの全ての子オブジェクトを削除
      foreach(Transform child in boardDisplay.transform) {
        Destroy(child.gameObject);
      }
      //描画
      for (int v = 0; v < HEIGHT; v++) { //vertical
        for (int h = 0; h < WIDTH; h++) { //horizontal
          GameObject piece = GetPrefab(board[v, h]); //GameObject型の変数pieceを宣言し、(v,h)に対応するprefabを代入
          //盤面がemptyのときのみコマを置く
          if (board[v, h] == Color.Empty) {
            int y = v;
            int x = h;
            piece.GetComponent<Button>().onClick.AddListener(() => { PutStone(y + "," + x); }); //動的に生成するpiece(Button)のひとつひとつにクリックイベントを予め(属性として)付加しておく
          }
          piece.transform.SetParent(boardDisplay.transform); //取得したPrefabをboardDisplayの子オブジェクトにする
        }
      }
    }

    //(v,h)成分にコマを置く関数
    public void PutStone(string position) {
      //引数をstringで受け取ったのでカンマ前後で分けてintに直す
      int v = int.Parse(position.Split(',')[0]); //positionをカンマでsplitして配列に変換し、その配列の0番目成分を読んでintにparseする
      int h = int.Parse(position.Split(',')[1]);
      ReverseAll(v, h); //実際にひっくり返す
      ShowBoard(); //画面を描画
      if (board[v, h] == player) { //(v,h)にコマが置けた場合は手番を反転させる emptyの場合は何もしない
        player = (player == Color.Black) ? Color.White : Color.Black; //手番を渡す 三項演算子 "変数名	=	条件式	?	trueの場合の値	:	falseの場合の値"
      }
      if (CheckPass()) {
        player = (player == Color.Black) ? Color.White : Color.Black;
        if (CheckPass()) {
          //終局時の処理
          gameOver = true;
          (int black, int white) count = CountStone();
          Debug.Log(count.black + "," + count.white);
        }
      }
      aiPutStone();
    }

    void aiPutStone() {
      /*
      if (player == Color.Black) {
        if (!gameOver) {
          aiPlayer.randomPut(Color.Black);
        }
      }
      */

      if (player == Color.White) {
        if (!gameOver) {
          aiPlayer.alphaBetaPutWhite();
        }
      }

    }

    //一方向にコマをひっくり返す関数
    void Reverse(int v, int h, int directionV, int directionH) {
      //調べる配列の位置を計算
      int y = v + directionV;
      int x = h + directionH;

      while (y >= 0 && y < HEIGHT && x >= 0 && x < WIDTH) { //配列の外を参照していないか判定 とにかく自分と同じ色のマスが登場するまで探索する
        if (board[y, x] == player) { //directionV,directionHを加えているため、(v,h)の1つ隣のマスから調べ始めている
          int reversingV = v + directionV, reversingH = h + directionH;
          int reversedCount = 0;
          while (!(reversingV == y && reversingH == x)) {
            board[reversingV, reversingH] = player; //実際にひっくり返す
            reversingV += directionV;
            reversingH += directionH;
            reversedCount++;
          }
          //同色が隣でなければコマを置く
          if (reversedCount > 0) {
            board[v, h] = player;
          }
          break;
        }
        else if (board[y, x] == Color.Empty) { //空きマスの場合
          break;
        }
        y += directionV;
        x += directionH;
      }
    }

    //全方向にひっくり返す関数
    public void ReverseAll(int v, int h) {
      Reverse(v, h, -1, -1); //左上
      Reverse(v, h, -1, 0); //上
      Reverse(v, h, -1, 1); //右上
      Reverse(v, h, 0,-1); //左
      Reverse(v, h, 0, 1); //右
      Reverse(v, h, 1, -1); //左下
      Reverse(v, h, 1, 0); //下
      Reverse(v, h, 1, 1); //右下
    }

    public bool CheckPass(){
      for (int v = 0; v < HEIGHT; v++) {
        for (int h = 0; h < WIDTH; h++) {
          if (board[v, h] == Color.Empty) {
            Color[,] boardTemp = new Color[HEIGHT, WIDTH]; //boardの状態保持用に配列を宣言
            Array.Copy(board, boardTemp, board.Length); //boardをboardTempにコピー
            ReverseAll(v, h);
            if (board[v, h] == player) {
              board = boardTemp; //boardを元に戻す
              return false;
            }
          }
        }
      }
      return true;
    }

    //盤面のコマの数を数える関数
    public (int black, int white) CountStone() {
      int countBlack = 0;
      int countWhite = 0;
      for (int v = 0; v < HEIGHT; v++) {
        for (int h = 0; h < WIDTH; h++) {
          switch(board[v, h]) {
            case Color.Black:
              countBlack++;
              break;
            case Color.White:
              countWhite++;
              break;
            default:
              break;
          }
        }
      }
      return (countBlack, countWhite);
    }

    //最初の1回だけ実行される
    void Start() {
      Initialize();
    }

}
