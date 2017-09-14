using System;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;

namespace eDrawingsPrinter {
  public partial class Form1 : Form {
    private eDrawingHostControl.eDrawingControl eDrawingControl1;
    private bool initialated = false;
    private Stack listbox1items = new Stack();

    public Form1() {
      InitializeComponent();
      Text = Text + " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    }

    private void InitializeEDrawingComponent() {
      this.eDrawingControl1 = new eDrawingHostControl.eDrawingControl();
      // 
      // eDrawingControl1
      // 
      this.panel1.Controls.Add(this.eDrawingControl1);
      this.eDrawingControl1.BackColor = System.Drawing.Color.White;
      this.eDrawingControl1.EnableFeatures = 32;
      this.eDrawingControl1.Location = new System.Drawing.Point(1, 1);
      this.eDrawingControl1.Name = "eDrawingControl1";
      this.eDrawingControl1.Size = new System.Drawing.Size(this.panel1.Width - 1, this.panel1.Height - 1);
      this.eDrawingControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
      this.eDrawingControl1.TabIndex = 0;
      initialated = true;
    }

    private void button1_Click(object sender, EventArgs e) {
      this.openFileDialog1.InitialDirectory = Properties.Settings.Default.LastDir;
      this.openFileDialog1.FileName = string.Empty;
      this.openFileDialog1.Multiselect = true;
      this.openFileDialog1.Filter = Properties.Settings.Default.OpenDialogFilter;
      if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK) {
        foreach (string file in this.openFileDialog1.FileNames) {
          Properties.Settings.Default.LastDir = Path.GetDirectoryName(this.openFileDialog1.FileNames[0]);
          if (!listBox1.Items.Contains(file)) {
            listBox1.Items.Add(file);
          }
        }
      }
    }

    private void print(FileInfo file) {
      eDrawingControl1.eDrawingControlWrapper.OpenDoc(file.FullName, false, false, true, "");
    }

    private void Form1_Load(object sender, EventArgs e) {
      Location = Properties.Settings.Default.LastLocation;
      Size = Properties.Settings.Default.LastSize;
      if (initialated) {
        ConnectEvents();
      } else {
        InitializeEDrawingComponent();
        ConnectEvents();
      }
    }

    private void ConnectEvents() {
      eDrawingControl1.eDrawingControlWrapper.OnFinishedLoadingDocument += eDrawingControlWrapper_OnFinishedLoadingDocument;
      eDrawingControl1.eDrawingControlWrapper.OnFailedLoadingDocument += eDrawingControlWrapper_OnFailedLoadingDocument;
      eDrawingControl1.eDrawingControlWrapper.OnFinishedPrintingDocument += eDrawingControlWrapper_OnFinishedPrintingDocument;
    }

    void eDrawingControlWrapper_OnFinishedPrintingDocument(string PrintJobName) {
      print(new FileInfo((string)listbox1items.Pop()));
    }

    void eDrawingControlWrapper_OnFailedLoadingDocument(string FileName, int ErrorCode, string ErrorString) {
      MessageBox.Show(string.Format("Failed to open '{0}' ([{1:x}] {2})", FileName, ErrorCode, ErrorString));
    }

    void eDrawingControlWrapper_OnFinishedLoadingDocument(string FileName) {
      eDrawingControl1.eDrawingControlWrapper.Refresh();
      FileInfo f = new FileInfo(FileName);
      int pages = eDrawingControl1.eDrawingControlWrapper.SheetCount;
      eDrawingControl1.eDrawingControlWrapper.SetPageSetupOptions(
        EModelView.EMVPrintOrientation.eLandscape, 1, 0, 0, 1, 7, GetDefaultPrinter(), 1, 1, 1, 1);
      eDrawingControl1.eDrawingControlWrapper.Print5(false,
        f.Name, false, false, false, EModelView.EMVPrintType.eScaleToFit, 4, 0, 0, false, 1, pages, "");
    }

    private void button2_Click(object sender, EventArgs e) {
      foreach (string item in listBox1.Items) {
        listbox1items.Push(item);
      }

      print(new FileInfo((string)listbox1items.Pop()));
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
      Properties.Settings.Default.LastSize = Size;
      Properties.Settings.Default.LastLocation = Location;
      Properties.Settings.Default.Save();
    }

    private void button3_Click(object sender, EventArgs e) {
      this.Close();
    }

    static string GetDefaultPrinter() {
      PrinterSettings settings = new PrinterSettings();
      foreach (string printer in PrinterSettings.InstalledPrinters) {
        settings.PrinterName = printer;
        if (settings.IsDefaultPrinter)
          return printer;
      }
      return string.Empty;
    }
  }
}
