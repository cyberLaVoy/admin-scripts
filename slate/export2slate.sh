#!/bin/bash

. ~/.bash_profile # CRON_DIR available
isTEST=""

# some magic to determine if we are in the TEST directory or not
script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
[[ "$script_dir" =~ TEST ]]
if [[ ${BASH_REMATCH[0]} != '' ]]
then
        export ORACLE_SID=""
        export isTEST="TEST"
fi

now=$(date '+%y%m%d%H%M')
log_file="$CRON_DIR/slate/$isTEST/logs/export2slate_${now}.log"

echo "BEGIN concurrent enrollment common applications processing." | tee -a $log_file
cd $CRON_DIR/slate/$isTEST/scripts/concurrent_enrollment/
python apps_processing.py | tee -a $log_file
echo "END concurrent enrollment common applications processing." | tee -a $log_file
echo "" | tee -a $log_file

echo "BEGIN electronic high school transcripts processing." | tee -a $log_file
cd $CRON_DIR/slate/$isTEST/scripts/hs_transcripts/
python process_transcripts.py | tee -a $log_file
echo "END electronic high school transcripts processing." | tee -a $log_file
echo "" | tee -a $log_file
