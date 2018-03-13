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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1 COMMENT='Tabella Anagrafiche delle Campagne Telefoniche del CP\r\nUn record per ogni anagrafica Cliente di Pratica di Recupero Crediti che abbia almeno 1 n. di telefono registrato\r\nAlla Tabella Anagra sono collegate le tabelle Telefoni e Chiamate ';

-- Dump dei dati della tabella cprovider.cpanagra: ~3 rows (circa)
/*!40000 ALTER TABLE `cpanagra` DISABLE KEYS */;
INSERT INTO `cpanagra` (`cpa_id`, `cpa_cpgid`, `cpa_rifter`, `cpa_rifpra`, `cpa_nome`, `cpa_numpty`, `cpa_calsts`, `cpa_calcnt`, `cpa_numpho`, `cpa_curpho`) VALUES
	(1, 2, '970413', '88988744561235', 'Roberta Brusa', 1, 1, 0, 2, 0),
	(2, 2, '970412', '87889977661701', 'Andrea Facchin', 0, 1, 0, 2, 0),
	(5, 3, '950200', '86888433454896', 'Marco Moret', 0, 1, 0, 1, 0);
/*!40000 ALTER TABLE `cpanagra` ENABLE KEYS */;

-- Dump della struttura di tabella cprovider.cpcalls
CREATE TABLE IF NOT EXISTS `cpcalls` (
  `cpl_id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT 'chiave primaria log chiamate',
  `cpl_cpaid` bigint(20) DEFAULT NULL COMMENT 'id tabella Anagrafica',
  `cpl_numpho` int(2) DEFAULT NULL COMMENT 'n progressivo di telefono della anagrafica: da 1 a n dove n = cpa_numpho',
  `cpl_seqcall` char(8) DEFAULT NULL COMMENT 'Data chiamata',
  `cpl_oracall` char(6) DEFAULT NULL COMMENT 'Ora inizio chiamata',
  `cpl_calsts` int(1) DEFAULT NULL COMMENT '1=Da chiamare; 2=Chiamata OK; 3=Chiamata KO; 9=Chiamata in Corso;....',
  `cpl_censts` int(1) DEFAULT NULL COMMENT 'stato risposta centralino: da questo deriviamo il campo sopra, stato',
  `cpl_oraEnd` char(6) DEFAULT NULL COMMENT 'Ora fine chiamata (se c''è stata una conversazione)',
  `cpl_caldur` int(3) DEFAULT NULL COMMENT 'Durata della chiamata in secondi (se c''è stata una conversazione)',
  `cpl_agent` varchar(200) DEFAULT NULL COMMENT 'Nome operatore in caso di risposta ok e passaggio chiamata all''operatore',
  PRIMARY KEY (`cpl_id`)
) ENGINE=InnoDB AUTO_INCREMENT=183 DEFAULT CHARSET=latin1 COMMENT='Tabella Chiamate effettuate dal centralino';

-- Dump dei dati della tabella cprovider.cpcalls: ~4 rows (circa)
/*!40000 ALTER TABLE `cpcalls` DISABLE KEYS */;
INSERT INTO `cpcalls` (`cpl_id`, `cpl_cpaid`, `cpl_numpho`, `cpl_seqcall`, `cpl_oracall`, `cpl_calsts`, `cpl_censts`, `cpl_oraEnd`, `cpl_caldur`, `cpl_agent`) VALUES
	(179, 2, 1, '20180312', '181519', 2, 2, '181519', 0, ''),
	(180, 1, 1, '20180312', '181519', 3, 3, '181520', 1, ''),
	(181, 1, 2, '20180312', '181520', 2, 2, '181520', 0, ''),
	(182, 5, 1, '20180312', '181520', 2, 2, '181520', 0, '');
/*!40000 ALTER TABLE `cpcalls` ENABLE KEYS */;

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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1 COMMENT='Tabella Campagne telefoniche Contact Provider (CP)\r\nUn record per ogni campagna inserita su Phones\r\nFa da Owner alle altre tabelle (anagrafiche e telefoni) \r\n';

-- Dump dei dati della tabella cprovider.cpcamp: ~2 rows (circa)
/*!40000 ALTER TABLE `cpcamp` DISABLE KEYS */;
INSERT INTO `cpcamp` (`cpg_id`, `cpg_campag`, `cpg_descri`, `cpg_type`, `cpg_maxcal`, `cpg_calitv`, `cpg_status`) VALUES
	(2, 'BIFIS1701C1', 'Primo contatto Banca Ifis affido 1701', 'I', 4, 3, 0),
	(3, 'XXXXXXXXXX', 'test', 'I', 2, 5, 0);
/*!40000 ALTER TABLE `cpcamp` ENABLE KEYS */;

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

-- Dump dei dati della tabella cprovider.cpphones: ~5 rows (circa)
/*!40000 ALTER TABLE `cpphones` DISABLE KEYS */;
INSERT INTO `cpphones` (`cpp_cpaid`, `cpp_numpho`, `cpp_phonum`, `cpp_calsts`, `cpp_censts`, `cpp_calcnt`) VALUES
	(1, 1, '3394154717', 1, 0, 0),
	(1, 2, '3334444444', 1, 0, 0),
	(2, 1, '3381999809', 1, 0, 0),
	(2, 2, '3389999999', 1, 0, 0),
	(5, 1, '3335656561', 1, 0, 0);
/*!40000 ALTER TABLE `cpphones` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
