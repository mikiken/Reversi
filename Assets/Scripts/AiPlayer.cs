using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiPlayer : MonoBehaviour {
    GameController gameController;

    void Start() {
      gameController = GameObject.Find("GameManager").GetComponent<GameController>();
    }

    public void randomPut(GameController.Color color){
        //可能な手を全て生成
        (string[] positionCandidates, int possiblePosition) candidates = searchPossiblePosition(color);
        var random = new System.Random();
        gameController.PutStone(candidates.positionCandidates[random.Next(candidates.possiblePosition)]);//ランダムに選んで手を打つ random.Next(M)で0以上Mより小さい範囲で乱数を生成する
    }

    public void alphaBetaPutWhite() {
      string bestPosition = "";
      int eval, evalMax = -200; //評価値の絶対値は184以下
      //可能な手を全て生成
      (string[] positionCandidates, int possiblePosition) candidates = searchPossiblePosition(GameController.Color.White);

      for (int i = 0; i < candidates.possiblePosition; i++) {
        int v = int.Parse(candidates.positionCandidates[i].Split(',')[0]);
        int h = int.Parse(candidates.positionCandidates[i].Split(',')[1]);
        GameController.Color[,] boardTemp = new GameController.Color[8, 8];
        Array.Copy(gameController.board, boardTemp, gameController.board.Length);
        gameController.ReverseAll(v, h);
        eval = alphaBetaMin(-200, 200, 7);
        gameController.board = boardTemp; //盤面を元に戻す
        if (eval > evalMax) {
          bestPosition = candidates.positionCandidates[i];
        }
      }
      gameController.PutStone(bestPosition); //生き残った最有力候補を打つ
    }

    int alphaBetaMax(int alpha, int beta, int limit) {
      //探索したノードが末端であるか深さ制限0ならそのノードの評価値を返す
      if((countPossiblePosition(gameController.board, GameController.Color.White) == 0 && countPossiblePosition(gameController.board, GameController.Color.Black) == 0) || limit <= 0) {
        return calcBoardRating(gameController.board, GameController.Color.White);
      }
      //可能な手を全て生成
      (string[] positionCandidates, int possiblePosition) candidates = searchPossiblePosition(GameController.Color.White);

      int score, scoreMax = 0;

      for (int i = 0; i < candidates.possiblePosition; i++) {
        int v = int.Parse(candidates.positionCandidates[i].Split(',')[0]);
        int h = int.Parse(candidates.positionCandidates[i].Split(',')[1]);
        GameController.Color[,] boardTemp = new GameController.Color[8, 8];
        Array.Copy(gameController.board, boardTemp, gameController.board.Length);
        gameController.ReverseAll(v, h);
        bool passFlag = false;
        if (gameController.CheckPass()) {
          passFlag = true;
        } else {
          gameController.player = (GameController.Color)((int)gameController.player * (-1));
        }
        score = alphaBetaMin(alpha, beta, limit - 1);
        gameController.board = boardTemp; //手を戻す
        if (!passFlag) {
          gameController.player = (GameController.Color)((int)gameController.player * (-1));
        }
        if (score >= beta) {
          return score;
        }

        if (score > scoreMax) {
          scoreMax = score;
          alpha = Math.Max(alpha, scoreMax);
        }
      }

      return scoreMax;
    }

    int alphaBetaMin(int alpha, int beta, int limit) {
      //探索したノードが末端であるか深さ制限0ならそのノードの評価値を返す
      if((countPossiblePosition(gameController.board, GameController.Color.White) == 0 && countPossiblePosition(gameController.board, GameController.Color.Black) == 0) || limit <= 0) {
        return calcBoardRating(gameController.board, GameController.Color.White);
      }

      //可能な手を全て生成
      (string[] positionCandidates, int possiblePosition) candidates = searchPossiblePosition(GameController.Color.White);

      int score, scoreMin = 0;

      for (int i = 0; i < candidates.possiblePosition; i++) {
        int v = int.Parse(candidates.positionCandidates[i].Split(',')[0]);
        int h = int.Parse(candidates.positionCandidates[i].Split(',')[1]);
        GameController.Color[,] boardTemp = new GameController.Color[8, 8];
        Array.Copy(gameController.board, boardTemp, gameController.board.Length);
        gameController.ReverseAll(v, h);
        bool passFlag = false;
        if (gameController.CheckPass()) {
          passFlag = true;
        } else {
          gameController.player = (GameController.Color)((int)gameController.player * (-1));
        }
        score = alphaBetaMax(alpha, beta, limit - 1);
        gameController.board = boardTemp; //手を戻す
        if (!passFlag) {
          gameController.player = (GameController.Color)((int)gameController.player * (-1));
        }
        if (score <= alpha) {
          return score;
        }

        if (score < scoreMin) {
          scoreMin = score;
          beta = Math.Max(beta, scoreMin);
        }
      }

      return scoreMin;
    }

    (string[] positionCandidates, int possiblePosition) searchPossiblePosition(GameController.Color color) {
      string[] positionCandidates = new string[64];
      int possiblePosition = 0;
      for (int v = 0; v < 8; v++) {
        for (int h = 0; h < 8; h++) {
          if (gameController.board[v, h] == GameController.Color.Empty) {
            GameController.Color[,] boardTemp = new GameController.Color[8, 8];
            Array.Copy(gameController.board, boardTemp, gameController.board.Length);
            gameController.ReverseAll(v, h);
            if (gameController.board[v, h] == color) {
              gameController.board = boardTemp;
              positionCandidates[possiblePosition] = v + "," + h;
              possiblePosition++; //最終的にデータが入っている配列のインデックスに+1した数になっていることに注意
            }
          }
        }
      }
      return (positionCandidates, possiblePosition);
    }


    int calcBoardRating(GameController.Color[,] board, GameController.Color color) {
      //着手可能箇所数の差を計算
      int stoneDifference = countPossiblePosition(board, GameController.Color.White) - countPossiblePosition(board, GameController.Color.Black);
      int boardRating = stoneDifference + calcCornerScoreAll(board, color);
      return boardRating;
    }

    int countPossiblePosition(GameController.Color[,] board, GameController.Color color) {
      int possiblePosition = 0;
      for (int v = 0; v < 8; v++) {
        for (int h = 0; h < 8; h++) {
          if (board[v, h] == GameController.Color.Empty) {
            GameController.Color[,] boardTemp = new GameController.Color[8, 8];
            Array.Copy(board, boardTemp, board.Length);
            gameController.ReverseAll(v, h);
            if (board[v, h] == color) {
              board = boardTemp;
              possiblePosition++;
            }
          }
        }
      }
      return possiblePosition;
    }

    int calcCornerScoreAll(GameController.Color[,] board, GameController.Color color) {
      //左上
      int upperLeftScore = calcCornerScore(board, color, 0, 0, 1, 0) + calcCornerScore(board, color, 0, 0, 1, 1) + calcCornerScore(board, color, 0, 0, 0, 1);
      //右上
      int upperRightScore = calcCornerScore(board, color, 0, 7, 1, 0) + calcCornerScore(board, color, 0, 7, 1, -1) + calcCornerScore(board, color, 0, 7, 0, -1);
      //左下
      int lowerLeftScore = calcCornerScore(board, color, 7, 0, -1, 0) + calcCornerScore(board, color, 7, 0, -1, 1) + calcCornerScore(board, color, 7, 0, 0, 1);
      //右下
      int lowerRightScore = calcCornerScore(board, color, 7, 7, -1, 0) + calcCornerScore(board, color, 7, 7, -1, -1) + calcCornerScore(board, color, 7, 7, 0, -1);
      //評価値を合計
      int cornerScoreAll = upperLeftScore + upperRightScore + lowerLeftScore + lowerRightScore;
      return cornerScoreAll;
    }

    int calcCornerScore(GameController.Color[,] board, GameController.Color color, int v, int h, int directionV, int directionH) {
      //角から3マス分を取得し配列に格納
      GameController.Color[] stoneLinePattern = new GameController.Color[3];
      for (int i =0; i < 3; i++) {
        stoneLinePattern[i] = board[v, h];
        v += directionV;
        h += directionH;
      }

      int cornerScore = 0;
      GameController.Color oppositeColor = (GameController.Color)((int)color * -1);

      if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, color, color})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, color, oppositeColor})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, color, GameController.Color.Empty})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, oppositeColor, color})) {
        cornerScore += 4;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, oppositeColor, oppositeColor})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, oppositeColor, GameController.Color.Empty})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, GameController.Color.Empty, color})) {
        cornerScore += 4;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, GameController.Color.Empty, oppositeColor})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {color, GameController.Color.Empty, GameController.Color.Empty})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, color, color})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, color, oppositeColor})) {
        cornerScore += -4;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, color, GameController.Color.Empty})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, oppositeColor, color})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, oppositeColor, oppositeColor})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, oppositeColor, GameController.Color.Empty})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, GameController.Color.Empty, color})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, GameController.Color.Empty, oppositeColor})) {
        cornerScore += -4;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {oppositeColor, GameController.Color.Empty, GameController.Color.Empty})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, color, color})) {
        cornerScore += -2;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, color, oppositeColor})) {
        cornerScore += -5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, color, GameController.Color.Empty})) {
        cornerScore += -1;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, oppositeColor, color})) {
        cornerScore += 5;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, oppositeColor, oppositeColor})) {
        cornerScore += 2;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, oppositeColor, GameController.Color.Empty})) {
        cornerScore += 1;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, GameController.Color.Empty, color})) {
        cornerScore += 3;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, GameController.Color.Empty, oppositeColor})) {
        cornerScore += -3;
      }
      else if (stoneLinePattern.SequenceEqual(new GameController.Color[] {GameController.Color.Empty, GameController.Color.Empty, GameController.Color.Empty})) {
        cornerScore += 0;
      }

      return cornerScore;
    }

}
