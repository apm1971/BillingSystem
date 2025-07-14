using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class CompanyListForm : Form
    {
        private List<Company> companies;
        
        public CompanyListForm()
        {
            InitializeComponent();
            LoadCompanies();
        }

        private void InitializeComponent()
        {
            this.dgvCompanies = new System.Windows.Forms.DataGridView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnSetActive = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCompanies)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCompanies
            // 
            this.dgvCompanies.AllowUserToAddRows = false;
            this.dgvCompanies.AllowUserToDeleteRows = false;
            this.dgvCompanies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCompanies.BackgroundColor = System.Drawing.Color.White;
            this.dgvCompanies.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCompanies.Location = new System.Drawing.Point(12, 12);
            this.dgvCompanies.MultiSelect = false;
            this.dgvCompanies.Name = "dgvCompanies";
            this.dgvCompanies.ReadOnly = true;
            this.dgvCompanies.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCompanies.Size = new System.Drawing.Size(676, 387);
            this.dgvCompanies.TabIndex = 0;
            this.dgvCompanies.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCompanies_CellDoubleClick);
            this.dgvCompanies.SelectionChanged += new System.EventHandler(this.dgvCompanies_SelectionChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(12, 412);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(120, 30);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add Company";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(138, 412);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(120, 30);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Edit Company";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnSetActive
            // 
            this.btnSetActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetActive.Enabled = false;
            this.btnSetActive.Location = new System.Drawing.Point(264, 412);
            this.btnSetActive.Name = "btnSetActive";
            this.btnSetActive.Size = new System.Drawing.Size(120, 30);
            this.btnSetActive.TabIndex = 3;
            this.btnSetActive.Text = "Set as Active";
            this.btnSetActive.UseVisualStyleBackColor = true;
            this.btnSetActive.Click += new System.EventHandler(this.btnSetActive_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(568, 412);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // CompanyListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(700, 454);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSetActive);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dgvCompanies);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "CompanyListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Company List";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCompanies)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvCompanies;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnSetActive;
        private System.Windows.Forms.Button btnClose;

        private void LoadCompanies()
        {
            try
            {
                companies = CompanyService.GetAllCompanies();
                
                dgvCompanies.AutoGenerateColumns = false;
                dgvCompanies.Columns.Clear();
                
                // Add columns
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "CompanyID",
                    HeaderText = "ID",
                    DataPropertyName = "CompanyID",
                    Width = 50,
                    Visible = false
                });
                
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "CompanyName",
                    HeaderText = "Company Name",
                    DataPropertyName = "CompanyName",
                    Width = 150
                });
                
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "PrintName",
                    HeaderText = "Print Name",
                    DataPropertyName = "PrintName",
                    Width = 150
                });
                
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Address",
                    HeaderText = "Address",
                    DataPropertyName = "Address",
                    Width = 200
                });
                
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "City",
                    HeaderText = "City",
                    DataPropertyName = "City",
                    Width = 100
                });
                
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "FinancialYearStart",
                    HeaderText = "FY Start",
                    DataPropertyName = "FinancialYearStart",
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
                });
                
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "FinancialYearEnd",
                    HeaderText = "FY End",
                    DataPropertyName = "FinancialYearEnd",
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
                });
                
                // Use a text column instead of checkbox for IsActive to avoid binding issues
                dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "IsActiveText",
                    HeaderText = "Active",
                    Width = 60
                });
                
                // Manually populate the grid to avoid binding issues
                dgvCompanies.Rows.Clear();
                foreach (var company in companies)
                {
                    int rowIndex = dgvCompanies.Rows.Add();
                    DataGridViewRow row = dgvCompanies.Rows[rowIndex];
                    
                    row.Cells["CompanyID"].Value = company.CompanyID;
                    row.Cells["CompanyName"].Value = company.CompanyName;
                    row.Cells["PrintName"].Value = company.PrintName;
                    row.Cells["Address"].Value = company.Address;
                    row.Cells["City"].Value = company.City;
                    row.Cells["FinancialYearStart"].Value = company.FinancialYearStart;
                    row.Cells["FinancialYearEnd"].Value = company.FinancialYearEnd;
                    row.Cells["IsActiveText"].Value = company.IsActive ? "Yes" : "No";
                    
                    // Apply formatting based on IsActive
                    if (company.IsActive)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.Font = new Font(dgvCompanies.Font, FontStyle.Bold);
                    }
                }
                
                // We're handling formatting manually in the row creation code above
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading companies: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Remove or comment out the dgvCompanies_CellFormatting method since we're handling formatting manually
        /*
        private void dgvCompanies_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // This method is no longer needed
        }
        */

        private void dgvCompanies_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvCompanies.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            
            if (hasSelection)
            {
                int selectedIndex = dgvCompanies.SelectedRows[0].Index;
                if (selectedIndex < companies.Count)
                {
                    bool isActive = companies[selectedIndex].IsActive;
                    btnSetActive.Enabled = !isActive;
                    btnSetActive.Text = isActive ? "Already Active" : "Set as Active";
                }
                else
                {
                    btnSetActive.Enabled = false;
                    btnSetActive.Text = "Set as Active";
                }
            }
            else
            {
                btnSetActive.Enabled = false;
                btnSetActive.Text = "Set as Active";
            }
        }

        private void dgvCompanies_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedCompany();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new CompanyForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadCompanies();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelectedCompany();
        }

        private void EditSelectedCompany()
        {
            if (dgvCompanies.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvCompanies.SelectedRows[0].Index;
                Company selectedCompany = companies[selectedIndex];
                
                // Create a deep copy of the company to avoid reference issues
                Company companyCopy = new Company
                {
                    CompanyID = selectedCompany.CompanyID,
                    CompanyName = selectedCompany.CompanyName,
                    PrintName = selectedCompany.PrintName,
                    Address = selectedCompany.Address,
                    City = selectedCompany.City,
                    FinancialYearStart = selectedCompany.FinancialYearStart,
                    FinancialYearEnd = selectedCompany.FinancialYearEnd,
                    IsActive = selectedCompany.IsActive,
                    CreatedOn = selectedCompany.CreatedOn
                };
                
                using (var form = new CompanyForm(companyCopy))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadCompanies();
                    }
                }
            }
        }

        private void btnSetActive_Click(object sender, EventArgs e)
        {
            if (dgvCompanies.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvCompanies.SelectedRows[0].Index;
                Company selectedCompany = companies[selectedIndex];
                
                // Create a deep copy of the company to avoid reference issues
                Company companyCopy = new Company
                {
                    CompanyID = selectedCompany.CompanyID,
                    CompanyName = selectedCompany.CompanyName,
                    PrintName = selectedCompany.PrintName,
                    Address = selectedCompany.Address,
                    City = selectedCompany.City,
                    FinancialYearStart = selectedCompany.FinancialYearStart,
                    FinancialYearEnd = selectedCompany.FinancialYearEnd,
                    IsActive = true, // Set as active
                    CreatedOn = selectedCompany.CreatedOn
                };
                
                try
                {
                    if (CompanyService.SaveCompany(companyCopy))
                    {
                        // Update the active company in Program
                        Program.ActiveCompany = companyCopy;
                        
                        MessageBox.Show($"{companyCopy.CompanyName} set as active company.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCompanies();
                        
                        // Set dialog result to OK to indicate success
                        this.DialogResult = DialogResult.OK;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error setting company as active: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 