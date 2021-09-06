using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Newtonsoft.Json;

namespace SimpleExcelToDataTable
{
    public partial class SimpleDataTable : Form
    {
        public SimpleDataTable()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == myOpenFileDialog.ShowDialog())
            {
                txtFileLocation.Text = myOpenFileDialog.FileName;
            }
            else
            {
                txtFileLocation.Text = "";
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtFileLocation.Text) && File.Exists(txtFileLocation.Text))
            {
                var dt = ReadExcel(txtFileLocation.Text);
                txtDataResult.Text = JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            else
            {
                MessageBox.Show("Invalid File!");
            }
        }

        private DataTable ReadExcel(string path)
        {
            IWorkbook workbook;
            DataTable myDt = new DataTable("table");

            // try to load workbook as xlsx if failed then xls.
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        workbook = new XSSFWorkbook(stream);
                    }
                    catch
                    {
                        workbook = null;
                    }
                    if (workbook == null)
                    {
                        workbook = new HSSFWorkbook(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Excel read error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return myDt; 
            }

            var sheet = workbook.GetSheetAt(0);

            var headerRow = sheet.GetRow(0);
            foreach(var headerCell in headerRow)
            {
                myDt.Columns.Add(headerCell.ToString());
            }

            for(int i = 1; i < sheet.PhysicalNumberOfRows; i++)
            {
                var sheetRow = sheet.GetRow(i);
                var dtRow = myDt.NewRow();
                dtRow.ItemArray = myDt.Columns
                    .Cast<DataColumn>()
                    .Select(o => sheetRow.GetCell(o.Ordinal, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString())
                    .ToArray();
                myDt.Rows.Add(dtRow);
            }

            return myDt;
        }
    }
}
