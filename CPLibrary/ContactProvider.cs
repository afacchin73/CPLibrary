using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CPLibrary
{
    using Phones.Messaging;
    using NetDialerProviderInterface;



    public class ContactProvider : IProviderInterface
    {

        private const string IniPath = "CPConfig.ini";
        //Contact Status Constants

        private const string ToBeCalled = "0";
        private const string Busy = "1";
        private const string NoAnswer = "2";
        private const string BadNumber = "3";
        private const string Failed = "4";
        private const string Completed = "5";
        private const string OperatorNotAvailable = "6";
        private const string InProcess = "9";

        //Call Mode Constants

        private const string PowerCall = "P";
        private const string ForcedPowerCall = "F";

        //Property Name and Value Contants

        private const string OverrideToCompleteName = "overrideToComplete";
        private const string IsToBeOverriddenValue = "YES";

        private const string AgentAllocationName = "agentAllocation";
        private const string IsAgentMandatory = "YES";

        ////Call Data Name Constants

        private const string CodCampagna = "CAMPAGNA";
        private const string CodAnag = "CODANAG";
        private const string CodCli = "CODCLI";
        private const string Cognome = "COGNOME";
        private const string Nome = "NOME";
        private const string TelefPr1 = "TELEFPR1";
        private const string TelefNr1 = "TELEFNR1";
        private const string TelefPr2 = "TELEFPR2";
        private const string TelefNr2 = "TELEFNR2";
        private const string Stato = "STATO";
        private const string DataOraApp = "DATAORAAPP";
        private const string CodOper = "CODOPER";
        private const string OperMode = "OPERMODE";
        private const string Notes = "NOTES";


        private string IdCampagna;
        private string NomeCampagna;

        
        private IDatabaseHandler dbHandler;

     

        public ContactProvider()
        {
            IdCampagna = "";
            NomeCampagna = "";
           

            Logger.Instance().WriteTrace(String.Format("Provider Class Initialize"));
          
            INIReader.Instance().IniPath = AppDomain.CurrentDomain.BaseDirectory + IniPath;

        }
    
        // INIZIALIZZA LA DLL
        public bool Init(string IdCampagna, string NomeCampagna, TDictionary contactStateConstants)
        {
            try
            {
                Logger.Instance().WriteTrace(String.Format("Init: Begin - Service : {0}", NomeCampagna));

                this.IdCampagna = IdCampagna;
                this.NomeCampagna = NomeCampagna;
 

                //Creation of proper DatabaseHandler, depending on .ini Configuration

                dbHandler = new DatabaseMYSQLHandler();


                dbHandler.ConnectionString = "Server=localhost;Database=cprovider;Uid=root;Pwd=root";
                dbHandler.ContactTable = "cpanagra";
                dbHandler.DbName = "cprovider";
                dbHandler.NomeCampagna = this.NomeCampagna;
                dbHandler.IdCampagna = this.IdCampagna;


                //Opening db Connection
                if (dbHandler.OpenConnection())
                {
                    dbHandler.CountContacts();
                    dbHandler.DatabaseRefreshed = true;
                }
                else
                {
                    Logger.Instance().WriteTrace(String.Format("INIT: Errors occured. Closing ContactProvider Istance for Service : {0}", NomeCampagna));
                    return false;
                }


                Logger.Instance().WriteTrace(string.Format("INIT: End - Service {0}", this.NomeCampagna));
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance().WriteTrace(string.Format("INIT: Exception occured. Closing ContactProvider Istance for Service {0}. Exception text: {1}", this.NomeCampagna, e.Message));
                return false;
            }
        }


       
        //  METODI CONTACT PROVIDER
        
        public int Contacts()
        {
            return dbHandler.CountContacts();// call alla classe dbHandler che gestisce la query per conteggiare i contatti
        }
 
        public bool GetContact(PhonesCallData contactCallData, ArrayList phoneNumbers, out string callMode, out bool eof)
        {
            return dbHandler.GetContact(contactCallData, phoneNumbers, out callMode, out eof);
        }

        public void RefreshDB()
        {
             dbHandler.RefreshDB();
        }

        public bool SetContact(PhonesCallData contactCallData, string contactStatus, string callMode)
        {
            return dbHandler.SetContact(contactCallData, contactStatus, callMode);
        }
 

        #region IDisposable Members 


        // Devo terminare il dbhandler
        public void Dispose()
        {
            dbHandler.Terminate();
        }

        #endregion

    }
}
