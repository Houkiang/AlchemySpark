using System;
using System.Collections.Generic;
using UnityEngine;
using static Match3.GameGrid;

namespace Match3
{
    public class LevelNumber : Level
    {
        [SerializeField] private int targetScore;

        public List<PieceType> obstaclePieceTypes = new List<PieceType>();
        public int _movesUsed = 0;
        //List<ColorClearCount> herb;
        List<ColorClearCount> potion;
        public int choose;
        public List<ColorClearCount> baseHerbCountNeed;

        private int herbToNextInt = 0;

        public override void Awake()
        {
            base.Awake();
            type = LevelType.Number;

            hud.SetScore(currentScore);
            hud.SetTarget(targetScore);
            hud.SetRemaining(numMoves);
            //herb = SumHerb();
            //potion = SumPotion();
            //hud.SetRemainingherb(herb);
            //hud.SetRemainingpotion(potion);
            
            
             foreach (var colorclearcount in herbClearThresholds)
            {
                if (requiredHerbCounts != null)
                {

                
                    if (colorclearcount.color == requiredHerbCounts[0].color)
                {
                    herbToNextInt= colorclearcount.count;
                }
                hud.SetHerbToNext(herbToNextInt.ToString());
                }
            }

        }

        public override void OnMove()
        {
            base.OnMove();
            _movesUsed++;

            hud.SetRemaining(numMoves - _movesUsed);
            //herb = SumHerb();
            //Debug.Log("Number  herb长度是" + herb.Count);
            //potion = SumPotion();
            //hud.SetRemainingherb(herb);
            //hud.SetRemainingpotion(potion); 
        }

        public override void Judgewin()
        {
            if (numMoves - _movesUsed != 0) return;
            if (CheckAllCleared())
            {
                Debug.Log($"win!!!!!!!");
                GameWin();
            }
            else
            {
                Debug.Log($"LOSE!!!!!!!");
                GameLose();
            }
        }

        public bool CheckAllCleared()
        {
            bool sheet = true;
            switch(choose)
            {
                case 1:
                    foreach (var required in requiredHerbCounts)
                    {
                        var collected = gameGrid.collectedHerbCounts.Find(c => c.color == required.color);
                        if (collected.count < required.count)
                        {
                            sheet =  false;
                        }
                    }
                    break;
                case 2:
                    for(int i = 0; i < baseHerbCountNeed.Count; i++)
                    {
                        for(int j = 0; j < gameGrid.clearedHerbCounts.Count; j++)
                        {
                            if (gameGrid.clearedHerbCounts[j].color == baseHerbCountNeed[i].color)
                            {
                                if (gameGrid.clearedHerbCounts[j].count < baseHerbCountNeed[i].count) sheet = false;
                            }
                        }
                    }
                    break;
            }

            return sheet;
        }

        public void SetHubHerbToNext(GamePiece piece)
        {
            if(requiredHerbCounts!=null&& piece.IsColored())
            if (piece.ColorComponent.Color== requiredHerbCounts[0].color)
            {
                
                herbToNextInt --;
                foreach(var colorclearcount in herbClearThresholds)
                {
                    if(colorclearcount.color== requiredHerbCounts[0].color)
                    {
                        if(herbToNextInt<=0)
                        herbToNextInt += colorclearcount.count;
                    }
                }

                hud.SetHerbToNext(herbToNextInt.ToString());
            }
            
            
            
        }
        public List<ColorClearCount> SumHerb() {
            List<ColorClearCount> herb = new List<ColorClearCount> ();

            HashSet<ColorType> hash = new HashSet<ColorType>();
            for(int i = 0; i < requiredHerbCounts.Count; i++)
            {
                hash.Add(requiredHerbCounts[i].color);
            }
            for(int j = 0; j < herbClearThresholds.Count; j++)
            {
                if (hash.Contains(herbClearThresholds[j].color)){
                    ColorClearCount herbstruct = new ColorClearCount(herbClearThresholds[j].color, herbClearThresholds[j].count - gameGrid.clearedHerbCounts[j].count);
                    //Debug.Log("数量是"+herbstruct.count + "颜色是" + herbClearThresholds[j].color);
                    herb.Add(herbstruct);
                }
            }
            for (int i = 0; i < herb.Count; i++)
            {
                Debug.Log(i + " " + "herb[i].color" + herb[i].color + " count" + herb[i].count);
            }
            //Debug.Log(herb[0].count);
            return herb;
        }
        public List<ColorClearCount> SumPotion()
        {
            List<ColorClearCount> potion = new List<ColorClearCount>();
            HashSet<ColorType> hash = new HashSet<ColorType>();
            for (int i = 0; i < requiredHerbCounts.Count; i++)
            {
                hash.Add(requiredHerbCounts[i].color);
            }
            for (int j = 0; j < herbClearThresholds.Count; j++)
            {
                if (hash.Contains(herbClearThresholds[j].color))
                {
                    ColorClearCount herbstruct = new ColorClearCount(requiredHerbCounts[j].color, requiredHerbCounts[j].count - gameGrid.collectedHerbCounts[j].count);
                    potion.Add(herbstruct);
                }
            }
            return potion;
        }
    }
}