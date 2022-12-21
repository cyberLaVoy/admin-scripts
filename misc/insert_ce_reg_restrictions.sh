#!/bin/bash
ORACLE_SID=PROD; export ORACLE_SID
ORAENV_ASK=NO; export ORAENV_ASK
PATH=/usr/local/bin:$PATH; export PATH
. /usr/local/bin/oraenv

. /home/.credentials

echo "BEGIN inserting concurrent enrollment registration restrictions..."

sql_output=`sqlplus $USERID/$PASSWD @/home/insert_ce_reg_restrictions.sql`
exit_code=$?
export sql_output exit_code
/bin/sh /home/sql_status.shl

echo "END concurrent enrollment registration restrictions inserted."
