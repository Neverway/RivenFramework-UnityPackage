using System.Reflection;

namespace RivenFramework.Utils.Reflection
{
    public interface IAttributeLoader<TMemberInfo> where TMemberInfo : MemberInfo
    {
        public void LoadAttribute(TMemberInfo memberInfo);
    }
    public interface IAttributeLoader : IAttributeLoader<MemberInfo> { }

    public interface IAttributeRuntimeLoader<TMemberInfo> : IAttributeLoader<TMemberInfo>
        where TMemberInfo : MemberInfo { }
    public interface IAttributeRuntimeLoader : IAttributeRuntimeLoader<MemberInfo> { }

    public interface IAttributeEditorLoader<TMemberInfo> : IAttributeLoader<TMemberInfo>
        where TMemberInfo : MemberInfo { }
    public interface IAttributeEditorLoader : IAttributeEditorLoader<MemberInfo> { }


}
