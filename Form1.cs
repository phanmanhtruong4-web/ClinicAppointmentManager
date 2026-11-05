using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClinicAppointmentManager
{
    public partial class Form1 : Form
    {
        private BindingList<Appointment> appointmentList;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeComponent();
            InitializeAppData();
            SetupDataGridView();
            BindControls();
        }

        private void InitializeAppData()
        {
            appointmentList = new BindingList<Appointment>
            {
                new Appointment { AppointmentId = "LH001", PatientName = "Nguyễn Văn A", PhoneNumber = "0901234567", AppointmentDate = DateTime.Now.AddHours(1), Doctor = "BS. Trần Văn Bình", Reason = "Đau đầu, chóng mặt", Status = "Chờ khám" },
                new Appointment { AppointmentId = "LH002", PatientName = "Trần Thị B", PhoneNumber = "0912345678", AppointmentDate = DateTime.Now.AddHours(2), Doctor = "BS. Lê Thị Hoa", Reason = "Khám tổng quát định kỳ", Status = "Đang khám" }
            };

            bindingSource = new BindingSource(appointmentList, null);
            dgvAppointments.DataSource = bindingSource;

            cmbDoctor.Items.AddRange(new string[] { "BS. Trần Văn Bình", "BS. Lê Thị Hoa", "BS. Nguyễn Hoàng Long", "BS. Phạm Thị Mai" });
            cmbStatus.Items.AddRange(new string[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            
            cmbFilterStatus.Items.Add("Tất cả trạng thái");
            cmbFilterStatus.Items.AddRange(new string[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cmbFilterStatus.SelectedIndex = 0;

            UpdateStatusBar();
        }

        private void SetupDataGridView()
        {
            dgvAppointments.AutoGenerateColumns = false;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.Columns.Clear();

            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentId", HeaderText = "Mã lịch hẹn", Width = 90 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PatientName", HeaderText = "Họ tên bệnh nhân", Width = 150 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PhoneNumber", HeaderText = "Số điện thoại", Width = 110 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AppointmentDate", HeaderText = "Ngày giờ hẹn", Width = 130, DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" } });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Doctor", HeaderText = "Bác sĩ", Width = 140 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Reason", HeaderText = "Lý do khám", Width = 180 });
            dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Trạng thái", Width = 100 });
        }

        private void BindControls()
        {
            dgvAppointments.SelectionChanged += (s, e) =>
            {
                if (dgvAppointments.CurrentRow?.DataBoundItem is Appointment current)
                {
                    txtAppointmentId.Text = current.AppointmentId;
                    txtPatientName.Text = current.PatientName;
                    txtPhoneNumber.Text = current.PhoneNumber;
                    dtpAppointmentDate.Value = current.AppointmentDate > dtpAppointmentDate.MinDate ? current.AppointmentDate : DateTime.Now;
                    cmbDoctor.Text = current.Doctor;
                    txtReason.Text = current.Reason;
                    cmbStatus.Text = current.Status;
                }
            };
        }

        private bool ValidateData()
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtAppointmentId.Text))
            {
                errorProvider1.SetError(txtAppointmentId, "Mã lịch hẹn không được để trống.");
                isValid = false;
            }
            else if (appointmentList.Any(a => a.AppointmentId == txtAppointmentId.Text.Trim() && (dgvAppointments.CurrentRow == null || ((Appointment)dgvAppointments.CurrentRow.DataBoundItem).AppointmentId != txtAppointmentId.Text.Trim())))
            {
                errorProvider1.SetError(txtAppointmentId, "Mã lịch hẹn đã tồn tại.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtPatientName.Text))
            {
                errorProvider1.SetError(txtPatientName, "Họ tên bệnh nhân bắt buộc phải có.");
                isValid = false;
            }

            if (!Regex.IsMatch(txtPhoneNumber.Text.Trim(), @"^0\d{9,10}$"))
            {
                errorProvider1.SetError(txtPhoneNumber, "Số điện thoại không hợp lệ.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                errorProvider1.SetError(txtReason, "Lý do khám bắt buộc phải nhập.");
                isValid = false;
            }

            if (dtpAppointmentDate.Value < DateTime.Now)
            {
                errorProvider1.SetError(dtpAppointmentDate, "Ngày giờ hẹn không được nằm trong quá khứ.");
                isValid = false;
            }

            return isValid;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateData()) return;

            var newAppt = new Appointment
            {
                AppointmentId = txtAppointmentId.Text.Trim(),
                PatientName = txtPatientName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                AppointmentDate = dtpAppointmentDate.Value,
                Doctor = cmbDoctor.Text,
                Reason = txtReason.Text.Trim(),
                Status = string.IsNullOrEmpty(cmbStatus.Text) ? "Chờ khám" : cmbStatus.Text
            };

            appointmentList.Add(newAppt);
            UpdateStatusBar();
            ClearInputs();
            MessageBox.Show("Thêm lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow?.DataBoundItem is Appointment current)
            {
                if (!ValidateData()) return;

                current.AppointmentId = txtAppointmentId.Text.Trim();
                current.PatientName = txtPatientName.Text.Trim();
                current.PhoneNumber = txtPhoneNumber.Text.Trim();
                current.AppointmentDate = dtpAppointmentDate.Value;
                current.Doctor = cmbDoctor.Text;
                current.Reason = txtReason.Text.Trim();
                current.Status = cmbStatus.Text;

                bindingSource.ResetBindings(false);
                UpdateStatusBar();
                MessageBox.Show("Cập nhật thông tin lịch hẹn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần cập nhật!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow?.DataBoundItem is Appointment current)
            {
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa lịch hẹn của bệnh nhân [{current.PatientName}]?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    appointmentList.Remove(current);
                    UpdateStatusBar();
                    ClearInputs();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearInputs()
        {
            txtAppointmentId.Clear();
            txtPatientName.Clear();
            txtPhoneNumber.Clear();
            txtReason.Clear();
            dtpAppointmentDate.Value = DateTime.Now;
            if (cmbDoctor.Items.Count > 0) cmbDoctor.SelectedIndex = 0;
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;
            errorProvider1.Clear();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void cmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string selectedStatus = cmbFilterStatus.SelectedItem?.ToString();
            var query = appointmentList.AsEnumerable();

            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Tất cả trạng thái")
            {
                query = query.Where(a => a.Status == selectedStatus);
            }

            string keyword = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.PatientName.ToLower().Contains(keyword) || a.PhoneNumber.Contains(keyword));
            }

            bindingSource.DataSource = new BindingList<Appointment>(query.ToList());
            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            int count = bindingSource.Count;
            toolStripStatusLabelCount.Text = $"Tổng số lịch hẹn hiển thị: {count}";
        }

        private void menuExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = $"LichHen_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("Mã lịch hẹn,Họ tên bệnh nhân,Số điện thoại,Ngày giờ hẹn,Bác sĩ,Lý do khám,Trạng thái");

                            foreach (var item in bindingSource.List)
                            {
                                if (item is Appointment a)
                                {
                                    sw.WriteLine($"\"{a.AppointmentId}\",\"{a.PatientName}\",\"{a.PhoneNumber}\",\"{a.AppointmentDate:dd/MM/yyyy HH:mm}\",\"{a.Doctor}\",\"{a.Reason}\",\"{a.Status}\"");
                                }
                            }
                        }
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
