using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Five
{
    public class TransTable
    {
        public List<int> computer = new List<int>();
        public List<int> human = new List<int>();

        public int code = 0;

        public int Rand()
        {
            return Random.Range(1, 1000000000);
        }

        public void Init()
        {
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
    }
}