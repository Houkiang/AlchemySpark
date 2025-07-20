using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Match3.GameGrid;

namespace Match3
{
    public class Level : MonoBehaviour
    {

        public int numMoves;
        public List<ColorClearCount> herbClearThresholds = new List<ColorClearCount>();//生成每种颜色高级药水的阈值
        public List<ColorClearCount> requiredHerbCounts = new List<ColorClearCount>();//通关需要的每种颜色药水的
        [System.Serializable]
        public struct ScoreTierEffect
        {
            [Tooltip("分数阈值，达到此分数触发该档次效果")]
            public int scoreThreshold;
            [Tooltip("该档次的动画剪辑（AnimationClip，第一帧应设置有效 Sprite，最后一帧设为空 Sprite）")]
            public AnimationClip animationClip;
            [Tooltip("该档次的音效（AudioClip）")]
            public AudioClip audioClip;
        }

        public  GameGrid gameGrid;
        public Hud hud;

        [SerializeField] public int score1Star;
        [SerializeField] public int score2Star;
        [SerializeField] public int score3Star;

        [SerializeField]
        [Tooltip("分数档次及其对应效果")]
        private ScoreTierEffect[] scoreTiers = new ScoreTierEffect[3]
        {
            new ScoreTierEffect { scoreThreshold = 100 }, // 低档
            new ScoreTierEffect { scoreThreshold = 500 }, // 中档
            new ScoreTierEffect { scoreThreshold = 1000 } // 高档
        };

        [SerializeField]
        [Tooltip("音效播放音量")]
        private float audioVolume = 1f;

        [SerializeField]
        [Tooltip("动画的 SpriteRenderer 的 Sorting Layer（确保不被遮挡）")]
        private string effectSortingLayer = "Default";

        [SerializeField]
        [Tooltip("动画的 SpriteRenderer 的 Sorting Order（确保在棋盘上方）")]
        private int effectSortingOrder = 10;

        protected LevelType type;
        public int currentScore { get; protected set; }
        public int lastMoveScore { get; protected set; }

        private bool _didWin;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private int _idleStateHash;

        public virtual  void Awake()
        {
            // 确保 GameObject 有 Animator 组件
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
                //Debug.LogWarning("Animator component not found on GameObject. Added automatically.");
            }

            // 确保 GameObject 有 SpriteRenderer
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                //Debug.LogWarning("SpriteRenderer not found on GameObject. Added automatically.");
            }

            // 设置 SpriteRenderer 的 Sorting Layer 和 Order
            _spriteRenderer.sortingLayerName = effectSortingLayer;
            _spriteRenderer.sortingOrder = effectSortingOrder;

            // 确保 SpriteRenderer 始终启用
            _spriteRenderer.enabled = true;

            // 获取 Idle 状态的哈希 ID
            _idleStateHash = Animator.StringToHash("Idle");
        }

        private void Start()
        {
            hud.SetScore(currentScore);
        }

        public LevelType Type => type;

        public virtual void OnMove()
        {
            //Debug.Log("OnMove 触发：玩家移动");
            foreach (GamePiece piece in gameGrid.GetPiecesOfType(PieceType.SpecialElement))
            {
                if (piece.IsColored())
                {
                    if (piece.ColorComponent.Color == ColorType.Blue)
                    {
                        piece.IncrementBlueMoveCount();
                        //Debug.Log($"蓝色药水计数：位置=({piece.X},{piece.Y}), 计数={piece.blueMoveCount}");
                    }
                    else if (piece.ColorComponent.Color == ColorType.Green)
                    {
                        piece.IncrementGreenMoveCount();
                        //Debug.Log($"绿色药水计数：位置=({piece.X},{piece.Y}), 计数={piece.greenMoveCount}");
                    }
                }
            }

            // 检查分数并触发特殊效果
           // StartCoroutine(HandleSpecialEffects());

            // 重置单次移动分数
            lastMoveScore = 0;
        }

        public virtual void OnPieceCleared(GamePiece piece)
        {
            currentScore += piece.score;
            lastMoveScore += piece.score;
            hud.SetScore(currentScore);
        }

        protected virtual IEnumerator WaitForGridFill()
        {
            while (gameGrid.IsFilling)
            {
                yield return null;
            }

            if (_didWin)
            {
                hud.OnGameWin(currentScore);
            }
            else
            {
                hud.OnGameLose();
            }
        }

        public virtual void GameWin()
        {
            gameGrid.GameOver();
            _didWin = true;
            StartCoroutine(WaitForGridFill());
        }

        public virtual void GameLose()
        {
            gameGrid.GameOver();
            _didWin = false;
            StartCoroutine(WaitForGridFill());
        }

        public IEnumerator HandleSpecialEffects()
        {
            yield return new WaitForSeconds(0.1f) ;
            // 获取当前移动的分数
            int moveScore = lastMoveScore;
            //Debug.Log($"Current move score: {moveScore}");

            // 查找最高适用的分数档次
            ScoreTierEffect? selectedTier = null;
            foreach (var tie in scoreTiers.OrderByDescending(t => t.scoreThreshold))
            {
                if (moveScore >= tie.scoreThreshold)
                {
                    selectedTier = tie;
                    break;
                }
            }

            // 如果没有达到任何档次，直接返回
            if (selectedTier == null)
            {
                //Debug.Log("Score too low, no effect triggered.");
                yield break;
            }

            var tier = selectedTier.Value;
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
                //Debug.LogWarning("Animator component not found on GameObject. Added automatically.");
            }
           
            // 播放音效
            if (tier.audioClip != null)
            {
                AudioSource.PlayClipAtPoint(tier.audioClip, Camera.main.transform.position, audioVolume);
                //Debug.Log($"Played audio {tier.audioClip.name} for score {moveScore}");
            }
            else
            {
                //Debug.LogWarning($"Audio clip for score tier {tier.scoreThreshold} is null.");
            }
        }
        public virtual void Judgewin()
        {

        }
    }
}