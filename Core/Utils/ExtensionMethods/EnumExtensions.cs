using System;

namespace RivenFramework.Utils
{
    public static class EnumExtensions
    {
        //todo: Verify this works
        public static T[] GetEnumValues<T>(this T src) where T : struct, IConvertible
        {
            CheckIfEnum<T>();

            return (T[])Enum.GetValues(src.GetType());
        }
        //todo: Verify this works
        public static T NextEnumValue<T>(this T currentValue) where T : struct, IConvertible
        {
            CheckIfEnum<T>();

            Array values = Enum.GetValues(currentValue.GetType());
            int index = Array.IndexOf(values, currentValue) + 1;
            return (T)values.GetValue(index % values.Length);

        }
        //todo: Verify this works
        public static T PreviousEnumValue<T>(this T currentValue) where T : struct, IConvertible
        {
            CheckIfEnum<T>();

            Array values = Enum.GetValues(currentValue.GetType());
            int index = Array.IndexOf(values, currentValue) - 1;
            return (T)values.GetValue(index % values.Length);
        }

        private static void CheckIfEnum<T>()
        {
            if (!typeof(T).IsEnum) 
                throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }
    }
}
