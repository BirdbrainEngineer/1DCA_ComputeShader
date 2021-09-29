using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cell{
    public int state, rule;
};

public class Board{
    public Cell[] board;
    public Board(int length){
        this.board = new Cell[length];
    }
    public float[] BoardToTexture(){
        float[] result = new float[this.board.Length * 4];
        for(int i = 0; i < this.board.Length; i++){
            var v = i * 4;
            result[v] = 0.0f;
            result[v + 1] = 0.0f;
            result[v + 2] = (float)this.board[i].rule;
            result[v + 3] = (float)this.board[i].state;
        }
        return result;
    }
}

public class Datastructure{
    public static readonly long MAXDATASIZE = 1073741824;

    public Board[] boards;
    public readonly List<uint[]> initData;
    public readonly int boardCount;
    public readonly int boardWidth;
    public readonly int boardGenerations;
    public readonly int boardLength;

    public Datastructure(int boardWidth, int boardGenerations, int boardsToSimulate, List<uint[]> initData){
        this.boardWidth = boardWidth;
        this.boardGenerations = boardGenerations;
        this.boardCount = boardsToSimulate;
        this.initData = initData;
        this.boardLength = this.boardWidth * this.boardGenerations;
        this.boards = new Board[this.boardCount];
        for(int i = 0; i < this.boardCount; i++){ 
            this.boards[i] = new Board(this.boardLength); 
        }
    }

    public static long EstimateSize(int width, int generations, int boards){
        return   (long)((long)width * 
                        (long)generations * 
                        (long)boards * 
                        (long)sizeof(int) * (long)2);
    }

    public Cell[] ParseInitializer(int boardIndex, int paddedWidth){
        Cell[] result = new Cell[paddedWidth];
        for(int i = 0; i < paddedWidth; i++){
            var dataVector = Mathf.FloorToInt(i / Dispatcher.SIMBLOCKSIZE);
            var bitState = (this.initData[boardIndex][dataVector] << (i % 32)) & 0x80000000;
            result[i].state = bitState == 0 ? 0 : 1;
            result[i].rule = 7;
        }
        return result;
    }
}
