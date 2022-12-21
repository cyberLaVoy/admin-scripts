#!/bin/bash
if [[ $exit_code != 0 ]] ;then
	if [ -z ${sql_output+x} ] ;
	then
		#looks like there's no sql_output variable
		if [[ -f $log_file ]]
		then
			#lets check the log file for oracle errors then
			errormessage=`cat "$log_file"|pcregrep --before-context 3 --multiline '(?s)^ORA-[[:digit:]]+:(.+(?=Disconnected)|.+(?=SP2-[[:digit:]]+))'`
			echo "[FEED_PROBLEM][[exit code $exit_code $errormessage]]"
		else
			#no log file, so just output the exit code
			echo "[FEED_PROBLEM][[exit code $exit_code]]"
		fi
	else
		#sql_output variable exists, look for oracle errors in it
		errormessage=`echo "$sql_output"|pcregrep --before-context 3 --multiline '(?s)^ORA-[[:digit:]]+:(.+(?=Disconnected)|.+(?=SP2-[[:digit:]]+))'`
		echo "[FEED_PROBLEM][[exit code $exit_code $errormessage]]"
	fi
elif [ `pcregrep -c '^SP2-[[:digit:]].*' $log_file` -gt 0 ] || [ `echo "$sql_output"|pcregrep -c '^SP2-[[:digit:]].*'` -gt 0 ] ; then
	#if there's no exit code then we need to check to see if there was an OS level error
	errormessage=`pcregrep -o '^SP2-[[:digit:]].*' $log_file || echo "$sql_output"|pcregrep -o '^SP2-[[:digit:]].*'`
	echo "[FEED_PROBLEM][[$errormessage]]"
fi
