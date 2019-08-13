using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
public partial class Five : MonoBehaviour
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

    public struct BlankPosition
    {
        public float score;
        public Position pos;
    }

    public const int MAX = 1000000;
    public const int GRID_WIDTH = 30;
    public const int COLUMN = 15;
    public const int ROW = 15;
    public const int DEPTH = 2;
    float ratio = 10;

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
    public Text win = null;
    public Text lose = null;
    int win_count = 0;
    int lose_count = 0;

    private Position last_puton_black = new Position(-1, -1);
    private Position last_puton_white = new Position(-1, -1);    

    public AudioSource sound = null;
    public GameObject thinking_object = null;
    public GameObject rule_root = null;
    public GameObject prefabRule = null;

    // Use this for initialization
    void Start()
    {
        CreateRules();
        profiler.Init();
        InitLogger();
        InitConfigData();
        record.Init();
        Reset();
    }

    public void InitConfigData()
    {        
        shape_score.Add(new Shape(50, new List<int>(new int[] { 0, 1, 1, 0, 0 })));
        shape_score.Add(new Shape(50, new List<int>(new int[] { 0, 0, 1, 1, 0 })));
        shape_score.Add(new Shape(200, new List<int>(new int[] { 1, 1, 0, 1, 0 })));

        shape_score.Add(new Shape(500, new List<int>(new int[] { 0, 0, 1, 1, 1 })));
        shape_score.Add(new Shape(500, new List<int>(new int[] { 1, 1, 1, 0, 0 })));
        shape_score.Add(new Shape(5000, new List<int>(new int[] { 0, 1, 1, 1, 0 })));

        shape_score.Add(new Shape(5000, new List<int>(new int[] { 0, 1, 0, 1, 1, 0 })));
        shape_score.Add(new Shape(5000, new List<int>(new int[] { 0, 1, 1, 0, 1, 0 })));
        shape_score.Add(new Shape(5000, new List<int>(new int[] { 1, 1, 1, 0, 1 })));

        shape_score.Add(new Shape(5000, new List<int>(new int[] { 1, 1, 0, 1, 1 })));
        shape_score.Add(new Shape(5000, new List<int>(new int[] { 1, 0, 1, 1, 1 })));
        shape_score.Add(new Shape(5000, new List<int>(new int[] { 1, 1, 1, 1, 0 })));

        shape_score.Add(new Shape(5000, new List<int>(new int[] { 0, 1, 1, 1, 1 })));
        shape_score.Add(new Shape(50000, new List<int>(new int[] { 0, 1, 1, 1, 1, 0 })));
        shape_score.Add(new Shape(MAX, new List<int>(new int[] { 1, 1, 1, 1, 1 })));
    }   

    public void CreateRules()
    {
        for(int i=0; i<COLUMN; i++)
        {
            GameObject ruleObj = GameObject.Instantiate(prefabRule);
            ruleObj.transform.parent = rule_root.transform;
            float x = -265 + i*GRID_WIDTH;
            float y = -456;
            ruleObj.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
            ruleObj.GetComponent<Text>().text = string.Format("{0}", i);
        }

        for (int i = 1; i < ROW; i++)
        {
            GameObject ruleObj = GameObject.Instantiate(prefabRule);
            ruleObj.transform.parent = rule_root.transform;
            float x = -278;
            float y = -448 + i * GRID_WIDTH;
            ruleObj.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
            ruleObj.GetComponent<Text>().text = string.Format("{0}", i);
        }
    }

    public void Reset()
    {
        last_puton_black = new Position(-1, -1);
        last_puton_white = new Position(-1, -1);
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

        game_end = false;

        GameObject pieceObj = GameObject.Instantiate(prefabBlack);
        pieceObj.transform.parent = panel.transform;
        pieceObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        pieceObj.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);

        message.text = "";

        record.Reset();

        ShowThinking(false);

        AIReset();
    }
    public void OnResetButtonClick()
    {
        Reset();
    }

    public void Regret()
    {

    }

    public void OnRegretButtonClick()
    {
        Regret();
    }

    public Position GetPositionFromMouseInput(float x, float y)
    {
        float offset_x = Screen.width / 2 - GRID_WIDTH * (COLUMN/2);
        float offset_y = Screen.height / 2 - GRID_WIDTH * (ROW/2);
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
            if (IsValid(pos) && !HasPiece(pos))
            {
                Puton(Piece.BLACK, pos);
                PutonCursor(pos, Piece.BLACK);
                CheckGameEnd();
                record.Hand(Piece.BLACK, pos);

                if (GameWin(Piece.BLACK) || GameWin(Piece.WHITE))
                    return;

                ai_computing = true;
                StartCoroutine(DoAI(pos));
            }
        }

        win.text = string.Format("Win:{0}", win_count);
        lose.text = string.Format("Lose:{0}", lose_count);
	}

    bool CheckGameEnd()
    {
        if (GameWin(Piece.BLACK))
        {
            message.text = "YOU WIN!";
            game_end = true;
            win_count++;
            return true;
        }
        else if (GameWin(Piece.WHITE))
        {
            message.text = "YOU LOSE";
            game_end = true;
            lose_count++;
            return true;
        }
        return false;

    }

    

    void PutonCursor(Position pos, Piece piece)
    {
        if (cursor != null)
            Destroy(cursor);
        cursor = GameObject.Instantiate(piece == Piece.BLACK ? prefabWhite : prefabBlack);
        cursor.transform.parent = panel.transform;
        float x = (pos.x - 7) * GRID_WIDTH;
        float y = (pos.y - 7) * GRID_WIDTH;
        cursor.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        cursor.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    public void Puton(Piece piece, Position pos)
    {
        GameObject pieceObj = GameObject.Instantiate(piece==Piece.BLACK?prefabBlack:prefabWhite);
        pieceObj.transform.parent = panel.transform;
        float x = (pos.x - (COLUMN/2)) * GRID_WIDTH;
        float y = (pos.y - (ROW/2)) * GRID_WIDTH;
        pieceObj.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        board[pos.x, pos.y] = (int)piece;
        if (piece == Piece.BLACK)
            last_puton_black = pos;
        else
            last_puton_white = pos;

        sound.Play();
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

    public void ShowThinking(bool show)
    {
        thinking_object.SetActive(show);
    }
    IEnumerator DoAI(Position pos)
    {
        ShowThinking(true);
        yield return null;

        //Position ai_pos = AI();
        ai_start_flag = true;
        while (ai_finish_flag == false)
            yield return null;

        ai_finish_flag = false;
        Position ai_pos = aiPos;

        Debug.LogFormat("AI detail search count:{0}, cut count:{1}", search_count, cut_count);
        yield return new WaitForSeconds(0.1f);
        Puton(Piece.WHITE, ai_pos);
        PutonCursor(ai_pos, Piece.WHITE);
        CheckGameEnd();
        record.Hand(Piece.WHITE, ai_pos);
        ShowThinking(false);
        ai_computing = false;
    }
}
