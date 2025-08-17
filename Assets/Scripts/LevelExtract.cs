using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Match3.GameGrid;
namespace Match3
{
    public class LevelExtract : Level
    {

        public List<ColorClearCount> herbToExtractThresholds = new List<ColorClearCount>();//生成每种颜色精华的阈值
        public ColorClearCount requiredExtractCounts;//通关需要的目标精华的数量
        public int currentRequiredExtractCount = 0; // Current count of the required extract
        public List<PieceType> obstaclePieceTypesForFault = new List<PieceType>();//错误精华收集产生的障碍

        public override void Awake()
        {
            base.Awake();
            type = LevelType.Extract;

            //hud.SetTarget(requiredExtractCounts.count);

        }



        void Update()
        {

        }
        public override void OnMove()
        {
            base.OnMove();


        }
        public override void JudgeWin()
        {
            base.JudgeWin();


        }
        public override void OnPieceCleared(GamePiece piece)
        {
            base.OnPieceCleared(piece);

        }
        public void OnExtractCleared(ColorType color)
        {

        }
    }
}
