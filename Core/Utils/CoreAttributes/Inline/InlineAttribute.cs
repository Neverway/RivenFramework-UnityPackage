using RivenFramework;
using UnityEngine;
[Todo("Not even sure this really works, test this out", "Errynei")]
public class InlineAttribute : PropertyAttribute
{
    public bool showFoldout = true;
    public bool createIfNull = true;

    public InlineAttribute(bool showFoldout = true, bool createIfNull = true)
    {
        this.showFoldout = showFoldout;
        this.createIfNull = createIfNull;
    }
}