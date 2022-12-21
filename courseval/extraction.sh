#!/bin/bash

. ~/.bash_profile

SID=""
WD=`dirname $0`

if [ "${WD: -4}" = "TEST" ]
then 
	SID=""
fi

cd $WD

today=`date '+%y%m%d%H%M'`
log_file=$WD/logs/extraction2courseval_${today}.log

for export_type in "faculty" "students" "courses" "enrollment" "course_removal"
do
	drop_location="remote/$export_type/$export_type"
	
	echo "Performing $export_type export..." | tee -a $log_file

	sql -S $DB_USER/$DB_PASS@$SID @scripts/${export_type}.sql > ${drop_location}.csv

	exit_code=${PIPESTATUS[0]}
	. $CRON_DIR/sql_status.sh
	if [[ $exit_code ==  0 ]]
	then
		echo "$export_type export completed successfully" | tee -a $log_file
	else
		echo "[FEED_PROBLEM][[there was a problem during $export_type export]]" | tee -a $log_file
	fi

	touch ${drop_location}.go
done
