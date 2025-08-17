using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Match3.GameGrid;

namespace Match3
{
    public class AlchemyManager : MonoBehaviour
    {
        private Queue<AlchemyMatchResult> _effectQueue;
        private GameGrid _gameGrid;
        private bool _isInitialized = false;
        

        [Header("LEVEL")]
        public Level _level;
        public Hud _hud;

        // 特效预制件引用
        [Header("效果预制件")]
        public GameObject boomEffectPrefab;
        public GameObject scoreEffectPrefab;
        public GameObject moveEffectPrefab;

        [Header("药水预制件")]
        public GameObject boomElixir;
        public GameObject scoreElixir;
        public GameObject moveElixir;

        [Header("药水消失特效")]
        public GameObject elixirVanishEffect; // 药水消失时的通用特效

        [Header("基础得分药水加成")]
        public int basicScoreBounce;



        private int boomEffectType=0;


        // 药水相关变量
        private Queue<GameObject> _elixirQueue = new Queue<GameObject>();
        private Vector3 _screenBottomCenter = new Vector3(0, 4.7f, 0); // 屏幕底部中央位置
        private Vector3 _offScreenPosition = new Vector3(0, 15f, 0); // 屏幕外位置

        // 在EnqueueEffect中调用此方法来生成药水


        private GameObject GetElixirPrefab(AlchemyType type)
        {
            switch (type)
            {
                case AlchemyType.BoomClear: return boomElixir;
                case AlchemyType.ScoreAdder: return scoreElixir;
                case AlchemyType.MovesAdder: return moveElixir;
                default: return null;
            }
        }

        // 创建药水 - 从消除位置生成
        public void CreateElixir(AlchemyMatchResult result)
        {
            GameObject elixirPrefab = GetElixirPrefab(result.MatchedFormula.AlchemyType);
            if (elixirPrefab == null)
            {
                Debug.LogWarning("未找到对应的药水预制件");
                return;
            }

            // 获取消除位置（匹配的第一个方块位置）
            Vector3 spawnPosition = GetSpawnPosition(result);

            // 在消除位置生成药水
            GameObject newElixir = Instantiate(elixirPrefab, spawnPosition, Quaternion.identity);

            // 开始移动协程（飞出屏幕）
            StartCoroutine(MoveElixirOutOfScreen(newElixir, spawnPosition));
        }

        // 获取药水生成位置（匹配的第一个方块位置）
        private Vector3 GetSpawnPosition(AlchemyMatchResult result)
        {
            if (result.MatchedPieces != null && result.MatchedPieces.Count > 0)
            {
                GamePiece firstPiece = result.MatchedPieces[0];
                return _gameGrid.GetWorldPosition(firstPiece.X, firstPiece.Y);
            }

            // 如果没有匹配的方块，使用棋盘中心位置
            return _gameGrid.GetWorldPosition(_gameGrid.xDim / 2, _gameGrid.yDim / 2);
        }

        // 药水飞出屏幕
        private IEnumerator MoveElixirOutOfScreen(GameObject elixir, Vector3 spawnPosition)
        {
            float duration = 0.8f;
            float elapsed = 0f;
            Vector3 startPos = spawnPosition;
            Vector3 targetPos = new Vector3(spawnPosition.x, _offScreenPosition.y, spawnPosition.z);

            //加入队列
            _elixirQueue.Enqueue(elixir);
            //Debug.Log($"药水加入队列，当前数量: {_elixirQueue.Count}");
            while (elapsed < duration)
            {
                if (elixir == null)
                {
                    yield break; // 如果药水被销毁，退出协程
                }
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                elixir.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

       
        }

        // 消耗药水
        public IEnumerator ConsumeElixir(AlchemyMatchResult result, Vector2 location)
        {
            if (_elixirQueue.Count == 0)
            {
                Debug.LogWarning("没有可消耗的药水");
                yield break;
            }
            
            GameObject elixir = _elixirQueue.Dequeue();

            // 将药水移动到屏幕底部中央（飞入起点）
            elixir.transform.position = _screenBottomCenter;
            elixir.SetActive(true); // 确保药水可见

            // 移动药水到效果中心
            yield return StartCoroutine(MoveElixirToEffect(elixir, location));
        }

        private IEnumerator MoveElixirToEffect(GameObject elixir, Vector3 targetPosition)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 startPos = elixir.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                elixir.transform.position = Vector3.Lerp(startPos, targetPosition, t);
                yield return null;
            }

