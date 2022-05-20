using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using QRCoder;
using Rechnungsverwaltung.Model;
using Rechnungsverwaltung.Printing;
using PrintDialog = System.Windows.Controls.PrintDialog;
using Rechnungsverwaltung.MQTT;

namespace Rechnungsverwaltung.ViewModel
{
    class RechnungenViewModel : INotifyPropertyChanged
    {
        #region Declarations

        private InvoiceList list;
        private PositionList posList;

        public InvoiceList List
        {
            get => list;
            set
            {
                list = value;
                RaisePropertyChanged();
            }
        }

        public PositionList PosList
        {
            get => posList;
            set
            {
                posList = value;
                RaisePropertyChanged();
            }
        }


        public List<Invoice> lists = new List<Invoice>();
        public List<PositionEntity> poslists = new List<PositionEntity>();
        public string ChosenName { get; set; }
        public string ChosenAdress { get; set; }
        public DateTime ChosenDate { get; set; } = DateTime.Now;
        public double ChosenAmount { get; set; }
        public int ChosenVat { get; set; }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ICommand PrintCommand { get; set; }

        public ICommand SendInvoicePositionCommand { get; set; }
        public ICommand SendInvoiceCommand { get; set; }


        public int ChosenID { get; set; }

        public Invoice currentInvoice;
        public PositionEntity currentPosition;
        public Invoice CurrentInvoice
        {
            get => currentInvoice;
            set
            {
                currentInvoice = value;
                RaisePropertyChanged(nameof(SeriesAmountInvoicePosition));

            }
        }
        public PositionEntity CurrentPosition
        {
            get => currentPosition;
            set
            {
                currentPosition = value;
                RaisePropertyChanged();
            }
        }


        public SeriesCollection SeriesCollectionInvoiceAmounts
        {
            get
            {
                using (var context = new InvoiceContext())
                {
                    var invoices = context.Rechnungen.OrderBy(i => i.InvoiceDate);

                    var seriesCollection = new SeriesCollection();

                    var lineSeries = new LineSeries
                    {
                        Title = "Rechnungsverlauf",
                        Values = new ChartValues<DateTimePoint>(),
                    };

                    foreach (var invoice in invoices)
                    {
                        lineSeries.Values.Add(new DateTimePoint
                        {
                            DateTime = invoice.InvoiceDate,
                            Value = invoice.Amount,

                        });
                    }

                    seriesCollection.Add(lineSeries);
                    return seriesCollection;
                }
            }
        }
        public SeriesCollection SeriesCollectionBubbleChart
        {
            get
            {
                
                List<BubbleChartMap> ListBubble = new List<BubbleChartMap>();
                var ScatterSeries = new ScatterSeries()
                {
                    Title = "Invoices",
                    Values = new ChartValues<ScatterPoint>(),
                    MinPointShapeDiameter = 15,
                    MaxPointShapeDiameter = 45
                };

                using (var ctx = new InvoiceContext())
                {
                    foreach (var Rechnung in ctx.Rechnungen)
                    {
                        ListBubble.Add(new BubbleChartMap { Amount = Rechnung.Amount, Date = Rechnung.InvoiceDate, NumberPositions = Rechnung.Position.Count() });
                            
                    }
                }
                var seriesCollection = new SeriesCollection()
                    {
                        new ScatterSeries
                        {
                            Values = new ChartValues<BubbleChartMap>(ListBubble),
                            Configuration = Mappers.Weighted<BubbleChartMap>()
                            .X(i => i.Date.Ticks)
                            .Y(i => i.Amount)
                            .Weight(i => i.NumberPositions)
                        }
                    };

                return seriesCollection;
            }
        }
        public SeriesCollection SeriesAmountInvoicePosition
        {
            get
            {
                Func<ChartPoint, string> labelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})";

                var seriesCollection = new SeriesCollection();

                if (CurrentInvoice != null)
                {
                    foreach (var position in CurrentInvoice.Position)
                    {
                        seriesCollection.Add(new PieSeries
                        {
                            Title = position.ItemNr.ToString(),
                            Values = new ChartValues<double> { position.Qty},
                            PushOut = position.Id == 1 ? 10 : 0,
                            DataLabels = true,
                            LabelPoint = labelPoint
                        });
                    }
                }

                

