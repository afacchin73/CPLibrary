using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using CPLibrary;
using Phones.Messaging;
using NetDialerProviderInterface;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace TestMariaDB
{
    class Program
    {

        public static String CampagnaId = "2";
        public static String NomeCampagna = "BIFIS1701C1";

        static void Main(string[] args)
        {
            ContactProvider cp = new ContactProvider();
            TDictionary dic = new TDictionary();// qui abbiamo gli stati
            // simulo il dialer.exe
            String callMode = String.Empty;// ritorna il tipo di chiamata predictive o power
            Boolean eof = false;// identifica se è finita la scansione della campagna allora restituisco true
            PhonesCallData CcData = new PhonesCallData();// ritornano i dati dell'anagrafica che sto chiamando che pubblicherò nel phonebar
            ArrayList phoneNums = new ArrayList();// lista di numeri da chiamare per noi sempre 1



            Campagna(cp, dic, ref callMode, ref eof, CcData, phoneNums);

            CampagnaId = "3";
            NomeCampagna = "XXXXXXXXXX";

            Campagna(cp, dic, ref callMode, ref eof, CcData, phoneNums);

            cp.RefreshDB();
        }

        private static void Campagna(ContactProvider cp, TDictionary dic, ref string callMode, ref bool eof, PhonesCallData CcData, ArrayList phoneNums)
        {
            Boolean res = cp.Init(CampagnaId, NomeCampagna, dic);
            if (res)
            {

                Int32 contatti = cp.Contacts();
                Console.WriteLine("CONTATTI: " + contatti);

            }


            int count = 0;
            eof = false;
            while (!eof)// finchè nn indico la fine
            {

                count++;
                res = cp.GetContact(CcData, phoneNums, out callMode, out eof);




                if (res)
                {
                    // chiamata KO
                    if (count == 1)
                        res = cp.SetContact(CcData, "2", "P");
                    else if (count == 2)
                        // chiamata OK
                        res = cp.SetContact(CcData, "3", "P");
                    else
                        res = cp.SetContact(CcData, "2", "P");
                }

                 

            }

            

        }








        private static void test_mariadb()
        {
            List<Anagrafica> ls_an = new List<Anagrafica> { };

            String cString = "Server=localhost;Database=cprovider;Uid=root;Pwd=root;";
            MySqlConnection mycn = new MySqlConnection();
            mycn.ConnectionString = cString;

            String query = "Select * from cpanagra";
            MySqlCommand mycm = new MySqlCommand(query, mycn);
            MySqlDataReader rd;
            try
            {

                mycn.Open();

                rd = mycm.ExecuteReader();

                while (rd.Read())
                {

                    ls_an.Add(new Anagrafica
                    {

                        id = rd.GetInt32(0),
                        campagna_id = rd.GetInt32(1),
                        riferimento_terzo = rd.GetString(2),
                        riferimento_pratica = rd.GetString(3),
                        denominazione = rd.GetString(4),
                        indirizzo = rd.GetString(5),
                        cap = rd.GetString(6),
                        citta = rd.GetString(7),
                        provincia = rd.GetString(8),
                        priorita = rd.GetInt32(9),
                        statoChiamata = rd.GetInt32(10),
                        numTelefoni = rd.GetInt32(11),
                        telefonoCorrente = rd.GetString(12)

                    });



                }

                mycn.Close();
            }
            catch (Exception ex)
            {


            }

            Console.ReadKey();
        }
    }

    class Anagrafica
    {
        public Int32 id;
        public Int32 campagna_id;
        public String riferimento_terzo;
        public String riferimento_pratica;
        public String denominazione;
        public String indirizzo;
        public String cap;
        public String citta;
        public String provincia;
        public Int32 priorita;
        public Int32 statoChiamata;
        public Int32 numTelefoni;
        public String telefonoCorrente;

    }
}
