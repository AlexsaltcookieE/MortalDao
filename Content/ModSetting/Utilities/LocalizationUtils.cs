using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace MortalDao.Content.ModSetting.Utilities
{
    public partial class MortalDaoUtils
    {
        public static LocalizedText GetText(string key)
        {
            return Language.GetOrRegister("Mods.MortalDao." + key);
        }
    }
}