                return seriesCollection;
            }
        }
        public Func<ChartPoint, string> PointLabel { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public Func<double, string> XFormatterBubble { get; set; }
        public Func<double, string> YFormatterBubble { get; set; }


        #endregion 
        
        public RechnungenViewModel()
        {

            ChosenID = 1;
            YFormatter = value => value.ToString("C");
            XFormatter = value => new DateTime((long)value).ToString("dd.MM.yyyy");

            YFormatterBubble = value => value.ToString("C");
            XFormatterBubble = value => new DateTime((long)value).ToString("dd.MM.yyyy");
            var ctx = new InvoiceContext();
            lists = ctx.Rechnungen.Include("Position").ToList();
            List = InvoiceList.ConvertFromList(lists);

            MQTTClient Client = new MQTTClient();
            Client.Init("Client1","localhost");

            #region ICommands

            AddCommand = new RelayCommand(e =>
            {

                Invoice invoice = new Invoice()
                {
                    CustomerName = ChosenName,
                    InvoiceDate = ChosenDate,
                    Amount = ChosenAmount,
                    Vat = ChosenVat,
                    CustomerAdress = ChosenAdress
                };

                try
                {
                    using (ctx = new InvoiceContext())
                    {
                        ctx.Rechnungen.Add(invoice); //Datensatz einfügen
                        ctx.SaveChanges();  //Speichern / commit


                        lists = ctx.Rechnungen.Include("Position").ToList();
                        List = InvoiceList.ConvertFromList(lists);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }
            );


            DeleteCommand = new RelayCommand(e =>
            {


                DialogResult dialog = System.Windows.Forms.MessageBox.Show("Endgültig Löschen?", "", MessageBoxButtons.YesNo);
                if (dialog == DialogResult.Yes)
                {
                    try
                    {
                        using (ctx = new InvoiceContext())
                        {
                            var user = ctx.Rechnungen.Find(CurrentInvoice.ID);
                            if (user != null)
                            {
                                ctx.Rechnungen.Remove(user);
                                ctx.SaveChanges();
                                lists = ctx.Rechnungen.Include("PositionEntity").ToList();
                                List = InvoiceList.ConvertFromList(lists);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            );

            PrintCommand = new RelayCommand(e =>
            {
                FlowDocument document = RechnungenViewModel.getFlowDocument("Printing/Invoice.xaml");

                var invoicePrintData = new InvoicePrintData();
                invoicePrintData.Invoice = CurrentInvoice;
                document.DataContext = invoicePrintData;
                invoicePrintData.Positions = (IList<PositionEntity>)CurrentInvoice.Position;

                invoicePrintData.BarCode = CreateBarCode("123");
                string qr = CurrentInvoice.ID.ToString();
                invoicePrintData.QrCode = CreateQrCode(qr);

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                    printDialog.PrintDocument((document as IDocumentPaginatorSource).DocumentPaginator, "Invoice");
            }

            );

            SendInvoiceCommand = new RelayCommand(async e =>
            {
                String isSuccessful = await Client.SendInvoice(CurrentInvoice);
                if(isSuccessful!="successful") System.Windows.Forms.MessageBox.Show("Sending Message Failed: Following Exception:"+isSuccessful);
            });

            SendInvoicePositionCommand = new RelayCommand(async e =>
            {
                
                String isSuccessful = await Client.SendInvoicePosition(CurrentPosition);
                if(isSuccessful!="successful") System.Windows.Forms.MessageBox.Show("Sending Message Failed: Following Exception:"+isSuccessful);
            }

            );
            #endregion


        }

        private static FlowDocument getFlowDocument(String path)
        {
            String rawDocument = "";
            using (StreamReader streamReader = File.OpenText(path))
            {
                rawDocument = streamReader.ReadToEnd();
            }

            FlowDocument flowDocument = XamlReader.Load(new XmlTextReader(new StringReader(rawDocument))) as FlowDocument;
            return flowDocument;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region BarCode QrCode
        private BitmapSource CreateBarCode(string toCode)
        {
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            System.Drawing.Image img = b.Encode(BarcodeLib.TYPE.CODE93, toCode, Color.Black, Color.White, 100, 50);

            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        private BitmapSource CreateQrCode(string toCode)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(toCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap result = qrCode.GetGraphic(20);

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         result.GetHbitmap(),
                         IntPtr.Zero,
                         Int32Rect.Empty,
                         BitmapSizeOptions.FromEmptyOptions());
        }
        #endregion
    }
}
