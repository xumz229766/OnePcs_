using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
namespace _OnePcs
{
    public partial class FrmCalibration:UIForm
    {
        int pos = 0;//0为左吸笔标定，1代表右吸笔标定
        BindingList<MeasurePressure> bList;
        public FrmCalibration(int _pos=0)
        {
            pos = _pos;
            InitializeComponent();
        }

        private void FrmCalibration_Load(object sender, EventArgs e)
        {
          
            if (pos == 0)
            {
                this.Text = "左吸笔电器比例阀和压力标定";
                bList = new BindingList<MeasurePressure>(PressureCalibration.lstPressureAndVL);
                this.uiDataGridView1.DataSource = bList;
            }
            else
            {
                this.Text = "右吸笔电器比例阀和压力标定";
                bList = new BindingList<MeasurePressure>(PressureCalibration.lstPressureAndVR);
                this.uiDataGridView1.DataSource = bList;
            }
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if(pos == 0)
            {
                PressureCalibration.homatL = null;
                double v= PressureCalibration.GetVBySuctionL((double)numericUpDown1.Value);
                lblOutV.Text = v.ToString("0.000") + "V";
            }else
            {
                PressureCalibration.homatR = null;
                double v = PressureCalibration.GetVBySuctionR((double)numericUpDown1.Value);
                lblOutV.Text = v.ToString("0.000") + "V";

            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
