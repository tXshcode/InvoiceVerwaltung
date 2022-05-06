using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Model
{
    class InvoiceInitializer : DropCreateDatabaseAlways<InvoiceContext>
    {
        protected override void Seed(InvoiceContext context)
        {
            IList<Invoice> defaults = new List<Invoice>();
            IList<PositionEntity> defaultPositions = new List<PositionEntity>();

            defaults.Add(new Invoice
            {
                CustomerName = "HTL",
                Amount = 100,
                CustomerAdress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 25),
                Vat = 20
                
            }) ;

            defaults.Add(new Invoice
            {
                CustomerName = "HAK",
                Amount = 200,
                CustomerAdress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 15),
                Vat = 20
            });

            defaults.Add(new Invoice
            {
                CustomerName = "HLW",
                Amount = 150,
                CustomerAdress = "Amstetten",
                InvoiceDate = new DateTime(2020, 01, 10),
                Vat = 20
            });

          

            defaultPositions.Add(new PositionEntity
            {
                ItemNr = 1,
                Qty = 50,
                Price = 100,
                InvoiceId = 1
            }) ;

            defaultPositions.Add(new PositionEntity
            {
                ItemNr = 3,
                Qty = 20,
                Price = 200,
                InvoiceId = 1
            });

            defaultPositions.Add(new PositionEntity
            {
                ItemNr = 5,
                Qty = 60,
                Price = 400,
                InvoiceId = 3
            });

            defaultPositions.Add(new PositionEntity
            {
                ItemNr = 6,
                Qty = 30,
                Price = 80,
                InvoiceId = 3
            });

            defaultPositions.Add(new PositionEntity
            {
                ItemNr = 9,
                Qty = 90,
                Price = 55,
                InvoiceId = 3
            });

            context.Rechnungen.AddRange(defaults);
            context.Positions.AddRange(defaultPositions);

            base.Seed(context);
        }
    }
}