            // 播放消失特效
            if (elixirVanishEffect != null)
            {
                GameObject effect = Instantiate(
                    elixirVanishEffect,
                    targetPosition,
                    Quaternion.identity);
                Destroy(effect, 1.5f);
            }

            // 销毁药水
            Destroy(elixir);
            //Debug.Log("药水已消耗并销毁");
        }





        // 初始化方法
        public void Initialize(GameGrid gameGrid)
        {
            if (_isInitialized) return;

            _gameGrid = gameGrid;
            _effectQueue = new Queue<AlchemyMatchResult>();
            _isInitialized = true;

            //Debug.Log("炼金管理器初始化完成");
        }

        // 添加效果到队列
        public void EnqueueEffect(AlchemyMatchResult result)
        {
            if (!_isInitialized)
            {
                Debug.LogError("炼金管理器未初始化");
                return;
            }
            
            _effectQueue.Enqueue(result);
            CreateElixir(result);
            //Debug.Log($"炼金效果入队: {result.MatchedFormula.AlchemyType}");
        }

        // 处理效果队列
        public IEnumerator ProcessEffectQueue()
        {
            if (!_isInitialized || _gameGrid == null)
            {
                Debug.LogError("无法处理效果队列 - 管理器未初始化");
                yield break;
            }

            //Debug.Log($"开始处理炼金效果队列 ({_effectQueue.Count} 个效果)");

            while (_effectQueue.Count > 0)
            {
                AlchemyMatchResult result = _effectQueue.Dequeue();
                yield return ExecuteAlchemyEffect(result);
            }

           // Debug.Log("炼金效果队列处理完成");
        }
        // 执行单个效果
        public IEnumerator ExecuteAlchemyEffect(AlchemyMatchResult result)
        {
            if (result == null)
            {
                Debug.LogError("无效的炼金效果结果");
                yield break;
            }

            Debug.Log($"执行炼金效果: {result.MatchedFormula.AlchemyType}");

            switch (result.MatchedFormula.AlchemyType)
            {
                case AlchemyType.BoomClear:
                    yield return ExecuteBoomEffect(result);
                    break;

                case AlchemyType.ScoreAdder:
                    yield return 0;//ExecuteScoreEffect(result);
                    break;
                case AlchemyType.MovesAdder:
                    yield return ExecuteMoveEffect(result);
                    break;
                default:
                    Debug.LogWarning($"未知的炼金类型: {result.MatchedFormula.AlchemyType}");
                    break;
            }
        }
        //步数增加效果
        private IEnumerator ExecuteMoveEffect(AlchemyMatchResult result)
        {
            // 1. 查找父对象 "Game UI Canvas"
            GameObject canvas = GameObject.Find("Game UI Canvas");
            if (canvas == null)
            {
                Debug.LogError("找不到 'Game UI Canvas' 对象！");
                
            }

            // 2. 在父对象下查找子对象 "Move"
            Transform MoveTransform = canvas.transform.Find("remaining movetime/remaining_bg");
            if (MoveTransform == null)
            {
                Debug.LogError("在Canvas下找不到 'remaining_bg' 子对象！");
               
            }

            // 3. 获取RectTransform组件
            RectTransform MoveRectTransform = MoveTransform.GetComponent<RectTransform>();
            if (MoveRectTransform == null)
            {
                Debug.LogError("'Move' 对象上无RectTransform组件！");
                
            }
           //Debug.Log("Move位置x="+ MoveTransform.position.x+"；y="+ MoveTransform.position.y);

            // 计算爆炸中心
            //Vector2 center = Vector2.zero;
            
            //center = CenterCompute(result, boomEffectType);
            // 获取爆炸中心的世界坐标
            Vector3 worldCenter = new Vector3(-9.5f,4.5f,0);
            // 消耗药水并移动到爆炸中心
            yield return StartCoroutine(ConsumeElixir(result, worldCenter));


            // 播放特效
            GameObject effect = null;
            if (moveEffectPrefab != null)
            {
                effect = Instantiate(moveEffectPrefab,
                    new Vector2(worldCenter.x, worldCenter.y),
                    Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("未分配MOVE特效预制件");
            }
            int bounce = 0;
            bounce += (result.MatchLength - 2);
            _level.movesUsed -= bounce;
            _hud.SetRemaining(_level.numMoves - _level.movesUsed);
            //LevelNumber _levelNumber=_level as LevelNumber;
            //_levelNumber._movesUsed -= bounce;
            //Debug.Log("_level.numMoves=" + _level.numMoves + "_levelNumber._movesUsed" + _levelNumber._movesUsed);
            //_hud.SetRemaining(_level.numMoves - _levelNumber._movesUsed);
            // 等待特效完成
            yield return new WaitForSeconds(0.5f);

            if (effect != null)
            {
                Destroy(effect);
            }
        }




        // 爆炸效果
        private IEnumerator ExecuteBoomEffect(AlchemyMatchResult result)
        {
            // 计算爆炸中心
            Vector2 center = Vector2.zero;
            boomEffectType++;
            boomEffectType %= 4;


            center = CenterCompute(result, boomEffectType);

            // 获取爆炸中心的世界坐标
            Vector3 worldCenter = _gameGrid.GetWorldPosition((int)center.x, (int)center.y);

            // 消耗药水并移动到爆炸中心
            yield return StartCoroutine(ConsumeElixir(result, worldCenter));

            Debug.Log("center=" + center + "  Type" + boomEffectType);
            // 播放特效
            GameObject effect = null;
            if (boomEffectPrefab != null)
            {
                effect = Instantiate(boomEffectPrefab,
                    _gameGrid.GetWorldPosition((int)center.x, (int)center.y),
                    Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("未分配爆炸特效预制件");
            }

            // 计算爆炸范围
            int radius = Mathf.Clamp(result.MatchLength / 2, 1, 3);

            // 执行爆炸清除
            List<Vector2Int> positionsToClear = new List<Vector2Int>();

            for (int x = (int)center.x - radius; x <= (int)center.x + radius; x++)
            {
                for (int y = (int)center.y - radius; y <= (int)center.y + radius; y++)
                {
                    if (x >= 0 && x < _gameGrid.xDim && y >= 0 && y < _gameGrid.yDim)
                    {
                        positionsToClear.Add(new Vector2Int(x, y));
                    }
                }
            }

            // 清除所有位置
            foreach (var pos in positionsToClear)
            {
                _gameGrid.ClearPiece(pos.x, pos.y);
            }

            // 等待特效完成
            yield return new WaitForSeconds(1.6f);

            if (effect != null)
            {
                Destroy(effect);
            }
        }

        private Vector2 CenterCompute(AlchemyMatchResult result,int type)
        {
            int X1= result.SpecialBackground.startX;
            int Y1= result.SpecialBackground.startY;
            int X2 = result.SpecialBackground.endX;
            int Y2 = result.SpecialBackground.endY;


            switch (type) 
            {
                case 0:
                    return new Vector2((float)(X1*1.5-X2*0.5-1),(float)(Y1*0.5+Y2*0.5));
                    
                case 1:
                    return new Vector2 ((float)(X2*1.5-X1*0.5+1),(float)(Y2*0.5+Y1*0.5));
                    
                case 2:
                    return new Vector2  ((float)(X1*0.5+X2*0.5),(float)(Y1*1.5-Y2*0.5-1));
                case 3:
                    return new Vector2 ((float )(X2*0.5+X1*0.5), (float)(Y2*1.5-Y1*0.5+1));
                default:
                    return Vector2.zero;
            }
        }


        public int GetQueueCount()
        {
            return _effectQueue?.Count ?? 0;
        }
    }
}
