using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Five
{
    public void TestCases()
    {
        Case1();
        Case2();
        Case3();
        Case4();
    }

    public void ClearBoard()
    {
        for(int i=0; i<COLUMN; i++)
        {
            for(int j=0; j<ROW; j++)
            {
                board[i, j] = 0;
            }
        }
    }

    public void Case1()
    {
        ClearBoard();
        board[7, 7] = (int)Piece.BLACK;
        float score = Evaluate(false);
        Debug.LogFormat("case 1 score = {0}", score);

        ClearBoard();
    }

    public void Case2()
    {
        ClearBoard();
        board[7, 7] = (int)Piece.BLACK;
        board[7, 6] = (int)Piece.BLACK;
        float score = Evaluate(false);
        Debug.LogFormat("case 2 score = {0}", score);

        ClearBoard();
    }

    public void Case3()
    {
        ClearBoard();
        board[7, 7] = (int)Piece.BLACK;
        board[7, 6] = (int)Piece.BLACK;
        board[7, 5] = (int)Piece.BLACK;
        float score = Evaluate(false);
        Debug.LogFormat("case 3 score = {0}", score);

        ClearBoard();
    }

    public void Case4()
    {
        ClearBoard();
        board[7, 7] = (int)Piece.BLACK;
        board[7, 6] = (int)Piece.BLACK;
        board[7, 8] = (int)Piece.BLACK;
        board[6, 7] = (int)Piece.BLACK;
        board[5, 7] = (int)Piece.BLACK;
        float score = Evaluate(false);
        Debug.LogFormat("case 4 score = {0}", score);

        ClearBoard();
    }
}