using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using De02.Database;
using System.Data.Entity;


namespace De02
{
    public partial class frmSanpham : Form
    {

        private QLSanpham db = new QLSanpham();

        public frmSanpham()
        {
            InitializeComponent();
        }

        private void frmSanpham_Load(object sender, EventArgs e)
        {
            LoadLoaiSP();
            LoadSanpham();

            SetControlsEnabled(false, false);
        }

        private void lvSanpham_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSanpham.SelectedItems.Count > 0)
            {
                var item = lvSanpham.SelectedItems[0];
                txtMaSP.Text = item.SubItems[0].Text;
                txtTenSP.Text = item.SubItems[1].Text;
                dtNgaynhap.Value = DateTime.Parse(item.SubItems[2].Text);
                cboLoaiSP.Text = item.SubItems[3].Text;

                SetControlsEnabled(false, true);
            }
            else
            {
                SetControlsEnabled(false, false);
                ClearControls();
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(true, false);

            ClearControls();

            txtMaSP.Focus();
        }

        private void ClearControls()
        {
            txtMaSP.Clear();
            txtTenSP.Clear();
            cboLoaiSP.SelectedIndex = 0;
            dtNgaynhap.Value = DateTime.Now;
            txtMaSP.Focus();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSP.Text))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(true, false);

            txtMaSP.Enabled = false;
            txtTenSP.Focus();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            var maSP = txtMaSP.Text;
            var sp = db.Sanphams.FirstOrDefault(s => s.MaSP == maSP);

            if (sp != null && MessageBox.Show("Xác nhận xóa?", "Xóa", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                db.Sanphams.Remove(sp);
                db.SaveChanges();
                LoadSanpham();
                MessageBox.Show("Xóa sản phẩm thành công!");
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSP.Text) ||
        string.IsNullOrWhiteSpace(txtTenSP.Text) ||
        cboLoaiSP.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var sp = db.Sanphams.FirstOrDefault(s => s.MaSP == txtMaSP.Text);
                if (sp != null)
                {
                    // Cập nhật thông tin
                    sp.TenSP = txtTenSP.Text.Trim();
                    sp.NgayNhap = dtNgaynhap.Value;
                    sp.MaLoai = cboLoaiSP.SelectedValue.ToString();
                }
                else
                {
                    // Thêm mới
                    sp = new Sanpham
                    {
                        MaSP = txtMaSP.Text.Trim(),
                        TenSP = txtTenSP.Text.Trim(),
                        NgayNhap = dtNgaynhap.Value,
                        MaLoai = cboLoaiSP.SelectedValue.ToString()
                    };
                    db.Sanphams.Add(sp);
                }

                db.SaveChanges();
                LoadSanpham();

                MessageBox.Show("Lưu thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                SetControlsEnabled(false, false);
                ClearControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnKLuu_Click(object sender, EventArgs e)
        {
            ClearControls();
            SetControlsEnabled(false, false);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng cửa sổ này không?",
                "Xác nhận đóng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                this.Close();
            }
        }


        private void LoadLoaiSP()
        {
            var loaiSPs = db.LoaiSPs.ToList();
            cboLoaiSP.DataSource = loaiSPs;
            cboLoaiSP.DisplayMember = "TenLoai";
            cboLoaiSP.ValueMember = "MaLoai";
        }

        private void LoadSanpham()
        {
            var sanphams = db.Sanphams.Include(s => s.LoaiSP).ToList();
            lvSanpham.Items.Clear();

            foreach (var sp in sanphams)
            {
                ListViewItem item = new ListViewItem(sp.MaSP);
                item.SubItems.Add(sp.TenSP);
                item.SubItems.Add(sp.NgayNhap.HasValue
                    ? sp.NgayNhap.Value.ToString("dd/MM/yyyy")
                    : "");
                item.SubItems.Add(sp.LoaiSP?.TenLoai ?? "");
                lvSanpham.Items.Add(item);
            }
        }

        private void SetControlsEnabled(bool isEnabled, bool allowEdit)
        {
            txtMaSP.Enabled = isEnabled;
            txtTenSP.Enabled = isEnabled;
            dtNgaynhap.Enabled = isEnabled;
            cboLoaiSP.Enabled = isEnabled;

            btnLuu.Enabled = isEnabled;
            btnKLuu.Enabled = isEnabled;

            btnThem.Enabled = !isEnabled;

            btnSua.Enabled = allowEdit;
            btnXoa.Enabled = allowEdit;
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            string keyword = txtTim.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadSanpham();
                return;
            }

            var searchResults = db.Sanphams
                                  .Include(s => s.LoaiSP)
                                  .Where(s => s.TenSP.Contains(keyword))
                                  .ToList();

            lvSanpham.Items.Clear();

            foreach (var sp in searchResults)
            {
                ListViewItem item = new ListViewItem(sp.MaSP);
                item.SubItems.Add(sp.TenSP);
                item.SubItems.Add(sp.NgayNhap.HasValue
                    ? sp.NgayNhap.Value.ToString("dd/MM/yyyy")
                    : "");
                item.SubItems.Add(sp.LoaiSP?.TenLoai ?? "");
                lvSanpham.Items.Add(item);
            }

            if (searchResults.Count == 0)
            {
                MessageBox.Show("Không tìm thấy sản phẩm phù hợp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
