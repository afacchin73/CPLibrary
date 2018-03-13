-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Versione server:              10.2.12-MariaDB - mariadb.org binary distribution
-- S.O. server:                  Win64
-- HeidiSQL Versione:            9.4.0.5125
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


-- Dump della struttura del database cprovider
CREATE DATABASE IF NOT EXISTS `cprovider` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `cprovider`;

-- Dump della struttura di tabella cprovider.cpanagra
CREATE TABLE IF NOT EXISTS `cpanagra` (
  `cpa_id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT 'chiave primaria anagrafica',
  `cpa_cpgid` bigint(20) NOT NULL COMMENT 'id tabella Campagna',
  `cpa_rifter` char(6) NOT NULL DEFAULT '0' COMMENT 'Per noi: Codice Terzo (chiave primaria tab. terzi)',
  `cpa_rifpra` char(14) NOT NULL DEFAULT '0' COMMENT 'Per noi: Codice Pratica (chiave primaria tab. pratic (codazi-numpra-numiac)',
  `cpa_nome` varchar(70) NOT NULL DEFAULT '0' COMMENT 'Denominazione o Cognome e Nome',
  `cpa_numpty` int(1) NOT NULL COMMENT 'da 0 a 9',
  `cpa_calsts` int(1) NOT NULL COMMENT '1=Da chiamare; 2=Chiamata OK; 3=Chiamata KO; 9=Chiamata in Corso;....',
  `cpa_calcnt` int(3) NOT NULL COMMENT 'Conteggio chiamate effettuate per questa anagrafica (su qualsiasi n. di telefono)',
  `cpa_numpho` int(2) NOT NULL COMMENT 'Numero di telefoni dell''anagrafica totale( Es 3 telefoni disponibili)',
  `cpa_curpho` int(2) NOT NULL COMMENT 'Numero del telefono utilizzato (Es 1, 2 o 3)',
  PRIMARY KEY (`cpa_id`),
  KEY `cpa_campagid` (`cpa_cpgid`),
  KEY `cpa_rifTerzo` (`cpa_rifter`),
  KEY `cpa_rifPratica` (`cpa_rifpra`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1 COMMENT='Tabella Anagrafiche delle Campagne Telefoniche del CP\r\nUn record per ogni anagrafica Cliente di Pratica di Recupero Crediti che abbia almeno 1 n. di telefono registrato\r\nAlla Tabella Anagra sono collegate le tabelle Telefoni e Chiamate ';

-- L’esportazione dei dati non era selezionata.
-- Dump della struttura di tabella cprovider.cpcalls
CREATE TABLE IF NOT EXISTS `cpcalls` (
  `cpl_id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT 'chiave primaria log chiamate',
  `cpl_cpaid` bigint(20) DEFAULT NULL COMMENT 'id tabella Anagrafica',
  `cpl_numpho` int(2) DEFAULT NULL COMMENT 'n progressivo di telefono della anagrafica: da 1 a n dove n = cpa_numpho',
  `cpl_datcall` char(8) DEFAULT NULL COMMENT 'Data chiamata',
  `cpl_oracall` char(6) DEFAULT NULL COMMENT 'Ora inizio chiamata',
  `cpl_calsts` int(1) DEFAULT NULL COMMENT '1=Da chiamare; 2=Chiamata OK; 3=Chiamata KO; 9=Chiamata in Corso;....',
  `cpl_censts` int(1) DEFAULT NULL COMMENT 'stato risposta centralino: da questo deriviamo il campo sopra, stato',
  `cpl_oraEnd` char(6) DEFAULT NULL COMMENT 'Ora fine chiamata (se c''è stata una conversazione)',
  `cpl_caldur` int(3) DEFAULT NULL COMMENT 'Durata della chiamata in secondi (se c''è stata una conversazione)',
  `cpl_agent` varchar(200) DEFAULT NULL COMMENT 'Nome operatore in caso di risposta ok e passaggio chiamata all''operatore',
  PRIMARY KEY (`cpl_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Tabella Chiamate effettuate dal centralino';

-- L’esportazione dei dati non era selezionata.
-- Dump della struttura di tabella cprovider.cpcamp
CREATE TABLE IF NOT EXISTS `cpcamp` (
  `cpg_id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT 'chiave',
  `cpg_campag` varchar(20) NOT NULL DEFAULT '0' COMMENT 'Nome campagna (corrisponde a quello di Phones)',
  `cpg_descri` varchar(2000) NOT NULL DEFAULT '0' COMMENT 'Descrizione della campagna',
  `cpg_type` char(1) NOT NULL DEFAULT '0' COMMENT 'IVR; Primo Contatto, ....',
  `cpg_maxcal` int(2) NOT NULL COMMENT 'n. massimo di chiamate consentite per anagrafica x n. di telefono funzionante',
  `cpg_calitv` int(4) NOT NULL COMMENT 'minuti di intervallo minimo tra 1 chiamata e la successiva',
  `cpg_status` int(1) NOT NULL COMMENT '0=Disabled; 1=Enabled; 2=Completed; 8=Stop Request Utente; 9=Stopped (in questo status il CP interrompe la sua attività a passa l''EOF al primo getContact che riceve)',
  PRIMARY KEY (`cpg_id`),
  UNIQUE KEY `cpg_campagna` (`cpg_campag`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1 COMMENT='Tabella Campagne telefoniche Contact Provider (CP)\r\nUn record per ogni campagna inserita su Phones\r\nFa da Owner alle altre tabelle (anagrafiche e telefoni) \r\n';

-- L’esportazione dei dati non era selezionata.
-- Dump della struttura di tabella cprovider.cpphones
CREATE TABLE IF NOT EXISTS `cpphones` (
  `cpp_cpaid` int(11) NOT NULL DEFAULT 0,
  `cpp_numpho` int(2) unsigned NOT NULL DEFAULT 1,
  `cpp_phonum` varchar(12) DEFAULT NULL COMMENT 'numero di telefono dell''anagrafica',
  `cpp_calsts` int(1) DEFAULT NULL COMMENT '1=Da chiamare; 2=Chiamata OK; 3=Chiamata KO; 9=Chiamata in Corso;....',
  `cpp_censts` int(1) DEFAULT NULL COMMENT 'stato risposta centralino: da questo deriviamo il campo sopra, stato',
  `cpp_calcnt` int(3) DEFAULT NULL COMMENT 'Conteggio chiamate effettuate per questo n. di telefono',
  PRIMARY KEY (`cpp_cpaid`,`cpp_numpho`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='Tabella Telefoni delle Anagrafiche delle Campagne del CP\r\nUn record per ogni numero telefonico della Anagrafica\r\nFa riferimento alla tabella Anagrafica';

-- L’esportazione dei dati non era selezionata.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
