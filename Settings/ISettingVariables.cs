using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMercenariesForMyTavern.Settings
{
    internal interface ISettingVariables
    {
        int minNumberOfMercenary { get; set; }
        int maxNumberOfMercenary { get; set; }
    }
}
