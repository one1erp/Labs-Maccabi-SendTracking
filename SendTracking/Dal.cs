using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms;

namespace SendTracking
{
    class Dal
    {

        private OracleConnection _connection;

        private OracleCommand cmd;

        private string sql;

        public Dal(OracleCommand cmd, OracleConnection _connection)
        {
            this.cmd = cmd;
            this._connection = _connection;
        }

        public string getLab_Name(string labName)
        {
            sql = string.Format("select lims.phrase_lookup (  'Lab Location' , '{0}' ) lab_name from dual", labName);
            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                MessageBox.Show("The Lab " +
                     labName +
                    "  does not exist or does not meet the conditions!", "Nautilus",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //txtEditEntity.Focus();
                return null;
            }
            else
            {
                while (reader.Read())
                {
                    return reader["LAB_NAME"].ToString();
                }
                return null;
            }
        }

        public string getBoxes(string from_lab, string to_lab, string boxName)
        {
            //where bu.u_status = 'B'
            sql = string.Format("select b.u_box_id,bu.U_STATUS from LIMS_SYS.u_box b " +
                "inner join LIMS_SYS.u_box_user bu on b.u_box_id = bu.u_box_id " +
                "where bu.u_from_lab = '{0}' and bu.u_to_lab = '{1}' and b.name = '{2}'", from_lab, to_lab, boxName);

            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                MessageBox.Show("ארגז לא נמצא","",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return null;
            }
            else
            {
                while (reader.Read())
                {
                    string status = reader["U_STATUS"].ToString();
                    if (status != "B")
                    {
                        MessageBox.Show("ארגז לא בסטטוס המתאים", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    else
                    {
                        return reader["u_box_id"].ToString();
                    }
                }
                return null;
            }
        }


        public bool updateBoxStatus(string boxId, string statusLabTrack)
        {
            //List<TrackEntities> entities = new List<TrackEntities>() { };
            OracleTransaction transaction = null;
            try
            {
                transaction = _connection.BeginTransaction();
                cmd.Connection = _connection;
                cmd.Transaction = transaction;

                //get all entities
                sql = string.Format("select U_TRACK_TABLE_NAME,U_TRACK_ITEM_ID from LIMS_SYS.U_LAB_TRACK_USER where U_BOX =  '{0}'", boxId);

                cmd = new OracleCommand(sql, _connection);
                OracleDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    MessageBox.Show("לא נמצאו ישויות בארגז זה", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                //update box
                sql = string.Format("update LIMS_SYS.U_BOX_USER set U_STATUS = '{0}',U_SEND_ON = {1} where U_BOX_ID = '{2}'", "C","SYSDATE", boxId);
                cmd = new OracleCommand(sql, _connection);
                cmd.ExecuteNonQuery();

                transaction.Commit();

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception en)
                {
                    MessageBox.Show(en.Message); ;
                }
                return false;
            }

        }
    }
}
