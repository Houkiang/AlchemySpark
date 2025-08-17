using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Match3.GameGrid;
namespace Match3
{
    public class LevelHerbs : Level
    {

        

        public ColorClearCount requiredHerbCounts; // Required herb counts for winning the level
        public int currentRequiredHerbCount = 0; // Current count of the required herb

        public override void Awake()
        {
            base.Awake();
            type = LevelType.Herbs;

            hud.SetTarget(requiredHerbCounts.count);

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
            foreach (var herb in grid.clearedHerbCounts)
            {
                if (herb.color == requiredHerbCounts.color && herb.count >= requiredHerbCounts.count)
                {

                    GameWin();
                    return;
                }
            }
            if (movesUsed >= numMoves)
            {
                GameLose();
            }

        }
        public override void OnPieceCleared(GamePiece piece)
        {
            base.OnPieceCleared(piece);
            if (piece.IsColored() && piece.ColorComponent.Color == requiredHerbCounts.color)
            {
                if(currentRequiredHerbCount>= requiredHerbCounts.count)
                {
                    return; // Already met the required herb count
                }
                else
                {
                    currentRequiredHerbCount++;
                }
                hud.SetTarget(requiredHerbCounts.count - currentRequiredHerbCount);
            }
        }
    }
}
