using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Match3
{
    public class GameGrid : MonoBehaviour
    {
        [System.Serializable]
        public struct PiecePrefab
        {
            public PieceType type;
            public GameObject prefab;
        }

        [System.Serializable]
        public struct PiecePosition
        {
            public PieceType type;
            public int x;
            public int y;
        }

        [System.Serializable]
        public struct ColorClearCount
        {
            public ColorType color;
            public int count;
            // Custom constructor
            public ColorClearCount(ColorType color, int count)
            {
                this.color = color;
                this.count = count;
            }
        }

        public int xDim;
        public int yDim;
        public float fillTime;

        public Level level;
        [SerializeField] private PiecePrefab[] piecePrefabs;
        [SerializeField] private GameObject backgroundPrefab;
        [SerializeField] private PiecePosition[] initialPieces;
        [SerializeField] private SpecialBackground[] specialBackgrounds;
        [SerializeField] private GameObject specialBackgroundPrefab;

        [SerializeField] private int minY = 2;
        public List<ColorClearCount> clearedHerbCounts = new List<ColorClearCount>();
        public List<ColorClearCount> collectedHerbCounts = new List<ColorClearCount>();
        [SerializeField] private int obstacleWidth = 2;
        [SerializeField] private int obstacleHeight = 2;


        public class AlchemyMatchResult
        {
            public SpecialBackground SpecialBackground;
            public SpecialBackground.AlchemyFormula MatchedFormula;
            public List<GamePiece> MatchedPieces;
            public int MatchLength;
        }

        private GameState _currentState = GameState.PlayerInput;
        private Queue<AlchemyMatchResult> _effectQueue = new Queue<AlchemyMatchResult>();
        private AlchemyManager _alchemyManager;
        private List<SpecialBackground> activeSpecialBackgrounds = new List<SpecialBackground>();
        private Dictionary<PieceType, GameObject> _piecePrefabDict;
        public GamePiece[,] _pieces;
        private bool _inverse;
        private GamePiece _pressedPiece;
        private GamePiece _enteredPiece;
        private bool _gameOver;
        public bool IsFilling { get; private set; }

        private void Awake()
        {
            //InitializeClearedHerbCounts();

            _piecePrefabDict = new Dictionary<PieceType, GameObject>();
            for (int i = 0; i < piecePrefabs.Length; i++)
            {
                if (!_piecePrefabDict.ContainsKey(piecePrefabs[i].type))
                {
                    _piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
                }
            }

            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    GameObject background = Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                    background.transform.parent = transform;
                }
            }

            for (int i = 0; i < specialBackgrounds.Length; i++)
            {
                SpecialBackground sb = specialBackgrounds[i];
                activeSpecialBackgrounds.Add(sb);
                for (int x = sb.startX; x <= sb.endX; x++)
                {
                    for (int y = sb.startY; y <= sb.endY; y++)
                    {
                        GameObject specialBg = Instantiate(specialBackgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                        specialBg.transform.parent = transform;
                    }
                }
            }

            _pieces = new GamePiece[xDim, yDim];
            for (int i = 0; i < initialPieces.Length; i++)

            {
                if (initialPieces[i].x >= 0 && initialPieces[i].x < xDim && initialPieces[i].y >= 0 && initialPieces[i].y < yDim)
                {
                    SpawnNewPiece(initialPieces[i].x, initialPieces[i].y, initialPieces[i].type);
                }
            }

            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (_pieces[x, y] == null)
                    {
                        SpawnNewPiece(x, y, PieceType.Empty);
                    }
                }
            }

            _alchemyManager = GetComponent<AlchemyManager>();
  
                if (_alchemyManager == null)
                {
                    // 尝试在整个场景中查找
                    _alchemyManager = FindObjectOfType<AlchemyManager>(true);

                    
                }
            
            _alchemyManager.Initialize(this);
        }

        private void Start()
        {
            StartCoroutine(GameLoop());
        }

        private void Update()
        {
            Debug.Log(_currentState);
        }

        //private void InitializeClearedHerbCounts()
        //{
        //    foreach (ColorType color in Enum.GetValues(typeof(ColorType)))/
        //    {
        //        if (color != ColorType.Any)
        //        {
        //            clearedHerbCounts.Add(new ColorClearCount { color = color, count = 0 });
        //            collectedHerbCounts.Add(new ColorClearCount { color = color, count = 0 });
        //        }
        //    }
        //}

        private void IncrementClearedHerbCount(ColorType color)
        {
            if (color == ColorType.Any) return;
            for (int i = 0; i < clearedHerbCounts.Count; i++)
            {
                ColorClearCount count = clearedHerbCounts[i];
                if (count.color == color)
                {
                    count.count++;
                    clearedHerbCounts[i] = count;
                    break;
                }
            }
        }

        private void IncrementCollectedHerbCount(ColorType color)
        {
            if (color == ColorType.Any) return;
            for (int i = 0; i < collectedHerbCounts.Count; i++)
            {
                ColorClearCount count = collectedHerbCounts[i];
                if (count.color == color)
                {
                    count.count++;
                    collectedHerbCounts[i] = count;
                    break;
                }
            }
        }

        private IEnumerator GameLoop()
        {
            _currentState = GameState.Filling;
            while (!_gameOver)
            {
                switch (_currentState)
                {
                    case GameState.PlayerInput:
                        
                        yield return HandlePlaerInputState();
                        
                        break;
                    case GameState.Swapping:
                        yield return HandleSwappingState();
                        break;
                    case GameState.NormalClearing:
                        yield return HandleNormalClearingState();
                        break;
                    case GameState.AlchemyClearing:
                        yield return HandleAlchemyClearingState();
                      
                        break;
                    case GameState.Filling:
                        yield return HandleFillingState();
                        break;
                    case GameState.AlchemyEffect:
                        yield return HandleAlchemyEffectState();
                        break;
                }
            }
        }
        private IEnumerator HandlePlaerInputState()
        {
            
            level.Judgewin();
            yield return null;
        }
        private IEnumerator HandleSwappingState()
        {

            // 执行交换
            GamePiece piece1 = _pressedPiece;
            GamePiece piece2 = _enteredPiece;
            SwapPieces(piece1, piece2);
            ResetSelection();
            // 等待交换动画完成
            yield return new WaitForSeconds(0);
            // 临时交换棋子位置进行检测
            _pieces[piece1.X, piece1.Y] = piece2;
            _pieces[piece2.X, piece2.Y] = piece1;
            List<GamePiece> normalMatch = null;
            bool hasRainbowEffect = false;
            hasRainbowEffect = (piece1.Type == PieceType.Rainbow || piece2.Type == PieceType.Rainbow);
            // 移动动画
            int piece1X = piece1.X;
            int piece1Y = piece1.Y;
            int piece2X = piece2.X;
            int piece2Y = piece2.Y;

            // 检测匹配类型
            if (CheckAlchemyMatchAfterSwap(piece1, piece2) != null)
            {

                piece1.MovableComponent.Move(piece2X, piece2Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                //Debug.Log("炼金");
                _currentState = GameState.AlchemyClearing;
                
                level.OnMove();
            }
            else
            {
                normalMatch = GetMatch(piece1, piece2.X, piece2.Y);
                if (normalMatch == null || normalMatch.Count < 3)
                {
                    normalMatch = GetMatch(piece2, piece1.X, piece1.Y);
                }
                if (normalMatch != null|| hasRainbowEffect)
                {

                    // 移动动画

                    piece1.MovableComponent.Move(piece2X, piece2Y, fillTime);
                    piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                    //Debug.Log("普通");
                    _currentState = GameState.NormalClearing;

                    //彩虹检测
                    ClearRainbow(piece1, piece2);
                    level.OnMove();
                }
               
                else
                {
                    _pieces[piece1.X, piece1.Y] = piece1;
                    _pieces[piece2.X, piece2.Y] = piece2;
                    //Debug.Log("没有匹配");
                    _currentState = GameState.PlayerInput;
                }
            }


        }
        private IEnumerator HandleNormalClearingState()
        {
            ClearAllValidMatches();
            yield return new WaitForSeconds(0.05f);
            _currentState = GameState.Filling;
        }

        private IEnumerator HandleAlchemyClearingState()
        {
            var alchemyMatches = CheckAllAlchemyMatches();
            if (alchemyMatches != null && alchemyMatches.Count > 0)
            {
                foreach (var match in alchemyMatches)
                {
                    foreach (var piece in match.MatchedPieces)
                    {
                        if (piece != null && piece.IsClearable())
                        {
                            ClearPiece(piece.X, piece.Y);
                        }
                    }
                    _alchemyManager.EnqueueEffect(match);
                }
            }
            yield return new WaitForSeconds(0.05f);
            _currentState = GameState.Filling;
        }

        private IEnumerator HandleFillingState()
        {
            //CheckBottomPotions();
            //Debug.Log("Fill即将进行");
            yield return StartCoroutine(Fill());
           // Debug.Log("fill未实现");
            if (CheckForNormalMatch())
            {
                _currentState = GameState.NormalClearing;
            }
            else if (CheckForAlchemyMatch())
            {
                _currentState = GameState.AlchemyClearing;
            }
            else if (CheckBluePotionSwaps() || CheckGreenPotionConversions() || CheckBottomPotions())
            {
                _currentState = GameState.Filling;
            }
            else if (_alchemyManager.GetQueueCount() > 0)
            {
                _currentState = GameState.AlchemyEffect;
            }
            else
            {
                _currentState = GameState.PlayerInput;
            }
        }

        private IEnumerator HandleAlchemyEffectState()
        {
            //Debug.Log("未执行Process");
            yield return _alchemyManager.ProcessEffectQueue();
            //Debug.Log("已执行完Process");
            if (HasEmptySpaces())
            {
                _currentState = GameState.Filling;
            }
            else if (_alchemyManager.GetQueueCount() > 0)
            {
                _currentState = GameState.AlchemyEffect;
            }
            else
            {
                _currentState = GameState.PlayerInput;
            }
        }

        private bool CheckBluePotionSwaps()
        {
            bool modified = false;
            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    GamePiece piece = _pieces[x, y];
                    if (piece == null || piece.Type != PieceType.SpecialElement || !piece.IsColored() ||
                        piece.ColorComponent.Color != ColorType.Blue || piece.blueMoveCount < 2)
                        continue;

                    piece.ResetBlueMoveCount();
                    if (y == 0 || _pieces[x, y - 1] == null || _pieces[x, y - 1].Type == PieceType.Empty ||
                        _pieces[x, y - 1].Type.ToString().Contains("impurity"))
                        continue;

                    GamePiece abovePiece = _pieces[x, y - 1];
                    _pieces[x, y] = abovePiece;
                    _pieces[x, y - 1] = piece;
                    piece.MovableComponent.Move(x, y - 1, fillTime);
                    abovePiece.MovableComponent.Move(x, y, fillTime);
                    modified = true;
                }
            }
            return modified;
        }

        private bool CheckGreenPotionConversions()
        {
            bool modified = false;
            for (int y = 0; y <= yDim - minY - 1; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    GamePiece piece = _pieces[x, y];
                    if (piece == null || piece.Type != PieceType.SpecialElement || !piece.IsColored() ||
                        piece.ColorComponent.Color != ColorType.Green || piece.greenMoveCount < 3)
                        continue;

                    piece.ResetGreenMoveCount();
                    List<GamePiece> validPieces = new List<GamePiece>();
                    for (int ty = 0; ty <= yDim - minY - 1; ty++)
                    {
                        for (int tx = 0; tx < xDim; tx++)
                        {
                            GamePiece target = _pieces[tx, ty];
                            if (target != null && target.Type != PieceType.Empty && target.Type != PieceType.SpecialElement)
                                validPieces.Add(target);
                        }
                    }

                    if (validPieces.Count == 0) continue;

                    GamePiece targetPiece = validPieces[UnityEngine.Random.Range(0, validPieces.Count)];
                    int targetX = targetPiece.X;
                    int targetY = targetPiece.Y;
                    Destroy(targetPiece.gameObject);
                    GamePiece newPiece = SpawnNewPiece(targetX, targetY, PieceType.SpecialElement);
                    if (newPiece.IsColored())
                        newPiece.ColorComponent.SetColor(ColorType.Green);
                    modified = true;
                }
            }
            return modified;
        }

        private bool GenerateWhitePotionObstacles(int x, int y)
        {
            bool modified = false;
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };
            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx < 0 || nx >= xDim || ny < 0 || ny >= yDim || (_pieces[nx, ny] != null && _pieces[nx, ny].Type == PieceType.SpecialElement))
                    continue;

                if (_pieces[nx, ny] != null && _pieces[nx, ny].Type != PieceType.Empty)
                    Destroy(_pieces[nx, ny].gameObject);
                SpawnNewPiece(nx, ny, PieceType.impurity1);
                modified = true;
            }
            return modified;
        }

        private IEnumerator Fill()
        {
            bool needsRefill = true;
            IsFilling = true;
            while (needsRefill)
            {
                yield return new WaitForSeconds(fillTime);
                while (FillStep())
                {
                    _inverse = !_inverse;
                    yield return new WaitForSeconds(fillTime);
                }
                needsRefill = ClearAllValidMatches();
            }
            IsFilling = false;
        }

        private bool FillStep()
        {
            bool movedPiece = false;
            for (int y = yDim - 2; y >= 0; y--)
            {
                for (int loopX = 0; loopX < xDim; loopX++)
                {
                    int x = _inverse ? xDim - 1 - loopX : loopX;
                    GamePiece piece = _pieces[x, y];
                    if (!piece.IsMovable()) continue;

                    GamePiece pieceBelow = _pieces[x, y + 1];
                    if (pieceBelow.Type == PieceType.Empty)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, fillTime);
                        _pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.Empty);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag += 2)
                        {
                            int diagX = _inverse ? x - diag : x + diag;
                            if (diagX < 0 || diagX >= xDim) continue;

                            GamePiece diagonalPiece = _pieces[diagX, y + 1];
                            if (diagonalPiece.Type != PieceType.Empty) continue;

                            bool hasPieceAbove = true;
                            for (int aboveY = y; aboveY >= 0; aboveY--)
                            {
                                GamePiece pieceAbove = _pieces[diagX, aboveY];
                                if (pieceAbove.IsMovable()) break;
                                if (pieceAbove.Type != PieceType.Empty)
                                {
                                    hasPieceAbove = false;
                                    break;
                                }
                            }
                            if (hasPieceAbove) continue;

                            Destroy(diagonalPiece.gameObject);
                            piece.MovableComponent.Move(diagX, y + 1, fillTime);
                            _pieces[diagX, y + 1] = piece;
                            SpawnNewPiece(x, y, PieceType.Empty);
                            movedPiece = true;
                            break;
                        }
                    }
                }
            }

            for (int x = 0; x < xDim; x++)
            {
                GamePiece pieceBelow = _pieces[x, 0];
                if (pieceBelow.Type != PieceType.Empty) continue;

                Destroy(pieceBelow.gameObject);
                GameObject newPiece = Instantiate(_piecePrefabDict[PieceType.Normal], GetWorldPosition(x, -1), Quaternion.identity, transform);
                _pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                _pieces[x, 0].Init(x, -1, this, PieceType.Normal);
                _pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                _pieces[x, 0].ColorComponent.SetColor((ColorType)UnityEngine.Random.Range(0, _pieces[x, 0].ColorComponent.NumColors));
                movedPiece = true;
            }
            return movedPiece;
        }

        public Vector2 GetWorldPosition(int x, int y)
        {
            return new Vector2(transform.position.x - xDim / 2.0f + x, transform.position.y + yDim / 2.0f - y);
        }

        private GamePiece SpawnNewPiece(int x, int y, PieceType type)
        {
            GameObject newPiece = Instantiate(_piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity, transform);
            _pieces[x, y] = newPiece.GetComponent<GamePiece>();
            _pieces[x, y].Init(x, y, this, type);
            return _pieces[x, y];
        }

        private static bool IsAdjacent(GamePiece piece1, GamePiece piece2) =>
            piece1 != null && piece2 != null &&
            ((piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1) ||
             (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1));

        private void SwapPieces(GamePiece piece1, GamePiece piece2)
        {
            if (!piece1.IsMovable() || !piece2.IsMovable()) return;
            //Debug.Log("piece1.X=" + piece1.X + " piece1.Y+" + piece1.Y);
            //Debug.Log("piece2.X=" + piece2.X + " piece2.Y+" + piece2.Y);
            return;
        }
        public void PressPiece(GamePiece piece)
        {
            if (_currentState != GameState.PlayerInput || _gameOver) return;
            _pressedPiece = piece;
        }

        public void EnterPiece(GamePiece piece)
        {
            if (_currentState != GameState.PlayerInput || _gameOver) return;
            _enteredPiece = piece;
        }

        public void ReleasePiece()
        {
            if (_currentState != GameState.PlayerInput || _gameOver || _pressedPiece == null || _enteredPiece == null)
            {
                ResetSelection();
                return;
            }
            if (IsAdjacent(_pressedPiece, _enteredPiece))
                _currentState = GameState.Swapping;
           // ResetSelection();
        }

        private void ResetSelection()
        {
            _pressedPiece = null;
            _enteredPiece = null;
        }

        private bool ClearAllValidMatches()
        {
            bool needsRefill = false;
            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    if (!_pieces[x, y].IsClearable()) continue;

                    List<GamePiece> match = GetMatch(_pieces[x, y], x, y);
                    if (match == null) continue;

                    PieceType specialPieceType = PieceType.Count;
                    int specialPieceX = match[0].X;
                    int specialPieceY = match[0].Y;

                    // Spawning special pieces
                    if (match.Count == 4)
                    {
                        if (_pressedPiece == null || _enteredPiece == null)
                        {
                            specialPieceType = (PieceType)UnityEngine.Random.Range((int)PieceType.RowClear, (int)PieceType.ColumnClear+1);
                        }
                        else if (_pressedPiece.Y == _enteredPiece.Y)
                        {
                            specialPieceType = PieceType.RowClear;
                        }
                        else
                        {
                            specialPieceType = PieceType.ColumnClear;
                        }
                    }
                    else if (match.Count >= 5)
                    {
                        specialPieceType = PieceType.Rainbow;
                    }

                    foreach (var gamePiece in match)
                    {
                        if (ClearPiece(gamePiece.X, gamePiece.Y))
                        {
                            needsRefill = true;
                            if (gamePiece == _pressedPiece || gamePiece == _enteredPiece)
                            {
                                specialPieceX = gamePiece.X;
                                specialPieceY = gamePiece.Y;
                            }
                        }
                    }

                    if (specialPieceType != PieceType.Count)
                    {
                        Destroy(_pieces[specialPieceX, specialPieceY]);
                        GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, specialPieceType);
                        if ((specialPieceType == PieceType.RowClear || specialPieceType == PieceType.ColumnClear) &&
                            newPiece.IsColored() && match[0].IsColored())
                        {
                            newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                        }
                        else if (specialPieceType == PieceType.Rainbow && newPiece.IsColored())
                        {
                            newPiece.ColorComponent.SetColor(ColorType.Any);
                        }
                    }

                    CheckAndGenerateSpecialElement(match);

                }
            }
            return needsRefill;
        }
        private void ClearRainbow(GamePiece piece1,GamePiece piece2)
        {
            {
                if (piece1.Type == PieceType.Rainbow && piece1.IsClearable() && piece2.IsColored())
                {
                    ClearColorPiece clearColor = piece1.GetComponent<ClearColorPiece>();
                    if (clearColor)
                    {
                        clearColor.Color = piece2.ColorComponent.Color;
                    }
                    ClearPiece(piece1.X, piece1.Y);
                }

                if (piece2.Type == PieceType.Rainbow && piece2.IsClearable() && piece1.IsColored())
                {
                    ClearColorPiece clearColor = piece2.GetComponent<ClearColorPiece>();
                    if (clearColor)
                    {
                        clearColor.Color = piece1.ColorComponent.Color;
                    }
                    ClearPiece(piece2.X, piece2.Y);
                }
            }
        }


        private void CheckAndGenerateSpecialElement(List<GamePiece> match)
        {
            GamePiece firstPiece = match[0];
            int specialX = firstPiece.X;
            int specialY = firstPiece.Y;
            ColorType matchColor = firstPiece.IsColored() ? firstPiece.ColorComponent.Color : ColorType.Any;

            LevelNumber levelNumber = level as LevelNumber;
            if (levelNumber != null && levelNumber.herbClearThresholds != null)
            {
                var herbThreshold = levelNumber.herbClearThresholds.Find(h => h.color == matchColor);
                if (herbThreshold.color == matchColor)
                {
                    var clearedHerb = clearedHerbCounts.Find(c => c.color == matchColor);
                    if (clearedHerb.count >= herbThreshold.count)
                    {
                        Destroy(_pieces[specialX, specialY].gameObject);
                        GamePiece newPiece = SpawnNewPiece(specialX, specialY, PieceType.SpecialElement);
                        if (newPiece.IsColored())
                        {
                            newPiece.ColorComponent.SetColor(matchColor);
                            if (matchColor == ColorType.White && GenerateWhitePotionObstacles(specialX, specialY))
                                StartCoroutine(Fill());
                        }
                        for (int i = 0; i < clearedHerbCounts.Count; i++)
                        {
                            var herbCount = clearedHerbCounts[i];
                            if (herbCount.color == matchColor)
                            {
                                herbCount.count = 0;
                                clearedHerbCounts[i] = herbCount;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool CheckBottomPotions()
        {
            bool modified = false;
            LevelNumber levelNumber = level as LevelNumber;
            if (levelNumber == null || levelNumber.requiredHerbCounts == null) return false;

            for (int x = 0; x < xDim; x++)
            {
                for (int y = yDim - minY; y < yDim; y++)
                {
                    GamePiece piece = _pieces[x, y];
                    if (piece == null || piece.Type != PieceType.SpecialElement || !piece.IsColored()) continue;

                    ColorType color = piece.ColorComponent.Color;
                    var requiredHerb = levelNumber.requiredHerbCounts.Find(r => r.color == color);
                    bool isRequiredColor = requiredHerb.color == color;

                    if (!levelNumber.CheckAllCleared())
                    {
                        if (!isRequiredColor)
                        {
                            Destroy(piece.gameObject);
                            GenerateObstacle(x, y);
                            modified = true;
                        }
                        else
                        {
                            ClearPiece(x, y);
                            modified = true;
                        }
                    }
                    else if (isRequiredColor)
                    {
                        ClearPiece(x, y);
                        modified = true;
                    }
                }
            }
            return modified;
        }

        private void GenerateObstacle(int startX, int startY)
        {
            LevelNumber levelNumber = level as LevelNumber;
            if (levelNumber == null || levelNumber.obstaclePieceTypes.Count == 0) return;

            PieceType selectedType = levelNumber.obstaclePieceTypes[UnityEngine.Random.Range(0, levelNumber.obstaclePieceTypes.Count)];
            for (int dx = 0; dx < obstacleWidth; dx++)
            {
                for (int dy = 0; dy < obstacleHeight; dy++)
                {
                    int x = startX + dx;
                    int y = startY + dy;
                    if (x >= 0 && x < xDim && y >= 0 && y < yDim)
                    {
                        Destroy(_pieces[x, y].gameObject);
                        SpawnNewPiece(x, y, selectedType);
                    }
                }
            }
        }

        private List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
        {
            if (!piece.IsColored()) return null;
            var color = piece.ColorComponent.Color;
            var horizontalPieces = new List<GamePiece>();
            var verticalPieces = new List<GamePiece>();
            var matchingPieces = new List<GamePiece>();

            horizontalPieces.Add(piece);
            for (int dir = 0; dir <= 1; dir++)
            {
                for (int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x = dir == 0 ? newX - xOffset : newX + xOffset;
                    if (x < 0 || x >= xDim) break;
                    if (_pieces[x, newY].IsColored() && _pieces[x, newY].ColorComponent.Color == color)
                        horizontalPieces.Add(_pieces[x, newY]);
                    else
                        break;
                }
            }

            if (horizontalPieces.Count >= 3)
                matchingPieces.AddRange(horizontalPieces);

            if (horizontalPieces.Count >= 3)
            {
                for (int i = 0; i < horizontalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int yOffset = 1; yOffset < yDim; yOffset++)
                        {
                            int y = dir == 0 ? newY - yOffset : newY + yOffset;
                            if (y < 0 || y >= yDim) break;
                            if (_pieces[horizontalPieces[i].X, y].IsColored() && _pieces[horizontalPieces[i].X, y].ColorComponent.Color == color)
                                verticalPieces.Add(_pieces[horizontalPieces[i].X, y]);
                            else
                                break;
                        }
                    }
                    if (verticalPieces.Count >= 2)
                    {
                        matchingPieces.AddRange(verticalPieces);
                        break;
                    }
                    verticalPieces.Clear();
                }
            }

            if (matchingPieces.Count >= 3)
                return matchingPieces;

            horizontalPieces.Clear();
            verticalPieces.Clear();
            verticalPieces.Add(piece);
            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y = dir == 0 ? newY - yOffset : newY + yOffset;
                    if (y < 0 || y >= yDim) break;
                    if (_pieces[newX, y].IsColored() && _pieces[newX, y].ColorComponent.Color == color)
                        verticalPieces.Add(_pieces[newX, y]);
                    else
                        break;
                }
            }

            if (verticalPieces.Count >= 3)
                matchingPieces.AddRange(verticalPieces);

            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    for (int dir = 0; dir <= 1; dir++)
                    {
                        for (int xOffset = 1; xOffset < xDim; xOffset++)
                        {
                            int x = dir == 0 ? newX - xOffset : newX + xOffset;
                            if (x < 0 || x >= xDim) break;
                            if (_pieces[x, verticalPieces[i].Y].IsColored() && _pieces[x, verticalPieces[i].Y].ColorComponent.Color == color)
                                horizontalPieces.Add(_pieces[x, verticalPieces[i].Y]);
                            else
                                break;
                        }
                    }
                    if (horizontalPieces.Count >= 2)
                    {
                        matchingPieces.AddRange(horizontalPieces);
                        break;
                    }
                    horizontalPieces.Clear();
                }
            }

            return matchingPieces.Count >= 3 ? matchingPieces : null;
        }

        private List<AlchemyMatchResult> CheckAllAlchemyMatches()
        {
            List<AlchemyMatchResult> matches = new List<AlchemyMatchResult>();
            foreach (SpecialBackground bg in activeSpecialBackgrounds)
            {
                for (int y = bg.startY; y <= bg.endY; y++)
                {
                    var match = CheckLineForAlchemy(bg, y, true);
                    if (match != null) matches.Add(match);
                }
                for (int x = bg.startX; x <= bg.endX; x++)
                {
                    var match = CheckLineForAlchemy(bg, x, false);
                    if (match != null) matches.Add(match);
                }
            }
            return matches;
        }


        private AlchemyMatchResult CheckAlchemyMatchAfterSwap(GamePiece piece1, GamePiece piece2)
        {
            int x1 = piece1.X, y1 = piece1.Y;
            int x2 = piece2.X, y2 = piece2.Y;

            // 检查两个棋子位置是否在特殊背景区域内
            HashSet<SpecialBackground> affectedBackgrounds = new HashSet<SpecialBackground>();

            foreach (SpecialBackground bg in activeSpecialBackgrounds)
            {
                if (bg.Contains(x1, y1) || bg.Contains(x2, y2))
                {
                    affectedBackgrounds.Add(bg);
                }
            }

            if (affectedBackgrounds.Count == 0) return null;
            //Debug.Log("CAllED");
            // 检查受影响区域
            foreach (SpecialBackground bg in affectedBackgrounds)
            {
                // 检查两个棋子所在的行
                var result = CheckLineForAlchemy(bg, y1, true);
                if (result != null) return result;

                result = CheckLineForAlchemy(bg, y2, true);
                if (result != null) return result;

                // 检查两个棋子所在的列
                result = CheckLineForAlchemy(bg, x1, false);
                if (result != null) return result;

                result = CheckLineForAlchemy(bg, x2, false);
                if (result != null) return result;
            }

            return null;
        }

        private AlchemyMatchResult CheckLineForAlchemy(SpecialBackground bg, int line, bool isHorizontal)
        {
            foreach (var formula in bg.alchemyFormulas)
            {
                int totalRequired = 0;
                Dictionary<ColorType, int> colorCount = new Dictionary<ColorType, int>();
                foreach (var req in formula.colorRequirements)
                {
                    totalRequired += req.requiredCount;
                    colorCount[req.color] = 0;
                }

                int start = -1, end = -1, length = 0;
                int min = isHorizontal ? bg.startX : bg.startY;
                int max = isHorizontal ? bg.endX : bg.endY;

                for (int pos = min; pos <= max; pos++)
                {
                    int x = isHorizontal ? pos : line;
                    int y = isHorizontal ? line : pos;
                    if (x < 0 || x >= xDim || y < 0 || y >= yDim) continue;

                    GamePiece piece = _pieces[x, y];
                    if (piece == null || !piece.IsColored()) continue;

                    ColorType color = piece.ColorComponent.Color;
                    bool inFormula = false;
                    foreach(var req in formula.colorRequirements)
                    {
                        if(req.color== color)
                        {
                            inFormula = true;
                            break; 
                        }
                    }

                    if (inFormula)
                    {
                        colorCount[color]++;
                        if (start == -1) start = pos;
                        end = pos;
                        length++;
                    }
                    else
                    {
                        if (length >= totalRequired && CheckColorRequirements(colorCount, formula))
                            return CreateAlchemyMatchResult(bg, formula, start, end, line, isHorizontal, length);
                        start = -1;
                        end = -1;
                        length = 0;
                        ResetColorCount(colorCount);
                    }
                }

                if (length >= totalRequired && CheckColorRequirements(colorCount, formula))
                    return CreateAlchemyMatchResult(bg, formula, start, end, line, isHorizontal, length);
            }
            return null;
        }

        private bool CheckColorRequirements(Dictionary<ColorType, int> colorCount, SpecialBackground.AlchemyFormula formula)
        {
            return formula.colorRequirements.All(req => colorCount.ContainsKey(req.color) && colorCount[req.color] >= req.requiredCount);
        }

        private void ResetColorCount(Dictionary<ColorType, int> colorCount)
        {
            foreach (var color in colorCount.Keys.ToList())
                colorCount[color] = 0;
        }

        private AlchemyMatchResult CreateAlchemyMatchResult(SpecialBackground bg, SpecialBackground.AlchemyFormula formula, int start, int end, int fixedAxis, bool isHorizontal, int length)
        {
            List<GamePiece> matchedPieces = new List<GamePiece>();
            for (int pos = start; pos <= end; pos++)
            {
                int x = isHorizontal ? pos : fixedAxis;
                int y = isHorizontal ? fixedAxis : pos;
                if (x >= 0 && x < xDim && y >= 0 && y < yDim && _pieces[x, y] != null)
                    matchedPieces.Add(_pieces[x, y]);
            }
            return new AlchemyMatchResult { SpecialBackground=bg, MatchedFormula = formula, MatchedPieces = matchedPieces, MatchLength = length };
        }

        private bool CheckForNormalMatch()
        {
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (_pieces[x, y] != null)
                    {
                        var match = GetMatch(_pieces[x, y], x, y);
                        if (match != null && match.Count >= 3)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool CheckForAlchemyMatch()
        {
            var matches = CheckAllAlchemyMatches();
            return matches != null && matches.Count > 0;
        }

        private bool HasEmptySpaces()
        {
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (_pieces[x, y] == null || _pieces[x, y].Type == PieceType.Empty)
                        return true;
                }
            }
            return false;
        }

        public bool ClearPiece(int x, int y)
        {
            if (x < 0 || x >= xDim || y < 0 || y >= yDim || _pieces[x, y] == null || !_pieces[x, y].IsClearable() || _pieces[x, y].ClearableComponent.IsBeingCleared)
                return false;

            GamePiece piece = _pieces[x, y];
            if (piece.Type == PieceType.SpecialElement && piece.IsColored())
                IncrementCollectedHerbCount(piece.ColorComponent.Color);
            else if (piece.IsColored())
                IncrementClearedHerbCount(piece.ColorComponent.Color);

            piece.ClearableComponent.Clear();
            level.OnPieceCleared(piece);
            SpawnNewPiece(x, y, PieceType.Empty);
            ClearObstacles(x, y);
            LevelNumber levelNumber = level as LevelNumber;
            levelNumber.SetHubHerbToNext(piece);
            return true;
        }

        private void ClearObstacles(int x, int y)
        {
            Dictionary<PieceType, PieceType> impurityDowngrade = new Dictionary<PieceType, PieceType>
            {
                { PieceType.impurity4, PieceType.impurity3 },
                { PieceType.impurity3, PieceType.impurity2 },
                { PieceType.impurity2, PieceType.impurity1 },
                { PieceType.impurity1, PieceType.Empty }
            };

            for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX += 2)
            {
                if (adjacentX < 0 || adjacentX >= xDim || _pieces[adjacentX, y] == null || !_pieces[adjacentX, y].IsClearable()) continue;

                GamePiece piece = _pieces[adjacentX, y];
                if (piece.Type == PieceType.Bubble)
                {
                    if (piece.IsColored()) IncrementClearedHerbCount(piece.ColorComponent.Color);
                    piece.ClearableComponent.Clear();
                    level.OnPieceCleared(piece);
                    SpawnNewPiece(adjacentX, y, PieceType.Empty);
                }
                else if (impurityDowngrade.ContainsKey(piece.Type))
                {
                    piece.ClearableComponent.Clear();
                    level.OnPieceCleared(piece);
                    SpawnNewPiece(adjacentX, y, impurityDowngrade[piece.Type]);
                }
            }

            for (int adjacentY = y - 1; adjacentY <= y + 1; adjacentY += 2)
            {
                if (adjacentY < 0 || adjacentY >= yDim || _pieces[x, adjacentY] == null || !_pieces[x, adjacentY].IsClearable()) continue;

                GamePiece piece = _pieces[x, adjacentY];
                if (piece.Type == PieceType.Bubble)
                {
                    if (piece.IsColored()) IncrementClearedHerbCount(piece.ColorComponent.Color);
                    piece.ClearableComponent.Clear();
                    level.OnPieceCleared(piece);
                    SpawnNewPiece(x, adjacentY, PieceType.Empty);
                }
                else if (impurityDowngrade.ContainsKey(piece.Type))
                {
                    piece.ClearableComponent.Clear();
                    level.OnPieceCleared(piece);
                    SpawnNewPiece(x, adjacentY, impurityDowngrade[piece.Type]);
                }
            }
        }

        public void ClearRow(int row)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (_pieces[x, row] != null && _pieces[x, row].Type != PieceType.SpecialElement)
                    ClearPiece(x, row);
            }
        }

        public void ClearColumn(int column)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (_pieces[column, y] != null && _pieces[column, y].Type != PieceType.SpecialElement)
                    ClearPiece(column, y);
            }
        }

        public void ClearColor(ColorType color)
        {
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if ((_pieces[x, y].IsColored() && _pieces[x, y].ColorComponent.Color == color) || color == ColorType.Any)
                        ClearPiece(x, y);
                }
            }
        }

        public void GameOver() => _gameOver = true;

        public List<GamePiece> GetPiecesOfType(PieceType type)
        {
            List<GamePiece> piecesOfType = new List<GamePiece>();
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    if (_pieces[x, y].Type == type)
                        piecesOfType.Add(_pieces[x, y]);
                }
            }
            return piecesOfType;
        }
    }
}