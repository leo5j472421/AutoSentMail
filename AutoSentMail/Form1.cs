using System;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Linq;

namespace snmail
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofdAttachment;
        String fileName = "";
        private List<string> mailKeyWord = new List<string>(){ "Email Address" , "cc" };
        public Form1()
        {
            InitializeComponent();
        }
        private DataTable ReadExcel(string fileName, string fileExt)
        {
            string conn = string.Empty;
            DataTable dtexcel = new DataTable();
            conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=YES';"; //for above excel 2007  
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1  
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable  
                }
                catch { }
            }
            return dtexcel;
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            try
            {
                ofdAttachment = new OpenFileDialog();
                ofdAttachment.Filter = "Excel|*xlsx";
                if (ofdAttachment.ShowDialog() == DialogResult.OK)
                {
                    fileName = ofdAttachment.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = ReadExcel(fileName, "xlsx");
                var columns = dt.Columns;
                var template = rtbBody.Text.Trim();
                foreach (DataRow row in dt.Rows)
                {
                    foreach(DataColumn column in columns)
                    {

                        if (mailKeyWord.Contains(column.ColumnName))
                            continue;
                        template = template.Replace("<<" + column.ColumnName + ">>", row[column.ColumnName].ToString());
                        template = template.Replace("{{" + column.ColumnName + "}}", row[column.ColumnName].ToString());

                    }
                    //Smpt Client Details
                    //gmail >> smtp server : smtp.gmail.com, port : 587 , ssl required
                    //yahoo >> smtp server : smtp.mail.yahoo.com, port : 587 , ssl required
                    SmtpClient clientDetails = new SmtpClient();
                    clientDetails.Port = 587;
                    clientDetails.Host = "smtp.gmail.com";
                    clientDetails.EnableSsl = true;
                    clientDetails.DeliveryMethod = SmtpDeliveryMethod.Network;
                    clientDetails.UseDefaultCredentials = false;
                    clientDetails.Credentials = new NetworkCredential(txtSenderEmail.Text.Trim(), txtSenderPassword.Text.Trim());

                    //Message Details
                    MailMessage mailDetails = new MailMessage();
                    mailDetails.From =new MailAddress(txtSenderEmail.Text.Trim());
                    row["email address"].ToString().Split(',').ToList().ForEach(x=>mailDetails.To.Add(x));
                    row["cc"].ToString().Split(',').ToList().ForEach(x=>mailDetails.CC.Add(x));
                    mailDetails.Subject = "Leo test report";
                    mailDetails.Body = template;

                    /*
                    //file attachment
                    if (fileName.Length>0)
                    {
                        Attachment attachment = new Attachment(fileName);
                        mailDetails.Attachments.Add(attachment);
                    }
                    */
                    clientDetails.Send(mailDetails);
                }

                MessageBox.Show("Your mail has been sent.");

                //System.Console.WriteLine(fileName);
                fileName = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

    }
}
