using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using System.Data.OracleClient;
//using Oracle.ManagedDataAccess.Client;
using System.Runtime.InteropServices;

using DalTracking;
using System.Reflection;
using System.Configuration;

namespace SendTracking
{
    [ComVisible(true)]
    [ProgId("SendTracking.TrackSending")]

    public partial class TrackSending : UserControl, IExtensionWindow
    {
        #region Private members

        private INautilusUser _ntlsUser;

        private IExtensionWindowSite2 _ntlsSite;

        private INautilusProcessXML _processXml;

        private INautilusServiceProvider _sp;

        private OracleConnection _connection;

        private OracleCommand cmd;

        private double sessionId;

        private string _connectionString;

        private INautilusRecordSet rs;

        public string _fromLab;

        public string _ToLab;

        public string _fromLabName;

        public string _ToLabName;

        public bool DEBUG;

        private DalTrackingCls dal;

        private string nextEntityStatus;
        #endregion


        public TrackSending()
        {
            InitializeComponent();
            BackColor = Color.FromName("Control");
            txtBox.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendBox();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            lblOkMsg.Text = "";
            lblErrorMsg.Text = "";
            if (e.KeyChar == (char)13 && txtBox.Text != "") //Enter
            {
                SendBox();
            }
        }
        string msg = "";
        private void SendBox()
        {


            lblErrorMsg.Visible = true;
            lblErrorMsg.Text = "";
            lblOkMsg.Visible = false;
            var box = dal.GetBoxByName(txtBox.Text.Trim());

            if (box != null)
            {
                if (box.from_lab != _fromLab)
                {


                    msg = string.Format("הארגז אינו מ {0}  ל {1}", _fromLabName, _ToLabName);
                    SetMsg(lblErrorMsg, msg, true);
                }
                if (box.status == BoxStatus.SENT)
                {
                    msg = ("הארגז כבר דווח כנשלח.");
                    SetMsg(lblErrorMsg, msg, true);

                }
                else if (box.status == BoxStatus.ARRIVED)
                {
                    msg = ("הארגז כבר הגיע.");
                    SetMsg(lblErrorMsg, msg, true);

                }
                else
                {
                    double user = 1;
                    if (DEBUG == false)
                    {
                        user = _ntlsUser.GetOperatorId();
                    }

                    bool res = dal.UpdateBoxStatus(box.id, BoxStatus.SENT, user, true);
                    dal.UpdateEntitiesBox(box.id, nextEntityStatus,_fromLabName);                   

                    if (res)
                    {
                        msg = "הארגז נשלח בהצלחה ל" + _ToLabName;
                        SetMsg(lblOkMsg, msg, true);
                        btnPrint_Click(null, null);
                        lblOkMsg.Visible = true;
                        lblErrorMsg.Visible = false;
                    }
                }
            }

            else
            {

                SetMsg(lblErrorMsg, "הארגז לא קיים במערכת!!", false);
            }
            txtBox.Text = null;
            txtBox.Focus();
        }
        private void SetMsg(Label lbl, string txt, bool valid)
        {
            lbl.Text = txt;
            lbl.ForeColor = valid ? Color.Blue : Color.Red;
        }

        #region Implementing IExtensionWindow

        public void PreDisplay()
        {
            INautilusDBConnection dbConnection;
            dal = new DalTrackingCls();

            if (!DEBUG)
            {
                dbConnection = _sp.QueryServiceProvider("DBConnection") as NautilusDBConnection;
                rs = _sp.QueryServiceProvider("RecordSet") as NautilusRecordSet;
                _connection = dal.GetConnection(dbConnection, _ntlsUser, _ntlsSite, _connectionString, sessionId, cmd, _connection);
            }
            else
            {
                _processXml = null;
                _connection = new OracleConnection("Data Source=samba;user id=lims_sys;password=lims_sys;");
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                dal._connection = _connection;
                _fromLab = "98";
                _ToLab = "991";

                if (_fromLab == "98")
                {
                    nextEntityStatus = "SZ";//נשלח לצפון
                }
                else
                {
                    nextEntityStatus = "SD";//נשלח לדרום
                }
            }

            _fromLabName = dal.GetLabLocation(_fromLab);
            _ToLabName = dal.GetLabLocation(_ToLab);

            label1.Text = "שליחה מ" + _fromLabName + " ל" + _ToLabName;
            txtBox.Focus();
        }

        public void SetParameters(string parameters)
        {
            try
            {
                int index = 0;
                var splitedParameters = parameters.Split(';');
                this._fromLab = splitedParameters[index++];
                this._ToLab = splitedParameters[index++];
                this.nextEntityStatus = splitedParameters[index++];

            }
            catch (Exception e)
            {
                MessageBox.Show("לא הוגדרו פרמטרים כראוי,לא ניתן להשתמש בתוכנית");
            }
        }

        public bool CloseQuery()
        {
            try
            {
                if (cmd != null) cmd.Dispose();
                if (_connection != null) _connection.Close();

                return true;
            }
            catch (Exception ex)
            {

                return true;
            }
        }

        public void Internationalise()
        {
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("Track Sending");
            _ntlsSite.SetWindowRegistryName("Track Sending");
            _ntlsSite.SetWindowTitle("Track Sending");
        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false;
        }

        public void SaveSettings(int hKey)
        {
        }

        public void Setup()
        {
        }

        public void refresh()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void SetServiceProvider(object serviceProvider)
        {
            _sp = serviceProvider as NautilusServiceProvider;

            if (_sp != null)
            {
                _processXml = _sp.QueryServiceProvider("ProcessXML") as NautilusProcessXML;
                _ntlsUser = _sp.QueryServiceProvider("User") as NautilusUser;

            }
            else
            {
                _processXml = null;
            }


        }

        public void RestoreSettings(int hKey)
        {

        }


        #endregion

        private void TrackSending_Resize(object sender, EventArgs e)
        {


            label1.Location = new Point(panel1.Width / 2 - label1.Width / 2, label1.Location.Y);
            panel1.Location = new Point(Width / 2 - panel1.Width / 2, panel1.Location.Y);

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void close_button_Click(object sender, EventArgs e)
        {
            if (_ntlsSite != null)
            {
                _ntlsSite.CloseWindow();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = assemblyPath + ".config";
            Configuration cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var appSettings = cfg.AppSettings;
            var zpl = appSettings.Settings["ZPL"].Value;
            var Count = appSettings.Settings["Count"].Value;
            var Printer = appSettings.Settings["Printer"].Value;

            PrintZPL(txtBox.Text.Trim(), zpl, Count, Printer);
        }
        private void PrintZPL(string boxName, string zpl, string count, string printerName)
        {
            int cnt;
            int.TryParse(count, out cnt);

            for (int i = 0; i < cnt; i++)
            {
                string newZpl = zpl;

                newZpl = newZpl.Replace("#P1#", boxName);
                newZpl = newZpl.Replace("#p1#", boxName);

                try
                {
                    RawPrinterHelper.SendStringToPrinter(printerName, newZpl);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ZPL Printing is failed! " + ex.Message + " ");
                }
            }
        }
        public void RunFromOther()
        {

        }
    }
}
