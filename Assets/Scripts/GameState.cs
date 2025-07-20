using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
  
        public enum GameState
        {
        PlayerInput,      // 玩家可操作状态
        Swapping,         // 交换动画中
        NormalClearing,   // 普通消除中
        AlchemyClearing,  // 炼金消除中
        Filling,          // 填充进行中
        AlchemyEffect     // 炼金特效执行中
    }
    
}
