using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO.Packaging;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Printing;
using System.Windows.Xps;
using System.Globalization;

namespace LabelPrinter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string font1 = "courier.ttf";
        const string font2 = "arial.ttf";

        MainViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.MainViewModel = new MainViewModel();
            this.DataContext = this.MainViewModel;

            this.MainViewModel.SearchText = "";


        }
                
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.SearchText = this.Search.Text;
        }

        private void PrintLabelButton_Click(object sender, RoutedEventArgs e)
        {


            Table table = new Table() { };
            //table.BorderThickness = new Thickness(1);
            //table.BorderBrush = Brushes.Black;
            table.FontFamily = new FontFamily( "Times New Roman" );

            double pixelsInOneMM = 3.77952755905512;

            table.Columns.Add(new TableColumn() { Width = new GridLength(65 * pixelsInOneMM) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(65 * pixelsInOneMM) });
            table.Columns.Add(new TableColumn() { Width = new GridLength(65 * pixelsInOneMM) });
                        
            TableRowGroup tableRowGroup = new TableRowGroup();
            
            AddressViewModel[][] avmArray = this.MainViewModel.AddressForPrinting;

            for (int i = 0; i < 7; i++)
            {
                TableRow tr = new TableRow();
               
                for (int j = 0; j < 3; j++)
                {
                    TableCell emptycell = new TableCell(new Paragraph(new Run("")));
                    TableCell cell1 = new TableCell() { Padding=new Thickness(24,12,2,2) };
                    //cell1.Background = Brushes.Green;
                    
                    cell1.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;

                    string [] lines = avmArray[i][j].Address.Split( new char[]{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

                    //cell1.BorderThickness = new Thickness(1);
                    //cell1.BorderBrush = Brushes.Black;

                    int fontSize = 14;
                    cell1.LineHeight = (32 * pixelsInOneMM) / 8;
                    double emptyLineFontSize = 8;

                    if (lines.Length == 0)
                    {

                        for (int r = 0; r < 8; r++)
                        {
                            cell1.Blocks.Add(new Paragraph(new Run(" ")) { FontSize = fontSize, LineHeight = cell1.LineHeight });
                        }

                    }
                    else
                    {
                        int totalLines = 0;

                        cell1.LineHeight = (32 * pixelsInOneMM) / 8;

                        foreach (string line in lines)
                        {
                            Run run = new Run(line){FontSize = fontSize };
                            cell1.Blocks.Add(new Paragraph(run) { TextAlignment = System.Windows.TextAlignment.Left, LineHeight = cell1.LineHeight });
                            FormattedText ft = new FormattedText(run.Text, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface(run.FontFamily, run.FontStyle, run.FontWeight, run.FontStretch), run.FontSize, Brushes.Black);
                          
                            int calcLines =  (int)(ft.Width / (60 * pixelsInOneMM))+1;

                            totalLines += calcLines;

                        }

                        int emptyLines = 8 - totalLines;

                        int startEmptyLines = emptyLines;
                        int endEmptyLines = emptyLines;
                        
                        for (int r = 0; r < startEmptyLines; r++)
                        {
                            cell1.Blocks.InsertBefore(cell1.Blocks.FirstBlock, new Paragraph(new Run(" ")) { FontSize = emptyLineFontSize, LineHeight = cell1.LineHeight });
                        }
                        
                        for (int r = 0; r < endEmptyLines; r++)
                        {
                            cell1.Blocks.InsertAfter(cell1.Blocks.LastBlock, new Paragraph(new Run(" ")) { FontSize = emptyLineFontSize, LineHeight = cell1.LineHeight });
                        }

                    }
                    
                    tr.Cells.Add(cell1);
                    
                }
                tableRowGroup.Rows.Add(tr);
            }


            table.RowGroups.Add(tableRowGroup);
            
            table.Margin = new Thickness(10,5,0,0);

            FlowDocument fd = new FlowDocument() {};

            fd.Blocks.Add(table);
            fd.Blocks.InsertBefore(fd.Blocks.FirstBlock, new Paragraph(new Run(" ")) { FontSize = 20 });

            //fd.PagePadding = new Thickness(6 * pixelsInOneMM, 16 * pixelsInOneMM, 0, 0);

            fd.PageWidth = 210 * pixelsInOneMM;
            fd.PageHeight = 297 * pixelsInOneMM;

            fd.MaxPageWidth = 210 * pixelsInOneMM;
            fd.MaxPageHeight = 297 * pixelsInOneMM;

            System.IO.MemoryStream s = new System.IO.MemoryStream();
            TextRange source = new TextRange(fd.ContentStart, fd.ContentEnd);
            source.Save(s, DataFormats.Xaml);
            
            FlowDocument copy = new FlowDocument();
            copy.PageWidth = 210 * pixelsInOneMM;
            copy.PageHeight = 297 * pixelsInOneMM;

            copy.MaxPageWidth = 210 * pixelsInOneMM;
            copy.MaxPageHeight = 297 * pixelsInOneMM;
            copy.ColumnWidth = double.PositiveInfinity;
                        
            TextRange dest = new TextRange(copy.ContentStart, copy.ContentEnd);
            dest.Load(s, DataFormats.Xaml);

            File.Delete(@"d:\\karthick\test.rtf");

            FileStream fs = new FileStream( @"d:\\karthick\test.rtf", FileMode.Create);
            dest.Save(fs, DataFormats.Rtf);
            fs.Close();

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                //Other settings
                DocumentPaginator paginator = ((IDocumentPaginatorSource)copy).DocumentPaginator;
                paginator.PageSize = new Size(816, 1056); //Set the size of A4 here accordingly
                printDialog.PrintDocument(paginator, "My");

            }

            //DoThePrint(fd);

        }

        private void DoThePrint(System.Windows.Documents.FlowDocument document)
        {
            // Clone the source document's content into a new FlowDocument.
            // This is because the pagination for the printer needs to be
            // done differently than the pagination for the displayed page.
            // We print the copy, rather that the original FlowDocument.
            System.IO.MemoryStream s = new System.IO.MemoryStream();
            TextRange source = new TextRange(document.ContentStart, document.ContentEnd);
            source.Save(s, DataFormats.Xaml);
            FlowDocument copy = new FlowDocument();
            TextRange dest = new TextRange(copy.ContentStart, copy.ContentEnd);
            dest.Load(s, DataFormats.Xaml);

            // Create a XpsDocumentWriter object, implicitly opening a Windows common print dialog,
            // and allowing the user to select a printer.

            // get information about the dimensions of the seleted printer+media.
            System.Printing.PrintDocumentImageableArea ia = null;
            System.Windows.Xps.XpsDocumentWriter docWriter = System.Printing.PrintQueue.CreateXpsDocumentWriter(ref ia);

            double pixelsInOneMM = 3.77952755905512;

            if (docWriter != null && ia != null)
            {
                DocumentPaginator paginator = ((IDocumentPaginatorSource)copy).DocumentPaginator;

                // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                paginator.PageSize = new Size(ia.MediaSizeWidth, ia.MediaSizeHeight);
                Thickness t = new Thickness(6 * pixelsInOneMM, 16 * pixelsInOneMM, 6 * pixelsInOneMM, 16 * pixelsInOneMM);  // copy.PagePadding;
                //copy.PagePadding = new Thickness(
                //                 Math.Max(ia.OriginWidth, t.Left),
                //                   Math.Max(ia.OriginHeight, t.Top),
                //                   Math.Max(ia.MediaSizeWidth - (ia.OriginWidth + ia.ExtentWidth), t.Right),
                //                   Math.Max(ia.MediaSizeHeight - (ia.OriginHeight + ia.ExtentHeight), t.Bottom));

                copy.ColumnWidth = double.PositiveInfinity;
                //copy.PageWidth = 528; // allow the page to be the natural with of the output device

                // Send content to the printer.
                docWriter.Write(paginator);
            }

        }

        private void SaveToXps()
        {
            string packageName = "abcd.xps";

            using (Package package = Package.Open(packageName))
            {
                XpsDocument xpsDocument = new XpsDocument(package);

                // Add the package content (false=without PrintTicket).
                AddPackageContent(xpsDocument, false);

                // Close the package.
                xpsDocument.Close();
            }
        }

        // ------------------------- AddPackageContent ----------------------------
        /// <summary>
        ///   Adds a predefined set of content to a given XPS document.</summary>
        /// <param name="xpsDocument">
        ///   The package to add the document content to.</param>
        /// <param name="attachPrintTicket">
        ///   true to include a PrintTicket with the
        ///   document; otherwise, false.</param>
        private void AddPackageContent(
            XpsDocument xpsDocument, bool attachPrintTicket)
        {
            try
            {
                PrintTicket printTicket = GetPrintTicketFromPrinter();
                // PrintTicket is null, there is no need to attach one.
                if (printTicket == null)
                    attachPrintTicket = false;

                // Add a FixedDocumentSequence at the Package root
                IXpsFixedDocumentSequenceWriter documentSequenceWriter =
                    xpsDocument.AddFixedDocumentSequence();

                // Add the 1st FixedDocument to the FixedDocumentSequence. - - - - -
                IXpsFixedDocumentWriter fixedDocumentWriter =
                    documentSequenceWriter.AddFixedDocument();

                // Add content to the 1st document
                AddDocumentContent(fixedDocumentWriter);

                // Commit the 1st Document
                fixedDocumentWriter.Commit();


                // If attaching PrintTickets, attach one at
                // the package FixedDocumentSequence level.
                if (attachPrintTicket)
                    documentSequenceWriter.PrintTicket = printTicket;

                // Commit the FixedDocumentSequence
                documentSequenceWriter.Commit();
            }
            catch (XpsPackagingException xpsException)
            {
                throw xpsException;
            }
        }// end:AddPackageContent()


        // ------------------------- AddDocumentContent ---------------------------
        /// <summary>
        ///   Adds a predefined set of content to a given document writer.</summary>
        /// <param name="fixedDocumentWriter">
        ///   The document writer to add the content to.</param>
        private void AddDocumentContent(IXpsFixedDocumentWriter fixedDocumentWriter)
        {
            // Collection of image and font resources used on the current page.
            //   Key: "XpsImage", "XpsFont"
            //   Value: List of XpsImage or XpsFont resources
            Dictionary<string, List<XpsResource>> resources;

            try
            {
                // Add Page 1 to current document.
                IXpsFixedPageWriter fixedPageWriter =
                    fixedDocumentWriter.AddFixedPage();

                // Add the resources for Page 1 and get the resource collection.
               // resources = AddPageResources(fixedPageWriter);

                // Write page content for Page 1.

                fixedPageWriter.XmlWriter.WriteStartElement("FixedPage");
                fixedPageWriter.XmlWriter.WriteAttributeString("Width", "816");
                fixedPageWriter.XmlWriter.WriteAttributeString("Height", "1056");
                fixedPageWriter.XmlWriter.WriteAttributeString("xmlns",
                    "http://schemas.microsoft.com/xps/2005/06");
                fixedPageWriter.XmlWriter.WriteAttributeString("xml:lang", "en-US");


                fixedPageWriter.XmlWriter.WriteStartElement("Table");

                fixedPageWriter.XmlWriter.WriteAttributeString("Margin", "6,16,0,0");

                fixedPageWriter.XmlWriter.WriteStartElement("Table.Columns");

                fixedPageWriter.XmlWriter.WriteStartElement("TableColumn");
                fixedPageWriter.XmlWriter.WriteAttributeString("Width", "63.5");
                fixedPageWriter.XmlWriter.WriteEndElement(); //column

                fixedPageWriter.XmlWriter.WriteStartElement("TableColumn");
                fixedPageWriter.XmlWriter.WriteAttributeString("Width", "3");
                fixedPageWriter.XmlWriter.WriteEndElement(); //column

                fixedPageWriter.XmlWriter.WriteStartElement("TableColumn");
                fixedPageWriter.XmlWriter.WriteAttributeString("Width", "63.5");
                fixedPageWriter.XmlWriter.WriteEndElement(); //column

                fixedPageWriter.XmlWriter.WriteStartElement("TableColumn");
                fixedPageWriter.XmlWriter.WriteAttributeString("Width", "3");
                fixedPageWriter.XmlWriter.WriteEndElement(); //column

                fixedPageWriter.XmlWriter.WriteStartElement("TableColumn");
                fixedPageWriter.XmlWriter.WriteAttributeString("Width", "63.5");
                fixedPageWriter.XmlWriter.WriteEndElement(); //column

                fixedPageWriter.XmlWriter.WriteEndElement(); //columns

                fixedPageWriter.XmlWriter.WriteStartElement("TableRowGroup");


                for (int i = 0; i < 7; i++)
                {

                    fixedPageWriter.XmlWriter.WriteStartElement("TableRow");

                    fixedPageWriter.XmlWriter.WriteStartElement("TableCell");
                    
                    fixedPageWriter.XmlWriter.WriteElementString("Paragraph", "Hello!!!");
                   
                    fixedPageWriter.XmlWriter.WriteEndElement(); //TableCell
                    
                    fixedPageWriter.XmlWriter.WriteEndElement(); //row

                }


                fixedPageWriter.XmlWriter.WriteEndElement(); //TableRowGroup
                
                fixedPageWriter.XmlWriter.WriteEndElement(); //table

                // Commit Page 1.
                fixedPageWriter.Commit();

            }
            catch (XpsPackagingException xpsException)
            {
                throw xpsException;
            }
        }// end:AddDocumentContent()


        // -------------------------- AddPageResources ----------------------------
        private Dictionary<string, List<XpsResource>>
                AddPageResources(IXpsFixedPageWriter fixedPageWriter)
        {
            // Collection of all resources for this page.
            //   Key: "XpsImage", "XpsFont"
            //   Value: List of XpsImage or XpsFont
            Dictionary<string, List<XpsResource>> resources =
                new Dictionary<string, List<XpsResource>>();

            // Collections of images and fonts used in the current page.
            List<XpsResource> xpsImages = new List<XpsResource>();
            List<XpsResource> xpsFonts = new List<XpsResource>();

            try
            {
                
                XpsFont xpsFont;

                // Add, Write, and Commit font 1 to the current page.
                xpsFont = fixedPageWriter.AddFont();
                WriteObfuscatedStream(
                    xpsFont.Uri.ToString(), xpsFont.GetStream(), font1);
                xpsFont.Commit();
                xpsFonts.Add(xpsFont);      // Add font1 as a required resource.
                
                // Add, Write, and Commit font2 to the current page.
                xpsFont = fixedPageWriter.AddFont(false);
                WriteToStream(xpsFont.GetStream(), font2);
                xpsFont.Commit();
                xpsFonts.Add(xpsFont);      // Add font2 as a required resource.

                // Return the image and font resources in a combined collection.
                resources.Add("XpsImage", xpsImages);
                resources.Add("XpsFont", xpsFonts);
                return resources;
            }
            catch (XpsPackagingException xpsException)
            {
                throw xpsException;
            }
        }// end:AddPageResources()


        // ---------------------- GetPrintTicketFromPrinter -----------------------
        /// <summary>
        ///   Returns a PrintTicket based on the current default printer.</summary>
        /// <returns>
        ///   A PrintTicket for the current local default printer.</returns>
        private PrintTicket GetPrintTicketFromPrinter()
        {
            PrintQueue printQueue = null;

            LocalPrintServer localPrintServer = new LocalPrintServer();

            // Retrieving collection of local printer on user machine
            PrintQueueCollection localPrinterCollection =
                localPrintServer.GetPrintQueues();

            System.Collections.IEnumerator localPrinterEnumerator =
                localPrinterCollection.GetEnumerator();

            if (localPrinterEnumerator.MoveNext())
            {
                // Get PrintQueue from first available printer
                printQueue = (PrintQueue)localPrinterEnumerator.Current;
            }
            else
            {
                // No printer exist, return null PrintTicket
                return null;
            }

            // Get default PrintTicket from printer
            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            PrintCapabilities printCapabilites = printQueue.GetPrintCapabilities();

            // Modify PrintTicket
            if (printCapabilites.CollationCapability.Contains(Collation.Collated))
            {
                printTicket.Collation = Collation.Collated;
            }

            if (printCapabilites.DuplexingCapability.Contains(
                    Duplexing.TwoSidedLongEdge))
            {
                printTicket.Duplexing = Duplexing.TwoSidedLongEdge;
            }

            if (printCapabilites.StaplingCapability.Contains(Stapling.StapleDualLeft))
            {
                printTicket.Stapling = Stapling.StapleDualLeft;
            }

            return printTicket;
        }// end:GetPrintTicketFromPrinter()


      


        // ----------------------------- WriteToStream ----------------------------
        private void WriteToStream(Stream stream, string resource)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;

            using (FileStream fileStream =
                new FileStream(resource, FileMode.Open, FileAccess.Read))
            {
                while ((bytesRead = fileStream.Read(buf, 0, bufSize)) > 0)
                {
                    stream.Write(buf, 0, bytesRead);
                }
            }
        }// end:WriteToStream()


        // ------------------------- WriteObfuscatedStream ------------------------
        private void WriteObfuscatedStream(
            string resourceName, Stream stream, string resource)
        {
            int bufSize = 0x1000;
            int guidByteSize = 16;
            int obfuscatedByte = 32;

            // Get the GUID byte from the resource name.  Typical Font name:
            //    /Resources/Fonts/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.ODTTF
            int startPos = resourceName.LastIndexOf('/') + 1;
            int length = resourceName.LastIndexOf('.') - startPos;
            resourceName = resourceName.Substring(startPos, length);

            Guid guid = new Guid(resourceName);

            string guidString = guid.ToString("N");

            // Parsing the guid string and coverted into byte value
            byte[] guidBytes = new byte[guidByteSize];
            for (int i = 0; i < guidBytes.Length; i++)
            {
                guidBytes[i] = Convert.ToByte(guidString.Substring(i * 2, 2), 16);
            }

            using (FileStream filestream = new FileStream(resource, FileMode.Open))
            {
                // XOR the first 32 bytes of the source
                // resource stream with GUID byte.
                byte[] buf = new byte[obfuscatedByte];
                filestream.Read(buf, 0, obfuscatedByte);

                for (int i = 0; i < obfuscatedByte; i++)
                {
                    int guidBytesPos = guidBytes.Length - (i % guidBytes.Length) - 1;
                    buf[i] ^= guidBytes[guidBytesPos];
                }
                stream.Write(buf, 0, obfuscatedByte);

                // copy remaining stream from source without obfuscation
                buf = new byte[bufSize];

                int bytesRead = 0;
                while ((bytesRead = filestream.Read(buf, 0, bufSize)) > 0)
                {
                    stream.Write(buf, 0, bytesRead);
                }
            }
        }// end:WriteObfuscatedStream()


    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public AddressViewModel [][] AddressForPrinting { get; set; }

        public List<AddressSelectionViewModel> AllAddresses { get; set; }

        public List<AddressSelectionViewModel> AddressForSelection { get; set; }

        private string searchText = string.Empty;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;

                if (!string.IsNullOrWhiteSpace(this.SearchText))
                {
                    this.AddressForSelection = this.AllAddresses.Where(a => a.Address.ToLower().Contains(this.SearchText.ToLower())).ToList();
                }
                else
                {
                    this.AddressForSelection = this.AllAddresses.ToList();
                }

                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("AddressForSelection"));
            }
        }

        public MainViewModel()
        {
            this.AddressForPrinting = new AddressViewModel[7][];

            for (int i = 0; i < 7; i++)
            {
                this.AddressForPrinting[i] = new AddressViewModel[3];
                for (int j = 0; j < 3; j++)
                    this.AddressForPrinting[i][j] = new AddressViewModel() { Address = string.Empty };
            }

            this.AllAddresses = new List<AddressSelectionViewModel>()
            {
                new AddressSelectionViewModel { Address=string.Empty },
                new AddressSelectionViewModel { Address=string.Empty },
                new AddressSelectionViewModel { Address=string.Empty }
            };

            //XElement addresses = XElement.Load(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Addresses.xml"), LoadOptions.None); 

            //this.AllAddresses = addresses.Descendants().Select
            //(
            //    node=>
            //    new AddressSelectionViewModel
            //    {
            //        Address=node.Value.Trim()
            //    }
            //).ToList();

        }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DeleteCommandViewModel : ICommand
    {

        public AddressSelectionViewModel AddressSelectionViewModel { get; set; }

        public DeleteCommandViewModel(AddressSelectionViewModel asvm)
        {
            this.AddressSelectionViewModel = asvm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MainViewModel mainVM = parameter as MainViewModel;

            if (mainVM != null)
            {
                //AddressSelectionViewModel asvm = mainVM.AddressForSelection
                //    .Where(a => a.Address.Trim(' ', '\n', '\r', '\t', ',', '.') == this.AddressSelectionViewModel.Address.Trim(' ', '\n', '\r', '\t', ',', '.'))
                //    .Single();

                mainVM.AllAddresses.Remove(AddressSelectionViewModel);

                mainVM.SearchText = mainVM.SearchText;

                XElement addresses = new XElement( "Addresses", 
                mainVM.AllAddresses.Select( 
                    a=>
                    new XElement ("Address",
                        new XCData( a.Address)
                    )
                    ));
                addresses.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Addresses.xml"));
                    


            }

        }


        public event EventHandler CanExecuteChanged;
    }

    public class AddCommandViewModel : ICommand
    {

        public AddressSelectionViewModel AddressSelectionViewModel { get; set; }

        public AddCommandViewModel(AddressSelectionViewModel asvm)
        {
            this.AddressSelectionViewModel = asvm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MainViewModel mainVM = parameter as MainViewModel;

            if (mainVM != null)
            {
                mainVM.AddressForPrinting
                    .SelectMany(a=>a)
                    .Where(a=>a.IsSelected)
                    .ToList()
                    .ForEach(a => { a.IsSelected = false; a.CanSave = false; a.Address = this.AddressSelectionViewModel.Address; });
            }

        }


        public event EventHandler CanExecuteChanged;
    }

    public class AddressSelectionViewModel : INotifyPropertyChanged
    {

        public AddressSelectionViewModel()
        {
            this.AddCommand = new AddCommandViewModel(this);
            this.DeleteCommand = new DeleteCommandViewModel(this);
        }

        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class AddressViewModel : INotifyPropertyChanged
    {
        private bool selected;

        public bool IsSelected
        {
            get { return selected; }
            set { selected = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSelected")); }
        }


        private bool isEmpty;

        public bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(this.Address); }
            set { isEmpty = value; if (string.IsNullOrWhiteSpace(this.Address)) this.CanSave = false; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsEmpty")); }
        }



        private Visibility Visible;

        public Visibility Visibility
        {
            get { return this.CanSave ? Visibility.Visible : Visibility.Hidden; }
            set {  }
        }

        private bool canSave;

        public bool CanSave
        {
            get { return canSave; }
            set { canSave = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Visibility")); }
        }
               

        private string address;

        public string Address
        {
            get { return address; }
            set { address = value; this.CanSave = !string.IsNullOrWhiteSpace(value); if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("Address")); PropertyChanged(this, new PropertyChangedEventArgs("IsEmpty")); } }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
