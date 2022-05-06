using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Model
{
    class InvoiceList
    {

        public ObservableCollection<Invoice> InvoiceLists { get; set; } = new ObservableCollection<Invoice>();
        public static InvoiceList ConvertFromList(List<Invoice> list)
        {
            return new InvoiceList
            {
                InvoiceLists = new ObservableCollection<Invoice>(list)
            };
        }
    }
}
