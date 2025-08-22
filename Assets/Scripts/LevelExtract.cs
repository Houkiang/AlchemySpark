using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Match3.GameGrid;
namespace Match3
{
    public class LevelExtract : Level
    {

        public List<ColorClearCount> herbToExtractThresholds = new List<ColorClearCount>();//生成每种颜色精华的阈值，默认999为无法生成对应颜色
        public ColorClearCount requiredExtractCounts;//通关需要的目标精华的数量
        public int currentRequiredExtractCount = 0; // Current count of the required extract
        public List<PieceType> obstaclePieceTypesForFault = new List<PieceType>();//错误精华收集产生的障碍

        public override void Awake()
        {
            base.Awake();
            type = LevelType.Extract;

            hud.SetTargetExtract(requiredExtractCounts.count);

            for (int i = 0; i < herbToExtractThresholds.Count; i++)
            {
                if (herbToExtractThresholds[i].count < 999)
                {
                    for (int j = 0; j < hud.extractToNext.Count; j++)
                    {
                        if (hud.extractToNext[j].color == herbToExtractThresholds[i].color)
                            hud.extractToNext[j].SetCount(herbToExtractThresholds[i].count);//必须保证hud中的extractToNext与herbToExtractThresholds中的存在关系
                    }
                }
            }
        }



        void Update()
        {

        }
        public override void OnMove()
        {
            base.OnMove();
            foreach (var piece in grid.activeExtractCollection)
            {
                piece.IncrementExtractMoveCount();
                //Debug.Log("坐标为"+piece.X+","+piece.Y+"提取物移动次数" + piece.extractMoveCount);
            }

        }
        public override void JudgeWin()
        {
            base.JudgeWin();
            if (currentRequiredExtractCount == requiredExtractCounts.count)
            {
                GameWin();
                return;
            }
            if (movesUsed >= numMoves)
            {
                GameLose();
            }
        }
        public override void OnPieceCleared(GamePiece piece)
        {
            base.OnPieceCleared(piece);

        }
        public void OnHerbToExtract(ColorType color, int currentHerbToExtract)
        {
           // var herbToExtract = hud.extractToNext.Find(h => h.color == color);
            for (int i = 0; i < hud.extractToNext.Count; i++)
            {
                if (hud.extractToNext[i].color == color)
                {
                    int herbToExtractThreshold = 0;
                    for (int j = 0; j < herbToExtractThresholds.Count; j++)
                    {
                        if (herbToExtractThresholds[j].color == color)
                        {
                            herbToExtractThreshold = herbToExtractThresholds[j].count;
                        }
                    }
                    if (herbToExtractThreshold >= currentHerbToExtract)
                    {
                        if(currentHerbToExtract==0)
                        {
                            hud.extractToNext[i].SetCount(herbToExtractThreshold);
                        }
                        else
                        {
                            hud.extractToNext[i].SetCount(herbToExtractThreshold - currentHerbToExtract);
                        }
                    }
                    else
                    {
                        hud.extractToNext[i].SetCount(0);
                    }
                    return;
                }
            }
        }

        public override void OnExtractCleared(ColorType color)
        {
            if (color == requiredExtractCounts.color)
            {
                if (currentRequiredExtractCount < requiredExtractCounts.count)
                    currentRequiredExtractCount++;
                hud.SetTargetExtract(requiredExtractCounts.count - currentRequiredExtractCount);
            }
        }
    }
}
