using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Candy_Machine
{
    public partial class Admin : Form
    {
        public Admin()
        {
            InitializeComponent();
        }

        // De variabele waar de PIN in wordt opgeslagen
        string PIN = "3001";
        // Input houdt bij wat de gebruiker invult
        string Input = "";
        // Length bepaalt op welke plek de gebruiker aan het typen is (voorbeeld: "000-" betekent dus Length = 3 want op plek 4 moet iets komen)
        int Length;

        CandyMachine frm1;

        private void Admin_Load(object sender, EventArgs e)
        {
            frm1 = Owner as CandyMachine;
            // Bij het inladen krijgt Length de waarde "1" (want Input is nog leeg)
            Length = Input.Length + 1;
        }

        // De 10 buttons met cijfers er op zijn gegroepeerd in een "ButtonClick"
        private void ButtonClick(object sender, EventArgs e)
        {
            // De verzender wordt bepaald
            Button btn = (Button)sender;
            // De string Name krijgt de tekst die op de knop staat. Met behulp van een switch-case wordt bepaald wie het is en wordt de juiste code uitgevoerd
            string Name = btn.Text;

            if (Length <= 4)
            {
                // De string "Input" wordt gevuld met de invoer van de gebruiker
                switch (Name)
                {
                    case "0":
                        Input += "0";
                        break;
                    case "1":
                        Input += "1";
                        break;
                    case "2":
                        Input += "2";
                        break;
                    case "3":
                        Input += "3";
                        break;
                    case "4":
                        Input += "4";
                        break;
                    case "5":
                        Input += "5";
                        break;
                    case "6":
                        Input += "6";
                        break;
                    case "7":
                        Input += "7";
                        break;
                    case "8":
                        Input += "8";
                        break;
                    case "9":
                        Input += "9";
                        break;
                }

                // ... wanneer de invoer van de gebruiker (dus string Input) gelijk is aan de PIN
                if (Input == PIN)
                {
                    btnReset.Enabled = true;
                    btnReset.ForeColor = SystemColors.ControlText;
                    txtPIN.Text = "WELKOM";

                    // De nummers worden uitgeschakeld
                    btn1.Enabled = false;
                    btn2.Enabled = false;
                    btn3.Enabled = false;
                    btn4.Enabled = false;
                    btn5.Enabled = false;
                    btn6.Enabled = false;
                    btn7.Enabled = false;
                    btn8.Enabled = false;
                    btn9.Enabled = false;
                    btn0.Enabled = false;
                }
                else
                {
                    // Deze switch-case zorgt ervoor dat de tekst die de gebruiker ziet, geloofwaardig lijkt.
                    switch (Length)
                    {
                        case 1:
                            // Wanneer er één getal is ingevoerd komen er 3 streepjes achter
                            txtPIN.Text = Input + "---";
                            break;
                        case 2:
                            // Wanneer er al 2 getallen staan nog 2 streepjes
                            txtPIN.Text = Input + "--";
                            break;
                        case 3:
                            // Nog één streepje
                            txtPIN.Text = Input + "-";
                            break;
                        case 4:
                            // De volledige invoer
                            txtPIN.Text = Input;
                            break;
                    }
                }
            }

            // Wanneer de Length 4 is, en de invoer van de gebruiker niet gelijk is aan de PIn wordt alles gereset
            if (Length == 4 && Input != PIN)
            {
                // De string "Input" wordt geleegd
                Input = "";
                // Length wordt 0, omdat onderaan de "Length++" dus dan wordt deze weer 1, zoals het hoort
                Length = 0;
            }

            Length++;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Dit zorgt ervoor dat het NUMPAD gebruikt kan worden om de PIN in te vullen. De knop wordt gedetecteerd m.b.v. de KeyCode en de bijbehorende knop krijgt een "PerformClick()"
        private void Admin_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad0:
                    btn0.PerformClick();
                    break;
                case Keys.NumPad1:
                    btn1.PerformClick();
                    break;
                case Keys.NumPad2:
                    btn2.PerformClick();
                    break;
                case Keys.NumPad3:
                    btn3.PerformClick();
                    break;
                case Keys.NumPad4:
                    btn4.PerformClick();
                    break;
                case Keys.NumPad5:
                    btn5.PerformClick();
                    break;
                case Keys.NumPad6:
                    btn6.PerformClick();
                    break;
                case Keys.NumPad7:
                    btn7.PerformClick();
                    break;
                case Keys.NumPad8:
                    btn8.PerformClick();
                    break;
                case Keys.NumPad9:
                    btn9.PerformClick();
                    break;
            }
        }

        // Bij het klikken van de Reset worden alle standaardwaardes de CSV ingeladen en opgeslagen
        private void btnReset_Click(object sender, EventArgs e)
        {
            // Variabelen aanmaken om te kunnen exporteren naar de "session.csv"
            int[] iCoin = new int[5];
            int[] Drankje = new int[6];
            int[] Flesje = new int[6];
            int[] iCoinB = new int[5];

            iCoin[0] = 20;
            iCoin[1] = 20;
            iCoin[2] = 10;
            iCoin[3] = 10;
            iCoin[4] = 10;

            for (int i = 0; i < 6; i++)
            {
                Drankje[i] = 0;
                Flesje[i] = 10;
                if (i < 5)
                {
                    iCoinB[i] = 0;
                }
            }

            int iTotaal = 0;
            int iTotaalBedrag = 0;

            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //    VARIABELEN + DEFAULT VALUES AANMAKEN

            // Dit hieronder is identiek aan de "SaveCSV" op Form1
            StringBuilder sb = new StringBuilder("Flesjes;Drankjes;Munten;Bak" + "\r\n");

            for (int j = 0; j < 6; j++)
            {
                if (j < 5)
                {
                    sb.Append(Flesje[j] + ";" + Drankje[j] + ";" + iCoin[j] + ";" + iCoinB[j]);
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(Flesje[j] + ";" + Drankje[j] + ";" + iTotaal + ";" + iTotaalBedrag);
                    sb.AppendLine();
                }
            }
            File.WriteAllText(@"session.csv", sb.ToString());

            // Hier wordt de Boolean op Form1 veranderd naar "true" zodat, bij focus, de CSV ingeladen wordt
            frm1.ResetClicked = true;
            // Exit klikken
            btnExit.PerformClick();
        }
    }
}
