using Match3;
using System;
using UnityEngine;

[Serializable]
public class SpecialBackground
{
    public int startX;
    public int endX;
    public int startY;
    public int endY;

    [Serializable]
    public struct ColorRequirement
    {
        public ColorType color;
        public int requiredCount;
    }
    [Serializable]
    public struct AlchemyFormula
    {
        public ColorRequirement[] colorRequirements;
        public AlchemyType AlchemyType;
    }

    public AlchemyFormula[] alchemyFormulas;

    public bool Contains(int x, int y)
    {
        return x >= startX && x <= endX && y >= startY && y <= endY;
    }

    // 计算区域宽度
    public int Width => endX - startX + 1;

    // 计算区域高度
    public int Height => endY - startY + 1;
}