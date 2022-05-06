using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Rechnungsverwaltung.Model;

namespace Rechnungsverwaltung.Printing
{
    class InvoicePrintData
    {
        public DateTime PrintingDate => DateTime.Now;
        public Invoice Invoice { get; set; }


        public IList<PositionEntity> Positions { get; set; }
        public BitmapSource BarCode { get; set; }
        public BitmapSource QrCode { get; set; }
    }
}
