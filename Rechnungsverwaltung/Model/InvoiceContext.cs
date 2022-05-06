using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Model
{
    class InvoiceContext : DbContext

    {
        public InvoiceContext()
        {

            Database.SetInitializer<InvoiceContext>(new InvoiceInitializer());

        }
        public DbSet<PositionEntity> Positions { get; set; }

        public DbSet<Invoice> Rechnungen { get; set; }
    }
}
