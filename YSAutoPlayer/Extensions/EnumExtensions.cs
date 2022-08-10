using System;
using System.ComponentModel;
using System.Linq;

namespace YSAutoPlayer.Extensions
{
    public static class EnumExtensions
    {
        public static string Description<TEnum>(this TEnum e) where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(e))
            {
                return "Undefined";
            }
            var descriptionAttr = typeof(TEnum).GetField(e.ToString())!.GetCustomAttributes(typeof(DescriptionAttribute), false).First();
            if (descriptionAttr == null)
            {
                return e.ToString();
            }
            return (descriptionAttr as DescriptionAttribute)!.Description;
        }
    }
}
