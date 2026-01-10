using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class GodownMasterUserControl : UserControl
    {
        private List<Godown> godowns = new List<Godown>();
        private List<Godown> filteredGodowns = new List<Godown>();
        private Godown currentGodown = new Godown();
        private bool isNewGodown = true;

        public GodownMasterUserControl()
        {
            InitializeComponent();
            LoadGodowns();
            ConfigureControls();
            SetupDataGrid();
            
            this.KeyDown += GodownMasterUserControl_KeyDown;
        }

        private void GodownMasterUserControl_Load(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ConfigureControls()
        {
            txtSearch.TabIndex = 0;
            txtGodownName.TabIndex = 1;
            txtGodownShortName.TabIndex = 2;
            btnSave.TabIndex = 3;
            btnNew.TabIndex = 4;
            btnDelete.TabIndex = 5;
            
            txtGodownName.CharacterCasing = CharacterCasing.Upper;
            txtGodownShortName.CharacterCasing = CharacterCasing.Upper;

            txtGodownName.MaxLength = 255;
            txtGodownShortName.MaxLength = 50;

            txtSearch.TextChanged += txtSearch_TextChanged;
            
            txtSearch.KeyDown += txtSearch_KeyDown;
            txtGodownName.KeyDown += Control_KeyDown;
            txtGodownShortName.KeyDown += Control_KeyDown;
            dgvGodowns.KeyDown += Control_KeyDown;
            
            txtSearch.PlaceholderText = "Type to search godowns...";
            txtGodownName.PlaceholderText = "Enter godown name";
            txtGodownShortName.PlaceholderText = "Enter short name";
            
            SetupButtonStyle(btnSave, Color.FromArgb(0, 122, 204));
            SetupButtonStyle(btnNew, Color.FromArgb(40, 167, 69));
            SetupButtonStyle(btnDelete, Color.FromArgb(220, 53, 69));
            
            btnSave.Text = "Save (Ctrl+S)";
            btnNew.Text = "New (Ctrl+N)";
            btnDelete.Text = "Delete (F8)";
        }

        private void SetupButtonStyle(Button button, Color baseColor)
        {
            button.BackColor = baseColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            
            button.MouseEnter += (s, e) => {
                button.BackColor = Color.FromArgb(
                    Math.Min(255, baseColor.R + 20),
                    Math.Min(255, baseColor.G + 20),
                    Math.Min(255, baseColor.B + 20)
                );
            };
            
            button.MouseLeave += (s, e) => {
                button.BackColor = baseColor;
            };
        }

        private void SetupDataGrid()
        {
            dgvGodowns.AutoGenerateColumns = false;
            dgvGodowns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGodowns.AllowUserToAddRows = false;
            dgvGodowns.AllowUserToDeleteRows = false;
            dgvGodowns.ReadOnly = true;
            dgvGodowns.MultiSelect = false;
            dgvGodowns.RowHeadersVisible = false;
            dgvGodowns.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 245, 245)
            };

            dgvGodowns.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvGodowns.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvGodowns.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvGodowns.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvGodowns.ColumnHeadersHeight = 35;
            dgvGodowns.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvGodowns.RowTemplate.Height = 30;

            dgvGodowns.Columns.Clear();

            dgvGodowns.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GodownID",
                HeaderText = "ID",
                Width = 60,
                Visible = false
            });

            dgvGodowns.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GodownName",
                HeaderText = "Godown Name",
                Width = 300,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvGodowns.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GodownShortName",
                HeaderText = "Short Name",
                Width = 200,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            typeof(DataGridView).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvGodowns, new object[] { true });

            dgvGodowns.SelectionChanged += dgvGodowns_SelectionChanged;
            dgvGodowns.CellDoubleClick += dgvGodowns_CellDoubleClick;
        }

        private void LoadGodowns()
        {
            try
            {
                godowns = GodownService.GetAllGodowns();
                dgvGodowns.DataSource = null;
                dgvGodowns.DataSource = godowns;

                lblTotalGodowns.Text = $"Total Godowns: {godowns.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading godowns: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            currentGodown = new Godown();
            isNewGodown = true;

            txtGodownName.Text = string.Empty;
            txtGodownShortName.Text = string.Empty;

            txtGodownName.Focus();
            btnDelete.Enabled = false;
        }

        /// <summary>
        /// Public method to start adding a new godown (for quick-add scenarios)
        /// </summary>
        public void StartNewGodown()
        {
            // Clear grid selection first to prevent SelectionChanged from re-populating
            dgvGodowns.ClearSelection();
            ClearForm();
        }

        private void PopulateForm(Godown godown)
        {
            currentGodown = godown;
            isNewGodown = false;

            txtGodownName.Text = godown.GodownName;
            txtGodownShortName.Text = godown.GodownShortName;

            btnDelete.Enabled = true;
        }

        private Godown GetGodownFromForm()
        {
            return new Godown
            {
                GodownID = currentGodown.GodownID,
                GodownName = txtGodownName.Text.Trim(),
                GodownShortName = txtGodownShortName.Text.Trim()
            };
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtGodownName.Text))
            {
                MessageBox.Show("Please enter Godown Name", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGodownName.Focus();
                return false;
            }

            if (isNewGodown && GodownService.GodownExists(txtGodownName.Text.Trim()))
            {
                MessageBox.Show("A godown with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGodownName.Focus();
                return false;
            }

            if (!isNewGodown && GodownService.GodownExists(txtGodownName.Text.Trim(), currentGodown.GodownID))
            {
                MessageBox.Show("A godown with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGodownName.Focus();
                return false;
            }

            return true;
        }

        #region Event Handlers

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                Godown godown = GetGodownFromForm();
                bool success;

                if (isNewGodown)
                {
                    success = GodownService.AddGodown(godown);
                    if (success)
                        MessageBox.Show("Godown added successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    success = GodownService.UpdateGodown(godown);
                    if (success)
                        MessageBox.Show("Godown updated successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (success)
                {
                    LoadGodowns();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving godown: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentGodown.GodownID == 0)
                return;

            if (MessageBox.Show("Are you sure you want to delete this godown?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = GodownService.DeleteGodown(currentGodown.GodownID);

                    if (success)
                    {
                        MessageBox.Show("Godown deleted successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadGodowns();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete godown. It may have transactions or opening stock.", "Delete Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting godown: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvGodowns_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvGodowns.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvGodowns.SelectedRows[0].Index;
                var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? godowns : filteredGodowns;

                if (selectedIndex >= 0 && selectedIndex < currentList.Count)
                {
                    Godown selectedGodown = currentList[selectedIndex];
                    PopulateForm(selectedGodown);
                }
            }
        }

        private void dgvGodowns_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? godowns : filteredGodowns;

            if (e.RowIndex >= 0 && e.RowIndex < currentList.Count)
            {
                Godown selectedGodown = currentList[e.RowIndex];
                PopulateForm(selectedGodown);
                txtGodownName.Focus();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredGodowns = godowns;
                dgvGodowns.DataSource = godowns;
            }
            else
            {
                filteredGodowns = godowns.FindAll(g => 
                    g.GodownName.ToLower().Contains(searchText) ||
                    (g.GodownShortName != null && g.GodownShortName.ToLower().Contains(searchText))
                );

                dgvGodowns.DataSource = null;
                dgvGodowns.DataSource = filteredGodowns;
            }

            lblTotalGodowns.Text = $"Total Godowns: {filteredGodowns.Count}";

            if (filteredGodowns.Count > 0)
            {
                dgvGodowns.ClearSelection();
                dgvGodowns.Rows[0].Selected = true;
            }
            else
            {
                ClearForm();
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (filteredGodowns.Count > 0)
                {
                    dgvGodowns.ClearSelection();
                    dgvGodowns.Rows[0].Selected = true;
                    PopulateForm(filteredGodowns[0]);
                    txtGodownName.Focus();
                }
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnSave.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnNew.PerformClick();
            }
            else if (e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtGodownName.Focus();
            }
            else if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtSearch.Focus();
            }
            else if (e.KeyCode == Keys.F8)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnDelete.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (txtSearch.Focused && !string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Clear();
                    txtSearch.Focus();
                }
                else if (!txtSearch.Focused)
                {
                    ClearForm();
                    txtSearch.Focus();
                }
            }
        }

        private void GodownMasterUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (txtSearch.Focused && !string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Clear();
                    txtSearch.Focus();
                }
                else if (!txtSearch.Focused)
                {
                    ClearForm();
                    txtSearch.Focus();
                }
                e.Handled = true;
            }
        }

        #endregion
    }
}

