using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GIModuleColorAttribute : Attribute
    {
        public const float headerAlpha = 0.12f;
        public static readonly Color defaultColor = new Color(1f, 1f, 1f, headerAlpha);


        public Color color;

        public GIModuleColorAttribute(float r, float g, float b)
        {
            color = new Color(r, g, b, headerAlpha);
        }
        public GIModuleColorAttribute(GIModuleColors _color)
        {
            switch (_color)
            {
                case GIModuleColors.White: color = Color.white; break;
                case GIModuleColors.Red: color = Color.red; break;
                case GIModuleColors.Green: color = Color.green; break;
                case GIModuleColors.Blue: color = Color.blue; break;
                case GIModuleColors.Magenta: color = Color.magenta; break;
                case GIModuleColors.Cyan: color = Color.cyan; break;
                case GIModuleColors.Yellow: color = Color.yellow; break;
                case GIModuleColors.Orange: color = new Color(1f, 0.4f, 0f); break;
                case GIModuleColors.Pink: color = new Color(1f, 0.6f, 1f); break;
                default: color = Color.white; break;
            }
            color.a = headerAlpha;
        }
    }
    public enum GIModuleColors
    {
        Default,
        White,
        Red,
        Green,
        Blue,
        Magenta,
        Cyan,
        Yellow,
        Orange,
        Pink
    }
}
