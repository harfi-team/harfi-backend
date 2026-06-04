using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Models.Constants
{
    public class FeedbackTypes
    {
        public const string Helpful = "ساعدني";
        public const string NeedCraftsman = "محتاج حرفي";

        public static readonly string[] All = [Helpful, NeedCraftsman];
    }
}
