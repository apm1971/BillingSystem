using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class CompanyForm : Form
    {
        private readonly Company _company;
        private readonly bool _isEditMode;

        // Constructor for new company
        public CompanyForm()
        {
            InitializeComponent();
            
            // Create a completely new company with empty values
            _company = new Company
            {
                CompanyID = 0,
                CompanyName = string.Empty,
                PrintName = string.Empty,
                Address = string.Empty,
                City = string.Empty,
                IsActive = true
            };
            
            // Set financial year dates
            var (startDate, endDate) = CompanyService.GenerateDefaultFinancialYear();
            _company.FinancialYearStart = startDate;
            _company.FinancialYearEnd = endDate;
            
            _isEditMode = false;
            this.Text = "Create Company";
        }

        // Constructor for editing existing company
        public CompanyForm(Company company)
        {
            InitializeComponent();
            
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _isEditMode = true;
            
            this.Text = "Edit Company";
        }

        private void InitializeComponent()
        {
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.lblPrintName = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.lblCity = new System.Windows.Forms.Label();
            this.lblFinancialYearStart = new System.Windows.Forms.Label();
            this.lblFinancialYearEnd = new System.Windows.Forms.Label();
            this.txtCompanyName = new System.Windows.Forms.TextBox();
            this.txtPrintName = new System.Windows.Forms.TextBox();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.txtCity = new System.Windows.Forms.TextBox();
            this.dtpFinancialYearStart = new System.Windows.Forms.DateTimePicker();
            this.dtpFinancialYearEnd = new System.Windows.Forms.DateTimePicker();
            this.chkSetAsActive = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblCompanyName
            // 
            this.lblCompanyName.Location = new System.Drawing.Point(12, 15);
            this.lblCompanyName.Name = "lblCompanyName";
            this.lblCompanyName.Size = new System.Drawing.Size(120, 23);
            this.lblCompanyName.TabIndex = 0;
            this.lblCompanyName.Text = "Company Name:";
            this.lblCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPrintName
            // 
            this.lblPrintName.Location = new System.Drawing.Point(12, 45);
            this.lblPrintName.Name = "lblPrintName";
            this.lblPrintName.Size = new System.Drawing.Size(120, 23);
            this.lblPrintName.TabIndex = 1;
            this.lblPrintName.Text = "Print Name:";
            this.lblPrintName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAddress
            // 
            this.lblAddress.Location = new System.Drawing.Point(12, 75);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(120, 23);
            this.lblAddress.TabIndex = 2;
            this.lblAddress.Text = "Address:";
            this.lblAddress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCity
            // 
            this.lblCity.Location = new System.Drawing.Point(12, 105);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(120, 23);
            this.lblCity.TabIndex = 3;
            this.lblCity.Text = "City:";
            this.lblCity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFinancialYearStart
            // 
            this.lblFinancialYearStart.Location = new System.Drawing.Point(12, 135);
            this.lblFinancialYearStart.Name = "lblFinancialYearStart";
            this.lblFinancialYearStart.Size = new System.Drawing.Size(120, 23);
            this.lblFinancialYearStart.TabIndex = 4;
            this.lblFinancialYearStart.Text = "Financial Year Start:";
            this.lblFinancialYearStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFinancialYearEnd
            // 
            this.lblFinancialYearEnd.Location = new System.Drawing.Point(12, 165);
            this.lblFinancialYearEnd.Name = "lblFinancialYearEnd";
            this.lblFinancialYearEnd.Size = new System.Drawing.Size(120, 23);
            this.lblFinancialYearEnd.TabIndex = 5;
            this.lblFinancialYearEnd.Text = "Financial Year End:";
            this.lblFinancialYearEnd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCompanyName
            // 
            this.txtCompanyName.Location = new System.Drawing.Point(138, 17);
            this.txtCompanyName.Name = "txtCompanyName";
            this.txtCompanyName.Size = new System.Drawing.Size(250, 20);
            this.txtCompanyName.TabIndex = 6;
            // 
            // txtPrintName
            // 
            this.txtPrintName.Location = new System.Drawing.Point(138, 47);
            this.txtPrintName.Name = "txtPrintName";
            this.txtPrintName.Size = new System.Drawing.Size(250, 20);
            this.txtPrintName.TabIndex = 7;
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(138, 77);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(250, 20);
            this.txtAddress.TabIndex = 8;
            // 
            // txtCity
            // 
            this.txtCity.Location = new System.Drawing.Point(138, 107);
            this.txtCity.Name = "txtCity";
            this.txtCity.Size = new System.Drawing.Size(250, 20);
            this.txtCity.TabIndex = 9;
            // 
            // dtpFinancialYearStart
            // 
            this.dtpFinancialYearStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFinancialYearStart.Location = new System.Drawing.Point(138, 137);
            this.dtpFinancialYearStart.Name = "dtpFinancialYearStart";
            this.dtpFinancialYearStart.Size = new System.Drawing.Size(120, 20);
            this.dtpFinancialYearStart.TabIndex = 10;
            // 
            // dtpFinancialYearEnd
            // 
            this.dtpFinancialYearEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFinancialYearEnd.Location = new System.Drawing.Point(138, 167);
            this.dtpFinancialYearEnd.Name = "dtpFinancialYearEnd";
            this.dtpFinancialYearEnd.Size = new System.Drawing.Size(120, 20);
            this.dtpFinancialYearEnd.TabIndex = 11;
            // 
            // chkSetAsActive
            // 
            this.chkSetAsActive.AutoSize = true;
            this.chkSetAsActive.Location = new System.Drawing.Point(138, 197);
            this.chkSetAsActive.Name = "chkSetAsActive";
            this.chkSetAsActive.Size = new System.Drawing.Size(163, 17);
            this.chkSetAsActive.TabIndex = 12;
            this.chkSetAsActive.Text = "Set as active company";
            this.chkSetAsActive.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(138, 230);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 30);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(268, 230);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 30);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CompanyForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(404, 275);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkSetAsActive);
            this.Controls.Add(this.dtpFinancialYearEnd);
            this.Controls.Add(this.dtpFinancialYearStart);
            this.Controls.Add(this.txtCity);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.txtPrintName);
            this.Controls.Add(this.txtCompanyName);
            this.Controls.Add(this.lblFinancialYearEnd);
            this.Controls.Add(this.lblFinancialYearStart);
            this.Controls.Add(this.lblCity);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.lblPrintName);
            this.Controls.Add(this.lblCompanyName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompanyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Company";
            this.Load += new System.EventHandler(this.CompanyForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Label lblPrintName;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.Label lblFinancialYearStart;
        private System.Windows.Forms.Label lblFinancialYearEnd;
        private System.Windows.Forms.TextBox txtCompanyName;
        private System.Windows.Forms.TextBox txtPrintName;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.TextBox txtCity;
        private System.Windows.Forms.DateTimePicker dtpFinancialYearStart;
        private System.Windows.Forms.DateTimePicker dtpFinancialYearEnd;
        private System.Windows.Forms.CheckBox chkSetAsActive;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private void CompanyForm_Load(object sender, EventArgs e)
        {
            if (_isEditMode)
            {
                // Populate fields with company data
                txtCompanyName.Text = _company.CompanyName;
                txtPrintName.Text = _company.PrintName;
                txtAddress.Text = _company.Address;
                txtCity.Text = _company.City;
                dtpFinancialYearStart.Value = _company.FinancialYearStart;
                dtpFinancialYearEnd.Value = _company.FinancialYearEnd;
                chkSetAsActive.Checked = _company.IsActive;
                
                // Disable start date in edit mode
                dtpFinancialYearStart.Enabled = false;
            }
            else
            {
                // IMPORTANT: Force clear all text fields for new company
                txtCompanyName.Clear();
                txtPrintName.Clear();
                txtAddress.Clear();
                txtCity.Clear();
                
                // Set default dates for new company
                dtpFinancialYearStart.Value = _company.FinancialYearStart;
                dtpFinancialYearEnd.Value = _company.FinancialYearEnd;
                chkSetAsActive.Checked = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            // For a new company, create a completely new instance to avoid any potential data sharing
            Company companyToSave;
            
            if (!_isEditMode)
            {
                // Create a fresh company object with values from the form
                companyToSave = new Company
                {
                    CompanyID = 0,
                    CompanyName = txtCompanyName.Text.Trim(),
                    PrintName = txtPrintName.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    City = txtCity.Text.Trim(),
                    FinancialYearStart = dtpFinancialYearStart.Value,
                    FinancialYearEnd = dtpFinancialYearEnd.Value,
                    IsActive = chkSetAsActive.Checked,
                    CreatedOn = DateTime.Now
                };
            }
            else
            {
                // Update existing company object with form values
                companyToSave = _company;
                companyToSave.CompanyName = txtCompanyName.Text.Trim();
                companyToSave.PrintName = txtPrintName.Text.Trim();
                companyToSave.Address = txtAddress.Text.Trim();
                companyToSave.City = txtCity.Text.Trim();
                companyToSave.FinancialYearEnd = dtpFinancialYearEnd.Value;
                companyToSave.IsActive = chkSetAsActive.Checked;
            }

            // Validate financial year dates
            if (companyToSave.FinancialYearEnd <= companyToSave.FinancialYearStart)
            {
                MessageBox.Show("Financial year end date must be after start date.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Save company
                if (CompanyService.SaveCompany(companyToSave))
                {
                    MessageBox.Show("Company saved successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving company: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInputs()
        {
            // Validate company name
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Company name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCompanyName.Focus();
                return false;
            }

            // Validate print name
            if (string.IsNullOrWhiteSpace(txtPrintName.Text))
            {
                MessageBox.Show("Print name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPrintName.Focus();
                return false;
            }

            // Financial year validation
            if (_company.FinancialYearEnd <= _company.FinancialYearStart)
            {
                MessageBox.Show("Financial year end date must be after start date.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dtpFinancialYearEnd.Focus();
                return false;
            }

            return true;
        }
    }
} 