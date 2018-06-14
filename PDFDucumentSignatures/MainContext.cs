using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PDFDucumentSignatures
{
    public class MainContext : INotifyPropertyChanged
    {
        string mainFilename = @"C:\Users\SimonHoulbrooke\Desktop\Noveum FMCS Review Issue 01.pdf";
        public string MainFilename  { get => mainFilename; set => UpdateProperty(ref mainFilename, value); }
        string signatureFilename = @"C:\Users\SimonHoulbrooke\Desktop\Noveum FMCS Review Issue 01 Signatures.pdf";
        public string SignatureFilename { get => signatureFilename; set => UpdateProperty(ref signatureFilename, value); }
        string outputFilename = @"C:\Users\SimonHoulbrooke\Desktop\fred.pdf";
        public string OutputFilename { get => outputFilename; set => UpdateProperty(ref outputFilename, value); }
        int page2Replace = 2;
        public int Page2Replace { get => page2Replace; set => UpdateProperty(ref page2Replace, value); }

        public ICommand BrowseMainFilename { get; set; } = new RelayCommand<MainContext>(BrowseForMain);
        public ICommand BrowseSignatureFilename { get; set; } = new RelayCommand<MainContext>(BrowseForSignature);
        public ICommand BrowseOutputFilename { get; set; } = new RelayCommand<MainContext>(BrowseForOutput);
        public ICommand RunTheProgram { get; set; } = new RelayCommand<MainContext>(MergeThePDFs);

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateProperty<T>(ref T item, T value, [CallerMemberName] string propertyName = "")
        {
            item = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void BrowseForMain(MainContext mc)
        {
            var openDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "PDF (*.pdf)|*.pdf",
                Title = "Main File to Read",
            };
            if (openDialog.ShowDialog() == true)
            {
                mc.MainFilename = openDialog.FileName;
            }
        }

        private static void BrowseForSignature(MainContext mc)
        {
            var openDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "PDF (*.pdf)|*.pdf",
                Title = "Signature File to Read",
            };
            if (openDialog.ShowDialog() == true)
            {
                mc.SignatureFilename = openDialog.FileName;
            }
        }

        private static void BrowseForOutput(MainContext mc)
        {
            var saveDialog = new SaveFileDialog()
            {
                Filter = "PDF (*.pdf)|*.pdf",
                Title = "Ouput File to Create",
                AddExtension = true,
            };
            if (saveDialog.ShowDialog() == true)
            {
                mc.OutputFilename = saveDialog.FileName;
            }
        }

        private static void MergeThePDFs(MainContext mc)
        {

            if (mc.page2Replace < 1)
            {
                MessageBox.Show("Page must be 1 or greater");
                return;
            }

            using (FileStream stream = new FileStream(mc.OutputFilename, FileMode.Create))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                PdfReader reader = null;
                try
                {
                    document.Open();
                    reader = new PdfReader(mc.MainFilename);
                    var r2 = new PdfReader(mc.SignatureFilename);
                    if (r2.NumberOfPages != 1)
                    {
                        MessageBox.Show($"{mc.SignatureFilename} must be exactly one page");
                        document.Close();
                        reader.Close();
                        r2.Close();
                        return;
                    }
                    var pageCount = Enumerable.Range(1, mc.page2Replace - 1).ToList();
                    pdf.AddDocument(reader, pageCount);
                    reader.Close();

                    pdf.AddDocument(r2);

                    reader = new PdfReader(mc.MainFilename);
                    pageCount = Enumerable.Range(mc.page2Replace + 1, reader.NumberOfPages).ToList();
                    pdf.AddDocument(reader, pageCount);
                    reader.Close();
                    document.Close();
                    MessageBox.Show("PDF documents merged successfully", "PDF Merge");
                }
                catch (Exception ex)
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    MessageBox.Show($"There was a problem: {ex.Message}", "Error");
                }

            }

        }

    }
}
