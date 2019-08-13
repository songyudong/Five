using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Five
{
    public class RecordHand
    {
        public Piece faction;
        public Position position;
        public RecordHand(Piece fac, Position pos)
        {
            faction = fac;
            position = pos;
        }
    }
    public class Record
    {
        string folder;
        string filename;
        string path;
        FileInfo fileInfo;
        int step = 0;

        public List<RecordHand> hands = new List<RecordHand>();

        public void Init()
        {
            folder = Application.dataPath + "/..";
            path = folder + "/Record/record.txt";
            if (File.Exists(path))
                File.Delete(path);
            fileInfo = new FileInfo(path);
        }

        string PieceToText(Piece piece)
        {
            if (piece == Piece.BLACK)
                return "Black";
            else
                return "White";
        }

        public void Hand(Piece piece, Position pos)
        {
            var sw = fileInfo.AppendText();
            sw.WriteLine(string.Format("{0} : {1, 2} {2, 2} -- {3}", PieceToText(piece), pos.x, pos.y, step));
            sw.Close();
            step++;
        }

        public void Reset()
        {
            Init();
            hands.Clear();
            step = 0;
        }
    }

    public Record record = new Record();
}