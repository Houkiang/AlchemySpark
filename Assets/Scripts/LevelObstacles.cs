using System.Collections.Generic;

namespace Match3
{
    public class LevelObstacles : Level
    {
        public List<GamePiece> obstacleTargets = new List<GamePiece>();
        public override void Awake()
        {
            base.Awake();
            type = LevelType.Obstacles;

        }
        public override void Start()
        {
            base.Start();
            foreach (var piecePosition in grid.initialPieces)
            {
                GamePiece piece = grid._pieces[piecePosition.x, piecePosition.y];
                if (piece != null)
                {
                    obstacleTargets.Add(piece);
                }
            }
            hud.SetTargetObstacles(obstacleTargets.Count);
        }
        public override void OnPieceCleared(GamePiece piece)
        {
            base.OnPieceCleared(piece);
            foreach (var target in obstacleTargets)
            {
                if (target.X == piece.X && target.Y == piece.Y)
                {
                    obstacleTargets.Remove(target);
                    hud.SetTargetObstacles(obstacleTargets.Count);
                    return;
                }
            }
        }
        public override void JudgeWin()
        {
            base.JudgeWin();
            if (obstacleTargets.Count == 0)
            {
                hud.SetScore(currentScore);
                GameWin();
            }
            else if (movesUsed >= numMoves)
            {
                GameLose();
            }
        }
    
    }
}
