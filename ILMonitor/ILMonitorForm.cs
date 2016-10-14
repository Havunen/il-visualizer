using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClrTest.Reflection {
    public partial class ILMonitorForm : Form {
        public ILMonitorForm() {
            InitializeComponent();
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticleToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (Form childForm in MdiChildren) {
                childForm.Close();
            }
        }

        AbstractXmlDataMonitor<MethodBodyInfo> m_monitor;

        private void ILMonitorForm_Load(object sender, EventArgs e) {
            m_monitor = new TcpDataMonitor<MethodBodyInfo>(22017);

            m_monitor.MonitorStatusChange += new AbstractXmlDataMonitor<MethodBodyInfo>.MonitorStatusChangeEventHandler(OnMonitorStatusChange);
            m_monitor.VisualizerDataReady += new AbstractXmlDataMonitor<MethodBodyInfo>.VisualizerDataReadyEventHandler(OnVisualizerDataReady);

            if (Properties.Settings.Default.AutomaticallyStart) {
                autoStartToolStripMenuItem.Checked = true;
                m_monitor.Start();
            }
        }

        void OnMonitorStatusChange(object sender, MonitorStatusChangeEventArgs e) {
            if (e.Status == MonitorStatus.Monitoring) {
                toolStripStatusLabel.Text = "Monitoring";
                toolStripStatusLabel.ForeColor = Color.Blue;
                startToolStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = true;

            } else {
                toolStripStatusLabel.Text = "Not Monitoring";
                toolStripStatusLabel.ForeColor = Color.Red;
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
            }
        }

        void OnVisualizerDataReady(object sender, VisualizerDataEventArgs<MethodBodyInfo> e) {
            MethodBodyInfo mbi = e.VisualizerData;
            MiniBrowser childForm = FindOrCreateChildForm(mbi);

            IncrementalMethodBodyInfo imbi;
            if (childForm.CurrentData != null)
                imbi = IncrementalMethodBodyInfo.Create(mbi, childForm.CurrentData.LengthHistory);
            else
                imbi = IncrementalMethodBodyInfo.Create(mbi);

            childForm.UpdateWith(imbi);
        }

        MiniBrowser FindOrCreateChildForm(MethodBodyInfo mbi) {
            foreach (Form form in MdiChildren) {
                MiniBrowser miniBrowser = form as MiniBrowser;
                if (miniBrowser == null) continue;
                if (miniBrowser.CurrentData == null) continue;

                if (mbi.Identity == miniBrowser.CurrentData.Identity) {
                    miniBrowser.Focus();
                    return miniBrowser;
                }
            }

            MiniBrowser newChild = new MiniBrowser();
            newChild.Text = mbi.MethodToString;
            newChild.MdiParent = this;
            newChild.Show();
            return newChild;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e) {
            m_monitor.Stop();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e) {
            m_monitor.Start();
        }

        private void ILMonitorForm_FormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings s = Properties.Settings.Default;
            s.AutomaticallyStart = autoStartToolStripMenuItem.Checked;
            s.Save();

            if (stopToolStripMenuItem.Enabled) {
                m_monitor.Stop();
            }
        }

        private void autoStartToolStripMenuItem_Click(object sender, EventArgs e) {
            autoStartToolStripMenuItem.Checked = !autoStartToolStripMenuItem.Checked;
        }

        private void showStatusBarToolStripMenuItem_Click(object sender, EventArgs e) {
            statusStrip.Visible = showStatusBarToolStripMenuItem.Checked;
        }
    }
}