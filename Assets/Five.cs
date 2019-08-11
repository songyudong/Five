using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    enum ProfilerFunction
    {
        GET_BLANK_LIST = 0,
        EVALUATE = 1,
        CAL_SCORE = 2,
        COUNT = 3,
    }

    class Profiler
    {
        public List<float> profiler = new List<float>();
        public List<float> enter = new List<float>();
        public List<int> count = new List<int>();
        public void Init()
        {
            for (int i = 0; i < (int)ProfilerFunction.COUNT; i++)
            {
                profiler.Add(0);
                enter.Add(0);
                count.Add(0);
            }
        }
        public void Reset()
        {
            for(int i=0; i<(int)ProfilerFunction.COUNT; i++)
            {
                profiler[i] = 0;
                enter[i] = 0;
                count[i] = 0;
            }

        }

        public void Enter(ProfilerFunction func)
        {
            enter[(int)func] = Time.realtimeSinceStartup;
        }

        public void Leave(ProfilerFunction func)
        {
            count[(int)func]++;
            profiler[(int)func] += Time.realtimeSinceStartup - enter[(int)func];
        }
    }   

    public class Shape
    {
        public float score;
        public List<int> pieces = new List<int>();
        public Shape(float score_, List<int> pieces_)
        {
            score = score_;
            pieces = pieces_;
        }

    }

    public class ScoreShape
    {
        public float score = 0;
        public List<Position> pieces = new List<Position>();
        public int x_direct = 0;
        public int y_direct = 0;
        public bool Exist(Position pos)
        {
            foreach (var item in pieces)
            {
                if (item.x == pos.x && item.y == pos.y)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public const int GRID_WIDTH = 30;
    public const int COLUMN = 15;
    public const int ROW = 15;
    public const int DEPTH = 3;
    float ratio = 2;

    Position next_point = new Position(0, 0);

    private List<Shape> shape_score = new List<Shape>();
    private int[,] board = new int[COLUMN, ROW];

    public GameObject prefabBlack;
    public GameObject prefabWhite;
    public GameObject prefabCursor;
    public GameObject panel;

    int search_count = 0;
    int cut_count = 0;

    bool game_end = false;
    bool ai_computing = false;

    GameObject cursor = null;
    public Text message = null;

    private Position last_puton_black = new Position(-1, -1);
    private Position last_puton_white = new Position(-1, -1);
    Profiler profiler = new Profiler();
    // Use this for initialization
    void Start()
    {
        int[] scores = { 50, 50, 200, 500, 500, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 50000, 99999999 };
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
        for (int i = 0; i < scores.Length; i++)
        {
            shape_score.Add(new Shape(scores[i], new List<int>(temp[i])));
        }

        profiler.Init();
        Reset();
    }

    

    public void Reset()
    {
        for (int m = 0; m < COLUMN; m++)
        {
            for (int n = 0; n < ROW; n++)
            {
                board[m, n] = 0;
            }
        }

        for (int i = 0; i < panel.transform.childCount; i++)
        {
            Destroy(panel.transform.GetChild(i).gameObject);
        }

        search_count = 0;
        cut_count = 0;

        game_end = false;

        GameObject pieceObj = GameObject.Instantiate(prefabBlack);
        pieceObj.transform.parent = panel.transform;
        pieceObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        pieceObj.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);

        message.text = "";
    }
    public void OnResetButtonClick()
    {
        Reset();
    }

    public Position GetPositionFromMouseInput(float x, float y)
    {
        float offset_x = 540 / 2 - GRID_WIDTH * 7;
        float offset_y = 960 / 2 - GRID_WIDTH * 7;
        int grid_x = Mathf.RoundToInt((x - offset_x) / GRID_WIDTH);
        int grid_y = Mathf.RoundToInt((y - offset_y) / GRID_WIDTH);
        return new Position(grid_x, grid_y);
    }

	// Update is called once per frame
	void Update ()
    {
		if(!game_end && !ai_computing && Input.GetMouseButtonUp(0))
        {            
            Position pos = GetPositionFromMouseInput(Input.mousePosition.x, Input.mousePosition.y);
            if (IsValid(pos))
            {
                Puton(Piece.BLACK, pos);
                PutonCursor(pos);
                CheckGameEnd();

                ai_computing = true;
                StartCoroutine(DoAI(pos));
            }
        }
	}

    bool CheckGameEnd()
    {
        if (GameWin(Piece.BLACK))
        {
            message.text = "YOU WIN!";
            game_end = true;
            return true;
        }
        else if (GameWin(Piece.WHITE))
        {
            message.text = "YOU LOSE";
            game_end = true;
            return true;
        }
        return false;

    }

    IEnumerator DoAI(Position pos)
    {
       
        yield return null;
        Position ai_pos = AI();
        Debug.LogFormat("AI detail search count:{0}, cut count:{1}", search_count, cut_count);
        Puton(Piece.WHITE, ai_pos);
        PutonCursor(ai_pos);        
        CheckGameEnd();
        ai_computing = false;

    }

    void PutonCursor(Position pos)
    {
        if (cursor != null)
            Destroy(cursor);
        cursor = GameObject.Instantiate(prefabCursor);
        cursor.transform.parent = panel.transform;
        float x = (pos.x - 7) * GRID_WIDTH;
        float y = (pos.y - 7) * GRID_WIDTH;
        cursor.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
    }

    public void Puton(Piece piece, Position pos)
    {
        GameObject pieceObj = GameObject.Instantiate(piece==Piece.BLACK?prefabBlack:prefabWhite);
        pieceObj.transform.parent = panel.transform;
        float x = (pos.x - 7) * GRID_WIDTH;
        float y = (pos.y - 7) * GRID_WIDTH;
        pieceObj.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        board[pos.x, pos.y] = (int)piece;
        if (piece == Piece.BLACK)
            last_puton_black = pos;
        else
            last_puton_white = pos;
    }

    public Position AI()
    {
        profiler.Reset();
        cut_count = 0;
        search_count = 0;
        float time = Time.realtimeSinceStartup;
        NegaMax(true, DEPTH, -99999999, 99999999);
        float delta = Time.realtimeSinceStartup - time;
        Debug.LogFormat("cost time {0} s", delta);
        Debug.LogFormat("cal score {0} s, count {1}", profiler.profiler[(int)ProfilerFunction.CAL_SCORE], profiler.count[(int)ProfilerFunction.CAL_SCORE]);
        Debug.LogFormat("evaluate cost {0} s, count {1}", profiler.profiler[(int)ProfilerFunction.EVALUATE], profiler.count[(int)ProfilerFunction.EVALUATE]);
        return next_point;        
    }

    public float NegaMax(bool is_ai, int depth, float alpha, float beta)
    {
        if(GameWin(Piece.BLACK) || GameWin(Piece.WHITE) || depth==0)
            return Evaluate(is_ai);

        List<BlankPosition> blank_list = GetSortBlankList(is_ai);
        foreach(var bp in blank_list)
        {
            var next_step = bp.pos;
            search_count++;
            if (!HasNeibour(next_step))
                continue;

            if (is_ai)
            {
                board[next_step.x, next_step.y] = (int)Piece.WHITE;
                if(depth==DEPTH)
                {
                    if(CheckGameEnd())
                    {
                        next_point = next_step;
                        return 99999999;
                    }
                }
            }
            else
            {
                board[next_step.x, next_step.y] = (int)Piece.BLACK;
            }

            float value = -NegaMax(!is_ai, depth - 1, -beta, -alpha);
            board[next_step.x, next_step.y] = 0;

            if(value>alpha)
            {
                if (depth == DEPTH)
                    next_point = next_step;

                if(value>=beta)
                {
                    cut_count++;
                    return beta;
                }

                alpha = value;
            }

        }
        return alpha;
    }

    public struct BlankPosition
    {
        public float score;
        public Position pos;
    }

    public List<BlankPosition> GetSortBlankList(bool is_ai)
    {
        List<BlankPosition> blank_list = new List<BlankPosition>();
        for (int m = 0; m < COLUMN; m++)
        {
            for (int n = 0; n < ROW; n++)
            {
                if (!HasPiece(new Position(m, n)))
                {
                    BlankPosition bp = new BlankPosition();
                    bp.score = 0;
                    if (last_puton_black.x >=0 && Mathf.Abs(m - last_puton_black.x) <= 1 && Mathf.Abs(n - last_puton_black.y) <= 1)
                        bp.score += 10;
                    if (last_puton_white.x >=0 && Mathf.Abs(m - last_puton_white.x) <= 1 && Mathf.Abs(n - last_puton_white.y) <= 1)
                        bp.score += 10;
                    if (HasNeibour(new Position(m, n)))
                        bp.score += 1;

                    bp.pos = new Position(m, n);
                    if(bp.score>0)
                        blank_list.Add(bp);
                }
            }
        }

        blank_list.Sort((a, b) => -a.score.CompareTo(b.score));
        return blank_list;
    }

    

    public bool HasNeibour(Position p)
    {
        for(int i=-1; i<=1; i++)
        {
            for(int j=-1; j<=1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                int m = p.x + i;
                int n = p.y + j;
                if (HasPiece(new Position(m, n)))
                    return true;
            }
        }
        return false;
    }

    public float Evaluate(bool is_ai)
    {
        profiler.Enter(ProfilerFunction.EVALUATE);
        var score_all_arr = new List<ScoreShape>();
        var score_all_arr_enemy = new List<ScoreShape>();
        float my_score = 0;
        float ememy_score = 0;

        Piece my_piece = GetPiece(is_ai);
        Piece enemy_piece = GetEnemyPiece((Piece)my_piece);
        for (int m=0; m<COLUMN; m++)
        {
            for(int n=0; n<ROW; n++)
            {
                if(board[m, n]==(int)my_piece)
                {
                    my_score += CalScore(m, n, my_piece, 0, 1, score_all_arr);
                    my_score += CalScore(m, n, my_piece, 1, 0, score_all_arr);
                    my_score += CalScore(m, n, my_piece, 1, 1, score_all_arr);
                    my_score += CalScore(m, n, my_piece, -1, 1, score_all_arr);
                }
                else if(board[m, n] == (int)enemy_piece)
                {
                    ememy_score += CalScore(m, n, enemy_piece, 0, 1, score_all_arr_enemy);
                    ememy_score += CalScore(m, n, enemy_piece, 1, 0, score_all_arr_enemy);
                    ememy_score += CalScore(m, n, enemy_piece, 1, 1, score_all_arr_enemy);
                    ememy_score += CalScore(m, n, enemy_piece, -1, 1, score_all_arr_enemy);
                }
            }
        }

        float total_score = my_score - ememy_score * ratio * 0.1f;
        profiler.Leave(ProfilerFunction.EVALUATE);
        return total_score;
    }

    public float CalScore(int m, int n, Piece faction, int x_direct, int y_direct, List<ScoreShape> score_all_arr)
    {
        profiler.Enter(ProfilerFunction.CAL_SCORE);
        float add_score = 0;
        ScoreShape max_score_shape = new ScoreShape();

        foreach(var item in score_all_arr)
        {
            if(item.Exist(new Position(m, n)) && item.x_direct==x_direct && item.y_direct == y_direct)
            {
                profiler.Leave(ProfilerFunction.CAL_SCORE);
                return 0;
            }
        }

        for(int offset=-5; offset<=0; offset++)
        {
            List<int> pos6 = new List<int>();
            List<int> pos5 = new List<int>();
            for(int i=0; i<=5; i++)
            {
                if(IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), GetEnemyPiece(faction)))
                {
                    pos6.Add(2);
                }
                else if(IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), faction))
                {
                    pos6.Add(1);
                }
                else
                {
                    pos6.Add(0);
                }
            }

            for (int c = 0; c < 5; c++)
                pos5.Add(pos6[c]);

            foreach(var item in shape_score)
            {
                if(Equal(pos5, item.pieces) || Equal(pos6, item.pieces))
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

        profiler.Leave(ProfilerFunction.CAL_SCORE);
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

    public Piece GetPiece(bool is_ai)
    {
        if (is_ai)
            return Piece.WHITE;
        return Piece.BLACK;
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
        return pos.x >= 0 && pos.x < COLUMN && pos.y >= 0 && pos.y < ROW;
    }

    public bool HasPiece(Position pos)
    {
        if (!IsValid(pos))
            return false;

        return board[pos.x, pos.y] != 0;
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
