using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace SCDwindowsForm
{
    public class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:8080/courier") };

        private ListBox lstCouriers;
        private TextBox txtCourierName;
        private TextBox txtCourierEmail;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnShowNoPending;
        private Button btnShowAllCouriers;
        private Button btnSendEmailToCouriers;
        private ListView lstManagers;
        private Button btnRefreshManagers;
        private Label lblName;
        private Label lblEmail;
        private TabControl tabControl;

        public Form1()
        {
            InitializeUI();
            LoadCouriers();
        }

        private void InitializeUI()
        {
            this.Text = "Courier Management System";
            this.Size = new Size(800, 600);

            // Inițializare TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Tab Curieri
            var tabCouriers = new TabPage("Gestionare Curieri");

            lstCouriers = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(540, 200),
                SelectionMode = SelectionMode.MultiExtended  // Permite selecție multiplă
            };

            btnShowNoPending = new Button
            {
                Text = "Curieri Fără Pachete în Așteptare",
                Location = new Point(20, 230),
                Size = new Size(200, 30)
            };
            btnShowNoPending.Click += btnShowNoPending_Click;

            btnShowAllCouriers = new Button
            {
                Text = "Toți Curierii",
                Location = new Point(230, 230),
                Size = new Size(120, 30)
            };
            btnShowAllCouriers.Click += (s, e) => LoadCouriers();

            btnSendEmailToCouriers = new Button
            {
                Text = "Trimite Email Selectați",
                Location = new Point(360, 230),
                Size = new Size(200, 30)
            };
            btnSendEmailToCouriers.Click += btnSendEmailToCouriers_Click;

            lblName = new Label
            {
                Text = "Nume:",
                Location = new Point(20, 280),
                Size = new Size(60, 20)
            };

            txtCourierName = new TextBox
            {
                Location = new Point(90, 280),
                Size = new Size(200, 20)
            };

            lblEmail = new Label
            {
                Text = "Email:",
                Location = new Point(20, 310),
                Size = new Size(60, 20)
            };

            txtCourierEmail = new TextBox
            {
                Location = new Point(90, 310),
                Size = new Size(200, 20)
            };

            btnAdd = new Button
            {
                Text = "Adaugă Curier",
                Location = new Point(90, 350),
                Size = new Size(120, 30)
            };
            btnAdd.Click += btnAddCourier_Click;

            btnDelete = new Button
            {
                Text = "Șterge Curier",
                Location = new Point(220, 350),
                Size = new Size(120, 30)
            };
            btnDelete.Click += btnDeleteCourier_Click;

            tabCouriers.Controls.AddRange(new Control[]
            {
                lstCouriers,
                btnShowNoPending,
                btnShowAllCouriers,
                btnSendEmailToCouriers,
                lblName,
                txtCourierName,
                lblEmail,
                txtCourierEmail,
                btnAdd,
                btnDelete
            });

            // Tab Manageri și Statistici
            var tabManagers = new TabPage("Manageri și Statistici");

            lstManagers = new ListView
            {
                Location = new Point(20, 20),
                Size = new Size(540, 400),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lstManagers.Columns.Add("Manager", 300);
            lstManagers.Columns.Add("Număr Pachete Livrate", 200);

            btnRefreshManagers = new Button
            {
                Text = "Actualizează Statistici",
                Location = new Point(20, 430),
                Size = new Size(150, 30)
            };
            btnRefreshManagers.Click += btnRefreshManagers_Click;

            tabManagers.Controls.AddRange(new Control[] { lstManagers, btnRefreshManagers });

            // Adăugare taburi
            tabControl.Controls.AddRange(new Control[] { tabCouriers, tabManagers });
            this.Controls.Add(tabControl);
        }

        private async void LoadCouriers()
        {
            try
            {
                var response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var couriers = JsonSerializer.Deserialize<List<Courier>>(json);

                lstCouriers.Items.Clear();
                foreach (var courier in couriers)
                {
                    lstCouriers.Items.Add($"{courier.courier_id}: {courier.name} - {courier.email}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea curierilor: {ex.Message}", "Eroare",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnShowNoPending_Click(object sender, EventArgs e)
        {
            try
            {
                var response = await client.GetAsync("courier/getAllCouriersNoPending");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var couriers = JsonSerializer.Deserialize<List<Courier>>(json);

                lstCouriers.Items.Clear();
                foreach (var courier in couriers)
                {
                    lstCouriers.Items.Add($"{courier.courier_id}: {courier.name} - {courier.email}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea curierilor fără pachete în așteptare: {ex.Message}",
                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRefreshManagers_Click(object sender, EventArgs e)
        {
            try
            {
                var response = await client.GetAsync("getAllManagersAndDeliveredPackages");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var managersData = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

                lstManagers.Items.Clear();
                foreach (var manager in managersData)
                {
                    var item = new ListViewItem(manager.Key);
                    item.SubItems.Add(manager.Value.ToString());
                    lstManagers.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea statisticilor: {ex.Message}",
                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnAddCourier_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourierName.Text) ||
                string.IsNullOrWhiteSpace(txtCourierEmail.Text))
            {
                MessageBox.Show("Completați toate câmpurile!", "Atenție",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var courier = new Courier
                {
                    name = txtCourierName.Text,
                    email = txtCourierEmail.Text
                };

                var json = JsonSerializer.Serialize(courier);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("", content);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Curier adăugat cu succes!", "Succes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtCourierName.Clear();
                txtCourierEmail.Clear();
                LoadCouriers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la adăugarea curierului: {ex.Message}", "Eroare",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDeleteCourier_Click(object sender, EventArgs e)
        {
            if (lstCouriers.SelectedItem == null)
            {
                MessageBox.Show("Selectați un curier pentru ștergere.", "Atenție",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = lstCouriers.SelectedItem.ToString();
            var courierId = selectedItem.Split(':')[0];

            try
            {
                var response = await client.DeleteAsync($"?id={courierId}");
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Curier șters cu succes!", "Succes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCouriers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la ștergerea curierului: {ex.Message}", "Eroare",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSendEmailToCouriers_Click(object sender, EventArgs e)
        {
            if (lstCouriers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Selectați cel puțin un curier!", "Atenție",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var emailForm = new Form())
            {
                emailForm.Text = "Trimite Email";
                emailForm.Size = new Size(500, 400);
                emailForm.StartPosition = FormStartPosition.CenterParent;

                var lblSubject = new Label
                {
                    Text = "Subiect:",
                    Location = new Point(10, 10),
                    Size = new Size(60, 20)
                };

                var txtSubject = new TextBox
                {
                    Location = new Point(10, 30),
                    Size = new Size(460, 20)
                };

                var lblBody = new Label
                {
                    Text = "Mesaj:",
                    Location = new Point(10, 60),
                    Size = new Size(60, 20)
                };

                var txtBody = new RichTextBox
                {
                    Location = new Point(10, 80),
                    Size = new Size(460, 200)
                };

                var lblRecipients = new Label
                {
                    Text = $"Destinatari selectați: {lstCouriers.SelectedItems.Count}",
                    Location = new Point(10, 290),
                    Size = new Size(460, 20)
                };

                var btnSend = new Button
                {
                    Text = "Trimite",
                    Location = new Point(200, 320),
                    Size = new Size(100, 30),
                    DialogResult = DialogResult.OK
                };

                emailForm.Controls.AddRange(new Control[] {
                    lblSubject, txtSubject, lblBody, txtBody, lblRecipients, btnSend
                });

                if (emailForm.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(txtSubject.Text) ||
                        string.IsNullOrWhiteSpace(txtBody.Text))
                    {
                        MessageBox.Show("Completați atât subiectul cât și mesajul!", "Atenție",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        var selectedEmails = lstCouriers.SelectedItems
                            .Cast<string>()
                            .Select(item => item.Split('-').Last().Trim())
                            .ToList();

                        var emailData = new
                        {
                            recipients = selectedEmails,
                            subject = txtSubject.Text,
                            body = txtBody.Text
                        };

                        var json = JsonSerializer.Serialize(emailData);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("courier/sendEmail", content);
                        response.EnsureSuccessStatusCode();

                        MessageBox.Show("Email-urile au fost trimise cu succes!", "Succes",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Eroare la trimiterea email-urilor: {ex.Message}",
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    public class Courier
    {
        public int courier_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public Courier manager { get; set; }
    }
}