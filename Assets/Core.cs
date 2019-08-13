using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Five
{
    public Position AI()
    {
        profiler.Reset();
        ClearLogger();

        cut_count = 0;
        search_count = 0;        
        float time = System.DateTime.Now.Millisecond / 1000.0f;
        NegaMax(true, DEPTH, -float.MaxValue, float.MaxValue);
        float delta = System.DateTime.Now.Millisecond / 1000.0f - time;
        Debug.LogFormat("cost time {0} s", delta);
        Debug.LogFormat("get blank list {0} s, count {1}", profiler.profiler[(int)ProfilerFunction.GET_BLANK_LIST], profiler.count[(int)ProfilerFunction.GET_BLANK_LIST]);
        Debug.LogFormat("cal score {0} s, count {1}", profiler.profiler[(int)ProfilerFunction.CAL_SCORE], profiler.count[(int)ProfilerFunction.CAL_SCORE]);
        Debug.LogFormat("evaluate cost {0} s, count {1}", profiler.profiler[(int)ProfilerFunction.EVALUATE], profiler.count[(int)ProfilerFunction.EVALUATE]);
        return next_point;
    }

    public float NegaMax(bool is_ai, int depth, float alpha, float beta)
    {        
        if (GameWin(Piece.BLACK) || GameWin(Piece.WHITE) || depth == 0)            
            return Evaluate(is_ai);

        Output(string.Format("++++++++++++++++++++++++++++++++++depth {0} alpha {1} beta {2}", depth, alpha, beta));
        List<BlankPosition> blank_list = GetSortBlankList(is_ai);
        foreach (var bp in blank_list)
        {
            var next_step = bp.pos;
            search_count++;
            if (!HasNeibour(next_step))
                continue;

            if (is_ai)
            {
                board[next_step.x, next_step.y] = (int)Piece.WHITE;                
            }
            else
            {
                board[next_step.x, next_step.y] = (int)Piece.BLACK;
            }

            Output(string.Format("[[[[[[[[[ puton {0} {1}, color {2}", next_step.x, next_step.y, board[next_step.x, next_step.y]));

            float value = -NegaMax(!is_ai, depth - 1, -beta, -alpha);

            Output(string.Format("@@@@@@@@@@@@ value {0} depth {1}", value, depth));

            board[next_step.x, next_step.y] = 0;

            Output(string.Format("]]]]]]]] remove {0} {1}", next_step.x, next_step.y));

            if (value > alpha)
            {
                alpha = value;
                Output(string.Format("!!!!!!!!!!modify alpha {0}", alpha));

                if (depth == DEPTH)
                {
                    next_point = next_step;
                    Output(string.Format("** set next point {0} {1}", next_point.x, next_point.y));
                }

                if (value >= beta)
                {
                    Output(string.Format("|||||||||||||||||cut success beta {0}", beta));
                    cut_count++;                    
                    break;
                }                
            }

        }

        Output(string.Format("-----------------------------------------alpha {0} beta {1}", alpha, beta));
        return alpha;
    }

    

    public void EvaBlankPosition(bool is_ai, BlankPosition bp)
    {
        Piece piece = is_ai ? Piece.WHITE : Piece.BLACK;
        board[bp.pos.x, bp.pos.y] = (int)piece;

        float score1 = CalBlankScore(bp.pos.x, bp.pos.y, piece, 0, 1);
        float score2 = CalBlankScore(bp.pos.x, bp.pos.y, piece, 1, 0);
        float score3 = CalBlankScore(bp.pos.x, bp.pos.y, piece, 1, 1);
        float score4 = CalBlankScore(bp.pos.x, bp.pos.y, piece, -1, 1);
        bp.score += Mathf.Max(score1, score2, score3, score4);
        board[bp.pos.x, bp.pos.y] = 0;

    }

    public float CalBlankScore(int m, int n, Piece faction, int x_direct, int y_direct)
    {
        float max_score = 0;
        for (int offset = -5; offset <= 0; offset++)
        {
            List<int> pos6 = new List<int>();
            List<int> pos5 = new List<int>();
            for (int i = 0; i <= 5; i++)
            {
                if (IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), GetEnemyPiece(faction)))
                {
                    pos6.Add(2);
                }
                else if (IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), faction))
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


            for (int index = shape_score.Count - 1; index >= 0; index--)
            {
                var item = shape_score[index];
                if (Equal(pos5, item.pieces) || Equal(pos6, item.pieces))
                {
                    if (item.score > max_score)
                    {
                        max_score = item.score;
                        break;
                    }
                }
            }
        }
        return max_score;
    }

    public List<BlankPosition> GetSortBlankList(bool is_ai)
    {
        profiler.Enter(ProfilerFunction.GET_BLANK_LIST);
        List<BlankPosition> blank_list = new List<BlankPosition>();
        for (int m = 0; m < COLUMN; m++)
        {
            for (int n = 0; n < ROW; n++)
            {
                if (!HasPiece(new Position(m, n)))
                {
                    BlankPosition bp = new BlankPosition();
                    bp.score = 0;
                    if (last_puton_black.x >= 0 && Mathf.Abs(m - last_puton_black.x) <= 1 && Mathf.Abs(n - last_puton_black.y) <= 1)
                        bp.score += 10;
                    if (last_puton_white.x >= 0 && Mathf.Abs(m - last_puton_white.x) <= 1 && Mathf.Abs(n - last_puton_white.y) <= 1)
                        bp.score += 10;
                    if (HasNeibour(new Position(m, n)))
                        bp.score += 1;

                    bp.pos = new Position(m, n);
                    EvaBlankPosition(is_ai, bp);
                    if (bp.score > 0)
                        blank_list.Add(bp);
                }
            }
        }

        blank_list.Sort((a, b) => -a.score.CompareTo(b.score));
        profiler.Leave(ProfilerFunction.GET_BLANK_LIST);
        return blank_list;
    }



    public bool HasNeibour(Position p)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
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
        for (int m = 0; m < COLUMN; m++)
        {
            for (int n = 0; n < ROW; n++)
            {
                if (board[m, n] == (int)my_piece)
                {
                    my_score += CalScore(m, n, my_piece, 0, 1, score_all_arr);
                    my_score += CalScore(m, n, my_piece, 1, 0, score_all_arr);
                    my_score += CalScore(m, n, my_piece, 1, 1, score_all_arr);
                    my_score += CalScore(m, n, my_piece, -1, 1, score_all_arr);
                }
                else if (board[m, n] == (int)enemy_piece)
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

        foreach (var item in score_all_arr)
        {
            if (item.Exist(new Position(m, n)) && item.x_direct == x_direct && item.y_direct == y_direct)
            {
                profiler.Leave(ProfilerFunction.CAL_SCORE);
                return 0;
            }
        }

        for (int offset = -5; offset <= 0; offset++)
        {
            List<int> pos6 = new List<int>();
            List<int> pos5 = new List<int>();
            for (int i = 0; i <= 5; i++)
            {
                if (IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), GetEnemyPiece(faction)))
                {
                    pos6.Add(2);
                }
                else if (IsPiece(new Position(m + (i + offset) * x_direct, n + (i + offset) * y_direct), faction))
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

            for (int index = shape_score.Count - 1; index >= 0; index--)
            {
                var item = shape_score[index];
                if (Equal(pos5, item.pieces) || Equal(pos6, item.pieces))
                {
                    if (item.score > max_score_shape.score)
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
                        break;
                    }
                }
            }
        }

        if (max_score_shape.pieces.Count > 0)
        {
            foreach (var item in score_all_arr)
            {
                foreach (var pt1 in item.pieces)
                {
                    foreach (var pt2 in max_score_shape.pieces)
                    {
                        if (Equal(pt1, pt2) && max_score_shape.score > 10 && item.score > 10)
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
        if (list1.Count == list2.Count)
        {
            for (int i = 0; i < list1.Count; i++)
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
}