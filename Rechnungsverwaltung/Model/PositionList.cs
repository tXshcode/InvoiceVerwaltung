using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Model
{
    class PositionList
    {
        public ObservableCollection<PositionEntity> PositionLists { get; set; } = new ObservableCollection<PositionEntity>();
        public static PositionList ConvertFromList(List<PositionEntity> list)
        {
            return new PositionList
            {
                PositionLists = new ObservableCollection<PositionEntity>(list)
            };
        }
    }
}
