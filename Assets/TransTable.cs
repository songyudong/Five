using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Five
{
    public class CacheItem
    {
        public float score = 0;
        public int depth = 0;
        public CacheItem(int depth_, float score_)
        {
            depth = depth_;
            score = score_;
        }
    }

    public class TransTable
    {
        public Dictionary<int, CacheItem> items = new Dictionary<int, CacheItem>();

        public List<int> computer = new List<int>();
        public List<int> human = new List<int>();

        public int code = 0;

        public int Rand()
        {
            return Random.Range(1, 1000000000);
        }

        public void Init()
        {
            computer.Clear();
            human.Clear();

            for(int i=0; i<COLUMN; i++)
            {
                for(int j=0; j<ROW; j++)
                {
                    computer.Add(Rand());
                    human.Add(Rand());
                }
            }

            code = Rand();
        }

        public int Go(int x, int y, Piece piece)
        {
            int index = x * ROW + y;
            code ^= (piece == Piece.BLACK ? human[index] : computer[index]);
            return code;
        }

        public void Cache(int depth, float score)
        {
            items.Add(code, new CacheItem(depth, score));
        }
    
    }

    public TransTable transTable = new TransTable();

    
}