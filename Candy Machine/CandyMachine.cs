using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Candy_Machine
{
    public partial class CandyMachine : Form
    {

        public CandyMachine()
        {
            InitializeComponent();
        }

        // iTotaal houdt ingeworpen geld bij en telt daar aankopen vanaf. Output = txtBedrag.Text
        Decimal iTotaal;
        // iTotaalBedrag houdt alleen de inworp bij, niet wat er vanaf getrokken wordt. Dit is nodig voor het opstellen van de bon
        Decimal iTotaalBedrag;

        // iCoin is de array voor de munten in de machine
        int[] iCoin = new int[5];
        /// iCoin[] -  Munt - Standaard
        /// 0       = €2,00 = 20
        /// 1       = €1,00 = 20
        /// 2       = €0,50 = 10
        /// 3       = €0,20 = 10
        /// 4       = €0,10 = 10

        // Drankje houdt bij hoeveel drankjes er gekozen zijn (bijvoorbeeld txtCassis)
        int[] Drankje = new int[6];
        /// Drankje[] - Drankje - Standaard
        /// 0         = Cassis   = 0
        /// 1         = Cola     = 0
        /// 2         = Fanta    = 0
        /// 3         = Rivella  = 0
        /// 4         = Spa      = 0
        /// 5         = Sprite   = 0

        // Flesje is de hoeveelheid flessen nog over in de machine
        int[] Flesje = new int[6];
        /// Flesje[] - Flesje - Standaard
        /// 0        = Cassis   = 10
        /// 1        = Cola     = 10
        /// 2        = Fanta    = 10
        /// 3        = Rivella  = 10
        /// 4        = Spa      = 10
        /// 5        = Sprite   = 10

        // iCoinB houdt de munten bij die niet meer in de buis pasten.
        int[] iCoinB = new int[5];
        /// iCoin[] -  Munt - Standaard
        /// 0       = €2,00 = 0
        /// 1       = €1,00 = 0
        /// 2       = €0,50 = 0
        /// 3       = €0,20 = 0
        /// 4       = €0,10 = 0

        // Deze boolean is nodig om het programma te kunnen resetten. Uitleg op Admin.cs en hier in functie Form1.Activated
        public bool ResetClicked = false;


        // Wanneer Form1 geladen wordt (dus bij de start van de applicatie), krijgt de gebruiker een MessageBox te zien
        // Bij keus van "Ja" op de vraag "Wilt u de vorige simulatie laden?" laadt het CSV-bestand in met gegevens van de vorige keer.
        // Bij "Nee" worden de standaardgegevens geladen
        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult load = MessageBox.Show("Wilt u de vorige simulatie laden?", "Doorgaan", MessageBoxButtons.YesNo);
            // Wanneer er "Ja" geantwoord wordt, laden de meest recente gegevens in.
            if (load == DialogResult.Yes)
            {
                LoadCSV();
            }
            // Zo niet, dan krijgen alle globale variabelen hun standaardwaardes mee
            else
            {
                iCoin[0] = 20;
                iCoin[1] = 20;
                iCoin[2] = 10;
                iCoin[3] = 10;
                iCoin[4] = 10;

                for (int i = 0; i < 6; i++)
                {
                    Drankje[i] = 0;
                    Flesje[i] = 10;
                    // Omdat de bovenste For-loop 6 keer rondgaat en iCoinB maar 5 plekken heeft, wordt deze alleen gevuld wanneer "i < 5". iCoinB[5] bestaat niet
                    if (i < 5)
                    {
                        iCoinB[i] = 0;
                    }
                }

                iTotaal = 0;
                iTotaalBedrag = 0;
            }
            Initiate();
        }

        // De Initiate-functie verplaatst variabelen naar de bijbehorende tekstboxen
        private void Initiate()
        {
            txtCassis.Text = Drankje[0].ToString();
            txtCola.Text = Drankje[1].ToString();
            txtFanta.Text = Drankje[2].ToString();
            txtRivella.Text = Drankje[3].ToString();
            txtSpa.Text = Drankje[4].ToString();
            txtSprite.Text = Drankje[5].ToString();

            txtFLESCassis.Text = Flesje[0].ToString();
            txtFLESCola.Text = Flesje[1].ToString();
            txtFLESFanta.Text = Flesje[2].ToString();
            txtFLESRivella.Text = Flesje[3].ToString();
            txtFLESSpa.Text = Flesje[4].ToString();
            txtFLESSprite.Text = Flesje[5].ToString();

            txtCOIN2e.Text = iCoin[0].ToString();
            txtCOIN1e.Text = iCoin[1].ToString();
            txtCOIN50c.Text = iCoin[2].ToString();
            txtCOIN20c.Text = iCoin[3].ToString();
            txtCOIN10c.Text = iCoin[4].ToString();

            txtErr.Text = "";
            // Omdat "Initiate" als eerste station wordt gebruikt bij het inladen, moeten de knoppen natuurlijk ook de juiste waardes weergeven
            ButtonCheck();
            // Hetzelfde geldt voor de progressbars net boven de munten op Form1
            pBar();
        }

        private void ButtonCheck()
        {
            // Om gebruik te kunnen maken van een korte loop i.p.v. 6 verschillende if-statements of een boom van switches, wordt er een array gebruikt die de controls vertegenwoordigen
            Button[] drinks = new Button[6];
            // Dezelfde volgorde wordt steeds aangehouden. Voorbeeld: 0 is altijd Cassis, 1 is altijd Cola.
            drinks[0] = btnCassis;
            drinks[1] = btnCola;
            drinks[2] = btnFanta;
            drinks[3] = btnRivella;
            drinks[4] = btnSpa;
            drinks[5] = btnSprite;

            TextBox[] inMachine = new TextBox[6];
            // Ook hier
            inMachine[0] = txtFLESCassis;
            inMachine[1] = txtFLESCola;
            inMachine[2] = txtFLESFanta;
            inMachine[3] = txtFLESRivella;
            inMachine[4] = txtFLESSpa;
            inMachine[5] = txtFLESSprite;

            // Wanner de "GEEN GELD"-error weergeven wordt in de tekstbox onder txtBedrag, worden alle buttons disabled
            // Ook wanneer iTotaal kleiner is dan €0.50, dan kan er immers niks gekocht worden wegens te weinig invoer
            // De bovenste twee zijn een OR-statement. Eén van die twee moet waar zijn IN COMBINATIE MET "ResetClicked == false"
            // Deze laatste is er later bijgekomen zodat het resetten van de machine mogelijk was zonder een aparte functie 
            //   aan te hoeven maken voor het corrigeren van kleuren, etc.
            if ((txtErr.Text == "GEEN GELD" || iTotaal < 0.5m) && ResetClicked == false)
            {
                for (int i = 0; i < 6; i++)
                {
                    // De knop uitschakelen
                    drinks[i].Enabled = false;
                    // Een rood accent voor visuele bevestiging (een uitgeschakelde knop is moeilijk te onderscheiden)
                    drinks[i].ForeColor = Color.Red;
                }
            }
            // Wanneer er geen bijzonderheden zijn start de "else". Dit is de normale flow
            // Per flesje wordt gekeken of hij gekocht kan worden of niet, zo nee dan wordt de knop uitgeschakeld.
            else
            {
                // Een array met de prijzen van de verschillende drankjes. Dit om een simpele loop te kunnen maken ipv een switch-boom.
                Decimal[] price = new Decimal[6];
                price[0] = 1;     // Cassis
                price[1] = 1.20m; // Cola
                price[2] = 1.10m; // Fanta
                price[3] = 0.90m; // Rivella
                price[4] = 0.50m; // Spa
                price[5] = 1;     // Sprite

                for (int i = 0; i < 6; i++)
                {
                    // Wanneer er meer dan 0 flesjes in de machine zitten van dit type
                    if (Flesje[i] > 0)
                    {
                        inMachine[i].BackColor = SystemColors.Menu;
                        // Wanneer er niet genoeg geld is voor het drankje
                        if (iTotaal < price[i])
                        {
                            drinks[i].Enabled = false;
                            drinks[i].ForeColor = Color.Red;
                        }
                        // Wanneer er wèl genoeg geld is voor het drankje
                        else
                        {
                            drinks[i].Enabled = true;
                            drinks[i].ForeColor = SystemColors.ControlText;
                        }
                    }
                    // Zo niet, blijft de knop uitgeschakeld en de kleur rood. Ook wordt het tekstboxje wat de flessen weergeeft rood.
                    else
                    {
                        drinks[i].Enabled = false;
                        drinks[i].ForeColor = Color.Red;
                        inMachine[i].BackColor = Color.Firebrick;
                    }

                }
            }
        }

        // Deze functie vult de progressbars boven de munten op Form1 naar de behorende waarde.
        // Alle bars worden per keer ververst. Dit is niet de meest efficiënte manier, maar wanneer
        //   er niet te veel kliks per seconde zijn, zorgt dit niet voor problemen.
        private void pBar()
        {
            // Om een loop te kunnen gebruiken, worden de progressbars in een Array gezet voor easy-access
            ProgressBar[] bar = new ProgressBar[5];
            bar[0] = pg2e;
            bar[1] = pg1e;
            bar[2] = pg50c;
            bar[3] = pg20c;
            bar[4] = pg10c;

            // Uitleg over verhogen Maximum:
            //
            // Eerst wijzigde ik per keer de waarde door "bar[i].Value = iCoin[i]". Dit zorgde voor een trage animatie
            // Een animatie doet zich voor om de balk 'vol te laten lopen'. Hierdoor lijkt er een ernstige vertraging 
            //   ergens te zitten. Echter, als de "bar[i].Value" 1 hoger wordt ingesteld en dan increment naar beneden,
            //   wordt de animatie overgeslagen. 
            // Om te voorkomen dat het programma crasht wanneer de max bereikt wordt (voorbeeld: "bar[0].Value = iCoin[0] +1"
            //   kan niet omdat '41' niet bestaat), verhoogt de "bar[i].Maximum" voordat de met één verhoogde iCoin er in
            //   gezet wordt. Dan komt de omlaag-increment om de animatie te blokkeren en daarna gaat de "bar[i].Maximum"
            //   weer met één omlaag. Zo wordt crashen voorkomen omdat "Out of Range Exception" niet mogelijk is.
            // Eerder was dit met If-statements gedaan maar de huidige aanpak leek het snelst.
            //
            // Einde Uitleg

            for (int i = 0; i < 5; i++)
            {
                bar[i].Maximum++;
                bar[i].Value = iCoin[i] + 1;
                bar[i].Value--;
                bar[i].Maximum--;
            }
        }

        // EmptySpaces wist de 'gebruikerssporen' van de vorige simulatie maar laat de flessen en munten staan.
        private void EmptySpaces()
        {
            // Welkom heten aan de nieuwe klant
            txtBedrag.Text = "WELKOM";

            // De door de oude gebruiker gekozen drankjes van het scherm halen
            for (int i = 0; i < 6; i++)
            {
                Drankje[i] = 0;
            }

            // Door iTotaal naar 0 te zetten, zorgt de functie "Wisselgeld" er voor dat de tekstboxen voor Wisselgeld leeg komen.
            // Deze hoeven dus niet apart leeggemaakt te worden, een verwijzing is genoeg (zie "btnEind").
            iTotaal = 0; iTotaalBedrag = 0;
        }

        // Deze functie haalt het verschuldigde wisselgeld van de aanwezige munten af wanneer een gebruiker op "btnEind" klikt. 
        // Wanneer een munt op is, wordt het resterende bedrag betaald met een andere munt. Dit gebeurt net zo lang tot het wisselgeld
        //   betaald is. Zitten er geen munten meer in de machine, laat het textvlak "txtErr" (onder txtBedrag) zien hoeveel er niet
        //   betaald kon worden, dit geld is weg. De kans dat de gebruiker dit ziet is echter heel klein, of men moet bijvoorbeeld  
        //   héél veel €2.00 munten in de machine gegooid hebben die niet meer in de buis paste en dus in de opvangbak beland zijn.

        private void Deduction()
        {
            // Easier access, tekstboxen in een array om gebruik te kunnen maken van een loop
            string[] change = new string[5];
            change[0] = txt2e.Text;
            change[1] = txt1e.Text;
            change[2] = txt50c.Text;
            change[3] = txt20c.Text;
            change[4] = txt10c.Text;

            // Een int waar de geparste string in komt te staan
            int Spare;

            // Een for-loop zodat er per munt gekeken kan worden of aftrekken mogelijk is. Zo niet, verrekenen met andere munten
            for (int i = 0; i < 5; i++)
            {
                // Wanneer de tekstbox niet leeg is, wordt de string eruit gehaald. De string is het aantal munten met een 'x' erachter.
                // De 'x' wordt verwijderd met behulp van "change[i].Remove(change[i].Length - 1)", wat overblijft wordt geparst in int "Spare"
                if (change[i] != "")
                {
                    // Het weghalen van de 'x'
                    change[i] = change[i].Remove(change[i].Length - 1);
                    // Getal parsen tot een int
                    Spare = int.Parse(change[i]);

                    // Wanneer het aantal munten in de machine groter of gelijk is aan het aantal wisselgeld van die munt, wordt het simpelweg afgetrokken
                    if (iCoin[i] >= Spare)
                    {
                        iCoin[i] -= Spare;
                    }
                    // Zo niet moet er betaald worden met andere munten.
                    else
                    {
                        // Decimal "Money" wordt aangemaakt om uit te kunnen rekenen wat de gebruiker verschuldigd is.
                        Decimal Money = 0;

                        // Ervoor zorgen dat de beschikbare munten gebruikt kunnen worden
                        // Voorbeeld: 2 €2 in machine, 3 € wisselgeld: Er blijft 1 € 2 over na deze while-loop
                        while (iCoin[i] != 0)
                        {
                            iCoin[i]--;
                            Spare--;
                        }

                        // Array met de waardes van elke munt
                        Decimal[] worth = new Decimal[5];
                        worth[0] = 2;
                        worth[1] = 1;
                        worth[2] = 0.5m;
                        worth[3] = 0.2m;
                        worth[4] = 0.1m;

                        for (int j = 0; j < 5; j++)
                        {
                            // Wanneer bijvoorbeeld "i == 0", dus de €2.00 op is, wordt de 'nog te betalen waarde' bepaald door te vermenigvuldigen.
                            Money = worth[i] * Spare;

                            // Wanneer de het aantal munten in de machine NIET 0 is && (Verschuldigd geld - cost) groter of gelijk aan 0, doet de while-loop zijn ding.
                            while ((iCoin[j] != 0) && ((Money - worth[j]) >= 0))
                            {
                                // Er wordt één coin afgeteld
                                iCoin[j]--;
                                // De waarde van de iCoin wordt van de schuld "Money" afgehaald.
                                Money -= worth[j];
                            }
                        }

                        // Wanneer het niet is gelukt om "Money" helemaal te vergoeden, komt er onder het totaalbedrag te staan hoeveel er verloren is.
                        if (Money != 0)
                        {
                            txtErr.Text = "€ " + Money.ToString("0.00") + " mislukt";
                        }
                        // Is dit wel gelukt, wordt de errorbox weer gewoon geleegd.
                        else
                        {
                            txtErr.Text = "";
                        }
                    }

                }
            }
        }

        private void WisselGeld()
        {
            // iTotaal wordt in een lokale variabele geplaatst zodat ermee gerekend kan worden
            Decimal iWissel = iTotaal;

            // In "Amounts" wordt de hoeveelheid wisselmunten tijdelijk opgeslagen. 
            Decimal[] amounts = new Decimal[5];
            amounts[0] = 0; // Voor €2.00
            amounts[1] = 0; // Voor €1.00
            amounts[2] = 0; // Voor €0.50
            amounts[3] = 0; // Voor €0.20
            amounts[4] = 0; // Voor €0.10

            // Weer gebruiken wij de waarde van de munt, verwerkt in een Array
            Decimal[] worth = new Decimal[5];
            worth[0] = 2;
            worth[1] = 1;
            worth[2] = 0.5m;
            worth[3] = 0.2m;
            worth[4] = 0.1m;

            // 5 munten, 5 rondjes in de loop (in volgorde van groot naar klein)
            for (int i = 0; i < 5; i++)
            {
                // Zolang van iWissel de waarde van de munt af kan zonder dat deze onder de 0 uit komt, "amounts[i]++" en "iWissel - worth[i]";
                while ((iWissel - worth[i]) >= 0)
                {
                    amounts[i]++;
                    iWissel -= worth[i];
                }
            }

            // Opvang is de array van Labels, gelijk aan het aantal munten dat niet in de buis paste.
            Label[] Opvang = new Label[5];
            Opvang[0] = lbleuro2o;
            Opvang[1] = lbleuro1o;
            Opvang[2] = lblcent50o;
            Opvang[3] = lblcent20o;
            Opvang[4] = lblcent10o;

            // De tekstboxen waar aantal munten voor wisselgeld komen te staan + "x";
            TextBox[] TextCoin = new TextBox[5];
            TextCoin[0] = txt2e;
            TextCoin[1] = txt1e;
            TextCoin[2] = txt50c;
            TextCoin[3] = txt20c;
            TextCoin[4] = txt10c;

            // Munten nog in de machine
            TextBox[] CoinsLeft = new TextBox[5];
            CoinsLeft[0] = txtCOIN2e;
            CoinsLeft[1] = txtCOIN1e;
            CoinsLeft[2] = txtCOIN50c;
            CoinsLeft[3] = txtCOIN20c;
            CoinsLeft[4] = txtCOIN10c;

            for (int i = 0; i < 5; i++)
            {
                // Dit if-statement zorgt ervoor dat munten die niet meer in de buis passen naar de Opvangbak gaan.
                if (i < 2)
                {
                    // Max van €1.00 en €2.00 = 40 munten, dus aftrekken wat er boven zit
                    while (iCoin[i] > 40)
                    {
                        iCoinB[i]++;
                        iCoin[i]--;
                    }
                }
                else
                {
                    // Max van €0.50, €0.20 en €0.10 = 20 munten, dus aftrekken wat er boven zit
                    while (iCoin[i] > 20)
                    {
                        iCoinB[i]++;
                        iCoin[i]--;
                    }
                }

                // Opvangbak refreshen
                Opvang[i].Text = iCoinB[i].ToString();

                // Aantal munten nog in de machine refreshen
                CoinsLeft[i].Text = iCoin[i].ToString();

                // Het aantal wisselmunten laten zien per munt
                if (amounts[i] > 0)
                {
                    // Wanneer er wisselgeld beschikbaar is voor de munt (wanneer "amounts[i] > 0") wordt het getal geschreven + een "x"
                    TextCoin[i].Text = amounts[i].ToString() + "x";
                }
                else
                {
                    // Zo niet wordt de tekstbox leeggelaten
                    TextCoin[i].Text = "";
                }
            }

            // Het txtWissel onderaan laat zien hoeveel er totaal nog in de machine zit. Duplicaat van txtBedrag maar vond ik wel fijn zodat totale waarde goed zichbaar was.
            txtWissel.Text = "€ " + iTotaal.ToString("0.00");
        }

        // ButtonClick
        // 
        // De 6 buttons met flessen er op hebben allemaal de Click-functie gekregen "ButtonClick". Met behulp van een switch-case wordt de geselecteerde button bepaald en 
        //   de bijbehorende actie uitgevoerd.
        // Als er niet genoeg geld in de machine zit om de bestelling te doen, was de knop al uitgezet door "ButtonCheck();". De if-statement die eerder verwerkt was in
        // dit blok is om deze reden verwijderd.

        private void ButtonClick(object sender, EventArgs e)
        {
            // De juiste knop wordt bepaald
            Button btn = (Button)sender;
            // De tekst op de knop uitgelezen en opgeslagen in string "name"
            string name = btn.Text;

            // Een switch kijkt welke string er overeen komt met "name"
            switch (name)
            {
                case "Cassis":
                    // Totaal - de kosten van het blikje
                    iTotaal -= 1;
                    // Aantal drankjes Cassis dat de gebruiker geselecteerd heeft, gaat omhoog
                    Drankje[0]++;
                    // Aantal Flesjes in de machine gaat naar beneden
                    Flesje[0]--;
                    // De tekstvelden op het Form krijgen de nieuwe waarde mee
                    txtCassis.Text = Drankje[0].ToString();
                    txtFLESCassis.Text = Flesje[0].ToString();
                    break;
                case "Cola":
                    iTotaal -= 1.2m;
                    Drankje[1]++;
                    Flesje[1]--;
                    txtCola.Text = Drankje[1].ToString();
                    txtFLESCola.Text = Flesje[1].ToString();
                    break;
                case "Fanta": // Zie "case Cassis"
                    iTotaal -= 1.1m;
                    Drankje[2]++;
                    Flesje[2]--;
                    txtFanta.Text = Drankje[2].ToString();
                    txtFLESFanta.Text = Flesje[2].ToString();
                    break;
                case "Rivella": // Zie "case Cassis"
                    iTotaal -= 0.9m;
                    Drankje[3]++;
                    Flesje[3]--;
                    txtRivella.Text = Drankje[3].ToString();
                    txtFLESRivella.Text = Flesje[3].ToString();
                    break;
                case "Spa": // Zie "case Cassis"
                    iTotaal -= 0.5m;
                    Drankje[4]++;
                    Flesje[4]--;
                    txtSpa.Text = Drankje[4].ToString();
                    txtFLESSpa.Text = Flesje[4].ToString();
                    break;
                case "Sprite": // Zie "case Cassis"
                    iTotaal -= 1;
                    Drankje[5]++;
                    Flesje[5]--;
                    txtSprite.Text = Drankje[5].ToString();
                    txtFLESSprite.Text = Flesje[5].ToString();
                    break;
            }
            // txtBedrag krijgt de nieuwe waarde
            txtBedrag.Text = "€ " + iTotaal.ToString("0.00");

            //Wisselgeld wordt berekend met functie "WisselGeld();"
            WisselGeld();

            // De knoppen worden gecontroleerd
            ButtonCheck();

        }

        private void PiClick(object sender, EventArgs e)
        {
            // De juiste knop wordt bepaald
            Button muntje = (Button)sender;
            // De naam van de knop uitgelezen en opgeslagen in string "name"
            string name = muntje.Name;

            // Exact hetzelfde principe als bij "ButtonClick();"
            switch (name)
            {
                case "btn2e":
                    iCoin[0]++;
                    iTotaal += 2;
                    iTotaalBedrag += 2;
                    break;
                case "btn1e":
                    iCoin[1]++;
                    iTotaal += 1;
                    iTotaalBedrag += 1;
                    break;
                case "btn50c":
                    iCoin[2]++;
                    iTotaal += 0.5m;
                    iTotaalBedrag += 0.5m;
                    break;
                case "btn20c":
                    iCoin[3]++;
                    iTotaal += 0.2m;
                    iTotaalBedrag += 0.2m;
                    break;
                case "btn10c":
                    iCoin[4]++;
                    iTotaal += 0.1m;
                    iTotaalBedrag += 0.1m;
                    break;
            }

            // txtBedrag krijgt het nieuwe ingeworpen bedrag
            txtBedrag.Text = "€ " + iTotaal.ToString("0.00");
            // Nieuw wisselgeld wordt bepaald
            WisselGeld();
            // De Progressbars worden vernieuwd
            pBar();
            // Alle errors worden gewist
            txtErr.Text = "";
            // De knoppen worden gecontroleerd
            ButtonCheck();
        }

        // Wanneer de gebruiker zijn bestelling heeft gedaan, kan hij of zij op 'Eind' klikken. Na het geven van een "bonnetje" in de vorm van een MessageBox,
        //   verwijdert de machine de gebruikerssporen. De nieuwe gebruiker kan aan de slag.
        private void btnEind_Click(object sender, EventArgs e)
        {
            // Er wordt gekeken of er iets besteld is. Zo niet, wordt het geld (verrekend) teruggegeven
            if (txtSpa.Text == "0" && txtCola.Text == "0" && txtFanta.Text == "0" && txtSprite.Text == "0" && txtRivella.Text == "0" && txtCassis.Text == "0")
            {
                MessageBox.Show("Betaling afgebroken" + Environment.NewLine + Environment.NewLine + "Wisselgeld: € " + iTotaalBedrag.ToString("0.00"));
                EmptySpaces();
                Deduction();
                Initiate();
            }
            // Als er wel één of meerdere flesjes aangeklikt zijn, laat de machine het bonnetje zien met daaronder het totale ingeworpen bedrag en haalt de gebruiksvelden vervolgens leeg
            else
            {
                // Om een Messagebox op te stellen, wordt er een string gemaakt die gevuld wordt met de benodigde informatie
                string message = "";

                // Een string met de teksten die achter de drankjes moeten komen, mochten ze besteld zijn. Dit is tòch een array geworden omdat het zo makkelijker te bewerken is later
                string[] drink = new string[6];
                drink[0] = " x    Cassis";
                drink[1] = " x    Cola";
                drink[2] = " x    Fanta";
                drink[3] = " x    Rivella";
                drink[4] = " x    Spa";
                drink[5] = " x    Sprite";

                // De standaard "Uw bestelling" bovenaan de bon
                message = "Uw bestelling: " + Environment.NewLine + Environment.NewLine;

                // Met deze loop worden alle drankjes nagegaan
                for (int i = 0; i < 6; i++)
                {
                    // Wanneer "Drankje[i]" een hogere waarde heeft dan 0, dus wanneer er 1 of meer besteld is, voeg toe aan de string (dus de bon)
                    if (Drankje[i] > 0)
                    {
                        message += "      " + Drankje[i].ToString() + drink[i] + Environment.NewLine;
                    }
                }

                // De bottom text + totaalbedrag & wisselgeld toevoegen
                message += Environment.NewLine + "Totaalbedrag: € " + (iTotaalBedrag - Decimal.Parse(txtWissel.Text.Remove(0, 2))) + Environment.NewLine + "Wisselgeld: " + txtWissel.Text + Environment.NewLine + Environment.NewLine + "Bedankt en tot ziens!";

                // De MessageBox met de bestelling
                MessageBox.Show(message, "Bestelling");

                EmptySpaces();
                Deduction();
                Initiate();
            }

            WisselGeld();
        }

        // LoadCSV laadt het CSV-bestand in. Er is geen OpenFileDialog omdat het bestand standaard de naam "session.csv" krijgt om het ongemerkt te laten gebeuren
        public void LoadCSV()
        {
            // Een lijst om de rijen in te laden wordt aangemaakt
            List<string> rowLoader = new List<string>();
            // Een array om de 4 geïmporteerde getallen in te zetten
            string[] Loader = new string[4];

            // Het bestand openen en de informatie inladen
            using (StreamReader sReader = new StreamReader(@"session.csv"))
            {
                while (!sReader.EndOfStream)
                {
                    // Alle rijen worden ingeladen
                    rowLoader.Add(sReader.ReadLine());
                }
            }

            // Een string wordt aangemaakt om de geïmporteerde rij te kunnen splitsen
            string import = "";

            for (int i = 1; i < 7; i++)
            {
                // De string krijgt de inhoud van "rowLoader[i]" toegewezen
                import = rowLoader[i];

                // De string wordt gesplitst in kleine stukjes met daarin de variabelen
                Loader = import.Split(';');

                // Deze loop zet de stukjes op de juiste plek terug
                for (int j = 0; j < 4; j++)
                {
                    // Wanneer "i < 6" wordt alles als normaal geladen
                    if (i < 6)
                    {
                        switch (j)
                        {
                            case 0:
                                Flesje[i - 1] = int.Parse(Loader[j]);
                                break;
                            case 1:
                                Drankje[i - 1] = int.Parse(Loader[j]);
                                break;
                            case 2:
                                iCoin[i - 1] = int.Parse(Loader[j]);
                                break;
                            case 3:
                                iCoinB[i - 1] = int.Parse(Loader[j]);
                                break;
                        }
                    }
                    // Wanneer "i == 6" zijn de laatste 2 getallen niet munten (daar zijn er maar 5 van namelijk, niet 6 zoals bij de flessen) maar de totaalbedragen.
                    else
                    {
                        switch (j)
                        {
                            case 0:
                                Flesje[i - 1] = int.Parse(Loader[j]);
                                break;
                            case 1:
                                Drankje[i - 1] = int.Parse(Loader[j]);
                                break;
                            case 2:
                                iTotaal = Decimal.Parse(Loader[j]);
                                break;
                            case 3:
                                iTotaalBedrag = Decimal.Parse(Loader[j]);
                                break;
                        }
                    }

                }
            }
            // De standaardverversingen
            WisselGeld();
            Initiate();
            ButtonCheck();
            txtBedrag.Text = "WELKOM";
        }

        private void SaveCSV()
        {
            // Een StringBuilder wordt voorbereid met als kop "Flesjes, Drankjes, Munten, Bak"
            StringBuilder sb = new StringBuilder("Flesjes;Drankjes;Munten;Bak" + "\r\n");

            // Er zijn 6 entries in de flessen, 5 munten + 1x een totaal. Samen ook 6
            for (int j = 0; j < 6; j++)
            {
                // Wanneer "j < 5" worden de de getallen behandeld als flessen en munten
                if (j < 5)
                {
                    // Een ";" wordt toegevoegd als scheidingsteken tussen de kolommen in
                    sb.Append(Flesje[j] + ";" + Drankje[j] + ";" + iCoin[j] + ";" + iCoinB[j]);
                    sb.AppendLine();
                }
                // Wanneer "j == 5" worden de laatste 2 getallen ingeladen als "iTotaal" en "iTotaalBedrag"
                else
                {
                    // Een ";" wordt toegevoegd als scheidingsteken tussen de kolommen in
                    sb.Append(Flesje[j] + ";" + Drankje[j] + ";" + iTotaal + ";" + iTotaalBedrag);
                    sb.AppendLine();
                }
            }
            // Het bestand wordt weggeschreve naar "session.csv" in dezelfde map als de .exe
            File.WriteAllText(@"session.csv", sb.ToString());
        }

        // De knop "Admin" opent het Administratorscherm dat beveiligd is met een pincode en de gebruiker toegang biedt tot de Reset-knop, mits de PIN-code bekend is
        private void btnAdmin_Click(object sender, EventArgs e)
        {
            Admin frm2 = new Admin();
            frm2.Show(this);
        }

        // De knop "Opvangbak" laat alleen zien wat de gecombineerde waarde is van de munten die niet meer in de buizen paste en dus opgevangen zijn in de bak.
        private void btnOpvangbak_Click(object sender, EventArgs e)
        {
            // Een variabele genaamd "total" wordt aangemaakt om de kosten in op te tellen
            Decimal total = 0;

            // Weer een array met de waarde van de munten
            Decimal[] worth = new Decimal[5];
            worth[0] = 2;
            worth[1] = 1;
            worth[2] = 0.5m;
            worth[3] = 0.2m;
            worth[4] = 0.1m;

            for (int i = 0; i < 5; i++)
            {
                // Het aantal munten wordt vermenigvuldigd met de waarde en opgeteld bij "total"
                total += (iCoinB[i] * worth[i]);
            }

            // Een MessageBox wordt weergeven met de verkregen informatie
            MessageBox.Show("Totale waarde opvangbak: € " + total.ToString("0.00"));
        }

        // Dit FormClosing-event biedt de gebruiker de optie om zijn sessie op te slaan. De MessageBox verschijnt bij het afsluiten van Form1, op welke manier dan ook
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Een DialogResult zorgt ervoor dat we iets kunnen met de "Ja"
            DialogResult choice = MessageBox.Show("Wilt u pslaan vóór he tafsluiten?", "Simulatie Opslaan", MessageBoxButtons.YesNo);

            // Wanneer "Ja", zal de informatie weggeschreven worden naar "session.csv"
            if (choice == DialogResult.Yes)
            {
                SaveCSV();
            }
            // Bij een "Nee" gebeurt er niks en sluit het programma gewoon af
        }

        // De Form1_Activated zorgt ervoor dat, wanneer het form weer naar de voorgrond gebracht wordt, er gekeken kan worden of dat is omdat er op de Reset-knop is geklikt op het Admin-form
        // Zo ja, is de Boolean "ResetClicked" true, zo nee is deze false. Bij "true" wordt het csv-bestand ingeladen en alle tekstboxen ververst. Direct daarna wordt de Boolean op "false" gezet.
        private void Form1_Activated(object sender, EventArgs e)
        {
            // Pas wanneer er op Reset (op form Admin) is geklikt, worden de gegevens veranderd.
            if (ResetClicked)
            {
                LoadCSV();

                WisselGeld();
                Initiate();

                // De boolean gaat weer terug naar False om onbedoeld inladen te voorkomen
                ResetClicked = false;
            }

        }
    }
}
