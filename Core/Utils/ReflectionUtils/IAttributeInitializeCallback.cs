using System.Reflection;
using System;

namespace RivenFramework.Utils
{
    public interface IGetAttributesWithMemberInfosOnLoad { }
    public interface IGetAttributesWithMemberInfosOnLoad<TAttribute, TMemberInfo> :
        IGetAttributesWithMemberInfosOnLoad
        where TAttribute : Attribute 
        where TMemberInfo : MemberInfo
    {
        public void OnGetAttributesWithInfoOnLoad(Tuple<TAttribute, TMemberInfo>[] attributeData);
    }
    public interface IGetAttributesWithMemberInfosOnRuntimeLoad<TAttribute, TMemberInfo> :
        IGetAttributesWithMemberInfosOnLoad<TAttribute, TMemberInfo>
        where TAttribute : Attribute
        where TMemberInfo : MemberInfo
    { }
#if UNITY_EDITOR
    public interface IGetAttributesWithMemberInfosOnEDITORLoad<TAttribute, TMemberInfo> :
        IGetAttributesWithMemberInfosOnLoad<TAttribute, TMemberInfo>
        where TAttribute : Attribute
        where TMemberInfo : MemberInfo
    { }
#endif

    public interface IGetTypesOnLoad<TType>
    {
        public void OnGetTypesOnLoad(TType[] types);
    }
}
