using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Five : MonoBehaviour
{
    public enum Piece
    {
        EMPTY = 0,
        BLACK = 1,
        WHITE = 2
    }

    
    public struct Position
    {
        public int x;
        public int y;

        public Position(int x_, int y_)
        {
            x = x_;
            y = y_;
        }
    }

    public class Shape
    {
        public int score;
        public List<int> pieces = new List<int>();
        public Shape(int score_, List<int> pieces_)
        {
            score = score_;
            pieces = pieces_;
        }
        
    }

    public class ScoreShape
    {
        public int score = 0;
        public List<Position> pieces = new List<Position>();
        public int x_direct = 0;
        public int y_direct = 0;
        public bool Exist(Position pos)
        {
            foreach(var item in pieces)
            {
                if(item.x==pos.x && item.y==pos.y)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public const int GRID_WIDTH = 40;
    public const int COLUMN = 15;
    public const int ROW = 15;
    public const int DEPTH = 3;
    int ratio = 1;

    private List<Position> list1 = new List<Position>();
    private List<Position> list2 = new List<Position>();
    private List<Position> list3 = new List<Position>();

    private List<Position> list_all = new List<Position>();

    Position next_point = new Position(0, 0);

    private List<Shape> shape_score = new List<Shape>();
    private int[,] board = new int[COLUMN, ROW];

    int change = 0;
    int g = 0;
    

    // Use this for initialization
    void Start ()
    {
        int[] scores = { 50, 50, 200, 500, 500, 5000, 5000, 5000, 5000, 5000, 5000, 50000, 99999999 };
        int[][] temp =
        {
            new int[]{ 0, 1, 1, 0, 0 },
            new int[]{ 0, 0, 1, 1, 0 },
            new int[]{ 1, 1, 0, 1, 0 },
            new int[]{ 0, 0, 1, 1, 1 },
            new int[]{ 1, 1, 1, 0, 0 },
            new int[]{ 0, 1, 1, 1, 0 },
            new int[]{ 0, 1, 0, 1, 1, 0 },
            new int[]{ 0, 1, 1, 0, 1, 0 },
            new int[]{ 1, 1, 1, 0, 1 },
            new int[]{ 1, 1, 0, 1, 1 },
            new int[]{ 1, 0, 1, 1, 1 },
            new int[]{ 1, 1, 1, 1, 0 },
            new int[]{ 0, 1, 1, 1, 1 },
            new int[]{ 0, 1, 1, 1, 1, 0 },
            new int[]{ 1, 1, 1, 1, 1 },
        };
        for(int i=0; i<scores.Length; i++)
        {
            shape_score.Add(new Shape(scores[i], new List<int>(temp[i])));
        }
        

        for(int i=0; i<COLUMN; i++)
        {
            for(int j=0; j<ROW; j++)
            {
                list_all.Add(new Position(i, j));
            }
        }
        
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public Position AI()
    {

        return new Position(0, 0);
    }

    public int CalScore(int m, int n, Piece faction, int x_direct, int y_direct, List<ScoreShape> score_all_arr)
    {
        int add_score = 0;
        ScoreShape max_score_shape = new ScoreShape();

        foreach(var item in score_all_arr)
        {
            if(item.Exist(new Position(m, n)) && item.x_direct==x_direct && item.y_direct == y_direct)
            {
                return 0;
            }
        }

        for(int offset=-5; offset<=0; offset++)
        {
            List<int> pos = new List<int>();
            for(int i=0; i<=5; i++)
            {
                if(IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), GetEnemyPiece(faction)))
                {
                    pos.Add(2);
                }
                else if(IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), faction))
                {
                    pos.Add(1);
                }
                else
                {
                    pos.Add(0);
                }
            }

            foreach(var item in shape_score)
            {
                if(Equal(pos, item.pieces))
                {
                    if(item.score>max_score_shape.score)
                    {
                        max_score_shape.score = item.score;
                        max_score_shape.pieces.Clear();
                        max_score_shape.pieces.Add(new Position(m + (0 + offset) * x_direct, n + (0 + offset) * y_direct));
                        max_score_shape.pieces.Add(new Position(m + (1 + offset) * x_direct, n + (1 + offset) * y_direct));
                        max_score_shape.pieces.Add(new Position(m + (2 + offset) * x_direct, n + (2 + offset) * y_direct));
                        max_score_shape.pieces.Add(new Position(m + (3 + offset) * x_direct, n + (3 + offset) * y_direct));
                        max_score_shape.pieces.Add(new Position(m + (4 + offset) * x_direct, n + (4 + offset) * y_direct));

                        max_score_shape.x_direct = x_direct;
                        max_score_shape.y_direct = y_direct;
                    }
                }
            }
        }

        if(max_score_shape.pieces.Count>0)
        {
            foreach(var item in score_all_arr)
            {
                foreach(var pt1 in item.pieces)
                {
                    foreach(var pt2 in max_score_shape.pieces)
                    {
                        if(Equal(pt1, pt2) && max_score_shape.score>10 && item.score>10)
                        {
                            add_score += max_score_shape.score + item.score;
                        }
                    }
                }
            }


            score_all_arr.Add(max_score_shape);
        }
        return add_score + max_score_shape.score;
    }

    public bool Equal(List<int> list1, List<int> list2)
    {
        if(list1.Count==list2.Count)
        {
            for(int i=0; i<list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }
        return false;
    }

    public bool Equal(Position p1, Position p2)
    {
        return p1.x == p2.x && p1.y == p2.y;
    }

    public Piece GetEnemyPiece(Piece piece)
    {
        if (piece == Piece.WHITE)
            return Piece.BLACK;
        else if (piece == Piece.BLACK)
            return Piece.WHITE;

        return Piece.EMPTY;
    }

    public bool IsValid(Position pos)
    {
        return pos.x >= 0 && pos.y < COLUMN && pos.y >= 0 && pos.y < ROW;
    }

    public bool IsPiece(Position pos, Piece piece)
    {
        if (!IsValid(pos))
            return false;
        return board[pos.x, pos.y] == (int)piece;
    }

    public bool GameWin(Piece piece)
    {
        for(int m=0; m<COLUMN; m++)
        {
            for(int n=0; n<ROW; n++)
            {
                if (n < ROW - 4
                    && board[m, n] == (int)piece
                    && board[m, n + 1] == (int)piece
                    && board[m, n + 2] == (int)piece
                    && board[m, n + 3] == (int)piece
                    && board[m, n + 4] == (int)piece)
                    return true;
                else if (m < ROW - 4
                    && board[m, n] == (int)piece
                    && board[m + 1, n] == (int)piece
                    && board[m + 2, n] == (int)piece
                    && board[m + 3, n] == (int)piece
                    && board[m + 4, n] == (int)piece)
                    return true;
                else if (m < ROW - 4 && n < ROW - 4
                    && board[m, n] == (int)piece
                    && board[m + 1, n + 1] == (int)piece
                    && board[m + 2, n + 2] == (int)piece
                    && board[m + 3, n + 3] == (int)piece
                    && board[m + 4, n + 4] == (int)piece)
                    return true;
                else if (m < ROW - 4 && n > 3
                    && board[m, n] == (int)piece
                    && board[m + 1, n - 1] == (int)piece
                    && board[m + 2, n - 2] == (int)piece
                    && board[m + 3, n - 3] == (int)piece
                    && board[m + 4, n - 4] == (int)piece)
                    return true;

            }
        }
        return false;
    }
}
